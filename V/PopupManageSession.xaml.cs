using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

/// <summary>
/// Popup for managing a gaming session.  Before showing it, call SetSession to associate the popup with a gaming session.  Use a
/// session from the sessions collection, not a copy of one.
/// Changes to fields on the screen are applied immediately to the session (by means of calls to the appropriate MainViewModel ,
/// methods: UpdateSession, SignUp, CancelSignUp).
/// </summary>
public partial class PopupManageSession : Popup
{
    public PopupManageSession()
    {
        InitializeComponent();

        try
        {
            if (MainPage.Instance.Width > 450)
            {
                this.colDef0.Width = new GridLength(300, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
            }
            else
            {
                double ww = 450 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(300 - ww / 3, GridUnitType.Absolute);
                this.colDef1.Width = new GridLength(100, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupManageSession ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);
    }

    public void SetSession(Session s)
    {
        this.BindingContext = s;
        if (s != null)
        {
            bool userIsProposer = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer;
            bool userIsAdmin = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.IsAdmin;
            if (userIsProposer || userIsAdmin)
            {
                this.btnNotes.IsVisible = true;
                this.btnState.IsVisible = true;
                this.btnWhatsAppLink.IsVisible = true;
            }
            else
            {
                this.btnNotes.IsVisible = false;
                this.btnState.IsVisible = false;
                this.btnWhatsAppLink.IsVisible = false;

                Grid.SetColumnSpan(this.lblNotes, 2);
                Grid.SetColumnSpan(this.slState, 2);
                Grid.SetColumnSpan(this.lblWhatsAppLink, 2);
            }
        }
    }

    async void btnWhatsAppClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            bool userIsProposer = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer;
            bool userIsAdmin = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.IsAdmin;
            if (s != null && (userIsProposer || userIsAdmin))
            {
                string whatsAppLink = await MainPage.Instance.DisplayPromptAsync("Manage Gaming Session", "To associate a chat with this game, get a link for the chat, and paste it here", initialValue: s.WhatsAppLink);

                if (s != null)
                {
                    MainViewModel.Instance.UpdateSession(true, s, whatsAppLink: whatsAppLink);
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "': Chat link has been updated to '" + whatsAppLink + "'");
                }
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting chat details: " + ex.Message);
        }
    }

    async void btnNotesClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            bool userIsProposer = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer;
            bool userIsAdmin = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.IsAdmin;
            if (s != null && (userIsProposer|| userIsAdmin))
            {
                string notes =
                    await MainPage.Instance.DisplayPromptAsync(
                        "Manage Gaming Session",
                        "Enter the notes to display on this session.  Do not enter text that is offensive or defamatory, or contains information about any person.", initialValue: s.Notes);

                if (s != null)
                {
                    MainViewModel.Instance.UpdateSession(true, s, notes: notes);
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "': Notes have been updated to '" + notes + "'");
                }
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting notes: " + ex.Message);
        }
    }

    async void btnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            if (s != null && MainViewModel.Instance.LoggedOnUser != null)
            {
                List<string> signUps = new List<string>();
                if (string.IsNullOrEmpty(s.SignUps))
                {
                    // no sign-ups
                }
                else if (!s.SignUps.Contains(','))
                {
                    // one sign-up
                    string handle = s.SignUps.Trim();
                    signUps.Add(handle);
                }
                else
                {
                    foreach (string h in s.SignUps.Split(','))
                    {
                        // multiple sign-ups, separated by commas and trailing spaces
                        string handle = h.Trim();
                        signUps.Add(handle);
                    }
                }

                bool showAdminPopup = false;
                if (signUps.Count > 0)
                {
                    bool userIsProposer = MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer;
                    bool userIsAdmin = MainViewModel.Instance.LoggedOnUser.IsAdmin;
                    if (userIsProposer || userIsAdmin)
                    {
                        string prompt = "Because you are " + (userIsAdmin ? "an admin" : "the proposer of the session") + " you can remove other people's sign-ups.  Do you want to do this?";
                        bool confirmation = await MainPage.Instance.DisplayAlert("Remove player from session", prompt, "Yes", "No");
                        if (confirmation)
                        {
                            showAdminPopup = true;
                        }
                    }
                }

                if (showAdminPopup) // show the popup for an admin or the proposer to remove any sign-up
                {
                    string personHandle = null;
                    if (signUps.Count == 1)
                        personHandle = signUps[0];
                    else
                    {
                        PopupItemsViewer personsViewer = new PopupItemsViewer();
                        personsViewer.SetItems(signUps);
                        var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
                        personHandle = popupResult as string;
                    }
                    if(personHandle!=null)
                    {
                        string prompt = "Please confirm that you want to remove "+personHandle+"'s sign from the gaming session.";
                        bool confirmation = await MainPage.Instance.DisplayAlert("Remove player from session", prompt, "Remove", "Don't remove");
                        if(confirmation)
                        {
                            MainViewModel.Instance.CancelSignUp(true, personHandle, s.Id, MainViewModel.Instance.LoggedOnUser.Handle);
                            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "The sign-up for '" + personHandle + "' to play '" + s.ToPlay + "' has been cancelled by '"+ MainViewModel.Instance.LoggedOnUser.Handle);
                        }
                    }
                }
                else if (MainViewModel.Instance.CurrentEvent.IsActive) // show the popup for a user to manage their own sign-ups and view user details
                {
                    string signUp = "Sign up to play";
                    string cancelSignUp = "Cancel my sign up";
                    string option;
                    string user = "View user details";
                    if (s.IsSignedUp(MainViewModel.Instance.LoggedOnUser.Handle))
                    {
                        option = cancelSignUp;
                    }
                    else
                    {
                        option = signUp;
                    }

                    string a;
                    if (signUps.Count > 0)
                        a = await MainPage.Instance.DisplayActionSheet("Sign up", "Cancel", null, option, user);
                    else
                        a = await MainPage.Instance.DisplayActionSheet("Sign up", "Cancel", null, option);

                    if (a == signUp)
                    {
                        if (s.State != "OPEN")
                        {
                            await MainPage.Instance.DisplayAlert("Sign-up", "The session isn't OPEN so new sign-ups are not allowed", "Dismiss");
                        }
                        else
                        {
                            MainViewModel.Instance.SignUp(true, MainViewModel.Instance.LoggedOnUser.Handle, s.Id, true, MainViewModel.Instance.LoggedOnUser.Handle);
                            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "'" + MainViewModel.Instance.LoggedOnUser.Handle + "' has signed up to play '" + s.ToPlay + "'");
                        }
                    }
                    else if (a == cancelSignUp)
                    {
                        MainViewModel.Instance.CancelSignUp(true, MainViewModel.Instance.LoggedOnUser.Handle, s.Id, MainViewModel.Instance.LoggedOnUser.Handle);
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "'" + MainViewModel.Instance.LoggedOnUser.Handle + "' has cancelled their sign-up for '" + s.ToPlay + "'");
                    }
                    else if (a == user)
                    {
                        PopupViewPersons personsViewer = new PopupViewPersons();
                        personsViewer.SetPersons(signUps);
                        var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
                    }
                }
                else // if the gaming event isn't active, you can only view sign-ups, not change them
                {
                    PopupViewPersons personsViewer = new PopupViewPersons();
                    personsViewer.SetPersons(signUps);
                    var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
                }
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error updating sign-up: "+ex.Message);
        }
    }
    

    async void btnStateClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            bool userIsProposer = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer;
            bool userIsAdmin = MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.IsAdmin;
            if (s != null && (userIsProposer || userIsAdmin))
            {
                var popup = new PopupSetSessionState();
                popup.SetSession(this.BindingContext as Session);
                await MainPage.Instance.ShowPopupAsync(popup);
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting state: " + ex.Message);
        }
    }

    private async void BggLink_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s != null && !string.IsNullOrEmpty(s.BggLink))
        {
            try
            {
                if (!await Browser.Default.OpenAsync(s.BggLink))
                {
                    MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to launch BGG using link '" + s.BggLink + "'");
                }
            }
            catch (Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to launch BGG using link '" + s.BggLink + "': "+ ex.Message);
            }
        }
    }

    private async void WhatsAppLink_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s != null && !string.IsNullOrEmpty(s.WhatsAppLink))
        {
            try
            {
                if(!await Browser.Default.OpenAsync(s.WhatsAppLink))
                {
                    MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to access chat using link '" + s.WhatsAppLink+"'");
                }
            }
            catch(Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to access chat using link '" + s.WhatsAppLink + "': "+ex.Message);
            }
        }
    }

    private async void Notes_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if(s!=null)
            await MainPage.Instance.DisplayAlert("Notes", s.Notes, "Dismiss");
    }

    void btnShareClicked(object sender, EventArgs e)
    {
        Session s = this.BindingContext as Session;

        string shareUrl = "lobsterconnect:///" + s.Id;

        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Sharing link has been copied: '" + shareUrl + "'");

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the method exit in the meantime
        Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(shareUrl);
        MainPage.Instance.DisplayAlert("Sharing", "A link to this session has been copied. Please paste it into a chat session, message or email to share it.", "Dismiss");
#pragma warning restore 4014
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(true, CancellationToken.None);
    }

    private async void lblReportContentTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;

            if (Email.Default.IsComposeSupported && s != null)
            {

                EmailMessage message = new EmailMessage()
                {
                    Subject = "LobsterConnect Inappropriate Content",
                    Body = "I note that the session details for "+ s.Id + "("+s.EventName+", "+ s.ToPlay+", #"+s.StartAt.Ordinal.ToString()+ ") contain inappropriate content.  Please take the necessary steps to address this concern.",
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>() { "moderator@turnipsoft.co.uk" }
                };

                await Email.Default.ComposeAsync(message);
                await CloseAsync(true, CancellationToken.None);
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