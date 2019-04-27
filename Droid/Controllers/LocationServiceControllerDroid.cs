using System;
using System.Threading.Tasks;

using Android.Content;
using Android.Locations;

using Xamarin.Forms;

namespace SportsConnection.Droid {
	
	public class LocationServiceControllerDroid {

		private static readonly string TAG = "LocationServiceControllerDroid";

		private Android.Locations.Location mCurrentLocation;
		public static LocationServiceConnection locationServiceConnection;
		public event EventHandler<LocationServiceConnectedEventArgs> locationServiceConnected = delegate {};


		/// <summary>
		/// This event fires when the ServiceConnection lets the client (our App class) know that
		/// the Service is connected. We use this event to start updating the UI with location
		/// updates from the Service.
		/// </summary>
		public void registerAndStart() {
			// Create a new service connection so we can get a binder to the service
			locationServiceConnection = new LocationServiceConnection(null);

			if (locationServiceConnection != null) {
				// Register an event to be fired when the service is connected
				// We will use this event to notify MainActivity when to start updating the UI
				locationServiceConnection.ServiceConnected += (object sender, LocationServiceConnectedEventArgs e) => {
					locationServiceConnected(this, e);
				};

				if (locationServiceConnected != null) {
					
					// Register the location service, it won't crash event if the user hasn't enabled GPS
					locationServiceConnected += (object sender, LocationServiceConnectedEventArgs e) => {

						if (locationService != null) {
							// Notifies us of location changes from the system
							locationService.LocationChanged += handleLocationChanged;

							// Notifies us of user changes to the location provider (ie the user disables or enables GPS)
							locationService.ProviderDisabled += handleProviderDisabled;
							locationService.ProviderEnabled += handleProviderEnabled;

							// Notifies us of the changing status of a provider (ie GPS no longer available)
							locationService.StatusChanged += handleStatusChanged;
						}
					};

					// Start the location service:
					startLocationService();
				}
			}
		}

		public static void startLocationService() {
			// Run the service on a background Thread
			new Task(() => {

				if (Android.App.Application.Context != null) {
					// The Intent tells the OS where to find our Service (the Context) and the Type of Service
					// we're looking for (LocationService)
					var locationServiceIntent = new Intent(Android.App.Application.Context, typeof(LocationService));
					locationServiceIntent.AddFlags(ActivityFlags.NewTask);

					// Start our main service
					Android.App.Application.Context.StartService(locationServiceIntent);

					// Finally, we can bind to the Service using our Intent and the ServiceConnection we
					// created in a previous step.
					Android.App.Application.Context.BindService(locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
				}

			}).Start();
		}

		public static void stopLocationService() {
			// Unbind from the LocationService; otherwise, StopSelf (below) will not work:
			if (locationServiceConnection != null && Android.App.Application.Context != null) {
				Android.App.Application.Context.UnbindService(locationServiceConnection);
			}

			// Stop the LocationService:
			if (locationService != null) {
				locationService.StopSelf();
			}
		}

		/// <summary>
		/// Handles changes on the user location.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public void handleLocationChanged(object sender, LocationChangedEventArgs e) {
			if (e.Location != null) {
				// Initialize user coordinates
				if (mCurrentLocation == null) {
					mCurrentLocation = e.Location;
					sendUserLocationListeners(mCurrentLocation);
				} 
				// Update the user coordinates only if the user has moved
				else if ((Math.Abs(mCurrentLocation.Latitude - e.Location.Latitude) > double.Epsilon) &&
				         (Math.Abs(mCurrentLocation.Longitude - e.Location.Longitude) > double.Epsilon)) {
					mCurrentLocation = e.Location;
					sendUserLocationListeners(mCurrentLocation);
				}
			}
		}

		/// <summary>
		/// Send a message object with details of the current user location to events which registered to receive
		/// instances of the 'UserLocationMessage' message object.
		/// </summary>
		/// <param name="location">Location.</param>
		/// 
		private void sendUserLocationListeners(Android.Locations.Location location) {
			var message = new UserLocationMessage {
				lat = location.Latitude,
				lng = location.Longitude,
				message = Txt.MSG_UPDATING_USER_LOCATION
			};

			if (location.Latitude.Equals(0.0) || location.Longitude.Equals(0.0)) {
				message.lat = SettingsController.getCurrentLatitude();
				message.lng = SettingsController.getCurrentLongitude();
			} else {
				SettingsController.setCurrentLatitude(location.Latitude);
				SettingsController.setCurrentLongitude(location.Longitude);
			}

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(message, UserLocationMessage.TAG);
			});
		}

		public void handleProviderDisabled(object sender, ProviderDisabledEventArgs e) {
			//..
		}

		public void handleProviderEnabled(object sender, ProviderEnabledEventArgs e) {
			//..
		}

		public void handleStatusChanged(object sender, StatusChangedEventArgs e) {
			//..
		}


		// Getters and Setters
		//....................
		public static LocationService locationService {
			get {
				if (locationServiceConnection.Binder != null) {

					if (locationServiceConnection.Binder.service != null) {
						return locationServiceConnection.Binder.service;
					}
				}

				DebugHelper.newMsg(TAG, "Failed to start location service");
				return null;
			}
		}

	}

}