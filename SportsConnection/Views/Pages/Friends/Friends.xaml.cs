using System;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class Friends : ContentPage {

		private readonly int IDX_TAB_FRIENDS = 1;
		private readonly int IDX_TAB_FRIENDSHIP_REQUESTS = 2;

		private UserRelationshipsController mUserRelsController = new UserRelationshipsController();

		private User mUser;
		private int mSelectedTab = 1;

		private BasePage mBasePage;


		#region Initialization
		public Friends() {
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
			initOnFriendClickListener();
			initOnRequestedFriendClickListener();
			initBtnAddFriendListener();
		}

		private void initBroadcastMessagesFriendRelationshipsListener() {
			MessagingCenter.Subscribe<FriendshipMessage>(this, FriendshipMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {

					if (message != null) {
						enableAndRebuildPageContent();
					}
				});
			});
		}

		private void initOnFriendClickListener() {
			listFriends.ItemSelected += (sender, e) => {
				var selectedUser = (User)e.SelectedItem;

				if (selectedUser != null) {
					if (selectedUser.uid != null) {
						Navigation.PushModalAsync(new Profile(selectedUser, true)).ConfigureAwait(true);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initOnRequestedFriendClickListener() {
			listFriendshipRequests.ItemSelected += (sender, e) => {
				var selectedUser = (User)e.SelectedItem;

				if (selectedUser != null) {
					if (selectedUser.uid != null) {
						Navigation.PushModalAsync(new Profile(selectedUser, false)).ConfigureAwait(true);
					}
				}

				((ListView)sender).SelectedItem = null;
			};
		}

		private void initBtnAddFriendListener() {
			var tapAddFriend = new TapGestureRecognizer();

			tapAddFriend.Tapped += (object sender, EventArgs args) => {
				Navigation.PushAsync(new AddFriend());
			};

			btnAddFriend.GestureRecognizers.Add(tapAddFriend);
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
			selectTab(mSelectedTab);
		}

		private void initData() {
			initCurrentUserAttributes();
			initFriends().ConfigureAwait(false);
			initFriendshipRequests().ConfigureAwait(false);
		}

		private void initCurrentUserAttributes() {
			if (mUser == null) {
				mUser = App.authController.getCurrentUser();
				mUserRelsController.loadUserRelationships(mUser).ConfigureAwait(true);
			}
		}

		private async Task initFriends() {
			mBasePage.showLoadingSpinner("");
			numberFriends.Text = "";

			if (mUser != null) {

				if (await mUserRelsController.loadUserRelationships(mUser)) {

					if (mUserRelsController.userFriends.Count > 0) {
						listFriends.ItemsSource = mUserRelsController.userFriends;

						if (mUserRelsController.userFriends.Count > 0) {
							numberFriends.Text = mUserRelsController.userFriends.Count.ToString();
							showFriends(true);
						} else {
							numberFriends.Text = "";
							showFriends(false);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						await mBasePage.hideLoadingSpinner();
						showFriends(false);
					}
				} else {
					await mBasePage.hideLoadingSpinner();
					showFriends(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showFriends(false);
			}
		}

		private async Task initFriendshipRequests() {
			mBasePage.showLoadingSpinner("");
			numberFriendshipRequests.Text = "";

			if (mUser != null) {

				if (await mUserRelsController.loadUserRelationships(mUser)) {

					if (mUserRelsController.pendingCurrentUserApproval.Count > 0) {
						listFriendshipRequests.ItemsSource = mUserRelsController.pendingCurrentUserApproval;

						if (mUserRelsController.pendingCurrentUserApproval.Count > 0) {
							numberFriendshipRequests.Text = mUserRelsController.pendingCurrentUserApproval.Count.ToString();
							showFriendshipRequests(true);
						} else {
							numberFriendshipRequests.Text = "";
							showFriendshipRequests(false);
						}

						if (mUserRelsController.userFriends.Count == 0 &&
						   mUserRelsController.pendingCurrentUserApproval.Count > 0) {
							selectTab(IDX_TAB_FRIENDSHIP_REQUESTS);
						}

						await mBasePage.hideLoadingSpinner();
					} else {
						await mBasePage.hideLoadingSpinner();
						showFriendshipRequests(false);
					}
				} else {
					await mBasePage.hideLoadingSpinner();
					showFriendshipRequests(false);
				}
			} else {
				await mBasePage.hideLoadingSpinner();
				showFriendshipRequests(false);
			}
		}

		private void initViews() {
			initImages();
			initToolbar();
		}

		private void initImages() {
			tabASelectorImage.Source = Constants.IMAGE_ICO_PEOPLE_BLACK;
			tabBSelectorImage.Source = Constants.IMAGE_ICO_FRIENSHIP_REQUEST_BLACK;
		}

		private void initToolbar() {
			Title = Txt.LBL_TITLE_FRIENDS;
		}
		#endregion


		#region BusinessLogic
		private void selectTab1(object sender, EventArgs e) {
			selectTab(IDX_TAB_FRIENDS);
			mSelectedTab = 1;
		}

		private void selectTab2(object sender, EventArgs e) {
			selectTab(IDX_TAB_FRIENDSHIP_REQUESTS);
			mSelectedTab = 2;
		}

		private void selectTab(int tabNumber) {
			switch (tabNumber) {
				case 1:
					tabA.IsVisible = true;
					tabB.IsVisible = false;
					
					tabASelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					
					numberFriends.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					numberFriendshipRequests.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;

					tabASelectorIndicator.IsVisible = true;
					tabBSelectorIndicator.IsVisible = false;

					Title = Txt.LBL_TITLE_FRIENDS;

				break;

				case 2:
					tabA.IsVisible = false;
					tabB.IsVisible = true;
					
					tabASelector.BackgroundColor = Color.FromHex(Colors.LIGHT_GRAY);
					tabBSelector.BackgroundColor = Color.FromHex(Colors.MEDIUM_GRAY);

					numberFriends.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					numberFriendshipRequests.Opacity = Constants.IMAGE_ALPHA_ENABLED;

					tabASelectorImage.Opacity = Constants.IMAGE_ALPHA_DISABLED;
					tabBSelectorImage.Opacity = Constants.IMAGE_ALPHA_ENABLED;
					
					tabASelectorIndicator.IsVisible = false;
					tabBSelectorIndicator.IsVisible = true;

					Title = Txt.LBL_FRIENDSHIP_REQUESTS;

				break;
			}
		}

		private void showFriends(bool show) {
			if (show) {
				viewFriends.IsVisible = true;
				emptyMsgFriendsContainer.IsVisible = false;
			} else {
				viewFriends.IsVisible = false;
				emptyMsgFriendsContainer.IsVisible = true;
			}
		}

		private void showFriendshipRequests(bool show) {
			if (show) {
				viewFriendshipRequests.IsVisible = true;
				emptyMsgFriendshipRequests.IsVisible = false;
			} else {
				viewFriendshipRequests.IsVisible = false;
				emptyMsgFriendshipRequests.IsVisible = true;
			}
		}

		public async void confirmFriendshipRequest(object sender, EventArgs e) {
			try {
				var selectedUser = (User)((CachedImage)sender).BindingContext;

				if (selectedUser != null) {
					mBasePage.showLoadingSpinner("");

					await mUserRelsController.addOrConfirmFriend(mUser, selectedUser);
					enableAndRebuildPageContent();
					await initFriendshipRequests();

					await mBasePage.hideLoadingSpinner();
				}
			} catch (Exception) {
				await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_FAILED_CONFIRM_FRIENDSHIP, Txt.LBL_BTN_OK);
				DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, Txt.MSG_FAILED_CONFIRM_FRIENDSHIP);
			}
		}

		public async void removeFriendship(object sender, EventArgs e) {
			try {
				var selectedUser = (User)((CachedImage)sender).BindingContext;

				if (selectedUser != null) {
					mBasePage.showLoadingSpinner("");

					await mUserRelsController.unfriend(mUser.uid, selectedUser.uid);
					enableAndRebuildPageContent();
					await initFriendshipRequests();

					await mBasePage.hideLoadingSpinner();
				}
			} catch (Exception) {
				await DisplayAlert(Txt.TITLE_OOPS, Txt.MSG_FAILED_DELETE_FRIENDSHIP, Txt.LBL_BTN_OK);
				DebugHelper.newMsg(Constants.TAG_USER_RELATIONSHIP_CONTROLLER, Txt.MSG_FAILED_DELETE_FRIENDSHIP);
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
		}
		#endregion

	}

}