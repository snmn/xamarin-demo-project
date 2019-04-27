using System.Threading.Tasks;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms;

namespace SportsConnection.Droid {
	[Service]
	public class GeofencingService : Service {

		public string TAG = "GeofencingService";
		public CancellationTokenSource cts;


		public override IBinder OnBind(Intent intent) {
			return null;
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
			cts = new CancellationTokenSource();

			Task.Run(() => {
				try {
					// Execute the Geofencing task from the PCL project
					var geofencingTask = new GeofencingTask();
					geofencingTask.RunTask(cts.Token).Wait();

				} catch (OperationCanceledException e) {
					e.PrintStackTrace();
					informListenersAboutServiceStatus();

				} finally {
					if (cts.IsCancellationRequested) {
						informListenersAboutServiceStatus();
					}
				}
			}, cts.Token);

			return StartCommandResult.Sticky;
		}

		public override bool StopService(Intent name) {
			informListenersAboutServiceStatus();
			return base.StopService(name);
		}

		public override void OnDestroy() {
			if (cts != null) {
				cts.Token.ThrowIfCancellationRequested();
				cts.Cancel();
			}

			informListenersAboutServiceStatus();
			base.OnDestroy();
		}

		private void informListenersAboutServiceStatus() {
			var message = new GeofencingServiceStopped();
			Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(message, GeofencingServiceStopped.TAG));	
		}

	}

}