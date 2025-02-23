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