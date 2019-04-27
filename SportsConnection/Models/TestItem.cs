﻿// To add offline sync support: add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore
// to all projects in the solution and uncomment the symbol definition OFFLINE_SYNC_ENABLED
// For Xamarin.iOS, also edit AppDelegate.cs and uncomment the call to SQLitePCL.CurrentPlatform.Init()
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342 
// #define OFFLINE_SYNC_ENABLED
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
	
	public class TestItem : AzureObject {

		private string mId;
		private string mName;
		private bool mDone;

		// Getters and Setters
		[JsonProperty(PropertyName = "id")]
		public string id {
			get {
				return mId;
			}
			set {
				mId = value;
			}
		}

		[JsonProperty(PropertyName = "name")]
		public string name {
			get {
				return mName;
			}
			set {
				mName = value;
			}
		}

		public string toString() {
			return "Todo object (" + id + "): " + name;
		}
	}


	/// <summary>
	/// This class manages instances of the TodoItem class
	/// </summary>
	public class TestItemManager {

		private string TAG = "TestItemManager";
		public static TestItemManager defaultInstance = new TestItemManager();


#if OFFLINE_SYNC_ENABLED
		IMobileServiceSyncTable<TestItem> todoTable;
#else
		IMobileServiceTable<TestItem> todoTable;
#endif

		private TestItemManager() {
#if OFFLINE_SYNC_ENABLED
			todoTable = App.azureController.client.GetSyncTable<TestItem>();
#else
			todoTable = App.azureController.client.GetTable<TestItem>();
#endif
		}

		public static TestItemManager DefaultManager {
			get {
				return defaultInstance;
			}
			private set {
				defaultInstance = value;
			}
		}

		public bool IsOfflineEnabled {
			get {
				#if OFFLINE_SYNC_ENABLED
				return todoTable is IMobileServiceSyncTable<TestItem>;
				#else
				return false;
				#endif
			}
		}

#if OFFLINE_SYNC_ENABLED
		public async Task SyncAsync() {
			ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

			try {
				await App.azureController.client.SyncContext.PushAsync();

				await todoTable.PullAsync(
					//The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					//Use a different query name for each unique query in your program
					"allTestItems",
					todoTable.CreateQuery());
			} catch (MobileServicePushFailedException exc) {
				if (exc.PushResult != null) {
					syncErrors = exc.PushResult.Errors;
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			// Simple error/conflict handling. A real application would handle the various errors like network conditions,
			// server conflicts and others via the IMobileServiceSyncHandler.
			if (syncErrors != null) {
				foreach (var error in syncErrors) {
					//Update failed, reverting to server's copy.
					await error.CancelAndUpdateItemAsync(error.Result);
					string errorMsg = string.Format(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);

					DebugHelper.newMsg(TAG, errorMsg);
				}
			}
		}
#endif

		/// <summary>
		/// Get all TodoItems
		/// </summary>
		/// <returns>A list of TodoItem</returns>
		/// <param name="syncItems">If set to <c>true</c> sync items.</param>
		public async Task<List<TestItem>> getItemsAsync(bool syncItems = false) {
			try {
#if OFFLINE_SYNC_ENABLED
				if (syncItems) {
					await SyncAsync();
				}
#endif

				List<TestItem> items = await todoTable.ToListAsync();
				return new List<TestItem>(items);

			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a TodoItem by its id
		/// </summary>
		/// <returns>A TodoItem or null</returns>
		/// <param name="todoItemId">string</param>
		public async Task<TestItem> getItemById(string todoItemId) {
			try {
				List<TestItem> todoItems = await todoTable.Where(
					todoItem => (todoItem.id == todoItemId)
				).ToListAsync();

				if (todoItems != null) {
					foreach (TestItem item in todoItems) {
						return item;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get object {0} from Azure backend", todoItemId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Get a TodoItem by its name.
		/// </summary>
		/// <returns>A TodoItem or null</returns>
		/// <param name="todoItemName">string</param>
		public async Task<TestItem> getItemByName(string todoItemName) {
			try {
				List<TestItem> todoItems = await todoTable.Where(
					todoItem => (todoItem.name == todoItemName)
				).ToListAsync();

				if (todoItems != null) {
					foreach (TestItem item in todoItems) {
						return item;
					}
				}
			} catch (MobileServiceInvalidOperationException msioe) {
				DebugHelper.newMsg(TAG, string.Format(@"Invalid sync operation: {0}", msioe.Message));
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to get object with name {0} from Azure backend", todoItemName));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return null;
		}

		/// <summary>
		/// Upsert a TodoItem.
		/// </summary>
		/// <returns>True if the item was saved</returns>
		/// <param name="item">A TodoItem</param>
		public async Task<bool> saveItemAsync(TestItem item) {
			try {
				if (item.id == null) {
					await todoTable.InsertAsync(item);
				} else {
					TestItem updateItem = await getItemById(item.id);
					updateItem.name = item.name;

					await todoTable.UpdateAsync(updateItem);
				}

				return true;
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to save object with name {0} from Azure backend", item.name));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

		/// <summary>
		/// Deletes a TodoItem item async.
		/// </summary>
		/// <returns>The item async.</returns>
		/// <param name="itemId">A TodoItem</param>
		public async Task<bool> deleteItemAsync(string itemId) {
			try {
				if (itemId != null) {
					TestItem itemToDelete = await getItemById(itemId);

					if (itemToDelete != null) {
						await todoTable.DeleteAsync(itemToDelete);
					} else {
						DebugHelper.newMsg(TAG, string.Format(@"Item with id {0} was already deleted", itemId));
					}

					return true;
				}
			} catch (Exception e) {
				DebugHelper.newMsg(TAG, string.Format(@"Failed to delete object with id {0} from Azure backend", itemId));
				DebugHelper.newMsg(TAG, string.Format(@"Error: {0}", e.ToString()));
			}

			return false;
		}

	}
}