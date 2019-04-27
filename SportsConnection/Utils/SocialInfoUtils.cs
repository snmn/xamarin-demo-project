using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SportsConnection {
	
	public class SocialInfoUtils {

		private string TAG = "FacebookUtils";


		public SocialInfoUtils() {
			DebugHelper.newMsg(TAG, "Starting facebook utils ...");
		}

		public static TwitterUser fetchAndGetTwitterUser(JToken rawSocialInfo) {
			TwitterUser twitterUser = new TwitterUser();
			twitterUser.authProvider = Constants.AUTH_OPT_TWITTER;

			var infoFromTwitter = rawSocialInfo[Constants.TWITTER_PROP_AUTH_PROVIDER];

			if (infoFromTwitter != null) {
				string accessToken = (string)infoFromTwitter[Constants.TWITTER_PROP_ACCESS_TOKEN];
				var claims = infoFromTwitter[Constants.TWITTER_PROP_CLAIMS];

				if (claims != null) {
					string userId = (string)claims[Constants.TWITTER_PROP_UPN];
					string name = (string)claims[Constants.TWITTER_PROP_NAME];
					string locale = (string)claims[Constants.TWITTER_PROP_LOCALE];
					string picture = (string)claims[Constants.TWITTER_PROP_PICTURE];

					if (accessToken != null) {
						twitterUser.accessToken = accessToken;
					}
					if (userId != null) {
						twitterUser.userId = userId;
					}
					if (name != null) {
						twitterUser.name = name;
					}
					if (locale != null) {
						twitterUser.locale = locale;
					}
					if (picture != null) {
						twitterUser.pictureUrl = picture;
					}
				}
			}

			return twitterUser;
		}

		public static GoogleUser fetchAndGetGoogleUser(JToken rawSocialInfo) {
			GoogleUser googleUser = new GoogleUser();
			googleUser.authProvider = Constants.AUTH_OPT_GOOGLE;

			var infoFromGoogle = rawSocialInfo[Constants.GOOGLE_PROP_AUTH_PROVIDER];

			if (infoFromGoogle != null) {
				string accessToken = (string)infoFromGoogle[Constants.GOOGLE_PROP_ACCESS_TOKEN];
				string userId = (string)infoFromGoogle[Constants.GOOGLE_PROP_USER_ID];
				var claims = infoFromGoogle[Constants.GOOGLE_PROP_CLAIMS];

				if (claims != null) {
					string emailAddress = (string)claims[Constants.GOOGLE_PROP_EMAIL];
					string name = (string)claims[Constants.GOOGLE_PROP_NAME];
					string givenName = (string)claims[Constants.GOOGLE_PROP_GIVEN_NAME];
					string surname = (string)claims[Constants.GOOGLE_PROP_SURNAME];
					string locale = (string)claims[Constants.GOOGLE_PROP_LOCALE];
					string picture = (string)claims[Constants.GOOGLE_PROP_PICTURE];

					if (accessToken != null) {
						googleUser.accessToken = accessToken;
					}

					if (userId != null) {
						googleUser.userId = userId;
					}

					if (emailAddress != null) {
						googleUser.emailAddress = emailAddress;
					}

					if (name != null) {
						googleUser.name = name;
					}

					if (givenName != null) {
						googleUser.givenName = givenName;
					}

					if (surname != null) {
						googleUser.surname = surname;
					}

					if (locale != null) {
						googleUser.locale = locale;
					}

					if (picture != null) {
						googleUser.pictureUrl = picture;
					}
				}
			}

			return googleUser;
		}

		public async static Task<FacebookUser> fetchAndGetFacebookUser(JToken rawSocialInfo) {
			FacebookUser facebookUser = new FacebookUser();
			facebookUser.authProvider = Constants.AUTH_OPT_FACEBOOK;

			var infoFromFacebook = rawSocialInfo[Constants.FACEBOOK_PROP_AUTH_PROVIDER];

			if (infoFromFacebook != null) {
				string accessToken = (string)infoFromFacebook[Constants.FACEBOOK_PROP_ACCESS_TOKEN];
				var claims = infoFromFacebook[Constants.FACEBOOK_PROP_CLAIMS];

				// Get the basic information of the facebook user
				if (claims != null) {
					var userId = "";
					var auxId = (string)claims[Constants.FACEBOOK_PROP_NAME_ID];

					if (auxId != null) {
						var scopedIdParts = auxId.Split(Constants.SEPARATOR_FORWARD_SLASH);

						if (scopedIdParts != null && scopedIdParts.Length > 0) {
							userId = scopedIdParts[scopedIdParts.Length - 1];
						}
					}

					string emailAddress = (string)claims[Constants.FACEBOOK_PROP_EMAIL];
					string name = (string)claims[Constants.FACEBOOK_PROP_NAME];
					string givenName = (string)claims[Constants.FACEBOOK_PROP_GIVEN_NAME];
					string surname = (string)claims[Constants.FACEBOOK_PROP_SURNAME];
					string locale = (string)claims[Constants.FACEBOOK_PROP_LOCALE];
					string gender = (string)claims[Constants.FACEBOOK_PROP_GENDER];
					string url = (string)claims[Constants.FACEBOOK_PROFILE_URL];
					string picture = string.Format(Constants.FACEBOOK_PATH_PICTURE, userId);

					if (accessToken != null) {
						facebookUser.accessToken = accessToken;
					}

					if (userId != null) {
						facebookUser.userId = userId;
					}

					if (emailAddress != null) {
						facebookUser.emailAddress = emailAddress;
					}

					if (name != null) {
						facebookUser.name = name;
					}

					if (givenName != null) {
						facebookUser.givenName = givenName;
					}

					if (surname != null) {
						facebookUser.surname = surname;
					}

					if (locale != null) {
						facebookUser.locale = locale;
					}

					if (picture != null) {
						facebookUser.pictureUrl = picture;
					}

					if (url != null) {
						facebookUser.profileUrl = url;
					}

					if (gender != null) {
						facebookUser.gender = gender;
					}
				}

				// Get the list of friends of the Facebook user
				RequestFacebookFriends requestFacebookFriends = new RequestFacebookFriends();
				requestFacebookFriends.accessToken = accessToken;

				var responseFacebookFriends = await AzureController.client.InvokeApiAsync<RequestFacebookFriends, BasicResponse>(
					Constants.API_PATH_GET_FACEBOOK_FRIENDS,
					requestFacebookFriends
				);

				var faceFriends = JToken.Parse(responseFacebookFriends.message);

				if (faceFriends != null) {
					var friends = faceFriends[Constants.FACEBOOK_FRIENDS];
					var summary = faceFriends[Constants.FACEBOOK_SUMMARY];

					if (friends != null) {
						foreach (var friend in friends) {
							facebookUser.facebookFriends = new System.Collections.Generic.List<FacebookFriend>();
							FacebookFriend faceFriend = new FacebookFriend();

							string friendName = (string)friend[Constants.FACEBOOK_FRIEND_NAME];
							string friendFacebookId = (string)friend[Constants.FACEBOOK_FRIEND_ID];

							if (friendName != null) {
								faceFriend.name = friendName;
							}

							if (friendFacebookId != null) {
								faceFriend.facebookId = friendFacebookId;
							}

							facebookUser.facebookFriends.Add(faceFriend);
						}
					}

					if (summary != null) {
						int totalNumFacebookFriends = (int)summary[Constants.FACEBOOK_NUM_FRIENDS];

						if (totalNumFacebookFriends > 0) {
							facebookUser.numFacebookFriends = totalNumFacebookFriends;
						}
					}
				}
			}

			return facebookUser;
		}
		 
	}

}