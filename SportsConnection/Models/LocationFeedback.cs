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
	
	public class LocationFeedback : AzureObject {

		private string mId;
		private int mUserCount;
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

		[JsonProperty(PropertyName = "userCount")]
		public int userCount {
			get {
				return mUserCount;
			}
			set {
				mUserCount = value;
			}
		}

		[JsonProperty(PropertyName = "createdBy")]
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

		public string toString() {
			return "LocationFeedback object (" + id + "): ";
		}

	}


	public class LocationFeedbackWrapper {

		private LocationFeedback mLocFeedback;
		private User mUser;
		private Location mLocation;

		public LocationFeedback core {
			get {
				return mLocFeedback;
			}
			set {
				mLocFeedback = value;
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

	}


	/// <summary>
	/// This class manages instances of the LocationFeedback class
	/// </summary>
	public class LocationFeedbackManager {

		private string TAG = "LocationFeedbackManager";

		#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<LocationFeedback> locationFeedbackTable;
		#else
		IMobileServiceTable<LocationFeedback> locationFeedbackTable;
		#endif

		public LocationFeedbackManager() {
			#if OFFLINE_SYNC_ENABLED
			locationFeedbackTable = AzureController.client.GetSyncTable<LocationFeedback>();
			#else
			locationFeedbackTable = AzureController.client.GetTable<LocationFeedback>();
			#endif
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return locationFeedbackTable is IMobileServiceSyncTable<LocationFeedback>;
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

				await locationFeedbackTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCLocationsFeedbacks",
					locationFeedbackTable.CreateQuery());
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
		/// Get all LocationFeedbacks
		/// </summary>
		/// <returns>A list of LocationFeedback objects</returns>
		/// <param name="syncItems">If set to <c>true</c> sync items.</param>
		public async Task<List<LocationFeedback>> getLocationFeedbacksAsync(bool syncItems = false) {
			try {
				#if OFFLINE_SYNC_ENABLED
				if (syncItems) {
					await syncAsync();
				}
				#endif

				List<LocationFeedback> users = await locationFeedbackTable.ToListAsync();
				return new List<LocationFeedback>(users);

			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));

			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a LocationFeedback by its id
		/// </summary>
		/// <returns>A LocationFeedback or null</returns>
		/// <param name="locationFeedbackId">A LocationFeedback id</param>
		public async Task<LocationFeedback> getLocationFeedbackById(string locationFeedbackId) {
			try {
				List<LocationFeedback> locFeeds = await locationFeedbackTable.Where(
					locationFeedback => (locationFeedback.id == locationFeedbackId)
				).ToListAsync();

				if (locFeeds != null) {
					foreach (LocationFeedback locFeed in locFeeds) {
						return locFeed;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get user {0} from Azure backend", locationFeedbackId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Upsert a Location Feedback Object
		/// </summary>
		/// <returns>True if the LocationFeedback was saved</returns>
		/// <param name="locationFeedback">A LocationFeedback</param>
		public async Task<bool> saveLocationFeedbackAsync(LocationFeedback locationFeedback) {
			try {
				if (locationFeedback.id == null) {
					LocationFeedback locFeed = await getLocationFeedbackById(locationFeedback.id);

					if (locFeed == null) {
						await locationFeedbackTable.InsertAsync(locationFeedback);

						#pragma warning disable CS4014
						await syncAsync();
						#pragma warning restore CS4014

						return true;
					} else {
						DebugHelper.newMsg(TAG, "A Location Feedback object with the same id already exists");
						return false;
					}
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to save location feedback object {0} in Azure backend", locationFeedback.toString()));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

		/// <summary>
		/// Deletes a LocationFeedback async.
		/// </summary>
		/// <returns>The LocationFeedback async.</returns>
		/// <param name="locationFeedbackId">A LocationFeedback id</param>
		public async Task<bool> deleteLocationFeedbackAsync(string locationFeedbackId) {
			try {
				if (locationFeedbackId != null) {
					LocationFeedback locationFeedback = await getLocationFeedbackById(locationFeedbackId);

					if (locationFeedback != null) {
						await locationFeedbackTable.DeleteAsync(locationFeedback);

						#pragma warning disable CS4014
						await syncAsync();
						#pragma warning restore CS4014

					} else {
						DebugHelper.newMsg(TAG, string.Format(@"LocationFeedback with id {0} was already deleted", locationFeedbackId));
					}

					return true;
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to delete LocationFeedback with id {0} from Azure backend", locationFeedbackId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

	}

}