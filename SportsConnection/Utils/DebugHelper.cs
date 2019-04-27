namespace SportsConnection {
	
	public class DebugHelper {
		
		public DebugHelper() {
			System.Diagnostics.Debug.WriteLine("DebugHelper was initialized");
		}

		public static void newMsg(string title, string msg) {
			System.Diagnostics.Debug.WriteLine(title + ":" + msg);
		}

	}

}