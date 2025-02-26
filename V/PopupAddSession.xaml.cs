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
            timeSlotLabels.Add(t.DayLabel + " " + t.TimeLabel);
        }

        this.pickerStartTime.ItemsSource = timeSlotLabels;

        // Do this later because it takes a while
        Model.DispatcherHelper.RunAsyncOnUI(() => this.lvGame.ItemsSource = new ObservableCollection<string>(allGames));
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

    private void SetGameNameFilter(string newFilter)
    {
        //Model.Logger.LogMessage(Model.Logger.Level.DEBUG, "SetGameNameFilter:", "'" + newFilter + "'");
        if(string.IsNullOrEmpty(newFilter))
        {
            // if the name filter is cleared, then reload the picker's items source
            currentGameNameFilter = "";

            List<string> existingGames = MainViewModel.Instance.GetAvailableGames();
            existingGames.Sort();

            existingGames.Insert(0, ADD_GAME_TEXT);

            this.lvGame.ItemsSource = new ObservableCollection<string>(existingGames);

        }
        else if(string.IsNullOrEmpty(currentGameNameFilter) || newFilter.Contains(currentGameNameFilter, StringComparison.InvariantCultureIgnoreCase))
        {
            // if the name filter is set to a value that is more specific than the existing name filter,
            // then weed out elments of the picker's existing item source.

            ObservableCollection<string> existingFilteredGames = (ObservableCollection<string>)this.lvGame.ItemsSource as ObservableCollection<string>;

            if(existingFilteredGames!=null && existingFilteredGames.Count()>0) // no good reason for it to be null or empty, but best be careful
            {
                // Count backwards from the last element of the list, removing strings that don't match the new
                // filter.  Count backwards because removing the last item doesn't change the index
                // of any earlier item in the list; stop before 1 because we want to keep the "[add a game ..." label in position 0.
                for(int i=existingFilteredGames.Count()-1; i>0; i--)
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

    private async void lvGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count==1 && e.CurrentSelection[0] as string == ADD_GAME_TEXT) // Selected the entry with the "[add a game ..." label
        {
            string newGameName = await MainPage.Instance.DisplayPromptAsync("New Game", "Enter the name of the game you want to add");
        }
    
    }
}
