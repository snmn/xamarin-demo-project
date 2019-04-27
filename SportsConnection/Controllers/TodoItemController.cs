using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SportsConnection {
	
	public class TodoItemController {

		private string TAG = "TodoItemController";
		private List<TodoItem> mTodoItems;
		public TodoItemManager todoItemsManager;

		public TodoItemController() {
			todoItemsManager = TodoItemManager.DefaultManager;
		}

		public async Task<bool> addTodoItem(string itemName) {
			TodoItem newItem = new TodoItem();
			newItem.name = itemName;
			await todoItemsManager.saveItemAsync(newItem);

			return true;
		}

		public async Task<bool> printItems() {
			mTodoItems = await todoItemsManager.getItemsAsync(true);

			if (mTodoItems != null) {
				foreach (TodoItem listItem in mTodoItems) {
					if (listItem != null) {
						DebugHelper.newMsg(TAG, listItem.toString());
					} else {
						DebugHelper.newMsg(TAG, "Item is null");
					}
				}

				return true;
			} else {
				DebugHelper.newMsg(TAG, "Could not get the list of items");
			}

			return false;
		}

		public async Task<TodoItem> getItemById(string itemId) {
			TodoItem item = await todoItemsManager.getItemById(itemId);

			return item;
		}

		public async Task<TodoItem> getItemByName(string itemName) {
			TodoItem item = await todoItemsManager.getItemByName(itemName);

			return item;
		}

		public async Task<bool> updateItem(TodoItem item) {
			if (item != null) {
				await todoItemsManager.saveItemAsync(item);
				return true;
			}

			return false;
		}

		public TodoItem getLastItem() {
			for (int i = 1; i <= mTodoItems.Count(); i++) {
				if (i == mTodoItems.Count() - 1) {
					return mTodoItems.ElementAt(i);
				}
			}

			return null;
		}

		public async Task<bool> deleteItem(string itemId) {
			if (itemId != null) {
				await todoItemsManager.deleteItemAsync(itemId);
				return true;
			}

			return false;
		}

	}

}