using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupDataHandling : Popup
{
    private static string _PolicyText=
            "In order to propose or sign up to gaming sessions in the app, you need to set up a user.  If you do this, then we will store and use personal data about you, so we can manage game sign-ups for you.\n" +
            "To set up a user you will have to enter a user handle (name) and password, and we will store this information in our on-line database.  " +
            "You may, additionally, provide a full name, phone number and/or email address.  You do not have to provide this data for the app to work, but if you do, it will be visible to other users of the app, and it will be stored in our on-line database.\n" +
            "When you create gaming sessions or sign up to play games, the app will record this information in its on-line database, and make it visible to other users of the app.\n" +
            "We will not use this data for any purpose other than managing game sign-ups for you, and we won't give or sell it to anyone else, except as may be required to meet our legal obligations (to law enforcement authorities, for example).\n" +
            "All data held in our on-line database is protected by 256-bit AES encryption, but note that, once it's been downloaded into the app, it is visible to anyone who uses the app, and it could in principle be accessed by anyone at all.  " +
            "Accordingly, you should not enter any sensitive data (credit card details, for example) into the application.  If you are concerned that sensitive data has been exposed, please contact us at dataprotection@turnipsoft.co.uk.\n" +
            "The Privacy and Data Handling menu option in the app will allow you to see all the personal data that is held about you, and get it removed it from our on-line database.  Alternatively, you can email data access or deletion requests to us at dataprotection@turnipsoft.co.uk.\n" +
            "You should not depend upon us to preserve any data for you; any data may be removed from the app at any time for technical reasons, without notice.\n" +
            "If your account is inactive (you don't propose or sign up to any games) for more that five years, then your personal data will be removed from our on-line database.\n";

    /// <summary>
    /// Popup to display the privacy and data management policy.  It contains additional controls to
    /// display what personal information is held for the current user (using the PrettyPrint method
    /// on the local journal, and the RelatesToUser method on JournalEntry) and to purge that data
    /// from the cloud sync service, using MainViewModel.Instance.PurgeUserData.
    /// </summary>
    public PopupDataHandling()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons, this.btnViewPolicy, this.btnViewPersonalData, this.btnPurgePersonalData);

        this.lbTextViewer.Text = _PolicyText;

        try
        {

            if (MainPage.Instance.Width > 500)
            {
                this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef2.Width = new GridLength(150, GridUnitType.Absolute);
            }
            else
            {
                double ww = 500 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(150 - ww / 3, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150 - ww / 3, GridUnitType.Absolute);
                this.colDef2.Width = new GridLength(150 - ww / 3, GridUnitType.Absolute);
            }

            if (MainPage.Instance.Height > 650)
            {
                this.rdefTextViewer.Height = new GridLength(400, GridUnitType.Absolute);
            }
            else
            {
                this.rdefTextViewer.Height = new GridLength(MainPage.Instance.Height - 250, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupDataHandling ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        if (MainViewModel.Instance.LoggedOnUser == null)
        {
            this.btnPurgePersonalData.IsVisible = false;
            this.btnViewPersonalData.IsVisible = false;
            this.btnViewPolicy.IsVisible = false;
        }
    }

    /// <summary>
    /// Close the popup
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(true, CancellationToken.None);
    }

    /// <summary>
    /// Display the data management policy in the scrollable view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnViewPolicyClicked(object sender, EventArgs e)
    {
        this.svTextViewer.ScrollToAsync(0, 0, false);
        this.lbTextViewer.Text = _PolicyText;
    }

    /// <summary>
    /// Display the current user's personal data in the scrollable view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnViewPersonalDataClicked(object sender, EventArgs e)
    {
        this.svTextViewer.ScrollToAsync(0, 0, false);

        if (MainViewModel.Instance.LoggedOnUser == null)
            this.lbTextViewer.Text = "NO LOGGED ON USER";
        else
        {
            this.lbTextViewer.Text = Journal.PrettyPrint(
                (x) => { return x.RelatesToUser(MainViewModel.Instance.LoggedOnUser.Handle); },
                MainViewModel.Instance);
                
        }
    }

    /// <summary>
    /// Purge personal data for the current user, after displaying a suitably scary warning
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnPurgePersonalDataClicked(object sender, EventArgs e)
    {
        if (MainViewModel.Instance.LoggedOnUser != null)
        {
            string personHandle = MainViewModel.Instance.LoggedOnUser.Handle;
            bool confirmation = await MainPage.Instance.DisplayAlert("Purge Data", "Please confirm you want to purge all information about '"+personHandle+"' from the app and its on-line database.  Please note that this action cannot be undone; once purged, the data is permanently and unrecoverably lost.", "Purge", "Don't purge");
            if (confirmation)
            {
                await MainViewModel.Instance.PurgeUserData(personHandle);
            }
        }
    }

}

