namespace SportsConnection.Droid {

	public static class AndroidUtils {

		public static void finishApp() {
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
		}

	}

}