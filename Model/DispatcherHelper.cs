using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.Model
{

    internal static class DispatcherHelper
    {
        internal static void RunAsyncOnUI(Action action)
        {
            try
            {
                if (_MainDispatcher == null)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunAsyncOnUI","main dispatcher not set up! ");
                    action();
                }
                else
                {
                    _MainDispatcher.Dispatch(() =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunAsyncOnUI",ex, "from dispatched thread");
                        }
                    });
                }
            }
            catch (Exception e2)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunAsyncOnUI", e2, "from dispatching thread");
            }
        }


        public static async Task<T> RunAsyncTaskOnUIAsync<T>(Func<Task<T>> func)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();

            if (_MainDispatcher == null)
                taskCompletionSource.SetException(new Exception("no dispatcher"));
            else
            {
                await _MainDispatcher.DispatchAsync(async () =>
                {
                    try
                    {
                        taskCompletionSource.SetResult(await func());
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                });
            }
            return await taskCompletionSource.Task;

        }
        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static async Task RunAsyncTaskOnUIAsync(
            Func<Task> func) =>
            await RunAsyncTaskOnUIAsync(async () => { await func(); return false; });

        public static void CheckBeginInvokeOnUI(Action action)
        {
            if (Environment.CurrentManagedThreadId == _MainThreadId)
            {
                action();
            }
            else
            {
                RunAsyncOnUI(() => {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.CheckBeginInvokeOnUI", ex);
                    }
                });
            }
        }

        public static void CheckRunOnNonUIThread(Action action)
        {
            if (Environment.CurrentManagedThreadId == _MainThreadId)
            {
                RunOnNewThread(() => {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.CheckRunOnNonUIThread", ex);
                    }
                });
            }
            else
            {
                action();
            }
        }

        public async static Task<bool> CheckRunOnNonUIThreadAsync(Action action)
        {
            if (Environment.CurrentManagedThreadId == _MainThreadId)
            {
                await RunOnNewThreadAsync(() => {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.CheckRunOnNonUIThread", ex, "from new thread");
                    }
                });
                return true;
            }
            else
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.CheckRunOnNonUIThread", ex, "from existing thread");
                    return false;
                }
            }

        }

        private static int _MainThreadId = -1;
        private static Microsoft.Maui.Dispatching.IDispatcher _MainDispatcher = null;

        public static bool Initialise(int mainThread, Microsoft.Maui.Dispatching.IDispatcher mainDispatcher)
        {
            //if(_MainThreadId==-1)
            //    _MainThreadId = Environment.CurrentManagedThreadId;
            _MainThreadId = mainThread;
            _MainDispatcher = mainDispatcher;

            return true;
        }

        public static bool UIDispatcherHasThreadAccess
        {
            get
            {
                if (Environment.CurrentManagedThreadId == _MainThreadId)
                    return true;
                else
                    return false;
            }

        }

        public static void RunOnNewThread(Action action)
        {
            try
            {
                System.Threading.Tasks.Task.Run(() => {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunOnNewThread", ex);
                    }
                });
            }
            catch (Exception e2)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunOnNewThread", e2);
            }
        }

        public async static Task<bool> RunOnNewThreadAsync(Action action)
        {
            try
            {
                await System.Threading.Tasks.Task.Run(() => {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunOnNewThreadAsync", ex);
                    }
                });
                return true;
            }
            catch (Exception e2)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherHelper.RunOnNewThreadAsync", e2);
                return false;
            }
        }

        private static System.Random _Random = null;
        public static void SleepRandom(int min, int max)
        {
            if (_Random == null)
                _Random = new System.Random();

            double r = min + _Random.NextDouble() * (max - min);

            if (r > 0)
                Sleep((int)r);
            else
            {
                Logger.LogMessage(Logger.Level.DEBUG, "DispatcherHelper.SleepRandom","interval is <=0 - not sleeping");
            }
        }

        public static void Sleep(int ms)
        {
            if (ms > 0)
            {
                using (System.Threading.ManualResetEvent ev = new System.Threading.ManualResetEvent(false))
                {
                    ev.WaitOne(ms);
                }
            }
            else
            {
                Logger.LogMessage(Logger.Level.DEBUG, "DispatcherHelper.Sleep","interval is <=0 - not sleeping");
            }
        }

        public static async Task<bool> SleepRandomAsync(int min, int max)
        {
            if (_Random == null)
                _Random = new System.Random();

            double r = min + _Random.NextDouble() * (max - min);

            if (r > 0)
            {
                await SleepAsync((int)r);
            }
            else
            {
                Logger.LogMessage(Logger.Level.DEBUG, "DispatcherHelper.SleepRandomAsync","interval is <=0 - not sleeping");
            }

            return true;
        }
        public static async Task<bool> SleepAsync(int ms)
        {
            if (ms > 0)
            {
                await Task.Delay(ms);
            }
            else
            {
                Logger.LogMessage(Logger.Level.DEBUG, "DispatcherHelper.SleepAsync","interval is <=0 - not sleeping");
            }

            return true;
        }

        public static void StopTimer(ref System.Threading.Timer timer)
        {
            try
            {
                if (timer != null)
                {
                    timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); // stop
                    timer.Dispose();
                    timer = null;
                }
                timer = null;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherTimer.StopTimer", ex);
            }
        }

        public static void StartTimer(ref System.Threading.Timer timer, int milliseconds, Action onTick)
        {
            try
            {
                StopTimer(ref timer);

                timer = new System.Threading.Timer(
                    (object o) =>
                    {
                        try
                        {
                            onTick();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "DispatcherTimer.StartTimer: Timer", ex);
                        }
                    }, null, milliseconds, System.Threading.Timeout.Infinite);

            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherTimer.StartTimer", ex);
            }
        }

        public static void StartRepeatingTimer(ref System.Threading.Timer timer, int milliseconds, Action onTick)
        {
            try
            {
                StopTimer(ref timer);

                timer = new System.Threading.Timer(
                        (object o) =>
                        {
                            try
                            {
                                onTick();
                            }
                            catch (Exception ex)
                            {
                                Logger.LogMessage(Logger.Level.ERROR, "DispatcherTimer.StarRepeatingtTimer: Timer", ex);
                            }
                        }, null, milliseconds, milliseconds);

            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "DispatcherTimer.StarRepeatingtTimer", ex);
            }
        }
    }
}
