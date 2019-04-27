using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace SportsConnection {

	/** 
	 * These are the collections managed by the UserRelationshipsController and their relationship to the 
	 * different user relationship status
	 * 
	 *		allRelationships           -> all relationships, no filter
	 *		userFriends                -> (Status_A == "Friends") && (Status_B == "Friends")
	 *		pendingCurrentUserApproval -> (Status_A = "Pending") && (Status_B == "Friends")
	 *		pendingInvitation          -> (Status_A = "Friends") && (Status_B == "Pending")
	 *		recommendedFriends         -> (!userFriends && !pendingInvitation && !pendingCurrentUserApproval)
	 *		facebookFriends            -> a list of the user's facebook friends returned by the Graph API
	 *		searchResults              -> a subset of recommendedFriends
	 *
	 *	Possible status of the users on the relationships:
	 *		1. Friends - Friends
     *      2. Friends - Pending
     *      3. Pending - Friends
     *      4. Blocked - Blocked
	 */
	public class UserRelationshipsController {

		// Managers
		public static UserRelationManager userRelationManager;

		// Model objects
		public ObservableCollection<User> userFriends = new ObservableCollection<User>();
		public ObservableCollection<User> pendingCurrentUserApproval = new ObservableCollection<User>();
		public ObservableCollection<User> pendingInvitation = new ObservableCollection<User>();
		public ObservableCollection<User> recommendedFriends = new ObservableCollection<User>();
		public ObservableCollection<User> facebookFriends = new ObservableCollection<User>();
		public ObservableCollection<User> searchResults = new ObservableCollection<User>();
		public List<User> tempExcludedFromSearchResults = new List<User>();

		public UserRelationshipsController() {
			userRelationManager = new UserRelationManager();	
		}

		/// <summary>
		/// Load the users in the relationships in Observable Collections according to the sort of relationship.
		/// </summary>
		/// <returns>True, if the users from the relationships were loaded</returns>
		public async Task<bool> loadUserRelationships(User user) {
			// Refresh the data variables
			resetUserRelationships();

			if (await userRelationManager.loadUserRelationships(user)) {

				// Load Friends
				foreach (KeyValuePair<string, UserRelation> kvp in userRelationManager.userFriends) {
					User userFriend = userRelationManager.getInteractionUserById(kvp.Key);

					if((!userFriends.Contains(userFriend)) && (userFriend != null)) {
						userFriends.Add(userFriend);
					}
				}

				// Load the list of users that are waiting for the current user approval
				foreach (KeyValuePair<string, UserRelation> kvp in userRelationManager.pendingCurrentUserApproval) {
					User userWaitingApproval = userRelationManager.getInteractionUserById(kvp.Key);

					if ((!pendingCurrentUserApproval.Contains(userWaitingApproval)) && (userWaitingApproval != null)) {
						pendingCurrentUserApproval.Add(userWaitingApproval);
					}
				}

				// Load the list of users to whom the current user has sent a friendship invitation and 
				// didn't get a reply yet
				foreach (KeyValuePair<string, UserRelation> kvp in userRelationManager.pendingInvitation) {
					User snobyUser = userRelationManager.getInteractionUserById(kvp.Key);

					if ((!pendingInvitation.Contains(snobyUser)) && (snobyUser != null)) {
						pendingInvitation.Add(snobyUser);
					}
				}

				// Load the list of recommended friends, the search universe for new users  
				await loadRecommendedFriends(user.uid);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Fetches a list of recommended friends based on all the users of the system, their location and on 
		/// the current user relationships.
		/// </summary>
		/// <returns>True, if the recommended friends list was loaded</returns>
		public async Task<bool> loadRecommendedFriends(string currentUserId) {
			UserManager userManager = new UserManager();
			UserCoordinateManager userCoordinateManager = new UserCoordinateManager();

			List<UserCoordinate> allUserCoordinates = await userCoordinateManager.getUserCoordinatesAsync();
			UserCoordinate currentUserCoordinate = null;
			List<string> recommendedUserUids = new List<string>();

			if (allUserCoordinates != null) {
				
				if (allUserCoordinates.Count > 0) {

					currentUserCoordinate = userCoordinateManager.getUserCoordinateById(currentUserId);

					// Pass a basic filter on the user coordinates and get the id of the recommended friends
					foreach (UserCoordinate userCoordinate in allUserCoordinates) {
						bool isCurrentUser = userCoordinate.userUid == currentUserId;
						bool isFriend = userRelationManager.userFriends.ContainsKey(userCoordinate.userUid);
						bool isPendingFriendshipRequest = userRelationManager.pendingInvitation.ContainsKey(userCoordinate.userUid);
						bool isAnotherUserWaitingForApproval = userRelationManager.pendingCurrentUserApproval.ContainsKey(userCoordinate.userUid);
						bool isBlockedByCurrentUser = userRelationManager.blockedByCurrentUser.ContainsKey(userCoordinate.userUid);

						if (!isCurrentUser && !isFriend && !isPendingFriendshipRequest && 
						    !isAnotherUserWaitingForApproval && !isBlockedByCurrentUser) {
							UserCoordinate anotherUserCoordinate = userCoordinateManager.getUserCoordinateById(userCoordinate.userUid);

							if (currentUserCoordinate != null && anotherUserCoordinate != null) {
								recommendedUserUids.Add(userCoordinate.userUid);
							}
						}
					}

					// Get the actual recommended User objects and order the list by their distance from the current user
					if (recommendedUserUids.Count > 0 && currentUserCoordinate != null) {
						List<User> unsortedUsersList = await userManager.getListUsersByIds(recommendedUserUids);
						List<User> sortedUsersList = new List<User>();

						// Set the distance between the users and the current user
						foreach (User user in unsortedUsersList) {
							UserCoordinate userCoordB = userCoordinateManager.getUserCoordinateById(user.uid);
							//user.distanceCurUser = getDistanceFromCoordAToB(currentUserCoordinate, userCoordB);
						}

						// Sort the recommended users list by distance
					//	sortedUsersList = unsortedUsersList.OrderBy(user => user.distanceCurUser).ToList();

						// Add the recommended users to the Observable collection of recommended users
						foreach (User user in sortedUsersList) {
							recommendedFriends.Add(user);
						}
					} else {
						return false;
					}
				} else {
					return false;
				}
			} else {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates the distance between two coordinates considering the curvature of the earth.
		/// </summary>
		/// <returns>The distance in meters from the coordinate A to the coordinate B</returns>
		/// <param name="coordA">Coordinate a.</param>
		/// <param name="coordB">Coordinate b.</param>
		private double getDistanceFromCoordAToB(UserCoordinate coordA, UserCoordinate coordB) {
			double coordALatitude = FormatUtils.stringToDouble(coordA.latitude);
			double coordALongitude = FormatUtils.stringToDouble(coordA.longitude);
			double coordBLatitude = FormatUtils.stringToDouble(coordB.latitude);
			double coordBLongitude = FormatUtils.stringToDouble(coordB.longitude);

			return GeoCoordinatesUtils.distance(coordALatitude, coordALongitude, 
			                                    coordBLatitude, coordBLongitude, 
			                                    Constants.DISTANCE_MEASURE);
		}

		public async Task<List<User>> getListUserFriends(User user) {
			if (user != null) {
				UserManager userManager = new UserManager();
				List<string> userFriendsIds = await userRelationManager.getFriendsOfUser(user);
				List<User> friends = await userManager.getListUsersByIds(userFriendsIds);

				return friends;
			}

			return null;
		}

		public DateTime? getLatestInteractionDate(string userUid, int relationshipType) {
			User interactionUser = userRelationManager.getInteractionUserById(userUid);
			UserRelation userRelation = null;

			if (interactionUser != null) {
				switch (relationshipType) {
					case Constants.FRIENDS:
						if (userRelationManager.userFriends.ContainsKey(interactionUser.uid)) {
							userRelation = userRelationManager.userFriends[interactionUser.uid];
						}
						break;

					case Constants.PENDING:
						if (userRelationManager.pendingInvitation.ContainsKey(interactionUser.uid)) {
							userRelation = userRelationManager.pendingInvitation[interactionUser.uid];
						}
						break;	

					case Constants.BLOCKED:
						if (userRelationManager.blockedByCurrentUser.ContainsKey(interactionUser.uid)) {
							userRelation = userRelationManager.blockedByCurrentUser[interactionUser.uid];
						}
						break;

					default:
						DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, 
					                       "Could not determine the type of the relationship");
						break;
				}

				if (userRelation != null) {
					int datesComparison = DateTime.Compare(userRelation.interactionDateA, userRelation.interactionDateB);

					if (datesComparison < 0) {
						return userRelation.interactionDateB;
					} else if (datesComparison >= 0) {
						return userRelation.interactionDateA;
					}
				}
			}

			return null;
		}

		public async Task<bool> addOrConfirmFriend(User currentUser, User addFriend) {
			if ((currentUser.uid != null && addFriend.uid != null) && (currentUser != null && addFriend != null)) {
				return await userRelationManager.addNewFriend(currentUser, addFriend);
			}

			return false;
		}

		public async Task<bool> unfriend(string userUidA, string userUidB) {
			if (userUidA != null && userUidB != null) {
				return await userRelationManager.deleteUserRelation(userUidA, userUidB);
			}

			return false;
		}

		public async Task<bool> blockFriend(string userUidA, string userUidB) {
			if (userUidA != null && userUidB != null) {
				UserRelation oldUserRelation = await userRelationManager.getRelationUsersAB(userUidA, userUidB);

				if (oldUserRelation != null) {
					return await userRelationManager.updateRelation(oldUserRelation, Constants.BLOCKED_STR, 
					                                                Constants.BLOCKED_STR);
				}
			}

			return false;
		}

		/// <summary>
		/// Try to get users that match the search terms and add them to an Observable collection, if
		/// the list was loaded.
		/// </summary>
		/// <returns> 
		/// True, if a list with the search results was loaded in the 'searchResults' ObeservableCollection
		/// </returns>
		/// <param name="searchTerms">Search terms</param>
		public bool loadSearchResults(string searchTerms, string currentUserUid) {
			try {
				if ((recommendedFriends.Count > 0) && (searchTerms != null)) {
					searchResults.Clear();
					var userReinsertedIntoSearchResults = new List<int>();
					var countTempRemovedItems = 0;

					foreach (User user in tempExcludedFromSearchResults) {
						var searchIsInNameForTemp = user.name.IndexOf(searchTerms, StringComparison.OrdinalIgnoreCase) >= 0;

						if (searchIsInNameForTemp && (user.uid != currentUserUid)) {

							if (!searchResults.Contains(user)) {
								searchResults.Add(user);
								userReinsertedIntoSearchResults.Add(countTempRemovedItems);
							}
						}

						countTempRemovedItems++;
					}

					foreach (User user in recommendedFriends) {
						var searchIsInName = user.name.IndexOf(searchTerms, StringComparison.OrdinalIgnoreCase) >= 0;

						if (searchIsInName && (user.uid != currentUserUid)) {

							if (!searchResults.Contains(user)) {
								searchResults.Add(user);
							}

						} else if (!tempExcludedFromSearchResults.Contains(user)) {
							tempExcludedFromSearchResults.Add(user);
						}
					}

					foreach (int position in userReinsertedIntoSearchResults) {
						tempExcludedFromSearchResults.RemoveAt(position);
					}

					if (searchResults.Count > 0) {
						return true;
					}
				}

				return false;
			} catch (Exception e) {
				e.ToString();
				return false;
			}
		}

		public void removeUserFromSearchResultsAndRecommendedFriends(User user) {
			if (recommendedFriends != null && searchResults != null) {
				if (recommendedFriends.Contains(user)) {
					recommendedFriends.Remove(user);
				}

				if (searchResults.Contains(user)) {
					searchResults.Remove(user);
				}
			}
		}

		/// <summary>
		/// Clear all the controller collections
		/// </summary>
		private void resetUserRelationships() {
			userFriends.Clear();
			pendingCurrentUserApproval.Clear();
			pendingInvitation.Clear();
			recommendedFriends.Clear();
			searchResults.Clear();
			facebookFriends.Clear();
			tempExcludedFromSearchResults.Clear();

			// Clear the manager's collections
			userRelationManager.resetHelperCollections();
		}

	}

}