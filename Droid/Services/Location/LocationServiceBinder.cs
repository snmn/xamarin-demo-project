using Android.OS;

namespace SportsConnection.Droid {

	public class LocationServiceBinder : Binder {

		protected LocationService mService;


		public LocationServiceBinder(LocationService service) {
			this.service = service;
		}


		// Getters and Setters
		//....................
		public LocationService service {
			set {
				this.mService = value;
			}
			get {
				return this.mService;
			}
		}

		public bool isBound {
			get; set;
		}

	}

}