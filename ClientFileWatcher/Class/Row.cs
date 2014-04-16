using System;
using System.IO;
using Client.DataModel;


namespace Client.Class
{
    public class Row
    {
        #region Public Interface

        public string Label { get; private set; }
        public string ProcessName { get; private set; }
        public string TargetFileName { get; private set; }
        public string SourceFileName { get; private set; }
        public DateTime Time { get; private set; }
        public FILE_EVENT_TYPE_ENUM Action { get; private set; }
        
        public static string GetExtension(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            var extension = Path.GetExtension(fileName);
            return extension.ToUpper();
        }

        public Row()
        {
        }

        public Row(Row message)
        {
            CreateRow(message.ProcessName, message.SourceFileName, message.TargetFileName, message.Action, message.Time);
        }

        public Row(string processName, string sourceFileName, string targetFileName, FILE_EVENT_TYPE_ENUM action)
        {
            DateTime now = DateTime.Now;
            CreateRow(Path.GetFileNameWithoutExtension(processName), sourceFileName, targetFileName, action, now);
        }

        private void CreateRow(string processName, string sourceFileName, string targetFileName, FILE_EVENT_TYPE_ENUM action, DateTime time)
        {
            try
            {
                ProcessName = processName.ToUpper();
                SourceFileName = sourceFileName.ToUpper();
                TargetFileName = targetFileName.ToUpper();
                Action = action;
                Time = time;

                Label = string.Format(processName + action + ": " + SourceFileName + " " + TargetFileName + " " + time.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            }
            catch
            {
            }
        }

        #endregion
    }
}
