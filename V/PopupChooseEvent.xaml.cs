using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupChooseEvent : Popup
{
    public PopupChooseEvent()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 350)
            {
                this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
            }
            else
            {
                double ww = 350 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupChooseEvent ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);
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