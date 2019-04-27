using System;

using Gcm.Client;

namespace SportsConnection.Droid {
	
	public class PushServiceControllerDroid {

		private static string TAG = "NotificationsController";


		/// <summary>
		/// Register the Push Notifications Service
		/// </summary>
		public void register() {
			try {
				// Ensure that everything was setup correctly
				GcmClient.CheckDevice(Android.App.Application.Context);
				GcmClient.CheckManifest(Android.App.Application.Context);

				// Register listener for push notifications
				GcmClient.Register(Android.App.Application.Context, PushHandlerBroadcastReceiver.SENDER_IDS);

			} catch (Java.Net.MalformedURLException) {
				DebugHelper.newMsg(TAG, "There was a problem loading your push notifications.");

			} catch (Exception e) {
				DebugHelper.newMsg(TAG, e.Message);
			}
		}

	}

}