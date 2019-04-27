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

namespace SportsConnection
{

    public class LocationSport : AzureObject
    {

        private string mId;
        private string mLocationId;
        private string mSportId;

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

        [JsonProperty(PropertyName = "sport")]
        public string sportId
        {
            get
            {
                return mSportId;
            }
            set
            {
                mSportId = value;
            }
        }

        public string toString()
        {
            return "LocationSport object (" + id + "): ";
        }

    }


    public class LocationSportWrapper
    {

        private LocationSport mLocationSport;
        private Location mLocation;
        private Sport mSport;

        public LocationSport core
        {
            get
            {
                return mLocationSport;
            }
            set
            {
                mLocationSport = value;
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
    /// This class manages instances of the LocationSport class
    /// </summary>
    public class LocationSportManager
    {

        private string TAG = "LocationSportManager";

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<LocationSport> locationSportTable;
#else
		IMobileServiceTable<LocationSport> locationSportTable;
#endif

        public LocationSportManager()
        {
#if OFFLINE_SYNC_ENABLED
            //		locationSportTable = AzureController.client.GetSyncTable<LocationSport>();
#else
			locationSportTable = AzureController.client.GetTable<LocationSport>();
#endif
        }

        public bool IsOfflineEnabled
        {
            get
            {
#if OFFLINE_SYNC_ENABLED
                return locationSportTable is IMobileServiceSyncTable<LocationSport>;
#else
				return false;
#endif
            }
        }

#if OFFLINE_SYNC_ENABLED
        public async Task syncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                //	await AzureController.client.SyncContext.PushAsync();

                await locationSportTable.PullAsync(
                    // The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    // Use a different query name for each unique query in your program
                    "allSCLocationSports",
                    locationSportTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    await error.CancelAndDiscardItemAsync();
                    DebugHelper.newMsg(TAG, string.Format(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.",
                                                          error.TableName, error.Item["id"]));
                }
            }
        }
#endif

        /// <summary>
        /// Get all LocationSport objects related to a Location
        /// </summary>
        /// <returns>A list of LocationSport objects</returns>
        public async Task<List<LocationSport>> getLocationSports(string locationId, bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await syncAsync();
                }
#endif

                List<LocationSport> locationSports = await locationSportTable.Where(
                    locationSport => (locationSport.locationId == locationId)
                ).ToListAsync();

                return new List<LocationSport>(locationSports);

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
        /// Get a LocationSport by the locationId and the sportId
        /// </summary>
        /// <returns>A Location or null</returns>
        public async Task<LocationSport> getLocationSportByIds(string locationId, string sportId)
        {
            try
            {
                List<LocationSport> locationSports = await locationSportTable.Where(
                    locationSport => (locationSport.locationId == locationId && locationSport.sportId == sportId)
                ).ToListAsync();

                if (locationSports != null)
                {
                    foreach (LocationSport locationSport in locationSports)
                    {
                        return locationSport;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get LocationSport for Location {0} and Sport {1} from Azure backend", locationId, sportId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Upsert a LocationSport object
        /// </summary>
        /// <returns>True if the LocationSport was saved</returns>
        /// <param name="locationSport">A LocationSport</param>
        public async Task<bool> saveLocationSportAsync(LocationSport locationSport)
        {
            try
            {
                if ((locationSport.locationId != null) && (locationSport.sportId != null))
                {
                    LocationSport updateLocationSport = await getLocationSportByIds(
                        locationSport.locationId, locationSport.sportId);

                    if (updateLocationSport == null)
                    {
                        await locationSportTable.InsertAsync(locationSport);

#pragma warning disable CS4014
                        await syncAsync();
#pragma warning restore CS4014

                        return true;
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "A LocationSport object with the same ids already exists");
                        return false;
                    }
                }
                else
                {
                    DebugHelper.newMsg(TAG, "The location id and the sport id cannot be null");
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, "Failed to save the LocationSport object in Azure backend");
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a LocationSport async.
        /// </summary>
        /// <returns>The LocationSport async.</returns>
        public async Task<bool> deleteLocationSportAsync(LocationSport locationSport)
        {
            try
            {
                if ((locationSport.locationId != null) && (locationSport.sportId != null))
                {
                    LocationSport deleteLocationSport = await getLocationSportByIds(
                        locationSport.locationId, locationSport.sportId);

                    if (deleteLocationSport != null)
                    {
                        await locationSportTable.DeleteAsync(deleteLocationSport);

#pragma warning disable CS4014
                        await syncAsync();
#pragma warning restore CS4014
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "The LocationSport relationship doesn't exist");
                    }

                    return true;
                }
                else
                {
                    DebugHelper.newMsg(TAG, "The location id and the sport id cannot be null");
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete LocationSport with id {0} from Azure backend", locationSport.id));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a LocationSport async.
        /// </summary>
        /// <returns>The LocationSport async.</returns>
        public async Task<bool> deleteLocationSportAsync(string locationId, string sportId)
        {
            try
            {
                if ((locationId != null) && (sportId != null))
                {
                    LocationSport deleteLocationSport = await getLocationSportByIds(locationId, sportId);

                    if (deleteLocationSport == null)
                    {
                        await locationSportTable.DeleteAsync(deleteLocationSport);

#pragma warning disable CS4014
                        await syncAsync();
#pragma warning restore CS4014

                        return true;
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "The LocationSport relationship doesn't exist");
                        return false;
                    }
                }
                else
                {
                    DebugHelper.newMsg(TAG, "The location id and the sport id cannot be null");
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete LocationSport"));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes all relationships between the location and the available list of sports
        /// </summary>
        /// <returns>The LocationSport async.</returns>
        public async Task<bool> cleanLocationSportRelationships(string locationId)
        {
            try
            {
                if (locationId != null)
                {
                    List<LocationSport> locationSports = await locationSportTable.Where(
                                    locationSport => (locationSport.locationId == locationId)
                    ).ToListAsync();

                    if (locationSports != null)
                    {
                        foreach (LocationSport locationSport in locationSports)
                        {
                            await locationSportTable.DeleteAsync(locationSport);
                        }
                    }

                    return true;
                }
                else
                {
                    DebugHelper.newMsg(TAG, "The location id and the sport id cannot be null");
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete LocationSport error: {0}", e.ToString()));
            }

            return false;
        }

    }

}