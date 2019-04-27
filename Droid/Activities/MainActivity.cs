using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android;

using FFImageLoading.Forms.Droid;
using Microsoft.WindowsAzure.MobileServices;

using Xamarin.Forms;
using Android.Content;
using Android.Locations;
using ImageCircle.Forms.Plugin.Droid;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SportsConnection.Droid{

	[Activity(
		Label = "SportsConnection.Droid",
		Icon = "@drawable/icon",
		Theme = "@style/MyTheme",
		MainLauncher = false,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		LaunchMode = LaunchMode.Multiple
	)]

	public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IAuth {

		// Expose a static instance of this activity, it is used by different services 
		// to access the Android Context globally
		private Bundle mSavedState;

		// Controllers
		public AuthControllerDroid authenticationController;
		public PushServiceControllerDroid pushNotificationServiceController;
		public LocationServiceControllerDroid locationServiceController;
		public GeofencingServiceControllerDroid geofencingServiceController;

		// Permissions
		private LocationManager mLocationManager;
		private const int mRequestLocationId = 0;
		private readonly string[] mPermissionsLocation = {
	  		Manifest.Permission.AccessCoarseLocation,
	  		Manifest.Permission.AccessFineLocation
		};

		// Dialogs
		private AlertDialog mDialogLocationPermissions;
		private AlertDialog mDialogLastTryLocationPermissions;


		#region Activity Construtor and Initialization
		/// <summary>
		/// Activity Life cycle method
		/// 
		/// This is method is called once to initialize the dependencies of the app and Xamarin.Forms.
		/// </summary>
		protected override void OnCreate(Bundle savedInstanceState) {
			setLatestAppState(savedInstanceState);

			RequestWindowFeature(WindowFeatures.ActionBarOverlay);
			base.OnCreate(getLatestAppState());

			Forms.Init(this, getLatestAppState());
			Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

			initUIComponents();
			initAzureBackend();
			initAuthentication();
			initServiceStateListeners();

			LoadApplication(new App());
			OnSaveInstanceState(getLatestAppState());
		}

		/// <summary>
		/// Activity Life cycle method
		/// 
		/// This is method is called automatically whenever the activity state is updated, usually after 
		/// its creation.
		/// </summary>
		protected override void OnSaveInstanceState(Bundle outState) {
			setLatestAppState(outState);

			if (verifyLocationPermissions() && !AndroidController.hasInitialized()) {
				initMaps();
				initControllersAndStartServices();

				// Clean initilization variables, set the state of the activity and register services.
				dismissPermissionsDialogs();
				AndroidController.setHasInitialized(true);

				OnResume();
			}
		}

		private void initMaps() {
			Xamarin.FormsMaps.Init(this, getLatestAppState());
		}

		private void initUIComponents() {
			TabLayoutResource = Resource.Layout.tabs;
			ToolbarResource = Resource.Layout.toolbar;

			var width = Resources.DisplayMetrics.WidthPixels;
			var height = Resources.DisplayMetrics.HeightPixels;
			var density = Resources.DisplayMetrics.Density;
			SettingsController.setPhoneWidth((width - Constants.PARAM_DENSITY_WIDTH) / density);
			SettingsController.setPhoneHeight((height - Constants.PARAM_DENSITY_HEIGHT) / density);

			CachedImageRenderer.Init();
			ImageCircleRenderer.Init();
		}

		private void initAzureBackend() {
			CurrentPlatform.Init();
		}

		/// <summary>
		/// Initializes the PCL authentication controller connected to the AzureMobileServices server
		/// </summary>
		private void initAuthentication() {
			AuthUserController.init(this);
		}

		private void initControllersAndStartServices() {
			pushNotificationServiceController = new PushServiceControllerDroid();
			pushNotificationServiceController.register();
		}

		private void dismissPermissionsDialogs() {
			if (mDialogLocationPermissions != null) {
				mDialogLocationPermissions.Hide();
				mDialogLocationPermissions = null;
			}

			if (mDialogLastTryLocationPermissions != null) {
				mDialogLastTryLocationPermissions.Hide();
				mDialogLastTryLocationPermissions = null;
			}
		}

		private void initServiceStateListeners() {
			MessagingCenter.Subscribe<GeofencingServiceStopped>(this, GeofencingServiceStopped.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {
					finishGeofencingService();
					OnResume();
				});
			});
		}
		#endregion


		#region Permissions Handling
		/// <summary>
		/// Activity Life cycle method
		/// 
		/// Refreshs the activity information and permissions.
		/// </summary>
		protected override void OnResume() {
			base.OnResume();

			if (verifyLocationPermissions()) {

				if (!areLocationServiceDependenciesOk()) {
					showEnableGpsDialog();

				} else {
					if (locationServiceController == null) {
						locationServiceController = new LocationServiceControllerDroid();
						locationServiceController.registerAndStart();
					}
					 
					if (geofencingServiceController == null) {
						geofencingServiceController = new GeofencingServiceControllerDroid();
						geofencingServiceController.registerAndStart();
					}
				}
			} else {
				if (locationServiceController != null && geofencingServiceController != null) {
					finishGeofencingService();
				}
			}
		}

		/// <summary>
		/// Handles the runtime permissions.
		/// </summary>
		private bool verifyLocationPermissions() {
			if (AndroidController.hasGrantedLocationPermission()) {
				return true;

			} else if ((int)Build.VERSION.SdkInt < 23) {
				AndroidController.setHasGrantedLocationPermission(true);
				return true;

			} else {
				getLocationPermissionAsync();
				return false;
			}
		}

		void getLocationPermissionAsync() {
			if ((int)Build.VERSION.SdkInt >= 23) {
				const string permission = Manifest.Permission.AccessFineLocation;

				if (CheckSelfPermission(permission) == (int)Permission.Granted) {
					AndroidController.setHasGrantedLocationPermission(true);
					OnSaveInstanceState(getLatestAppState());

				} else if (CheckSelfPermission(permission) == Permission.Denied ||
						  ShouldShowRequestPermissionRationale(permission)) {
					showGiveLocationPermissionsDialog();

				} else {
					RequestPermissions(mPermissionsLocation, mRequestLocationId);
				}
			}
		}

		public void showGiveLocationPermissionsDialog() {
			if (mDialogLocationPermissions == null) {
				mDialogLocationPermissions = (new AlertDialog.Builder(this)).Create();
				mDialogLocationPermissions.SetTitle(Txt.TITLE_WELCOME);
				mDialogLocationPermissions.SetMessage(Txt.MSG_WE_NEED_PERMISSION_ACCESS_LOCATION);
				mDialogLocationPermissions.SetButton(Txt.LBL_BTN_OK, handleLocationPermissionResponse);
				mDialogLocationPermissions.SetButton2(Txt.LBL_NO_THANKS, handleLocationPermissionResponse);
				mDialogLocationPermissions.Show();
			}
		}

		public void handleLocationPermissionResponse(object sender, DialogClickEventArgs e) {
			var objAlertDialog = sender as AlertDialog;

			if (e != null && objAlertDialog != null) {
				var btnClicked = objAlertDialog.GetButton(e.Which);

				if (btnClicked.Text.Equals(Txt.LBL_BTN_OK)) {
					RequestPermissions(mPermissionsLocation, mRequestLocationId);

				} else if (btnClicked.Text.Equals(Txt.LBL_NO_THANKS)) {
					showCanRunOnlyWithPermissions();
				}
			} else {
				showCanRunOnlyWithPermissions();
			}

			mDialogLocationPermissions = null;
		}

		public void showCanRunOnlyWithPermissions() {
			if (mDialogLastTryLocationPermissions == null) {
				mDialogLastTryLocationPermissions = (new AlertDialog.Builder(this)).Create();
				mDialogLastTryLocationPermissions.SetTitle(Txt.TITLE_ATTENTION);
				mDialogLastTryLocationPermissions.SetMessage(Txt.MSG_MUST_GIVE_PERMISSION_TO_LOCATION);
				mDialogLastTryLocationPermissions.SetButton(Txt.LBL_BTN_OK, handleLastWarningAboutLocationPermissions);
				mDialogLastTryLocationPermissions.SetButton2(Txt.LBL_FINISH_APP, handleLastWarningAboutLocationPermissions);
				mDialogLastTryLocationPermissions.Show();
			}
		}

		public void handleLastWarningAboutLocationPermissions(object sender, DialogClickEventArgs e) {
			var objAlertDialog = sender as AlertDialog;

			if (e != null && objAlertDialog != null) {
				var btnClicked = objAlertDialog.GetButton(e.Which);

				if (btnClicked.Text.Equals(Txt.LBL_BTN_OK)) {
					RequestPermissions(mPermissionsLocation, mRequestLocationId);

				} else if (btnClicked.Text.Equals(Txt.LBL_FINISH_APP)) {
					AndroidUtils.finishApp();

				} else {
					AndroidUtils.finishApp();
				}
			} else {
				AndroidUtils.finishApp();
			}

			mDialogLastTryLocationPermissions = null;
		}

		public bool areLocationServiceDependenciesOk() {
			mLocationManager = (LocationManager)Forms.Context.GetSystemService(LocationService);

			if (!mLocationManager.IsProviderEnabled(LocationManager.GpsProvider)) {
				return false;
			} else {
				return true;
			}
		}

		public void showEnableGpsDialog() {
			var dlgAlert = (new AlertDialog.Builder(this)).Create();
			dlgAlert.SetTitle(Txt.MSG_TURN_ON_GPS_TITLE);
			dlgAlert.SetMessage(Txt.MSG_TURN_ON_GPS);
			dlgAlert.SetButton(Txt.MSG_BTN_OK_TURN_ON_GPS, handleGPSDialogResponse);
			dlgAlert.SetButton2(Txt.MSG_BTN_CANCEL_TURN_ON_GPS, handleGPSDialogResponse);
			dlgAlert.Show();
		}

		public void handleGPSDialogResponse(object sender, DialogClickEventArgs e) {
			var objAlertDialog = sender as AlertDialog;
			var btnClicked = objAlertDialog.GetButton(e.Which);

			if (btnClicked.Text.Equals(Txt.MSG_BTN_OK_TURN_ON_GPS)) {
				redirectUserToGPSSettings();
			} else {
				if (!areLocationServiceDependenciesOk()) {
					showEnableGpsDialog();
				}
			}
		}

		public void redirectUserToGPSSettings() {
			var gpsSettingIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
			((MainActivity)Forms.Context).StartActivityForResult(gpsSettingIntent, 1);
		}

		/// <summary>
		/// Activity Life cycle method
		/// 
		/// This method receives the response of the permission dialogs that were presented to user.
		/// </summary>
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
			switch (requestCode) {

				case mRequestLocationId: {

					if (grantResults[0] == Permission.Granted) {
						AndroidController.setHasGrantedLocationPermission(true);
						OnSaveInstanceState(getLatestAppState());
					} else {
						showGiveLocationPermissionsDialog();
					}
				}

				break;
			}
		}

		private void finishGeofencingService() {
			GeofencingServiceControllerDroid.stopService();
			geofencingServiceController = null;
		}
		#endregion
			

		#region Authentication Handling
		/// <summary>
		/// Implements the authentication method required by the AuthInterface in order to get the user access
		/// credentials from the 3# party authenticator.
		/// 
		/// #pragma CS1701:
		/// Azure Mobile services, rely on an old version of the Newtonsoft.Json framework, but I'm assuming the
		/// current version of the framework provides backward compatibility.
		/// </summary>
		public async Task<MobileServiceUser> authenticate(string authOpt) {
			try {

                if (AzureController.client != null) {
					MobileServiceUser user = null;

                    #pragma warning disable CS1701
					try {
                        
                        if (authOpt == Constants.AUTH_OPT_FACEBOOK)
                        {
                            user = await AzureController.client.LoginAsync(
                                            this,
                                            MobileServiceAuthenticationProvider.Facebook,
                                            "sportsconnection://easyauth.callback");

                        } else if (authOpt == Constants.AUTH_OPT_GOOGLE) {
                            user = await AzureController.client.LoginAsync(
                                            this,
                                            MobileServiceAuthenticationProvider.Google,
                                            "easyauth.callback");
                            
						} else if (authOpt == Constants.AUTH_OPT_TWITTER) {
                            user = await AzureController.client.LoginAsync(
                                            this,
                                            MobileServiceAuthenticationProvider.Twitter,
                                            "easyauth.callback");
						}

						if (user != null) {
							return user;
						}
					} catch (Exception e) {
						if (e != null) {
							DebugHelper.newMsg(Constants.TAG_AUTH_CONTROLLER_DROID, e.StackTrace);
						}
					}
					#pragma warning restore CS1701
				}
			} catch (Exception ex) {
				DebugHelper.newMsg(Constants.TAG_AUTH_CONTROLLER_DROID, ex.Message);
			}

			return null;
		}
		#endregion


		#region State Handling
		private Bundle getLatestAppState() {
			return mSavedState;
		}

		private void setLatestAppState(Bundle state) {
			mSavedState = state;
		}
		#endregion


		#region Activity Destructor
		/// <summary>
		/// Ons the destroy.
		/// 
		/// Todo: Remove this fix when the 'Activity has been destroyed' bugfix get oficially released by Xamarin
		/// </summary>
		protected override void OnDestroy() {
			try {
				//finishGeofencingService();
				base.OnDestroy();
			} catch (Exception e) {
				e.ToString();
			}
		}
		#endregion

	}

}