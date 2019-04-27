using System.Threading.Tasks;

using Xamarin.Forms;

namespace SportsConnection {
	
	public class TodoTestModule {

		private string TAG = "TodoTestModule";
		private TodoItemController mTodoItemsController = new TodoItemController();

		public TodoTestModule() {
			// OnPlatform<T> doesn't currently support the "Windows" target platform, so we have this check here.
			if (mTodoItemsController.todoItemsManager.IsOfflineEnabled &&
				(Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)) {

				DebugHelper.newMsg(TAG, "The system is currently offline and running on a windows device...");
			}
		}

		public async Task<bool> runUnitTest() {
			DebugHelper.newMsg(TAG, "Starting TodoItem Storage Test ...");

			bool testResult = await executeTestLogic();

			if (testResult) {
				DebugHelper.newMsg(TAG, "TodoItem storage test has finished: SUCCEEDED");
			} else {
				DebugHelper.newMsg(TAG, "TodoItem storage test has finished: FAILED");
			}

			return testResult;
		}

		public async Task<bool> executeTestLogic() {
			// Take a look at the server and define the values below
			string NAME_ITEM_CREATE = "Item 9";
			string NAME_ITEM_UPDATE = "Updated item 9";
			string ID_ITEM_DELETE = "cd3032e0-bff8-4ad4-88a8-db5c83e08d9e";
									
			bool unitTestResult = true;

			// Create
			if (await mTodoItemsController.addTodoItem(NAME_ITEM_CREATE)) {
				DebugHelper.newMsg(TAG, "Created new item");

				// Read all
				if (await mTodoItemsController.printItems()) {
					DebugHelper.newMsg(TAG, "Read all items.");

					// Update
					TodoItem item = await mTodoItemsController.getItemByName(NAME_ITEM_CREATE);

					if (item == null) {
						DebugHelper.newMsg(TAG, "Failed to read item from server");
						unitTestResult = false;
					} else {
						DebugHelper.newMsg(TAG, "Read single item");

						item.name = NAME_ITEM_UPDATE;
						item.done = true;

						if (await mTodoItemsController.updateItem(item)) {
							DebugHelper.newMsg(TAG, "Updated item.");

						} else {
							DebugHelper.newMsg(TAG, "Failed to update item");
							unitTestResult = false;
						}
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to read all items.");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to created new item");
				unitTestResult = false;
			}

			// Delete
			if (await mTodoItemsController.deleteItem(ID_ITEM_DELETE)) {
				DebugHelper.newMsg(TAG, "Deleted item");

			} else {
				DebugHelper.newMsg(TAG, "Failed to delete item");
				unitTestResult = false;
			}

			// Read modified data
			await mTodoItemsController.printItems();

			return unitTestResult;
		}

	}

}