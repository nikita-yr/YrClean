using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CClean.Core.Models;
using CClean.Core.Services;

namespace CClean.UI;

public partial class MainWindow : Window
{
    // How many days a file must sit untouched before it's eligible for deletion.
    // Will move into a proper Settings screen later — hardcoded for now.
    private const int MinAgeDays = 14;

    private List<string> _lastScanRoots = new();
    private List<CacheSourceNode> _lastScanNodes = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => RunScan();
    }

    private void RunScan()
    {
        var settings = SettingsService.Load();
        var scanner = new FolderScanner();
        var sources = CacheSourceProvider.GetKnownSources()
            .Concat(AutoDiscoveryScanner.Discover())
            .ToList();
        var nodes = new List<CacheSourceNode>();
        var roots = new List<string>();

        foreach (var source in sources)
        {
            var resolvedPaths = source.ResolvePaths()
                .Where(path => !ExclusionFilter.IsExcluded(path, settings.ExcludedPaths))
                .ToList();
            if (resolvedPaths.Count == 0)
                continue;

            var allGroups = new List<CacheGroup>();
            foreach (var path in resolvedPaths)
            {
                allGroups.AddRange(scanner.ScanGrouped(path));
                roots.Add(path);
            }

            if (allGroups.Sum(group => group.TotalSizeBytes) == 0)
                continue;

            nodes.Add(new CacheSourceNode(source, allGroups, resolvedPaths.First()));
        }

        nodes = nodes.OrderByDescending(node => node.TotalSizeBytes).ToList();

        _lastScanNodes = nodes;
        _lastScanRoots = roots;
        ResultsTree.ItemsSource = nodes;
        SelectAllCheck.IsChecked = false;
    }

    private void SelectAllCheck_Checked(object sender, RoutedEventArgs e)
    {
        foreach (var node in _lastScanNodes)
            node.IsSelected = true;
    }

    private void SelectAllCheck_Unchecked(object sender, RoutedEventArgs e)
    {
        foreach (var node in _lastScanNodes)
            node.IsSelected = false;
    }

    private void CleanButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedFiles = _lastScanNodes
            .SelectMany(source => source.Children)
            .SelectMany(group => group.Children)
            .Where(file => file.IsSelected)
            .Select(file => file.FullPath)
            .ToList();

        if (selectedFiles.Count == 0)
        {
            System.Windows.MessageBox.Show("No files selected.", "Clean", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = System.Windows.MessageBox.Show(
            $"Delete {selectedFiles.Count} selected files?\nFiles accessed within the last {MinAgeDays} days will be skipped automatically for safety.",
            "Confirm cleanup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        var result = SafeDeleteService.DeleteFiles(selectedFiles, _lastScanRoots, MinAgeDays);

        System.Windows.MessageBox.Show(
            $"Deleted: {result.DeletedCount} files\nFreed: {SizeFormatter.Format(result.FreedBytes)}\nSkipped: {result.SkippedCount}",
            "Cleanup complete",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        RunScan();
    }

    private void ScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new SettingsWindow { Owner = this };
        window.ShowDialog();
    }

    private void AddToExclusions_Click(object sender, RoutedEventArgs e)
    {
        var item = GetClickedItem(sender);
        string? path = item switch
        {
            CacheGroupNode group => group.FullFolderPath,
            CacheSourceNode source => source.FullFolderPath,
            _ => null
        };

        if (path == null)
            return;

        var settings = SettingsService.Load();
        if (!settings.ExcludedPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            settings.ExcludedPaths.Add(path);
            SettingsService.Save(settings);
        }

        System.Windows.MessageBox.Show(
            $"Added to exclusions:\n{path}",
            "Exclusions",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        RunScan();
    }

    private object? GetClickedItem(object sender)
    {
        if (sender is MenuItem menuItem &&
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is FrameworkElement target)
        {
            return target.DataContext;
        }
        return null;
    }

    private void OpenLocation_Click(object sender, RoutedEventArgs e)
    {
        var item = GetClickedItem(sender);

        switch (item)
        {
            case CacheFileNode file:
                Process.Start("explorer.exe", $"/select,\"{file.FullPath}\"");
                break;
            case CacheGroupNode group:
                Process.Start("explorer.exe", $"\"{group.FullFolderPath}\"");
                break;
            case CacheSourceNode source:
                Process.Start("explorer.exe", $"\"{source.FullFolderPath}\"");
                break;
        }
    }

    private void SearchWeb_Click(object sender, RoutedEventArgs e)
    {
        var item = GetClickedItem(sender);
        string? name = item switch
        {
            CacheFileNode file => file.Name,
            CacheGroupNode group => group.Name,
            CacheSourceNode source => source.Name,
            _ => null
        };

        if (name == null) return;

        var query = Uri.EscapeDataString(name);
        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://www.google.com/search?q={query}",
            UseShellExecute = true
        });
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu && menu.PlacementTarget is Grid grid)
        {
            grid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0x00, 0xA2, 0xFF));
        }
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu && menu.PlacementTarget is Grid grid)
        {
            grid.ClearValue(Grid.BackgroundProperty);
        }
    }
}