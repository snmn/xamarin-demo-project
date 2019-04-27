using System;
using System.Globalization;

namespace SportsConnection {
	
	public class FormatUtils {

		public static double stringToDouble(string strNumber) {
			return double.Parse(strNumber, CultureInfo.InvariantCulture);
		}

		public static string doubleToString(double number) {
			return Convert.ToString(number).Replace(",", ".");
		}

	}

}