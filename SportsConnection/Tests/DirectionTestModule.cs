using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportsConnection {
	
	public class DirectionTestModule {

		private string TAG = "DirectionTestModule";
		private DirectionsController mDirectionController = new DirectionsController();
		bool unitTestResult = true;

		public DirectionTestModule() {
			DebugHelper.newMsg("\n" + TAG, "Starting Direction Test ...");
		}

		public async Task<bool> runUnitTest() {
			bool testResult = await executeTestLogic();

			if (testResult) {
				DebugHelper.newMsg(TAG, "Direction test has finished: SUCCEEDED");
			} else {
				DebugHelper.newMsg(TAG, "Direction test has finished: FAILED");
			}

			return testResult;
		}

		public async Task<bool> executeTestLogic() {
			await testLoadDirectionsFromAToB();

			return unitTestResult;
		}

		public async Task<bool> testLoadDirectionsFromAToB() {
			unitTestResult = false;

			// Test variables
			//...........................................................
			string testADestinationLat = "-8.036801";    // Cidade Universitária
			string testADestinationLng = "-34.9490501";
			string testBDestinationLat = "-8.075493";    // Estrada dos Remédios
			string testBDestinationLng = "-34.9094181";
			//...........................................................

			if (await mDirectionController.loadListDirections(testADestinationLat, testADestinationLng,
			                                                 testBDestinationLat, testBDestinationLng)) {
				if (mDirectionController.getRouteLegs() != null) {
					if (mDirectionController.getRouteLegs().Count > 0) {

						// Print all the legs of the route and their details
						foreach (DirectionLeg dirLeg in mDirectionController.getRouteLegs()) {
							// Print information about the leg
							DebugHelper.newMsg(TAG, "-------------------------------------");
							DebugHelper.newMsg(TAG, "Distance from location A to location B: " + dirLeg.distanceTxt);
							DebugHelper.newMsg(TAG, "-------------------------------------");
							DebugHelper.newMsg(TAG, "Departure address: " + dirLeg.startAddress);
							DebugHelper.newMsg(TAG, "Destination address: " + dirLeg.endAddress);
							DebugHelper.newMsg(TAG, "It's going to take around " + dirLeg.durationTxt + " for you to get there driving.");

							// Print information about each step
							DebugHelper.newMsg(TAG, "--------------------------------------");
							DebugHelper.newMsg(TAG, "Follow these instructions to get there");
							DebugHelper.newMsg(TAG, "--------------------------------------");

							int stepCount = 1;
							foreach (DirectionStep dirStep in dirLeg.steps) {
								DebugHelper.newMsg(TAG, "Step " + stepCount++);
								DebugHelper.newMsg(TAG, dirStep.instructions);
								DebugHelper.newMsg(TAG, "Distance: " + dirStep.distanceTxt);
								DebugHelper.newMsg(TAG, "Duration: " + dirStep.durationTxt);
								DebugHelper.newMsg(TAG, "...");
							}
							DebugHelper.newMsg(TAG, "--------------------------------------");
						}
						// ./Done using the directions

						unitTestResult = true;

					} else {
						DebugHelper.newMsg(TAG, "Failed to read directions.");
						unitTestResult = false;
					}
				} else {
					DebugHelper.newMsg(TAG, "Failed to read directions.");
					unitTestResult = false;
				}
			} else {
				DebugHelper.newMsg(TAG, "Failed to read directions.");
				unitTestResult = false;
			}

			return unitTestResult;
		}

	}

}