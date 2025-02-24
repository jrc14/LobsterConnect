using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupAddSession : Popup
{
    public PopupAddSession()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);

        List<string> existingGames = MainViewModel.Instance.GetAvailableGames();
        existingGames.Sort();

        existingGames.Insert(0, "[Add a game not in the list below]");

        List<string> timeSlotLabels = new List<string>();

        for(int s=0;s<SessionTime.NumberOfTimeSlots; s++)
        {
            SessionTime t = new SessionTime(s);
            timeSlotLabels.Add(t.DayLabel + " " + t.TimeLabel);
        }

        this.pickerGame.ItemsSource = existingGames;
        this.pickerStartTime.ItemsSource = timeSlotLabels;
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        //Tuple<string, string, bool> t = new Tuple<string, string, bool>(this.entryUserHandle.Text, this.entryPassword.Text, this.chkRememberMe.IsChecked);

        await CloseAsync(true, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }

    private void pickerGame_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}