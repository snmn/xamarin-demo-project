using System;
using System.IO;

using Xamarin.Forms;

[assembly: Dependency (typeof (PropertyUtilsDroid))]
namespace SportsConnection.Droid {

	public class PropertyUtilsDroid : FileManagerInterface {

		private string TAG = "PropertyUtils";


		public void saveText(string filename, string text) {
			filename += Constants.PROPERTY_FILE_EXTENSION;

			try {
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				var filePath = Path.Combine(documentsPath, filename);

				if (File.Exists(filePath)) {
					File.Delete(filePath);
				}

				File.WriteAllText(filePath, text);
			} catch (IOException e) {
				DebugHelper.newMsg(TAG, "Could not save property " + filename + e.Message);
			}
		}

		public string loadText(string filename) {
			filename += Constants.PROPERTY_FILE_EXTENSION;

			try {
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				var filePath = Path.Combine(documentsPath, filename);
				return File.ReadAllText(filePath);

			} catch (FileNotFoundException e) {
				DebugHelper.newMsg(TAG, "Could not save property " + filename + e.Message);
				return null;
			}
		}

	}

}