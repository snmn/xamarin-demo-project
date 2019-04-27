using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SportsConnection {
	
	public class LocationPostController {

		public LocationPostManager locationPostManager;

		private List<LocationPost> mLocationPosts = new List<LocationPost>();
		public List<LocationPostWrapper> locationPosts = new List<LocationPostWrapper>();
		private Location mLocation;


		public LocationPostController(Location location) {
			locationPostManager = new LocationPostManager();

			mLocation = location;
			loadLocationPosts().ConfigureAwait(false);
		}

		public async Task<bool> loadLocationPosts() {
			mLocationPosts = await locationPostManager.getLocationPostsAsync(mLocation.id, true);
			locationPosts = await getWrappedLocationPosts();
			return true;
		}

		public async Task<bool> addLocationPost(string locationId, string userId, string sportId, string title, 
		                                        string text, DateTime postedDate) {
			var locPost = new LocationPost();
			locPost.locationId = locationId;
			locPost.userId = userId;
			locPost.title = title;
			locPost.text = text;
			
			locPost.postedDate = postedDate;

			if (locationId != null && userId != null && sportId != null && text != null) {
				return await locationPostManager.saveLocationPostAsync(locPost);
			} else {
				return false;
			}
		}

		public async Task<bool> updateLocationPost(LocationPost post) {
			if (post != null) {
				return await locationPostManager.saveLocationPostAsync(post);
			}

			return false;
		}

		public async Task<List<LocationPost>> getLocationPosts(string locationId, bool refresh) {
			if (refresh) {
				mLocationPosts = await locationPostManager.getLocationPostsAsync(locationId);

			} else if (mLocationPosts == null) {
				mLocationPosts = await locationPostManager.getLocationPostsAsync(locationId);
			}

			return mLocationPosts;
		}

		public async Task<List<LocationPostWrapper>> getWrappedLocationPosts() {
			if (mLocationPosts != null) {
				var locationManager = new SCLocationManager();
				var userManager = new UserManager();
				var sportManager = new SportManager();

				var locationIds = new List<string>();
				var userIds = new List<string>();
				var sportIds = new List<string>();

				

				Dictionary<string, Location> locations = await locationManager.getDictLocationsByIds(locationIds);
				Dictionary<string, User> users = await userManager.getDictUsersByIds(userIds);
				Dictionary<string, Sport> sports = await sportManager.getDictSportsByIds(sportIds);

				locationPosts.Clear();

				foreach (LocationPost post in mLocationPosts) {
					if (post != null) {
						if (post.locationId != null && post.userId != null) {
							Location loc = null;
							User user = null;
							Sport sport = null;

							if (locations.ContainsKey(post.locationId)) {
								loc = locations[post.locationId];	
							}

							if (users.ContainsKey(post.userId)) {
								user = users[post.userId];
							}

							

							if (loc != null && user != null) {
								var filledLocationPost = new LocationPostWrapper();
								filledLocationPost.core = post;
								filledLocationPost.user = user;
								filledLocationPost.location = mLocation;

								if (sport != null) {
									filledLocationPost.sport = sport;
								}

								locationPosts.Add(filledLocationPost);
							}
						}
					}
				}
			}

			return locationPosts;
		}

		public async Task<LocationPost> getLocationPostById(string locationPostId) {
			return await locationPostManager.getLocationPostById(locationPostId);
		}

		public async Task<bool> deleteLocationPost(string locationPostId) {
			if (locationPostId != null) {
				return await locationPostManager.deleteLocationPostAsync(locationPostId);
			}

			return false;
		}

	}

}