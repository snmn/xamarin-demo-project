using System;
using Xamarin.Forms;

namespace SportsConnection {
	
	public class PlataformUtils {

		public static readonly string PLATAFORM_ANDROID = "Android";
		public static readonly string PLATAFORM_IOS = "iOS";

		public static string sPlatform = "";


		public static void init() {
			#if MONOTOUCH
				sPlatform = PLATAFORM_IOS;
			#else
				sPlatform = PLATAFORM_ANDROID;
			#endif
		}

		public static string getPlataform() {
			return sPlatform;
		}

		public static int getAndroidVersion() {
			var androidUtils = DependencyService.Get<IAndroid>();
			return androidUtils.getAndroidSdkVersion();
		}

		public static void tryToReleaseMemory() {
			if (getPlataform() == PLATAFORM_ANDROID) {
				GC.Collect();
			}
		}

		public static bool androidHasInitialize() {
			var androidUtils = DependencyService.Get<IAndroid>();
			return androidUtils.androidHasInitialized();
		}

		public static bool androidHasGrantedLocationPermission() {
			var androidUtils = DependencyService.Get<IAndroid>();
			return androidUtils.androidHasGrantedPermissionToLocation();
		}

	}

}