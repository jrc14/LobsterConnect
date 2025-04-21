using Foundation;
using LobsterConnect.VM;

namespace LobsterConnect;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /*
    [Export("application:continueUserActivity:restorationHandler:")]
    public override bool ContinueUserActivity(UIKit.UIApplication application, NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
    {
        Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.ContinueUserActivity","entered");
        if (userActivity != null)
        {
            string url = userActivity.WebPageUrl?.ToString();
            // use the url to extract any query parameters with values if needed

            Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.ContinueUserActivity","found url: "+url);

            Model.DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await Model.DispatcherHelper.SleepAsync(2000);

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
                MainViewModel.Instance.OpenSessionFromUrl(url);
#pragma warning restore 4014
            });
            
        }

        return true;
    }
    */
}


