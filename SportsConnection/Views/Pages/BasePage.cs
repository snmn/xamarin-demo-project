using System.Threading.Tasks;
using Xamarin.Forms;

namespace SportsConnection {
	
	public class BasePage : NoConnectionContainer.CallbackNoConnectionContainerTryRefresh {

		private AbsoluteLayout mPageContainer;
		private StackLayout mMainContainer;
		private MsgContainer mMsgLoadingContainer;
		private NoConnectionContainer mNoConnectionContainer;


		public BasePage(AbsoluteLayout pageContainer, StackLayout mainContainer, MsgContainer msgContainer, 
		                NoConnectionContainer noConnectionContainer) {
			mPageContainer = pageContainer;
			mMainContainer = mainContainer;
			mMsgLoadingContainer = msgContainer;
			mNoConnectionContainer = noConnectionContainer;

			init();
		}

		public void init() {
			mNoConnectionContainer.init(this);
			displayMainContainer();
		}

		/// <summary>
		/// Set the base page object to its default configuration.
		/// </summary>
		public void displayMainContainer() {
			mPageContainer.IsVisible = true;
			mMainContainer.IsVisible = true;
			mMsgLoadingContainer.IsVisible = false;
			mNoConnectionContainer.IsVisible = false;
		}

		/// <summary>
		/// Manages the visibility of the NoInternetConnection Container.
		/// </summary>
		public void displayNoInternetContainer() {
			mMainContainer.IsVisible = true;
			mMsgLoadingContainer.IsVisible = false;
			mNoConnectionContainer.IsVisible = true;
			mNoConnectionContainer.displayNoInternetContainer();
		}

		public void hideNoConnectionContainer() {
			displayMainContainer();
		}

		/// <summary>
		/// Manages the visibility of the MsgContainer
		/// </summary>
		public void displayMsgContainer(string msg, string icon) {
			mMainContainer.IsVisible = true;
			mMsgLoadingContainer.IsVisible = true;
			mNoConnectionContainer.IsVisible = false;
			mMsgLoadingContainer.displayMsgContainer(msg, icon);
		}

		public async Task<bool> hideMsgContainer() {
			await mMsgLoadingContainer.FadeTo(0, Constants.TIMEOUT_STD_FADE_OUT_ANIMATION, Easing.CubicInOut);
			displayMainContainer();

			return true;
		}

		/// <summary>
		/// Manages the visibility of the Loading dialog.
		/// </summary>
		public void showLoadingSpinner(string msg) {
			if (!mMsgLoadingContainer.IsVisible) {
				mPageContainer.IsVisible = true;
				mMainContainer.IsVisible = true;
				mMsgLoadingContainer.IsVisible = true;
				mNoConnectionContainer.IsVisible = false;
				mMsgLoadingContainer.showLoadingSpinner(msg);
			}
		}

		public async Task<bool> hideLoadingSpinner() {
			if (mMsgLoadingContainer.IsVisible) {
				await mMsgLoadingContainer.hideLoadingSpinner();
				displayMainContainer();
			}

			return true;
		}

		/// <summary>
		/// This method is called by the NoInternetConnection component and by the MsgContainer when a state 
		/// refresh is necessary.
		/// </summary>
		public void tryRefreshContainers() {
			displayMainContainer();
		}

		/// <summary>
		/// Apply a memory usage management pocilicy to the components of the base page.
		/// </summary>
		public void releaseUI() {
			mNoConnectionContainer.releaseUI();
			mMsgLoadingContainer.releaseUI();
		}

		public void rebuildUI() {
			mNoConnectionContainer.rebuildUI();
			mMsgLoadingContainer.rebuildUI();
		}

		/// <summary>
		/// The BasePageInterface defines methods that must be implemented by all of its clients. Ideally this
		/// methods would be in a BasePageContent, and the methods below would be implemented by inheritance 
		/// by each instance of BasePageContent. However Xamarin.Forms forces a ContentPage to always require 
		/// a layout for the code behind, therefore we would have trouble injecting the layout of each page.
		/// </summary>
		public interface BasePageInterface {
			void disableAndRelasePageContent();
			void enableAndRebuildPageContent();
			void checkConnectivity();
		}

	}

}