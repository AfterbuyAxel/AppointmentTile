using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NotificationsExtensions;
using NotificationsExtensions.Tiles;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
			get
			{
				if (this.appointmentList == null)
					this.appointmentList = new ObservableCollection<AppointmentModel>();

				return appointmentList;
			}
		}

		public MainPage()
		{
			this.InitializeComponent();
			this.AppointmentListView.ItemsSource = this.AppointmentList;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			BackgroundTaskRegistrator.Register();
		}

		private void HamburgerButton_Click(Object sender, RoutedEventArgs e)
		{
			this.BasicSplitView.IsPaneOpen = this.BasicSplitView.IsPaneOpen ? false : true;
		}

		private async void RefreshAppointmentListButton_Click(Object sender, RoutedEventArgs e)
		{
			var appointments = await AppointmentProvider.ReadAppointmentsAsync();
			this.appointmentList.Clear();
			foreach (var item in appointments)
			{
				this.appointmentList.Add(item);
			}
			TileUpdater.Update(TileCreator.Create(appointments));
		}

	}

	public class BackgroundTaskRegistrator
	{
		private const String TASK_NAME = "BackgroundUpdater";
		private const String TASK_ENTRY_POINT = "AppointmentTile.BackgroundUpdater";

		public async static void Register()
		{
			var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
			if (backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
				backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
			{
				foreach (var task in BackgroundTaskRegistration.AllTasks)
				{
					if (task.Value.Name == TASK_NAME)
					{
						task.Value.Unregister(true);
					}
				}

				BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
				taskBuilder.Name = TASK_NAME;
				taskBuilder.TaskEntryPoint = TASK_ENTRY_POINT;
				taskBuilder.SetTrigger(new TimeTrigger(15, false));
				var registration = taskBuilder.Register();
			}
		}
	}

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

		private static async Task<IEnumerable<AppointmentModel>> ReadAppointmentsFromCalendarAsync(AppointmentCalendar calendar,UInt32 maxItems,Double daysToGet,FindAppointmentsOptions findAppointmentsOptions)
		{
			List<AppointmentModel> result = new List<AppointmentModel>((Int32)maxItems);
			var appointments = await calendar.FindAppointmentsAsync(DateTimeOffset.Now, TimeSpan.FromDays(daysToGet), findAppointmentsOptions);
			foreach (var appointment in appointments)
			{
				result.Add(
					new AppointmentModel()
					{
						StartTime = appointment.StartTime.LocalDateTime,
						Subject = appointment.Subject,
						IsAllDay = appointment.AllDay,
						Duration = appointment.Duration
					});
			}
			return result;
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
			const String TEMPLATE_ALLDAY = "{0:m} {1}";
			const String TEMPLATE_STARTTIME = "{0:m} {0:t} {1:g} {2}";

			if (item.IsAllDay)
				return String.Format(TEMPLATE_ALLDAY, item.StartTime, item.Subject);

			return String.Format(TEMPLATE_STARTTIME, item.StartTime, item.Duration, item.Subject);
		}

	}

	public class AppointmentModel
	{
		public String Subject { get; internal set; }

		public DateTime StartTime { get; internal set; }

		public Boolean IsAllDay { get; internal set; }

		public String DayAndMonth { get { return this.StartTime.ToString("dd.MM"); } }

		public String Time { get { return this.IsAllDay ? String.Empty : this.StartTime.ToString("HH:mm"); } }

		public TimeSpan Duration { get; internal set; }
		public Boolean IsBirthday { get; internal set; }
		public SolidColorBrush DisplayBrush { get; internal set; }
	}

	public class TileUpdater
	{
		public static void Update(String xmlContent)
		{
			var xmldocument = new Windows.Data.Xml.Dom.XmlDocument();
			xmldocument.LoadXml(xmlContent);

			var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
			tileUpdater.Update(new TileNotification(xmldocument));
		}

	}

	public class BackgroundUpdater : IBackgroundTask
	{
		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

			var appointments = await AppointmentProvider.ReadAppointmentsAsync();

			TileUpdater.Update(TileCreator.Create(appointments));

			deferral.Complete();
		}
	}
}
