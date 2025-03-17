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
    }

    public void SetSession(Session s)
    {
        this.BindingContext = s;
        if (s != null)
        {
            if (MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer && MainViewModel.Instance.CurrentEvent.IsActive)
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

        V.Utilities.StylePopupButtons(this.btnDismiss, null, this.rdefButtons);
    }

    async void btnWhatsAppClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            if (s != null && MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
            {
                string whatsAppLink = await MainPage.Instance.DisplayPromptAsync("Manage Gaming Session", "To associate a WhatsApp chat with this game, get an 'invite to group' link for the chat, and paste it here", initialValue: s.WhatsAppLink);

                if (s != null)
                {
                    MainViewModel.Instance.UpdateSession(true, s, whatsAppLink: whatsAppLink);
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "': WhatsApp link has been updated to '" + whatsAppLink + "'");
                }
            }
        }
        catch(Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting WhatsApp details: " + ex.Message);
        }
    }

    async void btnNotesClicked(object sender, EventArgs e)
    {
        try
        {
            Session s = this.BindingContext as Session;
            if (s != null && MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
            {
                string notes = await MainPage.Instance.DisplayPromptAsync("Manage Gaming Session", "Notes to display on this session", initialValue: s.Notes);

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

                if (MainViewModel.Instance.CurrentEvent.IsActive)
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
                            await MainPage.Instance.DisplayAlert("Sign-up", "The session isn't OPEN so new sign-ups are not allowed", "dismiss");
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
            if (s != null && MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
            {
                if (s.State == "ABANDONED")
                {
                    bool res1;
                    bool res2 = false;
                    res1 = await MainPage.Instance.DisplayAlert("Gaming Session (Abandoned)", "Session is currently ABANDONED; you can reactivate it by it switching to OPEN or FULL.  Do you want to switch it to OPEN (additional people can join)?", "Yes", "No");
                    if (!res1) res2 = await MainPage.Instance.DisplayAlert("Gaming Session (Abandoned)", "Do you want to switch it to FULL (game will take place, but no one else can join)?", "Yes", "No");

                    if (res1)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "OPEN");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to OPEN by '" + s.Proposer + "'");
                    }
                    else if (res2)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "FULL");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to FULL by '" + s.Proposer + "'");
                    }
                }
                else if (s.State == "OPEN")
                {
                    bool res1;
                    bool res2 = false;
                    res1 = await MainPage.Instance.DisplayAlert("Gaming Session (Open)", "Session is currently OPEN; you can declare it FULL, or you can ABANDON it.  Do you want to switch it to FULL (game will take place, but no one else can join)?", "Yes", "No");
                    if (!res1) res2 = await MainPage.Instance.DisplayAlert("Gaming Session (Open)", "Do you want to switch it to ABANDONED (the game will not be happening)?", "Yes", "No");


                    if (res1)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "FULL");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to FULL by '" + s.Proposer + "'");
                    }
                    else if (res2)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "ABANDONED");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to ABANDONED by '" + s.Proposer + "'");
                    }
                }
                else if (s.State == "FULL")
                {
                    bool res1;
                    bool res2 = false;
                    res1 = await MainPage.Instance.DisplayAlert("Gaming Session (Full)", "Session is currently FULL; you can re-OPEN it, or you can ABANDON it.  Do you want to switch it to OPEN (additional people can join)?", "Yes", "No");
                    if (!res1) res2 = await MainPage.Instance.DisplayAlert("Gaming Session (Full)", "Do you want to switch it to ABANDONED (the game will not be happening)?", "Yes", "No");

                    if (res1)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "OPEN");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to OPEN by '" + s.Proposer + "'");
                    }
                    else if (res2)
                    {
                        MainViewModel.Instance.UpdateSession(true, s, state: "ABANDONED");
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "' set to ABANDONED by '" + s.Proposer + "'");
                    }
                }
                else
                {
                    // shouldn't happen
                    Logger.LogMessage(Logger.Level.ERROR, "PopupManageSession.btnStateClicked", "State string has an illegal value: " + s.State);
                }
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
                    MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to launch WhatsApp using link '" + s.WhatsAppLink+"'");
                }
            }
            catch(Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Failed to launch WhatsApp using link '" + s.WhatsAppLink + "': "+ex.Message);
            }
        }
    }

    private async void Notes_Tapped(object sender, TappedEventArgs e)
    {
        Session s = this.BindingContext as Session;
        if(s!=null)
            await MainPage.Instance.DisplayAlert("Notes", s.Notes, "Dismiss");
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(true, CancellationToken.None);
    }
}