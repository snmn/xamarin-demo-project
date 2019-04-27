using Xamarin.Forms;
using FFImageLoading.Forms;
using System.Collections.Generic;

namespace SportsConnection {
	
	public partial class Menu : ContentPage {

		public static readonly int MAIN_MENU_OPT_INDEX_SPLASH = -1;
		public static readonly int MAIN_MENU_OPT_INDEX_PROFILE;
		public static readonly int MAIN_MENU_OPT_INDEX_LOCATIONS = 1;
		public static readonly int MAIN_MENU_OPT_INDEX_YOUR_LOCATIONS = 2;
		public static readonly int MAIN_MENU_OPT_INDEX_YOUR_FRIENDS = 3;

		private List<MenuItem> mMainOptions;
		private List<MenuItem> mExtraOptions;


		public Menu() {
			InitializeComponent();
			initProfileOptions();
			initMainOptions();
			initExtraOptions();
			adjustMenuAccordingToOS();
		}

		private void initProfileOptions() {
			profileImage.Source = Constants.IMAGE_PLACEHOLDER_USER;

			var profileMenuItem = (new MenuItem {
				title = Constants.LBL_MENU_OPT_PROFILE,
				targetType = typeof(Profile),
				iconSource = Constants.IMAGE_PLACEHOLDER_USER
			});

			profilePic.BindingContext = profileImage;
			userName.BindingContext = profileMenuItem;
		}

		private void initMainOptions() {
			mMainOptions = new List<MenuItem>();

			mMainOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_PROFILE,
				targetType = typeof(Locations),
				iconSource = Constants.IMAGE_ICO_PROFILE_ORANGE
			});

			mMainOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_LOCATIONS,
				targetType = typeof(Locations),
				iconSource = Constants.IMAGE_ICO_MAP
			});

			mMainOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_YOUR_LOCATIONS,
				targetType = typeof(LocationsManagement),
				iconSource = Constants.IMAGE_ICO_LOCATION
			});

			mMainOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_FRIENDS,
				targetType = typeof(Friends),
				iconSource = Constants.IMAGE_ICO_FRIENDS
			});

			menuMainOptions.ItemsSource = mMainOptions;
		}

		private void initExtraOptions() {
			mExtraOptions = new List<MenuItem>();

			mExtraOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_SETTINGS,
				targetType = typeof(Settings),
				iconSource = Constants.IMAGE_ICO_SETTINGS
			});

			mExtraOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_ABOUT_US,
				targetType = typeof(About),
				iconSource = Constants.IMAGE_ICO_INFO
			});

			mExtraOptions.Add(new MenuItem {
				title = Constants.LBL_MENU_OPT_SIGNOUT,
				targetType = typeof(Settings),
				iconSource = Constants.IMAGE_ICO_SIGN_OUT
			});

			extraMenuOptions.ItemsSource = mExtraOptions;
		}

		private void adjustMenuAccordingToOS() {
			if (PlataformUtils.getPlataform().Equals(PlataformUtils.PLATAFORM_ANDROID)) {

				if (PlataformUtils.getAndroidVersion() >= 23) {
					profileMenuOption.Margin = new Thickness(0, Constants.STATUS_BAR_HEIGHT, 0, 0);			
				}
			}
		}

		/// <summary>
		/// Getters and Setters
		/// </summary>
		public CachedImage profileImage {
			set {
				profilePic = value;
			}
			get {
				return profilePic;
			}
		}

		public Label profileInfo {
			get {
				return userName;
			}
		}

		public List<MenuItem> mainMenu {
			get {
				return mMainOptions;
			}
		}

		public List<MenuItem> extraMenu {
			get {
				return mExtraOptions;
			}
		}

		public ListView mainMenuOptions {
			get {
				return menuMainOptions;
			}
		}

		public ListView extraMenuOptions {
			get {
				return menuExtraOptions;
			}
		}

	}

}