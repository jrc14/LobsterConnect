using CommunityToolkit.Maui.Views;
namespace LobsterConnect.V;

public partial class PopupLegalTerms : Popup
{
    private static string _LegalText =
                "We don’t claim that our app, LobsterConnect, is good for anything – if you think it is, that’s great, but it’s up to you to decide.  If LobsterConnect doesn’t work for you, that’s too bad, but it really is not our problem.  "
                + "If you lose millions because LobsterConnect messes up, it’s you that loses the millions, not us.  If you don’t like this disclaimer, just don’t use LobsterConnect.  We reserve the right to do the absolute minimum provided by law, up to and including nothing.  "
                + "Our liability for loss or damage of any nature will, in any case, be limited to the amount you paid for the app.\n\n"
                + "The app allows you to enter information about yourself and comments about board games.  It is a condition of your use of the app that you do not enter personal information about anyone else, or enter comments that are defamatory, obscene, offensive or otherwise inappropriate.  "
                + "If you see any content that violates this condition, you may report it to us at moderator@turnipsoft.co.uk.\n"
                + "Because the app allows you to enter personal data and to create and view user-created content, it is not suitable for persons under 18 years of age; such persons must not use the app.\n"
                + "If you have any queries regarding any of our terms, please contact us.";
    public PopupLegalTerms()
    {
        InitializeComponent();

        this.lbTextViewer.Text = _LegalText;

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