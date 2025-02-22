using CommunityToolkit.Maui.Views;
namespace LobsterConnect.V;

public partial class PopupLogIn : Popup
{
	public PopupLogIn()
	{
		InitializeComponent();

#if ANDROID
        this.btnOk.BackgroundColor = Colors.Transparent;
        this.btnCancel.BackgroundColor = Colors.Transparent;
        this.btnOk.TextColor = Colors.Black;
        this.btnCancel.TextColor = Colors.Black;
        this.btnCancel.HorizontalOptions = LayoutOptions.Start;
        this.btnOk.HorizontalOptions = LayoutOptions.End;
        this.btnCancel.VerticalOptions = LayoutOptions.End;
        this.btnOk.VerticalOptions = LayoutOptions.End;
        this.btnCancel.FontAttributes = FontAttributes.Bold;
        this.btnOk.FontAttributes = FontAttributes.Bold;
#elif WINDOWS
        this.btnOk.BackgroundColor = Colors.LightGrey;
        this.btnCancel.BackgroundColor = Colors.LightGrey;
        this.btnOk.TextColor = Colors.Black;
        this.btnCancel.TextColor = Colors.Black;
        this.rdefButtons.Height = new GridLength(80);
        this.btnCancel.HorizontalOptions = LayoutOptions.Center;
        this.btnOk.HorizontalOptions = LayoutOptions.Center;
        this.btnCancel.VerticalOptions = LayoutOptions.Center;
        this.btnOk.VerticalOptions = LayoutOptions.Center;

#endif

        string defaultUserName = "";
        if (Microsoft.Maui.Storage.Preferences.ContainsKey("userhandle"))
        {
            defaultUserName = Microsoft.Maui.Storage.Preferences.Get("userhandle", "");
            if(!string.IsNullOrEmpty(defaultUserName))
            {
                this.entryUserHandle.Text = defaultUserName;
            }
        }
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        Tuple<string, string> t = new Tuple<string, string>(this.entryUserHandle.Text, this.entryPassword.Text);

        await CloseAsync(t, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }
}