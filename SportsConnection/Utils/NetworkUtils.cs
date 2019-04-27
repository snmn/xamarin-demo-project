using System;
using Xamarin.Forms;

namespace SportsConnection {

	public class NetworkUtils {
		
		public static bool isOnline() {
			return DependencyService.Get<INetworkingUtils>().isAndroidPhoneOnline();
		}

	}

}