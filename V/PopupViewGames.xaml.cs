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
                        var popup = new PopupAddSession();
                        popup.SetChosenGame(gg.Name);
                        var popupResult = await MainPage.Instance.ShowPopupAsync(popup, CancellationToken.None);

                        Model.DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            this.PopulateListView(null); // null means 'load with the same thing as last time'
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
                            this.PopulateListView(null); // null means 'load with the same thing as last time'
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

            PopulateListView(btnSender.Value as string);
        }
    }

    // remember the last game type that we loaded the listview with, so we can do it again if necessary
    string _latestGameType = null;
    private void PopulateListView(string gameType=null)
    {
        if (gameType == null && _latestGameType != null)
            gameType = _latestGameType;

        _latestGameType = gameType;

        if (gameType as string == "all")
        {
            List<string> gameLabels = MainViewModel.Instance.GetAvailableGames();
            gameLabels.Sort();
            this.lvGames.ItemsSource = gameLabels;
        }
        else if (gameType as string == "sessions")
        {
            List<string> games = MainViewModel.Instance.GetAvailableGames();
            games.Sort();
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
            List<string> games = MainViewModel.Instance.GetAvailableGames();
            games.Sort();
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
}