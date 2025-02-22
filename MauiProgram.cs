using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

namespace LobsterConnect;


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
