using System;
using Windows.UI.Notifications;

namespace AppointmentTile.BackgroundTask
{
	public class Updater 
	{
		public static void Update(String xmlContent)
		{
			var xmldocument = new Windows.Data.Xml.Dom.XmlDocument();
			xmldocument.LoadXml(xmlContent);

			var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
			tileUpdater.Update(new TileNotification(xmldocument));
		}
	}
}
