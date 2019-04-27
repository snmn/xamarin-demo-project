using System;
using System.Threading.Tasks;

namespace SportsConnection {
	
	public interface AuthCallback {

		Task<bool> onAuthProcessCompleted(bool success);

	}

}