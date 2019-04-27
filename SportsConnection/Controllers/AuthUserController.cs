using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using System;
using Newtonsoft.Json;

namespace SportsConnection {

	public class AuthUserController {

		private UserManager mUserManager;

		private static IAuth mAuthenticator { get; set; }  // Must be implemented by each plataform, to perform login
		private IAuthUtils mLocalAuthStorage;              // Must be implemented by each plataform, to store credentials
		private string mCurrentAuthProvider;               // Can contain one of the these values: Constants.AUTH_OPT_FACEBOOK || Constants.AUTH_OPT_GOOGLE || Constants.AUTH_OPT_TWITTER
		private Action<bool> mAuthCallback;                // Callback of the authentication flow

		private User mCurrentUser;
		private FacebookUser mFacebookUser;
		private TwitterUser mTwitterUser;
		private GoogleUser mGoogleUser;

		private bool mAuthenticated;
		private bool mIsAuthenticating;


		public AuthUserController() {
			initLocalAuthStorage();
		}

		#region AuthenticationFlow
		/// <summary>
		/// Inits the local auth storage which keeps saved the user access credentials. The values are saved
		/// with a cryptographic key.
		/// </summary>
		public void initLocalAuthStorage() {
			mLocalAuthStorage = DependencyService.Get<IAuthUtils>();
		}

		/// <summary>
		/// Init the Azure authenticator object with an implementation made on each plataform
		/// </summary>
		/// <param name="auth">An instance of the authenticator interface</param>
		public static void init(IAuth auth) {
			setPlataformAuthenticator(auth);
		}

		public bool verifyUserCredentials() {
			return !TokenExtension.isTokenExpired(AzureController.client);
		}

		/// <summary>
		/// This is the first connection of this controller with the view layer. This method tries to by pass the 
		/// login process and return the result to the page which called the login function.
		/// </summary>
		public async Task verifyUserCredentialsAndTryByPass(Action<bool> callback) {
			setAuthProcessCallback(callback);
			setIsUserAuthenticating(true);

			if (doesUserHaveValidCredentials()) {
				
				if (getAuthProcessCallback() != null) {
					await checkUserCredentialsAndSignin(getAuthProcessCallback(), getCurrentAuthProvider());
				}

			} else if (getAuthProcessCallback() != null) {
				getAuthProcessCallback()(false);
			}
		}

		/// <summary>
		/// This method is the second connection between the controller and the view layer. It checks if the user 
		/// has valid access credentials, if they don't, starts the authentication flow until the they get it.
		/// 
		/// If the authentication token has expired, and there is a netwok access, call the sign in page,
		/// otherwise, finish the authentication proccess, because the token must be renewed.
		/// </summary>
		public async Task checkUserCredentialsAndSignin(Action<bool> callback, string authOpt) {
			setAuthProcessCallback(callback);

			if (doesUserHaveValidCredentials()) {

				if (TokenExtension.isTokenExpired(AzureController.client)) {

					if (NetworkUtils.isOnline()) {
						await trySignIn(authOpt);
					} else {
						if (getAuthProcessCallback() != null) {
							getAuthProcessCallback()(false);
						}
					}
				} else {
					await tryUpdateUserInfo();
				}
			} else {
				await trySignIn(authOpt);
			}
		}

		private async Task trySignIn(string authOpt) {
			if (await signIn(authOpt)) {
				await tryUpdateUserInfo();
			} else {
				if (getAuthProcessCallback() != null) {
					getAuthProcessCallback()(false);
				}
			}
		}

		private async Task tryUpdateUserInfo() {
			setAuthenticationStatus(true);

			if (NetworkUtils.isOnline()) {
				await updateUserSocialInfo();
			} else {
				await recoverUserSocialInfo();
			}
		}

		/// <summary>
		/// Call the authentication method on each plataform, Load the user statically from there and 
		/// get the operation result on the authenticated variable.
		/// </summary>
		public async Task<bool> signIn(string authOpt) {
			if (authOpt == null) {

				if (getAuthProcessCallback() != null) {
					getAuthProcessCallback()(false);
				}

				return false;
			}

			setCurrentAuthProvider(authOpt);

			if (isUserAuthenticated()) {
				setAuthenticationStatus(true);
				App.authController.setAzureUser(AzureController.client.CurrentUser, getCurrentAuthProvider());

			} else {
				MobileServiceUser user = null;

				if (authOpt == Constants.AUTH_OPT_FACEBOOK) {
					user = await getPlataformAuthenticator().authenticate(Constants.AUTH_OPT_FACEBOOK);

				} else if (authOpt == Constants.AUTH_OPT_FACEBOOK) {
					user = await getPlataformAuthenticator().authenticate(Constants.AUTH_OPT_FACEBOOK);

				} else if (authOpt == Constants.AUTH_OPT_FACEBOOK) {
					user = await getPlataformAuthenticator().authenticate(Constants.AUTH_OPT_FACEBOOK);
				}

				if (user != null) {
					App.authController.setAzureUser(user, authOpt);
					setAuthenticationStatus(true);
				} else {
					setAuthenticationStatus(false);
				}
			}

			return isUserAuthenticated();
		}

