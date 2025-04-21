using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;

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
            /*
            var parameters = uri.GetQueryParameters(null);
            if (parameters != null && parameters.Count > 0)
            {

            }
            */

            Model.DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await Model.DispatcherHelper.SleepAsync(2000);
                VM.MainViewModel.Instance.OpenSessionFromUrl(uri.ToString());
            });
        }
    }
}
