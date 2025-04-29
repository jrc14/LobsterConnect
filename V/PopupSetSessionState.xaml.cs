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
namespace LobsterConnect.V;

public partial class PopupSetSessionState : Popup
{
    /// <summary>
    /// Popup for changing the state of a session between OPEN, FULL and ABANDONED.  To associate
    /// the popup with a certain session, call the SetSession method after constructing the dialog.
    /// The buttons on the popup change the session state immediately by calling
    /// MainViewModel.Instance.UpdateSession.  There is no 'undo' function - if you want to change
    /// a session back to the previous state, then just tap on the button for that state.
    /// </summary>
    public PopupSetSessionState()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 350)
            {
                this.colDef0.Width = new GridLength(150, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150, GridUnitType.Absolute);
            }
            else
            {
                double ww = 350 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(150 - ww / 2, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupSetSessionState ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnDismiss, null, this.rdefButtons);
    }

    public void SetSession(Session s)
    {
        this.BindingContext = s;
    }

    void LabelOpen_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s == null || MainViewModel.Instance.LoggedOnUser == null)
            return;

        MainViewModel.Instance.UpdateSession(true, s, state: "OPEN");
        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to OPEN by '" + MainViewModel.Instance.LoggedOnUser.Handle + "'");
    }

    void LabelFull_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s == null || MainViewModel.Instance.LoggedOnUser == null)
            return;

        MainViewModel.Instance.UpdateSession(true, s, state: "FULL");
        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to FULL by '" + MainViewModel.Instance.LoggedOnUser.Handle + "'");
    }

    void LabelAbandoned_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s == null || MainViewModel.Instance.LoggedOnUser == null)
            return;

        MainViewModel.Instance.UpdateSession(true, s, state: "ABANDONED");
        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to ABANDONED by '" + MainViewModel.Instance.LoggedOnUser.Handle + "'");

    }

    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(null, CancellationToken.None);
    }
}