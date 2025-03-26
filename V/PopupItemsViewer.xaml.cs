using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupItemsViewer : Popup
{
    public PopupItemsViewer()
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
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupItemsViewer ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDone, this.rdefButtons);
    }

    public void SetItems(List<string> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                Logger.LogMessage(Logger.Level.ERROR, "PopupItemsViewer.SetItems: list mustn't be null or empty");
            }
            else
            {
                this.pickerItems.ItemsSource = items;
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting items: " + ex.Message);
        }
    }

    async void OnDoneClicked(object sender, EventArgs e)
    {
        string result = null;

        List<string> items = this.pickerItems.ItemsSource as List<string>;
        int i = this.pickerItems.SelectedIndex;
        if (items != null && i >= 0 && i < items.Count)
            result = items[i];
        await CloseAsync(result, CancellationToken.None);
    }

}