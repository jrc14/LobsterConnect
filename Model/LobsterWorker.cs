using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.Model
{
    /// <summary>
    /// A background worker. Start one of them by calling RunWorkerAsync, and tell it what to do by
    /// implementing a handler for the DoWork event.  It will raise RunWorkerCompleted when the work is
    /// finished (i.e. after the DoWork event handler exits).  It's copied from some code on the internet
    /// downloaded way back when (and AFAIR covered by an MIT licence).
    /// This app uses two background workers, one to maintain the log file, and one to manage the
    /// journal (saving it periodically, and syncing it with the cloud sync service)
    /// </summary>
    public class LobsterWorker
    {
        public LobsterWorker()
        {
        }

        public void Cancel()
        {
            if (!WorkerSupportsCancellation)
                throw new NotSupportedException();

            if (!IsBusy)
            {

            }
            else
            {
                CancellationPending = true;
            }
        }

        public bool CancellationPending { get; private set; }

        public event System.ComponentModel.ProgressChangedEventHandler ProgressChanged;

        public void ReportProgress(int percentProgress)
        {
            ReportProgress(percentProgress, null);
        }

        public void ReportProgress(int percentProgress, object userState)
        {

            if (ProgressChanged != null)
                Model.DispatcherHelper.RunAsyncOnUI(() =>
                {
                    ProgressChanged(this, new System.ComponentModel.ProgressChangedEventArgs(percentProgress, userState));
                });
        }

        public bool WorkerReportsProgress { get; set; }
        public bool WorkerSupportsCancellation { get; set; }
        public bool IsBusy { get; set; }

        public event DoWorkEventHandler DoWork;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (RunWorkerCompleted != null)
                RunWorkerCompleted(this, e);
        }

        public void RunWorkerAsync()
        {
            RunWorkerAsync(null);
        }

        public async void RunWorkerAsync(object userState)
        {
            if (DoWork != null)
            {
                CancellationPending = false;
                IsBusy = true;
                try
                {
                    var args = new DoWorkEventArgs { Argument = userState };
                    await Task.Factory.StartNew(() =>
                    {
                        DoWork(this, args);

                        IsBusy = false;
                        OnRunWorkerCompleted(new RunWorkerCompletedEventArgs { Result = args.Result });
                    }, TaskCreationOptions.LongRunning);
                }
                catch (Exception ex)
                {
                    IsBusy = false;
                    OnRunWorkerCompleted(new RunWorkerCompletedEventArgs { Error = ex });
                }
            }
        }
    }

    public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);

    public class DoWorkEventArgs : EventArgs
    {
        public DoWorkEventArgs()
        { }

        public DoWorkEventArgs(object argument)
        {
            Argument = argument;
        }

        public object Argument { get; set; }
        public bool Cancel { get; set; }
        public object Result { get; set; }
    }

    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);

    public class RunWorkerCompletedEventArgs : EventArgs
    {
        public RunWorkerCompletedEventArgs()
        { }

        public RunWorkerCompletedEventArgs(object result, Exception error, bool cancelled)
        {
            Result = result;
            Error = error;
            Cancelled = cancelled;
        }

        public Exception Error { get; set; }
        public object Result { get; set; }
        public bool Cancelled { get; set; }
    }
}