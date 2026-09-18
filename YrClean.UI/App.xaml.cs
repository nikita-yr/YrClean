using System.Windows;
using YrClean.Core.Models;
using YrClean.Core.Services;

namespace YrClean.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		if (e.Args.Contains("--elevated-clean") && e.Args.Length >= 2)
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			var manifestPath = e.Args[1];

			try
			{
				var request = PendingCleanRequestService.Load(manifestPath);
				var result = SafeDeleteService.DeleteFiles(
					request.FilePaths,
					request.AllowedRoots,
					request.MinAgeDays);

				NotificationService.ShowBalloonBlocking(
					"YrClean",
					$"Deleted {result.DeletedCount} administrator-protected files, freed {SizeFormatter.Format(result.FreedBytes)}.");
			}
			catch (Exception)
			{
				// Elevated cleanup must not leave a console or window open on failure.
			}
			finally
			{
				PendingCleanRequestService.Delete(manifestPath);
			}

			Shutdown();
			return;
		}

		if (e.Args.Contains("--auto-clean"))
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			var settings = SettingsService.Load();
			var result = AutoCleanRunner.Run(settings);

			if (settings.NotifyOnComplete)
			{
				try
				{
					NotificationService.ShowBalloonBlocking(
						"YrClean",
						$"Deleted {result.DeletedCount} files, freed {SizeFormatter.Format(result.FreedBytes)}.");
				}
				catch (Exception)
				{
					// Notification failure must not crash an unattended cleanup.
				}
			}

			Shutdown();
			return;
		}

		new MainWindow().Show();
	}
}

