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

#if OFFLINE_SYNC_ENABLED
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

namespace SportsConnection {
	
	public class UserFavoriteLocation : AzureObject {

		private string mId;
		private string mUserId;
		private string mFavoriteLocationId;

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

		[JsonProperty(PropertyName = "locationId")]
		public string locationId {
			get {
				return mFavoriteLocationId;
			}
			set {
				mFavoriteLocationId = value;
			}
		}

		// Utility methods
		public string toString() {
			return "UserFavoriteLocation object (" + id + ")";
		}

	}


	public class UserFavoriteLocationWrapper {

		private UserFavoriteLocation mUserFavoriteLocation;
		private User mUser;
		private Location mLocation;

		public UserFavoriteLocation core {
			get {
				return mUserFavoriteLocation;
			}
			set {
				mUserFavoriteLocation = value;
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

		public Location location {
			get {
				return mLocation;
			}
			set {
				mLocation = value;
			}
		}

		// Utility methods
		public string toString() {
			return "UserFavoriteLocation (" + user + ")" + " location (" + location + ")";
		}

	}


	/// <summary>
	/// This class manages instances of the UserFavoriteLocation class
	/// </summary>
	public class UserFavoriteLocationManager {

		private string TAG = "UserFavoriteLocationManager";

		#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<UserFavoriteLocation> userFavoriteLocationTable;
		#else
		IMobileServiceTable<UserFavoriteLocation> userFavoriteLocationTable;
		#endif

		public UserFavoriteLocationManager() {
			#if OFFLINE_SYNC_ENABLED
			userFavoriteLocationTable = AzureController.client.GetSyncTable<UserFavoriteLocation>();
			#else
			userFavoriteLocationTable = AzureController.client.GetTable<UserFavoriteLocation>();
			#endif
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return userFavoriteLocationTable is IMobileServiceSyncTable<UserFavoriteLocation>;
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

				await userFavoriteLocationTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCUserFavoriteLocations",
					userFavoriteLocationTable.CreateQuery());
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
		/// Get all UserFavoriteLocations
		/// </summary>
		/// <returns>A list of UserFavoriteLocations</returns>
		/// <param name="syncItems">If set to <c>true</c> sync items.</param>
		public async Task<List<UserFavoriteLocation>> getUserFavoriteLocationsAsync(bool syncItems = false) {
			try {
				#if OFFLINE_SYNC_ENABLED
				if (syncItems) {
					await syncAsync();
				}
				#endif

				List<UserFavoriteLocation> userFavoriteLocations = await userFavoriteLocationTable.ToListAsync();

				return new List<UserFavoriteLocation>(userFavoriteLocations);

			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));

			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a UserFavoriteLocation by its id
		/// </summary>
		/// <returns>A UserFavoriteLocation or null</returns>
		/// <param name="userFavoriteLocationId">A UserFavoriteLocation id</param>
		public async Task<UserFavoriteLocation> getUserFavoriteLocationById(string userFavoriteLocationId) {
			try {
				List<UserFavoriteLocation> userFavoriteLocations = await userFavoriteLocationTable.Where(
					userFavoriteLocation => (userFavoriteLocation.id == userFavoriteLocationId)
				).ToListAsync();

				if (userFavoriteLocations != null) {
					foreach (UserFavoriteLocation userFavLoc in userFavoriteLocations) {
						return userFavLoc;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get UserFavoriteLocation {0} from Azure backend", userFavoriteLocationId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a UserFavoriteLocation by the user and location ids
		/// </summary>
		/// <returns>A UserFavoriteLocation or null</returns>
		public async Task<UserFavoriteLocation> getUserFavoriteLocationByIds(string userId, string locationId) {
			try {
				List<UserFavoriteLocation> userFavoriteLocations = await userFavoriteLocationTable.Where(userFavoriteLocation => 
					(userFavoriteLocation.userId == userId) && 
					(userFavoriteLocation.locationId == locationId)
				).ToListAsync();

				if (userFavoriteLocations != null) {
					foreach (UserFavoriteLocation userFavoriteLocation in userFavoriteLocations) {
						return userFavoriteLocation;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get UserFavoriteLocation relationship between {0} , {1} from Azure backend",
													  userId, locationId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Upsert a UserFavoriteLocation
		/// </summary>
		/// <returns>True if the UserFavoriteLocation was saved</returns>
		/// <param name="userFavoriteLocation">A UserFavoriteLocation</param>
		public async Task<bool> saveUserFavoriteLocationAsync(UserFavoriteLocation userFavoriteLocation) {
			try {
				await syncAsync();

				UserFavoriteLocation createUserFavLoc = await getUserFavoriteLocationByIds(userFavoriteLocation.userId,
				                                                             userFavoriteLocation.locationId);
				
				if (createUserFavLoc == null) {
					await userFavoriteLocationTable.InsertAsync(userFavoriteLocation);
					return true;
				} else {
					DebugHelper.newMsg(TAG, "Failed to save user favorite location, relationship already exists in Azure backend");
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, "Failed to save user favorite location in Azure backend");
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

		/// <summary>
		/// Deletes a UserFavoriteLocation async
		/// </summary>
		/// <returns>The UserFavoriteLocation async</returns>
		public async Task<bool> deleteUserFavoriteLocationAsync(string userId, string locationId) {
			try {
				if ((userId != null) && (locationId != null)) {
					UserFavoriteLocation deleteUserFavoriteLocation = await getUserFavoriteLocationByIds(userId, locationId);

					if (deleteUserFavoriteLocation != null) {
						await userFavoriteLocationTable.DeleteAsync(deleteUserFavoriteLocation);

						#pragma warning disable CS4014
						await syncAsync();
						#pragma warning restore CS4014
					} else {
						DebugHelper.newMsg(TAG, string.Format(@"User Favorite Location relationship between user {0} and location {1} was already deleted", userId, locationId));
					}

					return true;
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to delete User Favorite Location relationship between user {0} and location {1}", userId, locationId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

	}

}