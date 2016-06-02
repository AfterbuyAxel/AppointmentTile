using System;
using Windows.UI.Xaml.Media;

namespace AppointmentTile.Data
{
	public class AppointmentModel
	{
		public String Subject { get; internal set; }

		public DateTime StartTime { get; internal set; }

		public Boolean IsAllDay { get; internal set; }

		public TimeSpan Duration { get; internal set; }
		public Boolean IsBirthday { get; internal set; }
		public SolidColorBrush DisplayBrush { get; internal set; }
	}
}
