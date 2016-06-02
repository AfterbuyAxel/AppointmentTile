using System;
using System.Collections.ObjectModel;
using AppointmentTile.BackgroundTask;
using AppointmentTile.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AppointmentTile.Frontend
{
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
			Registrator.Register();
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
			Updater.Update(TileCreator.Create(appointments));
		}

	}
}
