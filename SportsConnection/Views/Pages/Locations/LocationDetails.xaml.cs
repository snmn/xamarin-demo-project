using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class LocationDetails : ContentPage, BasePage.BasePageInterface {

		private readonly float MAP_ZOOM_LEVEL = 0.01f;
		private readonly int IDX_TAB_DIRECTIONS_TO_LOCATION = 1;
		private readonly int IDX_TAB_CHAT = 2;
		private readonly int IDX_TAB_PEOPLE_AT_LOCATION = 3;
        private readonly int MAP_DELAY_UPDATE_LOCATION = 7000;

		private MapsController mMapController;
		private DirectionsController mDirectionController = new DirectionsController();
		private SportsController mSportsController = new SportsController();
		private UserController mUserController = new UserController();
		private LocationController mLocationController = new LocationController();
		private LocationPostController mLocationPostsController;

		private ObservableCollection<DirectionStep> mDirectionsToLocation;
		private ObservableCollection<LocationPostWrapper> mWallPosts;
		private ObservableCollection<User> mUsersAtLocation;

		private Geopoint mCurrentLocation = new Geopoint();
		private User mUser;
		private Location mLocation;
		private Sport mSport;

		private bool mIsFavorite;

		private BasePage mBasePage;
		private ToolbarItem mBtnRefresh;
		private ToolbarItem mBtnToggleFavorite;
		private ToolbarItem mBtnUpdateLocation;
		private ToolbarItem mBtnDeleteLocation;

		private readonly TapGestureRecognizer mLocationNameTapGestureRecognizer = new TapGestureRecognizer();


		#region Initialization
		public LocationDetails(Location selectedLocation) {
			InitializeComponent();
			preLoadData(selectedLocation);

			initBasicComponents();
			initViews();
			initListeners();
		}

		private void preLoadData(Location selectedLocation) {
			mLocation = selectedLocation;
			mUser = App.authController.getCurrentUser();
		}

		private void initBasicComponents() {
			if (mBasePage == null) {
				mBasePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
			}
		}

		private void initViews() {
			initImages();
			initToolbar();
			initHeader().ConfigureAwait(false);
			initFavoriteStatus();
			initLocationName();
			initEmtpyMessages();
			selectTab(IDX_TAB_DIRECTIONS_TO_LOCATION);
		}

		private void initImages() {
			arrowToggleHeaderVisibilityImage.Source = Constants.IMAGE_ICO_GRAY_ARROW_UP;
			tabASelectorImage.Source = Constants.IMAGE_ICO_DIRECTIONS_BLACK;
			tabBSelectorImage.Source = Constants.IMAGE_ICO_CHAT_BLACK;
			tabCSelectorImage.Source = Constants.IMAGE_ICO_PEOPLE_BLACK;
			btnNewWallMessage.Source = Constants.IMAGE_ICO_SEND_MESSAGE;
		}

		private void initToolbar() {
			mBtnRefresh = new ToolbarItem {
				Command = new Command((sender) => {
					initData();
				}),
				Text = Txt.LBL_BTN_REFRESH,
				Priority = 0,
				Order = ToolbarItemOrder.Secondary
			};

			mBtnToggleFavorite = new ToolbarItem {
				Command = new Command((sender) => {
					toggleUserFavoriteLocationRelationship(sender, null);
				}),
				Text = Txt.LBL_BTN_ADD_TO_FAVORITE,
				Priority = 1,
				Order = ToolbarItemOrder.Secondary
			};

			ToolbarItems.Add(mBtnRefresh);
			ToolbarItems.Add(mBtnToggleFavorite);

			if (mUser != null) {

				if (mUser.uid != null && mLocation.userId != null) {

					if (mUser.uid == mLocation.userId) {
						mBtnUpdateLocation = new ToolbarItem {
							#pragma warning disable RECS0165
							Command = new Command(async (sender) => {
							#pragma warning restore RECS0165
								await navigateToUpdateLocation();
							}),
							Text = Txt.LBL_BTN_UPDATE_LOCATION,
							Priority = 1,
							Order = ToolbarItemOrder.Secondary
						};

						mBtnDeleteLocation = new ToolbarItem {
							#pragma warning disable RECS0165
							Command = new Command(async (sender) => {
							#pragma warning restore RECS0165
								await deleteLocation();
							}),
							Text = Txt.LBL_BTN_DELETE_LOCATION,
							Priority = 1,
							Order = ToolbarItemOrder.Secondary
						};

						ToolbarItems.Add(mBtnUpdateLocation);
						ToolbarItems.Add(mBtnDeleteLocation);
					}
				}
			}
		}

		private async Task initHeader() {
			mBasePage.showLoadingSpinner(Txt.MSG_LOADING_LOCATION_INFO);

			if (mLocation.id != null) {
				List<LocationSportWrapper> locationSports = await mSportsController.getLocationSports(mLocation.id);

				if (locationSports != null) {
					bool addedCustomImage = false;

					foreach (LocationSportWrapper locationSport in locationSports) {

						if (locationSport.sport != null) {

							if (locationSport.sport.name != null) {
								var cleanedSportName = SportsController.getCleanedSportName(locationSport.sport.name);

								if (cleanedSportName == SportsController.SPORT_NAME_BASKETBALL) {
									locationImage.Source = Constants.IMAGE_SPORT_BASKETBALL;
									mSport = locationSport.sport;
									addedCustomImage = true;

								} else if (cleanedSportName == SportsController.SPORT_NAME_SOCCER) {
									locationImage.Source = Constants.IMAGE_SPORT_SOCCER;
									mSport = locationSport.sport;
									addedCustomImage = true;
								}
							}
						}
					}

					if (addedCustomImage) {
						initHeaderContainers();
					} else {
						locationImage.IsVisible = false;
					}

					await mBasePage.hideLoadingSpinner();
				} else {
					await mBasePage.hideLoadingSpinner();
				}
			} else {
				await mBasePage.hideLoadingSpinner();
			}
		}

		private void initHeaderContainers() {
			headerContainer.IsVisible = false;

			headerContainer.WidthRequest = SettingsController.getPhoneWidth();
			headerContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			locationImage.WidthRequest = SettingsController.getPhoneWidth();
			locationImage.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			mapContainer.WidthRequest = SettingsController.getPhoneWidth();
			mapContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			showHeaderContainerBasedOnSelectedTab();
			toggleHeaderVisibility();
		}

		private void initFavoriteStatus() {
			#pragma warning disable RECS0165
			Device.BeginInvokeOnMainThread(async () => {
			#pragma warning restore RECS0165
				try {
					if (mUser != null) {

						if (mUser.uid != null && mLocation.id != null) {
							UserFavoriteLocationWrapper favoriteLocation =
							await mUserController.getUserFavoriteLocation(mUser.uid, mLocation.id);

							if (favoriteLocation != null) {
								mIsFavorite = true;
								mBtnToggleFavorite.Text = Txt.LBL_BTN_REMOVE_FROM_FAVORITE;
							} else {
								mIsFavorite = false;
								mBtnToggleFavorite.Text = Txt.LBL_BTN_ADD_TO_FAVORITE;
							}
						} else {
							mIsFavorite = false;
							mBtnToggleFavorite.Text = Txt.LBL_BTN_ADD_TO_FAVORITE;
						}
					} else {
						mIsFavorite = false;
						mBtnToggleFavorite.Text = Txt.LBL_BTN_ADD_TO_FAVORITE;
					}
				} catch (Exception ex) {
					mIsFavorite = false;
					mBtnToggleFavorite.Text = Txt.LBL_BTN_ADD_TO_FAVORITE;
					DebugHelper.newMsg(Constants.TAG_LOCATION_DETAILS, ex.StackTrace);
				}
			});
		}

		private void initLocationName() {
			Title = Txt.LBL_LOCATION_DETAILS;

			if (mLocation != null) {

				if (mLocation.name != null) {
					locationName.Text = mLocation.name;
				}
			}
		}

		private void initEmtpyMessages() {
			emptyMsgDirectionsToLocation.Text = Txt.MSG_EMPTY_LIST_DIRECTIONS;
			emptyMsgWallMessages.Text = Txt.MSG_EMPTY_LIST_WALL;
			emptyMsgUsersAtLocation.Text = Txt.MSG_EMPTY_LIST_USERS_AT_LOCATION;
		}

		private void initListeners() {
			initWallMessagesListener();
			initializeUserLocationListener();
			initLocationNameTapGestureListener();
			initSendMessageTapGestureListener();
			initDirectionsToLocationListener();
			initLocationWallMessagesListener();
			initUsersAtLocationListener();
		}

		private void initWallMessagesListener() {
			MessagingCenter.Subscribe<WallMessage>(this, WallMessage.TAG, message => {

				Device.BeginInvokeOnMainThread(() => {

					if (mLocation != null && message != null && mUser != null) {

						if (mLocation.id != null && message.locationId != null &&
							message.userId != null && mUser.id != null) {

							if (message.locationId.Equals(mLocation.id) &&
								!message.userId.Equals(mUser.id)) {
								initLocationWallMessages().ConfigureAwait(false);
							}
						}
					}
				});
			});
		}

		/// <summary>
		/// Starts listening to the Location Service and get the current user location.
		/// </summary>
		private void initializeUserLocationListener() {
			MessagingCenter.Subscribe<UserLocationMessage>(this, UserLocationMessage.TAG, message => {

				Device.BeginInvokeOnMainThread(() => {
					mCurrentLocation.setAltitude(message.altitude);
					mCurrentLocation.setLatitude(message.lat);
					mCurrentLocation.setLongitude(message.lng);

					SettingsController.setCurrentLatitude(message.lat);
					SettingsController.setCurrentLongitude(message.lng);

					initFormSubmitActualNumberUsers();
				});
			});
		}

		private void initLocationNameTapGestureListener() {
			mLocationNameTapGestureRecognizer.Tapped += (s, e) => {
				toggleHeaderVisibility();
			};

			locationNameContainer.GestureRecognizers.Add(mLocationNameTapGestureRecognizer);
		}

		private void initSendMessageTapGestureListener() {
			var tapSendMessage = new TapGestureRecognizer();

			tapSendMessage.Tapped += (object sender, EventArgs args) => {
				addMessageToWall().ConfigureAwait(false);
			};

			btnNewWallMessage.GestureRecognizers.Add(tapSendMessage);
		}

		private void initDirectionsToLocationListener() {
			listRoutesToLocation.ItemSelected += (sender, e) => {
				((ListView)sender).SelectedItem = null;
			};
		}
			
		private void initLocationWallMessagesListener() {
			listLocationWall.ItemSelected += (sender, e) => {
				var selectedPost = (LocationPostWrapper)e.SelectedItem;

				if (selectedPost != null) {
					if (selectedPost.user != null) {
						Navigation.PushModalAsync(new Profile(selectedPost.user, false)).ConfigureAwait(true);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initUsersAtLocationListener() {
			listUsersAtLocation.ItemSelected += (sender, e) => {
				var selectedUser = (User)e.SelectedItem;

				if (selectedUser != null) {
					if (selectedUser.uid != null) {
							Navigation.PushModalAsync(new Profile(selectedUser, false)).ConfigureAwait(true);
					}
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
			PlataformUtils.tryToReleaseMemory();
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
			mBasePage.releaseUI();
			initData();
		}

		private void initData() {
			initCurrentUserLocation();
			getUpdatedLocation().ConfigureAwait(true);
			initMap();
			initFormSubmitActualNumberUsers();
			initDirectionsToLocation().ConfigureAwait(false);

			if (mLocation != null) {
				mLocationPostsController = new LocationPostController(mLocation);
				initLocationWallMessages().ConfigureAwait(false);
			}

			initUsersAtLocation().ConfigureAwait(false);
		}

		private void initCurrentUserLocation() {
			mCurrentLocation.setLatitude(SettingsController.getCurrentLatitude());
			mCurrentLocation.setLongitude(SettingsController.getCurrentLongitude());
		}

		private async Task getUpdatedLocation() {
			if (mLocation != null) {
				if (mLocation.id != null) {
					mLocation = await mLocationController.getLocationById(mLocation.id);
				} else {
					await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_FAILED_TO_DELETE_LOCATION, Txt.LBL_BTN_OK);
					await Navigation.PopToRootAsync();
				}
			} else {
				await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_FAILED_TO_DELETE_LOCATION, Txt.LBL_BTN_OK);
				await Navigation.PopToRootAsync();
			}
		}

		private void initMap() {
			if (PlataformUtils.androidHasGrantedLocationPermission()) {
				mMapController = new MapsController();
				mapContainer.Children.Add(mMapController.map);

				mMapController.map.WidthRequest = SettingsController.getPhoneWidth();
				mMapController.map.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

				mMapController.setIsShowingSingleLocation(true);
				mMapController.setIsClickable(false);
				mMapController.clearMap();
				mMapController.addLocationPin(mLocation);
                navigateToUserLocation().ConfigureAwait(false);
			}
		}

		private void drawPathToLocation() {
			if (mDirectionController != null && mMapController != null) {
				if (mDirectionController.getEncodedPolyline() != null) {
					mMapController.drawPathToLocation(mDirectionController.getEncodedPolyline());
				}
			}
		}

		private async Task navigateToUserLocation() {
            await Task.Run(async () => {
                await Task.Delay(MAP_DELAY_UPDATE_LOCATION);
                
				Device.BeginInvokeOnMainThread(() => {
					if (mMapController != null) {
						mMapController.navigateToCurrentLocation(
							SettingsController.getCurrentLatitude(),
							SettingsController.getCurrentLongitude(),
							MAP_ZOOM_LEVEL
						);
					}
				});
            }, new System.Threading.CancellationToken());
		}

		private async Task initDirectionsToLocation() {
			showDirectionsToLocation(false);

			var userLat = mCurrentLocation.getLatitude().ToString();
			var userLng = mCurrentLocation.getLongitude().ToString();
			string locationLat = mLocation.localLatitude;
			string locationLng = mLocation.localLongitude;

			if (await mDirectionController.loadListDirections(userLat, userLng, locationLat, locationLng)) {
				if (mDirectionController.getRouteLegs() != null) {
					var distanceStr = mDirectionController.getInitialDistanceToLococation(Constants.MEASURE_MILES) + Txt.POSTFIX_DISTANCE;
					string timeStr = "";

					if (mDirectionController.getEstimatedTime() <= Constants.ONE_MINUTE_SECS) {
						timeStr = Txt.LBL_LESS_THAN_MINUTE;
					} else {
						timeStr = mDirectionController.getEstimatedArrivalTime() + Txt.POSTFIX_TIME;
					}

					directionsSummaryDistance.Text = distanceStr;
					directionsSummaryTime.Text = timeStr;

					if (mDirectionController.getRouteSteps().Count > 0) {
						listRoutesToLocation.EndRefresh();
						mDirectionsToLocation = new ObservableCollection<DirectionStep>(mDirectionController.getRouteSteps());
						listRoutesToLocation.ItemsSource = mDirectionsToLocation;

						showDirectionsToLocation(true);
						drawPathToLocation();
						mMapController.updateZoomLevel(Constants.DEFAULT_MAP_ZOOM_LEVEL);
					} else {
						showDirectionsToLocation(false);
					}
				} else {
					showDirectionsToLocation(false);
				}
			} else {
				showDirectionsToLocation(false);
			}
		}

		private async Task initLocationWallMessages() {
			if (await mLocationPostsController.loadLocationPosts()) {
				List<LocationPostWrapper> locationPosts = await mLocationPostsController.getWrappedLocationPosts();

				if (locationPosts != null) {

					if (locationPosts.Count > 0) {
						mWallPosts = new ObservableCollection<LocationPostWrapper>(locationPosts);
						listLocationWall.ItemsSource = mWallPosts;

						if (mWallPosts.Count - 1 > 0) {
							listLocationWall.ScrollTo(mWallPosts[mWallPosts.Count - 1], ScrollToPosition.End, false);
						}

						if (mWallPosts.Count > 0) {
							numberOfMessages.Text = mWallPosts.Count.ToString();
							showWallMessages(true);
						} else {
							showWallMessages(false);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						showWallMessages(false);
					}
				} else {
					showWallMessages(false);
				}
			} else {
				showWallMessages(false);
			}
		}

		private async Task initUsersAtLocation() {
			showUsersAtLocation(false);

			if (mLocation != null) {

				if (mLocation.id != null) {
					List<UserLocation> usersLocationRels = await mLocationController.getUserLocationsByLocationId(mLocation.id);

					if (usersLocationRels != null) {

						if (usersLocationRels.Count > 0) {
							var users = new List<User>();

							foreach (UserLocation userLocation in usersLocationRels) {

								if (userLocation.userId != null) {
									User user = await mUserController.getUserByUID(userLocation.userId);

									if (!userListContainsItem(users, user)) {
										users.Add(user);
									}
								}
							}

							mUsersAtLocation = new ObservableCollection<User>(users);
							listUsersAtLocation.ItemsSource = mUsersAtLocation;

							if (mUsersAtLocation.Count > 0) {
								numberOfPlayers.Text = usersLocationRels.Count.ToString();
								showUsersAtLocation(true);
							}
						}
					}
				}
			}
		}

		private bool userListContainsItem(List<User> users, User user) {
			foreach (User listUser in users) {

				if (listUser != null && user != null) {

					if (listUser.uid != null && user.uid != null) {

						if (listUser.uid.Equals(user.uid)) {
							return true;
						}
					}
				}
			}

			return false;
		}

		private void initFormSubmitActualNumberUsers() {
			if (mLocation != null && mCurrentLocation != null) {
				var distance = GeoCoordinatesUtils.distance(
											  mCurrentLocation.getLatitude(),
											  mCurrentLocation.getLongitude(),
											  FormatUtils.stringToDouble(mLocation.localLatitude),
											  FormatUtils.stringToDouble(mLocation.localLongitude),
											  Constants.MEASURE_KM);

				if (distance < Constants.DISTANCE_THRESHOLD_CHECKED_IN_LOCATION_RADIUS) {
					formSubmitActualNumberUsersAtLocation.IsVisible = true;
				} else {
					formSubmitActualNumberUsersAtLocation.IsVisible = false;
				}
			} else {
				formSubmitActualNumberUsersAtLocation.IsVisible = false;
			}
		}
		#endregion


		#region Business Logic
		private void selectTab1(object sender, EventArgs e) {
			selectTab(IDX_TAB_DIRECTIONS_TO_LOCATION);
		}

		private void selectTab2(object sender, EventArgs e) {
			selectTab(IDX_TAB_CHAT);
		}

		private void selectTab3(object sender, EventArgs e) {
			selectTab(IDX_TAB_PEOPLE_AT_LOCATION);
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

					numberOfMessages.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberOfPlayers.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabCSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorIndicator.IsVisible = true;
					tabBSelectorIndicator.IsVisible = false;
					tabCSelectorIndicator.IsVisible = false;

                    navigateToUserLocation().ConfigureAwait(false);
					drawPathToLocation();

					break;

				case 2:
					tabA.IsVisible = false;
					tabB.IsVisible = true;
					tabC.IsVisible = false;

					tabASelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);
					tabCSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);

					numberOfMessages.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					numberOfPlayers.Opacity = Constants.IMAGE_ALPHA_DISABLED;

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

					numberOfMessages.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberOfPlayers.Opacity = Constants.IMAGE_ALPHA_ENABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabCSelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;

					tabASelectorIndicator.IsVisible = false;
					tabBSelectorIndicator.IsVisible = false;
					tabCSelectorIndicator.IsVisible = true;

					break;
			}

			showHeaderContainerBasedOnSelectedTab();
		}

		private void showHeaderContainerBasedOnSelectedTab() {
			if (tabA.IsVisible) {
				displayMapOrHeaderImages(true);

			} else if (tabB.IsVisible) {
				displayMapOrHeaderImages(false);

			} else if (tabC.IsVisible) {
				displayMapOrHeaderImages(false);
			}
		}

		private void displayMapOrHeaderImages(bool showMap) {
			headerContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			if (mMapController != null) {
				if (mMapController.map != null) {
					mMapController.map.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;
				}
			}	

			if (showMap) {
				mapContainer.IsVisible = true;
				locationImage.IsVisible = false;
			} else {
				mapContainer.IsVisible = false;
				locationImage.IsVisible = true;
			}
		}

		private void showDirectionsToLocation(bool show) {
			if (show) {
				containerLblDirectionsToLocation.IsVisible = true;
				viewDirectionsToLocation.IsVisible = true;
				containerLblDirectionsToLocationSummary.IsVisible = true;
				emptyMsgDirectionsToLocationContainer.IsVisible = false;
			} else {
				containerLblDirectionsToLocation.IsVisible = false;
				containerLblDirectionsToLocationSummary.IsVisible = false;
				viewDirectionsToLocation.IsVisible = false;
				emptyMsgDirectionsToLocationContainer.IsVisible = true;
			}
		}

		private void showWallMessages(bool show) {
			if (show) {
				viewLocationWall.IsVisible = true;
				emptyMsgWallMessagesContainer.IsVisible = false;
			} else {
				viewLocationWall.IsVisible = false;
				emptyMsgWallMessagesContainer.IsVisible = true;
			}
		}

		private void showUsersAtLocation(bool show) {
			if (show) {
				listUsersAtLocation.IsVisible = true;
				emptyMsgUsersAtLocationContainer.IsVisible = false;
			} else {
				listUsersAtLocation.IsVisible = false;
				emptyMsgUsersAtLocationContainer.IsVisible = true;
			}
		}

		public void showNumPlayersReportInput(object sender, EventArgs e) {
			lblWrongNumberPlayers.IsVisible = false;
			btnWrongNumPlayers.IsVisible = false;
			entNumPlayers.IsVisible = true;
			btnSubNumPlayers.IsVisible = true;
			btnResetNumberOfPlayersViews.IsVisible = true;
		}

		public void resetDialogInformNumberPlayers(object sender, EventArgs e) {
			hideNumberPlayersReportInput();
		}

		private void hideNumberPlayersReportInput() {
			lblWrongNumberPlayers.IsVisible = true;
			btnWrongNumPlayers.IsVisible = true;
			entNumPlayers.IsVisible = false;
			btnSubNumPlayers.IsVisible = false;
			btnResetNumberOfPlayersViews.IsVisible = false;
			entNumPlayers.Text = "";
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

		public async Task navigateToUpdateLocation() {
			await Navigation.PushAsync(new CreateUpdateLocation(mLocation));
		}

		public async void toggleUserFavoriteLocationRelationship(object sender, EventArgs e) {
			if (mUser.id != null && mLocation.id != null) {
				mBasePage.showLoadingSpinner("");

				if (mIsFavorite) {
					await mUserController.deleteUserFavoriteLocationRelationship(
					mUser.uid, mLocation.id);
				} else {
					await mUserController.createUserFavoriteLocationRelationship(
					mUser.uid, mLocation.id);
				}

				await mBasePage.hideLoadingSpinner();
				initFavoriteStatus();
			}
		}

		public async Task addMessageToWall() {
			if (mUser != null && mLocation != null) {

				if (mUser.id != null && mLocation.id != null &&
					mLocation.name != null && !inputNewMessage.Text.Equals("")) {

					string locationId = mLocation.id;
					string userId = mUser.id;
					string title = mLocation.name;
					string text = inputNewMessage.Text;
					DateTime date = DateTime.UtcNow;
					string sportId = "";

					if (mSport != null) {
						if (mSport.id != null) {
							sportId = mSport.id;
						}
					}

					inputNewMessage.Text = "";

					await mLocationPostsController.addLocationPost(locationId, userId, sportId, title, text, date);
					await initLocationWallMessages();
				}
			}
		}

		public async void submitReportNumPlayers(object sender, EventArgs e) {
			int reportedNumUsers = 0;

			if (int.TryParse(entNumPlayers.Text, out reportedNumUsers)) {

				if (mUser != null && mLocation != null) {

					if (mUser.id != null && mLocation.id != null) {
						mBasePage.showLoadingSpinner("");

						await mLocationController.createFeedbackNumUsersAtLocation(
								reportedNumUsers,
								mUser.uid,
								mLocation.id);

						hideNumberPlayersReportInput();
						await mBasePage.hideLoadingSpinner();
						await DisplayAlert(Txt.TITLE_NICE, Txt.LBL_THANK_USER_FEEDBACK, Txt.LBL_BTN_OK);
					}
				}
			} else {
				await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_INSERT_VALID_NUMBER, Txt.LBL_BTN_OK);
			}
		}

		public async Task deleteLocation() {
			var answer = await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_ARE_U_SURE_DELETE_LOCATION,
											Txt.LBL_BTN_YES, Txt.LBL_BTN_NO);
			if (answer) {

				if (mLocation != null) {

					if (mLocation.id != null) {
						mBasePage.showLoadingSpinner("");

						if (await mLocationController.deleteLocation(mLocation.id)) {
							await DisplayAlert("", Txt.MSG_DELETED_LOCATION, Txt.LBL_BTN_OK);
						} else {
							await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_FAILED_TO_DELETE_LOCATION, Txt.LBL_BTN_OK);
						}

						await mBasePage.hideLoadingSpinner();
						await Navigation.PopToRootAsync();
					}
				}
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
		}

		private void releaseMap() {
			if (mMapController != null) {
				if (mMapController.map != null) {
					mapContainer.Children.Remove(mMapController.map);
					mMapController.map = null;
				}
			}

			mMapController = null;
		}
		#endregion

	}

}