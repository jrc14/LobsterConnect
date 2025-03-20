using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupViewPersons : Popup
{
    public PopupViewPersons()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);

        if (MainPage.Instance.Width < 350)
        {
            this.colDef0.Width = new GridLength(125, GridUnitType.Absolute);
            this.colDef1.Width = new GridLength(125, GridUnitType.Absolute);
        }
        else if (MainPage.Instance.Width < 400)
        {
            this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
            this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
        }
        else
        {
            this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
            this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
        }
    }

    public void SetPersons(List<string> personHandles)
    {
        try
        {
            if (personHandles == null || personHandles.Count == 0)
            {
                Logger.LogMessage(Logger.Level.ERROR, "PopupViewPersons.SetPersons: list mustn't be null or empty");
            }
            else
            {
                this.BindingContext = MainViewModel.Instance.GetPerson(personHandles[0]);
                if (personHandles.Count == 1)
                {
                    this.pickerPersons.IsVisible = false;
                    this.lbPickerPersons.IsVisible = false;
                }
                else
                {
                    this.pickerPersons.IsVisible = true;
                    this.lbPickerPersons.IsVisible = true;
                    this.pickerPersons.ItemsSource = personHandles;
                    this.pickerPersons.SelectedIndex = 0;
                }
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting person: "+ex.Message);
        }
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {

        await CloseAsync(true, CancellationToken.None);
    }

    private void pickerPersons_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            List<string> personHandles = this.pickerPersons.ItemsSource as List<string>;

            if (personHandles == null) return;
            int i = this.pickerPersons.SelectedIndex;
            if (i < 0) return;
            if (i >= personHandles.Count) return;
            Person p = MainViewModel.Instance.GetPerson(personHandles[i]);
            if (p is null) return;

            this.BindingContext = p;
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error selecting person: " + ex.Message);
        }

    }

    private void btnCopyEmail_Clicked(object sender, EventArgs e)
    {
        Person p = this.BindingContext as Person;
        if (p != null && !string.IsNullOrEmpty(p.Email))
        {
            Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(p.Email);
            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Copied text: '" + p.Email + "'");
        }
    }

    private void btnCopyPhoneNumber_Clicked(object sender, EventArgs e)
    {
        Person p = this.BindingContext as Person;
        if (p != null && !string.IsNullOrEmpty(p.PhoneNumber))
        {
            Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(p.PhoneNumber);
            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Copied text: '" + p.PhoneNumber + "'");
        }
    }
}