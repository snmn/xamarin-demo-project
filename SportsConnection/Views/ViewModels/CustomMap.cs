using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace SportsConnection {

	public class CustomMap : Map {

		public List<CustomPin> customPins { get; set; }


		public void openSelectedLocactionDetails(string locationId) {
			Locations.goToSelectedLocationDetails(locationId).ConfigureAwait(false);
		}

	}

}