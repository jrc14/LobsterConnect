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
                    + " - Finally the app will ask whether you want to sign up yourself; usually you'll want to answer YES, but it is quite OK to propose a session that you're not planning to participate in.\n";

                break;
            default:break;
        }

        this.SetCaption(caption);
        this.SetHintsText(hintsText);
        this.SetDontShowAgain(false, key);

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
            // don't save any preference because the show-again control was never made visible
        }
        else
        {
            Preferences.Set("DontShowAgain_" + this.settingsKey, this.cbxDontShowAgain.IsChecked);
        }
        await CloseAsync(true, CancellationToken.None);
    }
}