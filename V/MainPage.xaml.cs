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

using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

using LobsterConnect.VM;
using LobsterConnect.Model;

namespace LobsterConnect.V;

/// <summary>
/// There is only one content page in the app; it's this one.  It consists of a menu bar at the top
/// including controls for logging on, for adding a session, for setting a 'would like to play' wish-list,
/// for applying a filter and for selecting gaming events; below that is the main table of
/// planned sessions, and at the bottom is a window showing logged messages (most recent message at
/// the top).  The planned sessions can alternatively be shown as a vertical list (without any gaps).  A toggle
/// control is provided, to switch between these views.
/// There is a hamburger menu and a title bar heading - but these are defined in the AppShell.xaml file.
/// 
/// For design notes concerning the app as a whole, please refer to App.xaml.cs.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Construct the main page (expect this to be called from MAUI boilerplate code when the app is launched).
    /// Since we know that this will be called only once, and will be called soon after the app is launched,
    /// we put all the app initialisation code here.
    /// </summary>
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

        // The "UserHandle" preference contains the name of a person; if present, this person
        // will be automatically logged on at startup, without any need to enter a password.
        if (Preferences.ContainsKey("UserHandle"))
        {
            string defaultUserName = Preferences.Get("UserHandle", "");
            if (!string.IsNullOrEmpty(defaultUserName))
            {
				Person defaultUser= MainViewModel.Instance.GetPerson(defaultUserName);
                if(defaultUser != null)
				{
					MainViewModel.Instance.SetLoggedOnUser(defaultUser, true);
                }
            }
        }

        // If age verification is needed, we defer the loading of the gaming event until it's done (because
        // in some circumstances the gaming event initialisation can pop up an alert, and we don't want to be
        // showing two alerts at the same time.
        if (!Preferences.ContainsKey("AgeConfirmed"))
        {
            this.ParentChanged += AgeVerification; // the ParentChanged event fires when the main page is inserted into the visual hierarchy
        }
        else
        {
            // Make sure we have fetched the gaming event list from the cloud, and set the current event to something reasonable
            InitialiseGamingEvent();
        }

        // As long as this page is loaded, we need to respond to a 'sessions must be refreshed' event raised
        // by the viewmodel, by refreshing the main UI grids that show all the sessions.
        this.Loaded += (o, e) =>
		{
			MainViewModel.Instance.SessionsMustBeRefreshed += RefreshSessionsGrids;
		};

        this.Unloaded += (o, e) =>
        {
            MainViewModel.Instance.SessionsMustBeRefreshed -= RefreshSessionsGrids;
        };

        // Standard XAML-loading stuff.
        InitializeComponent();

#if IOS // nasty fix for a layout oddity with the layout toggle switch on iOS
        this.gdLayoutSwitch.HeightRequest=60;