		/// <summary>
		/// Get information from one of the Authenticator provider to update the current user info
		/// </summary>
		public async Task updateUserSocialInfo() {
			try {
				var request = new RequestUserInfo();
				request.user = getAzureUser();

				if (AzureController.client != null) {
					var response = await AzureController.client.InvokeApiAsync<RequestUserInfo, BasicResponse>(
						Constants.API_PATH_GET_EXTRA_INFO_FROM_IDT_PROVIDER,
						request);

					if (response != null) {

						if (response.message != null) {
							var socialUserJSON = JToken.Parse(response.message);

							if (socialUserJSON != null) {
								var updatedUser = new User();

								if (getCurrentAuthProvider() == Constants.AUTH_OPT_FACEBOOK) {
									var facebookUser = await SocialInfoUtils.fetchAndGetFacebookUser(socialUserJSON);

									if (facebookUser != null) {
										setFacebookUser(facebookUser);

										updatedUser.uid = getFacebookUser().emailAddress;
										updatedUser.facebookId = getFacebookUser().userId;
										updatedUser.name = getFacebookUser().name;
										updatedUser.profileImage = getFacebookUser().pictureUrl;

										saveFacebookUserLocally(getFacebookUser());
									}

								} 

								

								if (updatedUser.uid != null) {
									await setCurrentUser(updatedUser, false);
								} else {
									await setCurrentUser(null, false);
								}
							} else {
								DebugHelper.newMsg(Constants.TAG_ROOT, "Failed to get social info from authentication provider.");
								await setCurrentUser(null, false);
							}
						} else {
							DebugHelper.newMsg(Constants.TAG_ROOT, "Failed to get social info from authentication provider.");
							await setCurrentUser(null, false);
						}
					} else {
						DebugHelper.newMsg(Constants.TAG_ROOT, "Failed to get social info from authentication provider.");
						await setCurrentUser(null, false);
					}
				} else {
					DebugHelper.newMsg(Constants.TAG_ROOT, "Failed to make request to load social networks.");
					await setCurrentUser(null, false);
				}
			} catch (Exception e) {
				DebugHelper.newMsg(Constants.TAG_ROOT, e.Message + e.StackTrace);
				await setCurrentUser(null, false);
			}
		}

		/// <summary>
		/// The user doens't have an internet connection, but have a valid token, so we restore the last social information
		/// used to login an let the user access the system.
		/// </summary>
		public async Task recoverUserSocialInfo() {
			var recoveredUser = new User();

			if ((getLastUsedSocialAccount() == Constants.AUTH_OPT_FACEBOOK) &&
				(getFacebookUserFromLocalProperties() != null)) {
				var facebookUser = getFacebookUserFromLocalProperties();

				if (facebookUser != null) {
					setFacebookUser(facebookUser);

					recoveredUser.uid = getFacebookUser().emailAddress;
					recoveredUser.facebookId = getFacebookUser().userId;
					recoveredUser.name = getFacebookUser().name;
					recoveredUser.profileImage = getFacebookUser().pictureUrl;

					saveFacebookUserLocally(getFacebookUser());
				}

			} 

			if (recoveredUser.uid != null) {
				await setCurrentUser(recoveredUser, true);
			} else {
				await setCurrentUser(null, true);
			}
		}

		/// <summary>
		/// Deletes the access token provided by the authentication service to the user.
		/// </summary>
		public async Task<bool> signOut() {
			// Delete access token remotelly
			await AzureController.client.LogoutAsync();

			// Delete access token locally
			if (getLocalSafeStorage() != null) {
				getLocalSafeStorage().deleteCredentials();
			}

			// Reset the authentication status
			setAuthenticationStatus(false);

			// Reset the current user and all of its attributes
			mCurrentUser = null;
			setFacebookUser(null);
			setTwitterUser(null);
			setGoogleUser(null);

			return true;
		}

