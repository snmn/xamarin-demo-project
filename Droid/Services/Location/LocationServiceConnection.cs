using System;

using Android.Content;
using Android.OS;

namespace SportsConnection.Droid {

	public class LocationServiceConnection : Java.Lang.Object, IServiceConnection {

		protected LocationServiceBinder binder;
		public event EventHandler<LocationServiceConnectedEventArgs> ServiceConnected = delegate {};
		public LocationServiceBinder Binder { get { return this.binder; } set { this.binder = value; } }


		public LocationServiceConnection(LocationServiceBinder binder) {
			if (binder != null) {
				this.binder = binder;
			}
		}

		/// <summary>
		/// This gets called when a client tries to bind to the Service with an Intent and an instance of the 
		/// ServiceConnection. The system will locate a binder associated with the running Service 
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="service">Service.</param>
		public void OnServiceConnected(ComponentName name, IBinder service) {
			// Cast the binder located by the OS as our local binder subclass
			LocationServiceBinder serviceBinder = service as LocationServiceBinder;

			if (serviceBinder != null) {
				this.binder = serviceBinder;
				this.binder.isBound = true;

				// raise the service connected event
				this.ServiceConnected(this, new LocationServiceConnectedEventArgs() { Binder = service });

				// now that the Service is bound, we can start gathering some location data
				serviceBinder.service.StartLocationUpdates();
			}
		}

		/// <summary>
		/// This will be called when the Service unbinds, or when the app crashes.
		/// </summary>
		/// <param name="name">Name.</param>
		public void OnServiceDisconnected(ComponentName name) {
			this.binder.isBound = false;
		}

	}

}