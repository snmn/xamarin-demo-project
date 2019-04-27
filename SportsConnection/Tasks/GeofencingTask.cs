using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices;

namespace SportsConnection {
	
	public class GeofencingTask {

		public static MobileServiceClient client;

		private UserController mUserController = new UserController();
		private LocationController mLocationController = new LocationController();

		private static User sCurrentUser;
		private static string sCheckedInLocationId;

		private Geopoint mCurrentLocation;
		public static List<Location> sLocationsNearby;
		private List<string> mCheckedInLocationsIds;

		public static bool sMustUpdate;
		private bool mUserCoordinatesChanged;
		private bool mTaskHasFinishedIteration;


		public GeofencingTask() {
			setUserCoordinates(null);
			sLocationsNearby = new List<Location>();
			mCheckedInLocationsIds = new List<string>();

			setStatusMustUpdate(true);
			setStatusUserCoordinatesChanged(false);
			setStatusTaskHasCompletedIteration(true);
		}

		public async Task RunTask(CancellationToken token) {
			// This method is supposed to run as an infinite loop
			#pragma warning disable RECS0135

			await Task.Run(async () => {
				// Stops this task if the user cancels it
				token.ThrowIfCancellationRequested();

				// Start the listener that receives the current user location
				initializeLocationManagerListener();

				while (true) {
					// Give a slower pace to this infinite loop
					await Task.Delay(Constants.TIMEOUT_GEOLOC_UPDATE);
					DebugHelper.newMsg(Constants.TAG_GEOFENCING_TASK, "Executing");

					// Update the geo location of the current user on the server and look for check-ins/check-outs
					if (taskHasCompletedInteration() && userCoordinatesHaveChanged() && getCurrentUser() != null && getUserCoordinates() != null
					    || mustUpdate()) {
						
						setStatusTaskHasCompletedIteration(false);

						// Perform tasks related to the server
						await refreshCurrentCheckinsList();
						await updateCurrentUserLocation();
						await verifyPossibleCheckinsCheckouts();

						// Unlock the execution of the next iteration of the task
						setStatusTaskHasCompletedIteration(true);
						setStatusUserCoordinatesChanged(false);

						DebugHelper.newMsg(Constants.TAG_GEOFENCING_TASK, "Finished iteration");
					}
				}
			}, token);

			// Function does not reach its end or a 'return' statement by any of possible execution paths
			#pragma warning restore RECS0135
		}

