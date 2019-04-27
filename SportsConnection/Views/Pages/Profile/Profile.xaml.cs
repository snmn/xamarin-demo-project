using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace SportsConnection {
	
	public partial class Profile : ContentPage {
		
		private readonly int IDX_TAB_RECENT_LOCATIONS = 1;
		private readonly int IDX_TAB_BIO = 2;
		private readonly int IDX_TAB_FRIENDS = 3;

		private UserController mUserController = new UserController();
		private LocationController mLocationController = new LocationController();
		private UserRelationshipsController mUserRelsController = new UserRelationshipsController();

		private ObservableCollection<UserLocationWrapper> mRecentLocations;
		private ObservableCollection<User> mUserFriends;

		private User mUser;
		private bool mIsCurrentUser;
		private bool mIsFriend;

		private BasePage mBasePage;
		private readonly TapGestureRecognizer mLocationNameTapGestureRecognizer = new TapGestureRecognizer();


		#region Initialization
		public Profile(User user, bool isFriend) {
			mUser = user;
			mIsFriend = isFriend;

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
			initOnRecentLocationSelectedClickListener();
			initOnFriendSelectedClickListener();
			initCleanDescriptionListener();
			initBtnSaveDescriptionListener();
		}

		private void initLocationNameTapGestureListener() {
			mLocationNameTapGestureRecognizer.Tapped += (s, e) => {
				toggleHeaderVisibility();
			};

			userNameContainer.GestureRecognizers.Add(mLocationNameTapGestureRecognizer);
		}

		private void initOnRecentLocationSelectedClickListener() {
			listRecentLocations.ItemSelected += (sender, e) => {
				var selectedUserLocRel = (UserLocationWrapper)e.SelectedItem;

				if (selectedUserLocRel != null) {
                    
					if (selectedUserLocRel.location != null) {
                        Navigation.PushAsync(new LocationDetails(selectedUserLocRel.location)).ConfigureAwait(true);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initOnFriendSelectedClickListener() {
			listUserFriends.ItemSelected += (sender, e) => {
				var selectedUser = (User)e.SelectedItem;

				if (selectedUser != null) {
					if (selectedUser.uid != null) {
						Navigation.PushModalAsync(new Profile(selectedUser, false)).ConfigureAwait(true);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initCleanDescriptionListener() {
			var tapUserDescription = new TapGestureRecognizer();

			tapUserDescription.Tapped += (object sender, EventArgs args) => {
				cleanUserDescription();
			};

			btnCleanUserDescription.GestureRecognizers.Add(tapUserDescription);
		}

		private void initBtnSaveDescriptionListener() {
			var tapSaveUserDescription = new TapGestureRecognizer();

			tapSaveUserDescription.Tapped += (object sender, EventArgs args) => {
				saveUserDescription().ConfigureAwait(false);
			};

			btnSaveUserDescription.GestureRecognizers.Add(tapSaveUserDescription);
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
			selectTab(IDX_TAB_BIO);
		}

		private void initData() {
			initCurrentUserAttributes();
			initRecentLocations().ConfigureAwait(false);
			initUserBio();
			initUserFriends().ConfigureAwait(false);
		}

		private void initCurrentUserAttributes() {
			if (mUser != null && App.authController.getCurrentUser() != null) {
				mUserRelsController.loadUserRelationships(mUser).ConfigureAwait(true);

				if (mUser.uid != null && App.authController.getCurrentUser().uid != null) {
					
					if (mUser.uid == App.authController.getCurrentUser().uid) {
						mIsCurrentUser = true;
					}
				} else {
					mIsCurrentUser = false;
				}
			} else {
				mIsCurrentUser = false;
			}
		}

		private async Task initRecentLocations() {
			mBasePage.showLoadingSpinner("");

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

		private void initUserBio() {
			if (mUser != null) {
				if (mUser.profileImage != null) {
					userPicture.Source = mUser.profileImage;
				}

				if (mUser.name != null) {
					userName.Text = mUser.name;
				}

				if (mIsCurrentUser) {
					lblBio.Text = Txt.LBL_PROFILE_ABOUT_YOU;
					lblEmptyMsgUserFriends.Text = Txt.MSG_EMPTY_LIST_USER_FRIENDS_CURRENT_USER;
				} else {
					lblBio.Text = Txt.LBL_PROFILE_ABOUT_USER;
					lblEmptyMsgUserFriends.Text = Txt.MSG_EMPTY_LIST_USER_FRIENDS;
				}

				if (mUser.uid != null) {
					lblBio.Text = mUser.uid;
				}

			

				if (mIsCurrentUser) {
					showEditBio(true);
				} else {
					showEditBio(false);
				}

				showBio(true);
			} else {
				showEditBio(false);
				showBio(false);
			}
		}

		private async Task initUserFriends() {
			mBasePage.showLoadingSpinner("");

			if (mUser != null) {
				List<User> userFriends = await mUserRelsController.getListUserFriends(mUser);

				if (userFriends != null) {
					mUserFriends = new ObservableCollection<User>(userFriends);
					listUserFriends.ItemsSource = mUserFriends;

					if (mUserFriends.Count > 0) {
						numberFriends.Text = mUserFriends.Count.ToString();
						showUserFriends(true);
					} else {
						numberFriends.Text = "";
						showUserFriends(false);
					}

					await mBasePage.hideLoadingSpinner();
				} else {
					await mBasePage.hideLoadingSpinner();
					showUserFriends(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showUserFriends(false);
			}
		}

		private void initViews() {
			initImages();
			initToolbar();
			initHeader();
			initBottomActionBar();
		}

		private void initImages() {
			tabASelectorImage.Source = Constants.IMAGE_ICO_RECENT_LOCATIONS;
			tabBSelectorImage.Source = Constants.IMAGE_ICO_PROFILE_BLACK;
			tabCSelectorImage.Source = Constants.IMAGE_ICO_PEOPLE_BLACK;
			tabActionUnfriendImage.Source = Constants.IMAGE_ICO_UNFRIEND_WHITE;
			tabActionBlockImage.Source = Constants.IMAGE_ICO_BLOCK_USER_WHITE;
		}

		private void initToolbar() {
			Title = Txt.LBL_TITLE_PROFILE;
		}

		private void initHeader() {
			headerContainer.IsVisible = false;
			headerContainer.WidthRequest = SettingsController.getPhoneWidth();
			headerContainer.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;
			headerContainer.MinimumHeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			bgUserPicture.WidthRequest = SettingsController.getPhoneWidth();
			bgUserPicture.HeightRequest = SettingsController.getPhoneWidth() * Constants.MULTIPLIER_PAGE_HEADER_HEIGHT;

			toggleHeaderVisibility();
		}

		private void initBottomActionBar() {
			if (!mIsFriend) {
				bottomActionBar.IsVisible = false;
			} else {
				bottomActionBar.IsVisible = !mIsCurrentUser;
			}
		}
		#endregion


		#region BusinessLogic
		private void selectTab1(object sender, EventArgs e) {
			selectTab(IDX_TAB_RECENT_LOCATIONS);
		}

		private void selectTab2(object sender, EventArgs e) {
			selectTab(IDX_TAB_BIO);
		}

		private void selectTab3(object sender, EventArgs e) {
			selectTab(IDX_TAB_FRIENDS);
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

			numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_ENABLED;
			numberFriends.Opacity = Constants.IMAGE_ALPHA_DISABLED;
		
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

			numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
			numberFriends.Opacity = Constants.IMAGE_ALPHA_DISABLED;

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

			numberRecentLocations.Opacity = Constants.IMAGE_ALPHA_DISABLED;
			numberFriends.Opacity = Constants.IMAGE_ALPHA_ENABLED;

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

				var scaleUpMapAnimation = new Animation(v => headerContainer.Scale = v, 0, 1, Easing.SpringOut);
				headerAnimation.Add(0.3, 1, scaleUpMapAnimation);

				headerAnimation.Commit(headerContainer,
							   Constants.ANIMATION_HEADER_LOCATION_DETAILS,
							   Constants.ANIMATION_DEFAULT_INTERVAL_BETWEEN_ANIMATIONS,
							   600,
							   null,
							   null);
			} else {
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

		private void hideHeader() {
			headerContainer.IsVisible = false;
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

		private void showBio(bool show) {
			if (show) {
				viewBio.IsVisible = true;
				emptyMsgUserBio.IsVisible = false;
			} else {
				viewBio.IsVisible = false;
				emptyMsgUserBio.IsVisible = true;
			}
		}

		private void showEditBio(bool show) {
			if (show) {
				viewEditBio.IsVisible = true;
				viewShowBio.IsVisible = false;
			} else {
				viewEditBio.IsVisible = false;
				viewShowBio.IsVisible = true;
			}
		}

		private void showUserFriends(bool show) {
			if (show) {
				viewUserFriends.IsVisible = true;
				emptyMsgUserFriends.IsVisible = false;
			} else {
				viewUserFriends.IsVisible = false;
				emptyMsgUserFriends.IsVisible = true;
			}
		}

		public async void navigateToUserDetails(object sender, EventArgs e) {
			var selectedUser = (User)((Frame)sender).BindingContext;

			if (selectedUser != null) {
				await Navigation.PushAsync(new Profile(selectedUser, false));
			}
		}

		private void cleanUserDescription() {
			userDescription.Text = "";
		}

		private async Task saveUserDescription() {
			if (mUser != null) {
				mBasePage.showLoadingSpinner("");

				

				if (!await mUserController.updateUser(mUser)) {
					await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_FAILED_UPDATE_PROFILE, Txt.LBL_BTN_OK);
				}

				await mBasePage.hideLoadingSpinner();
			}
		}

		private async void actionUnfriendUser(object sender, EventArgs e) {
			var answer = await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_ARE_U_SURE_UNFRIEND_USER,
											Txt.LBL_BTN_YES, Txt.LBL_BTN_NO);
			if (answer) {
				try {
					if (!mIsCurrentUser && mUser != null) {
						mBasePage.showLoadingSpinner("");
						await mUserRelsController.unfriend(App.authController.getCurrentUser().uid, mUser.uid);
						await mBasePage.hideLoadingSpinner();
						await Navigation.PopModalAsync();
					}
				} catch (Exception) {
					await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_FAILED_DELETE_FRIENDSHIP, Txt.LBL_BTN_OK);
					DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, Txt.MSG_FAILED_DELETE_FRIENDSHIP);
				}
			}
		}

		private async void actionBlockUser(object sender, EventArgs e) {
			var answer = await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_ARE_U_SURE_BLOCK_USER,
											Txt.LBL_BTN_YES, Txt.LBL_BTN_NO);
			if (answer) {
				try {
					if (!mIsCurrentUser && mUser != null) {
						mBasePage.showLoadingSpinner("");
						await mUserRelsController.blockFriend(App.authController.getCurrentUser().uid, mUser.uid);
						await mBasePage.hideLoadingSpinner();
						await Navigation.PopModalAsync();
					}
				} catch (Exception) {
					await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_FAILED_DELETE_FRIENDSHIP, Txt.LBL_BTN_OK);
					DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, Txt.MSG_FAILED_DELETE_FRIENDSHIP);
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
			releaseImages();
		}

		private void releaseImages() {
			tabASelectorImage.Source = null;
			tabBSelectorImage.Source = null;
			tabCSelectorImage.Source = null;
			tabActionUnfriendImage.Source = null;
			tabActionBlockImage.Source = null;
		}
		#endregion

	}

}