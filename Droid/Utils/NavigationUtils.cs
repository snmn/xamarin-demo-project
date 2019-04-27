using SportsConnection.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(NavigationHelper))]
namespace SportsConnection.Droid {

	public class NavigationHelper : INavigationHelper {
		
		public void navigateToHomeScreen() {
			var activity = MainActivity.sCurrentActivity;
			activity.MoveTaskToBack(true);
		}

	}

}