using System;
using System.Configuration;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32.SafeHandles;

namespace ConfigEncrypt
{
    public class CustomActions
    {
        private const int Logon32ProviderDefault = 0;
        private const int Logon32LogonInteractive = 2; //Causes LogonUser to create a primary token. 
        private const string ServerId = "[SERVER]";
        private const string DatabaseId = "[DATABASE]";

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Set the app.config connection string and encrypt
        /// </summary>
        /// <param name="session">The install session</param>
        /// <returns></returns>
        [CustomAction]
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static ActionResult ConfigEncrypt(Session session)
        {
            try
            {
                session.Log("Begin ConfigEncrypt Custom Action");

                string exePath = session.CustomActionData["install"];
                string fullUsername = session.CustomActionData["user"];
                var splitNames = fullUsername.Split('\\');
                string domain = splitNames[0];
                string user = splitNames[1];
                string password = session.CustomActionData["pass"];
                string server = session.CustomActionData["server"];
                string database = session.CustomActionData["database"];

                if (!File.Exists(exePath) && !File.Exists(exePath + @".config"))
                {
                    throw new FileNotFoundException();
                }

                session.Log("Opening config {0}", exePath);
                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);
                session.Log("Getting connectionStrings section");
                var section = config.GetSection("connectionStrings") as ConnectionStringsSection;
                
				for(int i = 0; i < section.ConnectionStrings.Count; i++)
				{
				    section.ConnectionStrings[i].ConnectionString =
				        section.ConnectionStrings[i].ConnectionString.Replace(@"|SERVER|", server).Replace(@"|DATABASE|", database);
				}

                session.Log("Logging on username: {0} domain: {1}", user, domain);
                SafeTokenHandle safeTokenHandle;
                bool returnValue = LogonUser(user, domain, password, Logon32LogonInteractive, Logon32ProviderDefault, out safeTokenHandle);

                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }

                using (safeTokenHandle)
                {
                    session.Log("Impersonating {0}", user);
                    // Use the token handle returned by LogonUser. 
                    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle()))
                    {
                        session.Log("Protecting connectionStrings");
                        section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");

                        session.Log("Undoing impersonation");

                        impersonatedUser.Undo();
                    }
                }
                session.Log("Saving");
                config.Save();

                session.Log("End ConfigEncrypt Custom Action");
            }
            catch (Exception ex)
            {
                session.Log("ERROR in custom action ConfigEncrypt {0}", ex.ToString());
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}
