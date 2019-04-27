using System;

namespace SportsConnection {

	public interface ILocation {
		void ObtainMyLocation();
		event EventHandler<LocationInterfaceEventArgs> locationObtained;
	}

	public interface LocationInterfaceEventArgs {
		double lat {
			get; set;
		}
		double lng {
			get; set;
		}
	}

}