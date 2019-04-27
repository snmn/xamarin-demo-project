using Android.App;
using Android.Content;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

using Xamarin.Forms;

using SportsConnection.Droid.Services;


[assembly: Dependency(typeof(AndroidMethods))]	
namespace SportsConnection.Droid.Services {

	public class AndroidMethods : AndroidMethodsInterface {

		public static string checkedInLocationId = "";


		public void CreateNotification(int notificationType, int notificationId) {
			string notificationTitle = "";
			string notificationMsg = "";
			NotificationCompat.Builder builder = new NotificationCompat.Builder(Forms.Context);

			// Checked In Notification
			if (notificationType == NOTIFICATION_TYPE_USER_CHECKED_IN) {
				notificationTitle = Txt.NOTIFICATION_TITLE_CHECKED_IN;
				notificationMsg = Txt.NOTIFICATION_TITLE_CHECKED_IN;

				// Create an Intent to open the main activity, if there is an existing activity it must be killed and 
				// for the new activity to take place
				Intent resultIntent = new Intent(Forms.Context, typeof(MainActivity));
				resultIntent.PutExtra(AndroidMethods.PARAM_SELECTED_LOCATION_ID, App.checkedInLocationID);
				resultIntent.PutExtra(AndroidMethods.PARAM_LAST_SESSION_PID, Android.OS.Process.MyPid());
				resultIntent.AddFlags(ActivityFlags.SingleTop);

				// Construct a back stack for cross-task navigation
				TaskStackBuilder stackBuilder = TaskStackBuilder.Create(Forms.Context);
				stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
				stackBuilder.AddNextIntent(resultIntent);

				// Create the PendingIntent with the back stack          
				PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

				builder.SetContentTitle(notificationTitle)     // Notification title
					.SetContentText(notificationMsg)            // Notification message
					.SetSmallIcon(Resource.Drawable.ico_alert)  // Notification icon
					.SetAutoCancel(true)                       // Dismiss from the notif. area when clicked
					.SetContentIntent(resultPendingIntent)     // Start 2nd activity when the intent is clicked.
					.SetNumber(1);                             // Display the count in the Content Info

				// Checked Out Notification
			} else if (notificationType == NOTIFICATION_TYPE_USER_CHECKED_OUT) {
				notificationTitle = Txt.NOTIFICATION_TITLE_CHECKED_OUT;
				notificationMsg = Txt.NOTIFICATION_BODY_CHECKED_OUT;

				builder.SetContentTitle(notificationTitle)     // Notification title
					.SetContentText(notificationMsg)            // Notification message
					.SetSmallIcon(Resource.Drawable.ico_alert)  // Notification icon
					.SetAutoCancel(true)                       // Dismiss from the notif. area when clicked
					.SetNumber(1);                             // Display the count in the Content Info

				// Welcome Back Notification
			} else if (notificationType == NOTIFICATION_TYPE_WELCOME_BACK) {
				notificationTitle = Txt.NOTIFICATION_TITLE_WELCOME_BACK;
				notificationMsg = Txt.NOTIFICATION_BODY_WELCOME_BACK;

				builder.SetContentTitle(notificationTitle)     // Notification title
					.SetContentText(notificationMsg)            // Notification message
					.SetSmallIcon(Resource.Drawable.ico_alert)  // Notification icon
					.SetAutoCancel(true)                       // Dismiss from the notif. area when clicked
					.SetNumber(1);                             // Display the count in the Content Info
			}

			NotificationManager notificationManager = (NotificationManager)Forms.Context.GetSystemService(
				Context.NotificationService
			);
			notificationManager.Notify(notificationId, builder.Build());
		}

		public void SetCheckedInLocationId(string checkedInLocationId) {
			AndroidMethods.checkedInLocationId = checkedInLocationId;
		}

		public string GetCheckedInLocationId() {
			return AndroidMethods.checkedInLocationId;
		}

		public void KillOldAppInstance(int lastSessionId) {
			Android.OS.Process.KillProcess(lastSessionId);
		}

		public void CloseApp() {
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
		}

	}

}