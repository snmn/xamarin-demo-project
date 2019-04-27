namespace SportsConnection {
	
	public class Txt {

		// Global Messages
		public static readonly string MSG_LOADING = "Loading ...";
		public static readonly string MSG_SAVING = "Saving ...";
		public static readonly string MSG_NOTIFICATION_CHECKED_IN = "You've checked into a Sports Connect location";
		public static readonly string MSG_NOTIFICATION_TITLE_CHECKED_OUT = "You've checked out of a Sports Connect location";
		public static readonly string MSG_NOTIFICATION_BODY_CHECKED_OUT = "Bye, take a rest and come back soon.";
		public static readonly string MSG_NOTIFICATION_TITLE_NEW_FRIENDSHIP_REQUEST = "Someone wants to be friends with you";
		public static readonly string MSG_NOTIFICATION_BODY_POSTFIX_FRIENDSHIP = " wants to be your friend";
		public static readonly string MSG_INSERT_VALID_NUMBER = "Insert a valid number.";
		public static readonly string MSG_FEATURE_WILL_BE_AVAILABLE_SOON = "This feature is going to be available soon";
		public static readonly string MSG_ERROR_COULD_NOT_FIND_ACCOUNT = "There was a problem to identify your account, please sign out and sign int again";

		// Message titles
		public static readonly string TITLE_NICE = "Nice!";
		public static readonly string TITLE_THANKS = "Thanks";
		public static readonly string TITLE_ATTENTION = "Attention";
		public static readonly string TITLE_GENERAL_INFORMATION_INFORMAL = "Hey";
		public static readonly string TITLE_WELCOME = "Welcome";
		public static readonly string TITLE_OOPS = "Oops";

		// General labels, prefixes and postfixes
		public static readonly string LBL_NO_THANKS = "No thanks";
		public static readonly string LBL_LESS_THAN_MINUTE = "Less than a minute";
		public static readonly string LBL_FINISH_APP = "Finish app";

		public static readonly string PREFIX_DISTANCE = "Distance: ";
		public static readonly string PREFIX_TIME = "Estimated time: ";
		public static readonly string POSTFIX_DISTANCE = " miles";
		public static readonly string POSTFIX_TIME = " minutes";

		public static readonly string LBL_BTN_OK = "Ok";
		public static readonly string LBL_BTN_CLOSE = "Close";
		public static readonly string LBL_BTN_YES = "Yes";
		public static readonly string LBL_BTN_NO = "No";
		public static readonly string LBL_BTN_REFRESH = "Refresh";
		public static readonly string LBL_BTN_SIGN_FACEBOOK = "Sign with Facebook";
		public static readonly string LBL_BTN_SIGN_GOOGLE = "Sign with Google";
		public static readonly string LBL_BTN_SIGN_TWITTER = "Sign in with Twitter";
		public static readonly string LBL_BTN_ADD_TO_FAVORITE = "Add to favorites";
		public static readonly string LBL_BTN_UPDATE_LOCATION = "Update location";
		public static readonly string LBL_BTN_DELETE_LOCATION = "Delete location";
		public static readonly string LBL_BTN_REMOVE_FROM_FAVORITE = "Remove from favorites";
		public static readonly string LBL_BTN_SUBMIT = "Submit";

		// Onboarding messages
		public static readonly string MSG_AUTHENTICATED_TITLE = "Welcome to SportsConnect";
		public static readonly string MSG_AUTHENTICATED = "We're going help you find nice place to play around ya.";
		public static readonly string MSG_SIGNIN_WITH_SOCIAL_NETWORK = "Sign in with a social network of your choice.";
		public static readonly string MSG_LOADING_STUFF = "Hang on a second. \n We are loading  your account and a few locations ...";
		public static readonly string MSG_WELCOME = "We hope you enjoy SportsConnect ";

		// Location Service
		public static readonly string MSG_UPDATING_USER_LOCATION = "Updating current user location";
		public static readonly string MSG_TURN_ON_GPS_TITLE = "Hi, it's nice to have you around";
		public static readonly string MSG_TURN_ON_GPS = "Please, turn your GPS on to let Sports Connect find nice locations for you.";
		public static readonly string MSG_BTN_OK_TURN_ON_GPS = "Ok";
		public static readonly string MSG_BTN_CANCEL_TURN_ON_GPS = "Cancel";

		// Location Permissions
		public static readonly string MSG_WE_NEED_PERMISSION_ACCESS_LOCATION = "Give us permission to access your location, this way we can find the best places for you.";
		public static readonly string MSG_MUST_GIVE_PERMISSION_TO_LOCATION = "You must give permission to access your location in order to use SportsConnect.";

		// Networking
		public static readonly string MSG_SOLVE_CONNECTIVITY_ISSUE_FIRST = "It seems like you're not connected to the internet. Once you get connected hit the button bellow.";
		public static readonly string MSG_NO_CONNECTIVITY_ISSUE = "If you wanna have access to updated SportsConnect locations, please get connected to the internet.";
		public static readonly string MSG_NO_CONNECTIVITY = "Get online to receive updated info from SportsConnect";

		// Authentication
		public static readonly string LBL_SELECT_SOCIAL_NETWORK_LOGIN = "Select a social network and sign in";
		public static readonly string FORMAT_MSG_ALREADY_AUTHENTICATED = @"{0} you are already authenticated with your Facebook account, there's no need to do it again.";
		public static readonly string MSG_THERE_WAS_PROBLEM_SIGNOUT = "There was a problem signing you out, please try again later.";
		public static readonly string MSG_ERROR_TRY_AGAIN = "There was a problem signing you in, please try again.";

		// Menu
		public static readonly string MENU_TITLE = "Menu";

		// Upsert Location
		public static readonly string MSG_CREATE_LOCATION = "Create a location"; 
		public static readonly string MSG_GIVE_NAME_LOCATION = "You need to name the new location.";
		public static readonly string MSG_CHOOSE_ANOTHER_NAME = "This name is already been used, please choose another one.";
		public static readonly string MSG_SELECT_SPORT = "You need to select at list one sport to create a location.";
		public static readonly string MSG_COULD_NOT_CREATE_LOCATION_MIN_DISTANCE = "We cannot create a location too close to another. Please respect a minimum distance of 500 feet from another SportsConnect's location.";
		public static readonly string MSG_COULD_NOT_UPDATE = "We couldn't update this location. Try again later.";
		public static readonly string MSG_COULD_NOT_CREATE_LOCATION = "We could not create your location. Please try again later.";
		public static readonly string MSG_COULD_NOT_LOAD_SPORTS = "We could not load the list of Sports. Please try again later.";
		public static readonly string MSG_LOCATION_CREATED = "You've created a SportsConnect location, we're going to let people know about it.";
		public static readonly string MSG_LOCATION_UPDATED = "We've updated the location.";
		public static readonly string MSG_CREATING_LOCATION = "Wait a second, we are creating the location ...";
		public static readonly string MSG_UPDATING_LOCATION = "Wait a second, we are updating the location ...";
		public static readonly string MSG_PLACEHOLDER_LOCATION = "New Location";
		public static readonly string MSG_ARE_U_SURE_DELETE_LOCATION = "Are you sure you wanna delete this location? You won't be able to recover it.";
		public static readonly string LBL_UPDATE_LOCATION = "Update location";
		public static readonly string LBL_CREATE_LOCATION = "Create location";
		public static readonly string LBL_LOCATION_NAME = "Location name";
		public static readonly string LBL_WHAT_SPORTS_PLAYED = "What sports can be played here?";
		public static readonly string LBL_LOCATION_DESCRIPTION = "Location description";

		// Permissions
		public static readonly string MSG_INSTALL_GOOGLE_PLAY_SERVICES = "You need to install Google Play Services to run this app";
		public static readonly string MSG_FORMAT_GOOGLE_PLAY_SERVICES_PROBLEM = "There is a problem with Google Play Services on this device: {0} - {1}";

		// Find a location
		public static readonly string LBL_LOCATIONS_PAGE_TITLE = "Find a location";

		// Location Details
		public static readonly string MSG_LOADING_LOCATION_INFO = "Loading location info ...";
		public static readonly string MSG_EMPTY_LIST_DIRECTIONS = "We could not load the directions to the location.";
		public static readonly string MSG_EMPTY_LIST_WALL = "No messages so far.";
		public static readonly string MSG_EMPTY_LIST_USERS_AT_LOCATION = "No one around.";
		public static readonly string MSG_DELETED_LOCATION = "The location was successfully deleted.";
		public static readonly string MSG_FAILED_TO_DELETE_LOCATION = "We could not delete de location, try again later.";
		public static readonly string LBL_LOCATION_DETAILS = "Location details";
		public static readonly string LBL_THANK_USER_FEEDBACK = "Thank you for you feedback.";
		public static readonly string LBL_FOLLOW_STEPS_TO_GET_THERE = "Follow the steps below to get there";
		public static readonly string LBL_NO_ONE_ELSE_HERE = "No one else here";
		public static readonly string LBL_QUESTION_WRONG_NUMBER_PLAYERS = "Wrong Number of Players?";
		public static readonly string LBL_HOW_MANY_AT_LOCATION = "How many players?";

		// Manage locations
		public static readonly string LBL_TITLE_MANAGE_LOCATIONS = "Locations";
		public static readonly string LBL_FAVORITE_LOCATIONS = "Favorite locations";
		public static readonly string MSG_EMPTY_LIST_FAVORITE_LOCATIONS = "You don't have any favorite location.";
		public static readonly string MSG_ARE_U_SURE_UNFAVORITE_LOCATION = "Are you sure you wanna unfavorite this location?";
		public static readonly string LBL_RECENT_LOCATIONS = "Recent locations";
		public static readonly string MSG_EMPTY_LIST_RECENT_LOCATIONS = "There is not recent location to show.";
		public static readonly string LBL_USER_LOCATIONS = "Your locations";
		public static readonly string MSG_EMPTY_LIST_USER_LOCATIONS = "You haven't created any location so far.";

		// User profile
		public static readonly string LBL_TITLE_PROFILE = "Profile";
		public static readonly string LBL_USER_NAME = "User name";
		public static readonly string LBL_PROFILE_ABOUT_USER = "About";
		public static readonly string LBL_PROFILE_ABOUT_YOU = "Your info";
		public static readonly string LBL_USER_FRIENDS = "Friends";
		public static readonly string LBL_WRITE_SOMETHING_ABOUT_YOU = "Write something about you";
		public static readonly string LBL_USER_DESCRIPTION = "This user hasn't defined a description yet.";
		public static readonly string MSG_EMPTY_LIST_USER_FRIENDS_CURRENT_USER = "You don't have any friend yet :(";
		public static readonly string MSG_EMPTY_LIST_RECENT_LOCATIONS_UNKNOWN_USER = "We couldn't load the recent locations of this user";
		public static readonly string MSG_EMPTY_LIST_USER_FRIENDS = "We couldn't load the friends of this user";
		public static readonly string MSG_EMPTY_USER_BIO = "We couldn't load the profile information of this user";
		public static readonly string MSG_FAILED_UPDATE_PROFILE = "We could not update your profile information, try again later.";

		// Friends
		public static readonly string LBL_TITLE_FRIENDS = "Friends";
		public static readonly string LBL_FRIENDS = "Friends";
		public static readonly string LBL_FRIENDSHIP_REQUESTS = "Friendship Requests";
		public static readonly string MSG_EMPTY_LIST_FRIENDSHIP_REQUESTS = "There isn't any friendship request waiting for your approval.";
		public static readonly string MSG_FAILED_CONFIRM_FRIENDSHIP = "There was a problem to confirm this friendship request. Try again later";
		public static readonly string MSG_FAILED_DELETE_FRIENDSHIP = "There was a problem to delete finish this friendship. Try again later";

		// Add Friend
		public static readonly string LBL_TITLE_ADD_FRIEND = "Add Friends";
		public static readonly string MSG_EMPTY_LIST_RECOMMENDED_FRIENDS = "We don't have any friend recommendation for you right now.";
		public static readonly string MSG_EMPTY_LIST_FACEBOOK_FRIENDS = "We didn't have access to your list of friends from Facebook ";
		public static readonly string MSG_SENT_FRIENDSHIP_INVITATION = "Your friendship invitation was sent.";
		public static readonly string MSG_FAILED_SEND_FRIENDSHIP_INVITATION = "Your friendship invitation was sent.";

		// About
		public static readonly string TITLE_ABOUT = "About us";
		public static readonly string MSG_INVITE_FRIENDS_FROM_FACEBOOK = "Invite friends from Facebook";
		public static readonly string MSG_DID_NOT_FIND_MATCH_QUERY = "We didn't find any result that match your search terms";

		// Profile
		public static readonly string MSG_ARE_U_SURE_UNFRIEND_USER = "Are you sure you wanna unfriend this user?";
		public static readonly string MSG_ARE_U_SURE_BLOCK_USER = "Are you sure you wanna block this user?";

		// Settings
		public static readonly string TITLE_SETTINGS = "Settings";
		public static readonly string LBL_TERMS_AND_CONDITIONS = "Terms and Conditions";
		public static readonly string MSG_TERMS_AND_CONDITIONS_TITLE = "By installing the app you automatically agree with our terms and conditions:";
		public static readonly string MSG_TERMS_AND_CONDITIONS = "" +
            "1 - All your private information is safe and it won't be released to anyone without your permission.\n\n" +
            "2 - You're directly reponsible for all the public information you release on the app.\n\n" +
			"3 - The autorities are going to be informed of any sort of crime committed by you in the app.\n\n" +
			"4 - Threat people the same way you'd like to be threated.\n\n" +
            "5 - Have fun (;";
	}

}