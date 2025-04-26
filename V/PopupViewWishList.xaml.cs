using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;

namespace LobsterConnect.V;

/// <summary>
/// Popup for displaying the people interested in playing some game.  After constructing the popup,
/// but before showing it, call SetGame to tell the popup what game it should load.
/// </summary>
public partial class PopupViewWishList : Popup
{
    public PopupViewWishList()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 450)
            {
                this.colDef0.Width = new GridLength(200, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(200, GridUnitType.Absolute);
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(200 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(200 - ww / 2, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupViewWishList ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);
    }

    private string _gameName="";
    public void SetGame(string gameName)
    {
        this.lblTitle.Text = "Who'd like to play " + gameName + "?";
        LoadPersons(gameName);
        this._gameName = gameName;
    }

    void LoadPersons(string gameName)
    {
        if (MainViewModel.Instance.LoggedOnUser == null)
            return;

        this.gdPersons.RowDefinitions.Clear();
        this.gdPersons.Children.Clear();

        int r = 0;
        List<WishListItem> items = MainViewModel.Instance.GetWishListItemsForGame(gameName);
        foreach (WishListItem item in items)
        {
            gdPersons.RowDefinitions.Add(new RowDefinition() {Height=new GridLength(40)});
            gdPersons.Children.Add(new Label()
            {
                Text = item.Person
            }.Row(r).Column(0));

            gdPersons.Children.Add(new Label()
            {
                Text = item.Notes
            }.Row(r).Column(1));

            r++;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }


    private async void lblReportContentTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (Email.Default.IsComposeSupported)
            {

                EmailMessage message = new EmailMessage()
                {
                    Subject = "LobsterConnect Inappropriate Content",
                    Body = "I note that the wish-list details for '" + this._gameName + "' contain inappropriate content.  Please take the necessary steps to address this concern.",
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>() { "moderator@turnipsoft.co.uk" }
                };

                await Email.Default.ComposeAsync(message);
                await CloseAsync(true, CancellationToken.None);
            }
            else
            {
                MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "Sorry, the app cannot create an email for you.  Please send your report to moderator@turnipsoft.co.uk");
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error while composing email: " + ex.Message);
        }
    }
}