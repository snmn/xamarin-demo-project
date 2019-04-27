using System;
using Xamarin.Forms;

namespace SportsConnection {

	class MessagesTemplateSelector : DataTemplateSelector {

		private readonly DataTemplate mIncomingDataTemplate;
		private readonly DataTemplate mOutgoingDataTemplate;


		public MessagesTemplateSelector() {
			mIncomingDataTemplate = new DataTemplate(typeof(OtherUserMessageViewCell));
			mOutgoingDataTemplate = new DataTemplate(typeof(CurrentUserMessageViewCell));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container) {
			try {
				var messageVm = item as LocationPostWrapper;

				if (messageVm == null) {
					return null;
				}

				if (messageVm.user != null && App.authController != null) {

					if (messageVm.user.id != null && App.authController.getCurrentUser() != null) {

						if (App.authController.getCurrentUser().id != null) {

							if (messageVm.user.uid == App.authController.getCurrentUser().uid) {
								return mOutgoingDataTemplate;
							} else {
								return mIncomingDataTemplate;
							}
						} else {
							return mIncomingDataTemplate;
						}
					} else {
						return mIncomingDataTemplate;
					}
				} else {
					return mIncomingDataTemplate;
				}
			} catch (InvalidCastException e) {
				if (e.StackTrace != null) {
					DebugHelper.newMsg(Constants.TAG_TEMPLATE_SELECTOR, e.StackTrace);
				}

				return null;
			}
		}

	}

}