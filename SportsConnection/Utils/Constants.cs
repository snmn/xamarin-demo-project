namespace SportsConnection{
	
	public static class  Constants{

		// Global  
		public static readonly string APP_NAME = "SportsConnection";
		public static readonly int    STATUS_BAR_HEIGHT = 23;
		public static readonly int    PAGE_HEADER_IMG_HEIGHT = 200;
		public static readonly double MULTIPLIER_PAGE_HEADER_HEIGHT = 0.55;
		public static readonly int    FORM_TEXT_INPUT_CLEAN_BTN_PADDING = 80;
		public static readonly int    ONE_MINUTE_SECS = 60;
		public static readonly int    DEFAULT_MAP_ZOOM_LEVEL = 15;
		public static readonly char   DISTANCE_MEASURE = 'M';

		// Tags
		public static readonly string TAG_GEOFENCING_TASK = "GeofencingTask";
		public static readonly string TAG_AUTH_CONTROLLER = "AuthUserController";
		public static readonly string TAG_AUTH_CONTROLLER_DROID = "AuthControllerDroid";
		public static readonly string TAG_AUTH_PAGE = "TAG_AUTH_PAGE";
		public static readonly string TAG_DROID_APP = "DroidApplication";
		public static readonly string TAG_LOCATIONS = "Locations";
		public static readonly string TAG_LOCATION_UPSERT = "LocationUpsert";
		public static readonly string TAG_LOCATION_DETAILS = "LocationDetails";
		public static readonly string TAG_ROOT = "Root";
		public static readonly string TAG_TEMPLATE_SELECTOR = "TemplateSelector";
		public static readonly string TAG_USER_PROFILE = "UserProfile";
		public static readonly string TAG_ADD_FRIEND_PAGE = "AddFriendPage";
		public static readonly string TAG_USER_RELATIONSHIP_CONTROLLER = "UserRelationshipsController";

		// Remote URL's and local paths
		public static readonly string APPLICATION_URL = @"https://sportsconnectapp.azurewebsites.net";
		public static readonly string WEBSITE_URL = "http://sportsconnect.io";
		public static readonly string TUTORIAL_URL = "http://sportsconnect.io";
		public static readonly string PATH_LOCAL_DB_FILE = "localstore.db";

		// API paths
		public static readonly string API_PATH_GET_EXTRA_INFO_FROM_IDT_PROVIDER = "get_extra_info";
		public static readonly string API_PATH_GET_FACEBOOK_FRIENDS = "get_facebook_friends";
		public static readonly string API_PATH_GET_DIRECTIONS_TO_LOCATION = "get_directions_from_a_to_b";

		// Separators
		public static readonly char SEPARATOR_FORWARD_SLASH = '/';
		public static readonly char PUSH_NOTIFICATION_ATTRIBUTES_SEPARATOR = ';';
		public static readonly char PUSH_NOTIFICATION_KEY_VALUE_SEPARATOR = ':';

		// Facebook
		public static readonly string FACEBOOK_PROP_AUTH_PROVIDER = "facebook";
		public static readonly string FACEBOOK_PROP_NAME_ID = "nameidentifier";
		public static readonly string FACEBOOK_PROP_ACCESS_TOKEN = "access_token";
		public static readonly string FACEBOOK_PROP_CLAIMS = "claims";
		public static readonly string FACEBOOK_PROP_EMAIL = "emailaddress";
		public static readonly string FACEBOOK_PROP_NAME = "name";
		public static readonly string FACEBOOK_PROP_GIVEN_NAME = "givenname";
		public static readonly string FACEBOOK_PROP_SURNAME = "surname";
		public static readonly string FACEBOOK_PROP_LOCALE = "urn:facebook:locale";
		public static readonly string FACEBOOK_PROP_GENDER = "gender";
		public static readonly string FACEBOOK_PATH_PICTURE = "https://graph.facebook.com/{0}/picture?type=large";
		public static readonly string FACEBOOK_FRIENDS = "data";
		public static readonly string FACEBOOK_SUMMARY = "summary";
		public static readonly string FACEBOOK_FRIEND_NAME = "name";
		public static readonly string FACEBOOK_FRIEND_ID = "id";
		public static readonly string FACEBOOK_NUM_FRIENDS = "total_count";
		public static readonly string FACEBOOK_PROFILE_URL = "urn:facebook:link";

		// Google
		public static readonly string GOOGLE_PROP_AUTH_PROVIDER = "google";
		public static readonly string GOOGLE_PROP_ACCESS_TOKEN = "access_token";
		public static readonly string GOOGLE_PROP_CLAIMS = "claims";
		public static readonly string GOOGLE_PROP_USER_ID = "user_id";
		public static readonly string GOOGLE_PROP_EMAIL = "emailaddress";
		public static readonly string GOOGLE_PROP_NAME = "name";
		public static readonly string GOOGLE_PROP_GIVEN_NAME = "givenname";
		public static readonly string GOOGLE_PROP_SURNAME = "surname";
		public static readonly string GOOGLE_PROP_LOCALE = "locale";
		public static readonly string GOOGLE_PROP_PICTURE = "picture";

		// Twitter
		public static readonly string TWITTER_PROP_AUTH_PROVIDER = "twitter";
		public static readonly string TWITTER_PROP_ACCESS_TOKEN = "access_token";
		public static readonly string TWITTER_PROP_CLAIMS = "claims";
		public static readonly string TWITTER_PROP_UPN = "upn";
		public static readonly string TWITTER_PROP_NAME = "name";
		public static readonly string TWITTER_PROP_LOCALE = "urn:twitter:location";
		public static readonly string TWITTER_PROP_PICTURE = "urn:twitter:profile_image_url_https";

		// Login options
		public static readonly string AUTH_OPT_FACEBOOK = "facebook";
		public static readonly string AUTH_OPT_GOOGLE = "google";
		public static readonly string AUTH_OPT_TWITTER = "twitter";

		// Sports
		public static readonly string BASKETBALL_STR = "Basketball";
		public const int BASKETBALL = 0;
		public static readonly string SOCCER_STR = "Soccer";
		public const int SOCCER = 1;

		// User relationship status
		public static readonly string FRIENDS_STR = "Friends";
		public const int FRIENDS = 0;
		public static readonly string PENDING_STR = "Pending";
		public const int PENDING = 1;
		public static readonly string BLOCKED_STR = "Blocked";
		public const int BLOCKED = 2;

		// Parameters
		public static readonly string PARAM_SELECTED_LOCATION_ID = "checkedInLocationId";
		public static readonly string PARAM_LAST_SESSION_PID = "lastSessionPid";
		public static readonly float PARAM_DENSITY_WIDTH = 0.5f;
		public static readonly float PARAM_DENSITY_HEIGHT = 0.5f;

		// Notifications
		public static readonly int NOTIFICATION_TYPE_USER_CHECKED_IN = 1;
		public static readonly int NOTIFICATION_TYPE_USER_CHECKED_OUT = 2;
		public static readonly int NOTIFICATION_TYPE_FRIENDSHIP_REQUEST = 3;
		public static readonly string SETTING_NOTIFICATION_COUNT = "NotificationCount";

		// GCM service
		public static readonly string PARAM_VAL_TEMPLATE_GCM_BODY = "{\"data\":{\"message\":\"$(messageParam)\"}}";
		public static readonly string PARAM_KEY_TEMPLATE_BODY = "body";
		public static readonly string PARAM_KEY_GENERIC_MESSAGE = "genericMessage";
		public static readonly string PARAM_KEY_LAST_MESSAGE = "last_msg";
		public static readonly string PARAM_KEY_MESSAGE = "message";

		// Push notifications type
		public static readonly string PUSH_NOTIFICATION_TYPE_FRIENDSHIP = "friendship";
		public static readonly string PUSH_NOTIFICATION_TYPE_CHECKIN = "checkin";
		public static readonly string PUSH_NOTIFICATION_TYPE_WALL_MESSAGE = "wall_message";

		// Push notification attributes
		public static readonly string FRIENDSHIP_NOTIFICATION_PARAM_USERID_A = "userIdA";
		public static readonly string FRIENDSHIP_NOTIFICATION_PARAM_USERID_B = "userIdB";
		public static readonly string FRIENDSHIP_NOTIFICATION_PARAM_STATUS_A = "statusA";
		public static readonly string FRIENDSHIP_NOTIFICATION_PARAM_STATUS_B = "statusB";

		public static readonly string CHECKIN_NOTIFIVATION_PARAM_ID = "id";
		public static readonly string CHECKIN_NOTIFIVATION_PARAM_USER_ID = "userId";
		public static readonly string CHECKIN_NOTIFIVATION_PARAM_LOCATION_ID = "locationId";

		// Formats
		public static readonly string FORMAT_JSON = "json";

		// Geocoordinates
		public static readonly string PARAM_LAT = "Latitude";
		public static readonly string PARAM_LNG = "Longitude";
		public static readonly string PARAM_ALT = "Altitude";

		// Maps
		public static readonly string PARAM_PLACEHOLDER_PIN_ID = "placeholder_pin_Id_5423452345";

		// Geofencing Service
		public static readonly string PARAM_LATITUDE = "Latitude";
		public static readonly string PARAM_LONGITUDE = "Longitude";
		public static readonly string PARAM_NOTIFICATION_STATE = "IsRunning";
		public static readonly string PARAM_LAST_UPDATED = "LastUpdated";
		public static readonly string PARAM_DEBUGGER = "Debugger";
		public static readonly string PARAM_NOTIFICATION_COUNT = "NotificationCount";
		public static readonly string PROPERTY_CHECKEDIN_LOCATION_IDS = "checkedInLocationIds";
		public static readonly string PROPERTY_CHECKEDIN_LOCATION_NAMES = "checkedInLocationNames";
		public static readonly int TIMEOUT_GEOLOC_UPDATE = 3000;

		// Distance thresholds
		public static readonly double DISTANCE_THRESHOLD_CHECKED_IN_LOCATION_RADIUS = 0.2;
		public static readonly double DISTANCE_THRESHOLD_MIN_MOVEMENT_UPDATE_LOCATION = 0.05;
		public static readonly double DISTANCE_THRESHOLD_MIN_DISTANCE_NEARBY_LOCATIONS = 0.15;

		public static readonly char MEASURE_KM = 'K';
		public static readonly char MEASURE_MILES = 'M';

		// Files
		public static readonly string PROPERTY_FILE_EXTENSION = ".txt";

		// Local storage properties
		public static readonly string PROP_USER_PUBLIC_LOCATION = "prop_public_location";
		public static readonly string PROP_FACEBOOK_USER = "social_user_facebook";
		public static readonly string PROP_GOOGLE_USER = "social_user_google";
		public static readonly string PROP_TWITTER_USER = "social_user_twitter";
		public static readonly string PROP_AUTH_LOC_PASSWORD = "Password";
		public static readonly string PROP_AUTH_LOC_PROVIDER = "Auth_option";
		public static readonly string PROP_IS_FIRST_TIME = "is_first_time";
		public static readonly string PROP_LAST_USED_SOCIAL_ACC = "last_used_social_account";

		// Android System Services
		public static readonly string ANDROID_SYSTEM_SERVICE_LOCATION = "location";
		public static readonly string ANDROID_SYSTEM_SERVICE_CONNECTION = "connection";

		// Containers
		public static double MSG_CONTAINER_STD_HEIGHT = 100.0;

		// Animations
		public static readonly uint TIMEOUT_FADE_OUT_FAST_ANIMATION = 1;
		public static readonly uint TIMEOUT_FAST_FADE_IN_ANIMATION = 300;
		public static readonly uint TIMEOUT_STD_FADE_IN_ANIMATION = 1000;
		public static readonly uint TIMEOUT_STD_FADE_OUT_ANIMATION = 1000;
		public static readonly uint TIMEOUT_STD_SCALE_ANIMATION = 800;
		public static readonly int TIMEOUT_HIDE_LOADING_MIN_WAITING_TIME = 1000;
		public static readonly int TIMEOUT_LOAD_ABOUT_PAGE = 3000;

		// Main Menu
		public const string LBL_MENU_OPT_PROFILE = "Profile";
		public const string LBL_MENU_OPT_LOCATIONS = "Find a location";
		public const string LBL_MENU_OPT_YOUR_LOCATIONS = "Your locations";
		public const string LBL_MENU_OPT_FRIENDS = "Friends";
		public const string LBL_MENU_OPT_SETTINGS = "Settings";
		public const string LBL_MENU_OPT_ABOUT_US = "About us";
		public const string LBL_MENU_OPT_SIGNOUT = "Sign Out";

		public const string LBL_MENU_SPLASH = "Splash";
		public const string LBL_MENU_AUTHENTICATION = "Authentication";

		// Images
		public static readonly double IMAGE_ALPHA_ENABLED = 1;
		public static readonly double IMAGE_ALPHA_DISABLED = 0.3;

		public static readonly string IMAGE_BG_HOME = "bg_home.png";
		public static readonly string IMAGE_BG_AUTH = "bg_auth.png";
		public static readonly string IMAGE_BG_SPLASH = "bg_splash.png";
		public static readonly string IMAGE_BG_PROFILE = "bg_profile.png";

		public static readonly string IMAGE_ICO_LOGO = "logo.png";
		public static readonly string IMAGE_ICO_FACEBOOK = "ico_facebook.png";
		public static readonly string IMAGE_ICO_GOOGLE_PLUS = "ico_google_plus.png";
		public static readonly string IMAGE_ICO_TWITTER = "ico_twitter.png";
		public static readonly string IMAGE_ICO_HAMBURGUER = "hamburguer.png";
		public static readonly string IMAGE_ICO_NO_INTERNET_WHITE = "ico_internet_white.png";
		public static readonly string IMAGE_ICO_NO_INTERNET_BLACK = "ico_internet_black.png";
		public static readonly string IMAGE_ICO_MAP = "ico_map.png";
		public static readonly string IMAGE_ICO_LOCATION = "ico_location.png";
		public static readonly string IMAGE_ICO_PROFILE_ORANGE = "ico_profile_orange.png";
		public static readonly string IMAGE_ICO_PROFILE_BLACK = "ico_profile_black.png";
		public static readonly string IMAGE_ICO_FAVORITE_OFF = "ico_favorite_off.png";
		public static readonly string IMAGE_ICO_FAVORITE_ON = "ico_favorite_on.png";
		public static readonly string IMAGE_ICO_FRIENDS = "ico_friends.png";
		public static readonly string IMAGE_ICO_SETTINGS = "ico_settings.png";
		public static readonly string IMAGE_ICO_INFO = "ico_info.png";
		public static readonly string IMAGE_ICO_SIGN_OUT = "ico_sign_out.png";
		public static readonly string IMAGE_ICO_BTN_PLUS_MAIN = "ico_btn_plus_main.png";
		public static readonly string IMAGE_ICO_BTN_SAVE = "ico_save_white.png";
		public static readonly string IMAGE_ICO_UNCHECKED_BOX = "ico_btn_unchecked.png";
		public static readonly string IMAGE_ICO_CHECKED_BOX = "ico_btn_checked.png";
		public static readonly string IMAGE_ICO_SCROLL = "ico_scroll.png";
		public static readonly string IMAGE_ICO_DELETE_BLACK = "ico_delete_black.png";
		public static readonly string IMAGE_ICO_DIRECTIONS_BLACK = "ico_directions.png";
		public static readonly string IMAGE_ICO_CHAT_BLACK = "ico_chat.png";
		public static readonly string IMAGE_ICO_SEND_MESSAGE = "ico_send_message.png";
		public static readonly string IMAGE_ICO_PEOPLE_BLACK = "ico_people.png";
		public static readonly string IMAGE_ICO_GRAY_ARROW_UP = "ico_gray_arrow_up.png";
		public static readonly string IMAGE_ICO_FAVORITE_LOCATION_BLACK = "ico_favorite_black.png";
		public static readonly string IMAGE_ICO_USER_LOCATIONS = "ico_user_locations.png";
		public static readonly string IMAGE_ICO_RECENT_LOCATIONS = "ico_recent_locations.png";
		public static readonly string IMAGE_ICO_BTN_CLEAR_BLACK = "ico_btn_clear.png";
		public static readonly string IMAGE_ICO_BTN_SEE_DETAILS = "ico_see_details.png";
		public static readonly string IMAGE_ICO_BTN_EDIT = "ico_edit.png";
		public static readonly string IMAGE_ICO_BTN_DELETE = "ico_delete.png";
		public static readonly string IMAGE_ICO_BTN_SAVE_BLUE = "ico_save_blue.png";
		public static readonly string IMAGE_ICO_BTN_SAVE_BLACK = "ico_save_black.png";
		public static readonly string IMAGE_ICO_FRIENSHIP_REQUEST_BLACK = "ico_frienship_request_black.png";
		public static readonly string IMAGE_ICO_SEARCH = "ico_search.png";
		public static readonly string IMAGE_ICO_CONFIRM_GREEN = "ico_confirm_green.png";
		public static readonly string IMAGE_ICO_UNFRIEND_WHITE = "ico_unfriend_white.png";
		public static readonly string IMAGE_ICO_BLOCK_USER_WHITE = "ico_block_user_white.png";

		public static readonly string IMAGE_PLACEHOLDER_USER = "placeholder_user.png";

		public static readonly string IMAGE_STD_LOCATION_BANNER = "bg_sport.jpg";

		public static readonly string IMAGE_SPORT_BASKETBALL = "img_sport_basketball.png";
		public static readonly string IMAGE_SPORT_SOCCER = "img_sport_soccer.jpg";

		// UI
		public static readonly int UIPROP_CHECKBOX_SIZE = 30;

		// Sports
		public static readonly string SPORT_BASKETBALL = "Basketball";
		public static readonly int SPORT_BASKETBALL_NUM_PLAYERS = 10;
		public static readonly string SPORT_SOCCER = "Soccer";
		public static readonly int SPORT_SOCCER_NUM_PLAYERS = 24;

		// Animations
		public static readonly uint ANIMATION_DEFAULT_INTERVAL_BETWEEN_ANIMATIONS;
		public static readonly string ANIMATION_HEADER_LOCATION_DETAILS = "ANIMATION_HEADER_LOCATION_DETAILS";
		public static readonly uint ANIMATION_HEADER_LOCATION_DETAILS_EXEC_TIME_MILI = 500;

	}

}