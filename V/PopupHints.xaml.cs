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
namespace LobsterConnect.V;
public partial class PopupHints : Popup
{
    /// <summary>
    /// Popup for displaying the first run message for the app.
    /// </summary>
    public PopupHints()
    {
        InitializeComponent();

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

            if (MainPage.Instance.Height > 650)
            {
                this.rdefTextViewer.Height = new GridLength(400, GridUnitType.Absolute);
            }
            else
            {
                this.rdefTextViewer.Height = new GridLength(MainPage.Instance.Height - 300, GridUnitType.Absolute);
            }
        }
        catch (Exception ex)
        {
            Model.Logger.LogMessage(Model.Logger.Level.ERROR, "PopupHints ctor", ex, "While setting sizes for width " + MainPage.Instance.Width.ToString());
        }

        V.Utilities.StylePopupButtons(null, this.btnDismiss, this.rdefButtons);
    }

    public void SetCaption(string c)
    {
        this.lbCaption.Text = c;
    }

    public void SetHintsText(string h)
    {
        this.lbTextViewer.Text = h;
    }

    public void SetDontShowAgain(bool dontShowAgain, string key)
    {
        this.hslDontShowAgain.IsVisible = true;
        this.cbxDontShowAgain.IsChecked = dontShowAgain;
        this.settingsKey = key;

        Preferences.Set("DontShowAgain_" + key, dontShowAgain);
    }
    private string settingsKey;

    /// <summary>
    /// Returns true if the user has already said "don't show again" for the hints popup
    /// identified by key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool DontShowAgain(string key)
    {
        bool dontShowAgain = Preferences.Get("DontShowAgain_"+key, false);
        return dontShowAgain;
    }

    /// <summary>
    /// Set up a hints popup with the right caption and hints text for the specified key.  If the bool
    /// parameter is set, the popup will include a checkbox for specifying whether these hints should
    /// be shown again.
    /// </summary>
    /// <param name="key">the key to be used for looking up the caption and the hint text</param>
    /// <param name="showComboBox">true if you want the "don't show again" control to be visible</param>
    public PopupHints SetUp(string key, bool showComboBox)
    {
        string caption = null;
        string hintsText = null;

        switch(key)
        {
            case "AddSession":
                caption = "Add Session";
                hintsText =
                      " - You're proposing a new session, to play a certain game at a particular time. In the popup, please fill in the details of the session, including the game that will be played.\n"
                    + " - The app comes with a list of 500 popular games, but if your chosen game isn't on the list,there's an option to add a game (take care to enter the correct Board Game Geek URL, because it's hard to change this after the game is added).\n"
                    + " - There are numeric controls to set the minimum and maximum number of players that you want to include in this session.  Note that these are informational only - the app won't enforce these limits, though it will show a warning if they are violated.\n"
                    + " - Once you've created the session, you'll be prompted to add notes if you like.  Use this to specify any extra information, for instance indicating whether inexperienced players are welcome and a teach will be provided initially.\n"
                    + " - You'll then be given the opportunity to link to a chat for discussing sign-ups for this session. For a BGG Geek List link, just paste it from your web browser; for Wix use the 'share' button to get a link; for a WhatsApp link, set up a new group (or use an existing one that you own) and copy the 'invitation to group via link' link from it, and paste it into the popup in LobsterConnect.\n"
                    + " - Finally the app will ask whether you want to sign up yourself; usually you'll want to answer YES, but it is quite OK to propose a session that you're not planning to participate in.\n"
                    ;
                break;
            case "ManageSession":
                caption = "Manage Session";
                hintsText =
                      " - You're viewing details of a proposed gaming session to play the indicated game at the indicated time.  The top part of the popup will show the game name, start time, proposer name and BGG link.\n"
                    + " - Below that, are the chat link, notes and session state.  If you're the proposer of the session, you'll see buttons on the right hand side that you can use to change these elements (admin users can also make changes event if they aren't the proposer).\n"
                    + " - The chat link (if the proposer has set one up) will take you to a BGG Geek List entry, Wix page or WhatsApp chat for discussing signups to this gaming session.\n"
                    + " - The notes text is for any additional information that the proposer chooses to include - for instance indicating whether inexperienced players are welcome and a teach will be provided initially.\n"
                    + " - The session's state is one of OPEN, FULL or ABANDONED.  OPEN means that further players are welcome to join, FULL means that the gaming session is going ahead with the players who have already signed up, and ABANDONED means the session isn't happening after all.\n"
                    + " - Note that the state is not set automatically.  In principle, in some cases it could be updated automatically by referencing the minimum/maximum/current player counts for the session, but an automated rule would sometimes do something stupid.  Therefore state can be changed only by the person proposing the session.\n"
                    + " - If you're the organiser of a session, you should set it to FULL once you don't want any more players, and you should then keep an eye on it in case someone drops out (because in that case you might want to switch it back to OPEN).\n"
                    + " - Below this, is the button for signing up to the session.  The button will show a list of the people already signed up; tap on the button to sign up (or if you're already signed up, to cancel your sign-up).  Next to this button there is 'share link' button; if you want to share details of this session with another LobsterConnect user, tap this button to obtain a suitable 'deep link'.\n"
                    + " - Additionally, if you're the organiser, you'll have the option of removing other people's sign-ups.  There could be good reasons for wanting to remove someone else's sign-up, but obviously you should, as a matter of courtesy, contact the person whose sign-up you're cancelling.\n"
                    ;
                break;
            case "ChooseEvent":
                caption = "Choose Event";
                hintsText =
                      " - You're choosing among the available gaming events.\n"
                    + " - The main screen shows the name of the currently selected event, and the table below it shows all the gaming sessions at that event.  It only shows sessions for that one event, so if you want to look at sessions for a different event, you need to switch events.\n"
                    + " - This popup shows the list of events that you can switch between.  When you switch to a different event, you will see the gaming sessions planned for that event.\n"
                    + " - There will always be a 'TEST Convention' event, which you can select if you just want to mess around with the app, and see how to propose sessions, sign up to play games, or manage a wish-list of games\n"
                    + " - In addition to this, the list includes any LoB gaming events (such as LoBsterCon conventions) that are happening now, or being planned.\n"
                    + " - To select a gaming event, choose it from the list, and then tap OK.  To make no change to your choice of current event, tap Cancel.\n"
                    ;
                break;
            case "ManageFilter":
                caption = "Manage Filter";
                hintsText =
                      " - You're setting up a filter so that only some gaming sessions will be shown in the main screen.\n"
                    + " - You can filter according to the person proposing a session, or select '[Any Person]' to not filter by person.  If you're logged on at the moment, the second item in the list will be '[Me ...]', to make it easier to select yourself without having to hunt around for your name.  Next to the dropdown is a text field to search for a person name and skip straight to that person in the list.\n"
                    + " - You can filter by game name, entering some part of the game's name to see only sessions involving games having names matching the text you entered.\n"
                    + " - You can filter so that you see only sessions where a particular person has signed up.\n"
                    + " - You can choose a state (OPEN, FULL or ABANDONED), if you only want to see sessions in a particular state.\n"
                    + " - Using the 'Only games that I'd 'like to play' checkbox, you can filter to see only gaming sessions involving games on your personal 'would like to play' wishlist.\n"
                    + " - To apply the chosen filter settings, tap OK.  To make no change to the filter, tap Cancel.\n"
                    ;
                break;
            case "ManageWishList":
                caption = "Manage Wish-List";
                hintsText =
                      " - You're managing your wish-list, which is to say the list of games that you would like to play at the currently selected gaming event.\n."
                    + " - Putting a game on this list is not a definite arrangement to play any particular game at any particular time; rather it's an indication of interest.\n"
                    + " - By adding a game to your 'would like to play' wish-list, you are letting other players know of your interest in the game, so that if they too are interested, one of you might make a definite proposal to play the game.\n"
                    + " - If there are any games on your wish-list, you'll see them listed in this window; if no games are listed it means you haven't got any games on your wish-list to play at this event."
                    + " - Each game in the list will, next to the game name, show any comments you've added, and then a button with one or more *s on it.  The number of *s indicates how many people have indicated that that would like to play this game.\n"
                    + " - Tap on the button with the *s to take various actions, such as removing the game from your wish-list, update your comment, see details of the people who also want to play this game, or to propose a session to play the game.\n"
                    + " - To put a game onto your wish-list, tap the 'Add a Game I'd Like to Play' button; a popup will appear, on which you can choose a game to add.\n"


                    ;
                break;
            default:
                caption = key;
                hintsText = "Coding error: no help text is available for " + key;
                break;
        }

        this.SetCaption(caption);
        this.SetHintsText(hintsText);

        this.hslDontShowAgain.IsVisible = showComboBox;
        if(showComboBox)
        {
            this.SetDontShowAgain(false, key);
        }

        return this;
    }

    async void OnDismissClicked(object sender, EventArgs e)
    {
        if(string.IsNullOrEmpty(this.settingsKey))
        {
            // don't save any preference because the caller never provided a key
        }
        else if(this.hslDontShowAgain.IsVisible==false)
        {
            // don't save any preference because the don't-show-again control was never made visible
        }
        else
        {
            Preferences.Set("DontShowAgain_" + this.settingsKey, this.cbxDontShowAgain.IsChecked);
        }
        await CloseAsync(true, CancellationToken.None);
    }
}