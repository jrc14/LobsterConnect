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
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Java.Net;

namespace LobsterConnect;


[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { Intent.ActionView },
Categories = new[]
{
    Intent.ActionView,
    Intent.CategoryDefault,
    Intent.CategoryBrowsable,
},
DataScheme = "lobsterconnect", DataHost = "", DataPathPrefix = "/")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var uri = Intent?.Data;
        if (uri != null)
        {
            Model.Logger.LogMessage(Model.Logger.Level.INFO, "MainActivity.OnCreate", "intent data contains URI " + uri.ToString());

            Model.DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await Model.DispatcherHelper.SleepAsync(2000);
                VM.MainViewModel.Instance.OpenSessionFromUrl(uri.ToString());
            });
        }
    }
}
