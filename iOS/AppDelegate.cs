using System;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

using Foundation;
using UIKit;
using FFImageLoading.Forms.Touch;

namespace SportsConnection.iOS{
	
	[Register ("AppDelegate")]
	public class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IAuth{

		public override bool FinishedLaunching (UIApplication uiApplication, NSDictionary launchOptions){
			Xamarin.Forms.Forms.Init ();

			CurrentPlatform.Init();

            // IMPORTANT: uncomment this code to enable sync on Xamarin.iOS
            // For more information, see: http://go.microsoft.com/fwlink/?LinkId=620342
            SQLitePCL.CurrentPlatform.Init();

			// Register the authenticatite method
			AuthUserController.init(this);

			// Init the image loader library
			CachedImageRenderer.Init();


			LoadApplication (new App ());

			return base.FinishedLaunching (uiApplication, launchOptions);
		}

		/// <summary>
		/// Authenticate the specified authOpt.
		/// 
		/// #pragma CS1701:
		/// Azure Mobile services, rely on an old version of the Newtonsoft.Json framework, but I'm assuming the
		/// current version of the framework provides backward compatibility.
		/// </summary>
		public async Task<MobileServiceUser> authenticate(string authOpt) {
			try {
				if (App.authController.getAzureUser() == null) {

					MobileServiceUser user = null;

                    /*
					if (authOpt == Constants.AUTH_OPT_FACEBOOK) {
                        user = await AzureController.client.LoginAsync(
                            MobileServiceAuthenticationProvider.Facebook,
                            UIApplication.SharedApplication.KeyWindow.RootViewController);
                        
					} else if (authOpt == Constants.AUTH_OPT_GOOGLE) {
                        user = await AzureController.client.LoginAsync(
							MobileServiceAuthenticationProvider.Google,
                            UIApplication.SharedApplication.KeyWindow.RootViewController);

					} else if (authOpt == Constants.AUTH_OPT_TWITTER) {
                        user = await AzureController.client.LoginAsync(
                            MobileServiceAuthenticationProvider.Twitter,
                            UIApplication.SharedApplication.KeyWindow.RootViewController);
					}
					*/

					if (user != null) {
						return user;
					} else {
						var authAlert = new UIAlertView("Authentication", "You are now logged in "
														+ App.authController.getAzureUser().UserId, 
						                                null, "OK", null);
						authAlert.Show();
					}
				}
			} catch (Exception ex) {
				var authAlert = new UIAlertView("Authentication failed", ex.Message, null, "OK", null);
				authAlert.Show();
			}

			return null;
		}

	}

}