using ModernHttpClient;

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace SportsConnection {
	
	public class AzureController {

		public MobileServiceSQLiteStore store;
		public static MobileServiceClient client;


		public void init() {
			registerHTTPClient();
			registerLocalDB();
		}

		private void registerHTTPClient() {
			client = new MobileServiceClient(Constants.APPLICATION_URL, new NativeMessageHandler());
		}

		private void registerLocalDB() {
			// Init the local database
			store = new MobileServiceSQLiteStore(Constants.PATH_LOCAL_DB_FILE);

			// Register the local tables
			store.DefineTable<Sport>();
			store.DefineTable<Location>();
			store.DefineTable<User>();
			store.DefineTable<LocationFeedback>();
			store.DefineTable<LocationSport>();
			store.DefineTable<LocationPost>();
			store.DefineTable<UserSport>();
			store.DefineTable<UserLocation>();
			store.DefineTable<UserCoordinate>();
			store.DefineTable<UserFavoriteLocation>();
			store.DefineTable<UserRelation>();

			// Define a sync handler object to deal with conflicts between the local storage and the remote storage
			client.SyncContext.InitializeAsync(store);
			GeofencingTask.client = client;
		}

	}

}