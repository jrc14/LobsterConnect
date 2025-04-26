
using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using System.Collections.ObjectModel;
namespace LobsterConnect.V;

/// <summary>
/// Popup for entering the details of a new gaming session.  Once you've created it, call
/// SetTimeSlot if you want the time slot selection control to have an initial value.
/// If OK is clicked the popup will create a new session using MainViewModel.Instance.CreateSession,
/// and then close the popup.
/// The user can also use the game selection picker to create a new game if they need to; in that
/// case the MainViewModel.Instance.CreateGame method will be used to create it.
/// </summary>
public partial class PopupAddSession : Popup
{
    public PopupAddSession()
    {
        InitializeComponent();

        List<string> timeSlotLabels = new List<string>();

        // Note: SessionTime.NumberOfTimeSlots will vary depending on what kind of event
        // the vm's current event is.  We don't have to worry about that; the vm and the
        // SessionTime class will take care of this between them.
        for (int s=0;s<SessionTime.NumberOfTimeSlots; s++)
        {
            SessionTime t = new SessionTime(s);
            timeSlotLabels.Add(t.ToString());
        }

        this.pickerStartTime.ItemsSource = timeSlotLabels;

        try
        {
            if (MainPage.Instance.Width > 450)
            {
                this.colDef0.Width = new GridLength(250, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(250 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupAddSession ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons, this.btnChooseGame);
    }

    private string chosenGameName = null;
    public void SetChosenGame(string g)
    {
        this.chosenGameName = g;
        this.btnChooseGame.Text = "To play: " + g;
    }

    /// <summary>
    /// Set the position of the time slot selection picker.
    /// </summary>
    /// <param name="s"></param>
    public void SetTimeSlot(int s)
    {
        try
        {
            this.pickerStartTime.SelectedIndex = s;
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error setting time slot index: " + ex.Message);
        }
    }

    private async void OnChooseGameClicked(object sender, EventArgs e)
    {
        var popup = new PopupChooseGame();
        var popupResult = await MainPage.Instance.ShowPopupAsync(popup, CancellationToken.None);

        string s = popupResult as string;
        if(!string.IsNullOrEmpty(s))
        {
            Model.DispatcherHelper.RunAsyncOnUI(() => { SetChosenGame(s); });
        }
    }
    async void OnOkClicked(object sender, EventArgs e)
    {
        List<string> timeSlotLabels = this.pickerStartTime.ItemsSource as List<string>;
        string selectedTimeSlotLabel = this.pickerStartTime.SelectedItem as string;
        int selectedTimeIndex = timeSlotLabels.IndexOf(selectedTimeSlotLabel);
        if(selectedTimeIndex==-1)
        {
            await MainPage.Instance.DisplayAlert("Add Gaming Session", "Please select a time for this session", "Dismiss");
        }    
        else if (string.IsNullOrEmpty(this.chosenGameName))
        {
            await MainPage.Instance.DisplayAlert("Add Gaming Session", "Please select a game to be played in this session", "Dismiss");
        }
        else
        {
            try
            { 
                string notes = await MainPage.Instance.DisplayPromptAsync("Add Gaming Session", "Please add any note that you want to display on this session. Do not enter text that is offensive or defamatory, or contains information about any person.");

                string whatsAppLink = await MainPage.Instance.DisplayPromptAsync("Add Gaming Session", "If you want to associate a chat with this game, add the link here (for a WhatsApp chat, get an 'invite to group' link for the chat, and paste it here).");

                if (string.IsNullOrEmpty(notes)) notes = "NO NOTES";
                if (string.IsNullOrEmpty(whatsAppLink)) whatsAppLink = "NO CHAT LINK";

                int sitsMinimum = (int)Double.Round(this.stpMinimum.Value);
                int sitsMaximum = (int)Double.Round(this.stpMaximum.Value);

                string proposer = MainViewModel.Instance.LoggedOnUser.Handle;
                string eventName = MainViewModel.Instance.CurrentEvent.Name;

                SessionTime startTime = new SessionTime(selectedTimeIndex);

                string sessionId = Guid.NewGuid().ToString();

                MainViewModel.Instance.CreateSession(true, sessionId, proposer, this.chosenGameName, eventName, startTime, true, notes, whatsAppLink, null /*default to BGG link for the game*/ , sitsMinimum, sitsMaximum);

                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.INFO, "You (user '"+proposer+"') have created a session to play '"+this.chosenGameName+"' at "+startTime.ToString());

            }
            catch(Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error adding session: " +ex.Message);
            }

            await CloseAsync(true, CancellationToken.None);         
        }
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(false, CancellationToken.None);
    }

    private void stpMaximum_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int v = (int)Double.Round(e.NewValue);

        this.lblMaximum.Text = "Sits Maximum: " + v.ToString();
    }

    private void stpMinimum_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int v = (int)Double.Round(e.NewValue);

        this.lblMinimum.Text = "Sits Minimum: " + v.ToString();
    }
}
