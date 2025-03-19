using CommunityToolkit.Maui.Markup;

namespace LobsterConnect;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        
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

    private void AppShell_Tapped(object sender, TappedEventArgs e)
    {

    }

}
