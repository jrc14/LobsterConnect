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

using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;

namespace LobsterConnect.V;

/// <summary>
/// Popup for displaying the games that the logged on user has included in their 'would like to play' wish-list,
/// and for adding and removing games.  Don't show this popup when there is no logged on user,
/// because it won't do anything useful.
/// </summary>
public partial class PopupManageWishList : Popup
{
    public PopupManageWishList()
    {
        InitializeComponent();

        LoadWishList();

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
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupManageWishList ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons, this.btnAdd);
    }

    /// <summary>
    /// Populates the grid with all the wish-list entries for the currently logged on user,
    /// at the currently selected gaming event.
    /// </summary>
    void LoadWishList()
    {
        if (MainViewModel.Instance.LoggedOnUser == null)
            return;

        this.gdGames.RowDefinitions.Clear();
        this.gdGames.Children.Clear();

        int r = 0;
        List<WishListItem> items = MainViewModel.Instance.GetWishListItemsForPerson(MainViewModel.Instance.LoggedOnUser.Handle);
        foreach(WishListItem item in items)
        {
            int interest = MainViewModel.Instance.GetWishListItemsForGame(item.Game).Count;

            gdGames.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            gdGames.Children.Add(new Label()
            {
                Text = item.Game
            }.Row(r).Column(0));

            gdGames.Children.Add(new Label()
            {
                Text = item.Notes
            }.Row(r).Column(1));

            string btnLabel = "";
            for(int i=0;i<interest;i++)
            {
                btnLabel += "*";
            }

            gdGames.Children.Add(new Button()
            {
                Text = btnLabel,
                TextColor = Colors.White,
                Background = Colors.DarkGray,
                BorderColor = Colors.White,
                BorderWidth = 1,
                CornerRadius = 2,
                Margin=new Thickness(5),
                BindingContext = item
            }.Row(r).Column(2)
            .Invoke(b=>b.Clicked+=(object sender, EventArgs e)=>
            {
                Button btnSender = sender as Button;
                if (btnSender == null) return;
                WishListItem itemSender = btnSender.BindingContext as WishListItem;
                if (itemSender == null) return;
                Model.DispatcherHelper.RunAsyncOnUI(() => ShowItemActions(itemSender));
            }));

            r++;
        }
    }

    /// <summary>
    /// Display an action sheet for the things that the user can do with an existing wish-list item.
    /// </summary>
    /// <param name="item"></param>
    async void ShowItemActions(WishListItem item)
    {
        try
        {
            string option = await MainPage.Instance.DisplayActionSheet("Would Like to Play", "Dismiss", null,
                "No longer interested",
                "Update my notes",
                "Who else is interested?",
                "Propose a session to play");

            if (option == "No longer interested")
            {
                MainViewModel.Instance.DeleteWishList(true, item.Person, item.Game, item.GamingEvent);
                Model.DispatcherHelper.RunAsyncOnUI(LoadWishList);
            }
            else if (option == "Update my notes")
            {
                string notes = await MainPage.Instance.DisplayPromptAsync("Would Like to Play", "Please enter your updated notes regarding the game");

                if (notes == null) // Cancel
                    return;

                MainViewModel.Instance.UpdateWishList(true, item.Person, item.Game, item.GamingEvent, notes);
                Model.DispatcherHelper.RunAsyncOnUI(LoadWishList);
            }
            else if (option == "Who else is interested?")
            {
                var popup = new PopupViewWishList();
                popup.SetGame(item.Game);
                MainPage.Instance.ShowPopup(popup);
            }
            else if (option == "Propose a session to play")
            {
                await CloseAsync(null, CancellationToken.None);
                Model.DispatcherHelper.RunAsyncOnUI(async () =>
                {
                    if (!PopupHints.DontShowAgain("AddSession"))
                        await MainPage.Instance.ShowPopupAsync(new PopupHints().SetUp("AddSession", true), CancellationToken.None);

                    var popup = new PopupAddSession();
                    popup.SetChosenGame(item.Game);
                    MainPage.Instance.ShowPopup(popup);
                });
            }
        }
        catch(Exception ex)
        {
            await MainPage.Instance.DisplayAlert("Would Like to Play", "Sorry an error happened: " + ex.Message, "Dismiss");
        }
    }
    /// <summary>
    /// Dismiss just closes the popup; there is no state to be saved.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }

    /// <summary>
    /// Add a game to the user's wish-list
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnAddClicked(object sender, EventArgs e)
    {
        try
        {
            if (!PopupHints.DontShowAgain("ChooseGame"))
                await MainPage.Instance.ShowPopupAsync(new PopupHints().SetUp("ChooseGame", true), CancellationToken.None);

            var popup = new PopupChooseGame();
            var popupResult = await MainPage.Instance.ShowPopupAsync(popup, CancellationToken.None);

            string s = popupResult as string;

            if (string.IsNullOrEmpty(s))
                return;

            // Note that GetWishListItemsForPerson will only look at items for the currently selected event,
            // which is the behaviour we want here.
            bool duplicate = MainViewModel.Instance.GetWishListItemsForPerson(MainViewModel.Instance.LoggedOnUser.Handle)
                    .Any(w => w.Game == s);

            if (duplicate)
            {
                await MainPage.Instance.DisplayAlert("Would Like to Play", "That game is on your list already; you can't add it again", "Dismiss");
            }
            else
            {
                string notes = await MainPage.Instance.DisplayPromptAsync("Would Like to Play", "Please enter notes regarding the game - for instance, when are you hoping to play, or is there a particular variant you want to play? Do not enter text that is offensive or defamatory, or contains information about any person.");

                if (notes == null) // Cancel
                    return;

                MainViewModel.Instance.CreateWishList(true, MainViewModel.Instance.LoggedOnUser.Handle, s, MainViewModel.Instance.CurrentEvent.Name, notes);

                Model.DispatcherHelper.RunAsyncOnUI(LoadWishList);
            }
        }
        catch(Exception ex)
        {
            await MainPage.Instance.DisplayAlert("Would Like to Play", "Sorry an error happened: "+ex.Message, "Dismiss");
        }
    }

    void btnHelpClicked(Object o, EventArgs e)
    {
        MainPage.Instance.ShowPopup(new PopupHints().SetUp("ManageWishList", false));
    }
}