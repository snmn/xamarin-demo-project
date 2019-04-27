using System;
using Xamarin.Forms;

namespace SportsConnection {
	
	public partial class NoConnectionContainer : ContentView {

		private CallbackNoConnectionContainerTryRefresh mCallback;


		public NoConnectionContainer() {
			InitializeComponent();
		}

		public void init(CallbackNoConnectionContainerTryRefresh callback) {
			mCallback = callback;
			noConnectionContainer.IsVisible = false;
		}

		public void displayNoInternetContainer() {
			noConnectionContainer.IsVisible = true;
			noConnectionMsg.IsVisible = true;
			noConnectionMsg.Text = Txt.MSG_NO_CONNECTIVITY;
		}

		public void hideNoInternetContainer() {
			noConnectionContainer.IsVisible = false;
		}

		protected void tryRefreshContainers(object sender, EventArgs args) {
			mCallback.tryRefreshContainers();
		}

		public void releaseUI() {
			imgNoConnection.BindingContext = null;
			imgNoConnection.Source = null;
		}

		public void rebuildUI() {
			imgNoConnection.Source = Constants.IMAGE_ICO_NO_INTERNET_WHITE;
		}


		// Interfaces
		//...........
		public interface CallbackNoConnectionContainerTryRefresh {
			void tryRefreshContainers();
		}

	}

}