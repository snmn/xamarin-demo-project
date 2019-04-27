using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {
	
	public class LocationTestModule {
		
		private string TAG = "LocationTestModule";
		private readonly LocationController mLocationController = new LocationController();
		bool unitTestResult = true;


		public LocationTestModule() {
			DebugHelper.newMsg(TAG, "Starting Location Storage Test ...");
		}

		public async Task<bool> runUnitTest() {
			bool testResult = await executeTestLogic();

			if (testResult) {
				DebugHelper.newMsg(TAG, "Location storage test has finished: SUCCEEDED");
			} else {
				DebugHelper.newMsg(TAG, "Location storage test has finished: FAILED");
			}

			return testResult;
		}

		public async Task<bool> executeTestLogic() {
			await testLocationCRUD();
			await testUserLocationRelationships();
			await testLocationFeedbackRelationships();
			await testLocationPosts();

			return unitTestResult;
		}
			
		private async Task<bool> testLocationCRUD() {
			DebugHelper.newMsg("\n" + TAG, "Starting Location CRUD test ...");

			// Test variables
			//...........................................................
			string locationAName = "Location abc";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;

			string locationBName = "Location B";
			int locationBCapacity = 30;
			string locationBLatitude = "-8.036200";
			string locationBLongitude = "-34.9490200";
			bool locationBVerified = false;
			DateTime locationBVerifiedDate = DateTime.UtcNow;

			var testUser = new User();
			testUser.id = "216ffc53-72f7-4711-ac97-0386141550f9";
			testUser.name = "Test User";
			testUser.uid = "diego.silva@sportsconnect.io";
			testUser.birthDate = DateTime.UtcNow;
			//...........................................................

			// Create Locations
			if (await mLocationController.upsertLocation(locationAName, "", locationACapacity, locationALatitude, 
			                                          locationALongitude, locationAVerified, 
			                                          locationAVerifiedDate, testUser)) {

				if (await mLocationController.upsertLocation(locationBName, "", locationBCapacity, locationBLatitude,
													  locationBLongitude, locationBVerified,
													  locationBVerifiedDate, testUser)) {
					
					DebugHelper.newMsg(TAG, "Created new location");

					List<Location> currentLocs = await mLocationController.getLocations(true);

					// Read all Locations
					if (currentLocs != null) {
						DebugHelper.newMsg(TAG, "Read all locations.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to read all locations.");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to created new location");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get locations
			Location locationA = await mLocationController.getLocationByName(locationAName);
			Location locationB = await mLocationController.getLocationByName(locationBName);

			// Update location
			if (locationA == null) {
				DebugHelper.newMsg(TAG, "Failed to read location from server");
				unitTestResult = false;
			} else {
				DebugHelper.newMsg(TAG, "Read single location");

				locationA.name = "Udpated location name";

				if (await mLocationController.updateLocation(locationA)) {
					DebugHelper.newMsg(TAG, "Updated location.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to update location");
					unitTestResult = false;
				}
			}

			// Read all Locations
			List<Location> updatedLocs = await mLocationController.getLocations(true);
			var locationIds = new List<string>();

			if (updatedLocs != null) {
				foreach (Location loc in updatedLocs) {
					if (loc != null) {
						locationIds.Add(loc.id);
						DebugHelper.newMsg(TAG, string.Format(@"Location: {0}", loc.name));
					}
				}
				DebugHelper.newMsg(TAG, "Read all locations.");
			}

			// Get a dictionary of locations passing a list of ids
			Dictionary<string, Location> dictLocationsFromIds = await mLocationController.getDictLocationsByIds(locationIds);

			if (dictLocationsFromIds != null) {
				foreach (string key in dictLocationsFromIds.Keys) {
					Location loc = dictLocationsFromIds[key];
					DebugHelper.newMsg(TAG, string.Format(@"Location: {0}", loc.name));
				}
				DebugHelper.newMsg(TAG, "Got a dictionary of locations from ids.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to get a dictionary of locations from ids.");
				unitTestResult = false;
			}

			// Delete Locations
			if (await mLocationController.deleteLocation(locationA.id)) {
				if (await mLocationController.deleteLocation(locationB.id)) {
					DebugHelper.newMsg(TAG, "Deleted locations");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete location B");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete location A");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing Location CRUD test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testUserLocationRelationships() {
			DebugHelper.newMsg("\n" + TAG, "Starting UserLocation test ...");
			var userController = new UserController();

			// Test variables
			//...........................................................
			// Location 
			string locationAName = "Location ds";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;

			// User
			string userName = "Test User";
			string userEmail = "diego.silva@sportsconnect.io";
			//...........................................................

			// Create user
			if (await userController.addUser(userName, userEmail, "", "","", "", DateTime.UtcNow, 0)) {
				DebugHelper.newMsg(TAG, "Created user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get user
			User user = await userController.getUserByUID(userEmail);

			// Create Location
			if (await mLocationController.upsertLocation(locationAName, "", locationACapacity, locationALatitude,
													  locationALongitude, locationAVerified, locationAVerifiedDate, user)) {
				DebugHelper.newMsg(TAG, "Created location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get location
			Location location = await mLocationController.getLocationByName(locationAName);

			// UserLocation tests
			// ......................................................................................
			if (location != null && user != null) {

				// Read User Locations
				List<UserLocation> userLocations = await mLocationController.getUserLocationsRelationships();
				string userLocationId = "";

				if (userLocations != null) {
					if (userLocations.Count > 0) {
						foreach (UserLocation userLocation in userLocations) {
							DebugHelper.newMsg(TAG, "User Location : " + userLocation.toString());
						}
					} else {
						DebugHelper.newMsg(TAG, "User Location relationships table is empty.");
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to read User Location relationships.");
				}

				// Create UserLocation relationship
				if (await mLocationController.createRelUserLocation(user.id, location.id)) {
					DebugHelper.newMsg(TAG, "Created UserLocation relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create UserLocation relationship.");
					unitTestResult = false;
				}

				// Read User Locations
				userLocations = await mLocationController.getUserLocationsRelationships();

				if (userLocations != null) {
					foreach (UserLocation userLocation in userLocations) {
						DebugHelper.newMsg(TAG, "User Location : " + userLocation.toString());
						userLocationId = userLocation.id;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to read User Location relationships.");
				}

				// Read User Location wrapper
				UserLocationWrapper userLocWrap = await mLocationController.getUserLocationById(userLocationId);

				if (userLocWrap != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Created relationship between location {0} and user {1} at {2}", 
					                                      userLocWrap.location.name, 
					                                      userLocWrap.user.name, 
					                                      userLocWrap.core.CreatedAt));
				} else {
					DebugHelper.newMsg(TAG, "Failed to get full User Location object.");
					unitTestResult = false;
				}

				// Delete UserLocation relationship
				if (await mLocationController.deleteRelUserLocation(user.id, location.id)) {
					DebugHelper.newMsg(TAG, "Deleted UserLocation relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete UserLocation relationship.");
					unitTestResult = false;
				}

				// Read User Locations
				userLocations = await mLocationController.getUserLocationsRelationships();

				if (userLocations != null) {
					foreach (UserLocation userLocation in userLocations) {
						DebugHelper.newMsg(TAG, "User Location : " + userLocation.toString());
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to read User Location relationships.");
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to create dependencies for the LocatinFeedback tests");
				unitTestResult = false;
			}
			// ......................................................................................

			// Delete User
			if (user != null) {
				if (await userController.deleteUser(user.id)) {
					DebugHelper.newMsg(TAG, "Deleted test user");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete the test user");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get user");
				unitTestResult = false;
			}

			// Delete Location
			if (location != null) {
				if (await mLocationController.deleteLocation(location.id)) {
					DebugHelper.newMsg(TAG, "Deleted test location");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete test location");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get location");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing UserLocation test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testLocationFeedbackRelationships() {
			DebugHelper.newMsg("\n" + TAG, "Starting LocationFeedback relationship test ...");
			var userController = new UserController();

			// Test variables
			//...........................................................
			// Location 
			string locationAName = "Location ca";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;

			// User
			string userName = "Test User";
			string userEmail = "diego.silva@sportsconnect.io";
			//...........................................................

			// Create user
			if (await userController.addUser(userName, userEmail,"", "", "", "", DateTime.UtcNow, 0)) {
				DebugHelper.newMsg(TAG, "Created user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get user
			User user = await userController.getUserByUID(userEmail);

			// Create Location
			if (await mLocationController.upsertLocation(locationAName, "", locationACapacity, locationALatitude,
													  locationALongitude, locationAVerified, locationAVerifiedDate, user)) {
				DebugHelper.newMsg(TAG, "Created location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get location
			Location location = await mLocationController.getLocationByName(locationAName);

			// LocationFeedback tests
			// ......................................................................................
			if (location != null && user != null) {
				// Create
				if (await mLocationController.createFeedbackNumUsersAtLocation(10, user.uid, location.id)) {
					DebugHelper.newMsg(TAG, "Created Location feedback.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create Location feedback.");
				}

				// Read
				List<LocationFeedback> locationsFeedbacks = await mLocationController.getManualLocations();
				string locationFeedbackId = "";

				if (locationsFeedbacks == null) {
					DebugHelper.newMsg(TAG, "Failed to get Location feedbacks.");
				} else {
					foreach (LocationFeedback locationFeeback in locationsFeedbacks) {
						DebugHelper.newMsg(TAG, "Location feedback : " + locationFeeback.toString());
						locationFeedbackId = locationFeeback.id;
					}
				}

				// Read single
				LocationFeedbackWrapper manLocWrap = await mLocationController.getLocationFeedbackById(locationFeedbackId);

				if (manLocWrap == null) {
					DebugHelper.newMsg(TAG, "Failed to get Location feedback individually");
				} else {
					DebugHelper.newMsg(TAG, "Got Location feedback " + manLocWrap.location.name + " number of people "
									   + manLocWrap.core.userCount);
				}

				// Delete
				if (await mLocationController.deleteFeedbackNumUsersAtLocation(locationFeedbackId)) {
					DebugHelper.newMsg(TAG, "Deleted Location feedback");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete Location feedback");
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to create dependencies for the LocatinFeedback tests");
				unitTestResult = false;
			}
			// ......................................................................................

			// Delete User
			if (user != null) {
				if (await userController.deleteUser(user.id)) {
					DebugHelper.newMsg(TAG, "Deleted test user");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete the test user");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get user");
				unitTestResult = false;
			}

			// Delete Location
			if (location != null) {
				if (await mLocationController.deleteLocation(location.id)) {
					DebugHelper.newMsg(TAG, "Deleted test location");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete test location");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get location");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing LocationFeedback tests: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testLocationPosts() {
			DebugHelper.newMsg("\n" + TAG, "Starting LocationPosts test ...");
			var userController = new UserController();
			var sportController = new SportsController();

			// Test variables
			//...........................................................
			// Location 
			string locationAName = "Location vca";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;

			// User
			string userName = "Test User";
			string userEmail = "diego.silva@sportsconnect.io";

			// Sport
			string sportName = "Militar Pentatlo";
			int recNumPlayers = 15;
			//...........................................................

			// Create user
			if (await userController.addUser(userName, userEmail, "", "", "", "", DateTime.UtcNow, 0)) {
				DebugHelper.newMsg(TAG, "Created user");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get user
			User user = await userController.getUserByUID(userEmail);

			// Create Location
			if (await mLocationController.upsertLocation(locationAName, "", locationACapacity, locationALatitude, 
			                                          locationALongitude, locationAVerified, locationAVerifiedDate, user)) {
				DebugHelper.newMsg(TAG, "Created location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get location
			Location location = await mLocationController.getLocationByName(locationAName);

			// Create sport
			if (await sportController.addSport(sportName, recNumPlayers, user.id)){
				DebugHelper.newMsg(TAG, "Created new sport with name " + sportName);
			} else {
				DebugHelper.newMsg(TAG, "Failed to create sport");
				unitTestResult = false;
			}

			// Get sport
			Sport sport = await sportController.getSportByNameAsync(sportName);

			if (sport != null) {
				DebugHelper.newMsg(TAG, "Got sport object from backend");
			} else {
				DebugHelper.newMsg(TAG, "Failed to get new sport");
				unitTestResult = false;
			}

			// Location posts
			// ......................................................................................
			if (location != null && user != null && sport != null) {
				var locationPostController = new LocationPostController(location);

				// Create location post
				string postTitle = "How About a match this weekend?";
				string postMsg = "Ok, so I'm down for it";

				if (await locationPostController.addLocationPost(location.id, user.id, sport.id, postTitle,
																 postMsg, DateTime.UtcNow)) {
					DebugHelper.newMsg(TAG, "Created new location post");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create new location post");
					unitTestResult = false;
				}

				// Reload all location posts and get them
				await locationPostController.loadLocationPosts();
				List<LocationPostWrapper> locPosts = locationPostController.locationPosts;
				LocationPostWrapper selectedLocPost = null;

				if (locPosts != null) {
					if (locPosts.Count > 0) {
						foreach (LocationPostWrapper locPostWrap in locPosts) {
							DebugHelper.newMsg(TAG, string.Format(@"Post for location {0}, sent by user {1} at {2}: {3}",
																  locPostWrap.location.name, locPostWrap.user.name,
																  locPostWrap.core.postedDate.ToString(),
																  locPostWrap.core.text));

							// Read single location post
							selectedLocPost = locPostWrap;
						}
					} else {
						DebugHelper.newMsg(TAG, "There is no post for this location");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get location posts");
					unitTestResult = false;
				}

				// Update a location post
				if (selectedLocPost != null) {
					selectedLocPost.core.text = "No no, I didn't mean to say that";

					if (await locationPostController.updateLocationPost(selectedLocPost.core)) {
						DebugHelper.newMsg(TAG, "Updated location post.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to update the location post");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get individual location post");
					unitTestResult = false;
				}

				// Read updated location posts
				locPosts = await locationPostController.getWrappedLocationPosts();
				selectedLocPost = null;

				if (locPosts != null) {
					if (locPosts.Count > 0) {
						foreach (LocationPostWrapper locPostWrap in locPosts) {
							DebugHelper.newMsg(TAG, string.Format(@"Post for location {0}, sent by user {1} at {2}: {3}",
																  locPostWrap.location.name, locPostWrap.user.name,
							                                      locPostWrap.core.UpdatedAt.ToString(),
																  locPostWrap.core.text));

							// Read single location post
							selectedLocPost = locPostWrap;
						}
					} else {
						DebugHelper.newMsg(TAG, "There is no post for this location");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get location posts");
					unitTestResult = false;
				}

				// Delete a location post
				if (selectedLocPost != null) {
					if (await locationPostController.deleteLocationPost(selectedLocPost.core.id)) {
						DebugHelper.newMsg(TAG, "Deleted location post");
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete location post");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get location post");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to create dependencies to execute the Location Post test");
				unitTestResult = false;
			}
			// ......................................................................................

			// Delete User
			if (user != null) {
				if (await userController.deleteUser(user.id)) {
					DebugHelper.newMsg(TAG, "Deleted test user");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete the test user");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get user");
				unitTestResult = false;
			}

			// Delete Location
			if (location != null) {
				if (await mLocationController.deleteLocation(location.id)) {
					DebugHelper.newMsg(TAG, "Deleted test location");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete test location");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get location");
				unitTestResult = false;
			}

			if (sport != null) {
				if (await sportController.deleteSport(sport.id)) {
					DebugHelper.newMsg(TAG, "Deleted test sport");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete sport");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get and delete sport");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing LocationPosts tests: {0}", testResult));

			return unitTestResult;
		}	

	}

}