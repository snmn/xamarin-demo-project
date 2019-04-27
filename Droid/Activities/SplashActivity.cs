using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

using Android.Content.PM;
using Android.Gms.Common;

namespace SportsConnection.Droid {

	[Activity(
		Label = "SportsConnection",
		Theme = "@style/MyTheme.Splash",
		MainLauncher = true,
		NoHistory = true,
		LaunchMode = LaunchMode.SingleTop
	)]
	public class SplashActivity : AppCompatActivity {

		public static readonly int ID_INSTALL_GOOGLE_PLAY_SERVCIES_INTENT = 1000;
		private bool isGooglePlayServicesInstalled;


		public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState) {
			base.OnCreate(savedInstanceState, persistentState);
		}

		protected override void OnResume() {
			base.OnResume();

			if (verifyGooglePlayServicesIsInstalled()) {
				initApp();
			}
		}

		private bool verifyGooglePlayServicesIsInstalled() {
			int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);

			if (queryResult == ConnectionResult.Success) {
				return true;
			}

			if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult)) {
				Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, ID_INSTALL_GOOGLE_PLAY_SERVCIES_INTENT);
				errorDialog.SetTitle(Txt.MSG_INSTALL_GOOGLE_PLAY_SERVICES);
				errorDialog.Show();
			}

			return false;
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			switch (resultCode) {
			case Result.Ok:
			mIsGooglePlayServicesInstalled = true;
			OnResume();
			break;
			}
		}

		private void initApp() {
			StartActivity(new Intent(Application.Context, typeof(MainActivity)));
		}


		#region Getters and Setters
		public bool mIsGooglePlayServicesInstalled {
			get {
				return isGooglePlayServicesInstalled;
			}

			set {
				isGooglePlayServicesInstalled = value;
			}
		}
		#endregion

	}

}