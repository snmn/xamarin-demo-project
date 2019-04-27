using System;
using System.Linq;
using SportsConnection.iOS;
using Xamarin.Auth;
using Xamarin.Forms;


[assembly: Dependency(typeof(AuthControllerIOS))]
namespace SportsConnection.iOS {
	
	public class AuthControllerIOS : IAuthUtils {

		public string username {
			get {
				var account = AccountStore.Create().FindAccountsForService(Constants.APP_NAME)
										  .FirstOrDefault();
				return (account != null) ? account.Username : null;
			}
		}

		public string password {
			get {
				var account = AccountStore.Create().FindAccountsForService(Constants.APP_NAME)
										  .FirstOrDefault();
				return (account != null) ? account.Properties[Constants.PROP_AUTH_LOC_PASSWORD] : null;
			}
		}

		public string authOpt {
			get {
				var account = AccountStore.Create().FindAccountsForService(Constants.APP_NAME)
										  .FirstOrDefault();
				return (account != null) ? account.Properties[Constants.PROP_AUTH_LOC_PROVIDER] : null;
			}
		}

		public void saveCredentials(string username, string password, string authOpt) {
			if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) {
				Account account = new Account {
					Username = username
				};

				account.Properties.Add(Constants.PROP_AUTH_LOC_PASSWORD, password);
				account.Properties.Add(Constants.PROP_AUTH_LOC_PROVIDER, authOpt);
				AccountStore.Create().Save(account, Constants.APP_NAME);
			}
		}

		public void deleteCredentials() {
			var account = AccountStore.Create().FindAccountsForService(Constants.APP_NAME)
									  .FirstOrDefault();
			if (account != null) {
				AccountStore.Create().Delete(account, Constants.APP_NAME);
			}
		}

		public bool doCredentialsExist() {
			return AccountStore.Create().FindAccountsForService(Constants.APP_NAME)
							   .Any() ? true : false;
		}

	}

}