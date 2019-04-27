using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace SportsConnection {
	
	public class MapsController {

		public CustomMap map;
		public Geopoint currentLocation = new Geopoint();

		private static bool mIsShowingSingleLocation;
		private static bool mIsClickable;


		public MapsController() {
			initializeMap();
			initNearbyLocationsListener();
			initUserLocationListener();
			initNearbyLocationsFromCache();

			navigateToCurrentLocation();
			refreshMap();
		}

		/// <summary>
		/// Defines global map's preferences. 
		/// </summary>
		public void initializeMap() {
			map = new CustomMap {
				MapType = MapType.Street,
				WidthRequest = SettingsController.getPhoneWidth(),
				HeightRequest = SettingsController.getPhoneHeight()
			};

			map.IsShowingUser = true;
			map.customPins = new List<CustomPin>();
		}

		/// <summary>
		/// Starts listening to the Geofencing Service and get an updated list of nearby locations.
		/// </summary>
		private void initNearbyLocationsListener() {
			MessagingCenter.Subscribe<NearbyLocationsMessage>(this, NearbyLocationsMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {
					if (message != null) {
						updateNearbyLocationPins();	
						refreshPins();
					}
				});
			});
		}

		/// <summary>
		/// Starts listening to the Location Service and get the current user location.
		/// </summary>
		private void initUserLocationListener() {
			currentLocation.setLatitude(SettingsController.getCurrentLatitude());
			currentLocation.setLongitude(SettingsController.getCurrentLongitude());
				
			MessagingCenter.Subscribe<UserLocationMessage>(this, UserLocationMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {
					currentLocation.setAltitude(message.altitude);
					currentLocation.setLatitude(message.lat);
					currentLocation.setLongitude(message.lng);

					SettingsController.setCurrentLatitude(message.lat);
					SettingsController.setCurrentLongitude(message.lng);

					navigateToCurrentLocation();
				});
			});
		}	

		private void initNearbyLocationsFromCache() {
			var message = new UserLocationMessage {
				lat = SettingsController.getCurrentLatitude(),
				lng = SettingsController.getCurrentLongitude(),
				message = Txt.MSG_UPDATING_USER_LOCATION
			};

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(message, UserLocationMessage.TAG);
			});
		}

		/// <summary>
		/// Move the map's camera to the current user coordinate.
		/// </summary>
		public void navigateToCurrentLocation() {
			if (map != null) {
				map.MoveToRegion(MapSpan.FromCenterAndRadius(
						new Position(
                            SettingsController.getCurrentLatitude(),
                            SettingsController.getCurrentLongitude()
                        ),
						Distance.FromMiles(1.0))
				);
			}
		}

		/// <summary>
		/// Move the map's camera to the current user coordinate.
		/// </summary>
		public void navigateToCurrentLocation(double lat, double lng, double zoomLevel) {
			if (map != null) {
				map.MoveToRegion(new MapSpan(new Position(lat, lng), zoomLevel, zoomLevel));
			}
		}

		/// <summary>
		/// Draw the Pins of the nearby Locations
		/// </summary>
		public void updateNearbyLocationPins() {
			if (map != null && GeofencingTask.sLocationsNearby != null && !isShowingSingleLocation()) {
				// Remove all pins
				map.customPins.Clear();
				map.Pins.Clear();

				// Update the list of pins being displayed
				foreach (Location location in GeofencingTask.sLocationsNearby) {
					addLocationPin(location);
				}
			}
		}

		/// <summary>
		/// Insert a Pin at the location defined by the current user's latitude and longitude.
		/// This pin is supossed to be used only as a UI element.
		/// </summary>
		public void addPlaceholderPin() {
			if (currentLocation != null) {
				var pin = new CustomPin {
					pin = new Pin {
						Type = PinType.Place,
						Position = new Position(currentLocation.getLatitude(), currentLocation.getLongitude()),
						Label = ""
					},
					id = Constants.PARAM_PLACEHOLDER_PIN_ID
				};

				addPin(pin);
			}
		}

		public void removePlaceholderPin() {
			if (map != null) {
				CustomPin pinToRemove = null;

				foreach (CustomPin cp in map.customPins) {
					if (cp.id == Constants.PARAM_PLACEHOLDER_PIN_ID) {
						pinToRemove = cp;
					}
				}

				if (pinToRemove != null) {
					map.customPins.Remove(pinToRemove);
				}
			}
		}

		/// <summary>
		/// Get the information of a Location, create and insert a new CustomPin into the map
		/// </summary>
		/// <param name="loc"></param>
		public void addLocationPin(Location loc) {
			if (loc != null) {
				var lat = FormatUtils.stringToDouble(loc.localLatitude);
				var lng = FormatUtils.stringToDouble(loc.localLongitude);

				var pin = new CustomPin {
					pin = new Pin {
						Type = PinType.Place,
						Position = new Position(
							lat,
							lng
						),
						Label = loc.name
					},
					id = loc.id
				};

				addPin(pin);
			}
		}

		/// <summary>
		/// Update the list of Custom and Native pins
		/// </summary>
		/// <param name="pin">A CustomPin object</param>
		private void addPin(CustomPin pin) {
			if (map != null) {
				map.customPins.Add(pin);
			}

			refreshPins();
		}

		/// <summary>
		/// Remove a Pin from the map
		/// </summary>
		/// <param name="loc">A Location object</param>
		public void removeLocationPin(Location loc) {
			if (map != null && loc != null) {
				CustomPin pinToRemove = null;

				foreach (CustomPin cp in map.customPins) {
					if (cp.pin.Label == loc.name) {
						pinToRemove = cp;
					}
				}

				if (pinToRemove != null) {
					map.customPins.Remove(pinToRemove);
				}
			}
		}

		public void clearMap() {
			map.customPins.Clear();
		}

		public void refreshMap() {
			updateNearbyLocationPins();
			GeofencingTask.setStatusMustUpdate(true);
		}

		public void refreshPins() {
			var messageUpdatedPins = new UpdatePinsMessage();

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(messageUpdatedPins, UpdatePinsMessage.TAG);
			});
		}

		public void drawPathToLocation(string encodedPolyline) {
			var messageDrawPolyline = new DrawPolylineMessage();
			messageDrawPolyline.encodedPolyline = encodedPolyline;

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(messageDrawPolyline, DrawPolylineMessage.TAG);
			});
		}

		public void updateZoomLevel(int zoomLevel) {
			var messageUpdateZoomLevel = new ChangeZoomLevelMessage();
			messageUpdateZoomLevel.zoomLevel = zoomLevel;

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(messageUpdateZoomLevel, ChangeZoomLevelMessage.TAG);
			});
		}

		/// <summary>
		/// State variables
		/// </summary>
		public void setIsShowingSingleLocation(bool showingSingle) {
			mIsShowingSingleLocation = showingSingle;
		}

		public bool isShowingSingleLocation() {
			return mIsShowingSingleLocation;
		}

		public void setIsClickable(bool isClickable) {
			mIsClickable = isClickable;
		}

		public static bool isClickEnabled() {
			return mIsClickable;
		}

	}

}