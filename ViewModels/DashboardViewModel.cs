using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SophicIoTManager.Models;
using SophicIoTManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// ViewModel for the Dashboard view containing charts and statistics.
    /// Uses LiveCharts2 for real-time data visualization.
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        #region Fields

        private readonly IDeviceService _deviceService;
        private const int MAX_LINE_CHART_POINTS = 20;

        #endregion

        #region Observable Properties - Stats

        /// <summary>
        /// Total number of projects.
        /// </summary>
        [ObservableProperty]
        private int _totalProjects;

        /// <summary>
        /// Total number of gateways.
        /// </summary>
        [ObservableProperty]
        private int _totalGateways;

        /// <summary>
        /// Total number of devices.
        /// </summary>
        [ObservableProperty]
        private int _totalDevices;

        /// <summary>
        /// Number of online devices.
        /// </summary>
        [ObservableProperty]
        private int _onlineDevices;

        /// <summary>
        /// Number of offline devices.
        /// </summary>
        [ObservableProperty]
        private int _offlineDevices;

        /// <summary>
        /// Number of devices with errors.
        /// </summary>
        [ObservableProperty]
        private int _errorDevices;

        /// <summary>
        /// Whether the simulation is currently running.
        /// </summary>
        [ObservableProperty]
        private bool _isSimulationRunning;

        #endregion

        #region Observable Properties - Collections

        /// <summary>
        /// Collection of projects for display in the dashboard.
        /// </summary>
        public ObservableCollection<ProjectViewModel> ProjectsCollection { get; } = new();

        /// <summary>
        /// Collection of log entries for display.
        /// </summary>
        public ObservableCollection<DashboardLogEntry> LogEntries { get; } = new();

        /// <summary>
        /// Currently selected item name for details display.
        /// </summary>
        [ObservableProperty]
        private string _selectedItemName = "No Selection";

        /// <summary>
        /// Type of selected item (Project/Gateway/Device).
        /// </summary>
        [ObservableProperty]
        private string _selectedItemType = "";

        /// <summary>
        /// Details of selected item.
        /// </summary>
        [ObservableProperty]
        private string _selectedItemDetails = "";

        #endregion

        #region Chart Series

        /// <summary>
        /// Pie chart series for device status distribution.
        /// </summary>
        public ObservableCollection<ISeries> StatusPieSeries { get; } = new();

        /// <summary>
        /// Bar chart series for devices per project.
        /// </summary>
        public ObservableCollection<ISeries> DevicesPerProjectSeries { get; } = new();

        /// <summary>
        /// Line chart series for real-time sensor values.
        /// </summary>
        public ObservableCollection<ISeries> RealTimeValuesSeries { get; } = new();

        /// <summary>
        /// X-axis configuration for bar chart.
        /// </summary>
        public Axis[] BarChartXAxes { get; private set; } = Array.Empty<Axis>();

        /// <summary>
        /// Y-axis configuration for bar chart.
        /// </summary>
        public Axis[] BarChartYAxes { get; } = new Axis[]
        {
            new Axis
            {
                Name = "Devices",
                MinLimit = 0,
                LabelsPaint = new SolidColorPaint(SKColors.White)
            }
        };

        /// <summary>
        /// X-axis for line chart (time).
        /// </summary>
        public Axis[] LineChartXAxes { get; } = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                LabelsPaint = new SolidColorPaint(SKColors.White),
                Labeler = value => DateTime.FromOADate(value).ToString("HH:mm:ss")
            }
        };

        /// <summary>
        /// Y-axis for line chart (values).
        /// </summary>
        public Axis[] LineChartYAxes { get; } = new Axis[]
        {
            new Axis
            {
                Name = "Value",
                LabelsPaint = new SolidColorPaint(SKColors.White)
            }
        };

        #endregion

        #region Real-time Data Storage

        /// <summary>
        /// Stores recent values for each device for line chart.
        /// Key: DeviceId, Value: List of (Timestamp, Value) pairs.
        /// </summary>
        private readonly Dictionary<Guid, List<(DateTime Time, double Value)>> _deviceValueHistory = new();

        #endregion

        #region Constructor

        public DashboardViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            // Subscribe to device updates for real-time chart
            _deviceService.DeviceUpdated += OnDeviceUpdated;
            _deviceService.DeviceAdded += (s, e) => RefreshAllCharts();
            _deviceService.DeviceRemoved += (s, e) => RefreshAllCharts();
            _deviceService.ProjectAdded += (s, e) => RefreshAllCharts();
            _deviceService.ProjectRemoved += (s, e) => RefreshAllCharts();
            _deviceService.GatewayAdded += (s, e) => RefreshAllCharts();
            _deviceService.GatewayRemoved += (s, e) => RefreshAllCharts();

            InitializeCharts();
        }

        #endregion

        #region Initialization

        private void InitializeCharts()
        {
            // Initialize Pie Chart with empty data
            StatusPieSeries.Add(new PieSeries<int>
            {
                Name = "Online",
                Values = new[] { 0 },
                Fill = new SolidColorPaint(SKColor.Parse("#00E676"))
            });
            StatusPieSeries.Add(new PieSeries<int>
            {
                Name = "Offline",
                Values = new[] { 0 },
                Fill = new SolidColorPaint(SKColor.Parse("#95A5A6"))
            });
            StatusPieSeries.Add(new PieSeries<int>
            {
                Name = "Error",
                Values = new[] { 0 },
                Fill = new SolidColorPaint(SKColor.Parse("#FF5252"))
            });
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates all dashboard data from the service.
        /// Called when data is loaded or changed.
        /// </summary>
        public void UpdateFromProjects(IEnumerable<ProjectViewModel> projects)
        {
            var projectList = projects.ToList();

            // Update stats
            TotalProjects = projectList.Count;
            TotalGateways = projectList.Sum(p => p.Gateways.Count);
            TotalDevices = projectList.Sum(p => p.GetTotalDeviceCount());
            OnlineDevices = projectList.Sum(p => p.GetOnlineDeviceCount());

            // Calculate offline and error counts
            int offline = 0, error = 0;
            foreach (var project in projectList)
            {
                foreach (var device in project.Devices)
                {
                    if (device.Status == DeviceStatus.Offline) offline++;
                    else if (device.Status == DeviceStatus.Error) error++;
                }
                foreach (var gateway in project.Gateways)
                {
                    foreach (var device in gateway.Devices)
                    {
                        if (device.Status == DeviceStatus.Offline) offline++;
                        else if (device.Status == DeviceStatus.Error) error++;
                    }
                }
            }
            OfflineDevices = offline;
            ErrorDevices = error;

            // Update ProjectsCollection for UI binding
            ProjectsCollection.Clear();
            foreach (var project in projectList)
            {
                ProjectsCollection.Add(project);
            }

            // Update Pie Chart
            UpdatePieChart();

            // Update Bar Chart
            UpdateBarChart(projectList);
        }

        /// <summary>
        /// Refreshes all charts from current data.
        /// </summary>
        public void RefreshAllCharts()
        {
            // Stats will be updated when UpdateFromProjects is called
        }

        #endregion

        #region Chart Updates

        private void UpdatePieChart()
        {
            if (StatusPieSeries.Count >= 3)
            {
                ((PieSeries<int>)StatusPieSeries[0]).Values = new[] { OnlineDevices };
                ((PieSeries<int>)StatusPieSeries[1]).Values = new[] { OfflineDevices };
                ((PieSeries<int>)StatusPieSeries[2]).Values = new[] { ErrorDevices };
            }
        }

        private void UpdateBarChart(List<ProjectViewModel> projects)
        {
            DevicesPerProjectSeries.Clear();

            var labels = new List<string>();
            var values = new List<int>();

            foreach (var project in projects)
            {
                labels.Add(project.Name);
                values.Add(project.GetTotalDeviceCount());
            }

            DevicesPerProjectSeries.Add(new ColumnSeries<int>
            {
                Name = "Devices",
                Values = values.ToArray(),
                Fill = new SolidColorPaint(SKColor.Parse("#00D9FF"))
            });

            BarChartXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels.ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColors.White),
                    LabelsRotation = 15
                }
            };
            OnPropertyChanged(nameof(BarChartXAxes));
        }

        private void UpdateLineChart(Device device)
        {
            // Initialize history for this device if needed
            if (!_deviceValueHistory.ContainsKey(device.Id))
            {
                _deviceValueHistory[device.Id] = new List<(DateTime, double)>();
            }

            var history = _deviceValueHistory[device.Id];
            history.Add((DateTime.Now, device.Value));

            // Keep only last N points
            while (history.Count > MAX_LINE_CHART_POINTS)
            {
                history.RemoveAt(0);
            }

            // Rebuild line chart series
            RebuildLineChart();
        }

        private void RebuildLineChart()
        {
            RealTimeValuesSeries.Clear();

            // Define colors for different devices
            var colors = new[]
            {
                SKColor.Parse("#00D9FF"), // Cyan
                SKColor.Parse("#00E676"), // Green
                SKColor.Parse("#FFB300"), // Amber
                SKColor.Parse("#FF5252"), // Red
                SKColor.Parse("#7C4DFF"), // Purple
                SKColor.Parse("#FF6E40")  // Deep Orange
            };

            int colorIndex = 0;
            foreach (var kvp in _deviceValueHistory.Take(6)) // Limit to 6 lines for readability
            {
                var points = kvp.Value.Select(v => new LiveChartsCore.Defaults.DateTimePoint(v.Time, v.Value)).ToList();

                RealTimeValuesSeries.Add(new LineSeries<LiveChartsCore.Defaults.DateTimePoint>
                {
                    Name = $"Device {colorIndex + 1}",
                    Values = points,
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length], 2),
                    GeometrySize = 4,
                    GeometryStroke = new SolidColorPaint(colors[colorIndex % colors.Length], 2),
                    Fill = null
                });

                colorIndex++;
            }
        }

        #endregion

        #region Event Handlers

        private void OnDeviceUpdated(object? sender, DeviceEventArgs e)
        {
            if (e.Device.Status == DeviceStatus.Online)
            {
                UpdateLineChart(e.Device);
            }
        }

        #endregion

        #region Log Methods

        /// <summary>
        /// Adds a log entry to the dashboard log.
        /// </summary>
        public void AddLogEntry(string level, string message)
        {
            var entry = new DashboardLogEntry
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Level = level,
                Message = message
            };

            // Add at beginning (most recent first)
            LogEntries.Insert(0, entry);

            // Keep only last 50 entries
            while (LogEntries.Count > 50)
            {
                LogEntries.RemoveAt(LogEntries.Count - 1);
            }
        }

        /// <summary>
        /// Updates the selected item details for display.
        /// </summary>
        public void UpdateSelectedItem(object? item)
        {
            if (item is ProjectViewModel project)
            {
                SelectedItemName = project.Name;
                SelectedItemType = "Project";
                SelectedItemDetails = $"Gateways: {project.Gateways.Count}, Devices: {project.GetTotalDeviceCount()}";
            }
            else if (item is GatewayViewModel gateway)
            {
                SelectedItemName = gateway.Name;
                SelectedItemType = "Gateway";
                SelectedItemDetails = $"Status: {gateway.StatusDisplay}, Devices: {gateway.Devices.Count}";
            }
            else if (item is DeviceViewModel device)
            {
                SelectedItemName = device.Name;
                SelectedItemType = "Device";
                SelectedItemDetails = $"Status: {device.StatusDisplay}, Value: {device.ValueDisplay}";
            }
            else
            {
                SelectedItemName = "No Selection";
                SelectedItemType = "";
                SelectedItemDetails = "";
            }
        }

        /// <summary>
        /// Updates the simulation status.
        /// </summary>
        public void UpdateSimulationStatus(bool isRunning)
        {
            IsSimulationRunning = isRunning;
        }

        #endregion
    }

    /// <summary>
    /// Represents a log entry for the dashboard display.
    /// </summary>
    public class DashboardLogEntry
    {
        public string Timestamp { get; set; } = "";
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";

        /// <summary>
        /// Color based on log level.
        /// </summary>
        public string LevelColor => Level.ToLower() switch
        {
            "error" => "#FF6B6B",
            "warn" or "warning" => "#FFD166",
            "success" or "info" => "#06D6A0",
            _ => "#778DA9"
        };
    }
}
