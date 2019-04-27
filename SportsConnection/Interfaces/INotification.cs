using System;

namespace SportsConnection {
	
	public interface INotification {

		void createGeofencingNotification(int notificationType, int notificationId);

	}

}