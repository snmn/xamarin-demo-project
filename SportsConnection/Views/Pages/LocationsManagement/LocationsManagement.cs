using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class LocationsManagement : ContentPage {

		private readonly int IDX_TAB_FAVORITE_LOCATIONS = 1;
		private readonly int IDX_TAB_RECENT_LOCATIONS = 2;
		private readonly int IDX_TAB_YOUR_LOCATIONS = 3;

		private UserController mUserController = new UserController();
		private DirectionsController mDirectionController = new DirectionsController();
		private LocationController mLocationController = new LocationController();

		private ObservableCollection<UserFavoriteLocationWrapper> mFavoriteLocations;
		private ObservableCollection<UserLocationWrapper> mRecentLocations;
		private ObservableCollection<Location> mUserLocations;

		private User mUser;
		private Geopoint mCurrentLocation = new Geopoint();
		private Location mSelectedLocation;
		private MapsController mMapController;

		private BasePage mBasePage;
		private readonly TapGestureRecognizer mLocationNameTapGestureRecognizer = new TapGestureRecognizer();


		#region Initialization
		public LocationsManagement() {
			InitializeComponent();
			initBasicComponents();
			initListeners();
		}

		private void initBasicComponents() {
			if (mBasePage == null) {
				mBasePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
			}
		}

		private void initListeners() {
			initLocationNameTapGestureListener();
			initOnFavoriteLocationSelectedClickListener();
			initOnRecentLocationSelectedClickListener();
			initOnUserLocationSelectedClickListener();
		}

		private void initLocationNameTapGestureListener() {
			mLocationNameTapGestureRecognizer.Tapped += (s, e) => {
				toggleHeaderVisibility();
			};

			locationNameContainer.GestureRecognizers.Add(mLocationNameTapGestureRecognizer);
		}

		private void initOnFavoriteLocationSelectedClickListener() {
			listFavoriteLocations.ItemSelected += (sender, e) => {
				var selectedUserFavLocRel = (UserFavoriteLocationWrapper)e.SelectedItem;

				if (selectedUserFavLocRel != null) {
					if (selectedUserFavLocRel.location != null) {
						showSelectedLocation(selectedUserFavLocRel.location);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initOnRecentLocationSelectedClickListener() {
			listRecentLocations.ItemSelected += (sender, e) => {
				var selectedUserLocRel = (UserLocationWrapper)e.SelectedItem;

				if (selectedUserLocRel != null) {
					if (selectedUserLocRel.location != null) {
						showSelectedLocation(selectedUserLocRel.location);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initOnUserLocationSelectedClickListener() {
			listUserLocations.ItemSelected += (sender, e) => {
				var selectedLocation = (Location)e.SelectedItem;

				if (selectedLocation != null) {
					showSelectedLocation(selectedLocation);
				}

				((ListView)sender).SelectedItem = null;
			};
		}
		#endregion


		#region ReInitialization
		/// <summary>
		/// Page Life cycle method
		/// 
		/// The method rebuild the page content every time the page is resumed including the first time it is executed.
		/// </summary>
		protected override void OnAppearing() {
			base.OnAppearing();
			mBasePage.rebuildUI();
			checkConnectivity();
			enableAndRebuildPageContent();
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void checkConnectivity() {
			if (!NetworkUtils.isOnline()) {
				mBasePage.displayNoInternetContainer();
			}
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void enableAndRebuildPageContent() {
			initData();
			initViews();
		}

		private void initData() {
			initCurrentUserAttributes();
			initFavoriteLocations().ConfigureAwait(false);
			initRecentLocations().ConfigureAwait(false);
			initUserLocations().ConfigureAwait(false);
		}

		private void initCurrentUserAttributes() {
			mUser = App.authController.getCurrentUser();
			mCurrentLocation.setLatitude(SettingsController.getCurrentLatitude());
			mCurrentLocation.setLongitude(SettingsController.getCurrentLongitude());
		}

		private async Task initFavoriteLocations() {
			mBasePage.showLoadingSpinner(Txt.MSG_LOADING);

			List<UserFavoriteLocation> userFavLocRels = await mUserController.getUserFavoriteLocations(true);

			if (userFavLocRels != null) {
				var favoriteLocations = new List<UserFavoriteLocationWrapper>();

				foreach (UserFavoriteLocation userFavRel in userFavLocRels) {

					if (userFavRel.userId != null && userFavRel.locationId != null) {
						UserFavoriteLocationWrapper userFavLocObj = await mUserController.getUserFavoriteLocation(
							userFavRel.userId, userFavRel.locationId);

						if (userFavLocObj != null) {
							favoriteLocations.Add(userFavLocObj);
						}
					}
				}

				mFavoriteLocations = new ObservableCollection<UserFavoriteLocationWrapper>(favoriteLocations);
				listFavoriteLocations.ItemsSource = mFavoriteLocations;

				if (mFavoriteLocations.Count > 0) {
					numberFavoriteLocations.Text = mFavoriteLocations.Count.ToString();
					showFavoriteLocations(true);
				} else {
					numberFavoriteLocations.Text = "";
					showFavoriteLocations(false);
				}

				await mBasePage.hideLoadingSpinner();
			} else {
				showFavoriteLocations(false);
			}
		}

		private async Task initRecentLocations() {
			mBasePage.showLoadingSpinner(Txt.MSG_LOADING);

			if (mUser != null) {

				if (mUser.uid != null) {
					List<UserLocationWrapper> userLocations = await mLocationController.getUserRecentLocations(mUser.uid);

					if (userLocations != null) {
						mRecentLocations = new ObservableCollection<UserLocationWrapper>(userLocations);
						listRecentLocations.ItemsSource = mRecentLocations;

						if (mRecentLocations.Count > 0) {
							numberRecentLocations.Text = mRecentLocations.Count.ToString();
							showRecentLocations(true);
						} else {
							numberRecentLocations.Text = "";
							showRecentLocations(false);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						await mBasePage.hideLoadingSpinner();
						showRecentLocations(false);
					}
				} else {
					await mBasePage.hideLoadingSpinner();
					showRecentLocations(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showRecentLocations(false);
			}
		}

		private async Task initUserLocations() {
			mBasePage.showLoadingSpinner(Txt.MSG_LOADING);

			if (mUser != null) {

				if (mUser.uid != null) {
					List<Location> userLocations = await mLocationController.getLocationsOwnedByUser(mUser.uid);

					if (userLocations != null) {
						mUserLocations = new ObservableCollection<Location>(userLocations);
						listUserLocations.ItemsSource = mUserLocations;

						if (mUserLocations.Count > 0) {
							numberLocationsOwnedByUser.Text = mUserLocations.Count.ToString();
							showUserLocations(true);
						} else {
							numberLocationsOwnedByUser.Text = "";
							showUserLocations(false);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						await mBasePage.hideLoadingSpinner();
						showUserLocations(false);
					}
				} else {
					await mBasePage.hideLoadingSpinner();
					showUserLocations(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showUserLocations(false);
			}
		}

		private void initViews() {
			initImages();
			initToolbar();
			initMap();
			initHeader();
		}

		private void initImages() {
			arrowToggleHeaderVisibilityImage.Source = Constants.IMAGE_ICO_GRAY_ARROW_UP;
			tabASelectorImage.Source = Constants.IMAGE_ICO_FAVORITE_LOCATION_BLACK;
			tabBSelectorImage.Source = Constants.IMAGE_ICO_RECENT_LOCATIONS;
			tabCSelectorImage.Source = Constants.IMAGE_ICO_USER_LOCATIONS;
		}

		private void initToolbar() {
			Title = Txt.LBL_TITLE_MANAGE_LOCATIONS;
		}

		private void initMap() {
			if (PlataformUtils.androidHasGrantedLocationPermission()) {
				mMapController = new MapsController();
				mapContainer.Children.Add(mMapController.map);

				mMapController.setIsShowingSingleLocation(true);
				mMapController.setIsClickable(false);
				mMapController.clearMap();
			}
		}

		private void initHeader() {
			headerContainer.IsVisible = false;

			headerContainer.WidthRequest = SettingsController.getPhoneWidth();
			headerContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			mapContainer.WidthRequest = SettingsController.getPhoneWidth();
			mapContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			if (mMapController != null) {
				if (mMapController.map != null) {
					mMapController.map.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;
				}
			}

			toggleHeaderVisibility();
		}
		#endregion


		#region BusinessLogic
		private void selectTab1(object sender, EventArgs e) {
			selectTab(IDX_TAB_FAVORITE_LOCATIONS);
		}

		private void selectTab2(object sender, EventArgs e) {
			selectTab(IDX_TAB_RECENT_LOCATIONS);
		}

		private void selectTab3(object sender, EventArgs e) {
			selectTab(IDX_TAB_YOUR_LOCATIONS);
		}

		private void selectTab(int tabNumber) {
			switch (tabNumber) {
				case 1:
					tabA.IsVisible = true;
					tabB.IsVisible = false;
					tabC.IsVisible = false;

					tabASelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabCSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);

					numberFavoriteLocations.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberLocationsOwnedByUser.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					
					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabCSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorIndicator.IsVisible = true;
					tabBSelectorIndicator.IsVisible = false;
					tabCSelectorIndicator.IsVisible = false;

					break;

				case 2:
					tabA.IsVisible = false;
					tabB.IsVisible = true;
					tabC.IsVisible = false;

					tabASelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);
					tabCSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);

					numberFavoriteLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					numberLocationsOwnedByUser.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					tabCSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorIndicator.IsVisible = false;
					tabBSelectorIndicator.IsVisible = true;
					tabCSelectorIndicator.IsVisible = false;

					break;

				case 3:
					tabA.IsVisible = false;
					tabB.IsVisible = false;
					tabC.IsVisible = true;

					tabASelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabCSelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);

					numberFavoriteLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberLocationsOwnedByUser.Opacity = Constants.IMAGE_ALPHA_ENABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabCSelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;

					tabASelectorIndicator.IsVisible = false;
					tabBSelectorIndicator.IsVisible = false;
					tabCSelectorIndicator.IsVisible = true;

					break;
			}
		}

		private void toggleHeaderVisibility() {
			var headerAnimation = new Animation();

			if (!headerContainer.IsVisible) {
				headerContainer.IsVisible = true;

				var rotateLocNameArrowAnimation = new Animation(v => arrowToggleHeaderVisibilityImage.Rotation = v, 180, 360);
				headerAnimation.Add(0.3, 1, rotateLocNameArrowAnimation);

				var scaleUpMapAnimation = new Animation(v => headerContainer.Scale = v, 0, 1, Easing.SpringOut);
				headerAnimation.Add(0.3, 1, scaleUpMapAnimation);

				headerAnimation.Commit(headerContainer,
							   Constants.ANIMATION_HEADER_LOCATION_DETAILS,
							   Constants.ANIMATION_DEFAULT_INTERVAL_BETWEEN_ANIMATIONS,
							   600,
							   null,
							   (v, c) => showHeader());
			} else {
				var rotateLocNameArrowAnimation = new Animation(v => arrowToggleHeaderVisibilityImage.Rotation = v, 0, 180);
				headerAnimation.Add(0.3, 1, rotateLocNameArrowAnimation);

				var scaleDownMapAnimation = new Animation(v => headerContainer.Scale = v, 1, 0, Easing.SpringIn);
				headerAnimation.Add(0.3, 1, scaleDownMapAnimation);

				headerAnimation.Commit(headerContainer,
								   Constants.ANIMATION_HEADER_LOCATION_DETAILS,
								   Constants.ANIMATION_DEFAULT_INTERVAL_BETWEEN_ANIMATIONS,
								   600,
								   null,
								   (v, c) => hideHeader());
			}
		}

		private void showHeader() {
			arrowToggleHeaderVisibilityImage.Rotation = 360;
		}

		private void hideHeader() {
			headerContainer.IsVisible = false;
			arrowToggleHeaderVisibilityImage.Rotation = 180;
		}

		private void showFavoriteLocations(bool show) {
			if (show) {
				viewFavoriteLocations.IsVisible = true;
				emptyMsgFavoriteLocationsContainer.IsVisible = false;
			} else {
				viewFavoriteLocations.IsVisible = false;
				emptyMsgFavoriteLocationsContainer.IsVisible = true;
			}
		}

		private void showRecentLocations(bool show) {
			if (show) {
				viewRecentLocations.IsVisible = true;
				emptyMsgRecentLocationsContainer.IsVisible = false;
			} else {
				viewRecentLocations.IsVisible = false;
				emptyMsgRecentLocationsContainer.IsVisible = true;
			}
		}

		private void showUserLocations(bool show) {
			if (show) {
				viewUserLocations.IsVisible = true;
				emptyMsgUserLocationsContainer.IsVisible = false;
			} else {
				viewUserLocations.IsVisible = false;
				emptyMsgUserLocationsContainer.IsVisible = true;
			}
		}

		private void showSelectedLocation(Location location) {
			mSelectedLocation = location;

			mMapController.clearMap();
			mMapController.addLocationPin(mSelectedLocation);
			showDirectionsToLocation().ConfigureAwait(false);

			if (mSelectedLocation.name != null) {
				locationName.Text = mSelectedLocation.name;
			}
		}

		private async Task showDirectionsToLocation() {
			var userLat = mCurrentLocation.getLatitude().ToString();
			var userLng = mCurrentLocation.getLongitude().ToString();
			string locationLat = mSelectedLocation.localLatitude;
			string locationLng = mSelectedLocation.localLongitude;

			if (await mDirectionController.loadListDirections(userLat, userLng, locationLat, locationLng)) {

				if (mDirectionController.getEncodedPolyline() != null) {
					mMapController.drawPathToLocation(mDirectionController.getEncodedPolyline());
					mMapController.updateZoomLevel(Constants.DEFAULT_MAP_ZOOM_LEVEL);
				}
			}
		}

		public async void undoRelFavoriteLocation(object sender, EventArgs e) {
			var answer = await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_ARE_U_SURE_UNFAVORITE_LOCATION,
											Txt.LBL_BTN_YES, Txt.LBL_BTN_NO);
			if (answer) {
				var selectedUserFavLocRel = (UserFavoriteLocationWrapper)((Frame)sender).BindingContext;

				if (selectedUserFavLocRel != null) {
					
					if (selectedUserFavLocRel.location != null && selectedUserFavLocRel.user != null) {
						
						if (selectedUserFavLocRel.location.id != null && selectedUserFavLocRel.user.uid != null) {
							mBasePage.showLoadingSpinner("");
							await mUserController.deleteUserFavoriteLocationRelationship(
								selectedUserFavLocRel.user.uid, selectedUserFavLocRel.location.id);
							await mBasePage.hideLoadingSpinner();
							await initFavoriteLocations();
						}
					}
				}	
			}
		}

		public async void deleteLocation(object sender, EventArgs e) {
			var answer = await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_ARE_U_SURE_DELETE_LOCATION,
											Txt.LBL_BTN_YES, Txt.LBL_BTN_NO);
			if (answer) {
				var selectedLocation = (Location)((Frame)sender).BindingContext;

				if (selectedLocation != null) {

					if (selectedLocation.id != null) {
						mBasePage.showLoadingSpinner("");

						if (await mLocationController.deleteLocation(selectedLocation.id)) {
							await DisplayAlert("", Txt.MSG_DELETED_LOCATION, Txt.LBL_BTN_OK);
						} else {
							await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_FAILED_TO_DELETE_LOCATION, Txt.LBL_BTN_OK);
						}

						mMapController.refreshMap();

						await mBasePage.hideLoadingSpinner();

						initData();
					}
				}
			}
		}

		public async void navigateToUpdateLocation(object sender, EventArgs e) {
			var selectedLocation = (Location)((Frame)sender).BindingContext;

			if (selectedLocation != null) {
				await Navigation.PushAsync(new CreateUpdateLocation(selectedLocation));
			}
		}

		public async void navigateToLocationDetails(object sender, EventArgs e) {
			Location selectedLocation = null;

			try {
				var selectedUserFavLocRel = (UserFavoriteLocationWrapper)((Frame)sender).BindingContext;

				if (selectedUserFavLocRel != null) {

					if (selectedUserFavLocRel.location != null) {
						selectedLocation = selectedUserFavLocRel.location;
					}
				}
			} catch (InvalidCastException) {
				try {
					var selectedUserLocation = (UserLocationWrapper)((Frame)sender).BindingContext;

					if (selectedUserLocation != null) {

						if (selectedUserLocation.location != null) {
							selectedLocation = selectedUserLocation.location;
						}
					}
				} catch (InvalidCastException) {
					selectedLocation = (Location)((Frame)sender).BindingContext;
				}	
			}

			if (selectedLocation != null) {
				await Navigation.PushAsync(new LocationDetails(selectedLocation));	
			}
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
			mBasePage.releaseUI();
			releaseMap();
			releaseImages();
		}

		private void releaseMap() {
			try {
				if (mMapController != null) {
					if (mMapController.map != null) {
						mapContainer.Children.Remove(mMapController.map);
						mMapController.map = null;
					}
				}
			} catch (Exception e) {
				DebugHelper.newMsg(Constants.TAG_LOCATIONS, e.StackTrace);
			}

			mMapController = null;
		}

		private void releaseImages() {
			arrowToggleHeaderVisibilityImage.Source = null;
			tabASelectorImage.Source = null;
			tabBSelectorImage.Source = null;
			tabCSelectorImage.Source = null;
		}
		#endregion

	}

}