#endif

        // populate the main table and list of session details (the two null parameters don't mean anything)
        RefreshSessionsGrids(null, null);
    }

    public static MainPage Instance = null;

	protected override void OnAppearing()
	{
		base.OnAppearing();

        // Ridiculous workaround for the fact that MAUI doesn't deal properly with status bar colours.
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

    /// <summary>
    /// Show a message to confirm the user is an adult.  Note that this method is called in response to ParentChanged
    /// events on the main page (because this is the earliest indication we get that we have a UI that's capable
    /// of displaying an alert).
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public void AgeVerification(object o, EventArgs e)
    {
        if (this.Parent != null) // i.e. if the parent of MainPage has been set to something real.
        {
            this.ParentChanged -= AgeVerification; // remove this handler because we don't want to call it again

            DispatcherHelper.RunAsyncOnUI(async () =>
            {
                await DispatcherHelper.SleepAsync(2000);

                bool tooYoung = await DisplayAlert("Age Verification",
                    "Because the app allows you to enter personal data and to create and view user-created content, it is not suitable for persons under 18 years of age; such persons must not use the app. Are you aged 18 or more?",
                    "Yes", "No");
                // If the user is not over 18, hide the UI (the user won't be able to do anything more with the app).
                if (tooYoung)
                {
                    this.gdMainPage.IsVisible = false;

                    await this.DisplayAlert("Age Verification", "Because you answered NO to the age verification question, you will not be able to run the app.  Please close it now", "Dismiss");
                }
                else // If the user is 18 or over, save True into the app's preferences, so we don't ask again.
                {
                    Preferences.Set("AgeConfirmed", true);

                    // We didn't do this at the time, because we need to ensure that only one alert is displayed
                    // at a time.
                    InitialiseGamingEvent();

                    // Show the 'first run' message, with the quick-start notes for the app.
                    Popup popup = new PopupFirstRunMessage();
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                }
            });
        }
    }

    /// <summary>
    /// Reload the contents of the table of sessions and the list of sessions
    /// <param name="o"></param>
    /// <param name="a"></param>
    /// </summary>
    public void RefreshSessionsGrids(object o, SessionsRefreshEventArgs a)
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

        // If the table grid isn't changing its width, then we won't want to scroll it to a new position.
        bool resizeTableGrid = this.gdSlotLabels.WidthRequest!= sessions.Length * 100;

        this.gdSlotLabels.Children.Clear();
		this.gdSlotLabels.WidthRequest = sessions.Length * 100;

        if (resizeTableGrid)
        {
            // Position the row of time slot labels at coordinate 0.
            this.alSlotLabels.SetLayoutBounds(this.gdSlotLabels, new Rect(0, 0, gdSlotLabels.WidthRequest, 30));
            
            // Scroll the sessions table to (0,0)
            this.svSessionsTable.ScrollToAsync(0, 0, false);
        }

        this.gdSessionsTable.Children.Clear();
		this.gdSessionsTable.ColumnDefinitions.Clear();

        this.slSessionsList.Children.Clear();
        this.svSessionsList.ScrollToAsync(0, 0, false);

        this.gdSlotLabels.Children.Add(
			new StackLayout() {
				Orientation = StackOrientation.Horizontal,
				WidthRequest= sessions.Length * 100,
				HeightRequest=40,
				Spacing=0
			}
			.Assign(out StackLayout slSlotLabels));

		for(int s=0; s<sessions.Length; s++)
		{
			SessionTime t = new SessionTime(s);

            // a heading for this column, containing the label for the session time slot in the table view
			slSlotLabels.Children.Add(
				new Label() {
					WidthRequest = 100,
					HeightRequest = 40,
					TextColor=Colors.LightGray,
					HorizontalTextAlignment= TextAlignment.Center,
					Text = t.ToString() });

			this.gdSessionsTable.ColumnDefinitions.Add(new ColumnDefinition() { Width = 100 });
			if (sessions[s]!=null)
			{
                // Add a vertical stacklayout to hold all the sessions that belong in this time slot column in the table view
				this.gdSessionsTable.Add(
					new StackLayout() {
						HeightRequest = 75 * sessions[s].Count,
						WidthRequest = 100,
						Orientation = StackOrientation.Vertical,
						Spacing = 0,
						VerticalOptions=LayoutOptions.Start
					}
					.Assign(out StackLayout slSessions)
					.Invoke(sl => Grid.SetColumn(sl,s)));

                // Add each of the sessions that belong to this column in the table view
				foreach (Session session in sessions[s])
				{
                    View v = CreateSessionView(session);
                    slSessions.Children.Add(v);
                }

                // Add a horizontal stacklayout to the sessions list stacklayout, to contain the slot
                // label text, then all the sessions that belong in this time slot row in the list
                if (sessions[s]!=null && sessions[s].Count>0)
                {
                    this.slSessionsList.Add(new HorizontalStackLayout()
                    {
                        HeightRequest = 75,
                        WidthRequest = 100 * (1 + sessions[s].Count),
                        Spacing = 0,
                        HorizontalOptions = LayoutOptions.Start
                    }.Assign(out HorizontalStackLayout hslSessions));

                    hslSessions.Children.Add(new Label()
                    {
                        WidthRequest = 100,
                        HeightRequest = 75,
                        TextColor = Colors.Black,
                        Padding = 5,
                        HorizontalTextAlignment = TextAlignment.Start,
                        VerticalOptions = LayoutOptions.Center,
                        Text = t.ToString()  
                    });

                    foreach (Session session in sessions[s])
                    {
                        View v = CreateSessionView(session);
                        hslSessions.Children.Add(v);
                    }
                }
            }
		}
    }

    /// <summary>
    /// Create a view (a border) containing all the information about the session.  It will fit nicely into
    /// a space 75 wide and 100 high.  If tapped, it will present a popup for editing the session.
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    private View CreateSessionView(Session session)
    {
        Border bdr =
            new Border()
            {
                Stroke = Colors.White,
                HeightRequest = 73,
                WidthRequest = 98,
                Margin = 1,
                Padding = 1,
                Content = new StackLayout()
                {
                    Background = Colors.LightGray,
                    HeightRequest = 69,
                    WidthRequest = 94,
                    Orientation = StackOrientation.Vertical,
                    Spacing = 0,
                    Margin = 0,
                    Padding = 1,
                    Children =
                    {
                                    new Label() { // name of the game
										HeightRequest = 22,
                                        WidthRequest = 92,
                                        LineBreakMode = LineBreakMode.TailTruncation,
                                        Text = session.ToPlay },
                                    new Label() { // state of the session (OPEN/FULL/ABANDONED)
                                        HeightRequest = 22,
                                        WidthRequest = 92,
                                        LineBreakMode = LineBreakMode.NoWrap,
                                        }.Assign(out Label lbState),
                                    new StackLayout { // number of signups, compared with available seats
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
            };

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

        // Tapping on the session box will open the session management popup
        TapGestureRecognizer tr = new TapGestureRecognizer();
        tr.BindingContext = session;
        tr.Tapped += (object sender, TappedEventArgs e) =>
        {
            Session s = ((StackLayout)sender).BindingContext as Session;

#pragma warning disable 4014 // the method below will run asynchronously, but I am fine to let the tapped handler exit in the meantime
            ShowSessionManagementPopup(s);
#pragma warning restore 4014
        };
        slSession.GestureRecognizers.Add(tr);
        slSession.BindingContext = session;

        return bdr;
    }

    /// <summary>
    /// Handle the a tap on the user button by offering the user options (log in, set up user, edit user, log out)
    /// </summary>
    /// <param name="o">ignored</param>
    /// <param name="e">ignored</param>
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
                        // Capture name and password for the new user
                        var popup1 = new PopupLogIn();
                        var popupResult1 = await this.ShowPopupAsync(popup1, CancellationToken.None);

                        if (popupResult1 != null && popupResult1 is Tuple<string, string, bool>)
                        {
                            Tuple<string, string, bool> userAndPassword = (Tuple<string, string, bool>)popupResult1;

                            Person user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

                            // check the input was valid

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

                            // get them to re-enter the password, and fail if it does not match.
                            string password2 = await DisplayPromptAsync("New User", "Please enter the password again", keyboard: Keyboard.Password);
                            if(password2!= userAndPassword.Item2)
                            {
                                await DisplayAlert("New User", "The passwords did not match", "Dismiss");
                                return;
                            }

                            try
                            {
                                // Create a new user using the details provided
                                MainViewModel.Instance.CreatePerson(true, userAndPassword.Item1, password: Model.Utilities.PasswordHash(userAndPassword.Item2));

                                MainViewModel.Instance.LogUserMessage(Logger.Level.INFO, "User '" + userAndPassword.Item1 + "' has been created");

                                // Log in using the new user details
                                user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);
                                MainViewModel.Instance.SetLoggedOnUser(user, userAndPassword.Item3);

                                // Show a popup for entering all the attributes of the new user.
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
                        // Capture user name and password
						var popup = new PopupLogIn();
						var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

						if (popupResult != null && popupResult is Tuple<string, string, bool>)
						{
							Tuple<string, string, bool> userAndPassword = (Tuple<string, string, bool>)popupResult;

							Person user = MainViewModel.Instance.GetPerson(userAndPassword.Item1);

							if (user == null)
                            // check the user exists
                            {
                                await DisplayAlert("Login", "There isn't any user having user handle '" + userAndPassword.Item1 + "'", "Dismiss");
								return;
							}
                            else if (user.Password != Model.Utilities.PasswordHash(userAndPassword.Item2))
                            // check the password matches the one stored in the viewmodel
							{

								await DisplayAlert("Login", "That is the wrong password for user '" + userAndPassword.Item1 + "'", "Dismiss");
								return;
							}

                            // log the user in
							MainViewModel.Instance.SetLoggedOnUser(user, userAndPassword.Item3);
						}
					}
					catch (Exception ex)
					{
                        MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error logging in: " + ex.Message);
                    }
                }
            }
			else // the case where there is a user logged in at the moment
			{
				const string editUser = "Edit user details";
				const string changePassword = "Change password";
				const string logOut = "Log out";

				string a = await DisplayActionSheet("User actions", "Cancel", null, editUser, changePassword, logOut);

				if (a == editUser)
				{
                    // use a PopupPersonDetails to edit the user's attributes
                    var popup = new PopupPersonDetails();
                    popup.SetPerson(MainViewModel.Instance.LoggedOnUser);
                    var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
                }
				else if (a == changePassword)
				{
                    // capture existing password and new password (twice), then update the viewmodel.
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

    /// <summary>
    /// Add a new session using a PopupAddSession
    /// </summary>
    /// <param name="o">ignored</param>
    /// <param name="e">ignored</param>
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
                if(!PopupHints.DontShowAgain("AddSession"))
                    await this.ShowPopupAsync(new PopupHints().SetUp("AddSession", true), CancellationToken.None);

				var popup = new PopupAddSession();
				var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
			}
		}
		catch(Exception ex)
		{
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error adding session: " + ex.Message);
        }
    }

    /// <summary>
    /// Manage the user's wish-list using a PopupManageWishlist
    /// </summary>
    /// <param name="o">ignored</param>
    /// <param name="e">ignored</param>
    async void btnWishListClicked(Object o, EventArgs e)
    {
        try
        {
            if (MainViewModel.Instance.LoggedOnUser == null)
            {
                await DisplayAlert("Would Like to Play", "Log in first please, before setting up a list of games you'd like to play", "Dismiss");
                return;
            }
            else if (!MainViewModel.Instance.CurrentEvent.IsActive)
            {
                await DisplayAlert("Would Like to Play", "The current gaming event '" + MainViewModel.Instance.CurrentEvent.Name + "' is not active so games won't be happening in it", "Dismiss");
                return;
            }
            else
            {
                var popup = new PopupManageWishList();
                var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error managing wish-list: " + ex.Message);
        }
    }

    /// <summary>
    /// Manage the current filter using a PopupManageFilter
    /// </summary>
    /// <param name="o">ignored</param>
    /// <param name="e">ignored</param>
    async void btnFilterClicked(Object o, EventArgs e)
    {
        if (!PopupHints.DontShowAgain("ManageFilter"))
            await this.ShowPopupAsync(new PopupHints().SetUp("ManageFilter", true), CancellationToken.None);

        var popup = new PopupManageFilter();
        popup.SetFilter(new SessionFilter(MainViewModel.Instance.CurrentFilter));
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

		if(popupResult as SessionFilter!=null)
		{
			MainViewModel.Instance.CurrentFilter = popupResult as SessionFilter;
		}
    }

    /// <summary>
    /// Present the session management popup
    /// </summary>
    /// <param name="s">the session to be managed</param>
    /// <returns></returns>
	public async Task<bool> ShowSessionManagementPopup(Session s)
	{
        if (!PopupHints.DontShowAgain("ManageSession"))
            await this.ShowPopupAsync(new PopupHints().SetUp("ManageSession", true), CancellationToken.None);

        var popup = new PopupManageSession();
		popup.SetSession(s);
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

        return true;
	}

    /// <summary>
    /// When the sessions table scrollview is scrolled, we need to reposition the slot labels above it, to be displaced
    /// by the same amount.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void svSessionsTable_Scrolled(object sender, ScrolledEventArgs e)
    {
        this.alSlotLabels.SetLayoutBounds(this.gdSlotLabels, new Rect(-e.ScrollX, 0, gdSlotLabels.WidthRequest, 30));
	}

    /// <summary>
    /// When the gaming event label is tapped, we present a chooser for selecting switching between the available
    /// gaming events.
    /// </summary>
    /// <param name="sender">ignored</param>
    /// <param name="e">ignored</param>
    private async void lblEventTapped(object sender, TappedEventArgs e)
    {
        if (!PopupHints.DontShowAgain("ChooseEvent"))
            await this.ShowPopupAsync(new PopupHints().SetUp("ChooseEvent", true), CancellationToken.None);

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

    /// <summary>
    /// When the main table of sessions is tapped (in a region not containing a session) or when the header
    /// containing the slot labels is tapped, we show a popup for creating a new session, figuring out what
    /// session time slot to pick initially by looking at the X coordinate of the tap gesture.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void gridSessionTimeSlotTapped(object sender, TappedEventArgs e)
    {
        // Position relative to the container view
        Point? pt = e.GetPosition((View)sender);

        int sessionTimeSlot = (int)(pt.Value.X / 100); // session items are 100 pixels wide

        if (sessionTimeSlot >= SessionTime.NumberOfTimeSlots)
            sessionTimeSlot = SessionTime.NumberOfTimeSlots - 1;

        if (sessionTimeSlot < 0)
            sessionTimeSlot = 0;

        if (!PopupHints.DontShowAgain("AddSession"))
            await this.ShowPopupAsync(new PopupHints().SetUp("AddSession", true), CancellationToken.None);

        var popup = new PopupAddSession();
        popup.SetTimeSlot(sessionTimeSlot); // set the initial selected value in the time slot picker in the popup
        var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
    }

    /// <summary>
    /// Initialise the current gaming event to a valid value, using the saved preference if that exists
    /// and is valid.
    /// </summary>
    public void InitialiseGamingEvent()
	{
        List<string> availableEvents = MainViewModel.Instance.GetAvailableEvents();
        if (availableEvents.Count == 0) // the event list hasn't been fetched from the cloud sync service yet
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

    /// <summary>
    /// Handle the various things that can be done on the flyout menu (defined in AppShell.xaml).
    /// Note that there is a special check to disable all the actions if the user has not yet verified 
    /// that they are an adult.
    /// </summary>
    /// <param name="action"></param>
	public async void FlyoutMenuAction(string action)
	{
        if (!Preferences.ContainsKey("AgeConfirmed"))
            return;

        if (action=="event") // choose a gaming event
		{
			lblEventTapped(this, new TappedEventArgs(null));
        }
		else if (action=="people") // view a list of all persons
		{
			List<string> allPersons = MainViewModel.Instance.GetAvailablePersons();
            allPersons.Sort();
            PopupViewPersons personsViewer = new PopupViewPersons();
            personsViewer.SetPersons(allPersons);
            var popupResult = await MainPage.Instance.ShowPopupAsync(personsViewer, CancellationToken.None);
        }
        else if (action == "games") // view a list of all games
        {
            List<string> allGames = MainViewModel.Instance.GetAvailableGames();
            PopupViewGames gamesViewer = new PopupViewGames();
            var popupResult = await MainPage.Instance.ShowPopupAsync(gamesViewer, CancellationToken.None);
        }
        else if (action == "addsession") // add a new gaming session
		{
			btnAddSessionClicked(this, new EventArgs());
        }
        else if (action == "wishlist") // show the 'would like to play' popup
        {
            btnWishListClicked(this, new EventArgs());
        }
        else if (action == "filter") // modify the filter settings
		{
			btnFilterClicked(this, new EventArgs());
		}
        else if (action == "layout") // switch between table and list layout of sessions
        {
            this.swSessionsLayout.IsToggled = !this.swSessionsLayout.IsToggled;
        }
        else if (action == "openlink") // open a session link (an app url like lobsterconnect:///<session-id>)
        {
            string s = await DisplayPromptAsync("Open Link to a Session", "If you've been sent a link to a session (some text beginning 'lobsterconnect://') enter it here, to open the gaming session");
            if(!string.IsNullOrEmpty(s))
            {
                await MainViewModel.Instance.OpenSessionFromUrl(s);
            }
        }
        else if (action == "support") // show the support actions
		{
			string action2 = null;
			
            // Only if the user is an admin do we include the 'Admin Actions' items
			if(MainViewModel.Instance.LoggedOnUser!=null && MainViewModel.Instance.LoggedOnUser.IsAdmin)
				action2 = await DisplayActionSheet("Support", "Dismiss", null, "Admin Action", "Show First Run Message", "Email for support", "Reset App", "View Source Code");
			else
                action2 = await DisplayActionSheet("Support", "Dismiss", null, "Show First Run Message", "Email for support", "Reset App", "View Source Code");

            if (action2 == "Admin Action")
			{
				await ShowAdminActions();
            }
            else if (action2 == "Show First Run Message")
            {
                Popup popup = new PopupFirstRunMessage();
                var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);
            }
            else if (action2 == "Email for support")
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
                    MainViewModel.Instance.LogUserMessage(Logger.Level.ERROR, "Error while composing email: " + ex.Message);
                }
            }
            else if (action2 == "Reset App")
            {
                bool confirmation = await MainPage.Instance.DisplayAlert("Reset the application", "Please confirm you want to reset the application.  This will restore the application to its initial state and reload all game information from the internet.  Any recent changes that haven't synced yet will be lost.", "Reset", "Don't reset");
                if (confirmation)
                {
                    await MainViewModel.Instance.ResetApp();
                }
            }
            else if (action2 == "View Source Code")
            {
                await Browser.Default.OpenAsync("https://github.com/jrc14/LobsterConnect");
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

			string msg = "LobsterConnect © Turnipsoft 2025\n\nGNU General Public Licence v3.0\n\nVersion " + major.ToString() + "." + minor.ToString() + " build " + build.ToString();

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

    /// <summary>
    /// Diplay the possible actions that an admin can perform
    /// </summary>
    /// <returns></returns>
	async Task<bool> ShowAdminActions()
	{
#if DEBUG
        string action3 = await DisplayActionSheet("Admin Action", "Dismiss", null, "Add Gaming Event", "User Management", "De/Re-activate", "Add Test Data");
#else
        string action3 = await DisplayActionSheet("Admin Action", "Dismiss", null, "Add Gaming Event", "User Management", "De/Re-activate");
#endif
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
                else if (name.Contains(','))
                {
                    await DisplayAlert("Error", "Event names must not contain commas", "Dismiss");
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
            else if (action3 == "User Management")
            {
                string action4 = await DisplayActionSheet("User Admin", "Dismiss", null, "Grant Admin Rights", "Change Password", "Edit User Details");
                if (string.IsNullOrEmpty(action4))
                {
                    return false; // the admin cancelled the action sheet
                }

                List<string> personHandles = MainViewModel.Instance.GetAvailablePersons(true);
                personHandles.Sort();
                var itemsViewer = new PopupItemsViewer();
                itemsViewer.SetItems(personHandles);
                var popupResult = await MainPage.Instance.ShowPopupAsync(itemsViewer, CancellationToken.None);
                string toChange = popupResult as string;
                if (string.IsNullOrEmpty(toChange))
                {
                    return false; // the admin exited the person chooser popup without choosing a person
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
                        string newPassword = await DisplayPromptAsync("Password", "Enter the new password for '" + toChange + "':");

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
                    else // unrecognised action - we do not expect to end up here
                    {
                        return false;
                    }
                }
            }
            else if (action3 == "De/Re-activate")
            {
                // Care: don't change the strings on this action sheet without looking at the logic further down, which depends on particular string values
                string action4 = await DisplayActionSheet("Deactivate and Re-activate", "Continue", null, "Re-activate user", "Deactivate user", "Re-activate game", "Deactivate game", "Re-activate event", "Deactivate event");

                if (string.IsNullOrEmpty(action4)) // the admin cancelled the action sheet
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
                                return false; // the admin didn't confirm the deactivate/re-activate action
                            }
                        }
                        else
                        {
                            return false; // the admin didn't choose an item to perform the action on
                        }
                    }
                }
            }
#if DEBUG
            else if (action3 == "Add Test Data")
            {
                string action4 = await DisplayActionSheet("Test Data", "Dismiss", null, "Add Persons", "Add Sessions", "Add WishList");
                if (string.IsNullOrEmpty(action4))
                {
                    return false; // the admin cancelled the action sheet
                }
                if (action4 == "Add Persons")
                {
                    MainViewModel.Instance.AddTestPersons(true);
                    return true;
                }
                else if (action4 == "Add Sessions")
                {
                    GamingEvent e = MainViewModel.Instance.CurrentEvent;
                    MainViewModel.Instance.AddTestSessionsAndSignUps(e, true);
                    return true;
                }
                else if (action4 == "Add WishList")
                {
                    GamingEvent e = MainViewModel.Instance.CurrentEvent;
                    MainViewModel.Instance.AddTestWishList(e, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
#endif
            else
            {
                Logger.LogMessage(Logger.Level.ERROR, "MainPage.ShowAdminActions", "invalid action: " + action3);
                return false;
            }
        }
    }

}

