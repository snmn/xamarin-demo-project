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

    public class UserCoordinate : AzureObject
    {

        private string mId;
        private string mUserUid;
        private string mLatitude;
        private string mLongitude;


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

        [JsonProperty(PropertyName = "userId")]
        public string userUid
        {
            get
            {
                return mUserUid;
            }
            set
            {
                mUserUid = value;
            }
        }

        [JsonProperty(PropertyName = "latitude")]
        public string latitude
        {
            get
            {
                return mLatitude;
            }
            set
            {
                mLatitude = value;
            }
        }

        [JsonProperty(PropertyName = "longitude")]
        public string longitude
        {
            get
            {
                return mLongitude;
            }
            set
            {
                mLongitude = value;
            }
        }



        public string toString()
        {
            return "UserCoordinate object (" + id + ")";
        }

    }


    /// <summary>
    /// This class manages instances of the UserCoordinate class
    /// </summary>
    public class UserCoordinateManager
    {

        private string TAG = "UserCoordinateManager";
        public Dictionary<string, UserCoordinate> userCoordinates = new Dictionary<string, UserCoordinate>();

        IMobileServiceTable<UserCoordinate> userCoordinateTable;


        //public UserCoordinateManager() {
        //	if (AzureController.client != null) {
        //		userCoordinateTable = AzureController.client.GetTable<UserCoordinate>();
        //	} else {
        //		userCoordinateTable = GeofencingTask.client.GetTable<UserCoordinate>();
        //	}
        //}

        /// <summary>
        /// Get all UserCoordinates
        /// </summary>
        /// <returns>A list of UserCoordinate objects</returns>
        /// <param name="syncItems">If set to <c>true</c> sync items.</param>
        public async Task<List<UserCoordinate>> getUserCoordinatesAsync(bool syncItems = false)
        {
            try
            {
                List<UserCoordinate> userCoords = await userCoordinateTable.ToListAsync();

                foreach (UserCoordinate userCoordinate in userCoords)
                {
                    if (!userCoordinates.ContainsKey(userCoordinate.userUid))
                    {
                        userCoordinates.Add(userCoordinate.userUid, userCoordinate);
                    }
                    else
                    {
                        userCoordinates[userCoordinate.userUid] = userCoordinate;
                    }
                }

                return new List<UserCoordinate>(userCoords);

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
        /// Load all UserCoordinates into a local variable
        /// </summary>
        /// <returns>A list of UserCoordinate objects</returns>
        public async Task<bool> loadUsersCoordinatesAsync(bool syncItems = false)
        {
            try
            {
                List<UserCoordinate> userCoords = await userCoordinateTable.ToListAsync();

                foreach (UserCoordinate userCoordinate in userCoords)
                {
                    userCoordinates.Add(userCoordinate.userUid, userCoordinate);
                }

                return true;

            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));

            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Get a UserCoordinate by the user id
        /// </summary>
        /// <returns>A UserCoordiante or null</returns>
        /// <param name="userId">A User id</param>
        public async Task<UserCoordinate> getUserCoordinateByIdAsync(string userId)
        {
            try
            {
                List<UserCoordinate> userCoordiantes = await userCoordinateTable.Where(
                    userCoordinate => (userCoordinate.userUid == userId)
                ).ToListAsync();

                if (userCoordiantes != null)
                {
                    foreach (UserCoordinate userCoordinate in userCoordiantes)
                    {
                        return userCoordinate;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get userCoordinate for userId {0} from Azure backend", userId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get a UserCoordinate by the user id
        /// </summary>
        /// <returns>A UserCoordiante or null</returns>
        /// <param name="userId">A User id</param>
        public UserCoordinate getUserCoordinateById(string userId)
        {
            UserCoordinate userCoord = null;

            if (userCoordinates.TryGetValue(userId, out userCoord))
            {
                DebugHelper.newMsg(TAG, "Recovered user coordinate " + userCoord.toString());
                return userCoord;
            }

            return null;
        }

        /// <summary>
        /// Upsert a UserCoordinate
        /// </summary>
        /// <returns>True if the UserCoordinate was saved</returns>
        /// <param name="userCoordinate">A UserCoordiante</param>
        public async Task<bool> saveUserCoordinateAsync(UserCoordinate userCoordinate)
        {
            try
            {
                if ((userCoordinate.userUid != null) && (userCoordinate.latitude != null) &&
                    (userCoordinate.longitude != null))
                {
                    UserCoordinate updateUserCoordinate = await getUserCoordinateByIdAsync(userCoordinate.userUid);

                    if (updateUserCoordinate == null)
                    {
                        await userCoordinateTable.InsertAsync(userCoordinate);

                        return true;
                    }
                    else
                    {
                        updateUserCoordinate.latitude = userCoordinate.latitude;
                        updateUserCoordinate.longitude = userCoordinate.longitude;
                        //	updateUserCoordinate.altitude = userCoordinate.altitude;
                        updateUserCoordinate.userUid = userCoordinate.userUid;

                        await userCoordinateTable.UpdateAsync(updateUserCoordinate);
                        return true;
                    }
                }
                else
                {
                    DebugHelper.newMsg(TAG, "The user id, the latitude and longitude are mandatory to create a UserCoordinate");
                    return false;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, "Failed to save userCoordinate in Azure backend");
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a UserCoordinate async
        /// </summary>
        /// <returns>True, if the UserCoordinate was deleted</returns>
        /// <param name="userId">A User id</param>
        public async Task<bool> deleteUserCoordinateAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    UserCoordinate deleteUserCoordinate = await getUserCoordinateByIdAsync(userId);

                    if (deleteUserCoordinate != null)
                    {
                        await userCoordinateTable.DeleteAsync(deleteUserCoordinate);
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, string.Format(@"UserCoordinate with userId {0} was already deleted", userId));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete UserCoordinate for userId {0} from Azure backend", userId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

    }

}