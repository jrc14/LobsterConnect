using CommunityToolkit.Maui.Views;
namespace LobsterConnect.V;

public partial class PopupLogIn : Popup
{
	public PopupLogIn()
	{
		InitializeComponent();

        string defaultUserName = "";
        if (Microsoft.Maui.Storage.Preferences.ContainsKey("UserHandle"))
        {
            defaultUserName = Microsoft.Maui.Storage.Preferences.Get("UserHandle", "");
            if(!string.IsNullOrEmpty(defaultUserName))
            {
                this.entryUserHandle.Text = defaultUserName;
            }
        }

        try
        {
            if (MainPage.Instance.Width > 400)
            {
                this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(200, GridUnitType.Absolute);
                this.entryUserHandle.WidthRequest = 180;
                this.entryPassword.WidthRequest = 180;
            }
            else
            {
                double ww = 400 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(250 - ww / 2, GridUnitType.Absolute);
                this.entryUserHandle.WidthRequest = 180 - ww / 2;
                this.entryPassword.WidthRequest = 180 - ww / 2;
            }
        }
        catch(Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupLogin ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);
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