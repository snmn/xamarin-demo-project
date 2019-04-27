using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace SportsConnection {

	/// <summary>
	/// Hold the directions information to a given geo-coordinate
	/// </summary>
	public class DirectionsController {

		private string TAG = "DirectionsController";
		readonly string BODY_TAG_OPENING = @"<html><body leftmargin='0' topmargin='0' rightmargin='0' bottommargin='0'>";
		readonly string BODY_TAG_CLOSURE = "";

		private float mEstimatedArrivalTime;
		private JToken mOverviewPolyline;
		private string mSummary;
		private JToken mWarnings;
		private int mDistance;
		private List<DirectionLeg> mLegs = new List<DirectionLeg>();
		private List<DirectionStep> mSteps = new List<DirectionStep>();

		/// <summary>
		/// Make a request to the app's API to get the directions from the Google API.
		/// </summary>
		/// <returns>True, if the directions to the location were loaded.</returns>
		/// <param name="originLat">Origin lat.</param>
		/// <param name="originLng">Origin lng.</param>
		/// <param name="destLat">Destination lat.</param>
		/// <param name="destLng">Destination lng.</param>
		public async Task<bool> loadListDirections(string originLat, string originLng, string destLat, string destLng) {
			var request = new RequestDirectionsToLocation();
			request.responseFormat = Constants.FORMAT_JSON;
			request.origin = string.Format(@"{0},{1}", originLat, originLng);
			request.destination = string.Format(@"{0},{1}", destLat, destLng);

			var response = await AzureController.client.InvokeApiAsync<RequestDirectionsToLocation, BasicResponse>(
				Constants.API_PATH_GET_DIRECTIONS_TO_LOCATION,
				request
			);

			if (response != null) {
				if (response.message != null) {
					JObject rawDirections = JObject.Parse(response.message);
					decodeDirections(rawDirections);
				} else {
					mEstimatedArrivalTime = -1f;
				}
			} else {
				mEstimatedArrivalTime = -1f;
			}

			// Check if the route info was loaded and send a response.
			if (mLegs.Count > 0) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Get info from the directions JSON string and put them into a more readable format.
		/// </summary>
		/// <returns><c>true</c>, if directions was decoded, <c>false</c> otherwise.</returns>
		/// <param name="encodedDirections">Encoded directions.</param>
		private void decodeDirections(JObject encodedDirections) {
			mLegs = new List<DirectionLeg>();
			JToken jsonRoute = null;
			JToken legs = null;

			// Get route attributes
			if (encodedDirections["routes"] != null) {
				var aux = encodedDirections["routes"];

				if (aux != null) {
					if (aux.HasValues) {
						jsonRoute = aux[0];
					}
				}
			}

			if (jsonRoute != null) {
				if (jsonRoute.HasValues) {
					if (jsonRoute["overview_polyline"] != null) {
						mOverviewPolyline = jsonRoute["overview_polyline"];
					}

					if (jsonRoute["summary"] != null) {
						mSummary = (string)jsonRoute["summary"];
					}

					if (jsonRoute["warnings"] != null) {
						mWarnings = jsonRoute["warnings"];
					}

					if (jsonRoute["legs"] != null) {
						legs = jsonRoute["legs"];
					}

					// Get attributes of each leg of the route
					if (legs != null) {
						if (legs.HasValues) {

							// Try to decode each leg of the route
							foreach (JToken leg in legs) {
								if (leg.HasValues) {
									try {
										var dirLeg = new DirectionLeg();

										if (leg["distance"] != null) {
											JToken distanceJson = leg["distance"];

											if (distanceJson != null) {
												if (distanceJson.HasValues) {

													if (distanceJson["text"] != null) {
														dirLeg.distanceTxt = (string)distanceJson["text"];
													}

													if (distanceJson["value"] != null) {
														dirLeg.distanceVal = (int)distanceJson["value"];
													}
												}
											}
										}

										if (leg["duration"] != null) {
											JToken durationJson = leg["duration"];

											if (durationJson != null) {
												if (durationJson.HasValues) {
													if (durationJson["text"] != null) {
														dirLeg.durationTxt = (string)durationJson["text"];
													}

													if (durationJson["value"] != null) {
														dirLeg.durationVal = (int)durationJson["value"];
													}
												}
											}
										}

										if (leg["start_address"] != null) {
											dirLeg.startAddress = (string)leg["start_address"];
										}

										if (leg["end_address"] != null) {
											dirLeg.endAddress = (string)leg["end_address"];
										}

										if (leg["start_location"] != null) {
											JToken startLocJson = leg["start_location"];

											if (startLocJson != null) {
												if (startLocJson.HasValues) {
													if (startLocJson["lat"] != null) {
														dirLeg.startLocationLat = (double)startLocJson["lat"];
													}

													if (startLocJson["lng"] != null) {
														dirLeg.startLocationLng = (double)startLocJson["lng"];
													}
												}
											}
										}

										if (leg["end_location"] != null) {
											JToken startLocJson = leg["end_location"];

											if (startLocJson != null) {
												if (startLocJson.HasValues) {
													if (startLocJson["lat"] != null) {
														dirLeg.endLocationLat = (double)startLocJson["lat"];
													}

													if (startLocJson["lng"] != null) {
														dirLeg.endLocationLng = (double)startLocJson["lng"];
													}
												}
											}
										}

										// Get each step of the leg
										if (leg["steps"] != null) {
											dirLeg.steps = decodeLegSteps(leg["steps"]);
										}

										// Add the decoded leg into the list
										mLegs.Add(dirLeg);

										// Calculate the estimated time of the ride
										mEstimatedArrivalTime += dirLeg.durationVal;

									} catch (Exception e) {
										DebugHelper.newMsg(TAG, "Could not decode this step, we are skipping it." + e.Message);
									}
								}
							}
							// ./Done decoding the legs of the route

						}
					}
					// ./Validation

				}
			}
			// ./Done getting route attributes
		}

		private List<DirectionStep> decodeLegSteps(JToken jsonSteps) {
			var steps = new List<DirectionStep>();

			if (jsonSteps != null) {
				if (jsonSteps.HasValues) {

					// Try to decode each step of the leg
					foreach (JToken jsonStep in jsonSteps) {
						if (jsonStep.HasValues) {

							try {
								var step = new DirectionStep();

								if (jsonStep["distance"] != null) {
									JToken jsonDistance = jsonStep["distance"];

									if (jsonDistance != null) {
										if (jsonDistance.HasValues) {
											if (jsonDistance["text"] != null) {
												step.distanceTxt = (string)jsonDistance["text"];
											}

											if (jsonDistance["value"] != null) {
												step.distanceVal = (int)jsonDistance["value"];
												mDistance += step.distanceVal;
											}
										}
									}
								}

								if (jsonStep["duration"] != null) {
									JToken jsonDuration = jsonStep["duration"];

									if (jsonDuration != null) {
										if (jsonDuration.HasValues) {
											if (jsonDuration["text"] != null) {
												step.durationTxt = (string)jsonDuration["text"];
											}

											if (jsonDuration["value"] != null) {
												step.durationVal = (int)jsonDuration["value"];
											}
										}
									}
								}

								if (jsonStep["start_location"] != null) {
									JToken jsonStartLoc = jsonStep["start_location"];

									if (jsonStartLoc != null) {
										if (jsonStartLoc.HasValues) {
											if (jsonStartLoc["lat"] != null) {
												step.startLocationLat = (double)jsonStartLoc["lat"];
											}

											if (jsonStartLoc["lng"] != null) {
												step.startLocationLng = (double)jsonStartLoc["lng"];
											}
										}
									}
								}

								if (jsonStep["end_location"] != null) {
									JToken jsonEndLoc = jsonStep["end_location"];

									if (jsonEndLoc != null) {
										if (jsonEndLoc.HasValues) {
											if (jsonEndLoc["lat"] != null) {
												step.endLocationLat = (double)jsonEndLoc["lat"];
											}

											if (jsonEndLoc["lng"] != null) {
												step.endLocationLng = (double)jsonEndLoc["lng"];
											}
										}
									}
								}

								if (jsonStep["polyline"] != null) {
									step.encPolyline = jsonStep["polyline"];
								}

								if (jsonStep["html_instructions"] != null) {
									step.instructions = (string)jsonStep["html_instructions"];

									if (step.instructions != null) {
										var htmlStepSource = new HtmlWebViewSource();
										htmlStepSource.Html = BODY_TAG_OPENING + step.instructions + BODY_TAG_CLOSURE;
										step.htmlInstructions = htmlStepSource;

										string instructions = Regex.Replace(step.instructions, @"<[^>]*>", " ");
										instructions = Regex.Replace(instructions, @"Destination", "\nDestination");
										step.instructions = instructions;
									}
								}

								if (jsonStep["maneuver"] != null) {
									step.maneuver = (string)jsonStep["maneuver"];
								}

								if (jsonStep["travel_mode"] != null) {
									step.travelMode = (string)jsonStep["travel_mode"];
								}

								steps.Add(step);
								mSteps.Add(step);

							} catch (Exception e) {
								DebugHelper.newMsg(TAG, "Could not decode this step, we are skipping it." + e.Message);
							}
							// ./Done decoding step

						}
					}
					// ./Done decoding steps

				}
			}

			return steps;
		}

		/// <summary>
		/// Return a pretty label for the estimated arrival time
		/// </summary>
		/// <returns>The estimated arrival time.</returns>
		public string getEstimatedArrivalTime() {
			float time = (float)Math.Round(mEstimatedArrivalTime / 60, 2);
			return Convert.ToString(time);
		}

		public float getEstimatedTime() {
			return mEstimatedArrivalTime;
		}

		public string getEncodedPolyline() {
			if (mOverviewPolyline != null) {
				
				if (mOverviewPolyline["points"] != null) {
					return (string)mOverviewPolyline["points"];
				}
			}

			return null;
		}

		public string getRouteSummary() {
			return mSummary;
		}

		public JToken getWarnings() {
			return mWarnings;
		}

		public List<DirectionLeg> getRouteLegs() {
			return mLegs;
		}

		public List<DirectionStep> getRouteSteps() {
			return mSteps;
		}

		public float getInitialDistanceToLococation(char measure) {
			if (measure.Equals(Constants.MEASURE_MILES)) {
				return (float) Math.Round((mDistance * 0.00062137), 2);

			} else if (measure.Equals(Constants.MEASURE_KM)) {
				return mDistance;

			} else {
				return 0;
			}		
		}

	}

}