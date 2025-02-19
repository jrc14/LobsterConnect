using LobsterConnect.VM;

namespace LobsterConnect;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		MainViewModel.Instance.Load();


		// As long as this page is loaded, we need to respond to a 'sessions must be refreshed' event raised
		// by the view model, by refreshing the main UI grid that shows all the sessions

		this.Loaded += (o, e) =>
		{
			MainViewModel.Instance.SessionsMustBeRefreshed += RefreshSessionsGrid;
		};


        this.Unloaded += (o, e) =>
        {
            MainViewModel.Instance.SessionsMustBeRefreshed -= RefreshSessionsGrid;
        };

        InitializeComponent();

		RefreshSessionsGrid(null, null);
		
	}

	public void RefreshSessionsGrid(object o, EventArgs a)
	{
		List<Session>[] sessions = MainViewModel.Instance.GetAllSessions(MainViewModel.Instance.CurrentEvent);

    }
}

