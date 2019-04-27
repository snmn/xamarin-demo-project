// To add offline sync support: add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 

#define OFFLINE_SYNC_ENABLED
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System.Linq;

#if OFFLINE_SYNC_ENABLED
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif


namespace SportsConnection {

	public class UserRelation {

		[JsonProperty(PropertyName = "id")]
		public string id {
			get; set;
		}

		[JsonProperty(PropertyName = "userIdA")]
		public string userIdA {
			get; set;
		}

		[JsonProperty(PropertyName = "userIdB")]
		public string userIdB {
			get; set;
		}

		[JsonProperty(PropertyName = "statusA")]
		public string statusA {
			get; set;
		}

		[JsonProperty(PropertyName = "statusB")]
		public string statusB {
			get; set;
		}

		[JsonProperty(PropertyName = "interactionDateA")]
		public DateTime interactionDateA {
			get; set;
		}

		[JsonProperty(PropertyName = "interactionDateB")]
		public DateTime interactionDateB {
			get; set;
		}

	}

	public class UserRelationWrapper {

		public UserRelation core {
			get; set;
		}

		public User userA {
			get; set;
		}
		public User userB {
			get; set;
		}

	}


	/// <summary>
	/// This class manages instances of the UserRelation class
	/// </summary>
	public class UserRelationManager {

		private string TAG = "UserRelationManager";

		private MobileServiceCollection<UserRelation, UserRelation> userRelations;
		public Dictionary<string, UserRelation> userFriends = new Dictionary<string, UserRelation>();
		public Dictionary<string, UserRelation> pendingCurrentUserApproval = new Dictionary<string, UserRelation>();
		public Dictionary<string, UserRelation> pendingInvitation = new Dictionary<string, UserRelation>();
		public Dictionary<string, UserRelation> blockedByCurrentUser = new Dictionary<string, UserRelation>();

		public List<string> interactionsUserIds = new List<string>();
		public Dictionary<string, User> interactionsUsers = new Dictionary<string, User>();


		#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<UserRelation> userRelationTable;
		#else
		IMobileServiceTable<UserRelation> userRelationTable;
		#endif

		public UserRelationManager() {
			#if OFFLINE_SYNC_ENABLED
			userRelationTable = AzureController.client.GetSyncTable<UserRelation>();
			#else
			userRelationTable = AzureController.client.GetTable<UserRelation>();
			#endif
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return userRelationTable is IMobileServiceSyncTable<UserRelation>;
				#else
				return false;
				#endif
			}
		}

		#if OFFLINE_SYNC_ENABLED
		public async Task syncAsync() {
			ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

			try {
				await AzureController.client.SyncContext.PushAsync();

				await userRelationTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCUserRelation",
					userRelationTable.CreateQuery());
			} catch (MobileServicePushFailedException exc) {
				if (exc.PushResult != null) {
					syncErrors = exc.PushResult.Errors;
				}
			} 

