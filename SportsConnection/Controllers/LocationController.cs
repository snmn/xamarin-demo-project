using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {
	
	public class LocationController {

		public UserManager userManager;
		public SCLocationManager locationManager;
		public UserLocationManager userLocationManager;
		public LocationFeedbackManager locationFeedbackManager;


		public LocationController() {
			userManager = new UserManager();
			locationManager = new SCLocationManager();
			userLocationManager = new UserLocationManager();
			locationFeedbackManager = new LocationFeedbackManager();
		}

		public async Task<bool> upsertLocation(string name, string description, int capacity, string latitude, 
		                                       string longitude, bool verified, DateTime verifiedDate, User createdBy) {
			var newLocation = new Location();
			newLocation.name = name;
			
			newLocation.localLatitude = latitude;
			newLocation.localLongitude = longitude;
			newLocation.verified = verified;
			newLocation.verifiedDate = verifiedDate;
			newLocation.userId = createdBy.uid;
											
			return await locationManager.saveLocationAsync(newLocation);
		}

		public async Task<bool> createRelUserLocation(string uid, string locationId) {
			var newUserLocation = new UserLocation();
			newUserLocation.userId = uid;
			newUserLocation.locationId = locationId;

			return await userLocationManager.checkInLocationAsync(newUserLocation);
		}

		public async Task<bool> createFeedbackNumUsersAtLocation(int userCount,string uid, string locationId) {
			var locationFeedback = new LocationFeedback();
			locationFeedback.userId = uid;
			locationFeedback.locationId = locationId;
			locationFeedback.userCount = userCount;

			return await locationFeedbackManager.saveLocationFeedbackAsync(locationFeedback);
		}

		public async Task<bool> updateLocation(Location location) {
			if (location != null) {
				return await locationManager.saveLocationAsync(location);
			}

			return false;
		}

		public async Task<List<Location>> getLocations(bool refresh) {
			return await locationManager.getLocationsAsync();
		}

		public async Task<UserLocation> getUserLocationsByIds(string uid, string locationId) {
			if (uid != null && locationId != null) {
				return await userLocationManager.getUserLocationRelByIds(uid, locationId);
			} else {
				return null;
			}
		}

		public async Task<List<UserLocation>> getUserLocationsByUserId(string uid) {
			if (uid != null) {
				return await userLocationManager.getUserLocationsById(uid);
			} else {
				return null;
			}
		}

		public async Task<List<UserLocationWrapper>> getUserRecentLocations(string uid) {
			if (uid != null) {
				var recentUserLocationsRels = await userLocationManager.getRecentUserLocations(uid);

				if (recentUserLocationsRels != null) {
					var userLocations = new List<UserLocationWrapper>();

					foreach (UserLocation userLocRel in recentUserLocationsRels) {

						if (userLocRel != null) {
							
							if (userLocRel.locationId != null && userLocRel.userId != null) {
								var userLocation = new UserLocationWrapper();

								var location = await locationManager.getLocationById(userLocRel.locationId);
								var user = await userManager.getUserByUID(userLocRel.userId);

								if (location != null && user != null) {
									userLocation.location = location;
									userLocation.user = user;
									userLocation.core = userLocRel;

									userLocations.Add(userLocation);
								}
							}
						}
					}

					return userLocations;
				}
			}	

			return null;
		}

		public async Task<List<UserLocation>> getUserLocationsByLocationId(string locationId) {
			if (locationId != null) {
				return await userLocationManager.getUsersCheckedIntoLocation(locationId);
			} else {
				return null;
			}
		}

		public async Task<List<UserLocation>> getUserLocationsRelationships() {
			return await userLocationManager.getLocationsAsync();
		}

		public async Task<List<LocationFeedback>> getManualLocations() {
			return await locationFeedbackManager.getLocationFeedbacksAsync();
		}

		public async Task<Location> getLocationById(string locationId) {
			return await locationManager.getLocationById(locationId);
		}

		public async Task<Location> getLocationByName(string locationName) {
			return await locationManager.getLocationByName(locationName);
		}

		public async Task<UserLocationWrapper> getUserLocationById(string userLocationId) {
			UserLocation userLocation = await userLocationManager.getUserLocationById(userLocationId);

			if (userLocation != null) {
				User user = await userManager.getUserById(userLocation.userId);
				Location location = await locationManager.getLocationById(userLocation.locationId);

				var userLocWrap = new UserLocationWrapper();
				userLocWrap.core = userLocation;
				userLocWrap.user = user;
				userLocWrap.location = location;

				return userLocWrap;
			} else {
				return null;
			}
		}

		public async Task<Dictionary<string, Location>> getDictLocationsByIds(List<string> locationIds) {
			if (locationIds.Count > 0 && locationIds != null) {
				return await locationManager.getDictLocationsByIds(locationIds);
			}

			return null;
		}

		public async Task<LocationFeedbackWrapper> getLocationFeedbackById(string locationFeedbackId) {
			LocationFeedback locationFeedback = await locationFeedbackManager.getLocationFeedbackById(locationFeedbackId);

			if (locationFeedback != null) {
				User user = await userManager.getUserById(locationFeedback.userId);
				Location location = await locationManager.getLocationById(locationFeedback.locationId);

				var manLocWrap = new LocationFeedbackWrapper();
				manLocWrap.core = locationFeedback;
				manLocWrap.user = user;
				manLocWrap.location = location;

				return manLocWrap;
			} else {
				return null;
			}
		}

		public async Task<List<Location>> getLocationsOwnedByUser(string userUid) {
			if (userUid != null) {
				return await locationManager.getLocationsOwnedByUser(userUid);
			} else {
				return null;
			}
		}

		public async Task<bool> deleteLocation(string locationId) {
			if (locationId != null) {
				return await locationManager.deleteLocationAsync(locationId);
			}

			return false;
		}

		public async Task<bool> deleteRelUserLocation(string userId, string locationId) {
			if ((userId != null) && (locationId != null)){
				return await userLocationManager.checkOutFromLocationAsync(userId, locationId);
			}

			return false;
		}

		public async Task<bool> deleteFeedbackNumUsersAtLocation(string feedbackId) {
			if (feedbackId != null) {
				return await locationFeedbackManager.deleteLocationFeedbackAsync(feedbackId);
			}

			return false;
		}

	}

}