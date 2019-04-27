using System;
using System.Linq;
using System.Threading.Tasks;
using SportsConnection;

using Xamarin.Forms;

public class Root : MasterDetailPage {

	public static SportsConnection.Menu sMenu;


	public Root() {
		initMasterDetailLayout();
		initMasterDetailListeners();

		initSelectMenuOptionListener();
		initEndOfBootUpProcessListener();
		inittMessagesFriendRelationshipsListener();

		tryAuthenticationWithCashedCredentials();
	}

	private void initMasterDetailLayout() {
		// Initialize the Master (Menu)
        sMenu = new SportsConnection.Menu();
		Master = sMenu;

		// Initalize the Detail (Main page)
		navigateToSplashPage();
	}

	private void navigateToSplashPage() {
		Master.IsVisible = false;
		Detail = (Page)Activator.CreateInstance(typeof(Splash));
	}

	private void initMasterDetailListeners() {
		sMenu.profileInfo.GestureRecognizers.Add(new TapGestureRecognizer {
            Command = new Command(() => navigateToPage(SportsConnection.Menu.MAIN_MENU_OPT_INDEX_PROFILE))
		});

		sMenu.mainMenuOptions.ItemSelected += (sender, e) =>
		  navigateTo(e.SelectedItem as SportsConnection.MenuItem).ConfigureAwait(true);

		sMenu.extraMenuOptions.ItemSelected += (sender, e) =>
		  navigateTo(e.SelectedItem as SportsConnection.MenuItem).ConfigureAwait(true);
	}

	private void initSelectMenuOptionListener() {
		MessagingCenter.Subscribe<SelectMenuOptionMessage>(this, SelectMenuOptionMessage.TAG, message => {
			Device.BeginInvokeOnMainThread(() => {

				if (message != null) {
					navigateToPage(message.menuOptionIdx);
				}
			});
		});
	}

	private void initEndOfBootUpProcessListener() {
		MessagingCenter.Subscribe<SystemHasInitializedMessage>(this, SystemHasInitializedMessage.TAG, message => {
			Device.BeginInvokeOnMainThread(() => {

				if (message != null) {
					DebugHelper.newMsg(Constants.TAG_ROOT, "Boot process has completed");
				}
			});
		});
	}

	private void inittMessagesFriendRelationshipsListener() {
		MessagingCenter.Subscribe<FriendshipMessage>(this, FriendshipMessage.TAG, message => {
			Device.BeginInvokeOnMainThread(() => {

				if (message != null) {
					DebugHelper.newMsg(Constants.TAG_ROOT, "The current user has a new friendship related message");
				}
			});
		});
	}
	   
	void navigateToPage(int menuOptionIdx) {
        if (menuOptionIdx.Equals(SportsConnection.Menu.MAIN_MENU_OPT_INDEX_SPLASH)) {
			navigateToSplashPage();
		} else {
			var selectedItem = sMenu.mainMenu.ElementAt(menuOptionIdx);

			if (selectedItem != null) {
				navigateTo(selectedItem).ConfigureAwait(true);
			}
		}
	}

	private async Task<bool> navigateTo(SportsConnection.MenuItem menuItem) {
		if (menuItem == null) {
			return false;
		} else {
			Page displayPage = null;

			switch (menuItem.title) {
				case Constants.LBL_MENU_OPT_PROFILE:
					Master.IsVisible = true;
					displayPage = new Profile(App.authController.getCurrentUser(), false);

					break;

				case Constants.LBL_MENU_OPT_LOCATIONS:
					Master.IsVisible = true;
					displayPage = (Page)Activator.CreateInstance(typeof(Locations));
					break;

				case Constants.LBL_MENU_OPT_YOUR_LOCATIONS:
					Master.IsVisible = true;
					displayPage = (Page)Activator.CreateInstance(typeof(LocationsManagement));
					break;

				case Constants.LBL_MENU_OPT_FRIENDS:
					Master.IsVisible = true;
					displayPage = (Page)Activator.CreateInstance(typeof(Friends));
					break;

				case Constants.LBL_MENU_OPT_SETTINGS:
					Master.IsVisible = true;
					displayPage = (Page)Activator.CreateInstance(typeof(Settings));
					break;

				case Constants.LBL_MENU_OPT_ABOUT_US:
					Master.IsVisible = true;
					displayPage = (Page)Activator.CreateInstance(typeof(About));
					break;

				case Constants.LBL_MENU_OPT_SIGNOUT:
					await signOut();
					closeApp();

					break;
			}

			if (displayPage != null) {
				try {
					Detail = new NavigationPage(displayPage);
				} catch (Exception ex) {
					DebugHelper.newMsg(Constants.TAG_ROOT, ex.Message);
				}
			}

			resetBackgroundColorMenuItems();
			IsPresented = false;
		}

		return true;
	}

	private void resetBackgroundColorMenuItems() {
		sMenu.mainMenuOptions.SelectedItem = null;
		sMenu.extraMenuOptions.SelectedItem = null;
	}

	public static async Task signOut() {
		if (App.authController != null) {
			await App.authController.signOut();
		}
	}

	private void closeApp() {
		var androidController = DependencyService.Get<IAndroid>();

		if (androidController != null) {
			androidController.finishApp();
		}
	}

	/// <summary>
	/// Verify if the authentication token of the current user is valid. If it is update the user info and return
	/// true to the callback method below, otherwise return false.
	/// </summary>
	private void tryAuthenticationWithCashedCredentials() {
		Action<bool> completedAuthProcess = onAuthProcessCompleted;
		App.authController.verifyUserCredentialsAndTryByPass(completedAuthProcess).ConfigureAwait(true);
	}

	/// <summary>
	/// Perform the callback action for the method 'tryAuthenticationWithCashedCredentials'.
	/// </summary>
	public void onAuthProcessCompleted(bool success) {
		if (success) {
            navigateToPage(SportsConnection.Menu.MAIN_MENU_OPT_INDEX_LOCATIONS);
			displayWelcomeDialog();
			updateUIWithUserInfo();
			App.authController.finishAuthenticationProcess();
		} else {
			navigateToAuthentication();
		}
	}

	private void displayWelcomeDialog() {
		if (SettingsController.isFirstTime()) {
			DisplayAlert(Txt.TITLE_WELCOME, Txt.MSG_WELCOME, Txt.LBL_BTN_OK).ConfigureAwait(false);
		}
	}

	public static void updateUIWithUserInfo() {
		sMenu.profileImage.Source = App.authController.getCurrentUser().profileImage;
		sMenu.profileInfo.Text = App.authController.getCurrentUser().name;

		// Todo: Enable this line of code to test the integration of the controllers of the app with the backend.
		//executeBackendIntegrationTests();
	}

	private void executeBackendIntegrationTests() {
		var globalTestModule = new GlobalTestModule();
		globalTestModule.init().ConfigureAwait(true);
	}

	private void navigateToAuthentication() {
		Action<bool> authCallback = onAuthProcessCompleted;
		Navigation.PushModalAsync(new Authentication(authCallback));
	}

}