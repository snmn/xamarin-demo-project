using Xamarin.Forms;

namespace SportsConnection {

	public partial class Splash : ContentPage, BasePage.BasePageInterface {

		#region Initialization
		public Splash() {
			InitializeComponent();
		}
		#endregion


		#region ReInitialization
		/// <summary>
		/// Page Life cycle method
		/// 
		/// The method rebuild the page content every time the page is resumed.
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
				DisplayAlert(Txt.TITLE_ATTENTION, Txt.MSG_NO_CONNECTIVITY, Txt.LBL_BTN_OK);
			}
		}

		/// <summary>
		/// BasePageInterface method
		/// </summary>
		public void enableAndRebuildPageContent() {
			initToolbar();
			initImages();
			animateLogoAndLoading();
		}

		private void initToolbar() {
			NavigationPage.SetHasNavigationBar(this, false);
		}

		private void initImages() {
			bgImage.Source = Constants.IMAGE_BG_AUTH;
			imgLogoSportsConnect.Source = Constants.IMAGE_ICO_LOGO;
		}

		private void animateLogoAndLoading() {
			imgLogoSportsConnect.FadeTo(1, Constants.TIMEOUT_STD_FADE_IN_ANIMATION, Easing.CubicInOut).ConfigureAwait(true);
			loadingSpinner.FadeTo(1, Constants.TIMEOUT_STD_FADE_IN_ANIMATION, Easing.CubicInOut).ConfigureAwait(true);
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
			bgImage.Source = null;
			imgLogoSportsConnect.Source = null;
		}
		#endregion

	}

}