namespace SportsConnection {
	
	public interface IAndroid {

		void finishApp();

		int getAndroidSdkVersion();

		bool androidHasInitialized();

		bool androidHasGrantedPermissionToLocation();

	}

}