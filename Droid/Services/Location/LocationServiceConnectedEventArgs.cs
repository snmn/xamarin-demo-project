using System;
using Android.OS;

namespace SportsConnection.Droid {

	public class LocationServiceConnectedEventArgs : EventArgs {
		
		public IBinder Binder {
			get; set;
		}

	}

}