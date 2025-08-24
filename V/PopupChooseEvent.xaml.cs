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

/// <summary>
/// A popup for choosing between gaming events.  After constructing it, call
/// SetEventList(eventNames, initialEvent) to supply a list of events to choose between
/// and an initial selection for the picker control.  If a selection is made and OK is clicked
/// then the popup's return value will be the chosen event.
/// </summary>
public partial class PopupChooseEvent : Popup
{
    public PopupChooseEvent()
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
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupChooseEvent ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);
    }

    /// <summary>
    /// Set the list of events to choose between and the initial value
    /// </summary>
    /// <param name="eventNames">list of event names to choose between</param>
    /// <param name="initialEvent">the event name that will be selected initially</param>
    public void SetEventList(List<string> eventNames, string initialEvent)
    {
        this.pickerEvents.ItemsSource = eventNames;
        int i = eventNames.IndexOf(initialEvent);

        if (i >= 0)
            this.pickerEvents.SelectedIndex = i;
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {

        await CloseAsync(null, CancellationToken.None);
    }

    async void OnOkClicked(object sender, EventArgs e)
    {

        await CloseAsync(this.pickerEvents.SelectedItem as string, CancellationToken.None);
    }

    async void btnHelpClicked(Object o, EventArgs e)
    {
        await MainPage.Instance.ShowPopupAsync(new PopupHints().SetUp("ChooseEvent", false));
    }
}