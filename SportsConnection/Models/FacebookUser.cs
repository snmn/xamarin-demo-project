using System.Collections.Generic;

namespace SportsConnection {
	
	public class FacebookUser : SocialUser {

		public string emailAddress {
			get; set;
		}

		public string givenName {
			get; set;
		}

		public string surname {
			get; set;
		}

		public string gender {
			get; set;
		}

		public string profileUrl {
			get; set;
		}

		public int numFacebookFriends {
			get; set;
		}

		public List<FacebookFriend> facebookFriends {
			get; set;
		}

	}

}