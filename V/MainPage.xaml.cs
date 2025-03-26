using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

using LobsterConnect.VM;
using LobsterConnect.Model;

namespace LobsterConnect.V;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        MainPage.Instance = this;

        // After 30 days, we want the app to insist on loading its content from the cloud sync service,
        // and writing a new local journal file, to make sure that any changes to the cloud data (such as
        // purges of user data) will make it into the local journal eventually.
        double days = DateTime.UtcNow.Subtract(new DateTime(2025, 1, 1)).TotalDays;
        double lastRefreshed = Preferences.Get("LastRefreshedDate", 0.0);
        bool fullSync = false;
        if(days-lastRefreshed>30)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                fullSync = true;
                Preferences.Set("LastRefreshedDate", days);
            }
        }

        // Load the view model - which may involve replaying the journal from the local file (if it exists)
        // or may, if fullSync is set, mean waiting for the journal sync worker to fetch all the journal
        // records from the cloud sync service.
        MainViewModel.Instance.Load(fullSync);

        this.BindingContext = MainViewModel.Instance;

        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Local data has been loaded");

		Journal.EnsureJournalWorkerRunning();
        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Cloud sync service has been started");

        if (Preferences.ContainsKey("UserHandle"))
        {
            string defaultUserName = Preferences.Get("UserHandle", "");
            if (!string.IsNullOrEmpty(defaultUserName))
            {
				Person defaultUser= MainViewModel.Instance.GetPerson(defaultUserName);
                if(defaultUser != null)
				{
                    //MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '"+ defaultUserName+"' has been logged in automatically");
					MainViewModel.Instance.SetLoggedOnUser(defaultUser, true);
                }
            }
        }

        // If age verification is needed, we defer the loading of the gaming event until it's done (because
        // in some circumstances the gaming event initialisation can pop up an alert, and we don't want to be
        // showing two alerts at the same time.
        if (!Preferences.ContainsKey("AgeConfirmed"))
        {
            //this.gdMainPage.IsVisible = false;
            this.ParentChanged += AgeVerification;
        }
        else
        {
            // Make sure we have fetched the gaming event list from the cloud, and set the current event to something reasonable
            InitialiseGamingEvent();
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

    public static MainPage Instance = null;

	protected override void OnAppearing()
	{
		base.OnAppearing();

#if __ANDROID__
        try
        {
            Behavior existing = this.Behaviors.FirstOrDefault(b => b is StatusBarBehavior);

            if (existing != null)
                this.Behaviors.Remove(existing);

            this.Behaviors.Add(new StatusBarBehavior()
            {
                StatusBarColor = Colors.Red, // for some reason it does not work
                StatusBarStyle = StatusBarStyle.DarkContent
            });

        }
        catch (Exception ex)
        {
            Logger.LogMessage(Logger.Level.ERROR, "MainPage.OnAppearing", ex, "while setting status bar colour");
        }
#endif
    }

    public void AgeVerification(object o, EventArgs e)
    {
        if (this.Parent != null)
        {
            this.ParentChanged -= AgeVerification;

            DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await DispatcherHelper.SleepAsync(2000);

                bool confirmation = await DisplayAlert("Age Verification",
                    "Because the app allows you to enter personal data and to create and view user-created content, it is not suitable for persons under 18 years of age; such persons must not use the app. Are you under 18 years of age?",
                    "No", "Yes");

                if (!confirmation)
                {
                    this.gdMainPage.IsVisible = false;
                }
                else
                {
                    Preferences.Set("AgeConfirmed", true);

                    // We didn't do this at the time, because we need to ensure that only one alert is displayed at a time.
                    InitialiseGamingEvent();
                }
            });

            
        }
    }

    public void RefreshSessionsGrid(object o, SessionsRefreshEventArgs a)
	{
        List<Session>[] sessions;
        if (MainViewModel.Instance.CurrentEvent == null)
        {
            sessions = new List<Session>[0];
        }
        else
        {
            sessions = MainViewModel.Instance.GetAllSessions(MainViewModel.Instance.CurrentEvent.Name, MainViewModel.Instance.CurrentFilter);
        }

        // If the grid isn't changing its width, then we won't want to scroll to a new position.
        bool resizeGrid = this.gdSlotLabels.WidthRequest!= sessions.Length * 100;

        this.gdSlotLabels.Children.Clear();
		this.gdSlotLabels.WidthRequest = sessions.Length * 100;

        if (resizeGrid)
        {
            this.alSlotLabels.SetLayoutBounds(this.gdSlotLabels, new Rect(0, 0, gdSlotLabels.WidthRequest, 30));
            this.svSessions.ScrollToAsync(0, 0, false);
        }

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
								Background = Colors.LightGray,
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
                    lbState.BindingContext = session;
                    lbState.Bind(Label.TextColorProperty, "State", converter: new StateToColorConverter());

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

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the tapped handler exit in the meantime
                        ShowSessionManagementPopup(s);
#pragma warning restore 4014
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
                    bool confirmation = await MainPage.Instance.DisplayAlert("Create new user",
                         "Any information you enter here will be visible to other app users (and, potentially, anyone at all).\n"
                        +"Do not enter sensitive information (such as credit card numbers), do not enter personal information about anyone other than yourself, and do not enter anything defamatory, obscene, offensive or otherwise inappropriate.\n"
                        +"To see details of how we will use your personal information, tap the 'Review Policy' button below.", "Continue", "Review Policy");
                    if (!confirmation)
                    {
                        var popup = new PopupDataHandling();
                        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                    }
                    else
                    {
                        var popup1 = new PopupLogIn();
                        var popupResult1 = await this.ShowPopupAsync(popup1, CancellationToken.None);

                        if (popupResult1 != null && popupResult1 is Tuple<string, string, bool>)
                        {
                            Tuple<string, string, bool> userAndPassword = (Tuple<string, string, bool>)popupResult1;

                            Person user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

                            if (user != null)
                            {
                                await DisplayAlert("New User", "There is an existing user having user handle '" + userAndPassword.Item1 + "'", "Dismiss");
                                return;
                            }

                            if (string.IsNullOrEmpty(userAndPassword.Item1))
                            {
                                await DisplayAlert("New User", "You have to provide a user handle", "Dismiss");
                                return;
                            }

                            if (userAndPassword.Item1.Contains(','))
                            {
                                await DisplayAlert("New User", "A user handle must not contain any commas", "Dismiss");
                                return;
                            }

                            string password2 = await DisplayPromptAsync("New User", "Please enter the password again", keyboard: Keyboard.Password);
                            if(password2!= userAndPassword.Item2)
                            {
                                await DisplayAlert("New User", "The passwords did not match", "Dismiss");
                                return;
                            }

                            try
                            {
                                MainViewModel.Instance.CreatePerson(true, userAndPassword.Item1, password: Model.Utilities.PasswordHash(userAndPassword.Item2));

                                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '" + userAndPassword.Item1 + "' has been created");

                                user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

                                MainViewModel.Instance.SetLoggedOnUser(user, userAndPassword.Item3);

                                var popup2 = new PopupPersonDetails();
                                popup2.SetPerson(user);
                                var popupResult2 = await this.ShowPopupAsync(popup2, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting up user: " + ex.Message);
                            }
                        }
                    }
                }
				else if (a== logIn)
				{
					try
					{
						var popup = new PopupLogIn();
						var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

						if (popupResult != null && popupResult is Tuple<string, string, bool>)
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
					catch (Exception ex)
					{
                        MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error logging in: " + ex.Message);
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
					try
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
					catch (Exception ex)
					{
                        MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error setting password: " + ex.Message);
                    }
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
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error updating person: "+ex.Message);
        }
	}
    async void btnAddSessionClicked(Object o, EventArgs e)
    {
		try
		{
			if (MainViewModel.Instance.LoggedOnUser == null)
			{
				await DisplayAlert("Propose a Gaming Session", "Log in first please, before proposing a gaming session", "Dismiss");
				return;
			}
			else if (!MainViewModel.Instance.CurrentEvent.IsActive)
			{
				await DisplayAlert("Propose a Gaming Session", "The current gaming event '" + MainViewModel.Instance.CurrentEvent.Name + "' is not active so new sessions can't be proposed", "Dismiss");
				return;
			}
			else
			{
				var popup = new PopupAddSession();
				var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
			}
		}
		catch(Exception ex)
		{
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error adding session: " + ex.Message);
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
        var popup = new PopupManageSession();
		popup.SetSession(s);
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

        return true;
	}

    private void svSessions_Scrolled(object sender, ScrolledEventArgs e)
    {
        //this.alSlotLabels.WidthRequest = this.Width;
        this.alSlotLabels.SetLayoutBounds(this.gdSlotLabels, new Rect(-e.ScrollX, 0, gdSlotLabels.WidthRequest, 30));
	}

    private async void lblEventTapped(object sender, TappedEventArgs e)
    {
        var popup = new PopupChooseEvent();

		popup.SetEventList(MainViewModel.Instance.GetAvailableEvents(), MainViewModel.Instance.CurrentEvent.Name);

        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

		string eventName = popupResult as string;

		if(!string.IsNullOrEmpty(eventName) && eventName!=MainViewModel.Instance.CurrentEvent.Name)
		{
			MainViewModel.Instance.SetCurrentEvent(eventName);
            Microsoft.Maui.Storage.Preferences.Set("GamingEvent", eventName);
        }
    }

    private async void gridSessionTimeSlotTapped(object sender, TappedEventArgs e)
    {
        // Position relative to the container view
        Point? pt = e.GetPosition((View)sender);

        int sessionTimeSlot = (int)(pt.Value.X / 100);

        if (sessionTimeSlot >= SessionTime.NumberOfTimeSlots)
            sessionTimeSlot = SessionTime.NumberOfTimeSlots - 1;

        if (sessionTimeSlot < 0)
            sessionTimeSlot = 0;

        var popup = new PopupAddSession();
        popup.SetTimeSlot(sessionTimeSlot);
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
    }

    /// <summary>
    /// Initialise the current gaming event to a valid value, using the saved preference if that exists
    /// and is valid.
    /// </summary>
    public void InitialiseGamingEvent()
	{
        List<string> availableEvents = MainViewModel.Instance.GetAvailableEvents();
        if (availableEvents.Count == 0)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                DisplayAlert("No Network", "The app needs to access the internet, to fetch the list of available gaming events.  Please close the app, and return to it once you have an internet connection.", "Dismiss");
            }
            else
            {
                DisplayAlert("Sync", "The app is still fetching the list of available gaming events.  Please wait, and once an event name appears on the schedule screen, tap on it to make a selection.", "Dismiss");
            }
        }
        else
        {
            string defaultGamingEventName = null;
            if (Microsoft.Maui.Storage.Preferences.ContainsKey("GamingEvent"))
            {
                defaultGamingEventName = Microsoft.Maui.Storage.Preferences.Get("GamingEvent", "");
            }

            // if there is no saved event name, or if the saved event name doesn't appear on the list
            if (string.IsNullOrEmpty(defaultGamingEventName) || !MainViewModel.Instance.CheckEventName(defaultGamingEventName))
            {
                // then set the gaming event to a valid value (the first active event in the list, or if no
                // events are active, the first event on the list
                for (int e = 0; e < availableEvents.Count; e++)
                {
                    GamingEvent ee = MainViewModel.Instance.GetGamingEvent(availableEvents[e]);
                    if (ee != null && ee.IsActive)
                    {
                        defaultGamingEventName = ee.Name;
                        break;
                    }
                }
                if (defaultGamingEventName == null)
                    defaultGamingEventName = availableEvents[0];

                Microsoft.Maui.Storage.Preferences.Set("GamingEvent", defaultGamingEventName);
            }

            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Gaming event '" + defaultGamingEventName + "' has been selected");
            MainViewModel.Instance.SetCurrentEvent(defaultGamingEventName);
        }
    }

	public async void FlyoutMenuAction(string action)
	{
        if (!Preferences.ContainsKey("AgeConfirmed"))
            return;

        if (action=="event")
		{
			lblEventTapped(this, new TappedEventArgs(null));
        }
		else if (action=="people")
		{
			List<string> allPersons = MainViewModel.Instance.GetAvailablePersons();
            PopupViewPersons personsViewer = new PopupViewPersons();
            personsViewer.SetPersons(allPersons);
            var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
        }
		else if (action == "addsession")
		{
			btnAddSessionClicked(this, new EventArgs());
        }
		else if (action == "filter")
		{
			btnFilterClicked(this, new EventArgs());
		}
		else if (action == "support")
		{
			string action2 = null;
			
			if(MainViewModel.Instance.LoggedOnUser!=null && MainViewModel.Instance.LoggedOnUser.IsAdmin)
				action2 = await DisplayActionSheet("Support", "Dismiss", null, "Admin Action", "Email for support", "Reset App");
			else
                action2 = await DisplayActionSheet("Support", "Dismiss", null, "Email for support", "Reset App");

            if (action2 == "Admin Action")
			{
				await ShowAdminActions();
            }
			else if(action2 == "Email for support")
			{
				try
				{
					if (Email.Default.IsComposeSupported)
					{
						string fromPath = Logger.LogFilePath;
						string toPath = Path.Combine(FileSystem.CacheDirectory, "logfile" + Guid.NewGuid().ToString("N") + ".txt");
						lock (Logger.LogFileLock)
						{
							Model.Utilities.FileCopy(fromPath, toPath);
						}
						EmailAttachment attachment = new EmailAttachment(Logger.LogFilePath);
						EmailMessage message = new EmailMessage()
						{
							Subject = "LobsterConnect Support",
							Body = "\n\n I'm having trouble with LobsterConnect, please can you help out.  I attach the log file\n",
							BodyFormat = EmailBodyFormat.PlainText,
							To = new List<string>() { "lobsterconnect@turnipsoft.co.uk" },
							Attachments = new List<EmailAttachment> { attachment }
						};

						await Email.Default.ComposeAsync(message);
					}
					else
					{
                        MainViewModel.Instance.LogUserMessage(Logger.Level.WARNING, "Sorry, the app cannot create an email for you.  Please send your request to lobsterconnect@turnipsoft.co.uk");
                    }
				}
				catch (Exception ex)
				{
					MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error while composing email: "+ex.Message);
				}
            }
			else if (action2 == "Reset App")
			{
				bool confirmation = await MainPage.Instance.DisplayAlert("Reset the application", "Please confirm you want to reset the application.  This will restore the application to its initial state and reload all game information from the internet.  Any recent changes that haven't synced yet will be lost.", "Reset", "Don't reset");
				if(confirmation)
				{
					await MainViewModel.Instance.ResetApp();
				}
            }

        }
		else if (action == "legal")
		{
            var popup = new PopupLegalTerms();
            var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
        }
		else if (action == "privacy")
		{
			var popup = new PopupDataHandling();
            var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
		}
		else if (action == "about")
		{
            int major = AppInfo.Version.Major;
            int minor = AppInfo.Version.Minor;
            int build = AppInfo.Version.Build;

			string msg = "LobsterConnect © Turnipsoft 2025\n\nVersion " + major.ToString() + "." + minor.ToString() + " build " + build.ToString();

			await DisplayAlert("About", msg, "Dismiss");
        }
		else if (action == "help")
		{
			await Browser.Default.OpenAsync("https://www.turnipsoft.com/lobsterconnect/");
        }
		else
		{
			Logger.LogMessage(Logger.Level.ERROR, "MainPage.FlyoutMenuAtion", "invalid action: " + action);
		}
    }

	async Task<bool> ShowAdminActions()
	{
        string action3 = await DisplayActionSheet("Admin Action", "Dismiss", null, "Add Gaming Event", "User Management", "De/Re-activate");

        if (string.IsNullOrEmpty(action3))
        {
            return false;
        }
        else
        {

            if (action3 == "Add Gaming Event")
            {
                string name = await DisplayPromptAsync("New Event", "Enter the name of the new gaming event (preferably as YYYY-MM-DD EVENT NAME)");
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
                else if (MainViewModel.Instance.CheckEventName(name))
                {
                    await DisplayAlert("Error", "An event with that name already exists", "Dismiss");
                    return false;
                }
                else
                {
                    string eventType = await DisplayActionSheet("Set event type", "Dismiss", null, "CONVENTION", "DAY", "EVENING");
                    if (string.IsNullOrEmpty(eventType))
                    {
                        return false;
                    }
                    else
                    {
                        MainViewModel.Instance.CreateGamingEvent(true, name, eventType, true);
                        MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Gaming event '" + name + "' has been created");
                        return true;
                    }
                }
            }
            if (action3 == "User Management")
            {
                string action4 = await DisplayActionSheet("User Admin", "Dismiss", null, "Grant Admin Rights", "Change Password", "Edit User Details");
                if (string.IsNullOrEmpty(action4))
                {
                    return false;
                }

                List<string> personHandles = MainViewModel.Instance.GetAvailablePersons(true);
                personHandles.Sort();
                var itemsViewer = new PopupItemsViewer();
                itemsViewer.SetItems(personHandles);
                var popupResult = await MainPage.Instance.ShowPopupAsync(itemsViewer, CancellationToken.None);
                string toChange = popupResult as string;
                if (string.IsNullOrEmpty(toChange))
                {
                    return false;
                }
                else
                {
                    Person p = MainViewModel.Instance.GetPerson(toChange);

                    if (action4 == "Grant Admin Rights")
                    {
                        if (p.IsAdmin)
                        {
                            await DisplayAlert("Admin", "'" + p + "' is already an admin", "Dismiss");
                            return false;
                        }
                        else
                        {
                            MainViewModel.Instance.UpdatePerson(true, p, isAdmin: true);
                            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Admin rights have been granted to " + toChange);
                            return true;
                        }
                    }
                    else if (action4 == "Change Password")
                    {
                        string newPassword = await DisplayPromptAsync("Password", "Enter the new password for '"+ toChange+"':");

                        if (string.IsNullOrEmpty(newPassword))
                        {
                            return false;
                        }
                        else
                        {
                            MainViewModel.Instance.UpdatePerson(true, p, password: Model.Utilities.PasswordHash(newPassword));
                            MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Password has been updated for " + toChange);
                            return true;
                        }
                    }
                    else if (action4 == "Edit User Details")
                    {
                        var popup2 = new PopupPersonDetails();
                        popup2.SetPerson(p);
                        var popupResult2 = await this.ShowPopupAsync(popup2, CancellationToken.None);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (action3 == "De/Re-activate")
            {
                string action4 = await DisplayActionSheet("Deactivate and Re-activate", "Continue", null, "Re-activate user", "Deactivate user", "Re-activate game", "Deactivate game", "Re-activate event", "Deactivate event");

                if (string.IsNullOrEmpty(action4))
                {
                    return false;
                }
                else
                {
                    bool? ifActive = null;
                    if (action4.StartsWith("Re-activate"))
                        ifActive = false; // in the items list, include only inactive items
                    else if (action4.StartsWith("Deactivate"))
                        ifActive = true;  // in the items list, include only inactive items

                    List<string> items = null;
                    if (action4.EndsWith("user"))
                    {
                        items = MainViewModel.Instance.GetAvailablePersons(ifActive);
                    }
                    else if (action4.EndsWith("game"))
                    {
                        items = MainViewModel.Instance.GetAvailableGames(ifActive);
                    }
                    else if (action4.EndsWith("event"))
                    {
                        items = MainViewModel.Instance.GetAvailableEvents(ifActive);
                    }
                    else
                    {
                        Logger.LogMessage(Logger.Level.ERROR, "MainPage.FlyoutMenuAction", "admin action is broken");
                        return false;
                    }

                    if (items == null || items.Count == 0)
                    {
                        await DisplayAlert("Admin Action", "There aren't any applicable items for this action", "Dismiss");
                        return false;
                    }
                    else
                    {
                        items.Sort();

                        var itemsViewer = new PopupItemsViewer();
                        itemsViewer.SetItems(items);
                        var popupResult = await MainPage.Instance.ShowPopupAsync(itemsViewer, CancellationToken.None);

                        string toChange = popupResult as string;
                        if (!string.IsNullOrEmpty(toChange))
                        {
                            bool confirmation = await MainPage.Instance.DisplayAlert(action3, "Please confirm you want to proceed to " + action4 + " '" + toChange + "'", "Proceed", "Cancel");
                            if (confirmation)
                            {
                                bool toIsActive = true;
                                if (action4.StartsWith("Deactivate"))
                                    toIsActive = false;

                                if (action4.EndsWith("user"))
                                {
                                    MainViewModel.Instance.UpdatePerson(true,
                                        MainViewModel.Instance.GetPerson(toChange),
                                        isActive: toIsActive);
                                }
                                else if (action4.EndsWith("game"))
                                {
                                    MainViewModel.Instance.UpdateGame(true,
                                        MainViewModel.Instance.GetGame(toChange),
                                        isActive: toIsActive);
                                }
                                else if (action4.EndsWith("event"))
                                {
                                    MainViewModel.Instance.UpdateGamingEvent(true,
                                        MainViewModel.Instance.GetGamingEvent(toChange),
                                        isActive: toIsActive);
                                }
                                else
                                {
                                    Logger.LogMessage(Logger.Level.ERROR, "MainPage.FlyoutMenuAction", "admin action is broken");
                                    return false;
                                }
                                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "Admin action: " + action4 + " '" + toChange + "' has been completed.");
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainPage.SowAdminActions", "invalid action: " + action3);
                return false;
            }
        }
    }

}

