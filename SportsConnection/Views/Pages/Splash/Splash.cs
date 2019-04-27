using System;

using Xamarin.Forms;

namespace SportsConnection {
	
	public class Splash : ContentPage {
		
		public Splash() {
			
			Content = new StackLayout {
				Children = {
					new Label { Text = "Hello ContentPage" }
				}
			};
		}

	}

}