using System;

using Xamarin.Forms;

namespace SportsConnection {

	public class GeofencingController {

		public static bool hasInitialized = false;


		public void init() {
			if (!hasInitialized) {
				startBackgroundTask();

				hasInitialized = true;
			}
		}

		public void startBackgroundTask() {
			var message = new StartGeofencingTaskMessage();
			MessagingCenter.Send(message, StartGeofencingTaskMessage.TAG);
		}

		public void stopBackgroundTask() {
			var message = new GeofencingServiceStopped();
			MessagingCenter.Send(message, GeofencingServiceStopped.TAG);
		}

	}

}