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
using LobsterConnect.Model;
using LobsterConnect.VM;
using Microsoft.Maui.LifecycleEvents;
using System.Collections.ObjectModel;
namespace LobsterConnect.V;

public partial class PopupViewGames : Popup
{
    /// <summary>
    /// Popup for displaying a list of game names, and for viewing details of who has signed up
    /// to play them, or added them to their wish-list.
    /// </summary>
    public PopupViewGames()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 450)
            {
                this.colDef0.Width = new GridLength(200, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(200, GridUnitType.Absolute);
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(200 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(200 - ww / 2, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupViewGames ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);

        Model.DispatcherHelper.RunAsyncOnUI(() => { this.rbtnAll.IsChecked = true; }); // start with 'all games' as the chosen list option
    }

    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }

    /// <summary>
    /// When an item in the list vew is selected, show an action sheet asking what to do with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void lvGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            if (MainViewModel.Instance.LoggedOnUser == null)
            {
                await MainPage.Instance.DisplayAlert("Games List", "Log in first please, before proposing a gaming session or setting up a wish-list", "Dismiss");
                return;
            }
            else if (!MainViewModel.Instance.CurrentEvent.IsActive)
            {
                await MainPage.Instance.DisplayAlert("Games List", "The current gaming event '" + MainViewModel.Instance.CurrentEvent.Name + "' is not active", "Dismiss");
                return;
            }

            string g = e.CurrentSelection.Last() as string;
            if(!string.IsNullOrEmpty(g))
            {
                if (g.Contains(','))
                    g = g.Split(',')[0];

                Game gg = MainViewModel.Instance.GetGame(g);
                if (gg is null) return;

                string action = await MainPage.Instance.DisplayActionSheet("Game Actions:", "Dismiss", null, "Propose a Gaming Session", "Would Like to Play");

                if(action == "Propose a Gaming Session")
                {
                    try
                    {

                        if (!PopupHints.DontShowAgain("AddSession"))
                            await MainPage.Instance.ShowPopupAsync(new PopupHints().SetUp("AddSession", true), CancellationToken.None);

                        var popup = new PopupAddSession();
                        popup.SetChosenGame(gg.Name);
                        var popupResult = await MainPage.Instance.ShowPopupAsync(popup, CancellationToken.None);

                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            this.PopulateListView(null, null); // null means 'load with the same thing as last time'
                        });
                    }
                    catch (Exception ex)
                    {
                        MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error adding session: " + ex.Message);
                    }
                }
                else if(action == "Would Like to Play")
                {
                    // Note that GetWishListItemsForPerson will only look at items for the currently selected event,
                    // which is the behaviour we want here.
                    bool duplicate = MainViewModel.Instance.GetWishListItemsForPerson(MainViewModel.Instance.LoggedOnUser.Handle)
                            .Any(w => w.Game == gg.Name);

                    if (duplicate)
                    {
                        await MainPage.Instance.DisplayAlert("Would Like to Play", "That game is on your list already; you can't add it again", "Dismiss");
                    }
                    else
                    {
                        string notes = await MainPage.Instance.DisplayPromptAsync("Would Like to Play", "Please enter notes regarding the game - for instance, when are you hoping to play, or is there a particular variant you want to play? Do not enter text that is offensive or defamatory, or contains information about any person.");

                        if (notes == null) // Cancel
                            return;

                        MainViewModel.Instance.CreateWishList(true, MainViewModel.Instance.LoggedOnUser.Handle, gg.Name, MainViewModel.Instance.CurrentEvent.Name, notes);

                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            this.PopulateListView(null, null); // null means 'load with the same thing as last time'
                        });
                    }
                }
            }
        }
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if(e.Value) // button is now checked
        {
            RadioButton btnSender = sender as RadioButton;
            if (btnSender == null)
                return;

            PopulateListView(btnSender.Value as string, null);
        }
    }

    // remember the last game type loaded, and the game name filter in the listview, so we can do it again if necessary
    string _latestGameType = null;
    string _latestGameFilter = null;

    private void PopulateListView(string gameType=null, string gameFilter=null)
    {
        if (gameType == null && _latestGameType != null)
            gameType = _latestGameType;

        if (gameFilter == null && _latestGameFilter != null)
            gameFilter = _latestGameFilter;

        _latestGameType = gameType;
        _latestGameFilter= gameFilter;

        List<string> games = MainViewModel.Instance.GetAvailableGames(null, gameFilter);
        games.Sort();

        if (gameType as string == "all")
        {
            this.lvGames.ItemsSource = games;
        }
        else if (gameType as string == "sessions")
        {
            List<string> sessionLabels = new List<string>();
            foreach (string g in games)
            {
                List<Session> sessions = MainViewModel.Instance.GetSessionsForGame(g);
                if (sessions != null && sessions.Count > 0)
                {
                    sessions.Sort();
                    sessionLabels.Add(g + ", " + String.Join(',', sessions.Select(s => s.StartAt.ToString())));
                }
            }
            this.lvGames.ItemsSource = sessionLabels;
        }
        else if (gameType as string == "wishlist")
        {
            List<string> wishListabels = new List<string>();
            foreach (string g in games)
            {
                List<WishListItem> wishList = MainViewModel.Instance.GetWishListItemsForGame(g);
                if (wishList != null && wishList.Count > 0)
                {
                    wishListabels.Add(g + ", " + String.Join(',', wishList.Select(s => s.Person)));
                }
            }
            this.lvGames.ItemsSource = wishListabels;
        }
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
            PopulateListView(null, newFilter);
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

    async void btnHelpClicked(Object o, EventArgs e)
    {
        await MainPage.Instance.ShowPopupAsync(new PopupHints().SetUp("ViewGames", false));
    }
}