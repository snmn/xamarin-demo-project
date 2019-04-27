using System.Collections.Generic;

namespace SportsConnection {

	public class Geopoint {

		private Dictionary<string, double> mPosition = new Dictionary<string, double>();


		/// <summary>
		/// Initializes the params of the position dictionary
		/// </summary>
		public Geopoint() {
			position.Add(Constants.PARAM_LAT, 0.0);
			position.Add(Constants.PARAM_LNG, 0.0);
			position.Add(Constants.PARAM_ALT, 0.0);
		}

		// Getters and Setters
		//....................
		public Dictionary<string, double> position {
			get {
				return mPosition;
			}
			set {
				mPosition = value;
			}
		}

		public void setLatitude(double latitude) {
			this.position[Constants.PARAM_LAT] = latitude;
		}

		public double getLatitude() {
			return this.position[Constants.PARAM_LAT];
		}

		public void setLongitude(double longitude) {
			this.position[Constants.PARAM_LNG] = longitude;
		}

		public double getLongitude() {
			return this.position[Constants.PARAM_LNG];
		}

		public void setAltitude(double altitude) {
			this.position[Constants.PARAM_ALT] = altitude;
		}

		public double getAltitude() {
			return this.position[Constants.PARAM_ALT];
		}

	}

}