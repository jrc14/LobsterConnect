
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

        List<string> allGames = MainViewModel.Instance.GetAvailableGames();
        allGames.Sort();
        allGames.Insert(0, ADD_GAME_TEXT);

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
                this.entryGameFilter.WidthRequest = 90;
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(250 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);

                this.entryGameFilter.WidthRequest = 50;
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupAddSession ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);

        // Populate the games list view (do this later because it takes a while to finish)
        Model.DispatcherHelper.RunAsyncOnUI(() => this.lvGame.ItemsSource = new ObservableCollection<string>(allGames));
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

    async void OnOkClicked(object sender, EventArgs e)
    {
        List<string> timeSlotLabels = this.pickerStartTime.ItemsSource as List<string>;
        string selectedTimeSlotLabel = this.pickerStartTime.SelectedItem as string;
        int selectedTimeIndex = timeSlotLabels.IndexOf(selectedTimeSlotLabel);
        if(selectedTimeIndex==-1)
        {
            await MainPage.Instance.DisplayAlert("Add Gaming Session", "Please select a time for this session", "Dismiss");
        }    
        else if (this.lvGame.SelectedItem ==null && this.addedGameName == null)
        {
            await MainPage.Instance.DisplayAlert("Add Gaming Session", "Please select a game to be played in this session", "Dismiss");
        }
        else
        {
            try
            {
                string gameName;

                // this.addedGameName gets set if the user has chosen to add a new game in this popup
                if (this.lvGame.SelectedItem as string ==null)
                    gameName = this.addedGameName;
                else
                    gameName = this.lvGame.SelectedItem as string;

                string notes = await MainPage.Instance.DisplayPromptAsync("Add Gaming Session", "Please add any note that you want to display on this session. Do not enter text that is offensive or defamatory, or contains information about any person.");

                string whatsAppLink = await MainPage.Instance.DisplayPromptAsync("Add Gaming Session", "If you want to associate a WhatsApp chat with this game, please get an 'invite to group' link for the chat, and paste it here");

                if (string.IsNullOrEmpty(notes)) notes = "NO NOTES";
                if (string.IsNullOrEmpty(whatsAppLink)) whatsAppLink = "NO WHATSAPP LINK";

                int sitsMinimum = (int)Double.Round(this.stpMinimum.Value);
                int sitsMaximum = (int)Double.Round(this.stpMaximum.Value);

                string proposer = MainViewModel.Instance.LoggedOnUser.Handle;
                string eventName = MainViewModel.Instance.CurrentEvent.Name;

                SessionTime startTime = new SessionTime(selectedTimeIndex);

                string sessionId = Guid.NewGuid().ToString();

                MainViewModel.Instance.CreateSession(true, sessionId, proposer, gameName, eventName, startTime, true, notes, whatsAppLink, null /*default to BGG link for the game*/ , sitsMinimum, sitsMaximum);

                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.INFO, "You (user '"+proposer+"') have created a session to play '"+gameName+"' at "+startTime.ToString());

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

    /// <summary>
    /// Call this method to set the filter string that is used to restrict the list of
    /// available game names.  Don't call it too often because it does a lot of work on the UI
    /// thread.  Use the method ThrottledSetGameNameFilter instead of calling this one directly
    /// in order to avoid calling this method too often
    /// </summary>
    /// <param name="newFilter"></param>
    private void SetGameNameFilter(string newFilter)
    {
        try
        {
            if (string.IsNullOrEmpty(newFilter))
            {
                // if the name filter is cleared, then reload the picker's items source
                currentGameNameFilter = "";

                List<string> existingGames = MainViewModel.Instance.GetAvailableGames();
                existingGames.Sort();

                existingGames.Insert(0, ADD_GAME_TEXT);

                this.lvGame.ItemsSource = new ObservableCollection<string>(existingGames);

            }
            /* REMOVED THIS CUNNING OPTIMISATION BECAUSE, ON IPHONE, REMOVING ELEMENTS FROM AN OBSERVABLE COLLECTION
             * IS A HORRIBLY SLOW OPERATION
            else if (string.IsNullOrEmpty(currentGameNameFilter) || newFilter.Contains(currentGameNameFilter, StringComparison.InvariantCultureIgnoreCase))
            {
                // if the name filter is set to a value that is more specific than the existing name filter,
                // then weed out elments of the picker's existing item source.

                ObservableCollection<string> existingFilteredGames = (ObservableCollection<string>)this.lvGame.ItemsSource as ObservableCollection<string>;

                if (existingFilteredGames != null && existingFilteredGames.Count() > 0) // no good reason for it to be null or empty, but best be careful
                {
                    // Count backwards from the last element of the list, removing strings that don't match the new
                    // filter.  Count backwards because removing the last item doesn't change the index
                    // of any earlier item in the list; stop before 1 because we want to keep the "[add a game ..." label in position 0.
                    for (int i = existingFilteredGames.Count() - 1; i > 0; i--)
                    {
                        if (!existingFilteredGames[i].Contains(newFilter, StringComparison.InvariantCultureIgnoreCase))
                            existingFilteredGames.RemoveAt(i);
                    }
                }

                currentGameNameFilter = newFilter;
            }
            */
            else
            {
                // The game filter is a new value, not contained within the old filter.  We must reload the
                // item source.
                List<string> existingFilteredGames = MainViewModel.Instance.GetAvailableGames();
                existingFilteredGames.Sort();

                existingFilteredGames.Insert(0, ADD_GAME_TEXT);
                for (int i = existingFilteredGames.Count - 1; i > 0; i--)
                {
                    if (!existingFilteredGames[i].Contains(newFilter, StringComparison.InvariantCultureIgnoreCase))
                        existingFilteredGames.RemoveAt(i);
                }

                this.lvGame.ItemsSource = new ObservableCollection<string>(existingFilteredGames);
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error setting filter: " + ex.Message);
        }
    }
    private string currentGameNameFilter="";

    private void entryGameFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        ThrottledSetGameNameFilter(e.NewTextValue);
    }


    /// <summary>
    /// Set the filter string that is used to restrict the list of available game names, but do so
    /// via a timer, which avoids actually changing the filter more often that two times per second.
    /// </summary>
    /// <param name="newFilter"></param>
    private void ThrottledSetGameNameFilter(string newFilter)
    {
        _pendingNewFilter = newFilter;

        Model.DispatcherHelper.StartTimer(ref filterTimer, 500, () =>
        {
            Model.DispatcherHelper.RunAsyncOnUI(() =>
            {
                SetGameNameFilter(_pendingNewFilter);
            });
        });
    }
    System.Threading.Timer filterTimer = null;
    private string _pendingNewFilter = null;

    // The string for the 'add another game' item at the top of the game picker.
    private const string ADD_GAME_TEXT = "[Add a game not in the list below]";
    private string addedGameName = null; // if the user chooses the 'add another game' option, this will be its name

    /// <summary>
    /// If the user selects the top item in the game picker, and it still contains the 'add a new game'
    /// prompt text, then prompt the user create a new game, and put its name at the top of the game
    /// picker (also set this.addedGameName to the name, so that it will be used for creating a
    /// new session, unless a different game was subsequently chosen using the picker control)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void lvGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count==1 && e.CurrentSelection[0] as string == ADD_GAME_TEXT) // Selected the entry with the "[add a game ..." label
        {
            ObservableCollection<string> lvStrings = this.lvGame.ItemsSource as ObservableCollection<string>;

            if (lvStrings == null)
                return;

            string newGameName = await MainPage.Instance.DisplayPromptAsync("New Game", "Enter the name of the game you want to add");

            if (string.IsNullOrEmpty(newGameName))
                return;

            if(lvStrings.Contains(newGameName))
            {
                await MainPage.Instance.DisplayAlert("New Game", "There is already a game called '" + newGameName + "'", "Dismiss");
                return;
            }


            string newGameBggLink = await MainPage.Instance.DisplayPromptAsync("New Game", "Please enter a BGG URL link for '" + newGameName + "'");
            if (string.IsNullOrEmpty(newGameBggLink))
            {
                await MainPage.Instance.DisplayAlert("New Game", "The BGG URL Link can't be left blank'", "Dismiss");
                return;
            }

            try
            {
                MainViewModel.Instance.CreateGame(true, newGameName, newGameBggLink);

                lvStrings[0] = newGameName;

                this.addedGameName = newGameName;
            }
            catch(Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error creating game: "+ex.Message);
            }

        }

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