		/// <summary>
		/// This method must be called at the end of the authentication flow, to inform the controller that the
		/// user info has been loaded into the UI. This information can be used as a flag to validated the user data
		/// during the authentication process.
		/// </summary>
		public void finishAuthenticationProcess() {
			setIsUserAuthenticating(false);
		}
		#endregion


		#region AuthenticationState
		/// <summary>
		/// Check if the user has credentials stored locally and restore them into the authentication user.
		/// </summary>
		public bool doesUserHaveValidCredentials() {
			if (getLocalSafeStorage().doCredentialsExist()) {
				// Restore variables related to authentication
				setCurrentAuthProvider(getLocalSafeStorage().authOpt);

				// Restore the access credentials with values from the local Authentication Storage
				string userId = getLocalSafeStorage().username;
				string token = getLocalSafeStorage().password;

				// Update the current user's credentials
				var user = new MobileServiceUser(userId);
				user.MobileServiceAuthenticationToken = token;

				// Update the Azure object currently authenticated
				App.authController.setAzureUser(user, getCurrentAuthProvider());

				return true;
			}

			return false;
		}

		/// <summary>
		/// Manage the state flags that control the authentication status of the current user.
		/// </summary>
		public void setAuthenticationStatus(bool authStatus) {
			mAuthenticated = authStatus;
		}

		public bool isUserAuthenticated() {
			return mAuthenticated;
		}

		public void setIsUserAuthenticating(bool status) {
			mIsAuthenticating = status;
		}

		public bool isAuthenticating() {
			return mIsAuthenticating;
		}

		/// <summary>
		/// Check if the current user has activated the Facebook account
		/// </summary>
		public bool isFacebookAccountEnabled() {
			if (getCurrentUser() != null) {

				if (getCurrentUser().facebookId != null) {
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		/// <summary>
		/// Check if the current user already have a valid account from another identify provider.
		/// </summary>
	

		/// <summary>
		/// Return an instance of the object that stores attributes locally encrypting they information.
		/// </summary>
		/// <returns>The local safe storage.</returns>
		private IAuthUtils getLocalSafeStorage() {
			return mLocalAuthStorage;
		}
		#endregion


		#region GettersAndSetters
		/// <summary>
		/// Set and Get the object responsible for the login process on each plataform. This method must
		/// be executed on each plataform because the authentication library needs the Plataform context.
		/// </summary>
		private static void setPlataformAuthenticator(IAuth plataformAuthenticator) {
			mAuthenticator = plataformAuthenticator;
		}

		private static IAuth getPlataformAuthenticator() {
			return mAuthenticator;
		}

		/// <summary>
		/// Set and Get the object that is going to handle the response of the login attempt
		/// </summary>
		private void setAuthProcessCallback(Action<bool> callback) {
			mAuthCallback = callback;
		}

		private Action<bool> getAuthProcessCallback() {
			return mAuthCallback;
		}

		/// <summary>
		/// Set and Get the name of the auth provider that is currenlty being used.
		/// </summary>
		private void setCurrentAuthProvider(string provider) {
			mCurrentAuthProvider = provider;
		}

		private string getCurrentAuthProvider() {
			return mCurrentAuthProvider;
		}

		/// <summary>
		/// Sets the azure user. The Azure user is used by Azure Mobile Services HTTP client to send requests, this
		/// object must be used only for managing the authentication status of the current user, to get informations
		/// of the currently logged SportsConnect user, user the `getCurrentUser()` method.
		/// </summary>
		public void setAzureUser(MobileServiceUser user, string authOpt) {
			if (user != null) {
				// Initializes the HTTP client with the authenticated user
				AzureController.client.CurrentUser = user;
				GeofencingTask.client.CurrentUser = user;

				// Store the access credentials on the local Auth storage
				getLocalSafeStorage().saveCredentials(
					user.UserId,
					AzureController.client.CurrentUser.MobileServiceAuthenticationToken,
					authOpt
				);
			}
		}

		/// <summary>
		/// Returns the user that manages the authentication credentials of the HTTP client. This user must be used
		/// only for authentication related processes.
		/// </summary>
		public MobileServiceUser getAzureUser() {
			return AzureController.client.CurrentUser;
		}

		/// <summary>
		/// Create or update the current SportsConnect user information, based on the social information gathered 
		/// from the one of the authentication providers.
		/// 
		/// This method makes sure that the app is going to work only if there is a valid user object.
		/// </summary>
		public async Task setCurrentUser(User user, bool recovering) {
			if (user != null) {
				mUserManager = new UserManager();

				if (!recovering) {
					await mUserManager.upsertUserAsync(user, user.uid);
				}

				user = await mUserManager.getUserByUID(user.uid);

				if (user != null) {
					mCurrentUser = user;
					GeofencingTask.setCurrentUser(getCurrentUser());
					saveLastUsedSocialAccount(getCurrentIdentifyProvider());

					if (getAuthProcessCallback() != null) {
						getAuthProcessCallback()(true);
					}
				} else {
					await signOut();
					mCurrentUser = null;

					if (getAuthProcessCallback() != null) {
						getAuthProcessCallback()(false);
					}
				}
			} else {
				await signOut();
				mCurrentUser = null;

				if (getAuthProcessCallback() != null) {
					getAuthProcessCallback()(false);
				}
			}
		}

		/// <summary>
		/// Gets the current user. This method must be used all over the system to manage the currently logged 
		/// SportsConnect user. 
		/// </summary>
		public User getCurrentUser() {
			return mCurrentUser;
		}

		/// <summary>
		/// Return the identify provider of the current user.
		/// </summary>
		public string getCurrentIdentifyProvider() {
			if (getCurrentUser() != null) {

				
					return Constants.AUTH_OPT_FACEBOOK;
				
			} else {
				return null;
			}
		}

		/// <summary>
		/// Manages the social information of the current user
		/// </summary>
		public void setFacebookUser(FacebookUser user) {
			mFacebookUser = user;
		}

		public FacebookUser getFacebookUser() {
			return mFacebookUser;
		}

		public void setTwitterUser(TwitterUser user) {
			mTwitterUser = user;
		}

		public TwitterUser getTwitterUser() {
			return mTwitterUser;
		}

		public void setGoogleUser(GoogleUser user) {
			mGoogleUser = user;
		}

		public GoogleUser getGoogleUser() {
			return mGoogleUser;
		}
		#endregion


		#region LocallPersistency
		/// <summary>
		/// Manage the social profiles of the current user in the local storage.
		/// </summary>
		public void saveFacebookUserLocally(FacebookUser socialUser) {
			var jsonUser = JsonConvert.SerializeObject(socialUser);
			Application.Current.Properties[Constants.PROP_FACEBOOK_USER] = jsonUser;
		}

		public FacebookUser getFacebookUserFromLocalProperties() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_FACEBOOK_USER)) {
				var jsonUser = (string)Application.Current.Properties[Constants.PROP_FACEBOOK_USER];

				if (jsonUser != null) {
					try {
						var recFacebookUser = JsonConvert.DeserializeObject<FacebookUser>(jsonUser);
						return recFacebookUser;
					} catch (InvalidCastException) {
						return null;
					}
				}
			}

			return null;
		}

