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
using Newtonsoft.Json.Linq;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
#endif

namespace SportsConnection
{


    public class User : AzureObject
    {

        // Server only attributes
        private string mId;
        private string mUID;
        private string mName;
        private string mProfileImage = "";
        private string mFacebookId;
        private DateTime mBirthDate;

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

        [JsonProperty(PropertyName = "uid")]
        public string uid
        {
            get
            {
                return mUID;
            }
            set
            {
                mUID = value;
            }
        }

        [JsonProperty(PropertyName = "facebookId")]
        public string facebookId
        {
            get
            {
                return mFacebookId;
            }
            set
            {
                mFacebookId = value;
            }
        }

        [JsonProperty(PropertyName = "birthDate")]
        public DateTime birthDate
        {
            get
            {
                return mBirthDate;
            }
            set
            {
                mBirthDate = value;
            }
        }

        [JsonProperty(PropertyName = "profileImage")]
        public string profileImage
        {
            get
            {
                return mProfileImage;
            }
            set
            {
                mProfileImage = value;
            }
        }



        public string toString()
        {
            return "User object (" + id + "): " + name;
        }
    }


    /// <summary>
    /// This class manages instances of the User class
    /// </summary>
    public class UserManager
    {

        private string TAG = "UserManager";

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<User> userTable;
#else
		IMobileServiceTable<User> userTable;
#endif

        //        public UserManager()
        //        {
        //            if (AzureController.client != null)
        //            {
        //#if OFFLINE_SYNC_ENABLED
        //                userTable = AzureController.client.GetSyncTable<User>();
        //#else
        //				userTable = AzureController.client.GetTable<User>();
        //#endif
        //            }
        ////            else
        ////            {
        ////#if OFFLINE_SYNC_ENABLED
        ////                userTable = GeofencingTask.client.GetSyncTable<User>();
        ////#else
        ////				userTable = GeofencingTask.client.GetTable<User>();
        ////#endif
        ////            }
        //        }

        public bool IsOfflineEnabled
        {
            get
            {
#if OFFLINE_SYNC_ENABLED
                return userTable is IMobileServiceSyncTable<User>;
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
                //    await AzureController.client.SyncContext.PushAsync();

                await userTable.PullAsync(
                    // The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    // Use a different query name for each unique query in your program
                    "allSCUsers",
                    userTable.CreateQuery()
                );
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

                    DebugHelper.newMsg(TAG, string.Format(@"Error executing sync operation. Item: {0} ({1}). 
					Operation discarded.", error.TableName, error.Item["id"]));
                }
            }
        }
#endif

        /// <summary>
        /// Get all Users
        /// </summary>
        /// <returns>A list of Users</returns>
        /// <param name="syncItems">If set to <c>true</c> sync items.</param>
        public async Task<List<User>> getUsersAsync(bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await syncAsync();
                }
#endif

                List<User> users = await userTable.ToListAsync();
                return new List<User>(users);

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
        /// Get a User by its id
        /// </summary>
        /// <returns>A User or null</returns>
        /// <param name="userId">A User id</param>
        public async Task<User> getUserById(string userId)
        {
            try
            {
                List<User> users = await userTable.Where(
                    user => (user.id == userId)
                ).ToListAsync();

                if (users != null)
                {
                    foreach (User user in users)
                    {
                        return user;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get user {0} from Azure backend", userId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get a list of users related to a list of ids
        /// </summary>
        /// <returns>A list of User objects or null.</returns>
        /// <param name="userUids">A list with user ids</param>
        public async Task<List<User>> getListUsersByIds(List<string> userUids)
        {
            List<User> users = null;

            if ((userUids.Count > 0) && (userUids != null))
            {
                try
                {
                    users = await userTable.Where(
                        user => userUids.Contains(user.uid)
                    ).ToListAsync();

                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}. Probably there 
					isn't a user for one of the user ids on the list start fetching objects manually ...",
                                                          msioe.Message, msioe.StackTrace));

                    // Perform alternative (Bad) operation
                    List<User> tempUsers = await getUsersAsync();
                    users = new List<User>();

                    if (tempUsers != null)
                    {
                        foreach (User user in tempUsers)
                        {
                            foreach (string userUid in userUids)
                            {
                                if (userUid == user.uid)
                                {
                                    users.Add(user);
                                }
                            }
                        }
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "Failed to get users from Azure backend");
                    }

                    if (users.Count == 0)
                    {
                        DebugHelper.newMsg(TAG, "Failed to get users from Azure backend");
                    }

                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get users from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return users;
        }

        /// <summary>
        /// Get a dictionary of users related to a list of ids
        /// </summary>
        /// <returns>A dictionary of User objects or null.</returns>
        /// <param name="userIds">A list with user ids</param>
        public async Task<Dictionary<string, User>> getDictUsersByIds(List<string> userIds)
        {
            Dictionary<string, User> users = new Dictionary<string, User>();

            if ((userIds.Count > 0) && (userIds != null))
            {
                try
                {
                    List<User> usersList = await userTable.Where(
                        user => userIds.Contains(user.id)
                    ).ToListAsync();

                    if (usersList != null)
                    {
                        foreach (User user in usersList)
                        {
                            if (user != null)
                            {
                                users.Add(user.id, user);
                            }
                        }
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "Could not get a list of users with the given ids");
                    }
                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get users from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return users;
        }

        /// <summary>
        /// Get a User by its email
        /// </summary>
        /// <returns>A User or null</returns>
        /// <param name="uid">A User id</param>
        public async Task<User> getUserByUID(string uid)
        {
            try
            {
                await syncAsync();

                List<User> users = await userTable.Where(
                    user => (user.uid == uid)
                ).ToListAsync();

                if (users != null)
                {
                    foreach (User user in users)
                    {
                        return user;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get user with email {0} from Azure backend", uid));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Upsert a User
        /// </summary>
        /// <returns>True if the User was saved</returns>
        /// <param name="user">A User</param>
        public async Task<bool> upsertUserAsync(User user, string uid)
        {
            try
            {
                if (uid != null)
                {
                    User existingUser = await getUserByUID(uid);

                    if (existingUser == null)
                    {
                        // Set a system setting for later usage
                        SettingsController.setIsFirtTime(true);

                        // Create the user
                        user.uid = uid;
                        user.birthDate = DateTime.UtcNow;
                        await userTable.InsertAsync(user);
                        await syncAsync();
                        return true;
                    }
                    else
                    {
                        // Set a system setting for later usage
                        SettingsController.setIsFirtTime(false);

                        if (user.name != null)
                        {
                            existingUser.name = user.name;
                        }

                        if (user.facebookId != null)
                        {
                            existingUser.facebookId = user.facebookId;
                        }

                        if (user.profileImage != null)
                        {
                            existingUser.profileImage = user.profileImage;
                        }



                        await userTable.UpdateAsync(existingUser);

                        await syncAsync();

                        return true;
                    }
                }
                else
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Failed to save user {0} in Azure backend", user.toString()));
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to save user {0} in Azure backend", user.toString()));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a User async.
        /// </summary>
        /// <returns>The User async.</returns>
        /// <param name="userId">A User id</param>
        public async Task<bool> deleteUserAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    User deleteUser = await getUserByUID(userId);

                    if (deleteUser != null)
                    {
                        await userTable.DeleteAsync(deleteUser);

                        await syncAsync();

                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, string.Format(@"Item with id {0} was already deleted", userId));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete user with id {0} from Azure backend", userId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

    }

}