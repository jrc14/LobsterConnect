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
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.LifecycleEvents;

namespace LobsterConnect;

/// <summary>
/// Boilerplate.  I added a couple of fonts.  For general description of the app, see App.xaml.cs.
///
/// App startup and the initial loading of the viewmodel are managed in the contructor of the main
/// page - see MainPage.xaml.cs.
/// </summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FiraMono-Regular.ttf", "FiraMonoRegular");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentSystemIcons-Regular");
            })
            .ConfigureLifecycleEvents(lifecycle =>
            {
#if IOS || MACCATALYST
                // For whatever reason, none of this actually seems to work (perhaps because I am
                // using the older 'App Link' approach rather than the now preferred 'Universal Link'
                // setup).  The only way that iOS is actually asking my
                // app to activate using a URL link is through the invocation of AppDelegate.OpenUrl.
                lifecycle.AddiOS(ios =>
                {
                    // Universal link delivered to FinishedLaunching after app launch.
                    ios.FinishedLaunching((app, data) =>
                    {
                        try
                        {
                            if(app!=null && app.UserActivity!=null && app.UserActivity.WebPageUrl!=null)
                            {
                                Model.Logger.LogMessage(Model.Logger.Level.INFO, "FinishedLaunching lifecycle handler","WebPageUrl=" + app.UserActivity.WebPageUrl.AbsoluteString);
                            }
                            return HandleAppLink(app.UserActivity);
                        }
                        catch(Exception ex)
                        {
                            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "FinishedLaunching lifecycle handler", ex);
                            return true;
                        }
                    });

                    // Universal link delivered to ContinueUserActivity when the app is running or suspended.
                    ios.ContinueUserActivity((app, userActivity, handler) =>
                    {
                        try
                        {
                            if(userActivity != null && userActivity.WebPageUrl != null)
                            {
                                Model.Logger.LogMessage(Model.Logger.Level.INFO, "ContinueUserActivity lifecycle handler","WebPageUrl=" + userActivity.WebPageUrl.AbsoluteString);
                            }
                            return HandleAppLink(userActivity);
                        }
                        catch (Exception ex)
                        {
                            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "ContinueUserActivity lifecycle handler", ex);
                            return true;
                        }
                    });


                    if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
                    {
                        // Universal link delivered to SceneWillConnect after app launch
                        ios.SceneWillConnect((scene, sceneSession, sceneConnectionOptions) =>
                        {
                            try
                            {
                                var userActivity = sceneConnectionOptions.UserActivities.ToArray().FirstOrDefault(a => a.ActivityType == Foundation.NSUserActivityType.BrowsingWeb);
                                if (userActivity != null && userActivity.WebPageUrl != null)
                                {
                                    Model.Logger.LogMessage(Model.Logger.Level.INFO, "SceneWillConnect lifecycle handler", "WebPageUrl=" + userActivity.WebPageUrl.AbsoluteString);
                                    HandleAppLink(userActivity);
                                }


                            }
                            catch (Exception ex)
                            {
                                Model.Logger.LogMessage(Model.Logger.Level.ERROR, "SceneWillConnect lifecycle handler", ex);
                            }
                        });

                        // Universal link delivered to SceneContinueUserActivity when the app is running or suspended
                        ios.SceneContinueUserActivity((scene, userActivity) =>
                        {
                            try
                            {
                                if (userActivity != null && userActivity.WebPageUrl != null)
                                {
                                    Model.Logger.LogMessage(Model.Logger.Level.INFO, "SceneContinueUserActivity lifecycle handler", "WebPageUrl=" + userActivity.WebPageUrl.AbsoluteString);
                                    return HandleAppLink(userActivity);
                                }
                                else
                                    return true;
                            }
                            catch (Exception ex)
                            {
                                Model.Logger.LogMessage(Model.Logger.Level.ERROR, "SceneContinueUserActivity lifecycle handler", ex);
                                return true;
                            }
                        });
                    }
                });
#endif
            });
        

		return builder.Build();
	}

#if IOS || MACCATALYST
    /// <summary>
    /// Process the app deep link provided, in the case of iOS installations, by the lifecycle handlers defined above.
    /// </summary>
    /// <param name="userActivity"></param>
    /// <returns></returns>
    static bool HandleAppLink(Foundation.NSUserActivity userActivity)
    {
        if (userActivity is not null
        && userActivity.ActivityType == Foundation.NSUserActivityType.BrowsingWeb
        && userActivity.WebPageUrl is not null)
        {
            //Model.Logger.LogMessage(Model.Logger.Level.INFO, "MauiProgram.HandleAppLink", "entered");

            string url = userActivity.WebPageUrl?.ToString();
            // use the url to extract any query parameters with values if needed

            Model.Logger.LogMessage(Model.Logger.Level.INFO, "MauiProgram.HandleAppLink", "found url: " + url);

            Model.DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await Model.DispatcherHelper.SleepAsync(2000);

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
                VM.MainViewModel.Instance.OpenSessionFromUrl(url);
#pragma warning restore 4014
            });            

            return true;
        }

        return false;
    }
#endif
}
