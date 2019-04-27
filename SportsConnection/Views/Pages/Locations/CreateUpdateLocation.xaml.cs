using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class CreateUpdateLocation : ContentPage, BasePage.BasePageInterface {

		private LocationController mLocationController = new LocationController();
		private SportsController mSportsController = new SportsController();

		private Location mLocation;
		private List<Location> mNearbyLocations = new List<Location>();
		private bool mIsCreating = true;
		private List<Sport> mAvailableSports = new List<Sport>();
		private List<LocationSportWrapper> mLocationSports = new List<LocationSportWrapper>();

		private BasePage mBasePage;
		private ObservableCollection<SimpleListItemAdapter> mSportsList;


		#region Initilization
		public CreateUpdateLocation(Location location) {
			InitializeComponent();
			initBasicComponents();
			initPageType(location);
		}

		private void initBasicComponents() {
			mBasePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
		}

		private void initPageType(Location location) {
			if (location != null) {
				mLocation = location;
				mIsCreating = false;
			} else {
				mIsCreating = true;
			}
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
			enablePageContent();
			rebuildUI();
			initListeners();
			initData();
		}

		public void enablePageContent() {
			btnCleanLocName.IsEnabled = true;
			btnCleanLocDescription.IsEnabled = true;
			btnSave.IsEnabled = true;
		}

		private void rebuildUI() {
			mBasePage.rebuildUI();
			btnCleanLocName.Source = Constants.IMAGE_ICO_DELETE_BLACK;
			btnCleanLocDescription.Source = Constants.IMAGE_ICO_DELETE_BLACK;
		}

		private void initListeners() {
			initCleanNameListener();
			initCleanDescriptionListener();
		}

		private void initCleanNameListener() {
			var tapLocName = new TapGestureRecognizer();

			tapLocName.Tapped += (object sender, EventArgs args) => {
				cleanLocName();
			};

			btnCleanLocName.GestureRecognizers.Add(tapLocName);
		}

		private void initCleanDescriptionListener() {
			var tabLocDescription = new TapGestureRecognizer();

			tabLocDescription.Tapped += (object sender, EventArgs args) => {
				cleanLocDescription();
			};

			btnCleanLocDescription.GestureRecognizers.Add(tabLocDescription);
		}

		private void initData() {
			try {
				mBasePage.showLoadingSpinner(Txt.MSG_LOADING);

				initListSports().ConfigureAwait(true);

				if (mIsCreating) {
					initNearbyLocationsList().ConfigureAwait(true);
				}

				initializeViews();

				mBasePage.hideLoadingSpinner().ConfigureAwait(true);
			} catch (Exception e) {
				DebugHelper.newMsg(Constants.TAG_LOCATION_UPSERT, e.Message);
				DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_COULD_NOT_LOAD_SPORTS, Txt.LBL_BTN_OK).ConfigureAwait(true);

				Navigation.PopAsync().ConfigureAwait(true);
			}
		}

		private async Task initListSports() {
			mAvailableSports = await mSportsController.getSports(true);

			if (mAvailableSports != null) {

				if (mAvailableSports.Count == 0) {
					await mSportsController.getSports(true);
				}
			}

			if (!mIsCreating) {
				mLocationSports = await mSportsController.getLocationSports(mLocation.id);
			}

			initAvailableSports();
		}

		private void initAvailableSports() {
			if (mAvailableSports != null) {
				var tempListSports = new List<SimpleListItemAdapter>();

				foreach (Sport sport in mAvailableSports) {

					if (sport != null) {
						var itemAdapter = new SimpleListItemAdapter(sport.id, sport.name, false);
						tempListSports.Add(itemAdapter);
					}
				}

				if (!mIsCreating) {

					for (int i = 0; i < tempListSports.Count; i++) {

						if (tempListSports.ElementAt(i) != null) {

							foreach (var locSportWrapper in mLocationSports) {

								if (locSportWrapper != null) {

									if (locSportWrapper.sport != null) {

										if (locSportWrapper.sport.id == tempListSports.ElementAt(i).id) {
											tempListSports.ElementAt(i).toggleCheck();
										}
									}
								}
							}
						}
					}
				}

				mSportsList = new ObservableCollection<SimpleListItemAdapter>(tempListSports);
				listSports.ItemsSource = mSportsList;

				listSports.ItemSelected -= toggleListViewItem;
				listSports.ItemSelected += toggleListViewItem;
			}
		}

		private void toggleListViewItem(object sender, EventArgs e) {
			try {
				if (((ListView)sender).SelectedItem != null) {
					var item = (SimpleListItemAdapter)((ListView)sender).SelectedItem;

					if (item != null) {
						foreach (SimpleListItemAdapter listItem in mSportsList) {
							if (listItem.id == item.id) {
								listItem.toggleCheck();
							}
						}
					}

					((ListView)sender).SelectedItem = null;
				}
			} catch (Exception) {
				DebugHelper.newMsg(Constants.TAG_LOCATION_UPSERT, "Failed to toggle check the listview item");
			}
		}

		private async Task initNearbyLocationsList() {
			if (mIsCreating) {
				mNearbyLocations = await mLocationController.getLocations(true);

				if (mNearbyLocations != null) {

					if (mNearbyLocations.Count > 0) {
						await checkLocationsWithinCreationBlockRange();
					}
				}
			}
		}

		private async Task checkLocationsWithinCreationBlockRange() {
			var lat = SettingsController.getCurrentLatitude();
			var lng = SettingsController.getCurrentLongitude();

			if (mNearbyLocations != null) {

				foreach (Location location in mNearbyLocations) {

					if (location != null) {
						var distance = GeoCoordinatesUtils.distance(
											  lat,
											  lng,
											  FormatUtils.stringToDouble(location.localLatitude),
											  FormatUtils.stringToDouble(location.localLongitude),
											  Constants.MEASURE_KM
							);

						DebugHelper.newMsg(Constants.TAG_LOCATION_UPSERT, distance.ToString());

						if (distance < Constants.DISTANCE_THRESHOLD_MIN_DISTANCE_NEARBY_LOCATIONS) {
							await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_COULD_NOT_CREATE_LOCATION_MIN_DISTANCE, Txt.LBL_BTN_OK);
							await Navigation.PopAsync();
							break;
						}
					}
				}
			}
		}

		private void initializeViews() {
			initLocationName();
			initDescription();
			initBtnSave();
		}

		private void initLocationName() {
			locationName.WidthRequest = Width - Constants.FORM_TEXT_INPUT_CLEAN_BTN_PADDING;

			if (!mIsCreating) {
				Title = Txt.LBL_UPDATE_LOCATION;
				locationName.Text = mLocation.name;
			} else {
				Title = Txt.LBL_CREATE_LOCATION;
			}
		}

		private void initDescription() {
			locationDescription.WidthRequest = Width - Constants.FORM_TEXT_INPUT_CLEAN_BTN_PADDING;

			
		}

		private void initBtnSave() {
			if (mIsCreating) {
				btnSave.Text = Txt.LBL_CREATE_LOCATION;
			} else {
				btnSave.Text = Txt.LBL_UPDATE_LOCATION;
			}
		}
		#endregion


		#region Business Logic
		private async void validateLocationInfoAndSave(object sender, EventArgs e) {
			disablePageContent();

			if (string.IsNullOrEmpty(locationName.Text)) {
				await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_GIVE_NAME_LOCATION, Txt.LBL_BTN_OK);
			} else {

				if (getSelectedSports() != null) {

					if (getSelectedSports().Count == 0) {
						await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_SELECT_SPORT, Txt.LBL_BTN_OK);
					} else {

						if (mIsCreating) {
							mBasePage.showLoadingSpinner(Txt.MSG_CREATING_LOCATION);

							if (await createLocation()) {
								await DisplayAlert(Txt.TITLE_NICE, Txt.MSG_LOCATION_CREATED, Txt.LBL_BTN_OK);
							}
						} else {
							mBasePage.showLoadingSpinner(Txt.MSG_UPDATING_LOCATION);

							if (await updateLocation()) {
								await DisplayAlert(Txt.TITLE_THANKS, Txt.MSG_LOCATION_UPDATED, Txt.LBL_BTN_OK);
							}
						}

						await Navigation.PopAsync();
						enablePageContent();
					}
				}
			}
		}

		private async Task<bool> createLocation() {
			var lat = FormatUtils.doubleToString(SettingsController.getCurrentLatitude());
			var lng = FormatUtils.doubleToString(SettingsController.getCurrentLongitude());
			var locName = locationName.Text;
			var locDescription = locationDescription.Text;

			if (lat != null && lng != null) {

				if (locName != "") {
					mLocation = await mLocationController.getLocationByName(locName);

					if (mLocation == null) {

						if (await mLocationController.upsertLocation(locName, locDescription, 0, lat, lng, false,
															  DateTime.UtcNow, App.authController.getCurrentUser())) {

							mLocation = await mLocationController.getLocationByName(locName);

							if (mLocation != null) {

								foreach (Sport sport in getSelectedSports()) {
									await mSportsController.createLocationSportRelationship(mLocation.id, sport.id);
								}

								return true;
							} else {
								await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_COULD_NOT_CREATE_LOCATION, Txt.LBL_BTN_CLOSE);
							}
						} else {
							await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_COULD_NOT_CREATE_LOCATION, Txt.LBL_BTN_CLOSE);
						}
					} else {
						await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_CHOOSE_ANOTHER_NAME, Txt.LBL_BTN_CLOSE);
					}
				} else {
					await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_GIVE_NAME_LOCATION, Txt.LBL_BTN_CLOSE);
				}
			}

			return false;
		}

		private async Task<bool> updateLocation() {
			disablePageContent();

			mLocation.name = locationName.Text;
			//mLocation.description = locationDescription.Text;
			mLocation.localLatitude = mLocation.localLatitude;
			mLocation.localLongitude = mLocation.localLongitude;
			//mLocation.capacity = 0;
			mLocation.userId = App.authController.getCurrentUser().uid;
			mLocation.verified = false;

			if (await mLocationController.updateLocation(mLocation)) {
				await mSportsController.cleanLocationSportRelationships(mLocation.id);

				foreach (Sport sport in getSelectedSports()) {
					await mSportsController.createLocationSportRelationship(mLocation.id, sport.id);
				}

				return true;
			} else {
				await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_COULD_NOT_UPDATE, Txt.LBL_BTN_CLOSE);
				return false;
			}
		}

		private List<Sport> getSelectedSports() {
			var selectedSports = new List<Sport>();

			if (mSportsList != null) {
				for (int i = 0; i < mSportsList.Count; i++) {
					SimpleListItemAdapter item = mSportsList[i];

					if (item.isChecked) {
						for (int j = 0; j < mAvailableSports.Count; j++) {
							Sport locSport = mAvailableSports[j];

							if (locSport != null) {
								if (locSport.id == item.id) {
									selectedSports.Add(locSport);
								}
							}
						}
					}
				}
			}

			return selectedSports;
		}

		private void cleanLocName() {
			locationName.Text = "";
		}

		private void cleanLocDescription() {
			locationDescription.Text = "";
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
			disablePageContent();
			releaseUI();
			releaseListeners();
		}

		public void disablePageContent() {
			btnCleanLocName.IsEnabled = false;
			btnCleanLocDescription.IsEnabled = false;
			btnSave.IsEnabled = false;
		}

		private void releaseUI() {
			mBasePage.releaseUI();

			btnCleanLocName.Source = null;
			btnCleanLocName.BindingContext = null;
			btnCleanLocDescription.Source = null;
			btnCleanLocDescription.BindingContext = null;

			listSports.BindingContext = null;
		}

		private void releaseListeners() {
			btnCleanLocName.GestureRecognizers.Clear();
			btnCleanLocDescription.GestureRecognizers.Clear();
		}
		#endregion

	}

}