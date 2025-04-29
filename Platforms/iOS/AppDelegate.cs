/*
    Copyright (C) 2025 Turnipsoft Ltd, Jim Chapman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Foundation;
using LobsterConnect.VM;
using UIKit;
using CoreSpotlight;

namespace LobsterConnect;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // As far as I can tell, this is never getting called.  The only way that iOS is actually asking my
    // app to activate using a URL link is through the invocation of OpenUrl.
    [Export("application:continueUserActivity:restorationHandler:")]
    public override bool ContinueUserActivity(UIKit.UIApplication application, NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
    {
        try
        {
            Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.ContinueUserActivity", "entered");

            if (userActivity != null)
            {
                string url = null;

                switch (userActivity.ActivityType)
                {
                    case "NSUserActivityTypeBrowsingWeb":
                        url = userActivity.WebPageUrl.AbsoluteString;
                        break;
                    case "com.apple.corespotlightitem":
                        if (userActivity.UserInfo.ContainsKey(CSSearchableItem.ActivityIdentifier))
                            url = userActivity.UserInfo.ObjectForKey(CSSearchableItem.ActivityIdentifier).ToString();
                        break;
                    default:
                        if (userActivity.UserInfo.ContainsKey(new NSString("link")))
                            url = userActivity.UserInfo[new NSString("link")].ToString();
                        break;
                }

                //string url = userActivity.WebPageUrl?.ToString();
                // use the url to extract any query parameters with values if needed

                Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.ContinueUserActivity", "found url: " + url);

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
        catch(Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "AppDelegate.ContinueUserActivity", ex, "when processing url");
            return true;
        }
    }
    
    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.OpenUrl", "entered");

        try
        {
            string urlString = url.AbsoluteString;

            Model.Logger.LogMessage(Model.Logger.Level.INFO, "AppDelegate.OpenUrl", "found url: " + urlString);

            Model.DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await Model.DispatcherHelper.SleepAsync(2000);

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
                MainViewModel.Instance.OpenSessionFromUrl(urlString);
#pragma warning restore 4014
            });
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "AppDelegate.OpenUrl", ex, "when processing url");
            return true;
        }
        return false;
    }
}


