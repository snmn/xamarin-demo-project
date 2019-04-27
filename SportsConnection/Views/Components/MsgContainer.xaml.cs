using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {
	
	public partial class MsgContainer : ContentView {


		public MsgContainer() {
			InitializeComponent();
		}

		public void init(TapGestureRecognizer tapToHideEvent) {
			msgContainer.GestureRecognizers.Add(tapToHideEvent);
			msgTxt.Text = "";
			msgImg.Source = Constants.IMAGE_ICO_INFO;
		}

		public void displayMsgContainer(string msg, string icon) {
			msgContainer.IsVisible = true;
			msgTxt.Text = msg;

			if (icon != null) {
				msgImg.Source = icon;
				msgImg.IsVisible = true;
			} else {
				msgImg.IsVisible = false;
			}
		}

		public void hideMsgContainer(object sender, EventArgs args) {
			msgContainer.IsVisible = false;
		}

		public void showLoadingSpinner(string msg) {
			msgContainer.IsVisible = true;
			msgImg.IsVisible = false;

			if (msg != null) {
				msgTxt.IsVisible = true;
				msgTxt.Text = msg;
			} else {
				msgTxt.IsVisible = false;
			}

			loadingSpinner.IsVisible = true;
			loadingSpinner.IsRunning = true;
		}

		public async Task<bool> hideLoadingSpinner() {
			// Make sure the user is going to able to read the message before removing it
			if (msgTxt.IsVisible) {
				await Task.Delay(Constants.TIMEOUT_HIDE_LOADING_MIN_WAITING_TIME);
			}

			// Stop the spinner
			loadingSpinner.IsRunning = false;
			loadingSpinner.IsVisible = false;

			// Handle the containers vibility
			msgContainer.IsVisible = false;

			return true;
		}

		public void releaseUI() {
			msgImg.BindingContext = null;
			msgImg.Source = null;
		}

		public void rebuildUI() {
			msgImg.Source = Constants.IMAGE_ICO_INFO;
		}

	}

}