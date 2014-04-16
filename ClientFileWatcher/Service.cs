using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.Class;

namespace Client
{
    public partial class Service : ServiceBase
    {
        private FileWatcher _fileWatcher;

        public Service()
        {
            InitializeComponent();
            CanPauseAndContinue = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
#if !DEBUG
                StartWatching();
#endif
            }
            catch
            {
            }
        }

        protected override void OnContinue()
        {
            try
            {
                StartWatching();
            }
            catch
            {
            }
        }

        protected override void OnPause()
        {
            try
            {
                _fileWatcher.EndWatching();
            }
            catch
            {

            }
        }

        protected override void OnStop()
        {
            try
            {
                _fileWatcher.EndWatching();
            }
            catch
            {

            }
        }

        protected void StartWatching()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            }
            finally
            {
                try
                {
                    _fileWatcher = new FileWatcher();
                    _fileWatcher.StartWatching();
                }
                catch (Exception e)
                {
                    Stop();
                    throw new InvalidOperationException("Unable to startup.");
                }
            }
        }
    }
}
