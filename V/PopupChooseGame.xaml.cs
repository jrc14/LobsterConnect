/*
    Copyright (C) 2025 Turnipsoft Ltd, Jim Chapman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
namespace LobsterConnect.V;

/// <summary>
/// Popup for picking a game - the user can use the game selection picker to pick a game, or they
/// can create a new game if they need to; in that case the MainViewModel.Instance.CreateGame
/// method will be used to create it.
/// </summary>
public partial class PopupChooseGame : Popup
{
    public PopupChooseGame()
    {
        InitializeComponent();

        List<string> allGames = MainViewModel.Instance.GetAvailableGames();
        allGames.Sort();

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
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupChooseGame ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons, this.btnAddGame);

        // Populate the games list view (do this later because it takes a while to finish)
        Model.DispatcherHelper.RunAsyncOnUI(() => this.lvGame.ItemsSource = new ObservableCollection<string>(allGames));
    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(this.chosenGameName))
        {
            await MainPage.Instance.DisplayAlert("New Game", "Please choose or add a game before tapping OK", "Dismiss");
        }
        else
        {
            if (!string.IsNullOrEmpty(this.newGameName) && !string.IsNullOrEmpty(this.newGameBggLink))
            {
                try
                {
                    MainViewModel.Instance.CreateGame(true, this.newGameName, this.newGameBggLink);

                    MainViewModel.Instance.LogUserMessage(Model.Logger.Level.INFO, "Added a new game: " + this.newGameName);
                }
                catch (Exception ex)
                {
                    MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error creating game: " + ex.Message);
                }
            }

            await CloseAsync(this.chosenGameName, CancellationToken.None);
        }
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }

    /// <summary>
    /// Call this method to set the filter string that is used to restrict the list of
    /// available game names.  Don't call it too often because it does a lot of work on the UI
    /// thread.  Use the method ThrottledSetGameNameFilter instead of calling this one directly
    /// in order to avoid calling this method too often.
    /// Note BTW that there is, in theory, a clever optimisation you could use, whereby, when the filter
    /// extended ("Bob" becomes "Bobby" for instance) you filter in place the exisitng contents of 
    /// this.lvGame.ItemsSource rather then generating them all again.  In practice this doesn't work because the
    /// operation of removing elements from an observable collection is horribly slow on iOS.
    /// </summary>
    /// <param name="newFilter"></param>
    private void SetGameNameFilter(string newFilter)
    {
        try
        {
            List<string> existingGames = MainViewModel.Instance.GetAvailableGames();
            existingGames.Sort();

            if (!string.IsNullOrEmpty(newFilter))
            {
                for (int i = existingGames.Count - 1; i >= 0; i--) // work down from the top, so we can can remove items as we go
                {
                    if (!existingGames[i].Contains(newFilter, StringComparison.InvariantCultureIgnoreCase))
                        existingGames.RemoveAt(i);
                }
            }

            this.lvGame.ItemsSource = new ObservableCollection<string>(existingGames);

        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error setting filter: " + ex.Message);
        }
    }

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


    private string newGameName = null;
    private string newGameBggLink = null;
    private string chosenGameName = null;

    private async void OnAddGameClicked(object sender, EventArgs e)
    {
        List<string> existingGames = MainViewModel.Instance.GetAvailableGames();

        string g = await MainPage.Instance.DisplayPromptAsync("New Game", "Enter the name of the game you want to add");

        if (string.IsNullOrEmpty(g))
            return;

        if (g.Contains(','))
        {
            await MainPage.Instance.DisplayAlert("New Game", "Game names must not contain commas", "Dismiss");
            return;
        }

        if (existingGames.Contains(g))
        {
            await MainPage.Instance.DisplayAlert("New Game", "There is already a game called '" + newGameName + "'", "Dismiss");
            return;
        }


        string l  = await MainPage.Instance.DisplayPromptAsync("New Game", "Please enter a BGG URL link for '" + g + "'");
        if (string.IsNullOrEmpty(l))
        {
            await MainPage.Instance.DisplayAlert("New Game", "The BGG URL Link can't be left blank'", "Dismiss");
            return;
        }

        this.newGameName = g;
        this.newGameBggLink = l;
        this.chosenGameName = g;

        this.lblChosenGame.Text = "You've chosen: " + this.chosenGameName;
    }

    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lvGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            string g = e.CurrentSelection.Last() as string;
            if(!string.IsNullOrEmpty(g))
            {
                this.chosenGameName = g;
                this.newGameName = null;
                this.newGameBggLink = null;

                this.lblChosenGame.Text = "You've chosen: " + this.chosenGameName;
            }
        }
    }

    void btnHelpClicked(Object o, EventArgs e)
    {
        MainPage.Instance.ShowPopup(new PopupHints().SetUp("ChooseGame", false));
    }
}
