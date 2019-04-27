using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using SportsConnection;
using SportsConnection.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationControllerDroid))]
namespace SportsConnection.Droid {
	
	public class NotificationControllerDroid : INotification {

		/// <summary>
		/// Create one of the Geofencing related notifications 'CheckId' or 'CheckOut'
		/// </summary>
		/// <param name="notificationType">Notification type.</param>
		/// <param name="notificationId">Notification identifier.</param>
		public void createGeofencingNotification(int notificationType, int notificationId) {
			string notificationTitle = Txt.MSG_NOTIFICATION_CHECKED_IN;
			string notificationMsg = Txt.MSG_NOTIFICATION_CHECKED_IN;

			// Create an intent to show ui
			var uiIntent = new Intent(Forms.Context, typeof(MainActivity));
			uiIntent.PutExtra(Constants.PARAM_SELECTED_LOCATION_ID, GeofencingTask.getCheckedInLocationId());
			uiIntent.PutExtra(Constants.PARAM_LAST_SESSION_PID, Android.OS.Process.MyPid());
			uiIntent.AddFlags(ActivityFlags.SingleTop);

			// Use Notification Builder
			NotificationCompat.Builder builder = new NotificationCompat.Builder(Forms.Context);

			// Create the notification
			// we use the pending intent, passing our ui intent over which will get called
			// when the notification is tapped.
			var notification = builder.SetContentIntent(PendingIntent.GetActivity(Forms.Context, 0, uiIntent, 0))
					.SetSmallIcon(Resource.Drawable.logo_notification)
					.SetTicker(notificationTitle)
					.SetContentTitle(notificationTitle)
					.SetContentText(notificationMsg)

					// Set the notification sound
					.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))

					// Auto cancel will remove the notification once the user touches it
					.SetAutoCancel(true).Build();

			// Show the notification
			NotificationManager notificationManager = (NotificationManager)Forms.Context.GetSystemService(
				Context.NotificationService
			);

			notificationManager.Notify(1, notification);
		}

		/// <summary>
		/// Creates a simple notification with title and description for a general purposes messages, such as
		/// friendship requests.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="notificationManager">Notification manager.</param>
		/// <param name="title">Title.</param>
		/// <param name="desc">Desc.</param>
		public void createSimpleNotification(Context context, NotificationManager notificationManager, 
		                                         string title, string desc) {
			// Create an intent to show UI
			var uiIntent = new Intent(context, typeof(MainActivity));

			// Use Notification Builder
			NotificationCompat.Builder builder = new NotificationCompat.Builder(context);

			// Create the notification
			// we use the pending intent, passing our ui intent over which will get called
			// when the notification is tapped.
			var notification = builder.SetContentIntent(PendingIntent.GetActivity(context, 0, uiIntent, 0))
			                          .SetSmallIcon(Resource.Drawable.logo_notification)
									  .SetTicker(title)
									  .SetContentTitle(title)
									  .SetContentText(desc)

								 	  // Set the notification sound
									  .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))

									  // Auto cancel will remove the notification once the user touches it
									  .SetAutoCancel(true).Build();

			// Show the notification
			notificationManager.Notify(1, notification);
		}

	}

}