// To add offline sync support: add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 

//#define OFFLINE_SYNC_ENABLED
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

namespace SportsConnection
{

    public class LocationPost : AzureObject
    {

        private string mId;
        private string mLocationId;
        private string mUserId;
        private string mTitle;
        private string mText;
        private DateTime mPostedDate;

        [JsonProperty(PropertyName = "id")]
        public string id
        {
            get
            {
                return mId;
            }
            set
            {
                mId = value;
            }
        }

        [JsonProperty(PropertyName = "locationId")]
        public string locationId
        {
            get
            {
                return mLocationId;
            }
            set
            {
                mLocationId = value;
            }
        }

        [JsonProperty(PropertyName = "userId")]
        public string userId
        {
            get
            {
                return mUserId;
            }
            set
            {
                mUserId = value;
            }
        }


        [JsonProperty(PropertyName = "title")]
        public string title
        {
            get
            {
                return mTitle;
            }
            set
            {
                mTitle = value;
            }
        }

        [JsonProperty(PropertyName = "text")]
        public string text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
            }
        }

        [JsonProperty(PropertyName = "postedDate")]
        public DateTime postedDate
        {
            get
            {
                return mPostedDate;
            }
            set
            {
                mPostedDate = value;
            }
        }

        public string toString()
        {
            return "LocationPost object (" + id + ")";
        }

    }


    public class LocationPostWrapper
    {

        private LocationPost mLocPost;
        private User mUser;
        private Location mLocation;
        private Sport mSport;

        public LocationPost core
        {
            get
            {
                return mLocPost;
            }
            set
            {
                mLocPost = value;
            }
        }

        public User user
        {
            get
            {
                return mUser;
            }
            set
            {
                mUser = value;
            }
        }

        public Location location
        {
            get
            {
                return mLocation;
            }
            set
            {
                mLocation = value;
            }
        }

        public Sport sport
        {
            get
            {
                return mSport;
            }
            set
            {
                mSport = value;
            }
        }

    }


    /// <summary>
    /// This class manages instances of the LocationPost class
    /// </summary>
    public class LocationPostManager
    {

        private string TAG = "LocationPostManager";

#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<LocationPost> locationPostTable;
#else
        IMobileServiceTable<LocationPost> locationPostTable;
#endif

        public LocationPostManager()
        {
#if OFFLINE_SYNC_ENABLED
			locationPostTable = AzureController.client.GetSyncTable<LocationPost>();
#else
            //	locationPostTable = AzureController.client.GetTable<LocationPost>();
#endif
        }

        public bool IsOfflineEnabled
        {
            get
            {
#if OFFLINE_SYNC_ENABLED
				return locationPostTable is IMobileServiceSyncTable<LocationPost>;
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

				await locationPostTable.PullAsync(
					// The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					// Use a different query name for each unique query in your program
					"allSCLocationPosts",
					locationPostTable.CreateQuery());
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
        /// Get all posts of a location
        /// </summary>
        /// <returns>A list of LocationPost objects</returns>
        /// <param name="syncItems">If set to <c>true</c> sync items.</param>
        public async Task<List<LocationPost>> getLocationPostsAsync(string locationId, bool syncItems = true)
        {
            try
            {
                List<LocationPost> locationPosts = await locationPostTable.Where(
                    locationPost => (locationPost.locationId == locationId)
                ).OrderBy(locationPost => locationPost.postedDate).ToListAsync();

                return new List<LocationPost>(locationPosts);

            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));

            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get a LocationPost object by its id
        /// </summary>
        /// <returns>A LocationPost or null</returns>
        /// <param name="locationPostId">A LocationPost id</param>
        public async Task<LocationPost> getLocationPostById(string locationPostId)
        {
            try
            {
                List<LocationPost> locPosts = await locationPostTable.Where(
                    locationPost => (locationPost.id == locationPostId)
                ).ToListAsync();

                if (locPosts != null)
                {
                    foreach (LocationPost locPost in locPosts)
                    {
                        return locPost;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get locationPost {0} from Azure backend", locationPostId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Upsert a Location Post object
        /// </summary>
        /// <returns>True if the LocationPost was saved</returns>
        /// <param name="locationPost">A LocationPost object</param>
        public async Task<bool> saveLocationPostAsync(LocationPost locationPost)
        {
            try
            {
                if (locationPost.id == null)
                {
                    await locationPostTable.InsertAsync(locationPost);

#pragma warning disable CS4014
#if OFFLINE_SYNC_ENABLED
					await syncAsync();
#endif
#pragma warning restore CS4014

                    return true;
                }
                else
                {
                    LocationPost locPost = await getLocationPostById(locationPost.id);

                    if (locPost != null)
                    {
                        locPost.locationId = locationPost.locationId;
                        locPost.userId = locationPost.userId;
                        //locPost.sportId = locationPost.sportId;
                        locPost.postedDate = locationPost.postedDate;
                        locPost.text = locationPost.text;
                        locPost.title = locationPost.title;

                        await locationPostTable.UpdateAsync(locPost);

#pragma warning disable CS4014
#if OFFLINE_SYNC_ENABLED
						await syncAsync();
#endif
#pragma warning restore CS4014

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to save location post object {0} in Azure backend", locationPost.toString()));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a LocationPost object async.
        /// </summary>
        /// <returns>True, if the LocationPost object was deleted.</returns>
        /// <param name="locationPostId">A LocationPost id</param>
        public async Task<bool> deleteLocationPostAsync(string locationPostId)
        {
            try
            {
                if (locationPostId != null)
                {
                    LocationPost locationPost = await getLocationPostById(locationPostId);

                    if (locationPost != null)
                    {
                        await locationPostTable.DeleteAsync(locationPost);

#pragma warning disable CS4014
#if OFFLINE_SYNC_ENABLED
						await syncAsync();
#endif
#pragma warning restore CS4014
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, string.Format(@"LocationPost with id {0} was already deleted", locationPostId));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete LocationPost with id {0} from Azure backend", locationPostId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

    }

}