using System;
using SportsConnection;

using Xamarin.Forms;

public partial class Root : MasterDetailPage, AuthCallback {

	private string TAG = "Root";

	public Menu menu;


	public Root() {
		// Initialize the master and the detail page
		menu = new Menu();
		Master = menu;

		// Initialize the Detail page as the Location page
		Detail = new NavigationPage(new Locations());

		// Add event listeners to the menu option's
		menu.profileMenuOptions.Clicked += (sender, e) => navigateToProfilePage();
		menu.mainMenuOptions.ItemSelected += (sender, e) => navigateTo(e.SelectedItem as SportsConnection.MenuItem);
		menu.settingsMenuOptions.ItemSelected += (sender, e) => navigateTo(e.SelectedItem as SportsConnection.MenuItem);

		// Initialize the authentication flow and try to by pass the login page if the user already has a valid 
		// access token
		App.authController.checkUserCredentialsAndTryByPass(this);
	}

	/// <summary>
	/// Navigate to the page indicated by the MenuItem's 'targetType' attribute
	/// </summary>
	/// <param name="menuItem">MenuItem object</param>
	private void navigateTo(SportsConnection.MenuItem menuItem) {
		if (menuItem == null) {
			return;
		}

		Page displayPage = null;

		// Process the Settings menu options
		if (typeof(Settings) == menuItem.targetType) {
			switch (menuItem.title) {
			case Constants.LBL_MENU_OPT_SETTINGS:
			displayPage = (Page)Activator.CreateInstance(typeof(Settings));
			break;

			case Constants.LBL_MENU_OPT_ABOUT_US:
			displayPage = (Page)Activator.CreateInstance(typeof(About));
			break;

			case Constants.LBL_MENU_OPT_SIGNOUT:
			signOut();
			break;

			default:
			break;
			}
		} else {
			// Process the regular menu options
			displayPage = (Page)Activator.CreateInstance(menuItem.targetType);
		}

		// Try to navigate to the selected page
		try {
			Detail = new NavigationPage(displayPage);
		} catch (Exception ex) {
			DebugHelper.newMsg(TAG, ex.Message);
		}

		// Reset the background color of a selected item of a menu option
		menu.mainMenuOptions.SelectedItem = null;
		menu.settingsMenuOptions.SelectedItem = null;
		IsPresented = false;
	}

	/// <summary>
	/// Create a new instance of the Profile page and redirect the user
	/// </summary>
	void navigateToProfilePage() {
		Page displayPage = (Page)Activator.CreateInstance(typeof(Profile));

		try {
			Detail = new NavigationPage(displayPage);
		} catch (Exception ex) {
			DebugHelper.newMsg(TAG, ex.Message);
		}

		IsPresented = false;
	}

	/// <summary>
	/// Create a new instance of the Locations page and redirect the user
	/// </summary>
	void navigateToLocationsPage() {
		Page displayPage = (Page)Activator.CreateInstance(typeof(Locations));

		try {
			Detail = new NavigationPage(displayPage);
		} catch (Exception ex) {
			DebugHelper.newMsg(TAG, ex.Message);
		}

		IsPresented = false;
	}

#pragma warning disable RECS0165
	/// <summary>
	/// Remove the Facebook access token, log the user out of the system and redirects it 
	/// to the authentication page.
	/// </summary>
	public async void signOut() {
		await App.authController.signOut();
	}
#pragma warning restore RECS0165

	/// <summary>
	/// Get the response of the authentication by pass attempt
	/// </summary>
	/// <param name="success">If set to <c>true</c> success.</param>
	public void onAuthProcessCompleted(bool success) {
		if (success) {
			// ..
			navigateToLocationsPage();
		} else {
			OnAppearing();
		}
	}

#pragma warning disable RECS0165
	/// <summary>
	/// Validate the user data before showing any page.
	/// </summary>
	protected override async void OnAppearing() {
		base.OnAppearing();

		if (!App.authController.isUserAuthenticated()) {
			await Navigation.PushModalAsync(new Authentication());
		}
	}
#pragma warning restore RECS0165

}