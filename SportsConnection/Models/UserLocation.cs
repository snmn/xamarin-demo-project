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
	
	public class UserLocation : AzureObject {

		private string mId;
		private string mUserId;
		private string mLocationId;


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
				return mLocationId;
			}
			set {
				mLocationId = value;
			}
		}

		[JsonProperty(PropertyName = "checkOut")]
		public bool checkOut {
			get; set;
		}

		// Utility methods
		public string toString() {
			return "UserLocation object (" + id + ")";
		}

	}

	public class UserLocationWrapper {

		private UserLocation mUserLocation;
		private User mUser;
		private Location mLocation;

		public UserLocation core {
			get {
				return mUserLocation;
			}
			set {
				mUserLocation = value;
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
			return "UserLocation user (" + user + ")" + " location (" + location + ")";
		}
	}


	/// <summary>
	/// This class manages instances of the UserLocation class
	/// </summary>
	public class UserLocationManager {

		private string TAG = "UserLocationManager";
		private bool mHasPurgedTable = false;


		#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<UserLocation> userLocationTable;
		#else
		IMobileServiceTable<UserLocation> userLocationTable;
		#endif

		public UserLocationManager() {
			#if OFFLINE_SYNC_ENABLED
			userLocationTable = AzureController.client.GetSyncTable<UserLocation>();
			#else
			userLocationTable = AzureController.client.GetTable<UserLocation>();
			#endif
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return userLocationTable is IMobileServiceSyncTable<UserLocation>;
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

				await userLocationTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCUserLocations",
					userLocationTable.CreateQuery());
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
		/// Get all UserLocations
		/// </summary>
		/// <returns>A list of UserLocations</returns>
		/// <param name="syncItems">If set to <c>true</c> sync items.</param>
		public async Task<List<UserLocation>> getLocationsAsync(bool syncItems = false) {
			try {
				#if OFFLINE_SYNC_ENABLED
				if (syncItems) {
					await syncAsync();
				}
				#endif
	
				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.checkOut == false)
				).ToListAsync();

				return new List<UserLocation>(userLocations);

			} catch (MobileServiceInvalidOperationException msioe) {
				if (msioe != null) {
					if (msioe.Message != null) {
						DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
					}
				}
			} catch (Exception e) {
				if (e != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
				}
			}

			return null;
		}

		/// <summary>
		/// Get a UserLocation by its id
		/// </summary>
		/// <returns>A Location or null</returns>
		/// <param name="userLocationId">A UserLocation id</param>
		public async Task<UserLocation> getUserLocationById(string userLocationId) {
			try {
				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.id == userLocationId && userLocation.checkOut == false)
				).ToListAsync();

				if (userLocations != null) {
					
					foreach (UserLocation userLocation in userLocations) {
						return userLocation;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				if (msioe != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
				}
			} catch (Exception e) {
				if (e != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed to get userLocation {0} from Azure backend", userLocationId));
					DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
				}
			}

			return null;
		}

		/// <summary>
		/// Get a List of all locations where the user is currently checked in
		/// </summary>
		/// <returns>A List of Locations or null</returns>
		public async Task<List<UserLocation>> getUserLocationsById(string userId) {
			try {
				// Implement a data consistency policy to clean up duplicated userLocation relationships
				var finalUserLocationList = new List<UserLocation>();
				var duplicatesList = new List<UserLocation>();
	
				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.userId == userId && userLocation.checkOut == false)
				).ToListAsync();

				if (userLocations != null) {
					
					foreach (UserLocation userLocation in userLocations) {
						
						if (finalUserLocationList.Contains(userLocation)) {
							duplicatesList.Add(userLocation);
						} else {
							finalUserLocationList.Add(userLocation);
						}
					}

					await cleanUpDuplicateRelationships(duplicatesList);

					return finalUserLocationList;
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				if (e.Message != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed {0}", e.Message));
				}
			}

			return null;
		}

		/// <summary>
		/// Get a List of all locations where the user is currently checked in
		/// </summary>
		/// <returns>A List of Locations or null</returns>
		public async Task<List<UserLocation>> getUsersCheckedIntoLocation(string locationId) {
			try {
				// Implement a data consistency policy to clean up duplicated userLocation relationships
				var finalUserLocationList = new List<UserLocation>();
				var duplicatesList = new List<UserLocation>();

				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.locationId == locationId && userLocation.checkOut == false)
				).ToListAsync();

				if (userLocations != null) {
					
					foreach (UserLocation userLocation in userLocations) {
						
						if (finalUserLocationList.Contains(userLocation)) {
							duplicatesList.Add(userLocation);
						} else {
							finalUserLocationList.Add(userLocation);
						}
					}

					await cleanUpDuplicateRelationships(duplicatesList);

					return finalUserLocationList;
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				if (e.Message != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed {0}", e.Message));
				}
			}

			return null;
		}

		/// <summary>
		/// Get the list of ids of all the locations where the user has checked in.
		/// </summary>
		/// <returns>A List of Locations or null</returns>
		public async Task<List<UserLocation>> getRecentUserLocations(string userUid) {
			try {
				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.userId == userUid)
				).OrderBy(
					userLocation => userLocation.UpdatedAt
				).ToListAsync();

				if (userLocations != null) {
					return userLocations;
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				if (e.Message != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed {0}", e.Message));
				}
			}

			return null;
		}

		/// <summary>
		/// Get a UserLocation by the user and location ids
		/// </summary>
		/// <returns>A Location or null</returns>
		public async Task<UserLocation> getUserLocationRelByIds(string userId, string locationId) {
			try {
				// Implement a data consistency policy to clean up duplicated userLocation relationships
				var finalUserLocationList = new List<UserLocation>();
				var duplicatesList = new List<UserLocation>();
				UserLocation searchedUserLocation = null;

				List<UserLocation> userLocations = await userLocationTable.Where(
					userLocation => (userLocation.locationId == locationId && userLocation.checkOut == false)
				).ToListAsync();

				if (userLocations != null) {
					
					foreach (UserLocation userLocation in userLocations) {
						
						if (userLocation.userId != null && userLocation.locationId != null) {
							
							if (userLocation.userId == userId && userLocation.locationId == locationId) {
								searchedUserLocation = userLocation;
							}

						} else if (finalUserLocationList.Contains(userLocation)) {
							duplicatesList.Add(userLocation);

						} else {
							finalUserLocationList.Add(userLocation);
						}
					}

					await cleanUpDuplicateRelationships(duplicatesList);

					return searchedUserLocation;
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				
				if (userId != null && locationId != null && e != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed to get userLocation relationship between {0} , {1} from Azure backend",userId, locationId));
					DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
				}
			}

			return null;
		}

		private async Task<bool> cleanUpDuplicateRelationships(List<UserLocation> duplicateRelationships) {
			if (duplicateRelationships != null) {
				
				foreach (UserLocation userLocation in duplicateRelationships) {
					
					if (userLocation.userId != null && userLocation.locationId != null) {
						await checkOutFromLocationAsync(userLocation.userId, userLocation.locationId);
					}
				}

				return true;
			} else {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to delete duplicated user location relationshiop"));
			}

			return false;
		}

		/// <summary>
		/// Upsert a Location.
		/// </summary>
		/// <returns>True if the UserLocation was saved</returns>
		/// <param name="userLocation">A UserLocation</param>
		public async Task<bool> checkInLocationAsync(UserLocation userLocation) {
			try {
				UserLocation createUserLocation = await getUserLocationRelByIds(userLocation.userId, userLocation.locationId);

				if (createUserLocation == null) {
					userLocation.checkOut = false;
					await userLocationTable.InsertAsync(userLocation);

					#if OFFLINE_SYNC_ENABLED
					await syncAsync();
					#endif

					return true;
				} else {
					DebugHelper.newMsg(TAG, string.Format(@"Failed to save user location, relationship already exists in Azure backend", userLocation.id));
				}
			} catch (Exception e) {
				
				if (e != null && userLocation != null) {
					
					if (userLocation.id != null) {
						DebugHelper.newMsg(TAG, string.Format(@"Failed to save user lcoation with id {0} in Azure backend", userLocation.id));
						DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));	
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Deletes a UserLocation async.
		/// </summary>
		/// <returns>The UserLocation async.</returns>
		public async Task<bool> checkOutFromLocationAsync(string userId, string locationId) {
			try {
				if ((userId != null) && (locationId != null)){
					UserLocation deleteUserLocation = await getUserLocationRelByIds(userId, locationId);

					if (deleteUserLocation != null) {
						deleteUserLocation.checkOut = true;
						await userLocationTable.UpdateAsync(deleteUserLocation);

						#if OFFLINE_SYNC_ENABLED
						await syncAsync();
						#endif

					} else {
						DebugHelper.newMsg(TAG, string.Format(@"User Location relationship between user {0} and location {1} was already deleted", userId, locationId));
					}

					return true;
				}
			} catch (Exception e) {
				if (userId != null && locationId != null && e != null) {
					DebugHelper.newMsg(TAG, string.Format(@"Failed to delete User Location relationship between user {0} and location {1}", userId, locationId));
					DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));

					if (!mHasPurgedTable) {
						mHasPurgedTable = true;
						await userLocationTable.PurgeAsync();
						return await checkOutFromLocationAsync(userId, locationId);
					}
				}
			}

			return false;
		}

	}

}