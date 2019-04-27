using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Auth;
using Xamarin.Forms;

using Microsoft.WindowsAzure.MobileServices;

using SportsConnection.Droid;

[assembly: Dependency(typeof(AuthControllerDroid))]
namespace SportsConnection.Droid {

	public class AuthControllerDroid : IAuthUtils {

		/// <summary>
		/// Getters and Setters for the encripted user account information stored locally.
		/// </summary>
		public string username {
			get {
#pragma warning disable CS0618 // Type or member is obsolete
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(Constants.APP_NAME)
#pragma warning restore CS0618 // Type or member is obsolete
                                          .FirstOrDefault();
				return (account != null) ? account.Username : null;
			}
		}

		public string password {
			get {
#pragma warning disable CS0618 // Type or member is obsolete
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(Constants.APP_NAME)
#pragma warning restore CS0618 // Type or member is obsolete
                                          .FirstOrDefault();
				return (account != null) ? account.Properties[Constants.PROP_AUTH_LOC_PASSWORD] : null;
			}
		}

		public string authOpt {
			get {
#pragma warning disable CS0618 // Type or member is obsolete
                var account = AccountStore.Create(Forms.Context).FindAccountsForService(Constants.APP_NAME)
#pragma warning restore CS0618 // Type or member is obsolete
                                          .FirstOrDefault();
				return (account != null) ? account.Properties[Constants.PROP_AUTH_LOC_PROVIDER] : null;
			}
		}

		public void saveCredentials(string userName, string password, string authOpt) {
			
			if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password)) {

				var account = new Account {
					Username = userName
				};

				account.Properties.Add(Constants.PROP_AUTH_LOC_PASSWORD, password);
				account.Properties.Add(Constants.PROP_AUTH_LOC_PROVIDER, authOpt);
#pragma warning disable CS0618 // Type or member is obsolete
                AccountStore.Create(Forms.Context).Save(account, Constants.APP_NAME);
#pragma warning restore CS0618 // Type or member is obsolete
            }
		}

		public void deleteCredentials() {
#pragma warning disable CS0618 // Type or member is obsolete
            var account = AccountStore.Create(Forms.Context).FindAccountsForService(Constants.APP_NAME).FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete

            if (account != null) {
#pragma warning disable CS0618 // Type or member is obsolete
                AccountStore.Create(Forms.Context).Delete(account, Constants.APP_NAME);
#pragma warning restore CS0618 // Type or member is obsolete
            }
		}

		public bool doCredentialsExist() {
#pragma warning disable CS0618 // Type or member is obsolete
            return AccountStore.Create(Forms.Context).FindAccountsForService(Constants.APP_NAME).Any();
#pragma warning restore CS0618 // Type or member is obsolete
        }

	}

}