			if (syncErrors != null) {
				foreach (var error in syncErrors) {
					await error.CancelAndDiscardItemAsync();
					DebugHelper.newMsg(TAG, string.Format(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.",
														  error.TableName, error.Item["id"]));
				}
			}
		}
		#endif

		/// <summary>
		/// Get all relationships of the current user with other users 
		/// </summary>
		public async Task<bool> loadUserRelationships(User currentUser) {
			resetHelperCollections();
			List<UserRelation> userRelationList = new List<UserRelation>();

			try {
				await syncAsync();

				userRelations = await userRelationTable.Where(UserRelation =>
						UserRelation.userIdA == currentUser.uid ||
						UserRelation.userIdB == currentUser.uid
				).ToCollectionAsync();

				if (userRelations != null) {

					foreach (UserRelation ur in userRelations) {
						userRelationList.Add(ur);

						// Current user is the userA in the relation being processed
						if (ur.userIdA == currentUser.uid) {
							// User friends
							if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.FRIENDS_STR)) {
								if (!userFriends.ContainsKey(ur.userIdB)) {
									userFriends.Add(ur.userIdB, ur);
								}
							}

							// Pending current user approval
							if ((ur.statusA == Constants.PENDING_STR) && (ur.statusB == Constants.PENDING_STR)) {
								if (!pendingCurrentUserApproval.ContainsKey(ur.userIdB)) {
									pendingCurrentUserApproval.Add(ur.userIdB, ur);
								}
							}

							// Invitation made to another user that are on a 'Pending' status
							if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.PENDING_STR)) {
								if (!pendingInvitation.ContainsKey(ur.userIdB)) {
									pendingInvitation.Add(ur.userIdB, ur);
								}
							}

							// Users blocked by the current user
							if (ur.statusB == Constants.BLOCKED_STR) {
								if (!blockedByCurrentUser.ContainsKey(ur.userIdB)) {
									blockedByCurrentUser.Add(ur.userIdB, ur);
								}
							}

							interactionsUserIds.Add(ur.userIdB);

						} else {
							// Current user is the userB in the relation being processed.
							// User friends
							if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.FRIENDS_STR)) {
								if (!userFriends.ContainsKey(ur.userIdA)) {
									userFriends.Add(ur.userIdA, ur);
								}
							}

							// Pending current user approval
							if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.PENDING_STR)) {
								if (!pendingCurrentUserApproval.ContainsKey(ur.userIdA)) {
									pendingCurrentUserApproval.Add(ur.userIdA, ur);
								}
							}

							// Invitation made to another user that are on a 'Pending' status
							if ((ur.statusA == Constants.PENDING_STR) && (ur.statusB == Constants.FRIENDS_STR)) {
								if (!pendingInvitation.ContainsKey(ur.userIdA)) {
									pendingInvitation.Add(ur.userIdA, ur);
								}
							}

							// Users blocked by the current user
							if (ur.statusA == Constants.BLOCKED_STR) {
								if (!blockedByCurrentUser.ContainsKey(ur.userIdA)) {
									blockedByCurrentUser.Add(ur.userIdA, ur);
								}
							}

							interactionsUserIds.Add(ur.userIdA);
						}
					}

					// Load all the user objects that interacted in some way with the user and let them available for
					// future queries
					if (await loadInteractionsUsers()) {
						return true;
					} else {
						DebugHelper.newMsg(TAG, "Failed to load the user interactions and their users");
						return false;
					}
				}

			} catch (Exception e) {
				DebugHelper.newMsg(TAG, "Failed to load the user interactions and their users" + e.Message);
			}

			return false;
		}

		/// <summary>
		/// Load the list of user objects that somehow are related to the current user;
		/// </summary>
		public async Task<bool> loadInteractionsUsers() {
			try {
				UserManager userManager = new UserManager();	
				List<User> users = await userManager.getListUsersByIds(interactionsUserIds);

				if (users != null) {
					foreach (User user in users) {
						if (!interactionsUsers.ContainsKey(user.uid)) {
							interactionsUsers.Add(user.uid, user);
						}
					}
				}

				return true;
			} catch (Exception e) {
				DebugHelper.newMsg("Failed to load list of users who interacted with the current user: ", e.Message);
			}

			return false;
		}

		/// <summary>
		/// Get a user relation based on both users.
		/// </summary>
		public async Task<UserRelation> getRelationUsersAB(string userIdA, string userIdB) {
			UserRelation userABRelation = null;

			try {
				userRelations = await userRelationTable.Where(UserRelation => (
					(UserRelation.userIdA == userIdA && UserRelation.userIdB == userIdB) ||
					(UserRelation.userIdB == userIdA && UserRelation.userIdA == userIdB)
				)).ToCollectionAsync();

				if (userRelations.Count > 0) {
					UserRelation ur = userRelations.ElementAt(0);
				}

				foreach(UserRelation ur in userRelations) {
					if (ur != null) {
						await deleteUserRelation(ur);
					}
				}

			} catch (Exception e) {
				DebugHelper.newMsg("UserRelation-getABRelation: ", e.Message);
			}

			return userABRelation;
		}

		/// <summary>
		/// Get the list of friends of a user
		/// </summary>
		public async Task<Dictionary<string, UserRelation>> getUserFriends(User user) {
			try {
				List<UserRelation> userFriendRelations = await userRelationTable.Where(
					 userRelation => 
					(userRelation.userIdA == user.uid || userRelation.userIdB == user.uid)
				).ToListAsync();

				if (userFriendRelations != null) {
					
					foreach (UserRelation ur in userFriendRelations) {

						if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.FRIENDS_STR)) {

							// Current user is the userA in the relation being processed.
							if (ur.userIdA == user.uid) {
								userFriends.Add(ur.userIdB, ur);
							}
							// Current user is the userB in the relation being processed.
							else {
								userFriends.Add(ur.userIdA, ur);
							}
						}
					}

					return userFriends;
				}
			} catch (Exception e) {
				DebugHelper.newMsg("Failed to get UserRelation: ", e.Message);
			}

			return null;
		}

		/// <summary>
		/// Get the list of friends of a user, without saving the results in a local variable
		/// </summary>
		public async Task<List<string>> getFriendsOfUser(User user) {
			List<string> userFriendsIds = new List<string>();

			try {
				List<UserRelation> userFriendRelations = await userRelationTable.Where(
					 userRelation =>
					(userRelation.userIdA == user.uid || userRelation.userIdB == user.uid) &&
					(userRelation.statusA == Constants.FRIENDS_STR && userRelation.statusB == Constants.FRIENDS_STR)
				).ToListAsync();

				if (userFriendRelations != null) {

					foreach (UserRelation ur in userFriendRelations) {

						if ((ur.statusA == Constants.FRIENDS_STR) && (ur.statusB == Constants.FRIENDS_STR)) {

							// Current user is the userA in the relation being processed.
							if (ur.userIdA == user.uid) {
								userFriendsIds.Add(ur.userIdB);
							}
							// Current user is the userB in the relation being processed.
							else {
								userFriendsIds.Add(ur.userIdA);
							}
						}
					}

					return userFriendsIds;
				}
			} catch (Exception e) {
				DebugHelper.newMsg("Failed to get UserRelation: ", e.Message);
			}

			return null;
		}

		/// <summary>
		/// Create a new user relationship with the other user and sets the status of the current user to 'Friend'.
		/// Once the invited user accepts the invitation, the status of both users in the relationship will 
		/// be 'Friend'.
		/// </summary>
		public async Task<bool> addNewFriend(User currentUser, User addFriend) {
			UserRelation userRelation = await getRelationUsersAB(currentUser.uid, addFriend.uid);

			// If the relation already exists, identify who is the current user in the relationship 
			// A or B and update the relationship status
			if (userRelation != null) {
				bool result;

				if (userRelation.userIdA == currentUser.uid) {
					result = await updateRelation(userRelation, Constants.FRIENDS_STR, userRelation.statusB);
				} else {
					result = await updateRelation(userRelation, userRelation.statusA, Constants.FRIENDS_STR);
				}

				return result;

			} else {
				UserRelation newUserRelation = new UserRelation {
					userIdA = currentUser.uid,
					userIdB = addFriend.uid,
					statusA = Constants.FRIENDS_STR,
					statusB = Constants.PENDING_STR,
					interactionDateA = DateTime.UtcNow,
					interactionDateB = DateTime.UtcNow
				};

				try {
					await userRelationTable.InsertAsync(newUserRelation);
					await syncAsync();

					return true;
				} catch (Exception e) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed to add user {0} as friend of the current user", addFriend.name));
					DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
				}
			}

			return false;
		}

		/// <summary>
		/// Update the status of both users in a given relationship.
		/// </summary>
		public async Task<bool> updateRelation(UserRelation oldUserRelation, string relationshipStatusA,
			string relationshipStatusB) {

			UserRelation updatedUserRelation = new UserRelation {
				id = oldUserRelation.id,
				userIdA = oldUserRelation.userIdA,
				userIdB = oldUserRelation.userIdB,
				statusA = relationshipStatusA,
				statusB = relationshipStatusB,
				interactionDateA = DateTime.UtcNow,
				interactionDateB = oldUserRelation.interactionDateB
			};

			try {
				await userRelationTable.UpdateAsync(updatedUserRelation);
				await syncAsync();

				return true;
			} catch (Exception e) {
				DebugHelper.newMsg("Error while updating the user relation ", e.Message);
			}

			return false;
		}

		/// <summary>
		/// Deletes a relationship between two users
		/// </summary>
		public async Task<bool> deleteUserRelation(UserRelation userRelation) {
			if (userRelation != null) {
				if (userRelation.id != null) {
					try {
						await userRelationTable.DeleteAsync(userRelation);
						await syncAsync();
						await userRelationTable.PurgeAsync();

						return true;
					} catch (Exception e) {
						DebugHelper.newMsg("Error while updating a user relation ", e.Message);
					}

					return false;
				} else {
					DebugHelper.newMsg("Error while updating a user relation ", "The userRelation id is mandatory");
					return false;
				}
			} else {
				DebugHelper.newMsg("Error while updating a user relation ", "The userRelation cannot be null");
				return false;
			}
		}

		/// <summary>
		/// Delete a user relation that belongs to the user A and the user B.
		/// </summary>
		public async Task<bool> deleteUserRelation(string userUidA, string userUidB) {
			try {
				UserRelation userRelation = await getRelationUsersAB(userUidA, userUidB);

				if (userRelation != null) {
					await deleteUserRelation(userRelation);
					await syncAsync();

					return true;
				} else {
					DebugHelper.newMsg(TAG, "The userRelation cannot be null");
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, "The userRelation cannot be null" + e.Message);
			}

			return false;
		}

		/// <summary>
		/// Try to get a User by searching its id on the list of interactions of the current user
		/// </summary>
		public User getInteractionUserById(string uid) {
			User outUser = null;

			if (interactionsUsers.TryGetValue(uid, out outUser)) {
				return outUser;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Refresh the collections.
		/// </summary>
		public void resetHelperCollections() {
			userFriends = new Dictionary<string, UserRelation>();
			pendingCurrentUserApproval = new Dictionary<string, UserRelation>();
			pendingInvitation = new Dictionary<string, UserRelation>();
			blockedByCurrentUser = new Dictionary<string, UserRelation>();
		}

	}

}