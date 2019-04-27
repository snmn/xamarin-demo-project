using Android.Content;

namespace SportsConnection.Droid {
	
	public class GeofencingServiceControllerDroid {

		public void registerAndStart() {
			var intent = new Intent(Android.App.Application.Context, typeof(GeofencingService));
			intent.AddFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StartService(intent);
		}

		public static void stopService() {
			var intent = new Intent(Android.App.Application.Context, typeof(GeofencingService));
			intent.AddFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StopService(intent);
		}

	}

}