using System;

using Microsoft.WindowsAzure.MobileServices;

namespace SportsConnection {
	
	public class AzureObject {

		[CreatedAt]
		public DateTime CreatedAt {
			get; set;
		}

		[UpdatedAt]
		public DateTime UpdatedAt {
			get; set;
		}

		[Version]
		public string Version {
			get; set;
		}

		[Deleted]
		public bool Deleted {
			get; set;
		}

	}

}