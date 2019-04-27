using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
namespace SportsConnection.Droid {
	
	[BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new string[] { "@PACKAGE_NAME@" })]

	public class PushHandlerBroadcastReceiver : GcmBroadcastReceiverBase<GcmService> {
		public static string[] SENDER_IDS = { "912264309839" };
	}

	[Service]
	public class GcmService : GcmServiceBase {

		private string TAG = "GcmService";
		public static string registrationID { get; private set; }


		public GcmService() : base(PushHandlerBroadcastReceiver.SENDER_IDS) {
			Log.Debug(TAG, "Starging GCM service ...");
		}

		protected override void OnRegistered(Context context, string registrationId) {
			if (AzureController.client != null) {
				registrationID = registrationId;
				var push = AzureController.client.GetPush();
				Device.BeginInvokeOnMainThread(() => Register(push, null).ConfigureAwait(true));
			}
		}

		public async Task<bool> Register(Push push, IEnumerable<string> tags) {
			try {
				var templates = new JObject();

				templates[Constants.PARAM_KEY_GENERIC_MESSAGE] = new JObject{
					{Constants.PARAM_KEY_TEMPLATE_BODY, Constants.PARAM_VAL_TEMPLATE_GCM_BODY}
				};

				// Assuming assembly reference matches identity
				#pragma warning disable CS1701
				await push.RegisterAsync(registrationID, templates);
				#pragma warning restore CS1701

			} catch (Exception ex) {
				Log.Debug(TAG, tags.ToString());
				Log.Debug(TAG, ex.Message);
			}

			return true;
		}

		/// <summary>
		/// This method is called when a message from the push notification is received.
		/// The super class definition is done with a 'void' return type, thus the warning
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="intent">Intent.</param>
		#pragma warning disable RECS0165
		protected override async void OnMessage(Context context, Intent intent) {
			// Notification controller is the object used to display notifications to the UI
			var notificationController = new NotificationControllerDroid();
			var notificationManager = GetSystemService(NotificationService) as NotificationManager;
			var msg = new StringBuilder();

			// Build the message with the intent data
			if (intent != null && intent.Extras != null) {
				foreach (var key in intent.Extras.KeySet()) {
					msg.AppendLine(key + "=" + intent.Extras.Get(key));
				}
			}

			// Store the message
			var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
			var edit = prefs.Edit();
			edit.PutString(Constants.PARAM_KEY_LAST_MESSAGE, msg.ToString());
			edit.Commit();

			// Define the message depending on the notification content
			var title = Constants.APP_NAME;
			var message = intent.Extras.GetString(Constants.PARAM_KEY_MESSAGE);

			if (message != null) {
				message = await fetchNotificationContent(message);
			}

			// Create a new notification with the message
			if (!string.IsNullOrEmpty(message)) {
				notificationController.createSimpleNotification(context, notificationManager, title, message);
			}
		}
		#pragma warning restore RECS0165

		private async Task<string> fetchNotificationContent(string notificationContent) {
			var notifParams = notificationContent.Split(Constants.PUSH_NOTIFICATION_ATTRIBUTES_SEPARATOR);

			if (notifParams.Length > 0) {
				var notificationType = notifParams[0];
				var keyValuePairs = new Dictionary<string, string>();

				// Extract parameters from the notification
				if (notificationType != null) {

					// Extract the key value pairs from the push notification
					for (int i = 1; i < notifParams.Length; i++) {
						var keyValuePair = notifParams[i].Split(Constants.PUSH_NOTIFICATION_KEY_VALUE_SEPARATOR);

						if ((keyValuePair[0] != null) && (keyValuePair[1] != null)) {
							keyValuePairs.Add(keyValuePair[0], keyValuePair[1]);
						}
					}

					// Friendship notification
					if (notificationType == Constants.PUSH_NOTIFICATION_TYPE_FRIENDSHIP) {
						return await fetchFriendshipNotificationMsg(keyValuePairs);
					}

					// Checkin notification
					if (notificationType == Constants.PUSH_NOTIFICATION_TYPE_CHECKIN) {
						return fetchCheckinNotificationMsg(keyValuePairs);
					}

					// Checkin notification
					if (notificationType == Constants.PUSH_NOTIFICATION_TYPE_WALL_MESSAGE) {
						return broadcastWallMessageNotification(keyValuePairs);
					}
				}
			}

			return "";
		}

		private async Task<string> fetchFriendshipNotificationMsg(Dictionary<string, string> keyValuePairs) {
			var userManager = new UserManager();
			string userIdA = keyValuePairs[Constants.FRIENDSHIP_NOTIFICATION_PARAM_USERID_A];
			string userIdB = keyValuePairs[Constants.FRIENDSHIP_NOTIFICATION_PARAM_USERID_B];
			string statusA = keyValuePairs[Constants.FRIENDSHIP_NOTIFICATION_PARAM_STATUS_A];
			string statusB = keyValuePairs[Constants.FRIENDSHIP_NOTIFICATION_PARAM_STATUS_B];
			string msg = "";

			// Verify if the current user is involved in the notification
			if (userIdA != null && userIdB != null && statusA != null && statusB != null) {

				// Check if current user is A or B
				if (App.authController.getCurrentUser() != null) {

					if (App.authController.getCurrentUser().id != null) {

						// Current user is user A
						if (userIdA == App.authController.getCurrentUser().id) {
							User user = await userManager.getUserById(userIdB);

							if (user != null) {
								if (statusB == Constants.FRIENDS_STR) {
									msg += user.name + Txt.MSG_NOTIFICATION_BODY_POSTFIX_FRIENDSHIP;
								}
							}
						}
						// Current user is user B
						else if (userIdB == App.authController.getCurrentUser().id) {
							User user = await userManager.getUserById(userIdA);

							if (user != null) {
								if (statusA == Constants.FRIENDS_STR) {
									msg += user.name + Txt.MSG_NOTIFICATION_BODY_POSTFIX_FRIENDSHIP;
								}
							}
						}
					}
				}

				var message = new FriendshipMessage();
				message.txt = msg;
				Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(message, FriendshipMessage.TAG));

				return msg;
			}

			return "";
		}

		private string fetchCheckinNotificationMsg(Dictionary<string, string> keyValuePairs) {
			string msg = "";
			string id = keyValuePairs[Constants.CHECKIN_NOTIFIVATION_PARAM_ID];
			string userId = keyValuePairs[Constants.CHECKIN_NOTIFIVATION_PARAM_USER_ID];
			string locationId = keyValuePairs[Constants.CHECKIN_NOTIFIVATION_PARAM_LOCATION_ID];

			// Verify if the current user is involved in the notification
			if (userId != null && locationId != null) {

				// Check if current user is A or B
				if (App.authController.getCurrentUser() != null) {

					if (App.authController.getCurrentUser().uid != null) {

						// Current user is user A
						if (userId == App.authController.getCurrentUser().uid) {
							msg += Txt.MSG_NOTIFICATION_CHECKED_IN;
						}
					}
				} 
			}

			return msg;
		}

		private string broadcastWallMessageNotification(Dictionary<string, string> keyValuePairs) {
			string userId = keyValuePairs[Constants.CHECKIN_NOTIFIVATION_PARAM_USER_ID];
			string locationId = keyValuePairs[Constants.CHECKIN_NOTIFIVATION_PARAM_LOCATION_ID];

			// Verify if the current user is involved in the notification
			if (userId != null && locationId != null) {

				// Send broadcast notification for registered listeners
				var message = new WallMessage();
				message.locationId = locationId;
				message.userId = userId;
				Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(message, WallMessage.TAG));
			}

			return "";	
		}

		protected override void OnUnRegistered(Context context, string registrationId) {
			Log.Error(TAG, "Unregistered RegisterationId : " + registrationId);
		}

		protected override void OnError(Context context, string errorId) {
			Log.Error(TAG, "GCM Error: " + errorId);
		}

	}

}