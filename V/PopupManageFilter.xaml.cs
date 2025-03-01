using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupManageFilter : Popup
{
    public PopupManageFilter()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnSave, this.btnCancel, this.rdefButtons);
    }

    public void SetFilter(SessionFilter f)
    {
        List<string> persons = MainViewModel.Instance.GetAvailablePersons(true);
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

        await CloseAsync(f, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(false, CancellationToken.None);
    }

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