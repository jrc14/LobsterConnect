using CommunityToolkit.Maui.Views;
namespace LobsterConnect.V;

public partial class PopupLogIn : Popup
{
	public PopupLogIn()
	{
		InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);

        string defaultUserName = "";
        if (Microsoft.Maui.Storage.Preferences.ContainsKey("UserHandle"))
        {
            defaultUserName = Microsoft.Maui.Storage.Preferences.Get("UserHandle", "");
            if(!string.IsNullOrEmpty(defaultUserName))
            {
                this.entryUserHandle.Text = defaultUserName;
            }
        }

        if (MainPage.Instance.Width < 350)
        {
            this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
            this.entryUserHandle.WidthRequest = 80;
            this.entryPassword.WidthRequest = 80;
        }
        else if (MainPage.Instance.Width < 400)
        {
            this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
            this.entryUserHandle.WidthRequest = 130;
            this.entryPassword.WidthRequest = 130;
        }
        else
        {
            this.colDef1.Width = new GridLength(200, GridUnitType.Absolute);
            this.entryUserHandle.WidthRequest = 180;
            this.entryPassword.WidthRequest = 180;
        }
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        Tuple<string, string, bool> t = new Tuple<string, string,bool>(this.entryUserHandle.Text, this.entryPassword.Text, this.chkRememberMe.IsChecked);

        await CloseAsync(t, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }
}