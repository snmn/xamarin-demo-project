using System;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;

using Xamarin.Forms;

namespace SportsConnection.Droid {

	[Application]
	public class AndroidApplication : Android.App.Application {

		public static ConnectivityManager sConnectivityManager;


		public AndroidApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer) {
			DebugHelper.newMsg(Constants.TAG_DROID_APP, "Starting the Android application...");
		}

		public override void OnCreate() {
			base.OnCreate();
			sConnectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
		}

	}

}