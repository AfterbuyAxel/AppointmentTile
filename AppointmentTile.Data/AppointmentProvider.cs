using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;

namespace AppointmentTile.Data
{
	public class AppointmentProvider
	{
		public static async Task<IEnumerable<AppointmentModel>> ReadAppointmentsAsync()
		{
			Double daysToGet = 60;
			UInt32 maxItems = 6;
			Boolean includeHidden = false;

			List<AppointmentModel> sortedList = new List<AppointmentModel>((Int32)maxItems);

			var store = await GetStoreAsync();
			if (store == null)
				return sortedList;

			var findAppointmentOptions = new FindAppointmentsOptions();

			findAppointmentOptions.MaxCount = maxItems;
			findAppointmentOptions.IncludeHidden = includeHidden;

			var calendars = await store.FindAppointmentCalendarsAsync(includeHidden ? FindAppointmentCalendarsOptions.IncludeHidden : FindAppointmentCalendarsOptions.None);

			foreach (var calendar in calendars)
			{
				sortedList.AddRange(await ReadAppointmentsFromCalendarAsync(calendar, maxItems, daysToGet, findAppointmentOptions));
			}

			return sortedList.OrderBy(s => s.StartTime).Take((Int32)maxItems);
		}

		private static async Task<IEnumerable<AppointmentModel>> ReadAppointmentsFromCalendarAsync(AppointmentCalendar calendar, UInt32 maxItems, Double daysToGet, FindAppointmentsOptions findAppointmentsOptions)
		{
			var isBirthday = (
				calendar.DisplayName.IndexOf("Geburtstag", StringComparison.CurrentCultureIgnoreCase) >= 0 ||
				calendar.DisplayName.IndexOf("birthday", StringComparison.CurrentCultureIgnoreCase) >= 0
				);
			List<AppointmentModel> result = new List<AppointmentModel>((Int32)maxItems);
			var appointments = await calendar.FindAppointmentsAsync(DateTimeOffset.Now, TimeSpan.FromDays(daysToGet), findAppointmentsOptions);
			foreach (var appointment in appointments)
			{				
				result.Add(
					new AppointmentModel()
					{
						StartTime = appointment.StartTime.LocalDateTime,
						Subject = isBirthday ? "Geb." + RemoveBirthday(appointment.Subject) : appointment.Subject,
						IsAllDay = appointment.AllDay,
						Duration = appointment.Duration
					});
			}
			return result;
		}

		private static String RemoveBirthday(String subject)
		{
			if (subject.StartsWith("Geburtstag von ", StringComparison.CurrentCulture))
				return subject.Substring(15);
			if (subject.EndsWith("birthday", StringComparison.CurrentCulture))
				return subject.Substring(0,subject.Length - 9);
			return subject;
		}

		private static async Task<AppointmentStore> GetStoreAsync()
		{
			try
			{
				return await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
			}
			catch (Exception)
			{
				return null;
			}
		}

	}
}
