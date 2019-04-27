using Android.App;

namespace SportsConnection.Droid {
	
	public class MessageUtilsDroid {
		
		public static void showInfoMessage(string title, string msg) {
			AlertDialog.Builder builder = new AlertDialog.Builder(Application.Context);
			builder.SetTitle(title);
			builder.SetMessage(msg);
			builder.Create().Show();
		}

	}

}