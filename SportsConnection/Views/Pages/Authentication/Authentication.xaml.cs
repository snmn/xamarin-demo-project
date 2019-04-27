
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {

	public partial class Authentication : ContentPage, BasePage.BasePageInterface {

		private BasePage mBasePage;
		private Action<bool> mMasterDetailAuthCallback;


		#region Initialization
		public Authentication(Action<bool> masterDetailAuthCallback) {
			mMasterDetailAuthCallback = masterDetailAuthCallback;
			InitializeComponent();
			initBasePageComponents();
			enableAndRebuildPageContent();
			initListeners();
		}

		private void initBasePageComponents() {
			mBasePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
		}

		private void initListeners() {
			var btnFacebookTapGestureRecognizer = new TapGestureRecognizer();
			btnFacebookTapGestureRecognizer.Tapped += (s, e) => {
				signInWithFacebook().ConfigureAwait(true);
			};

			var btnTwitterTapGestureRecognizer = new TapGestureRecognizer();
			btnTwitterTapGestureRecognizer.Tapped += (s, e) => {
				signInWithTwitter().ConfigureAwait(true);
			};

			var btnGooglePlusTapGestureRecognizer = new TapGestureRecognizer();
			btnGooglePlusTapGestureRecognizer.Tapped += (s, e) => {
				signInWithGoogle().ConfigureAwait(true);
			};

			btnLoginWithFacebook.GestureRecognizers.Add(btnFacebookTapGestureRecognizer);
			btnLoginWithTwitter.GestureRecognizers.Add(btnTwitterTapGestureRecognizer);
			btnLoginWithGooglePlus.GestureRecognizers.Add(btnGooglePlusTapGestureRecognizer);
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
			} else {
				mBasePage.hideNoConnectionContainer();
			}
		}

		/// <summary>
		/// BasePageInterface method
		/// 
		/// Unlock and refresh the page content.
		/// </summary>
		public void enableAndRebuildPageContent() {
			initMainComponents();
			initImages();
		}

		private void initMainComponents() {
			mainContainer.IsEnabled = true;
			mBasePage.rebuildUI();
		}

		private void initImages() {
			bgImage.Source = Constants.IMAGE_BG_AUTH;
			imgSportsConnectLogo.Source = Constants.IMAGE_ICO_LOGO;
			btnLoginWithFacebook.Source = Constants.IMAGE_ICO_FACEBOOK;
			btnLoginWithTwitter.Source = Constants.IMAGE_ICO_TWITTER;
			btnLoginWithGooglePlus.Source = Constants.IMAGE_ICO_GOOGLE_PLUS;
		}
		#endregion


		#region Business Logic
		private async Task signInWithFacebook() {
			if (NetworkUtils.isOnline()) {

				if (App.authController.isUserAuthenticated()) {

					if (App.authController.isFacebookAccountEnabled()) {
						DebugHelper.newMsg(Constants.TAG_AUTH_PAGE,
										   string.Format(Txt.FORMAT_MSG_ALREADY_AUTHENTICATED,
										   App.authController.getCurrentUser().name));

						onAuthProcessCompleted(true);
					} else {
						await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_NO_CONNECTIVITY, Txt.LBL_BTN_OK);
					}
				} else {
					mBasePage.showLoadingSpinner(Txt.MSG_LOADING_STUFF);
					Action<bool> authCallback = onAuthProcessCompleted;
					await App.authController.checkUserCredentialsAndSignin(authCallback, Constants.AUTH_OPT_FACEBOOK);
				}
			} else {
				mBasePage.displayNoInternetContainer();
			}
		}

		private async Task signInWithTwitter() {
			if (NetworkUtils.isOnline()) {

				if (App.authController.isUserAuthenticated()) {

					if (App.authController.getCurrentUser() != null) {
						DebugHelper.newMsg(Constants.TAG_AUTH_PAGE,
										   string.Format(Txt.FORMAT_MSG_ALREADY_AUTHENTICATED,
										   App.authController.getCurrentUser().name));

						onAuthProcessCompleted(true);
					} else {
						await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_NO_CONNECTIVITY, Txt.LBL_BTN_OK);
					}
				} else {
					mBasePage.showLoadingSpinner(Txt.MSG_LOADING_STUFF);
					Action<bool> authCallback = onAuthProcessCompleted;
					await App.authController.checkUserCredentialsAndSignin(authCallback, Constants.AUTH_OPT_TWITTER);
				}
			} else {
				mBasePage.displayNoInternetContainer();
			}
		}

		private async Task signInWithGoogle() {
			if (NetworkUtils.isOnline()) {

				if (App.authController.isUserAuthenticated()) {

					if (App.authController.getCurrentUser() != null) {
						DebugHelper.newMsg(Constants.TAG_AUTH_PAGE,
										   string.Format(Txt.FORMAT_MSG_ALREADY_AUTHENTICATED,
										   App.authController.getCurrentUser().name));

						onAuthProcessCompleted(true);
					} else {
						await DisplayAlert(Txt.TITLE_GENERAL_INFORMATION_INFORMAL, Txt.MSG_NO_CONNECTIVITY, Txt.LBL_BTN_OK);
					}
				} else {
					mBasePage.showLoadingSpinner(Txt.MSG_LOADING_STUFF);
					Action<bool> authCallback = onAuthProcessCompleted;
					await App.authController.checkUserCredentialsAndSignin(authCallback, Constants.AUTH_OPT_GOOGLE);
				}
			} else {
				mBasePage.displayNoInternetContainer();
			}
		}

		/// <summary>
		/// Perform the callback action of the authentication by pass.
		/// </summary>
		public void onAuthProcessCompleted(bool success) {
			mBasePage.hideLoadingSpinner().ConfigureAwait(false);

			if (success) {
				if (mMasterDetailAuthCallback != null) {
					mMasterDetailAuthCallback(true);
					Navigation.PopModalAsync();
				}
			} else if (!NetworkUtils.isOnline()) {
				mBasePage.displayNoInternetContainer();
			}
		}
		#endregion


		#region Desctructor
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
		/// Release the UI to free memory
		/// </summary>
		public void disableAndRelasePageContent() {
			mainContainer.IsEnabled = false;
			mBasePage.releaseUI();
			releaseUI();
		}

		/// <summary>
		/// BasePageInterface method
		/// 
		/// Lock and release the page content.
		/// </summary>
		private void releaseUI() {
			bgImage.Source = null;
			imgSportsConnectLogo.Source = null;
			btnLoginWithFacebook.Source = null;
			btnLoginWithTwitter.Source = null;
			btnLoginWithGooglePlus.Source = null;
		}
		#endregion

	}

}