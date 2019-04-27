using Xamarin.Forms;

using SportsConnection.Droid;
using Android.OS;

[assembly: Dependency(typeof(AndroidController))]
namespace SportsConnection.Droid {

	public class AndroidController : IAndroid {

		private static bool mHasGrantedLocationPermission = false;
		private static bool mHasInitialized = false;


		/// <summary>
		/// IAndroid interface implementation
		/// </summary>
		public void finishApp() {
			Process.KillProcess(Process.MyPid());
		}

		public int getAndroidSdkVersion() {
			return (int)Build.VERSION.SdkInt;
		}

		public bool androidHasInitialized() {
			return hasInitialized();	
		}

		public bool androidHasGrantedPermissionToLocation() {
			return hasGrantedLocationPermission();
		}

		/// <summary>
		/// Android plataform implementation
		/// </summary>
		public static bool hasGrantedLocationPermission() {
			return mHasGrantedLocationPermission;
		}

		public static void setHasGrantedLocationPermission(bool status) {
			mHasGrantedLocationPermission = status;
		}

		public static bool hasInitialized() {
			return mHasInitialized;
		}

		public static void setHasInitialized(bool status) {
			mHasInitialized = status;

			if (mHasInitialized) {
				var message = new SystemHasInitializedMessage {
					status = true
				};

				Device.BeginInvokeOnMainThread(() => {
					MessagingCenter.Send(message, SystemHasInitializedMessage.TAG);
				});
			}
		}

	}

}