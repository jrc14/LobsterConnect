using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupChooseEvent : Popup
{
    public PopupChooseEvent()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);

        if (MainPage.Instance.Width < 350)
        {
            this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
 
        }
        else
        {
            this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
        }
    }

    public void SetEventList(List<string> eventNames, string initialEvent)
    {
        this.pickerEvents.ItemsSource = eventNames;
        int i = eventNames.IndexOf(initialEvent);

        if (i >= 0)
            this.pickerEvents.SelectedIndex = i;
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {

        await CloseAsync(null, CancellationToken.None);
    }

    async void OnOkClicked(object sender, EventArgs e)
    {

        await CloseAsync(this.pickerEvents.SelectedItem as string, CancellationToken.None);
    }

}