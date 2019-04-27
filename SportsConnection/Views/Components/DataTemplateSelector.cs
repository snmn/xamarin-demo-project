using System;
using Xamarin.Forms;

namespace SportsConnection {
	
	public class DataTemplateSelector {
		
		public virtual DataTemplate SelectTemplate(object item, BindableObject container) {
			return null;
		}
	}

}
