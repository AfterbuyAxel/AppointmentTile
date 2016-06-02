using AppointmentTile.Data;
using Windows.ApplicationModel.Background;

namespace AppointmentTile.BackgroundTask
{
	public class UpdateTask : IBackgroundTask
	{
		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

			var appointments = await AppointmentProvider.ReadAppointmentsAsync();

			Updater.Update(TileCreator.Create(appointments));

			deferral.Complete();
		}
	}
}
