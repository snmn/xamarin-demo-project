using Xamarin.Forms;

namespace SportsConnection{
	
	public class App : Application {

		public static AzureController azureController;
		public static AuthUserController authController;
		public static GeofencingController geofencingController;

		public static bool sIsDebugging;


		public App (){
			initPlataform();
			initDebugMode();
			initAzureService();
			initAuthenticationController();
			initGeofencingController();

			MainPage = new Root();
		}

		/// <summary>
		/// Initialize the plataform info.
		/// </summary>
		private void initPlataform() {
			PlataformUtils.init();
		}

		/// <summary>
		/// Turn debugging on or off.
		/// </summary>
		private void initDebugMode() {
			sIsDebugging = true;
		}

		/// <summary>
		/// Create the controller that manages the Azure local data storage and the access to the server
		/// endpoints.
		/// </summary>
		private void initAzureService() {
			azureController = new AzureController();
			azureController.init();
		}

		/// <summary>
		/// Sets the authentication controller.
		/// </summary>
		private void initAuthenticationController() {
			authController = new AuthUserController();
		}

		/// <summary>
		/// Initialize the Geofencing controller.
		/// </summary>
		private void initGeofencingController() {
			geofencingController = new GeofencingController();
			geofencingController.init();
		}

	}

}