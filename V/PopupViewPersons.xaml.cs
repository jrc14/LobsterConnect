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

public partial class PopupViewPersons : Popup
{
    /// <summary>
    /// Popup for displaying a list of person handles, and for viewing their personal details.
    /// To set up the list of person handles, call the SetPersons method after constructing
    /// the popup.  When a person is selected from the list, their details will be shown in
    /// the controls on the popup.  This is accomplished by binding to the properties of the
    /// relevant Person object (since the binding is one way from the Person to the control, this
    /// is a safe thing to do).
    /// The popup, when dismissed, will return the person handle of the selected person (if a person
    /// was selected).
    /// </summary>
    public PopupViewPersons()
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
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupViewPersons ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);
    }

    /// <summary>
    /// Set the list of persons that the dialog will display
    /// </summary>
    /// <param name="personHandles"></param>
    public void SetPersons(List<string> personHandles)
    {
        try
        {
            if (personHandles == null || personHandles.Count == 0)
            {
                Logger.LogMessage(Logger.Level.ERROR, "PopupViewPersons.SetPersons: list mustn't be null or empty");
            }
            else
            {
                this.BindingContext = MainViewModel.Instance.GetPerson(personHandles[0]);
                if (personHandles.Count == 1)
                {
                    this.pickerPersons.IsVisible = false;
                    this.lbPickerPersons.IsVisible = false;
                }
                else
                {
                    this.pickerPersons.IsVisible = true;
                    this.lbPickerPersons.IsVisible = true;
                    this.pickerPersons.ItemsSource = personHandles;
                    this.pickerPersons.SelectedIndex = 0;
                }
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting person: "+ex.Message);
        }
    }

    /// <summary>
    /// When dismissed, look at the picker and if a person handle is selected, return it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnDismissClicked(object sender, EventArgs e)
    {
        string result = null;

        List<string> personHandles = this.pickerPersons.ItemsSource as List<string>;
        int i = this.pickerPersons.SelectedIndex;
        if (personHandles != null && i >= 0 && i < personHandles.Count)
            result = personHandles[i];
        await CloseAsync(result, CancellationToken.None);
    }

    /// <summary>
    /// When the picker selection is changed, change the binding context on self, so that the
    /// person's details are displayed in the appropriate fields.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void pickerPersons_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            List<string> personHandles = this.pickerPersons.ItemsSource as List<string>;

            if (personHandles == null) return;
            int i = this.pickerPersons.SelectedIndex;
            if (i < 0) return;
            if (i >= personHandles.Count) return;
            Person p = MainViewModel.Instance.GetPerson(personHandles[i]);
            if (p is null) return;

            this.BindingContext = p;
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error selecting person: " + ex.Message);
        }

    }

    /// <summary>
    /// Copy the selected person's email to the clipboard
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnCopyEmail_Clicked(object sender, EventArgs e)
    {
        Person p = this.BindingContext as Person;
        if (p != null && !string.IsNullOrEmpty(p.Email))
        {
            Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(p.Email);
            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Copied text: '" + p.Email + "'");
        }
    }

    /// <summary>
    /// Copy the selected person's phone number to the clipboard
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnCopyPhoneNumber_Clicked(object sender, EventArgs e)
    {
        Person p = this.BindingContext as Person;
        if (p != null && !string.IsNullOrEmpty(p.PhoneNumber))
        {
            Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(p.PhoneNumber);
            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Copied text: '" + p.PhoneNumber + "'");
        }
    }

    /// <summary>
    /// The dialog contains a warning about what content would be inappropriate.  This label allows
    /// the user to report inappropriate content by sending an email to the developer.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void lblReportContentTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Person p = this.BindingContext as Person;

            if (Email.Default.IsComposeSupported && p!=null)
            {

                EmailMessage message = new EmailMessage()
                {
                    Subject = "LobsterConnect Inappropriate Content",
                    Body = "I note that the user details for '"+p.Handle+"' contain inappropriate content.  Please take the necessary steps to address this concern.",
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>() { "moderator@turnipsoft.co.uk" }
                };

                await Email.Default.ComposeAsync(message);
                await CloseAsync(null, CancellationToken.None);
            }
            else
            {
                MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "Sorry, the app cannot create an email for you.  Please send your report to moderator@turnipsoft.co.uk");
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error while composing email: " + ex.Message);
        }

    }
}