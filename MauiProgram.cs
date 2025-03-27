using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

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
            });

		return builder.Build();
	}
}
