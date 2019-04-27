using System;

namespace SportsConnection {

	public class UserLocationMessage {

		public static string TAG = "UserLocationMessage";


		public string message {
			get; set;
		}
		public double altitude {
			get; set;
		}
		public double lat {
			get; set;
		}
		public double lng {
			get; set;
		}

	}

}