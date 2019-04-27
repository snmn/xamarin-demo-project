using System;


namespace SportsConnection {
	
	public interface IAuthUtils {

		string username { get; }

		string password { get; }

		string authOpt { get; }

		void saveCredentials(string username, string password, string authOpt);

		void deleteCredentials();

		bool doCredentialsExist();

	}

}