using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
using LobsterConnect.Model;

namespace LobsterConnect.V;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		MainPage.Instance = this;

		MainViewModel.Instance.Load();

		this.BindingContext = MainViewModel.Instance;

        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Data has been loaded");

        if (Microsoft.Maui.Storage.Preferences.ContainsKey("UserHandle"))
        {
            string defaultUserName = Microsoft.Maui.Storage.Preferences.Get("UserHandle", "");
            if (!string.IsNullOrEmpty(defaultUserName))
            {
				Person defaultUser= MainViewModel.Instance.GetPerson(defaultUserName);
                if(defaultUser != null)
				{
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '"+ defaultUserName+"' has been logged in automatically");
					MainViewModel.Instance.SetLoggedOnUser(defaultUser, true);
                }
            }
        }

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
		List<Session>[] sessions = MainViewModel.Instance.GetAllSessions(MainViewModel.Instance.CurrentEvent, MainViewModel.Instance.CurrentFilter);

		this.gdSlotLabels.Children.Clear();
		this.gdSlotLabels.WidthRequest = sessions.Length * 100;


        this.gdSessions.Children.Clear();
		this.gdSessions.ColumnDefinitions.Clear();

		this.gdSlotLabels.Children.Add(
			new StackLayout() {
				Orientation = StackOrientation.Horizontal,
				WidthRequest= sessions.Length * 100,
				HeightRequest=40,
				Spacing=0
			}
			.Assign(out StackLayout slSlotLabels));

		for(int s=0; s<sessions.Count(); s++)
		{
			SessionTime t = new SessionTime(s);
			slSlotLabels.Children.Add(
				new Label() {
					WidthRequest = 100,
					HeightRequest = 40,
					TextColor=Colors.LightGray,
					HorizontalTextAlignment= TextAlignment.Center,
					Text = t.ToString() });

			this.gdSessions.ColumnDefinitions.Add(new ColumnDefinition() { Width = 100 });
			if (sessions[s]!=null)
			{
				this.gdSessions.Add(
					new StackLayout() {
						HeightRequest = 75 * sessions[s].Count(),
						WidthRequest = 100,
						Orientation = StackOrientation.Vertical,
						Spacing = 0,
						VerticalOptions=LayoutOptions.Start
					}
					.Assign(out StackLayout slSessions)
					.Invoke(sl => Grid.SetColumn(sl,s)));
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
                                        Text = session.ToPlay },
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
                                            new Label(){ HeightRequest = 22, Text=": "+session.Proposer},
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
					tr.BindingContext = session;
                    tr.Tapped += (object sender, TappedEventArgs e)=>
					{
						Session s = ((StackLayout) sender ).BindingContext as Session;

						ShowSessionManagementPopup(s);
					};
					slSession.GestureRecognizers.Add(tr);
					slSession.BindingContext = session;
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
                    var popup1 = new PopupLogIn();
                    var popupResult1 = await this.ShowPopupAsync(popup1, CancellationToken.None);

                    if (popupResult1 != null && popupResult1 is Tuple<string, string,bool>)
                    {
                        Tuple<string, string,bool> userAndPassword = (Tuple<string, string,bool>)popupResult1;

                        Person user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

                        if (user != null)
                        {
                            await DisplayAlert("Login", "There is an existing user having user handle '" + userAndPassword.Item1 + "'", "Dismiss");
                            return;
                        }

                        if (string.IsNullOrEmpty(userAndPassword.Item1))
                        {
                            await DisplayAlert("Login", "You have to provide a user handle", "Dismiss");
                            return;
                        }

                        if (userAndPassword.Item1.Contains(','))
                        {
                            await DisplayAlert("Login", "A user handle must not contain any commas", "Dismiss");
                            return;
                        }

                        MainViewModel.Instance.CreatePerson(true, userAndPassword.Item1, password: Model.Utilities.PasswordHash(userAndPassword.Item2));

                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '" + userAndPassword.Item1 + "' has been created");

						user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

                        MainViewModel.Instance.SetLoggedOnUser(user, userAndPassword.Item3);

                        var popup2 = new PopupPersonDetails();
						popup2.SetPerson(user);
                        var popupResult2 = await this.ShowPopupAsync(popup2, CancellationToken.None);
                    }
                    
                }
				else if (a== logIn)
				{
                    var popup = new PopupLogIn();
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

					if(popupResult!=null && popupResult is Tuple<string,string, bool>)
					{
						Tuple<string, string, bool> userAndPassword = (Tuple<string, string, bool>)popupResult;

						Person user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

						if (user == null)
						{
							await DisplayAlert("Login", "There isn't any user having user handle '" + userAndPassword.Item1 + "'", "Dismiss");
							return;
						}
						else if (user.Password != Model.Utilities.PasswordHash(userAndPassword.Item2))
						{
                            await DisplayAlert("Login", "That is the wrong password for user '" + userAndPassword.Item1 + "'", "Dismiss");
                            return;
                        }

						MainViewModel.Instance.SetLoggedOnUser(user, userAndPassword.Item3);
                    }
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
                    var popup = new PopupPersonDetails();
                    popup.SetPerson(MainViewModel.Instance.LoggedOnUser);
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                }
				else if (a == changePassword)
				{
					if (!string.IsNullOrEmpty(MainViewModel.Instance.LoggedOnUser.Password))
					{
						string existing = await DisplayPromptAsync("Password", "Enter the current password for " + MainViewModel.Instance.LoggedOnUser.Handle, keyboard: Keyboard.Password);

						if (Model.Utilities.PasswordHash(existing) != MainViewModel.Instance.LoggedOnUser.Password)
						{
							await DisplayAlert("Password", "That is not the right password for " + MainViewModel.Instance.LoggedOnUser.Handle, "Dismiss");
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

					MainViewModel.Instance.UpdatePerson(true, MainViewModel.Instance.LoggedOnUser, password: Model.Utilities.PasswordHash(newPassword2));
                    MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Password has been updated for " + MainViewModel.Instance.LoggedOnUser.Handle);
                }
				else if (a == logOut)
				{
                    MainViewModel.Instance.SetLoggedOnUser(null);
				}
			}
		}
		catch(Exception ex)
		{
            Logger.LogMessage(Logger.Level.ERROR, "MainPage.btnUserClicked", ex);
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, ex.Message);
        }

	}
    async void btnAddSessionClicked(Object o, EventArgs e)
    {
		if (MainViewModel.Instance.LoggedOnUser == null)
		{
            await DisplayAlert("Propose a Gaming Session", "Log in first please, before proposing a gaming session", "Dismiss");
            return;
        }
		else
		{
			var popup = new PopupAddSession();
			var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
		}
    }
    async void btnFilterClicked(Object o, EventArgs e)
    {
        var popup = new PopupManageFilter();
        popup.SetFilter(new SessionFilter(MainViewModel.Instance.CurrentFilter));
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

		if(popupResult as SessionFilter!=null)
		{
			MainViewModel.Instance.CurrentFilter = popupResult as SessionFilter;
		}
    }

	async Task<bool> ShowSessionManagementPopup(Session s)
	{
		if(MainViewModel.Instance.LoggedOnUser!=null && MainViewModel.Instance.LoggedOnUser.Handle == s.Proposer)
		{
			// If the logged on user is the proposer of this session, they're allowed to make changes to it

		}
        var popup = new PopupManageSession();
		popup.SetSession(s);
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);


        return true;
	}

	public static MainPage Instance = null;

    private void svSessions_Scrolled(object sender, ScrolledEventArgs e)
    {
		//this.gdSlotLabels.Margin = new Thickness(-e.ScrollX, 0, 0, 0);
        this.alSlotLabels.WidthRequest = this.Width;
        this.alSlotLabels.SetLayoutBounds(this.gdSlotLabels, new Rect(-e.ScrollX, 0, gdSlotLabels.WidthRequest, 40));
	}
}

