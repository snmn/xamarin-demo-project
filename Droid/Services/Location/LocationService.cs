using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Locations;

namespace SportsConnection.Droid {
	
	[Service]
	public class LocationService : Service, ILocationListener {
		
		public event EventHandler<LocationChangedEventArgs> LocationChanged = delegate { };
		public event EventHandler<ProviderDisabledEventArgs> ProviderDisabled = delegate { };
		public event EventHandler<ProviderEnabledEventArgs> ProviderEnabled = delegate { };
		public event EventHandler<StatusChangedEventArgs> StatusChanged = delegate { };

		// Set our location manager as the system location service
		protected LocationManager locationManager = Application.Context
		                                              .GetSystemService(Constants.ANDROID_SYSTEM_SERVICE_LOCATION) 
		                                              as LocationManager;
		public IBinder binder;


		public override void OnCreate() {
			base.OnCreate();
		}

		/// <summary>
		/// This gets called when StartService is called in our App class
		/// </summary>
		/// <returns>The start command.</returns>
		/// <param name="intent">Intent.</param>
		/// <param name="flags">Flags.</param>
		/// <param name="startId">Start identifier.</param>
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
			return StartCommandResult.Sticky;
		}

		/// <summary>
		/// This gets called once, the first time any client bind to the Service and returns an instance of the 
		/// LocationServiceBinder. All future clients will reuse the same instance of the binder
		/// </summary>
		/// <returns>The binder object.</returns>
		/// <param name="intent">Intent.</param>
		public override IBinder OnBind(Intent intent) {
			binder = new LocationServiceBinder(this);
			return binder;
		}

		/// <summary>
		/// Handle location updates from the location manager
		/// </summary>
		public void StartLocationUpdates() {
			//We can set different location criteria based on requirements for our app -
			//for example, we might want to preserve power, or get extreme accuracy
			var locationCriteria = new Criteria();

			locationCriteria.Accuracy = Accuracy.NoRequirement;
			locationCriteria.PowerRequirement = Power.NoRequirement;

			// Get provider: GPS, Network, etc.
			var locationProvider = locationManager.GetBestProvider(locationCriteria, true);

			// Get an initial fix on location
			locationManager.RequestLocationUpdates(locationProvider, 2000, 0, this);

            Android.Locations.Location loc = locationManager.GetLastKnownLocation(LocationManager.NetworkProvider);

            if (loc != null) {
				SettingsController.setCurrentLatitude(loc.Latitude);
				SettingsController.setCurrentLongitude(loc.Longitude);            
            }
		}

		/// <summary>
		/// Stop and destroy existing references made by the service.
		/// </summary>
		public override void OnDestroy() {
			base.OnDestroy();
			locationManager.RemoveUpdates(this);
		}

		public void OnLocationChanged(Android.Locations.Location location) {
            SettingsController.setCurrentLatitude(location.Latitude);
            SettingsController.setCurrentLongitude(location.Longitude);

			this.LocationChanged(this, new LocationChangedEventArgs(location));
		}

		public void OnProviderDisabled(string provider) {
			this.ProviderDisabled(this, new ProviderDisabledEventArgs(provider));
		}

		public void OnProviderEnabled(string provider) {
			this.ProviderEnabled(this, new ProviderEnabledEventArgs(provider));
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {
			this.StatusChanged(this, new StatusChangedEventArgs(provider, status, extras));
		}

	}

}