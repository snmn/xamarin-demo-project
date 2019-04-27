using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class Locations : ContentPage, BasePage.BasePageInterface {

		private MapsController mMapController;

		private BasePage mBasePage;
		private static INavigation sNavigation {
			get; set;
		}


		#region Initilization
		public Locations() {
			InitializeComponent();
			initBasicComponents();
			initHeader();
			initBtnAddLocation();
			initNavigationForStaticUsage();
		}

		private void initBasicComponents() {
			if (mBasePage == null) {
				mBasePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
			}
		}

		private void initHeader() {
			NavigationPage.SetHasNavigationBar(this, true);
			Title = Txt.LBL_LOCATIONS_PAGE_TITLE;
		}

		private void initBtnAddLocation() {
			var btnAddLocationTapGestureRecognizer = new TapGestureRecognizer();

			btnAddLocationTapGestureRecognizer.Tapped += (s, e) => {
				Navigation.PushAsync(new CreateUpdateLocation(null)).ConfigureAwait(false);
			};

			btnAddLocation.GestureRecognizers.Add(btnAddLocationTapGestureRecognizer);
		}

		private void initNavigationForStaticUsage() {
			sNavigation = Navigation;
		}
		#endregion


		#region ReInitilization
		/// <summary>
		/// Page Life cycle method
		/// 
		/// The method rebuild the page content every time the page is resumed including the first time it is executed.
		/// </summary>
		protected override void OnAppearing() {
			base.OnAppearing();
			checkConnectivity();
			enableAndRebuildPageContent();
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void checkConnectivity() {
			if (!NetworkUtils.isOnline()) {
				mBasePage.displayNoInternetContainer();
			} else {
				mBasePage.hideNoConnectionContainer();
			}
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void enableAndRebuildPageContent() {
			enablePageContent();
            initMapAsync();
		}

		private void enablePageContent() {
			pageContainer.IsEnabled = true;
			mainContainer.IsEnabled = true;
			mapContainer.IsEnabled = true;
		}

        private void initMapAsync() {
            if (PlataformUtils.androidHasGrantedLocationPermission()) {
                mMapController = new MapsController();
                mapContainer.Children.Add(mMapController.map);

                mMapController.setIsClickable(true);
                mMapController.setIsShowingSingleLocation(false);
                mMapController.refreshMap();
                mMapController.navigateToCurrentLocation();
            } else {
                navigateToSplashUntilInitialiationIsDone();
            }
        }

        private void navigateToSplashUntilInitialiationIsDone() {
			var message = new SelectMenuOptionMessage {
				menuOptionIdx = Menu.MAIN_MENU_OPT_INDEX_SPLASH
			};

			Device.BeginInvokeOnMainThread(() => {
				MessagingCenter.Send(message, SelectMenuOptionMessage.TAG);
			});
		}
		#endregion


		#region Business Logic
		public static async Task goToSelectedLocationDetails(string locationId) {
			var locationController = new LocationController();
			var location = await locationController.getLocationById(locationId);
			await sNavigation.PushAsync(new LocationDetails(location));
		}
		#endregion


		#region Destruction
		/// <summary>
		/// Page Life cycle method
		/// 
		/// The methods below release the page content everytime it is destroyed. it is 
		/// important to release memory and avoid OutOfMemory errors on Android devices, due to the lack of 
		/// efficiency of the Java Garbage Collector that must be called explicitly after the disposal of the 
		/// elements.
		/// </summary>
		protected override void OnDisappearing() {
			disableAndRelasePageContent();
			PlataformUtils.tryToReleaseMemory();
			base.OnDisappearing();
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void disableAndRelasePageContent() {
			pageContainer.IsEnabled = false;
			mainContainer.IsEnabled = false;
			mapContainer.IsEnabled = false;

			releaseMap();
		}

		private void releaseMap() {
			try {
				mapContainer.Children.Remove(mMapController.map);
				mMapController = null;
			} catch (Exception e) {
				DebugHelper.newMsg(Constants.TAG_LOCATIONS, e.StackTrace);
			}
		}
		#endregion

	}

}