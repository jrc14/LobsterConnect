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
    }

    public void SetPersons(List<string> personHandles)
    {
        if(personHandles==null || personHandles.Count==0)
        {
            Logger.LogMessage(Logger.Level.ERROR, "PopupViewPersons.SetPersons: list mustn't be null or empty");
        }
        else
        {
            this.BindingContext = MainViewModel.Instance.GetPerson(personHandles[0]);
            if(personHandles.Count==1)
            {
                this.pickerPersons.IsVisible = false;
                this.lbPickerPersons.IsVisible = false;
            }
            else
            {
                this.pickerPersons.IsVisible = true;
                this.lbPickerPersons.IsVisible = true;
                this.pickerPersons.ItemsSource = personHandles;
            }    
        }    
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {

        await CloseAsync(true, CancellationToken.None);
    }
}