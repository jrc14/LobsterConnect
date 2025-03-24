using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupItemsViewer : Popup
{
    public PopupItemsViewer()
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

    async void OnDismissClicked(object sender, EventArgs e)
    {
        string result = null;

        List<string> items = this.pickerItems.ItemsSource as List<string>;
        int i = this.pickerItems.SelectedIndex;
        if (items != null && i >= 0 && i < items.Count)
            result = items[i];
        await CloseAsync(result, CancellationToken.None);
    }

}