using System.Collections.Generic;

namespace SportsConnection {

	/// <summary>
	/// Defines part of a path from a point A to a point B. It contains a list of detailed steps of the direction and
	/// meta information about the path.
	/// </summary>
	public class DirectionLeg {

		public string distanceTxt {
			get; set;
		}

		public int distanceVal {
			get; set;
		}

		public string durationTxt {
			get; set;
		}

		public int durationVal {
			get; set;
		}

		public string startAddress {
			get; set;
		}

		public string endAddress {
			get; set;
		}

		public double startLocationLat {
			get; set;
		}

		public double startLocationLng {
			get; set;
		}

		public double endLocationLat {
			get; set;
		}

		public double endLocationLng {
			get; set;
		}

		public List<DirectionStep> steps {
			get; set;
		}

	}

}