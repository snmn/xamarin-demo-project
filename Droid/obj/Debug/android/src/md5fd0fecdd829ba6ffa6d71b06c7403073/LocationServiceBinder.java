package md5fd0fecdd829ba6ffa6d71b06c7403073;


public class LocationServiceBinder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SportsConnection.Droid.LocationServiceBinder, SportsConnection.Droid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", LocationServiceBinder.class, __md_methods);
	}


	public LocationServiceBinder ()
	{
		super ();
		if (getClass () == LocationServiceBinder.class)
			mono.android.TypeManager.Activate ("SportsConnection.Droid.LocationServiceBinder, SportsConnection.Droid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public LocationServiceBinder (md5fd0fecdd829ba6ffa6d71b06c7403073.LocationService p0)
	{
		super ();
		if (getClass () == LocationServiceBinder.class)
			mono.android.TypeManager.Activate ("SportsConnection.Droid.LocationServiceBinder, SportsConnection.Droid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SportsConnection.Droid.LocationService, SportsConnection.Droid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
