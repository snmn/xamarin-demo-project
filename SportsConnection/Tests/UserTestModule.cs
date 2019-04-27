using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {
	
	public class UserTestModule {
		
		private string TAG = "UserTestModule";

		public bool unitTestResult = true;
		private UserController mUserController = new UserController();
		private UserRelationshipsController mUserRelController = new UserRelationshipsController();

		public UserTestModule() {
			DebugHelper.newMsg(TAG, "Starting User unit tests ...");
		}

		public async Task<bool> runUnitTest() {
			bool testResult = await executeTestLogic();

			if (testResult) {
				DebugHelper.newMsg(TAG, "User storage test has finished: SUCCEEDED");
			} else {
				DebugHelper.newMsg(TAG, "User storage test has finished: FAILED");
			}

			return testResult;
		}

		public async Task<bool> executeTestLogic() {
			await testUserCRUD();
			await testUserCoordinateRelationship();
			await testUserFavLocRelationship();
			await testUserFriendshipRelationships();

			return unitTestResult;
		}

		private async Task<bool> testUserCRUD() {
			DebugHelper.newMsg("\n" + TAG, "Starting userCRUD tests...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			// user a
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "google1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;

			// user b
			string userBEmail = "userB@email.com";
			string userBName = "User B";
			string userBProfileImage = "https://pbs.twimg.com/profile_images/743138182607175680/ZJzktgBk_400x400.jpg";
			string userBFacebookId = "asdf41234b";
			string userBGoogleId = "google1234b";
			string userBTwitterId = "twitter1234b";
			int userBdistance = 140;
			//...........................................................

			// Create
			if (await mUserController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId,
			                                  userAGoogleId, userATwitterId,DateTime.UtcNow, userAdistance)) {
				if (await mUserController.addUser(userBName, userBEmail, userBProfileImage, userBFacebookId, 
				                                  userBGoogleId, userBTwitterId, DateTime.UtcNow, userBdistance)) {
					DebugHelper.newMsg(TAG, "Created new users");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create user B");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to create user A");
				unitTestResult = false;
			}

			// Read all
			List<User> users = await mUserController.getUsers(true);
			List<string> testUserIds = new List<string>();

			if (users != null) {
				foreach (User listUser in users) {
					DebugHelper.newMsg(TAG, string.Format(@"User: {0}", listUser.id));
					testUserIds.Add(listUser.id);
				}
				DebugHelper.newMsg(TAG, "Read all users.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to read all users.");
			}

			// Read users
			User userA = await mUserController.getUserByUID(userAEmail);
			User userB = await mUserController.getUserByUID(userBEmail);

			// Update
			if (userA == null) {
				DebugHelper.newMsg(TAG, "Failed to read user A from server");
				unitTestResult = false;
			} else {
				DebugHelper.newMsg(TAG, "Read single user");

				userA.name = "Updated user name";

				if (await mUserController.updateUser(userA)) {
					DebugHelper.newMsg(TAG, "Updated user A");
				} else {
					DebugHelper.newMsg(TAG, "Failed to update user");
					unitTestResult = false;
				}
			}

			// Read modified data
			List<User> updatedUsers = await mUserController.getUsers(true);

			if (updatedUsers != null) {
				foreach (User listUser in updatedUsers) {
					DebugHelper.newMsg(TAG, string.Format(@"User: {0}", listUser.id));
				}
				DebugHelper.newMsg(TAG, "Read all users.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to read all users.");
			}

			// Get a list of users passing a list of ids
			List<User> usersFromIds = await mUserController.getListUsersByIds(testUserIds);

			if (usersFromIds != null) {
				foreach (User listUser in usersFromIds) {
					DebugHelper.newMsg(TAG, string.Format(@"User: {0}", listUser.name));
				}
				DebugHelper.newMsg(TAG, "Got a list of users from ids.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to get a list of users from ids.");
				unitTestResult = false;
			}

			// Get a dictionary of users passing a list of ids
			Dictionary<string, User> dictUsersFromIds = await mUserController.getDictUsersByIds(testUserIds);

			if (dictUsersFromIds != null) {
				foreach (string key in dictUsersFromIds.Keys) {
					User user = dictUsersFromIds[key];
					DebugHelper.newMsg(TAG, string.Format(@"User: {0}", user));
				}
				DebugHelper.newMsg(TAG, "Got a dictionary of users from ids.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to get a dictionary of users from ids.");
				unitTestResult = false;
			}

			// Delete test users
			if (await mUserController.deleteUser(userA.id)) {
				if (await mUserController.deleteUser(userB.id)) {
					DebugHelper.newMsg(TAG, "Deleted all users.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete user B.");
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete user A.");
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing userCRUD tests: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testUserCoordinateRelationship() {
			DebugHelper.newMsg("\n" + TAG, "Starting UserCoordinate test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			// user a
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "google1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;

			string latitude = "-8.052374";
			string longitude = "-34.8979171";
			double altitude = 2000;
			//...........................................................

			// Create
			if (await mUserController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId, 
			                                  userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				DebugHelper.newMsg(TAG, "Created user A");
			} else {
				DebugHelper.newMsg(TAG, "Failed to create user A");
				unitTestResult = false;
			}

			// Get user
			User userA = await mUserController.getUserByUID(userAEmail);

			// User Coordinate
			//...........................................................
			if (userA != null) {
				DebugHelper.newMsg(TAG, "Got test user, starting user coordinate test");

				// Read all
				List<UserCoordinate> userCoordinates = await mUserController.getUserCoordinates(true);

				if (userCoordinates == null) {
					DebugHelper.newMsg(TAG, "Failed to get UserCoordinates.");
					unitTestResult = false;
				} else {
					if (userCoordinates.Count > 0) {
						foreach (UserCoordinate userCoord in userCoordinates) {
							DebugHelper.newMsg(TAG, "UserCoordinate : " + userCoord.toString());
						}
					} else {
						DebugHelper.newMsg(TAG, "UserCoordinate table is empty.");
					}
				}

				// Create
				if (await mUserController.createUserCoordianteRelationship(userA.uid, latitude, longitude, altitude)) {
					DebugHelper.newMsg(TAG, "Created UserCoordinate relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create UserCoordinate relationship.");
					unitTestResult = false;
				}

				// Read individual user coordinate
				UserCoordinate userCoordinate = await mUserController.getUserCoordinate(userA.uid);

				if (userCoordinate == null) {
					DebugHelper.newMsg(TAG, "Failed to get UserCoordinate.");
					unitTestResult = false;
				} else {
					DebugHelper.newMsg(TAG, "UserCoordinate : " + userCoordinate.toString());
				}

				// Update
				if (userCoordinate != null) {
				//	userCoordinate.altitude = 1000;

					if (await mUserController.updateUserCoordinate(userCoordinate)) {
						DebugHelper.newMsg(TAG, "Updated UserCoordinate.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to update UserCoordinate.");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to update UserCoordinate.");
					unitTestResult = false;
				}

				// Read all
				userCoordinates = await mUserController.getUserCoordinates(true);

				if (userCoordinates == null) {
					DebugHelper.newMsg(TAG, "Failed to get UserCoordinates.");
					unitTestResult = false;
				} else {
					if (userCoordinates.Count > 0) {
						foreach (UserCoordinate userCoord in userCoordinates) {
							DebugHelper.newMsg(TAG, "UserCoordinate : " + userCoord.toString());
							userCoordinate = userCoord;
						}
					} else {
						DebugHelper.newMsg(TAG, "UserCoordinate table is empty.");
						unitTestResult = false;
					}
				}

				// Delete
				if (userCoordinate != null) {
					if (await mUserController.deleteUserCoordinate(userCoordinate.userUid)) {
						DebugHelper.newMsg(TAG, "Deleted UserCoordinate.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete UserCoordinate");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete UserCoordinate");
					unitTestResult = false;
				}

				// Read all
				userCoordinates = await mUserController.getUserCoordinates(true);

				if (userCoordinates == null) {
					DebugHelper.newMsg(TAG, "Failed to get UserCoordinate.");
				} else {
					if (userCoordinates.Count > 0) {
						foreach (UserCoordinate userCoord in userCoordinates) {
							DebugHelper.newMsg(TAG, "UserCoordinate : " + userCoord.toString());
						}
					} else {
						DebugHelper.newMsg(TAG, "UserCoordinate table is empty.");
					}
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to get test user");
				unitTestResult = false;
			}

			// Delete test users
			if (await mUserController.deleteUser(userA.uid)) {
				DebugHelper.newMsg(TAG, "Deleted test user.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test user A.");
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing UserCoordinate relationship test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testUserFavLocRelationship() {
			DebugHelper.newMsg("\n" + TAG, "Starting User FavoriteLocation test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			LocationController locationController = new LocationController();

			// user a
			string userAEmail = "userA@email.com";
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "google1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;

			// location 
			string locationAName = "Location A";
			int locationACapacity = 50;
			string locationALatitude = "-8.036801";
			string locationALongitude = "-34.9490501";
			bool locationAVerified = false;
			DateTime locationAVerifiedDate = DateTime.UtcNow;
			//...........................................................

			// Create
			if (await mUserController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId, 
			                                  userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				DebugHelper.newMsg(TAG, "Created user A");
			} else {
				DebugHelper.newMsg(TAG, "Failed to create user A");
				unitTestResult = false;
			}

			// Get user
			User userA = await mUserController.getUserByUID(userAEmail);

			// Create Location
			if (await locationController.upsertLocation(locationAName,"", locationACapacity, locationALatitude,
													  locationALongitude, locationAVerified, locationAVerifiedDate, userA)) {
				DebugHelper.newMsg(TAG, "Created location");
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new location");
				unitTestResult = false;
			}

			// Get location
			Location location = await locationController.getLocationByName(locationAName);

			// User FavoriteLocation
			//...........................................................
			if (userA != null && location != null) {
				
				// Read all
				List<UserFavoriteLocation> userFavLocs = await mUserController.getUserFavoriteLocations(true);

				if (userFavLocs == null) {
					DebugHelper.newMsg(TAG, "Failed to get the user favorite locations.");
					unitTestResult = false;
				} else {
					if (userFavLocs.Count > 0) {
						foreach (UserFavoriteLocation userFavoriteLocation in userFavLocs) {
							DebugHelper.newMsg(TAG, "User favorite location : " + userFavoriteLocation.toString());
						}
					} else {
						DebugHelper.newMsg(TAG, "The user favorite location table is empty.");
					}
				}

				// Create
				if (await mUserController.createUserFavoriteLocationRelationship(userA.id, location.id)) {
					DebugHelper.newMsg(TAG, "Created user favorite location relationship.");
				} else {
					DebugHelper.newMsg(TAG, "Failed to create user favorite location relationship.");
					unitTestResult = false;
				}

				// Read individual
				UserFavoriteLocationWrapper userFavLoc = await mUserController.getUserFavoriteLocation(
					userA.id, location.id);

				if (userFavLoc == null) {
					DebugHelper.newMsg(TAG, "Failed to get the user favorite location.");
					unitTestResult = false;
				} else {
					DebugHelper.newMsg(TAG, "User favorite location : " + userFavLoc.core.toString());
				}

				// Delete
				if (userFavLoc != null) {
					if (await mUserController.deleteUserFavoriteLocationRelationship(userFavLoc.core.userId,
																					 userFavLoc.core.locationId)) {
						DebugHelper.newMsg(TAG, "Deleted favorite location.");
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete the favorite location");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete the favorite location");
					unitTestResult = false;
				}

				// Read all
				userFavLocs = await mUserController.getUserFavoriteLocations(true);

				if (userFavLocs == null) {
					DebugHelper.newMsg(TAG, "Failed to get the user favorite locations.");
				} else {
					if (userFavLocs.Count > 0) {
						foreach (UserFavoriteLocation userFavoriteLocation in userFavLocs) {
							DebugHelper.newMsg(TAG, "User favorite location : " + userFavoriteLocation.toString());
						}
					} else {
						DebugHelper.newMsg(TAG, "User Favorite location table is empty.");
					}
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to create dependencies for the user favorite location test");
				unitTestResult = false;
			}
			//...........................................................

			// Delete test user
			if (await mUserController.deleteUser(userA.id)) {
				DebugHelper.newMsg(TAG, "Deleted test user.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test user A.");
				unitTestResult = false;
			}

			// Delete test location
			if (await locationController.deleteLocation(location.id)) {
				DebugHelper.newMsg(TAG, "Deleted test location.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test location.");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing User FavoriteLocation relationship test: {0}", testResult));

			return unitTestResult;
		}

		private async Task<bool> testUserFriendshipRelationships() {
			DebugHelper.newMsg("\n" + TAG, "Starting User Friendship Relationships test ...");
			unitTestResult = true;

			// Test variables
			//...........................................................
			// user a
			string userAEmail;
			if (App.authController.getCurrentUser() != null) {
				userAEmail = App.authController.getCurrentUser().uid;
			} else {
				userAEmail = "userA@email.com";
			}
			string userAName = "User A";
			string userAProfileImage = "http://www.howtogetthewomanofyourdreams.com/wp-content/uploads/2013/03/womanslide21.png";
			string userAFacebookId = "asdf1234";
			string userAGoogleId = "google1234";
			string userATwitterId = "twitter1234";
			int userAdistance = 0;

			// user b
			string userBEmail = "userB@email.com";
			string userBName = "User B";
			string userBProfileImage = "https://pbs.twimg.com/profile_images/743138182607175680/ZJzktgBk_400x400.jpg";
			string userBFacebookId = "asdf41234b";
			string userBGoogleId = "google1234b";
			string userBTwitterId = "twitter1234b";
			int  userBdistance = 140;

			// user c
			string userCEmail = "userC@email.com";
			string userCName = "User C";
			string userCProfileImage = "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcSz0MiXAHGn0uEPWz8ttDHjHai89CX6rRn93EBN2hJkcn8fZ2Cy";
			string userCFacebookId = "fasa1234";
			string userCGoogleId = "google1234C";
			string userCTwitterId = "twitter1234C";
			int  userCdistance = 150;

			// user d
			string userDEmail = "userD@email.com";
			string userDName = "User D";
			string userDProfileImage = "http://media.ufc.tv/fighter_images/Kid_Yamamoto/YAMAMOTO_NORIFUMI.png";
			string userDFacebookId = "afvaaa1423";
			string userDGoogleId = "google1234d";
			string userDTwitterId = "twitter1234d";
			int  userDdistance = 160;

			// user e
			string userEEmail = "userE@email.com";
			string userEName = "User E";
			string userEProfileImage = "https://cdn.pastemagazine.com/www/articles/BranStarkMainTheory.jpg";
			string userEFacebookId = "afvaaa1423";
			string userEGoogleId = "google1234E";
			string userETwitterId = "twitter1234E";
			int  userEdistance = 200;

			// user coordinate a
			string userALatitude = "-8.051055";
			string userALongitude = "-34.9481021";
			double userAAltitude = 2000;

			// user coordiante b
			string userBLatitude = "-8.04961";
			string userBLongitude = "-34.9431351";
			double userBAltitude = 2000;

			// user coordinate c
			string userCLatitude = "-8.05183";
			string userCLongitude = "-34.9395941";
			double userCAltitude = 2000;

			// user coordinate d
			string userDLatitude = "-8.054114";
			string userDLongitude = "-34.9354531";
			double userDAltitude = 2000;

			// user coordinate e
			string userELatitude = "-8.054996";
			string userELongitude = "-34.9339401";
			double userEAltitude = 2000;
			//...........................................................

			// Create test users
			if (await mUserController.addUser(userAName, userAEmail, userAProfileImage, userAFacebookId, 
			                                  userAGoogleId, userATwitterId, DateTime.UtcNow, userAdistance)) {
				if (await mUserController.addUser(userBName, userBEmail, userBProfileImage, userBFacebookId, 
				                                  userBGoogleId, userBTwitterId, DateTime.UtcNow, userBdistance)) {
					if (await mUserController.addUser(userCName, userCEmail, userCProfileImage, userCFacebookId, 
					                                  userCGoogleId, userCTwitterId, DateTime.UtcNow, userCdistance)) {
						if (await mUserController.addUser(userDName, userDEmail, userDProfileImage, userDFacebookId, 
						                                  userDGoogleId, userDTwitterId, DateTime.UtcNow, userDdistance)) {
							if (await mUserController.addUser(userEName, userEEmail, userEProfileImage, userEFacebookId, 
							                                  userEGoogleId, userETwitterId, DateTime.UtcNow, userEdistance)) {
								DebugHelper.newMsg(TAG, "Test users were successfully created");
							} else {
								DebugHelper.newMsg(TAG, "Failed to create user E ...");
								unitTestResult = false;
							}
						} else {
							DebugHelper.newMsg(TAG, "Failed to create user D ...");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to create user C ...");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to create user B ...");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to create user A ...");
				unitTestResult = false;
			}

			// Get test users
			User userA = await mUserController.getUserByUID(userAEmail);
			User userB = await mUserController.getUserByUID(userBEmail);
			User userC = await mUserController.getUserByUID(userCEmail);
			User userD = await mUserController.getUserByUID(userDEmail);
			User userE = await mUserController.getUserByUID(userEEmail);

			if (userA != null) {
				if (userB != null) {
					if (userC != null) {
						if (userD != null) {
							if (userE != null) {
								DebugHelper.newMsg(TAG, "Got test users, creating user coordinates ...");

								// Update the user's coordinates
								if (await mUserController.createUserCoordianteRelationship(userA.id, userALatitude, userALongitude, userAAltitude)) {
									DebugHelper.newMsg(TAG, "Updated UserCoordinate of the user A.");
								} else {
									DebugHelper.newMsg(TAG, "Failed to update UserCoordinate of the user A.");
									unitTestResult = false;
								}

								// Update the user's coordinates
								if (await mUserController.createUserCoordianteRelationship(userB.id, userBLatitude, userBLongitude, userBAltitude)) {
									DebugHelper.newMsg(TAG, "Updated UserCoordinate of the user B.");
								} else {
									DebugHelper.newMsg(TAG, "Failed to update UserCoordinate of the user B.");
									unitTestResult = false;
								}

								// Update the user's coordinates
								if (await mUserController.createUserCoordianteRelationship(userC.id, userCLatitude, userCLongitude, userCAltitude)) {
									DebugHelper.newMsg(TAG, "Updated UserCoordinate of the user C.");
								} else {
									DebugHelper.newMsg(TAG, "Failed to update UserCoordinate of the user C.");
									unitTestResult = false;
								}

								// Update the user's coordinates
								if (await mUserController.createUserCoordianteRelationship(userD.id, userDLatitude, userDLongitude, userDAltitude)) {
									DebugHelper.newMsg(TAG, "Updated UserCoordinate of the user D.");
								} else {
									DebugHelper.newMsg(TAG, "Failed to update UserCoordinate of the user D.");
									unitTestResult = false;
								}

								// Update the user's coordinates
								if (await mUserController.createUserCoordianteRelationship(userE.id, userELatitude, userELongitude, userEAltitude)) {
									DebugHelper.newMsg(TAG, "Updated UserCoordinate of the user E.");
								} else {
									DebugHelper.newMsg(TAG, "Failed to update UserCoordinate of the user E.");
									unitTestResult = false;
								}

							} else {
								DebugHelper.newMsg(TAG, "Failed to get user E ...");
								unitTestResult = false;
							}
						} else {
							DebugHelper.newMsg(TAG, "Failed to get user D ...");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to get user C ...");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get user B ...");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to get user A ...");
				unitTestResult = false;
			}

			if (await checkUserFriendshipRelationships(userA, userC)) {
				DebugHelper.newMsg(TAG, "Done checking friendship relationships of the user A, at this point " +
				                   "it's ok if the relationships are empty but the recommended friends list.");
			} else {
				DebugHelper.newMsg(TAG, "Failed to load friendship relationships for the user A ...");
				unitTestResult = false;
			}

			// Search for the UserD, must return a list with the UserD in it.
			if (searchUser("User D", userA.id) != -1) {
				DebugHelper.newMsg(TAG, "Ok, we got something in the search routine");
			} else {
				DebugHelper.newMsg(TAG, "Failed to perform search, unespected behavior ...");
				unitTestResult = false;
			}

			// Create friendship relationships
			if (await mUserRelController.addOrConfirmFriend(userA, userB)) {
				DebugHelper.newMsg(TAG, "User A sent friendship request to user B");
			} else {
				DebugHelper.newMsg(TAG, "Failed to send friendship request from user A to user B");
				unitTestResult = false;
			}

			if (await mUserRelController.addOrConfirmFriend(userA, userC)) {
				DebugHelper.newMsg(TAG, "User A sent friendship request to user C");
			} else {
				DebugHelper.newMsg(TAG, "Failed to send friendship request from user A to user C");
				unitTestResult = false;
			}

			if (await mUserRelController.addOrConfirmFriend(userA, userD)) {
				DebugHelper.newMsg(TAG, "User A sent friendship request to user D");
			} else {
				DebugHelper.newMsg(TAG, "Failed to send friendship request from user A to user D");
				unitTestResult = false;
			}

			if (await mUserRelController.addOrConfirmFriend(userE, userA)) {
				DebugHelper.newMsg(TAG, "User E sent friendship request to user A");
			} else {
				DebugHelper.newMsg(TAG, "Failed to confirm relationship status between user A");
				unitTestResult = false;
			}

			// Check user relationships again
			if (await checkUserFriendshipRelationships(userA, userC)) {
				DebugHelper.newMsg(TAG, "Done checking friendship relationships of the user A...");
			} else {
				DebugHelper.newMsg(TAG, "Failed to load friendship relationships for the user A ...");
				unitTestResult = false;
			}

			// Block friend B for user A
			if (await mUserRelController.blockFriend(userA.id, userB.id)) {
				DebugHelper.newMsg(TAG, "User B is now banned from the user's A life in the app ...");
			} else {
				DebugHelper.newMsg(TAG, "Failed to unfriend for User B for User A ...");
				unitTestResult = false;
			}

			// User C confirm friendship request made by User A
			if (await mUserRelController.addOrConfirmFriend(userC, userA)) {
				DebugHelper.newMsg(TAG, "User C confirmed friendship request made by the user A");
			} else {
				DebugHelper.newMsg(TAG, "Failed to confirmed friendship request made by the user A");
				unitTestResult = false;
			}

			// Reload the user relationships
			// userfriends, must return C
			// pendingCurrentUserApproval, must return E
			// pendingInvitation, must return D
			// recommendedFriends, must return [D]
			// getListUserFriends C, must return A
			if (await checkUserFriendshipRelationships(userA, userC)) {
				DebugHelper.newMsg(TAG, "Done checking friendship relationships of the user A...");
			} else {
				DebugHelper.newMsg(TAG, "Failed to load friendship relationships for the user A ...");
				unitTestResult = false;
			}

			// Delete frienship relationships
			if (await mUserRelController.unfriend(userA.id, userB.id)) {
				DebugHelper.newMsg(TAG, "Delete friendship relationship between users A and B");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete friendship relationship between users A and B");
				unitTestResult = false;
			}

			if (await mUserRelController.unfriend(userA.id, userC.id)) {
				DebugHelper.newMsg(TAG, "Delete friendship relationship between users A and C");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete friendship relationship between users A and C");
				unitTestResult = false;
			}

			if (await mUserRelController.unfriend(userA.id, userD.id)) {
				DebugHelper.newMsg(TAG, "Delete friendship relationship between users A and D");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete friendship relationship between users A and D");
				unitTestResult = false;
			}

			if (await mUserRelController.unfriend(userA.id, userE.id)) {
				DebugHelper.newMsg(TAG, "Delete friendship relationship between users A and E");
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete friendship relationship between users A and E");
				unitTestResult = false;
			}

			// Delete test users' coordinates
			if (await mUserController.deleteUserCoordinate(userA.id)) {
				if (await mUserController.deleteUserCoordinate(userB.id)) {
					if (await mUserController.deleteUserCoordinate(userC.id)) {
						if (await mUserController.deleteUserCoordinate(userD.id)) {
							if (await mUserController.deleteUserCoordinate(userE.id)) {
								DebugHelper.newMsg(TAG, "Delete test users' coordinates.");
							} else {
								DebugHelper.newMsg(TAG, "Failed to delete test users' coordinates.");
								unitTestResult = false;
							}
						} else {
							DebugHelper.newMsg(TAG, "Failed to delete test users' coordinates.");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to delete test users' coordinates.");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to delete test users' coordinates.");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to delete test users' coordinates.");
				unitTestResult = false;
			}

			// Delete test users
			if (await mUserController.deleteUser(userA.id)) {
				if (await mUserController.deleteUser(userB.id)) {
					if (await mUserController.deleteUser(userC.id)) {
						if (await mUserController.deleteUser(userD.id)) {
							if (await mUserController.deleteUser(userE.id)) {
								DebugHelper.newMsg(TAG, "Test users were successfully deleted");
							} else {
								DebugHelper.newMsg(TAG, "Failed to delete user E ...");
								unitTestResult = false;
							}
						} else {
							DebugHelper.newMsg(TAG, "Failed to create user D ...");
							unitTestResult = false;
						}
					} else {
						DebugHelper.newMsg(TAG, "Failed to create user C ...");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to create user B ...");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to create user A ...");
				unitTestResult = false;
			}

			string testResult = "";

			if (unitTestResult) {
				testResult = "SUCCESS";
			} else {
				testResult = "FAILED";
			}

			DebugHelper.newMsg(TAG, string.Format(@"Finishing User Friendship status' test: {0}", testResult));

			return unitTestResult;
		}

		public async Task<bool> checkUserFriendshipRelationships(User userA, User UserC) {
			// Load the user relationships, all the lists must be null, but the recommended friends list
			if (await mUserRelController.loadUserRelationships(userA)) {

				// Show the list of friends of the current user
				if (mUserRelController.userFriends.Count > 0) {
					foreach (User user in mUserRelController.userFriends) {
						DebugHelper.newMsg(TAG, "Friends: " + user.name);
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get the list of friends");
				}

				// Show the list of users waiting for the current user approval
				if (mUserRelController.pendingCurrentUserApproval.Count > 0) {
					foreach (User user in mUserRelController.pendingCurrentUserApproval) {
						DebugHelper.newMsg(TAG, "Waiting for current user approval: " + user.name);
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get the list of users who are waiting for the current user approval");
				}

				// Show the list of users from who the current user is waiting approval
				if (mUserRelController.pendingInvitation.Count > 0) {
					foreach (User user in mUserRelController.pendingInvitation) {
						DateTime? lastInterDate = mUserRelController.getLatestInteractionDate(user.id, Constants.PENDING);

						if (lastInterDate != null) {
							DebugHelper.newMsg(TAG, string.Format(@"Waiting approval from user {0} invitation sent at {1}", 
							                                      user.name, lastInterDate.ToString()));
						} else {
							DebugHelper.newMsg(TAG, "Waiting approval from user: " + user.name);
						}
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get the list of users that haven't responded to the current user's friendship request");
				}

				// Show the list of recommended friends
				if (mUserRelController.recommendedFriends.Count > 0) {
					foreach (User user in mUserRelController.recommendedFriends) {
						DebugHelper.newMsg(TAG, "Recommended friend: " + user.name);
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get the list of recommended friends");
				}

				// Show the list of friends of the C user 
				List<User> friendsOfUserC = await mUserRelController.getListUserFriends(UserC);

				if (friendsOfUserC != null){
					if (friendsOfUserC.Count > 0) {
						foreach (User user in friendsOfUserC) {
							DebugHelper.newMsg(TAG, "Friend of UserC: " + user.name);
						}
					} else {
						DebugHelper.newMsg(TAG, "List of friends of the user C is empty");
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to get the list of friends of the user C");
				}

			} else {
				DebugHelper.newMsg(TAG, "Failed to load the user's relationships ...");
				unitTestResult = false;
			}

			return unitTestResult;
		}

		public int searchUser(string searchTerms, string currentUserId) {
			if (mUserRelController.loadSearchResults(searchTerms, currentUserId)) {
				if (mUserRelController.searchResults.Count > 0) {
					foreach (User user in mUserRelController.searchResults) {
						DebugHelper.newMsg(TAG, "Search results: " + user.name);
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to load search results ...");
				}

				return mUserRelController.searchResults.Count;
			} else {
				DebugHelper.newMsg(TAG, "Failed to load search results ...");
			}

			return -1;
		}

	}

}