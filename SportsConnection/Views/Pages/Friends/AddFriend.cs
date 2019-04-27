using System;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class AddFriend : ContentPage {

		private UserRelationshipsController mUserRelsController = new UserRelationshipsController();

		private User mUser;
		private bool mCurrentContainerIsSearchResults = false;

		private BasePage mBasePage;


		#region Initialization
		public AddFriend() {
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
			initBroadcastMessagesFriendRelationshipsListener();
			initSearchBarTextListener();
			initUserDetailsListener();
		}

		private void initBroadcastMessagesFriendRelationshipsListener() {
			MessagingCenter.Subscribe<FriendshipMessage>(this, FriendshipMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {

					if (message != null) {
						DebugHelper.newMsg(Constants.TAG_ADD_FRIEND_PAGE, "User accepted friendship request");
					}
				});
			});
		}

		private void initSearchBarTextListener() {
			searchInput.TextChanged += (sender, e) => {
				if (searchInput.Text == "") {
					if (mCurrentContainerIsSearchResults) {
						msgEmptyListSearchResults.Text = Txt.MSG_EMPTY_LIST_RECOMMENDED_FRIENDS;
						mCurrentContainerIsSearchResults = false;
						mUserRelsController.searchResults.Clear();
						mUserRelsController.recommendedFriends.Clear();
					}

					reloadData();
				} else {
					if (!mCurrentContainerIsSearchResults) {
						msgEmptyListSearchResults.Text = Txt.MSG_DID_NOT_FIND_MATCH_QUERY;
						mCurrentContainerIsSearchResults = true;
						listSearchResults.ItemsSource = mUserRelsController.searchResults;
					}

					searchFriend(sender, e);
				}
			};
		}

		private void initUserDetailsListener() {
			listSearchResults.ItemSelected += (sender, e) => {
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
			initRecommendedFriends().ConfigureAwait(false);
			initFacebookFriends().ConfigureAwait(false);
		}

		private void initCurrentUserAttributes() {
			if (mUser == null) {
				mUser = App.authController.getCurrentUser();
			}
		}

		private void reloadData() {
			showSearchResultsContainer(true);
			initRecommendedFriends().ConfigureAwait(true);
		}

		private async Task initRecommendedFriends() {
			mBasePage.showLoadingSpinner("");

			if (mUser != null) {

				if (await mUserRelsController.loadUserRelationships(mUser)) {

					if (mUserRelsController.recommendedFriends.Count > 0) {
						listSearchResults.ItemsSource = mUserRelsController.recommendedFriends;

						if (mUserRelsController.recommendedFriends.Count > 0) {
							showSearchResults(true);
						} else {
							showSearchResults(false);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						await mBasePage.hideLoadingSpinner();
						showSearchResults(false);
					}
				} else {
					await mBasePage.hideLoadingSpinner();
					showSearchResults(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showSearchResults(false);
			}
		}

		private async Task initFacebookFriends() {
			mBasePage.showLoadingSpinner("");

			if (mUser != null) {

				// Todo: >>>> Get list of Facebook friends
				// ..
				//./Todo

			} else {
				await mBasePage.hideLoadingSpinner();
				showSearchResults(false);
			}
		}

		private void initViews() {
			initToolbar();
		}

		private void initToolbar() {
			Title = Txt.LBL_TITLE_ADD_FRIEND;
		}
		#endregion


		#region BusinessLogic
		private void showSearchResults(bool show) {
			if (show) {
				listSearchResults.IsVisible = true;
				emptyMsgSearchResultsContainer.IsVisible = false;
			} else {
				listSearchResults.IsVisible = false;
				emptyMsgSearchResultsContainer.IsVisible = true;
			}
		}

		private void showFacebookFriends(bool show) {
			if (show) {
				listFacebookFriends.IsVisible = true;
				emptyMsgFacebookFriendsContainer.IsVisible = false;
			} else {
				listFacebookFriends.IsVisible = false;
				emptyMsgFacebookFriendsContainer.IsVisible = true;
			}
		}

		private void showSearchResultsContainer(bool show) {
			if (show) {
				pageSecSearchResults.IsVisible = true;
				pageSecFacebookFriends.IsVisible = false;
			} else {
				pageSecSearchResults.IsVisible = false;
				pageSecFacebookFriends.IsVisible = true;
			}
		}

		public async void navigateToUserDetails(object sender, EventArgs e) {
			var selectedUser = (User)((Frame)sender).BindingContext;

			if (selectedUser != null) {
				await Navigation.PushAsync(new Profile(selectedUser, false));
			}
		}

		public async void inviteFacebookFriends(object sender, EventArgs e) {
			await DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_FEATURE_WILL_BE_AVAILABLE_SOON, Txt.LBL_BTN_OK);
		}

		private async void searchFriend(object sender, EventArgs e) {
			showSearchResultsContainer(true);

			if (mUser != null) {
				mUserRelsController.loadSearchResults(searchInput.Text, mUser.uid);

				if (mUserRelsController.searchResults.Count > 0) {
					showSearchResults(true);
				} else {
					showSearchResults(false);
				}
			} else {
				await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_ERROR_COULD_NOT_FIND_ACCOUNT, Txt.LBL_BTN_OK);
				await Navigation.PopAsync();
			}
		}

		public async void sendFriendshipInvitation(object sender, EventArgs e) {
			try {
				var selectedUser = (User)((CachedImage)sender).BindingContext;
				((CachedImage)sender).Source = Constants.IMAGE_ICO_CHECKED_BOX;

				if (selectedUser != null) {
					await mUserRelsController.addOrConfirmFriend(mUser, selectedUser);
					mUserRelsController.removeUserFromSearchResultsAndRecommendedFriends(selectedUser);
					searchInput.Text = "";

					await DisplayAlert("", Txt.MSG_SENT_FRIENDSHIP_INVITATION, Txt.LBL_BTN_OK);
				}
			} catch (Exception) {
				await DisplayAlert(Txt.TITLE_NICE, Txt.MSG_FAILED_SEND_FRIENDSHIP_INVITATION, Txt.LBL_BTN_OK);
				DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, Txt.MSG_FAILED_SEND_FRIENDSHIP_INVITATION);
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
		}
		#endregion
	}

}