using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace SportsConnection {

	/// <summary>
	/// Defines each step of the path from a point A to a point B.
	/// </summary>
	public class DirectionStep {

		private string mInstruction;
		private HtmlWebViewSource mHtmlInstruction;
		private float mHeight;


		public string distanceTxt {
			get; set;
		}

		public int distanceVal {
			get; set;
		}

		public string durationTxt {
			get; set;
		}

		public int durationVal {
			get; set;
		}

		public double startLocationLat {
			get; set;
		}

		public double startLocationLng {
			get; set;
		}

		public double endLocationLat {
			get; set;
		}

		public double endLocationLng {
			get; set;
		}

		public float contentHeight {
			get {
				mHeight = getInstructionsContainerHeight();
				return mHeight;
			}
			set {
				mHeight = value;
			}
		}

		private float getInstructionsContainerHeight() {
			int LINE_HEIGHT = 25;
			int MIN_CHARS_PER_ROW = 30;

			float instructionHeight = LINE_HEIGHT;

			if (instructions != null) {
				instructionHeight = (instructions.Length / MIN_CHARS_PER_ROW) * LINE_HEIGHT;

				if (instructionHeight <= 1.0) {
					instructionHeight = LINE_HEIGHT;
				}
			}

			return instructionHeight;
		}

		public JToken encPolyline {
			get; set;
		}

		public string instructions {
			get {
				return mInstruction;
			}
			set {
				mInstruction = value;
			}
		}

		public HtmlWebViewSource htmlInstructions {
			get {
				return mHtmlInstruction;
			}
			set {
				mHtmlInstruction = value;
			}
		}

		public string maneuver {
			get; set;
		}

		public string travelMode {
			get; set;
		}

	}

}