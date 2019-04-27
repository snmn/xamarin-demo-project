using System.Collections.Generic;

namespace SportsConnection {
	
	public class NearbyLocationsMessage {
		
		public static string TAG = "NearbyLocationsMessage";


		public string message {
			get; set;
		}

		public List<Location> nearbyLocations {
			get; set;
		}

	}

}