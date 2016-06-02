using System;
using Windows.ApplicationModel.Background;

namespace AppointmentTile.BackgroundTask
{
	public class Registrator
	{
		private const String TASK_NAME = "AppointmentTileUpdater";
		private const String TASK_ENTRY_POINT = "AppointmentTile.BackgroundTask.TileUpdateTask";

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
}
