using CommunityToolkit.Maui.Markup;
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

		this.gdSlotLabels.Children.Clear();

		this.gdSessions.Children.Clear();
		this.gdSessions.RowDefinitions.Clear();

		this.gdSlotLabels.Children.Add(
			new StackLayout() { Orientation = StackOrientation.Vertical, WidthRequest=100, HeightRequest=sessions.Length*50 }
				.Assign(out StackLayout slSlotLabels));

		for(int s=0; s<sessions.Count(); s++)
		{
			SessionTime t = new SessionTime(s);
			slSlotLabels.Children.Add(
				new Label() { WidthRequest = 100, HeightRequest = 50, Text = t.DayLabel + " " + t.TimeLabel });

			this.gdSessions.RowDefinitions.Add(new RowDefinition() { Height = 50 });
			if (sessions[s]!=null)
			{
				this.gdSessions.Add(
					new StackLayout() { HeightRequest = 50, WidthRequest = 100 * sessions[s].Count(), Orientation = StackOrientation.Horizontal, HorizontalOptions=LayoutOptions.Start }
						.Assign(out StackLayout slSessions)
						.Invoke(sl => Grid.SetRow(sl,s)));
				foreach(Session session in sessions[s])
				{
					slSessions.Children.Add(
						new Label() { HeightRequest = 50, WidthRequest = 100, Text = session.ToPlay + ":" + session.SignUps });
				}
			}
		}
    }
}

