using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {

	public class SportTestModule {

		private string TAG = "SportTestModule";
		private SportsController mSportsController = new SportsController();
		bool unitTestResult = true;


		public SportTestModule() {
			DebugHelper.newMsg(TAG, "Starting Sport Storage Test ...");
		}

		public async Task<bool> runUnitTest() {
			bool testResult = await executeTestLogic();

			if (testResult) {
				DebugHelper.newMsg(TAG, "Sport storage test has finished: SUCCEEDED");
			} else {
				DebugHelper.newMsg(TAG, "Sport storage test has finished: FAILED");
			}

			return testResult;
		}

		public async Task<bool> executeTestLogic() {
			await testSportCRUD();
			await testUserSportsRelationships();
			await testLocationSportsRelationships();

			return unitTestResult;
		}

		private async Task<bool> testSportCRUD() {
			DebugHelper.newMsg("\n" + TAG, "Starting Sport CRUD test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			UserController userController = new UserController();

			// Sports
			string sportAName = "Sport A";
			int sportARecNum = 5;

			string sportBName = "Sport B";
			int sportBRecNum = 10;

			// User
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "goggle1243";
			string userATwitterId = "twitter1243";
			int userAdistance = 0;
			//...........................................................

			// Create test user
			if (await userController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId, 
			                                 userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				DebugHelper.newMsg(TAG, "Created new user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to create a new user");
				unitTestResult = false;
			}

			// Get user
			User user = await userController.getUserByUID(userAEmail);

			// Create Sport
			if (await mSportsController.addSport(sportAName, sportARecNum, user.id)) {
				if (await mSportsController.addSport(sportBName, sportBRecNum, user.id)) {
					DebugHelper.newMsg(TAG, "Created new sports");
				} else {
					DebugHelper.newMsg(TAG, "Failed to created new sports");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new sports");
				unitTestResult = false;
			}

			// Read all Sports
			List<Sport> currentSports = await mSportsController.getSports(true);
			List<string> sportIds = new List<string>();

			if (currentSports != null) {
				foreach (Sport spt in currentSports) {
					if (spt != null) {
						sportIds.Add(spt.id);
					}
				}
				DebugHelper.newMsg(TAG, "Read all sports.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to read all sports.");
				unitTestResult = false;
			}

			// Update Sport
			Sport sport = await mSportsController.getSportByNameAsync(sportAName);

			if (sport == null) {
				DebugHelper.newMsg(TAG, "Failed to read sport from server");
				unitTestResult = false;
			} else {	
				DebugHelper.newMsg(TAG, "Read single sport");

				sport.name = "Sport A updated name";

				if (await mSportsController.updateSport(sport)) {
					DebugHelper.newMsg(TAG, "Updated sport");
				} else {
					DebugHelper.newMsg(TAG, "Failed to update sport");
					unitTestResult = false;
				}
			}

			// Get a dictionary of sports passing a list of ids
			Dictionary<string, Sport> dictSportsFromIds = await mSportsController.getDictSportsByIds(sportIds);

			if (dictSportsFromIds != null) {
				foreach (string key in dictSportsFromIds.Keys) {
					Sport spt = dictSportsFromIds[key];
					DebugHelper.newMsg(TAG, string.Format(@"Location: {0}", spt));
				}
				DebugHelper.newMsg(TAG, "Got a dictionary of sports from ids.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to get a dictionary of sports from ids.");
				unitTestResult = false;
			}

			// Read sport individually
			Sport sportA = await mSportsController.getSportByNameAsync("Sport A updated name");
			Sport sportB = await mSportsController.getSportByNameAsync(sportBName);

			// Delete Sport
			if (await mSportsController.deleteSport(sportA.id)) {
				if (await mSportsController.deleteSport(sportB.id)) {
					DebugHelper.newMsg(TAG, "Deleted sports");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete sport B");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete sport A");
				unitTestResult = false;
			}

			// Delete test user
			if (await userController.deleteUser(user.id)) {
				DebugHelper.newMsg(TAG, "Deleted test user.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test user.");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing Sport CRUD test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testUserSportsRelationships() {
			DebugHelper.newMsg("\n" + TAG, "Starting UserSport test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			UserController userController = new UserController();

			// Sport
			string sportAName = "Sport A";
			int sportARecNum = 5;
			string sportATestId = "189b41f2-d4d9-477d-9f19-f2b4f80657c1";

			// User
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "goggle1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;
			//...........................................................

			// Create
			if (await userController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId,
			                                 userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				DebugHelper.newMsg(TAG, "Created new user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to create a new user");
				unitTestResult = false;
			}

			// Get user
			User userA = await userController.getUserByUID(userAEmail);

			// Create Sport
			if (await mSportsController.addSport(sportAName, sportARecNum, sportATestId)) {
				DebugHelper.newMsg(TAG, "Created new sport");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new sport");
				unitTestResult = false;
			}

			// Get sport
			Sport sport = await mSportsController.getSportByNameAsync(sportAName);

			// UserSport tests
			//...........................................................
			if (userA != null && sport != null) {

				// Read
				List<UserSportWrapper> userSports = await mSportsController.getUserSports(userA.id);

				if (userSports == null) {
					DebugHelper.newMsg(TAG, "Failed to get User Sports.");
				} else {
					if (userSports != null) {
						if (userSports.Count > 0) {
							foreach (UserSportWrapper userSport in userSports) {
								DebugHelper.newMsg(TAG, "User Sport : " + userSport.core.toString());
							}
						} else {
							DebugHelper.newMsg(TAG, "User Sports table is empty.");
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to get User Sports.");
					}
				}

				// Create
				if (await mSportsController.createUserSportRelationship(userA.id, sport.id)) {
					DebugHelper.newMsg(TAG, "Created UserSport relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create UserSport relationship.");
					unitTestResult = false;
				}

				// Read
				userSports = await mSportsController.getUserSports(userA.id);
				UserSport userSportToDelete = null;

				if (userSports == null) {
					DebugHelper.newMsg(TAG, "Failed to get User Sports.");
					unitTestResult = false;
				} else {
					if (userSports != null) {
						if (userSports.Count > 0) {
							foreach (UserSportWrapper userSport in userSports) {
								DebugHelper.newMsg(TAG, "User Sport : " + userSport.core.toString());
								userSportToDelete = userSport.core;
							}
						} else {
							DebugHelper.newMsg(TAG, "User Sports table is empty.");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to get User Sports.");
						unitTestResult = false;
					}
				}

				// Delete
				if (userSportToDelete != null) {
					if (await mSportsController.deleteUserSportRelation(userSportToDelete)) {
						DebugHelper.newMsg(TAG, "Deleted UserSport.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete UserSport");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete UserSport");
					unitTestResult = false;
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to get dependencies for the UserSport test.");
				unitTestResult = false;
			}
			//...........................................................

			// Delete test user
			if (await userController.deleteUser(userA.id)) {
				DebugHelper.newMsg(TAG, "Deleted test user.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test user.");
				unitTestResult = false;
			}

			// Delete test Sport
			if (await mSportsController.deleteSport(sport.id)) {
				DebugHelper.newMsg(TAG, "Deleted test sport");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test sport");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing UserSport relationship test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testLocationSportsRelationships() {
			DebugHelper.newMsg("\n" + TAG, "Starting LocationSport test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			LocationController locationController = new LocationController();
			UserController userController = new UserController();

			// User
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "goggle1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;

			// Sport
			string sportAName = "Sport A";
			int sportARecNum = 5;
			string sportATestId = "189b41f2-d4d9-477d-9f19-f2b4f80657c1";

			// Location
			string locationAName = "Location A";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;
			//...........................................................

			// Create
			if (await userController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId,
			                                 userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				DebugHelper.newMsg(TAG, "Created new user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to create a new user");
				unitTestResult = false;
			}

			// Get user
			User user = await userController.getUserByUID(userAEmail);

			// Create Sport
			if (await mSportsController.addSport(sportAName, sportARecNum, sportATestId)) {
				DebugHelper.newMsg(TAG, "Created new sport");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new sport");
				unitTestResult = false;
			}

			// Get sport
			Sport sport = await mSportsController.getSportByNameAsync(sportAName);

			// Create Location
			if (await locationController.upsertLocation(locationAName, "", locationACapacity, locationALatitude,
													  locationALongitude, locationAVerified,
													  locationAVerifiedDate, user)) {
				DebugHelper.newMsg(TAG, "Created new location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get location
			Location location = await locationController.getLocationByName("Location A");

			// SportLocation tests
			//...........................................................
			if (user != null && sport != null && location != null) {
				
				// Read
				List<LocationSportWrapper> locationSports = await mSportsController.getLocationSports(location.id);

				if (locationSports == null) {
					DebugHelper.newMsg(TAG, "Failed to get Location Sports.");
				} else {
					if (locationSports != null) {
						if (locationSports.Count > 0) {
							foreach (LocationSportWrapper locationSport in locationSports) {
								DebugHelper.newMsg(TAG, "Location Sport : " + locationSport.core.toString());
							}
						} else {
							DebugHelper.newMsg(TAG, "Location Sports table is empty.");
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to get Location Sports.");
					}
				}

				// Create
				if (await mSportsController.createLocationSportRelationship(location.id, sport.id)) {
					DebugHelper.newMsg(TAG, "Created LocationSport relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create LocationSport relationship.");
					unitTestResult = false;
				}

				// Read
				locationSports = await mSportsController.getLocationSports(location.id);
				LocationSport locSport = null;

				if (locationSports == null) {
					DebugHelper.newMsg(TAG, "Failed to get Location Sports.");
					unitTestResult = false;
				} else {
					if (locationSports != null) {
						if (locationSports.Count > 0) {
							foreach (LocationSportWrapper locSports in locationSports) {
								DebugHelper.newMsg(TAG, "Location Sport : " + locSports.core.toString());
								locSport = locSports.core;
							}
						} else {
							DebugHelper.newMsg(TAG, "Location Sports table is empty.");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to get Location Sports.");
						unitTestResult = false;
					}
				}

				// Delete
				if (locSport != null) {
					if (await mSportsController.deleteLocationSportRelation(locSport)) {
						DebugHelper.newMsg(TAG, "Deleted LocationSport.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete LocationSport");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete LocationSport");
					unitTestResult = false;
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to create dependencies for the SportLocation test.");
				unitTestResult = false;
			}
			//...........................................................

			// Delete test user
			if (await userController.deleteUser(user.id)) {
				DebugHelper.newMsg(TAG, "Deleted test user.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test user.");
				unitTestResult = false;
			}

			// Delete test Location
			if (await locationController.deleteLocation(location.id)) {
				DebugHelper.newMsg(TAG, "Deleted test location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test location");
				unitTestResult = false;
			}

			// Delete test Sport
			if (await mSportsController.deleteSport(sport.id)) {
				DebugHelper.newMsg(TAG, "Deleted test sport");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test sport");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing LocationSport relationship test: {0}", testResult));

			return unitTestResult;
		}

	}

}