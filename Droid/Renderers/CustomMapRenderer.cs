using System.Collections.Generic;
using System.ComponentModel;

using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

using SportsConnection;
using SportsConnection.Droid;
using System;
using Android.Util;
using Android.Graphics;
using Android.Support.V4.Content;

/**
 * Create custom renderer for the default Google map implementation created by Xamarin and connect 
 * it to the CustomMap class on the PCL code. 
 */
[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace SportsConnection.Droid {

	public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback,
	GoogleMap.IOnMarkerClickListener {

		private readonly string TAG = "CustomMapRenderer";
		private readonly int POLYLINE_COLOR = 0x6600B3FD;
		private readonly string ATTR_DRAW_PINS = "VisibleRegion";
		private readonly int MARKER_AREA_DP = 50;

		public GoogleMap map;
		public CustomMap formsMap;
		public List<CustomPin> customPins;
		public Marker lastOpenned;
		public LatLng[] decodedLatLngs;

		private string mEncodedPolyline;
		private CameraPosition mCameraPosition;

		public bool isDrawn;


		public CustomMapRenderer() {
			initListenerForUpdatedPins();
			initListenerForPolyline();
			initListenerModifyZoom();
		}

		/// <summary>
		/// Invoke the GetMapAsync method which returns an instance of the map.
		/// </summary>
		/// <param name="e">A view object</param>
		protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e) {
			base.OnElementChanged(e);

			if (e.OldElement != null) {
				map.InfoWindowClick -= OnInfoWindowClick;
			}

			if (e.NewElement != null) {
				initRendererAttributes(e);

				if (Control != null) {
					Control.GetMapAsync(this);
				}
			}
		}

		private void initRendererAttributes(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e) {
			if (e != null) {
				formsMap = (CustomMap)e.NewElement;
				customPins = formsMap.customPins;
			}
		}

		/// <summary>
		/// Callback function called by the GetMapAsync method.
		/// </summary>
		/// <param name="googleMap">A googleMap</param>
		public void OnMapReady(GoogleMap googleMap) {
			// Set the map's properties
			map = googleMap;
			map.SetInfoWindowAdapter(this);
			map.UiSettings.ZoomControlsEnabled = false;

			// Override the events of the map's elements
			map.InfoWindowClick += OnInfoWindowClick;
			map.SetOnMarkerClickListener(this);

			initCameraChangedListener();
		}

		/// <summary>
		/// Insert a custom pin into the map.
		/// </summary>
		/// <param name="sender">A GoogleMap object</param>
		/// <param name="e">An event</param>
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (sender != null && e != null) {
				try {
					base.OnElementPropertyChanged(sender, e);

					if (e.PropertyName.Equals(ATTR_DRAW_PINS) && !isDrawn) {
						drawPins();
					}
				} catch (Exception expt) {
					if (expt.StackTrace != null) {
						DebugHelper.newMsg(TAG, expt.StackTrace);
					}
				}
			}
		}

		public void drawPins() {
			if (map != null) {
				map.Clear();

				if (customPins != null && customPins.Count > 0) {
					var regularPinDescriptor = getRegularPinDescriptor();

					if (regularPinDescriptor != null) {

						foreach (var pin in customPins) {
							var marker = new MarkerOptions();
							marker.SetPosition(new LatLng(pin.pin.Position.Latitude, pin.pin.Position.Longitude));
							marker.SetTitle(pin.pin.Label);
							marker.SetSnippet(pin.pin.Address);
							marker.SetIcon(regularPinDescriptor);

							map.AddMarker(marker);
						}

						isDrawn = true;
					}
				}
			}
		}

		private BitmapDescriptor getRegularPinDescriptor() {
			try {
				// var myIcon = new Google.MarkerImage("images/marker-icon.png", null, null, null, new google.maps.Size(21, 30));
				var px = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, MARKER_AREA_DP, Resources.DisplayMetrics);

				// Create a Bitmap to draw on
				using (var bitmap = Bitmap.CreateBitmap(px, px, Bitmap.Config.Argb8888)) {

					// Create a canvas to draw on
					using (var canvas = new Canvas(bitmap)) {

						// Load additional Drawables to draw on top of the circle
						using (var pin = ContextCompat.GetDrawable(Context, Resource.Drawable.ico_pin)) {
							pin.SetBounds((int)(px * 0.1f), (int)(px * 0.1f), (int)(px * 0.7f), px);
							pin.Draw(canvas);
						}
					}

					return BitmapDescriptorFactory.FromBitmap(bitmap);
				}
			} catch (Exception e) {
				if (e.StackTrace != null) {
					DebugHelper.newMsg(TAG, e.StackTrace);
				}
			}

			return null;
		}

		/// <summary>
		/// Method executed when the layout changes
		/// </summary>
		/// <param name="changed">hasChanged?</param>
		/// <param name="l">left coordinate</param>
		/// <param name="t">top coordinate</param>
		/// <param name="r">right coordinate</param>
		/// <param name="b">bottom coordinate</param>
		protected override void OnLayout(bool changed, int l, int t, int r, int b) {
			try {
				base.OnLayout(changed, l, t, r, b);

				isDrawn &= !changed;
			} catch (MissingMethodException) {
				DebugHelper.newMsg(TAG, "Tried to access the map when it's not ready");
			}
		}

		/// <summary>
		/// This method is called whenever a user clicks on a pin. It calls the PCL method
		/// that opens the location details page.
		/// </summary>
		/// <param name="sender">A GoogleMap object</param>
		/// <param name="e">A click event</param>
		void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e) {
			if (MapsController.isClickEnabled()) {
				var customPin = GetCustomPin(e.Marker);

				if (customPin != null) {
					if (!string.IsNullOrWhiteSpace(customPin.id)) {
						var id = customPin.id;
						formsMap.openSelectedLocactionDetails(id);
					}
				}
			}
		}

		/// <summary>
		/// Handles clicks on the pins calling the ShowInfoWindow method to display the marker information
		/// </summary>
		/// <param name="marker">A marker object</param>
		/// <returns>True means that the default event should not run</returns>
		public bool OnMarkerClick(Marker marker) {
			if (lastOpenned != null) {
				lastOpenned.HideInfoWindow();
			}

			if (marker != null) {
				// Open the info window for the marker
				marker.ShowInfoWindow();

				// Re-assign the last openned marker, this way we can close it later
				lastOpenned = marker;

				// Returning true means that we are handling the behavior of the MarkerClick
				return true;
			}

			return false;
		}

		/// <summary>
		/// Return a view which corresponds to the custom info window of a selected marker or null if the 
		/// the layout inflater doesn't initialize.
		/// </summary>
		/// <param name="marker">A marker</param>
		/// <returns>A view</returns>
		public Android.Views.View GetInfoContents(Marker marker) {
			var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService)
								  as Android.Views.LayoutInflater;

			if (inflater != null) {
				// Create a new view to receive the info window container
				Android.Views.View view;

				// Get the custom pin object of the selected marker
				var customPin = GetCustomPin(marker);

				if (customPin != null) {
					// Identify the sort of pin and load the proper view
					view = inflater.Inflate(Resource.Layout.map_location_info_dialog, null);

					// Load the view information
					var infoTitle = view.FindViewById<TextView>(Resource.Id.mapLocationInfoTitle);
					var infoDescription = view.FindViewById<TextView>(Resource.Id.mapLocationInfoDescrition);

					if (infoTitle != null) {
						infoTitle.Text = marker.Title;
					}
					if (infoDescription != null) {
						infoDescription.Text = marker.Snippet;
					}

					return view;
				}
			}

			return null;
		}

		/// <summary>
		/// Return null for the default response of the getInfoWindow method, in order to override the behavior
		/// of the info window.
		/// </summary>
		/// <param name="marker">A marker</param>
		/// <returns>A view</returns>
		public Android.Views.View GetInfoWindow(Marker marker) {
			return null;
		}

		/// <summary>
		/// Get a Custom pin from the list based on a selected marker.
		/// </summary>
		/// <param name="marker">A marker</param>
		/// <returns>A customPin object (C# pin object)</returns>
		CustomPin GetCustomPin(Marker marker) {
			if (marker != null) {
				var position = new Position(marker.Position.Latitude, marker.Position.Longitude);

				foreach (var pin in customPins) {
					if (pin.pin.Position == position) {
						return pin;
					}
				}
			}

			return null;
		}

		private void initCameraChangedListener() {
			if (map != null) {
				map.CameraChange += delegate (object sender, GoogleMap.CameraChangeEventArgs e) {
					if (sender != null && e != null) {
						mCameraPosition = e.Position;

						if (mCameraPosition.ToString() != null) {
							DebugHelper.newMsg(TAG, "Current zoom level : " + mCameraPosition);
						}
					}

					if (mEncodedPolyline != null) {
						drawPolyline(mEncodedPolyline);
					}
				};
			}
		}

		private void updateZoomLevel(int zoomLevel) {
			if (map != null) {
				map.AnimateCamera(CameraUpdateFactory.ZoomTo(zoomLevel));
			}
		}

		public void drawPolyline(string encodedPolyline) {
			if (map != null) {
				mEncodedPolyline = encodedPolyline;
				decodedLatLngs = getDecodedPolyline(encodedPolyline);

				var polylineoption = new PolylineOptions();
				polylineoption.InvokeColor(POLYLINE_COLOR);
				polylineoption.Geodesic(true);
				polylineoption.Add(decodedLatLngs);
				map.AddPolyline(polylineoption);
			}
		}

		private LatLng[] getDecodedPolyline(string encodedPolyline) {
			var lstDecodedPoints = FnDecodePolylinePoints(encodedPolyline);
			var latLngs = new LatLng[lstDecodedPoints.Count];
			int idx = -1;

			foreach (Location loc in lstDecodedPoints) {
				var lat = FormatUtils.stringToDouble(loc.localLatitude);
				var lng = FormatUtils.stringToDouble(loc.localLongitude);
				latLngs[++idx] = new LatLng(lat, lng);
			}

			return latLngs;
		}

		private List<Location> FnDecodePolylinePoints(string encodedPoints) {
			if (string.IsNullOrEmpty(encodedPoints))
				return null;

			var poly = new List<Location>();
			var polylinechars = encodedPoints.ToCharArray();
			int index = 0;
			int currentLat = 0;
			int currentLng = 0;
			int next5bits;
			int sum;
			int shifter;

			while (index < polylinechars.Length) {
				// Calculate next latitude
				sum = 0;
				shifter = 0;

				do {
					next5bits = polylinechars[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylinechars.Length);

				if (index >= polylinechars.Length)
					break;

				currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				// Calculate next longitude
				sum = 0;
				shifter = 0;

				do {
					next5bits = polylinechars[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylinechars.Length);

				if (index >= polylinechars.Length && next5bits >= 32)
					break;

				currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				var p = new Location();
				p.localLatitude = (Convert.ToDouble(currentLat) / 100000.0).ToString();
				p.localLongitude = (Convert.ToDouble(currentLng) / 100000.0).ToString();

				poly.Add(p);
			}

			return poly;
		}


		#region MessageListeners
		/// <summary>
		/// Listens to the Geofencing Service and get an updated list of nearby locations.
		/// </summary>
		private void initListenerForUpdatedPins() {
			MessagingCenter.Subscribe<UpdatePinsMessage>(this, UpdatePinsMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(drawPins);
			});
		}

		/// <summary>
		/// Gets an encoded polyline and try to draw the path given by the decoded polyline
		/// </summary>
		private void initListenerForPolyline() {
			MessagingCenter.Subscribe<DrawPolylineMessage>(this, DrawPolylineMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {
					if (message != null) {
						if (message.encodedPolyline != null) {
							drawPolyline(message.encodedPolyline);
						}
					}
				});
			});
		}

		/// <summary>
		/// Listens to request to update the zoom level of the map.
		/// </summary>
		private void initListenerModifyZoom() {
			MessagingCenter.Subscribe<ChangeZoomLevelMessage>(this, ChangeZoomLevelMessage.TAG, message => {
				Device.BeginInvokeOnMainThread(() => {
					if (message != null) {
						updateZoomLevel(message.zoomLevel);
					}
				});
			});
		}
		#endregion

	}

}

/* Notes:
 * 
 * This code can be used to draw something else into the map marker:
       using (var paint = new Paint(PaintFlags.AntiAlias)) {
			// Give the Paint a color
			paint.Color = Android.Graphics.Color.Argb(210, 204, 222, 204);

			// Draw stuff with it, in this case a circle
			canvas.DrawCircle(px / 2f, px / 2f, px / 2f, paint);
	   }
*/
