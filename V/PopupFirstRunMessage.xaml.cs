using CommunityToolkit.Maui.Views;
namespace LobsterConnect.V;

public partial class PopupFirstRunMessage : Popup
{
    private static string _FirstRunText =
         " - The app needs an internet connection so it can send and receive information about what games are being played, and who is playing them. When you first start the app, please make sure you have a good internet connection, so it can download everything. After that, the app can be used off-line - it will keep track of any changes you make, and sync them just as soon as it has a connection.\n"
        + " - You can view the schedule of planned games without logging in to the app, but if you want to propose gaming sessions, or join existing sessions, you'll need to sign in. Tap the 'person' icon at the top right of the page to set up a new user name ('user handle') and password.\n"
        + " - LobsterConnect can manage schedules for many different events; to pick an event, tap the event name above the schedule table (or tap the 'hamburger' symbol at the top left to open the main app menu, and pick the 'Change Event' option).\n"
        + " - To see what gaming sessions are planned for the event, scroll around the schedule table. Note that sessions can be OPEN (still looking for extra people to join), FULL (the game's going to be played, but the organiser isn't looking for any more players to join) or ABANDONED (the organiser has decided that the game isn't going to be happening after all). When you tap on an item in the schedule table, you'll be able to see the game details, including a link for WhatsApp chat about the gaming session (if the organiser has set one up). At the bottom you'll see a button with a 'people' icon on it, and maybe some user names next to it; tap that button to sign up (or if you're already signed up, tap on it to remove your sign-up).\n"
        + " - If you want to propose a new gaming session (to play a particular game at a particular time), tap in a blank space in the schedule table (or choose 'Add Session' from the main menu). Then fill in the details of the session. The app comes with a list of 500 popular games, but if your chosen game isn't on the list, tap on the [add a game not on the list below] item at the top of the list, and enter the game details (take care to enter the correct Board Game Geek URL, because it's hard to change this after the game is added). Once you've created the session, you'll be prompted to add notes if you like, and then you'll be given the opportunity to provide a WhatApp chat for discussing sign-ups for this session. To get that link, open WhatsApp, set up a new group (or use an existing one that you own) and copy the 'invitation to group via link' link from it, and paste it into the popup in LobsterConnect.\n"
        + " - There's a 'filter' button above the schedule table, which you can use to show only some gaming sessions (only ones involving a particular game, or organised by a particular person, or signed up to by a particular person, etc.)\n"
        + " - Keep an eye on the scrolling list of messages at the bottom of the screen. If people join or leave a session that you organised, you'll get notifications in this area. Or if a session you joined is declared FULL or ABANDONED, you'll also get a notification here.";

    /// <summary>
    /// Popup for displaying the legal terms associated with the app.
    /// </summary>
    public PopupFirstRunMessage()
    {
        InitializeComponent();

        this.lbTextViewer.Text = _FirstRunText;

        try
        {
            if (MainPage.Instance.Width > 500)
            {
                this.colDef0.Width = new GridLength(450, GridUnitType.Absolute);
            }
            else
            {
                double ww = 500 - MainPage.Instance.Width;

                this.colDef0.Width = new GridLength(450 - ww, GridUnitType.Absolute);
            }

            if (MainPage.Instance.Height > 600)
            {
                this.rdefTextViewer.Height = new GridLength(400, GridUnitType.Absolute);
            }
            else
            {
                this.rdefTextViewer.Height = new GridLength(MainPage.Instance.Height - 250, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupLegalTerms ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }


        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {
        await CloseAsync(true, CancellationToken.None);
    }
}