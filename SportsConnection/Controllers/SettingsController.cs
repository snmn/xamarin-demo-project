using Xamarin.Forms;

namespace SportsConnection {

	public static class SettingsController {

		private static float mPhoneWidth = 0.0f;
		private static float mPhoneHeight = 0.0f;


		public static void setAllowPublishLocation(bool allowStatus) {
			Application.Current.Properties[Constants.PROP_USER_PUBLIC_LOCATION] = allowStatus;
		}

		public static bool getAllowPublishLocation() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_USER_PUBLIC_LOCATION)) {
				return (bool)Application.Current.Properties[Constants.PROP_USER_PUBLIC_LOCATION];
			}

			return true;
		}

		public static void setIsFirtTime(bool isFirstTime) {
			Application.Current.Properties[Constants.PROP_IS_FIRST_TIME] = isFirstTime;
		}

		public static bool isFirstTime() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_IS_FIRST_TIME)) {
				return (bool)Application.Current.Properties[Constants.PROP_IS_FIRST_TIME];
			}

			return true;
		}

		public static void setPhoneWidth(float width) {
			mPhoneWidth = width;
		}

		public static float getPhoneWidth() {
			return mPhoneWidth;
		}

		public static void setPhoneHeight(float height) {
			mPhoneHeight = height;
		}

		public static float getPhoneHeight() {
			return mPhoneHeight;
		}

		public static void setCurrentLatitude(double lat) {
			Application.Current.Properties[Constants.PARAM_LATITUDE] = lat;
		}

		public static double getCurrentLatitude() {
			if (Application.Current.Properties.ContainsKey(Constants.PARAM_LATITUDE)) {
				return (double) Application.Current.Properties[Constants.PARAM_LATITUDE];
			}

			return 0.0;
		}

		public static void setCurrentLongitude(double lng) {
			Application.Current.Properties[Constants.PARAM_LONGITUDE] = lng;
		}

		public static double getCurrentLongitude() {
			if (Application.Current.Properties.ContainsKey(Constants.PARAM_LONGITUDE)) {
				return (double)Application.Current.Properties[Constants.PARAM_LONGITUDE];
			}

			return 0.0;
		}

	}

}