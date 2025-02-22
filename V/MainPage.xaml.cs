using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using LobsterConnect.Model;
namespace LobsterConnect.V;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		MainViewModel.Instance.Load();

		this.BindingContext = MainViewModel.Instance;

        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Data has been loaded");

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
			new StackLayout() {
				Orientation = StackOrientation.Vertical,
				WidthRequest=100,
				HeightRequest=sessions.Length*75,
				Spacing=0
			}
			.Assign(out StackLayout slSlotLabels));

		for(int s=0; s<sessions.Count(); s++)
		{
			SessionTime t = new SessionTime(s);
			slSlotLabels.Children.Add(
				new Label() {
					WidthRequest = 100,
					HeightRequest = 75,
					Text = t.DayLabel + " " + t.TimeLabel });

			this.gdSessions.RowDefinitions.Add(new RowDefinition() { Height = 75 });
			if (sessions[s]!=null)
			{
				this.gdSessions.Add(
					new StackLayout() {
						HeightRequest = 75,
						WidthRequest = 100 * sessions[s].Count(),
						Orientation = StackOrientation.Horizontal,
						Spacing = 0,
						HorizontalOptions=LayoutOptions.Start
					}
					.Assign(out StackLayout slSessions)
					.Invoke(sl => Grid.SetRow(sl,s)));
				foreach (Session session in sessions[s])
				{
					slSessions.Children.Add(
						new Border()
						{
							Stroke = Colors.White,
							HeightRequest = 73,
							WidthRequest = 98,
							Margin=1,
							Padding=1,
							Content = new StackLayout()
							{
								Background = Colors.Gray,
								HeightRequest = 69,
								WidthRequest = 94,
								Orientation = StackOrientation.Vertical,
								Spacing=0,
								Margin=0,
								Padding=1,
								Children =
								{
									new Label() {
										HeightRequest = 22,
										WidthRequest = 92,
										LineBreakMode = LineBreakMode.NoWrap,
                                        Text = session.ToPlay + ": " + session.Proposer },
                                    new Label() {
                                        HeightRequest = 22,
                                        WidthRequest = 92,
                                        LineBreakMode = LineBreakMode.NoWrap,
                                        }.Assign(out Label lbState),
                                    new StackLayout {
                                        HeightRequest = 22,
                                        WidthRequest = 92,
                                        Orientation = StackOrientation.Horizontal,
                                        Spacing=0,
										Margin=0,
										Padding=0,
                                        Children =
										{
											new Label() {
												HeightRequest = 22,
											}
											.Assign(out Label lbNumSignUps),
											new Label(){ HeightRequest = 22, Text="/"},
                                            new Label() {
                                                HeightRequest = 22,
                                            }
                                            .Assign(out Label lbSitsMinimum),
                                            new Label(){ HeightRequest = 22, Text="-"},
                                            new Label() {
                                                HeightRequest = 22,
                                            }
                                            .Assign(out Label lbSitsMaximum),
                                        }
                                    }
                                }
							}.Assign(out StackLayout slSession)
						});

                    lbState.BindingContext = session;
                    lbState.Bind(Label.TextProperty, "State");

                    lbNumSignUps.BindingContext = session;
                    lbNumSignUps.Bind(Label.TextProperty, "NumSignUps");

                    lbSitsMinimum.BindingContext = session;
                    lbSitsMinimum.Bind(Label.TextProperty, "SitsMinimum");

                    lbSitsMaximum.BindingContext = session;
                    lbSitsMaximum.Bind(Label.TextProperty, "SitsMaximum");

					TapGestureRecognizer tr = new TapGestureRecognizer();
                    tr.Tapped += (object sender, TappedEventArgs e)=>
					{
					};
					slSession.GestureRecognizers.Add(tr);

                    /*
                    DataTemplate dt = new DataTemplate(() =>
                    {
                        var lbl = new Label
                        {
                            TextColor = FgColour
                        };
                        lbl.SetBinding(Label.TextProperty, "Item1");

                        return new ViewCell { View = lbl };
                    });
					*/
                }
			}
		}
    }

    async void btnUserClicked(Object o, EventArgs e)
	{
		try
		{
			if (MainViewModel.Instance.LoggedOnUser == null)
			{



                const string createNewUser = "Set up a new user";
                const string logIn = "Log in";

                string a = await DisplayActionSheet("User actions", "Cancel", null, createNewUser, logIn);

				if(a == createNewUser)
				{
                    var popup = new PopupNewUserDetails();
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                }
				else if (a== logIn)
				{
                    var popup = new PopupLogIn();
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                }
            }
			else
			{
				const string editUser = "Edit user details";
				const string changePassword = "Change password";
				const string logOut = "Log out";

				string a = await DisplayActionSheet("User actions", "Cancel", null, editUser, changePassword, logOut);

				if (a == editUser)
				{

				}
				else if (a == changePassword)
				{
					if (!string.IsNullOrEmpty(MainViewModel.Instance.LoggedOnUser.Password))
					{
						string existing = await DisplayPromptAsync("Password", "Enter the current password for " + MainViewModel.Instance.LoggedOnUser.Handle, keyboard: Keyboard.Password);

						if (existing != MainViewModel.Instance.LoggedOnUser.Password)
						{
							await DisplayAlert("Password", "That is not the right password for " + MainViewModel.Instance.LoggedOnUser.Password, "Dismiss");
							return;
						}
					}
					string newPassword1 = await DisplayPromptAsync("Password", "Enter the new password:", keyboard: Keyboard.Password);
					if (string.IsNullOrEmpty(newPassword1))
					{
						return;
					}
					string newPassword2 = await DisplayPromptAsync("Password", "Enter the new password again:", keyboard: Keyboard.Password);
					if (newPassword1 != newPassword2)
					{
						await DisplayAlert("Password", "Passwords did not match.  The password has not been changed", "Dismiss");
						return;
					}
					MainViewModel.Instance.UpdatePerson(true, MainViewModel.Instance.LoggedOnUser, password: newPassword2);
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Password has been updated for " + MainViewModel.Instance.LoggedOnUser.Handle);
                }
				else if (a == logOut)
				{
					MainViewModel.Instance.LoggedOnUser = null;
					MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User has been logged out");
				}
			}
		}
		catch(Exception ex)
		{
            Logger.LogMessage(Logger.Level.ERROR, "MainPage.btnUserClicked", ex);
        }

	}
    void btnAddSessionClicked(Object o, EventArgs e)
    {

    }
    void btnFilterClicked(Object o, EventArgs e)
    {

    }
}