		public void saveGoogleUserLocally(GoogleUser socialUser) {
			var jsonUser = JsonConvert.SerializeObject(socialUser);
			Application.Current.Properties[Constants.PROP_GOOGLE_USER] = jsonUser;
		}

		public GoogleUser getGoogleUserFromLocalProperties() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_GOOGLE_USER)) {
				var jsonUser = (string)Application.Current.Properties[Constants.PROP_GOOGLE_USER];

				if (jsonUser != null) {
					try {
						var recGoogleUser = JsonConvert.DeserializeObject<GoogleUser>(jsonUser);
						return recGoogleUser;
					} catch (InvalidCastException) {
						return null;
					}
				}
			}

			return null;
		}

		public void saveTwitterUserLocally(TwitterUser socialUser) {
			var jsonUser = JsonConvert.SerializeObject(socialUser);
			Application.Current.Properties[Constants.PROP_TWITTER_USER] = jsonUser;
		}

		public TwitterUser getTwitterUserFromLocalProperties() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_TWITTER_USER)) {
				var jsonUser = (string)Application.Current.Properties[Constants.PROP_TWITTER_USER];

				if (jsonUser != null) {
					try {
						var recTwitterUser = JsonConvert.DeserializeObject<TwitterUser>(jsonUser);
						return recTwitterUser;
					} catch (InvalidCastException) {
						return null;
					}
				}
			}

			return null;
		}

		public void saveLastUsedSocialAccount(string socialNetwork) {
			Application.Current.Properties[Constants.PROP_LAST_USED_SOCIAL_ACC] = socialNetwork;
		}

		public string getLastUsedSocialAccount() {
			if (Application.Current.Properties.ContainsKey(Constants.PROP_LAST_USED_SOCIAL_ACC)) {
				var socialNetwork = (string)Application.Current.Properties[Constants.PROP_LAST_USED_SOCIAL_ACC];

				if (socialNetwork != null) {
					return socialNetwork;
				}
			}

			return null;
		}
		#endregion

	}

}