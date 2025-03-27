using CommunityToolkit.Maui.Markup;
using LobsterConnect.V;

namespace LobsterConnect;

/// <summary>
/// The boilerplate code for app shell UI setup.  A new menu template is added to make the flyout menu style
/// acceptable, and a method is added to pass flyout menu actions to the MainPage, where they belong.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Display flyout menu items in a way that is at least legible, which is more than you can say
        // for the out-of-the-box defaults provided by MAUI.
        this.MenuItemTemplate = new DataTemplate(() =>
        {
            return new Grid()
            {
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                },
                Margin = new Thickness(0, 2, 0, 2),
                Children =
                {
                    new Image()
                    {
                        Margin=new Thickness(0,0,0,0),
                        VerticalOptions = LayoutOptions.Center,
                        HeightRequest=24
                    }
                    .Bind(Image.SourceProperty, "Icon")
                    .Invoke(i => i.Loaded += (o, e) =>
                    {
                        try
                        {
                            Image iSender = o as Image;
                            if(iSender!=null)
                            {
                                FontImageSource src = iSender.Source as FontImageSource;
                                if(src!=null)
                                {
                                    src.Color=Colors.White;
                                }
                            }
                        }
                        catch
                        {

                        }
                    })
                    .Row(0).Column(0),
                    new Label()
                    {
                        Margin=new Thickness(10,0,0,0),
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        HeightRequest=30,
                        TextColor= Colors.White,
                    }
                    .Bind(Label.TextProperty,"Text")
                    .Row(0).Column(1),
                }
            };
        });
        
    }

    /// <summary>
    /// Some day we might decide to do things when the main title bar is tapped
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AppShell_Tapped(object sender, TappedEventArgs e)
    {

    }

    /// <summary>
    /// Taps on flyout menu items come here, and are handed off the the main page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AppShell_MenuItemClicked(object sender, EventArgs e)
    {
        this.FlyoutIsPresented = false;

        if (sender is MenuItem)
        {
            MainPage.Instance.FlyoutMenuAction(((MenuItem)sender).CommandParameter as string);
        }
    }
}
