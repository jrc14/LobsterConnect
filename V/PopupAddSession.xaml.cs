
using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using System.Collections.ObjectModel;
namespace LobsterConnect.V;

public partial class PopupAddSession : Popup
{
    public PopupAddSession()
    {
        InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);

        List<string> allGames = MainViewModel.Instance.GetAvailableGames();
        allGames.Sort();
        allGames.Insert(0, ADD_GAME_TEXT);

        List<string> timeSlotLabels = new List<string>();

        for(int s=0;s<SessionTime.NumberOfTimeSlots; s++)
        {
            SessionTime t = new SessionTime(s);
            timeSlotLabels.Add(t.ToString());
        }

        this.pickerStartTime.ItemsSource = timeSlotLabels;

        if (MainPage.Instance.Width < 350)
        {
            this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
            this.entryGameFilter.WidthRequest = 50;
            this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
        }
        else if (MainPage.Instance.Width<400)
        {
            this.colDef0.Width = new GridLength(200, GridUnitType.Absolute);
            this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
            this.entryGameFilter.WidthRequest = 50;
        }
        else
        {
            this.colDef0.Width = new GridLength(250, GridUnitType.Absolute);
            this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
            this.entryGameFilter.WidthRequest = 90;
        }

        // Do this later because it takes a while
        Model.DispatcherHelper.RunAsyncOnUI(() => this.lvGame.ItemsSource = new ObservableCollection<string>(allGames));
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        //Tuple<string, string, bool> t = new Tuple<string, string, bool>(this.entryUserHandle.Text, this.entryPassword.Text, this.chkRememberMe.IsChecked);

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

                if (this.lvGame.SelectedItem as string ==null)
                    gameName = this.addedGameName;
                else
                    gameName = this.lvGame.SelectedItem as string;

                string notes = await MainPage.Instance.DisplayPromptAsync("Add Gaming Session", "Please add any note that you want to display on this session");

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

    private void SetGameNameFilter(string newFilter)
    {
        try
        {
            //Model.Logger.LogMessage(Model.Logger.Level.DEBUG, "SetGameNameFilter:", "'" + newFilter + "'");
            if (string.IsNullOrEmpty(newFilter))
            {
                // if the name filter is cleared, then reload the picker's items source
                currentGameNameFilter = "";

                List<string> existingGames = MainViewModel.Instance.GetAvailableGames();
                existingGames.Sort();

                existingGames.Insert(0, ADD_GAME_TEXT);

                this.lvGame.ItemsSource = new ObservableCollection<string>(existingGames);

            }
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

    System.Threading.Timer filterTimer = null;
    private string _pendingNewFilter = null;
    private void ThrottledSetGameNameFilter(string newFilter)
    {
        //Model.Logger.LogMessage(Model.Logger.Level.DEBUG, "new:", "'" + newFilter + "'");

        _pendingNewFilter = newFilter;

        Model.DispatcherHelper.StartTimer(ref filterTimer, 500, () =>
        {
            Model.DispatcherHelper.RunAsyncOnUI(() =>
            {
                //Model.Logger.LogMessage(Model.Logger.Level.DEBUG, "applying:", "'" + _pendingNewFilter + "'");
                SetGameNameFilter(_pendingNewFilter);
            });
        });
    }

    private const string ADD_GAME_TEXT = "[Add a game not in the list below]";
    private string addedGameName = null;

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
