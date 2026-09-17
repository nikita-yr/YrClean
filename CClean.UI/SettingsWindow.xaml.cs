using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CClean.Core.Models;
using CClean.Core.Services;

namespace CClean.UI;

public partial class SettingsWindow : Window
{
    private readonly CleanSettings _settings;

    public SettingsWindow()
    {
        InitializeComponent();
        _settings = SettingsService.Load();

        ScheduleEnabledCheck.IsChecked = _settings.ScheduleEnabled;
        FrequencyCombo.SelectedIndex = (int)_settings.Frequency;
        DayCombo.SelectedIndex = (int)_settings.WeeklyDay == 0 ? 6 : (int)_settings.WeeklyDay - 1;
        TimeTextBox.Text = _settings.RunAtTime;
        NotifyCheck.IsChecked = _settings.NotifyOnComplete;
        IncludeAutoDiscoveredCheck.IsChecked = _settings.IncludeAutoDiscoveredInScheduledRun;

        UpdateDayVisibility();
    }

    private void FrequencyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateDayVisibility();

    private void UpdateDayVisibility()
    {
        bool isHourly = FrequencyCombo.SelectedIndex == 0;
        bool isWeekly = FrequencyCombo.SelectedIndex == 2;

        TimeLabel.Visibility = isHourly ? Visibility.Collapsed : Visibility.Visible;
        TimeTextBox.Visibility = isHourly ? Visibility.Collapsed : Visibility.Visible;
        DayLabel.Visibility = isWeekly ? Visibility.Visible : Visibility.Collapsed;
        DayCombo.Visibility = isWeekly ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        bool isHourly = FrequencyCombo.SelectedIndex == 0;

        if (!isHourly && !TimeSpan.TryParseExact(TimeTextBox.Text, @"hh\:mm", CultureInfo.InvariantCulture, out _))
        {
            StatusText.Text = "Invalid time format. Use HH:mm, e.g. 03:00.";
            return;
        }

        _settings.ScheduleEnabled = ScheduleEnabledCheck.IsChecked == true;
        _settings.Frequency = (ScheduleFrequency)FrequencyCombo.SelectedIndex;
        _settings.RunAtTime = TimeTextBox.Text;
        _settings.NotifyOnComplete = NotifyCheck.IsChecked == true;
        _settings.IncludeAutoDiscoveredInScheduledRun = IncludeAutoDiscoveredCheck.IsChecked == true;

        var dayNames = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                               DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
        _settings.WeeklyDay = dayNames[DayCombo.SelectedIndex];

        SettingsService.Save(_settings);

        var exePath = Environment.ProcessPath!;
        var vbsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "invisible.vbs");
        var ok = TaskSchedulerService.Register(_settings, exePath, vbsPath);

        StatusText.Text = ok
            ? (_settings.ScheduleEnabled ? "Schedule saved and task registered." : "Schedule disabled, task removed.")
            : "Failed to update the scheduled task.";
    }
}