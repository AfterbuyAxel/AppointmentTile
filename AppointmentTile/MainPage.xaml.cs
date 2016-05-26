using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace AppointmentTile
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		private ObservableCollection<AppointmentModel> appointmentList;

		public ObservableCollection<AppointmentModel> AppointmentList
		{
			get { return appointmentList; }
		}

		public MainPage()
        {
            this.InitializeComponent();
			this.AppointmentListView.ItemsSource = this.AppointmentList;
			this.FillAppointmentList();
        }

		private async void FillAppointmentList()
		{
			this.appointmentList = new ObservableCollection<AppointmentModel>();
			AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
			var calendars = await store.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions.None);
			var daysToGet = TimeSpan.FromDays(7);
			var findAppointmentOptions = new FindAppointmentsOptions() { IncludeHidden = false, MaxCount = 10 };
			foreach (var calendar in calendars)
			{
				var appointments = await calendar.FindAppointmentsAsync(DateTimeOffset.Now, daysToGet, findAppointmentOptions);
				foreach (var appointment in appointments)
				{
					this.appointmentList.Add(new AppointmentModel() { StartTime = appointment.StartTime, Subject = appointment.Subject });
				}
			}
		}

		private static async Task<AppointmentStore> GetStore()
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

		private void HamburgerButton_Click(Object sender, RoutedEventArgs e)
		{
			this.BasicSplitView.IsPaneOpen = this.BasicSplitView.IsPaneOpen ? false : true;
		}
	}

	public class AppointmentModel
	{
		public String Subject { get; set; }
		public DateTimeOffset StartTime { get; set; }
	}
}
