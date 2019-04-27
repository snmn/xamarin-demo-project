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

    public class Sport : AzureObject
    {

        private string mId;
        private string mName;
        private int mRecNumPlayers;
        private string mCreatorId;

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

        [JsonProperty(PropertyName = "recNumPlayers")]
        public int recNumPlayers
        {
            get
            {
                return mRecNumPlayers;
            }
            set
            {
                mRecNumPlayers = value;
            }
        }

        [JsonProperty(PropertyName = "createdBy")]
        public string userId
        {
            get
            {
                return mCreatorId;
            }
            set
            {
                mCreatorId = value;
            }
        }

        public string toString()
        {
            return "Sport object (" + name + "): ";
        }

    }


    /// <summary>
    /// This class manages instances of the Sport class
    /// </summary>
    public class SportManager
    {

        private string TAG = "SportManager";

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<Sport> sportTable;
#else
		IMobileServiceTable<Sport> sportTable;
#endif

        public SportManager()
        {
#if OFFLINE_SYNC_ENABLED
            //	sportTable = AzureController.client.GetSyncTable<Sport>();
#else
			sportTable = AzureController.client.GetTable<Sport>();
#endif
        }

        public bool IsOfflineEnabled
        {
            get
            {
#if OFFLINE_SYNC_ENABLED
                return sportTable is IMobileServiceSyncTable<Sport>;
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

                await sportTable.PullAsync(
                    // The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    // Use a different query name for each unique query in your program
                    "allSCSports",
                    sportTable.CreateQuery());
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
        /// Get all Sports
        /// </summary>
        /// <returns>A list of Sport objects</returns>
        /// <param name="syncItems">If set to <c>true</c> sync items.</param>
        public async Task<List<Sport>> getSportsAsync(bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await syncAsync();
                }
#endif

                List<Sport> sports = await sportTable.ToListAsync();
                return new List<Sport>(sports);

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
        /// Get a Sport by its id
        /// </summary>
        /// <returns>A Sport or null</returns>
        /// <param name="sportId">A Sport id</param>
        public async Task<Sport> getSportById(string sportId)
        {
            try
            {
                List<Sport> sports = await sportTable.Where(
                    sport => (sport.id == sportId)
                ).ToListAsync();

                if (sports != null)
                {
                    foreach (Sport sport in sports)
                    {
                        return sport;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get sport {0} from Azure backend", sportId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Get a list of Sport objects by their ids
        /// </summary>
        /// <returns>A List of Sport objects or null</returns>
        /// <param name="sportsIds">A list of Sport ids</param>
        public async Task<List<Sport>> getSportByIds(List<string> sportsIds)
        {
            List<Sport> sports = new List<Sport>();

            if ((sportsIds.Count > 0) && (sportsIds != null))
            {
                try
                {
                    sports = await sportTable.Where(
                        sport => sportsIds.Contains(sport.id)
                    ).ToListAsync();

                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get sports from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return sports;
        }

        /// <summary>
        /// Get a dictionary of sports related to a list of ids
        /// </summary>
        /// <returns>A dictionary of Sport objects or null.</returns>
        /// <param name="sportIds">A list with sport ids</param>
        public async Task<Dictionary<string, Sport>> getDictSportsByIds(List<string> sportIds)
        {
            Dictionary<string, Sport> sports = new Dictionary<string, Sport>();

            if ((sportIds.Count > 0) && (sportIds != null))
            {
                try
                {
                    List<Sport> sportsList = await sportTable.Where(
                        sport => sportIds.Contains(sport.id)
                    ).ToListAsync();

                    if (sportsList != null)
                    {
                        foreach (Sport sport in sportsList)
                        {
                            if (sport != null)
                            {
                                sports.Add(sport.id, sport);
                            }
                        }
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "Could not get a list of sports with the given ids");
                    }
                }
                catch (MobileServiceInvalidOperationException msioe)
                {
                    DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
                }
                catch (Exception e)
                {
                    DebugHelper.newMsg(TAG, "Failed to get sports from Azure backend");
                    DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
                }
            }

            return sports;
        }

        /// <summary>
        /// Get a Sport by its name
        /// </summary>
        /// <returns>A Sport or null</returns>
        /// <param name="sportName">A Sport name</param>
        public async Task<Sport> getSportByName(string sportName)
        {
            try
            {
                List<Sport> sports = await sportTable.Where(
                    sport => (sport.name == sportName)
                ).ToListAsync();

                if (sports != null)
                {
                    foreach (Sport sport in sports)
                    {
                        return sport;
                    }
                }
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to get sport {0} from Azure backend", sportName));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Upsert a Sport object
        /// </summary>
        /// <returns>True if the Sport was saved</returns>
        /// <param name="sport">A Sport</param>
        public async Task<bool> saveSportAsync(Sport sport)
        {
            try
            {
                if (sport.id == null)
                {
                    if ((sport.name != null))
                    {
                        if (await getSportByName(sport.name) == null)
                        {
                            await sportTable.InsertAsync(sport);

#pragma warning disable CS4014
                            await syncAsync();
#pragma warning restore CS4014

                            return true;
                        }
                        else
                        {
                            DebugHelper.newMsg(TAG, string.Format("The sport name {0} already exist", sport.name));
                            return false;
                        }
                    }
                }
                else
                {
                    Sport currentSport = await getSportById(sport.id);

                    if (currentSport != null)
                    {
                        currentSport.name = sport.name;
                        currentSport.recNumPlayers = sport.recNumPlayers;
                        await sportTable.UpdateAsync(sport);

#pragma warning disable CS4014
                        await syncAsync();
#pragma warning restore CS4014

                        return true;
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, "Failed to find the given Sport");
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to save sport {0} in Azure backend", sport.toString()));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

        /// <summary>
        /// Deletes a Sport async.
        /// </summary>
        /// <returns>True if the Sport was deleted</returns>
        /// <param name="sportId">A Sport id</param>
        public async Task<bool> deleteSportAsync(string sportId)
        {
            try
            {
                if (sportId != null)
                {
                    Sport sport = await getSportById(sportId);

                    if (sport != null)
                    {
                        await sportTable.DeleteAsync(sport);

#pragma warning disable CS4014
                        await syncAsync();
#pragma warning restore CS4014
                    }
                    else
                    {
                        DebugHelper.newMsg(TAG, string.Format(@"Sport with id {0} was already deleted", sportId));
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.newMsg(TAG, string.Format(@"Failed to delete Sport with id {0} from Azure backend", sportId));
                DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
            }

            return false;
        }

    }

}