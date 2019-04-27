using Android.Net;
using SportsConnection.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(NetworkingUtilsDroid))]
namespace SportsConnection.Droid {
	
	public class NetworkingUtilsDroid : INetworkingUtils {
		
		public bool isAndroidPhoneOnline() {
			
			if (AndroidApplication.sConnectivityManager != null) {
				NetworkInfo activeConnection = AndroidApplication.sConnectivityManager.ActiveNetworkInfo;
				return (activeConnection != null) && activeConnection.IsConnected;
			}

			return false;
		}

	}

}