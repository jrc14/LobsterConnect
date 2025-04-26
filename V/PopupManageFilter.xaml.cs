using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using System.Globalization;
namespace LobsterConnect.V;

/// <summary>
/// Popup for managing the filter that is used to restrict the game sessions shown in the
/// table in the main UI.  To set the initial value for the controls (proposer, game name,
/// people signed up, session state) pass a suitable SessionFilter to the SetFilter method on the
/// popup.  If the user clicks the save button, then the popup will return a result consisting of 
/// a new filter, populated according to the values now in the controls (proposer, game name,
/// people signed up, session state).
/// The proposer and signups controls are pickers for choosing person names.  Because there
/// could be a lot of person names to go through, each of these controls is accompanied by a text
/// entry field that can be used to find the right person.
/// </summary>
public partial class PopupManageFilter : Popup
{
    public PopupManageFilter()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 450)
            {
                this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(250, GridUnitType.Absolute);
                this.pickerProposer.WidthRequest = 150;
                this.entryProposer.WidthRequest = 80;
                this.pickerSignUps.WidthRequest = 150;
                this.entrySignUps.WidthRequest = 80;
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(150-ww/3, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(250-2*ww/3, GridUnitType.Absolute);
                this.pickerProposer.WidthRequest = 150 - ww / 3;
                this.entryProposer.WidthRequest = 80 - ww / 3;
                this.pickerSignUps.WidthRequest = 150 - ww / 3;
                this.entrySignUps.WidthRequest = 80 - ww / 3;
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupManageFilter ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnSave, this.btnCancel, this.rdefButtons);
    }

    /// <summary>
    /// Pass in a filter whose contents will be read to set up the controls on the popup.
    /// </summary>
    /// <param name="f"></param>
    public void SetFilter(SessionFilter f)
    {
        List<string> persons = MainViewModel.Instance.GetAvailablePersons(true); // active persons only
        persons.Sort();

        persons.Insert(0, "[Any Person]");

        List<string> allStates = new List<string>() { "[Any State]", "OPEN", "FULL", "ABANDONED" };

        this.pickerProposer.ItemsSource = persons;
        if (!string.IsNullOrEmpty(f.Proposer) && persons.Contains(f.Proposer))
        {
            int p = persons.IndexOf(f.Proposer);
            this.pickerProposer.SelectedIndex = p;
        }
        else
        {
            this.pickerProposer.SelectedIndex = 0;
        }

        if(!string.IsNullOrEmpty(f.ToPlay))
        {
            this.entryGameName.Text = f.ToPlay;
        }

        this.pickerSignUps.ItemsSource = persons;
        if (!string.IsNullOrEmpty(f.SignUpsInclude) && persons.Contains(f.SignUpsInclude))
        {
            int p = persons.IndexOf(f.SignUpsInclude);
            this.pickerSignUps.SelectedIndex = p;
        }
        else
        {
            this.pickerSignUps.SelectedIndex = 0;
        }

        this.pickerState.ItemsSource = allStates;
        if(!string.IsNullOrEmpty(f.State))
        {
            this.pickerState.SelectedIndex = allStates.IndexOf(f.State);
        }
        else
        {
            this.pickerState.SelectedIndex = 0;
        }

        this.switchWatchList.IsToggled = f.OnWishList;
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        SessionFilter f = new SessionFilter();

        if (this.pickerProposer.SelectedIndex > 0)
            f.Proposer = this.pickerProposer.SelectedItem as string;

        if (!string.IsNullOrEmpty(this.entryGameName.Text))
            f.ToPlay = this.entryGameName.Text;

        if (this.pickerSignUps.SelectedIndex > 0)
            f.SignUpsInclude = this.pickerSignUps.SelectedItem as string;

        if (this.pickerState.SelectedIndex > 0)
            f.State = this.pickerState.SelectedItem as string;

        f.OnWishList = this.switchWatchList.IsToggled;

        MainViewModel.Instance.LogUserMessage(Model.Logger.Level.INFO, "Filter has been set: '"+f.Description+"'");

        await CloseAsync(f, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(false, CancellationToken.None);
    }

    /// <summary>
    /// When the proposer text entry field is changed, scroll the proposer picker to the first
    /// item that matches the text.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void entryProposer_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
            this.pickerProposer.SelectedIndex = 0;
        else
        {
            List<string> s=this.pickerProposer.ItemsSource as List<string>;

            if (s == null)
                return;

            for(int i=1; i<s.Count;i++)
            {
                if (s[i].StartsWith(e.NewTextValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.pickerProposer.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// When the signups text entry field is changed, scroll the signups picker to the first
    /// item that matches the text.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void entrySignUps_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
            this.pickerSignUps.SelectedIndex = 0;
        else
        {
            List<string> s = this.pickerSignUps.ItemsSource as List<string>;

            if (s == null)
                return;

            for (int i = 1; i < s.Count; i++)
            {
                if (s[i].StartsWith(e.NewTextValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.pickerSignUps.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}