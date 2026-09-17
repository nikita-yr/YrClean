using System.Windows;
using CClean.Core.Models;
using CClean.Core.Services;

namespace CClean.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		if (e.Args.Contains("--auto-clean"))
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			var settings = SettingsService.Load();
			var result = AutoCleanRunner.Run(settings);

			if (settings.NotifyOnComplete)
			{
				try
				{
					NotificationService.ShowBalloon(
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