		/// <summary>
		/// Initialize the variable which carries the Check-In-Out state of the user and the nearby locations,
		/// this is necessary to guarantee that the checkout behavior is going to work across multiple 
		/// sessions.
		/// </summary>
		/// <returns>True, if the current list of checkins was updated.</returns>
		private async Task<bool> refreshCurrentCheckinsList() {
			if (getCurrentUser() != null) {
				
				if (getCurrentUser().uid != null) {
					mCheckedInLocationsIds.Clear();

					var currentCheckIns = await mLocationController.getUserLocationsByUserId(getCurrentUser().uid);

					if (currentCheckIns != null) {
						
						foreach (UserLocation userLocation in currentCheckIns) {
							
							if (userLocation.locationId != null) {
								mCheckedInLocationsIds.Add(userLocation.locationId);
							}
						}

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Update the user location on the server
		/// </summary>
		private async Task<bool> updateCurrentUserLocation() {
			if (getUserCoordinates() != null && userCoordinatesHaveChanged()) {
				var userCoordinate = new UserCoordinate();
				userCoordinate.latitude = FormatUtils.doubleToString(getUserCoordinates().getLatitude());
				userCoordinate.longitude = FormatUtils.doubleToString(getUserCoordinates().getLongitude());
				userCoordinate.userUid = getCurrentUser().uid;

				return await mUserController.updateUserCoordinate(userCoordinate);
			}

			return false;
		}

		/// <summary>
		/// Verify if the user has checked-in into a Sports Connect Location
		/// </summary>
		private async Task<bool> verifyPossibleCheckinsCheckouts() {
			if (getUserCoordinates() != null) {
				sLocationsNearby = await mLocationController.getLocations(true);

				sendBroadcastMessageNearbyLocationsHaveBeenUpdated();

				if (sLocationsNearby != null) {

					foreach (Location location in sLocationsNearby) {

						if (location != null) {
							var distance = GeoCoordinatesUtils.distance(
											  getUserCoordinates().getLatitude(),
											  getUserCoordinates().getLongitude(),
											  FormatUtils.stringToDouble(location.localLatitude),
											  FormatUtils.stringToDouble(location.localLongitude),
											  Constants.MEASURE_KM);

							// Send a notification to the foreground if the user has checked-in into a location, or 
							// if the user is currently checked-in, verify if there is the need of performing a check-out
							if (!isUserCheckedInIntoLocation(location.id) &&
								(distance < Constants.DISTANCE_THRESHOLD_CHECKED_IN_LOCATION_RADIUS)) {
								return await verifyStatusAndCheckIn(location);

							} else if (isUserCheckedInIntoLocation(location.id) &&
									   (distance > Constants.DISTANCE_THRESHOLD_CHECKED_IN_LOCATION_RADIUS)) {
								return await verifyStatusAndCheckOut(location);
							}
						}
					}

					if (sLocationsNearby.Count > 0) {
						setStatusMustUpdate(false);
					}
				}
			}
				
			return false;
		}

		private bool isUserCheckedInIntoLocation(string selectedLocationId) {
			if (mCheckedInLocationsIds.Contains(selectedLocationId)) {
				return true;
			} else {
				return false;
			}
		}

		private async Task<bool> verifyStatusAndCheckIn(Location location) {
			if (location != null) {

				if (location.id != null) {

					if (!mCheckedInLocationsIds.Contains(location.id)) {

						if (await mLocationController.createRelUserLocation(getCurrentUser().uid, location.id)) {
							checkIn(location.id, location.name);
							return true;
						}
					}
				}
			}

			return false;
		}

		private void checkIn(string locationId, string locationName) {
			if ((locationId != null) && (locationName != null)) {
				setCheckedInLocationId(locationId);
				mCheckedInLocationsIds.Add(locationId);
			}
		}

		private async Task<bool> verifyStatusAndCheckOut(Location location) {
			if (location != null){
				if (await mLocationController.deleteRelUserLocation(getCurrentUser().uid, location.id)) {
					checkOut(location.id);
					return true;
				}
			}

			return false;
		}

		private void checkOut(string locationId) {
			if (locationId != null) {
				setCheckedInLocationId("");
				mCheckedInLocationsIds.Remove(locationId);
			}
		}


		// Getters and Setters
		//....................
		public static void setCurrentUser(User user) {
			sCurrentUser = user;
		}

		public static User getCurrentUser() {
			return sCurrentUser;
		}

		public void setUserCoordinates(Geopoint userCoordinates) {
			mCurrentLocation = userCoordinates;
		}

		public Geopoint getUserCoordinates() {
			if (mCurrentLocation == null) {
				var userCoordinates = new Geopoint();
				userCoordinates.setLatitude(SettingsController.getCurrentLatitude());
				userCoordinates.setLongitude(SettingsController.getCurrentLongitude());
				userCoordinates.setAltitude(0.0);

				mCurrentLocation = userCoordinates;
			}

			return mCurrentLocation;
		}

		public void setCheckedInLocationId(string locationId) {
			sCheckedInLocationId = locationId;
		}

		public static string getCheckedInLocationId() {
			return sCheckedInLocationId;
		}

		public void setStatusUserCoordinatesChanged(bool coordinatesChangeStatus) {
			mUserCoordinatesChanged = coordinatesChangeStatus;
		}

		public bool userCoordinatesHaveChanged() {
			return mUserCoordinatesChanged;
		}

		public void setStatusTaskHasCompletedIteration(bool iterationCompleted) {
			mTaskHasFinishedIteration = iterationCompleted;
		}

		public bool taskHasCompletedInteration() {
			return mTaskHasFinishedIteration;
		}

		public static void setStatusMustUpdate(bool mustUpdate) {
			sMustUpdate = mustUpdate;
		}

		public static bool mustUpdate() {
			return sMustUpdate;
		}

		public bool userIsCheckedIn() {
			if (mCheckedInLocationsIds != null) {

				if (mCheckedInLocationsIds.Count > 0) {
					return true;
				}
			}

			return false;
		}

		public bool listLocationEmpty() {
			return sLocationsNearby.Count == 0;
		}


		// Messages
		// ........
		/// <summary>
		/// Start listening to the navite location controllers and get the current user location
		/// </summary>
		private void initializeLocationManagerListener() {
			MessagingCenter.Subscribe<UserLocationMessage>(this, UserLocationMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {

					if (message != null) {
						
						// Get the initial user location
						if (getUserCoordinates() == null) {
							var userCoordinates = new Geopoint();
							userCoordinates.setLatitude(message.lat);
							userCoordinates.setLongitude(message.lng);
							userCoordinates.setAltitude(message.altitude);
							setUserCoordinates(userCoordinates);
							setStatusUserCoordinatesChanged(true);

						} else {
							// Update the user location only if a movement threshold was reached (50 meters)
							var distance = GeoCoordinatesUtils.distance(
												  getUserCoordinates().getLatitude(),
												  getUserCoordinates().getLongitude(),
												  message.lat,
												  message.lng,
												  Constants.MEASURE_KM);

							if (distance > Constants.DISTANCE_THRESHOLD_MIN_MOVEMENT_UPDATE_LOCATION) {
								getUserCoordinates().setAltitude(message.altitude);
								getUserCoordinates().setLatitude(message.lat);
								getUserCoordinates().setLongitude(message.lng);
								setStatusUserCoordinatesChanged(true);
							}
						}
					}
				});
			});
		}

		/// <summary>
		/// Send an updated list of nearby Locations to anyone who listens to this event.
		/// </summary>
		public void sendBroadcastMessageNearbyLocationsHaveBeenUpdated() {
			var message = new NearbyLocationsMessage {
				message = "Updated list of nearby locations",
				nearbyLocations = sLocationsNearby
			};

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(message, NearbyLocationsMessage.TAG);
			});
		}
	
	}

}