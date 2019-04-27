using System.ComponentModel;
using System.Threading.Tasks;

namespace SportsConnection {

	public class SimpleListItemAdapter : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public string id { 
			get; set; 
		}

		public string name {
			get; set;
		}

		public bool isChecked {
			get; set;
		}

		public string checkStatusImg {
			get; set;
		}


		public SimpleListItemAdapter() {}

		public SimpleListItemAdapter(string id, string name, bool selected) {
			this.id = id;
			this.name = name;
			isChecked = selected;
			checkStatusImg = Constants.IMAGE_ICO_UNCHECKED_BOX;
		}

		public void toggleCheck() {
			isChecked = !isChecked;

			if (isChecked) {
				checkStatusImg = Constants.IMAGE_ICO_CHECKED_BOX;
			} else {
				checkStatusImg = Constants.IMAGE_ICO_UNCHECKED_BOX;
			}

			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs("checkStatusImg"));
			}
		}

	}

}