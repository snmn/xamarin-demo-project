using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {
	
	public partial class About : ContentPage {

		public BasePage basePage;


		public About() {
			InitializeComponent();
			initBasicComponents();
		}

		private void initBasicComponents() {
			if (basePage == null) {
				basePage = new BasePage(pageContainer, mainContainer, msgContainer, noConnectionContainer);
			}
		}

		/// <summary>
		/// Page Life cycle method
		/// 
		/// The method rebuild the page content every time the page is resumed including the first time it is executed.
		/// </summary>
		protected override void OnAppearing() {
			base.OnAppearing();
			PlataformUtils.tryToReleaseMemory();
			initViews();
		}
	
		private void initViews() {
			basePage.showLoadingSpinner("");

			Title = Txt.TITLE_ABOUT;
			browser.Source = Constants.WEBSITE_URL;

			waitAndExecute(Constants.TIMEOUT_LOAD_ABOUT_PAGE, () => basePage.hideLoadingSpinner().ConfigureAwait(false)).ConfigureAwait(false);
		}

		private async Task waitAndExecute(int milisec, Action actionToExecute) {
			await Task.Delay(milisec);
			actionToExecute();
		}

		/// <summary>
		/// Page Life cycle method
		/// 
		/// The methods below release the page content everytime it is destroyed. it is 
		/// important to release memory and avoid OutOfMemory errors on Android devices, due to the lack of 
		/// efficiency of the Java Garbage Collector that must be called explicitly after the disposal of the 
		/// elements.
		/// </summary>
		protected override void OnDisappearing() {
			PlataformUtils.tryToReleaseMemory();
			base.OnDisappearing();
		}

	}

}