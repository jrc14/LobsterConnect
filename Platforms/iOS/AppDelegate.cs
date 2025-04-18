using Foundation;
using LobsterConnect.VM;

namespace LobsterConnect;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    [Export("application:continueUserActivity:restorationHandler:")]
    public override bool ContinueUserActivity(UIKit.UIApplication application, NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
    {
        if (userActivity != null)
        {
            string url = userActivity.WebPageUrl?.ToString();
            // use the url to extract any query parameters with values if needed

            Model.DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MainViewModel.Instance.OpenSessionFromUrl(url);
            });
            
        }

        return true;
    }
}


