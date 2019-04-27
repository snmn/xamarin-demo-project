using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {

	public class SportsController {

		public static string SPORT_NAME_BASKETBALL = "basketball";
		public static string SPORT_NAME_SOCCER = "soccer";

		public UserManager userManager;
		public UserSportManager userSportManager;
		public SportManager sportManager;
		public LocationSportManager locationSportManager;
		public SCLocationManager locationManager;


		public SportsController() {
			userManager = new UserManager();
			userSportManager = new UserSportManager();
			sportManager = new SportManager();
			locationSportManager = new LocationSportManager();
			locationManager = new SCLocationManager();
		}

		public async Task<bool> createListSports() {
			if (await addSport(Constants.SPORT_BASKETBALL, Constants.SPORT_BASKETBALL_NUM_PLAYERS, App.authController.getCurrentUser().id)) {
				if (await addSport(Constants.SPORT_SOCCER, Constants.SPORT_SOCCER_NUM_PLAYERS, App.authController.getCurrentUser().id)) {
					return true;
				}
			}

			return false;
		}

		public async Task<bool> addSport(string name, int recNumPlayers, string creatorId) {
			Sport newSport = new Sport();
			newSport.name = name;
			newSport.recNumPlayers = recNumPlayers;
			newSport.userId = creatorId;

			return await sportManager.saveSportAsync(newSport);
		}

		public async Task<bool> createUserSportRelationship(string userId, string sportId) {
			UserSport newUserSport = new UserSport();
			newUserSport.userId = userId;
			newUserSport.sportId = sportId;

			return await userSportManager.saveUserSportAsync(newUserSport);
		}

		public async Task<bool> createLocationSportRelationship(string locationId, string sportId) {
			LocationSport newLocationSport = new LocationSport();
			newLocationSport.locationId = locationId;
			newLocationSport.sportId = sportId;

			return await locationSportManager.saveLocationSportAsync(newLocationSport);
		}

		public async Task<bool> updateSport(Sport sport) {
			if (sport != null) {
				return await sportManager.saveSportAsync(sport);
			}

			return false;
		}

		public async Task<List<Sport>> getSports(bool refresh) {
			return await sportManager.getSportsAsync(refresh);
		}

		public async Task<Dictionary<string, Sport>> getDictSportsByIds(List<string> sportIds) {
			if (sportIds.Count > 0 && sportIds != null) {
				return await sportManager.getDictSportsByIds(sportIds);
			}

			return null;
		}

		public async Task<Sport> getSportByIdAsync(string sportId) {
			return await sportManager.getSportById(sportId);
		}

		public async Task<Sport> getSportByNameAsync(string sportName) {
			return await sportManager.getSportByName(sportName);
		}

		public async Task<Sport> getSportById(string sportId) {
			List<Sport> sports = await getSports(true);

			foreach (Sport sport in sports) {
				if (sport.id != null) {
					if (sport.id == sportId) {
						return sport;
					}
				}
			}

			return null;
		}

		public async Task<List<UserSportWrapper>> getUserSports(string userId) {
			List<UserSport> userSports = await userSportManager.getUserSports(userId, false);
			List<UserSportWrapper> userSportsFilled = new List<UserSportWrapper>();

			if (userSports != null) {
				foreach (UserSport userSport in userSports) {
					User user = await userManager.getUserById(userId);
					Sport sport = await getSportById(userSport.sportId);

					if ((user != null) && (sport != null)) {
						UserSportWrapper userSportWrapper = new UserSportWrapper();
						userSportWrapper.core = userSport;
						userSportWrapper.user = user;
						userSportWrapper.sport = sport;

						userSportsFilled.Add(userSportWrapper);
					}
				}

				return userSportsFilled;
			} else {
				return null;
			}
		}

		public async Task<List<LocationSportWrapper>> getLocationSports(string locationId) {
			List<LocationSport> locationSports = await locationSportManager.getLocationSports(locationId, true);
			List<LocationSportWrapper> locationSportsFilled = new List<LocationSportWrapper>();

			if (locationSports != null) {
				foreach (LocationSport locationSport in locationSports) {
					Location location = await locationManager.getLocationById(locationId);
					Sport sport = await getSportById(locationSport.sportId);

					if ((location != null) && (sport != null)) {
						LocationSportWrapper locationSportWrapper = new LocationSportWrapper();
						locationSportWrapper.core = locationSport;
						locationSportWrapper.location = location;
						locationSportWrapper.sport = sport;

						locationSportsFilled.Add(locationSportWrapper);
					}
				}

				return locationSportsFilled;
			} else {
				return null;
			}
		}

		public async Task<bool> deleteSport(string sportId) {
			if (sportId != null) {
				return await sportManager.deleteSportAsync(sportId);
			}

			return false;
		}

		public async Task<bool> deleteUserSportRelation(UserSport userSport) {
			if ((userSport.sportId != null) && (userSport.userId != null)) {
				return await userSportManager.deleteUserSportAsync(userSport);
			}

			return false;
		}

		public async Task<bool> deleteLocationSportRelation(LocationSport locationSport) {
			if ((locationSport.sportId != null) && (locationSport.locationId != null)) {
				return await locationSportManager.deleteLocationSportAsync(locationSport);
			}

			return false;
		}

		public async Task<bool> deleteLocationSportRelation(string locationId, string sportId) {
			if ((sportId != null) && (locationId != null)) {
				return await locationSportManager.deleteLocationSportAsync(locationId, sportId);
			}

			return false;
		}

		public async Task<bool> cleanLocationSportRelationships(string locationId) {
			if (locationId != null) {
				return await locationSportManager.cleanLocationSportRelationships(locationId);
			}

			return false;
		}

		public static string getCleanedSportName(string rawSportName) {
			string cleanedSportsName = rawSportName.ToLower();
			cleanedSportsName = cleanedSportsName.Replace(" ", "");

			return cleanedSportsName;
		}

	}

}