using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace SportsConnection {
	
	public interface IAuth {
		
		Task<MobileServiceUser> authenticate(string authOpt);

	}

}