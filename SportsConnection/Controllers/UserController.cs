using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SportsConnection {
	
	public class UserController {

		public UserManager userManager;
		public UserCoordinateManager userCoordinateManager;
		public UserFavoriteLocationManager userFavoriteLocationManager;

		private List<User> mUsers;
		private List<UserCoordinate> mUserCoordinates;
		private List<UserFavoriteLocation> mUserFavoriteLocations;


		public UserController() {
			userManager = new UserManager();
			userCoordinateManager = new UserCoordinateManager();
			userFavoriteLocationManager = new UserFavoriteLocationManager();
		}

		public async Task<bool> addUser(string name, string email, string profileImage, string facebookId, 
		                                string googleId, string twitterId, DateTime birthDate, int distanceCurrentUser) {
			User newUser = new User();
			newUser.name = name;
			newUser.uid = email;
			newUser.profileImage = profileImage;
			newUser.facebookId = facebookId;
			newUser.birthDate = birthDate;
			

			await userManager.upsertUserAsync(newUser, email);

			return true;
		}

		public async Task<bool> createUserCoordianteRelationship(string userUid, string latitude, string longitude, 
		                                                double altitude) {
			UserCoordinate newUserCoordinate = new UserCoordinate();
			newUserCoordinate.userUid = userUid;
			newUserCoordinate.latitude = latitude;
			newUserCoordinate.longitude = longitude;
			

			await userCoordinateManager.saveUserCoordinateAsync(newUserCoordinate);
			return true;
		}

		public async Task<bool> createUserFavoriteLocationRelationship(string userUid, string locationId) {
			UserFavoriteLocation newUserFavoriteLocation = new UserFavoriteLocation();
			newUserFavoriteLocation.userId = userUid;
			newUserFavoriteLocation.locationId = locationId;

			await userFavoriteLocationManager.saveUserFavoriteLocationAsync(newUserFavoriteLocation);
			return true;
		}

		public async Task<List<User>> getUsers(bool refresh) {
			if (refresh) {
				mUsers = await userManager.getUsersAsync(false);
				return mUsers;
			} else {
				if (mUsers != null) {
					return mUsers;
				} else {
					mUsers = await userManager.getUsersAsync(false);
					return mUsers;
				}
			}
		}

		public async Task<List<UserCoordinate>> getUserCoordinates(bool refresh) {
			if (refresh) {
				mUserCoordinates = await userCoordinateManager.getUserCoordinatesAsync(false);
				return mUserCoordinates;
			} else {
				if (mUserCoordinates != null) {
					return mUserCoordinates;
				} else {
					mUserCoordinates = await userCoordinateManager.getUserCoordinatesAsync(false);
					return mUserCoordinates;
				}
			}
		}

		public async Task<List<User>> getListUsersByIds(List<string> userIds) {
			if (userIds.Count > 0 && userIds != null) {
				return await userManager.getListUsersByIds(userIds);	
			}

			return null;
		}

		public async Task<Dictionary<string, User>> getDictUsersByIds(List<string> userIds) {
			if (userIds.Count > 0 && userIds != null) {
				return await userManager.getDictUsersByIds(userIds);
			}

			return null;
		}

		public async Task<List<UserFavoriteLocation>> getUserFavoriteLocations(bool refresh) {
			if (refresh) {
				mUserFavoriteLocations = await userFavoriteLocationManager.getUserFavoriteLocationsAsync(refresh);
				return mUserFavoriteLocations;
			} else {
				if (mUserFavoriteLocations != null) {
					return mUserFavoriteLocations;
				} else {
					mUserFavoriteLocations = await userFavoriteLocationManager.getUserFavoriteLocationsAsync(refresh);
					return mUserFavoriteLocations;
				}
			}
		}

		public async Task<UserFavoriteLocationWrapper> getUserFavoriteLocation(string userUid, string locationId) {
			UserFavoriteLocation userFavoriteLocation = await userFavoriteLocationManager.getUserFavoriteLocationByIds(userUid, locationId);
			UserFavoriteLocationWrapper userFavoriteLocationFilled = new UserFavoriteLocationWrapper();

			if (userFavoriteLocation != null) {
				SCLocationManager locationManager = new SCLocationManager();
				User user = await getUserByUID(userUid);
				Location location = await locationManager.getLocationById(locationId);

				if ((user != null) && (location != null)) {
					userFavoriteLocationFilled.core = userFavoriteLocation;
					userFavoriteLocationFilled.user = user;
					userFavoriteLocationFilled.location = location;

					return userFavoriteLocationFilled;
				}
			}

			return null;
		}

		public async Task<User> getUserById(string userUid) {
			return await userManager.getUserById(userUid);
		}

		public async Task<User> getUserByUID(string uid) {
			return await userManager.getUserByUID(uid);
		}

		public User getLastUser() {
			for (int i = 1; i <= mUsers.Count(); i++) {
				if (i == mUsers.Count() - 1) {
					return mUsers.ElementAt(i);
				}
			}

			return null;
		}

		public async Task<UserCoordinate> getUserCoordinate(string userUid) {
			return await userCoordinateManager.getUserCoordinateByIdAsync(userUid);
		}

		public async Task<bool> updateUser(User user) {
			if (user != null) {
				return await userManager.upsertUserAsync(user, user.uid);
			}

			return false;
		}

		public async Task<bool> updateUserCoordinate(UserCoordinate userCoordinate) {
			if (userCoordinate != null) {
				return await userCoordinateManager.saveUserCoordinateAsync(userCoordinate);
			}

			return false;
		}

		public async Task<bool> deleteUser(string userUid) {
			if (userUid != null) {
				return await userManager.deleteUserAsync(userUid);
			}

			return false;
		}

		public async Task<bool> deleteUserCoordinate(string userUid) {
			if (userUid != null) {
				return await userCoordinateManager.deleteUserCoordinateAsync(userUid);
			}

			return false;
		}

		public async Task<bool> deleteUserFavoriteLocationRelationship(string userUid, string locationId) {
			if ((userUid != null) && (locationId != null)) {
				return await userFavoriteLocationManager.deleteUserFavoriteLocationAsync(userUid, locationId);
			}

			return false;
		}

	}

}