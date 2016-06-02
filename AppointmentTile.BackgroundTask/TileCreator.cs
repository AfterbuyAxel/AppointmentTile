using System;
using System.Collections.Generic;
using AppointmentTile.Data;
using NotificationsExtensions;
using NotificationsExtensions.Tiles;

namespace AppointmentTile.BackgroundTask
{
	public class TileCreator
	{
		public static String Create(IEnumerable<AppointmentModel> appointments)
		{
			return new TileContent()
			{
				Visual = new TileVisual()
				{
					Branding = TileBranding.None,
					TileWide = new TileBinding()
					{
						Content = CreateTileContent(appointments)
					}
				}
			}.GetContent();

		}

		private static ITileBindingContent CreateTileContent(IEnumerable<AppointmentModel> appointments)
		{
			var content = new TileBindingContentAdaptive() { TextStacking = TileTextStacking.Top };

			foreach (var item in appointments)
			{
				content.Children.Add(CreateTileContentChild(item));
			}

			return content;
		}

		private static ITileAdaptiveChild CreateTileContentChild(AppointmentModel item)
		{
			return new AdaptiveText() { Text = CreateItemText(item), HintAlign = AdaptiveTextAlign.Left, HintMaxLines = 1 };
		}

		private static String CreateItemText(AppointmentModel item)
		{
			const String TEMPLATE_ALLDAY = "{0:dd.MM} {1}";
			const String TEMPLATE_STARTTIME = "{0:dd.MM} {0:t} {1:g} {2}";

			if (item.IsAllDay)
				return String.Format(TEMPLATE_ALLDAY, item.StartTime, item.Subject);

			return String.Format(TEMPLATE_STARTTIME, item.StartTime, item.Duration, item.Subject);
		}

	}
}
