using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

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
            if (MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
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
        Session s = this.BindingContext as Session;
        if (s != null && MainViewModel.Instance.LoggedOnUser!=null && MainViewModel.Instance.LoggedOnUser.Handle==s.Proposer)
        {
            string whatsAppLink = await MainPage.Instance.DisplayPromptAsync("Manage Gaming Session", "To associate a WhatsApp chat with this game, get an 'invite to group' link for the chat, and paste it here", initialValue: s.WhatsAppLink);

            if (s != null)
            {
                MainViewModel.Instance.UpdateSession(true, s, whatsAppLink: whatsAppLink);
                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "': WhatsApp link updated to '" + whatsAppLink + "'");
            }
        }
    }

    async void btnNotesClicked(object sender, EventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s != null && MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
        {
            string notes = await MainPage.Instance.DisplayPromptAsync("Manage Gaming Session", "Notes to display on this session", initialValue: s.Notes);

            if (s != null)
            {
                MainViewModel.Instance.UpdateSession(true, s, notes: notes);
                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Session for '" + s.ToPlay + "': Notes updated to '" + notes + "'");
            }
        }
    }

    async void btnSignUpClicked(object sender, EventArgs e)
    {
        Session s = this.BindingContext as Session;
        if (s != null && MainViewModel.Instance.LoggedOnUser != null)
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

            List<string> signUps = new List<string>();
            foreach (string h in s.SignUps.Split(','))
            {
                string handle = h.Trim();
                if (handle != MainViewModel.Instance.LoggedOnUser.Handle)
                    signUps.Add(handle);
            }

            string a;
            if(signUps.Count>0)
                a = await MainPage.Instance.DisplayActionSheet("Sign up", "Cancel", null, option, user);
            else
                a = await MainPage.Instance.DisplayActionSheet("Sign up", "Cancel", null, option);

            if (a==signUp)
            {
                s.AddSignUp(MainViewModel.Instance.LoggedOnUser.Handle);
                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "'"+ MainViewModel.Instance.LoggedOnUser.Handle+"' has signed up to play '" + s.ToPlay + "'");
            }
            else if (a== cancelSignUp)
            {
                s.RemoveSignUp(MainViewModel.Instance.LoggedOnUser.Handle);
                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "'" + MainViewModel.Instance.LoggedOnUser.Handle + "' has cancelled their sign-up for '" + s.ToPlay + "'");
            }
            else if (a==user)
            {
                PopupViewPersons personsViewer = new PopupViewPersons();
                personsViewer.SetPersons(signUps);
                var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
            }

        }
    }

    async void btnStateClicked(object sender, EventArgs e)
    {

        Session s = this.BindingContext as Session;
        if (s != null && MainViewModel.Instance.LoggedOnUser != null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
        {
            if (s.State == "ABANDONED")
            {
                bool res1;
                bool res2=false;
                res1 = await MainPage.Instance.DisplayAlert("Gaming Session (Abandoned)", "Session is currently ABANDONED; you can reactivate it by it switching to OPEN or FULL.  Do you want to switch it to OPEN (additional people can join)?", "Yes", "No");
                if(!res1) res2 = await MainPage.Instance.DisplayAlert("Gaming Session (Abandoned)", "Do you want to switch it to FULL (game will take place, but no one else can join)?", "Yes", "No");

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
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, ex.Message);
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
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, ex.Message);
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