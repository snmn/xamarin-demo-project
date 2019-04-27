// To add offline sync support: add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace SportsConnection
{

    public class Location : AzureObject
    {

        private string mId;
        private string mName;
        private string mLocalLatitude;
        private string mLocalLongitude;
        private bool mVerified;
        private DateTime mVerifiedDate;
        private string mUserId;

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

        [JsonProperty(PropertyName = "name")]
        public string name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }



        [JsonProperty(PropertyName = "localLatitude")]
        public string localLatitude
        {
            get
            {
                return mLocalLatitude;
            }
            set
            {
                mLocalLatitude = value;
            }
        }

        [JsonProperty(PropertyName = "localLongitude")]
        public string localLongitude
        {
            get
            {
                return mLocalLongitude;
            }
            set
            {
                mLocalLongitude = value;
            }
        }

        [JsonProperty(PropertyName = "verified")]
        public bool verified
        {
            get
            {
                return mVerified;
            }
            set
            {
                mVerified = value;
            }
        }

        [JsonProperty(PropertyName = "dateVerified")]
        public DateTime verifiedDate
        {
            get
            {
                return mVerifiedDate;
            }
            set
            {
                mVerifiedDate = value;
            }
        }

        [JsonProperty(PropertyName = "user_Id")]
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

        public string toString()
        {
            return "Location object (" + id + "): " + name;
        }
    }


    /// <summary>
    /// This class manages instances of the Location class
    /// </summary>
    public class SCLocationManager
    {

        private string TAG = "LocationManager";

        IMobileServiceTable<Location> locationTable;

        //public SCLocationManager() {
        //	if (AzureController.client != null) {
        //		locationTable = AzureController.client.GetTable<Location>();
        //	} else {
        //		locationTable = GeofencingTask.client.GetTable<Location>();
        //	}
        //}

        /// <summary>
        /// Get all Locations
        /// </summary>
        /// <returns>A list of Locations</returns>
        /// <param name="syncItems">If set to <c>true</c> sync items.</param>
        public async Task<List<Location>> getLocationsAsync()
        {
            try
            {
                List<Location> locations = await locationTable.ToListAsync();
                return new List<Location>(locations);

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
        /// Get a Location by its id
        /// </summary>
        /// <returns>A Location or null</returns>
        /// <param name="locationId">A Location id</param>
        public async Task<Location> getLocationById(string locationId)
        {
            try
            {
                List<Location> locations = await locationTable.Where(
                    location => (location.id == locationId)
                ).ToListAsync();

                if (locations != null)
                {
                    foreach (Location location in locations)
                    {
                        return location;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get object {0} from Azure backend", locationId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get a list of Location objects by their ids
        /// </summary>
        /// <returns>A List of Location objects or null</returns>
        /// <param name="locationIds">A list of Location ids</param>
        public async Task<List<Location>> getLocationByIds(List<string> locationIds)
        {
            List<Location> locations = new List<Location>();

            if ((locationIds.Count > 0) && (locationIds != null))
            {
                try
                {
                    locations = await locationTable.Where(
                        location => locationIds.Contains(location.id)
                    ).ToListAsync();

                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get locations from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return locations;
        }

        /// <summary>
        /// Get a dictionary of locations related to a list of ids
        /// </summary>
        /// <returns>A dictionary of Location objects or null.</returns>
        /// <param name="locationIds">A list with locations' ids</param>
        public async Task<Dictionary<string, Location>> getDictLocationsByIds(List<string> locationIds)
        {
            Dictionary<string, Location> locations = new Dictionary<string, Location>();

            if ((locationIds.Count > 0) && (locationIds != null))
            {
                try
                {
                    List<Location> locationsList = await locationTable.Where(
                        location => locationIds.Contains(location.id)
                    ).ToListAsync();

                    if (locationsList != null)
                    {
                        foreach (Location location in locationsList)
                        {
                            if (location != null)
                            {
                                locations.Add(location.id, location);
                            }
                        }
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "Could not get a list of locations with the given ids");
                    }
                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get locations from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return locations;
        }

        /// <summary>
        /// Get a Location by its name.
        /// </summary>
        /// <returns>A Location or null</returns>
        /// <param name="locationName">A Location name</param>
        public async Task<Location> getLocationByName(string locationName)
        {
            try
            {
                List<Location> locations = await locationTable.Where(
                    location => (location.name == locationName)
                ).ToListAsync();

                if (locations != null)
                {
                    foreach (Location location in locations)
                    {
                        return location;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get object with name {0} from Azure backend", locationName));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get the locations created by the user with the given uid.
        /// </summary>
        public async Task<List<Location>> getLocationsOwnedByUser(string userUid)
        {
            try
            {
                List<Location> locations = await locationTable.Where(
                    location => (location.userId == userUid)
                ).ToListAsync();

                if (locations != null)
                {
                    return locations;
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get object {0} from Azure backend", userUid));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Upsert a Location.
        /// </summary>
        /// <returns>True if the location was saved</returns>
        /// <param name="location">A Location</param>
        public async Task<bool> saveLocationAsync(Location location)
        {
            try
            {
                if (location.id == null)
                {
                    await locationTable.InsertAsync(location);

                    return true;
                }
                else
                {
                    Location updateLocation = await getLocationById(location.id);
                    updateLocation.name = location.name;
                    //	updateLocation.capacity = location.capacity;
                    updateLocation.localLatitude = location.localLatitude;
                    updateLocation.localLongitude = location.localLongitude;
                    updateLocation.verified = location.verified;
                    updateLocation.verifiedDate = location.verifiedDate;
                    updateLocation.userId = location.userId;
                    //	updateLocation.description = location.description;

                    await locationTable.UpdateAsync(updateLocation);

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to save object with name {0} in Azure backend", location.name));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a Location item async.
        /// </summary>
        /// <returns>The Location async.</returns>
        /// <param name="locationId">A Location id</param>
        public async Task<bool> deleteLocationAsync(string locationId)
        {
            try
            {
                if (locationId != null)
                {
                    Location deleteLocation = await getLocationById(locationId);

                    if (deleteLocation != null)
                    {
                        await locationTable.DeleteAsync(deleteLocation);

                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, string.Format(@"Location with id {0} was already deleted", locationId));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete location with id {0} from Azure backend", locationId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

    }

}