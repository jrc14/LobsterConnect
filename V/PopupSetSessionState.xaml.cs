using CommunityToolkit.Maui.Views;
using LobsterConnect.Model;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupSetSessionState : Popup
{
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



    /*
     

    
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

     */
}