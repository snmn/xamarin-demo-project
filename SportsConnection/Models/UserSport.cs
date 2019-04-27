// To add offline sync support: add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 

#define OFFLINE_SYNC_ENABLED
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

namespace SportsConnection {
	
	public class UserSport : AzureObject {
		
		private string mId;
		private string mUserId;
		private string mSportId;

		[JsonProperty(PropertyName = "id")]
		public string id {
			get {
				return mId;
			}
			set {
				mId = value;
			}
		}

		[JsonProperty(PropertyName = "userId")]
		public string userId {
			get {
				return mUserId;
			}
			set {
				mUserId = value;
			}
		}

		[JsonProperty(PropertyName = "sportId")]
		public string sportId {
			get {
				return mSportId;
			}
			set {
				mSportId = value;
			}
		}

		public string toString() {
			return "UserSport object (" + id + "): ";
		}

	}


	public class UserSportWrapper {

		private UserSport mUserSports;
		private User mUser;
		private Sport mSport;

		public UserSport core {
			get {
				return mUserSports;
			}
			set {
				mUserSports = value;
			}
		}

		public User user {
			get {
				return mUser;
			}
			set {
				mUser = value;
			}
		}

		public Sport sport {
			get {
				return mSport;
			}
			set {
				mSport = value;
			}
		}

	}


	/// <summary>
	/// This class manages instances of the UserSports class
	/// </summary>
	public class UserSportManager {

		private string TAG = "UserSportManager";

		#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<UserSport> userSportTable;
		#else
		IMobileServiceTable<UserSport> userSportTable;
		#endif

		public UserSportManager() {
			#if OFFLINE_SYNC_ENABLED
			userSportTable = AzureController.client.GetSyncTable<UserSport>();
			#else
			userSportTable = AzureController.client.GetTable<UserSport>();
			#endif
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return userSportTable is IMobileServiceSyncTable<UserSport>;
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

				await userSportTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCUserSports",
					userSportTable.CreateQuery());
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
		/// Get all UserSport of a user
		/// </summary>
		/// <returns>A list of UserSports objects</returns>
		public async Task<List<UserSport>> getUserSports(string userId, bool syncItems = false) {
			try {
				#if OFFLINE_SYNC_ENABLED
				if (syncItems) {
					await syncAsync();
				}
				#endif

				List<UserSport> userSports = await userSportTable.Where(
					userSport => (userSport.userId == userId)
				).ToListAsync();

				return new List<UserSport>(userSports);

			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));

			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a UserSport by the userId and the sportId
		/// </summary>
		/// <returns>A UserSport or null</returns>
		public async Task<UserSport> getUserSportByIds(string userId, string sportId) {
			try {
				List<UserSport> userSports = await userSportTable.Where(
					userSport => (userSport.userId == userId && userSport.sportId == sportId)
				).ToListAsync();

				if (userSports != null) {
					foreach (UserSport userSport in userSports) {
						return userSport;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get user sport for user {0} and sport {1} from Azure backend", userId, sportId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Upsert a UserSport
		/// </summary>
		/// <returns>True if the UserSport was saved</returns>
		/// <param name="userSport">A UserSport</param>
		public async Task<bool> saveUserSportAsync(UserSport userSport) {
			try {
				if ((userSport.userId != null) && (userSport.sportId != null)) {
					UserSport updateUserSport = await getUserSportByIds(userSport.userId, userSport.sportId);

					if (updateUserSport == null) {
						await userSportTable.InsertAsync(userSport);

						#pragma warning disable CS4014
						await syncAsync();
						#pragma warning restore CS4014

						return true;

					} else {
						DebugHelper.newMsg(TAG, "A UserSport object with the same ids already exists");
						return false;
					}
				} else {
					DebugHelper.newMsg(TAG, "The user id and the sport id cannot be null");
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, "Failed to save the UserSport object {0} in Azure backend");
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

		/// <summary>
		/// Deletes a UserSport async.
		/// </summary>
		/// <returns>The UserSport async.</returns>
		public async Task<bool> deleteUserSportAsync(UserSport userSport) {
			try {
				if ((userSport.id != null) && (userSport.sportId != null)) {
					UserSport deleteUserSport = await getUserSportByIds(userSport.userId, userSport.sportId);

					if (deleteUserSport != null) {
						await userSportTable.DeleteAsync(userSport);

						#pragma warning disable CS4014
						await syncAsync();
						#pragma warning restore CS4014

					} else {
						DebugHelper.newMsg(TAG, "The UserSport relationship doesn't exist");
					}

					return true;
				} else {
					DebugHelper.newMsg(TAG, "The user id and the sport id cannot be null");
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to delete UserSport with id {0} from Azure backend", userSport.id));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

	}

}