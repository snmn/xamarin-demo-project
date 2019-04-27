using Xamarin.Forms;

namespace SportsConnection {

	public class NotificationController {

		public static int sNotificationCount;


		public NotificationController() {
			sNotificationCount = 0;
		}

		public static void createCheckedInNotification() {

			if (PlataformUtils.getPlataform() == PlataformUtils.PLATAFORM_ANDROID) {
				DependencyService.Get<INotification>().createGeofencingNotification(
					Constants.NOTIFICATION_TYPE_USER_CHECKED_IN,
					++sNotificationCount
				);
			}
		}

	}  

}