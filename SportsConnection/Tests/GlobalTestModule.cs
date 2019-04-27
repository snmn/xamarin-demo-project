using System.Threading.Tasks;

namespace SportsConnection {
	
	public class GlobalTestModule {

		private string TAG = "GlobalTestModule";

		public GlobalTestModule() {
			DebugHelper.newMsg(TAG, "Starting server modules' tests ...");
		}

		public async Task<bool> init() {
			if (App.sIsDebugging) {
				// Test User module
				UserTestModule userTestModule = new UserTestModule();

				if (await userTestModule.runUnitTest()) {
					DebugHelper.newMsg(TAG, "User module WORKS.");
				} else {
					DebugHelper.newMsg(TAG, "User module FAILED.");
				}

				// Test Location module
				LocationTestModule locationTestModule = new LocationTestModule();

				if (await locationTestModule.runUnitTest()) {
					DebugHelper.newMsg(TAG, "Location module WORKS.");
				} else {
					DebugHelper.newMsg(TAG, "Location module FAILED.");
				}

				// Test Sport module
				SportTestModule sportsTestModule = new SportTestModule();

				if (await sportsTestModule.runUnitTest()) {
					DebugHelper.newMsg(TAG, "Sport module WORKS.");
				} else {
					DebugHelper.newMsg(TAG, "Sport module FAILED.");
				}

				// Directions module
				DirectionTestModule directionTestModule = new DirectionTestModule();

				if (await directionTestModule.runUnitTest()) {
					DebugHelper.newMsg(TAG, "Direction module WORKS.");
				} else {
					DebugHelper.newMsg(TAG, "Direction module FAILED.");
				}
			}

			return true;
		}

	}

}