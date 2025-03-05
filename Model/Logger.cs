
namespace LobsterConnect.Model
{
    /// <summary>
    /// Methods for writing to the app log file, which is logfile.txt in the app's working directyory.
    /// The log file is maintained by a background worker thread, so log operations
    /// don't block the UI thread.  It is not allowed to grow beyond 10MB in size; when it does,
    /// a new log file is started and the old one is renamed (to a name incoporating a GUID).
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log a message to the app log file
        /// </summary>
        /// <param name="l">severity</param>
        /// <param name="context">what you were doing when the error happened (for instance, class name and methond name)</param>
        /// <param name="details">what went wrong</param>
        static public void LogMessage(Level l, string context, string details="")
        {
            LogMessage(l, context, null, details);
        }
            
        /// <summary>
        /// Log a message to the app log file, for cases when an exception has been caught
        /// </summary>
        /// <param name="l">severity</param>
        /// <param name="context">what you were doing when the error happened (for instance, class name and methond name)</param>
        /// <param name="ex">the exception that was thrown</param>
        /// <param name="details">what went wrong</param>

        static public void LogMessage(Level l, string context, Exception ex, string details = "")
        {
#if !DEBUG
            if (l == Level.DEBUG)
                return;
#endif
            string msg = "";

            switch (l)
            {
                case Level.DEBUG: msg = "DEBUG: "; break;
                case Level.INFO: msg = "INFO: "; break;
                case Level.WARNING: msg = "WARNING: "; break;
                case Level.ERROR: msg = "ERROR: "; break;
                default: break;
            }

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            msg = now + ": " + msg;

            msg += context;

            if (ex != null)
            {
                msg += " " + ex.ToString();

                if (ex.Message != null)
                {
                    msg += " - " + ex.Message + " ";
                }
            }

            if (msg.Length > 200)
                msg = msg.Substring(0, 199) + "[truncated]";

            if (!string.IsNullOrEmpty(details))
            {
                msg += ": " + details;

                if (msg.Length > 400)
                    msg = msg.Substring(0, 399) + "[truncated]";
            }
        

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }


            lock (LogLinesLock)
            {
                LogLinesToWrite.Add(msg);
            }

            if (LogWorker == null)
            {
                Model.DispatcherHelper.RunAsyncOnUI(StartLogWorker);
            }
        }

        public enum Level { DEBUG, INFO, WARNING, ERROR};

        private static void StartLogWorker()
        {
            try
            {
                if (_LogWorkerStarting || _LogWorkerStarted)
                {
                    return; // only going to do this once.
                }

                _LogWorkerStarting = true;

                if (LogWorker != null)
                {
                    return; // only going to do this once.
                }
                LogWorker = new Model.LobsterWorker();
                LogWorker.WorkerReportsProgress = false;
                LogWorker.WorkerSupportsCancellation = false;
                LogWorker.RunWorkerCompleted += (object sender, Model.RunWorkerCompletedEventArgs e) =>
                {
                    // First, handle the case where an exception was thrown.
                    if (e.Error != null)
                    {

                    }
                    else if (e.Cancelled)
                    {

                    }
                    else
                    {

                    }
                    _LogWorkerStarting = false;
                    _LogWorkerStarted = false;
                    LogWorker = null;
                };
                LogWorker.DoWork += (object sender, Model.DoWorkEventArgs e) =>
                {
                    try
                    {
                        // Get the LobsterWorker that raised this event.
                        LobsterWorker w = sender as LobsterWorker;

                        string logFilePath = Path.Combine(App.ProgramFolder, "logfile.txt");

                        int waitTime = 1000;

                        while (true)
                        {
                            if (App.ApplicationSuspended)
                            {
                                e.Result = true;
                                return;
                            }
                            if (w != LogWorker)
                            {
                                e.Result = true;
                                return;
                            }

                            List<string> linesToWrite = null;
                            lock (LogLinesLock)
                            {
                                if (LogLinesToWrite.Count > 0)
                                {
                                    linesToWrite = new List<string>();
                                    linesToWrite.AddRange(LogLinesToWrite);
                                    LogLinesToWrite = new List<string>();
                                }
                            }

                            if (linesToWrite != null)
                            {
                                waitTime = 1000;

                                if (System.Threading.Monitor.TryEnter(LogFileLock, 10000))
                                {
                                    try
                                    {
                                        long fileLength = 0;

                                        using (Stream fs = System.IO.File.Open(logFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                        {
                                            fs.Seek(0, SeekOrigin.End);

                                            using (StreamWriter logFileWriter = new StreamWriter(fs))
                                            {
                                                foreach (string m in linesToWrite)
                                                {
                                                    logFileWriter.WriteLine(m);
                                                }

                                                fileLength = fs.Position;
                                            }
                                        }

                                        if (fileLength > 1024L * 1024L * 1024L)
                                        {
                                            Utilities.FileDeleteIfExists(logFilePath);
                                        }
                                        else if (fileLength > 10L * 1024L * 1024L)
                                        {
                                            string moveFilePath = Path.Combine(App.ProgramFolder, "logfile" + Guid.NewGuid().ToString("N") + ".txt");

                                            Utilities.FileMove(logFilePath, moveFilePath);
                                            Utilities.FileDeleteIfExists(logFilePath);

                                            using (Stream fs = System.IO.File.Open(logFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                            {
                                                using (StreamWriter logFileWriter = new StreamWriter(fs))
                                                {
                                                    logFileWriter.WriteLine("LOG FILE EXCEEDED 10MB.  Previous file contents stored at: " + moveFilePath);
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    finally
                                    {
                                        System.Threading.Monitor.Exit(LogFileLock);
                                    }
                                }
                            }
                            else
                            {
                                if (waitTime < 60000)
                                    waitTime = waitTime + 1000;
                            }
                            System.Threading.Tasks.Task.Delay(waitTime).Wait();
                        }


                    }
                    catch (Exception)
                    {
                        e.Result = false;
                    }
                };

                LogWorker.RunWorkerAsync(true);
                _LogWorkerStarted = true;
            }
            catch (Exception )
            {
                // Don't report errors, because it will just lead to this function calling itself recursively, which cannot be a useful thing to do.
            }
        }

        // Lock this lock if you're planning to mess with the log file (copying it or anything)
        // so that while you're working on the file, the log worker won't try to do anything with the file.
        private static LobsterLock LogFileLock = new Model.LobsterLock();

        private static LobsterWorker LogWorker = null;
        private static List<string> LogLinesToWrite = new List<string>();
        private static LobsterLock LogLinesLock = new Model.LobsterLock();
        private static bool _LogWorkerStarting = false;
        private static bool _LogWorkerStarted = false;
    }
}
