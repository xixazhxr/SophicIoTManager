using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophicIoTManager.Models;
using SophicIoTManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// Main ViewModel for the TTN-style IoT Dashboard.
    /// Manages hierarchical navigation and CRUD operations.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private readonly IDeviceService _deviceService;

        #endregion

        #region Child ViewModels

        /// <summary>
        /// ViewModel for the dashboard charts and statistics.
        /// </summary>
        public DashboardViewModel Dashboard { get; }

        /// <summary>
        /// ViewModel for the floating edit modal.
        /// </summary>
        public EditModalViewModel EditModal { get; }

        #endregion

        #region Collapsible Sections

        /// <summary>
        /// Whether the Dashboard section is expanded.
        /// </summary>
        [ObservableProperty]
        private bool _isDashboardExpanded = true;

        /// <summary>
        /// Whether the Projects section is expanded.
        /// </summary>
        [ObservableProperty]
        private bool _isProjectsExpanded = true;

        #endregion

        #region Observable Properties

        /// <summary>
        /// Collection of projects for TreeView display.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ProjectViewModel> _projects = new();

        /// <summary>
        /// Currently selected item in the TreeView (Project, Gateway, or Device).
        /// </summary>
        [ObservableProperty]
        private object? _selectedItem;

        /// <summary>
        /// Collection of system log entries.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<LogEntry> _systemLogs = new();

        /// <summary>
        /// Whether the simulation is running.
        /// </summary>
        [ObservableProperty]
        private bool _isSimulationRunning;

        /// <summary>
        /// Status bar message.
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = "Ready";

        /// <summary>
        /// Total online device count.
        /// </summary>
        [ObservableProperty]
        private int _onlineDeviceCount;

        /// <summary>
        /// Total device count.
        /// </summary>
        [ObservableProperty]
        private int _totalDeviceCount;

        #endregion

        #region New Item Form Properties

        [ObservableProperty]
        private string _newProjectName = string.Empty;

        [ObservableProperty]
        private string _newProjectDescription = string.Empty;

        [ObservableProperty]
        private string _newGatewayName = string.Empty;

        [ObservableProperty]
        private string _newGatewayEUI = string.Empty;

        [ObservableProperty]
        private string _newGatewayLocation = string.Empty;

        [ObservableProperty]
        private string _newGatewayFrequency = "EU868";

        [ObservableProperty]
        private string _newDeviceName = string.Empty;

        [ObservableProperty]
        private DeviceType _newDeviceType = DeviceType.Temperature;

        #endregion

        #region Computed Properties

        public DeviceType[] DeviceTypes => Enum.GetValues<DeviceType>();
        public string[] FrequencyBands => new[] { "EU868", "US915", "AS923", "AU915", "CN470", "IN865" };

        /// <summary>
        /// Gets the selected item as ProjectViewModel.
        /// </summary>
        public ProjectViewModel? SelectedProject => SelectedItem as ProjectViewModel;

        /// <summary>
        /// Gets the selected item as GatewayViewModel.
        /// </summary>
        public GatewayViewModel? SelectedGateway => SelectedItem as GatewayViewModel;

        /// <summary>
        /// Gets the selected item as DeviceViewModel.
        /// </summary>
        public DeviceViewModel? SelectedDevice => SelectedItem as DeviceViewModel;

        /// <summary>
        /// Determines which detail view to show.
        /// </summary>
        public string SelectedItemType => SelectedItem switch
        {
            ProjectViewModel => "Project",
            GatewayViewModel => "Gateway",
            DeviceViewModel => "Device",
            _ => "None"
        };

        #endregion

        #region Constructor

        public MainViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            // Initialize child ViewModels
            Dashboard = new DashboardViewModel(deviceService);
            EditModal = new EditModalViewModel(deviceService);

            // Subscribe to service events
            _deviceService.ProjectAdded += OnProjectAdded;
            _deviceService.ProjectRemoved += OnProjectRemoved;
            _deviceService.GatewayAdded += OnGatewayAdded;
            _deviceService.GatewayRemoved += OnGatewayRemoved;
            _deviceService.GatewayUpdated += OnGatewayUpdated;
            _deviceService.DeviceAdded += OnDeviceAdded;
            _deviceService.DeviceRemoved += OnDeviceRemoved;
            _deviceService.DeviceUpdated += OnDeviceUpdated;
            _deviceService.LogAdded += OnLogAdded;

            // Load data and start simulation
            _ = LoadDataAsync();
            StartSimulation();
            Dashboard.UpdateSimulationStatus(IsSimulationRunning);
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            StatusMessage = "Loading projects...";

            try
            {
                var projects = await _deviceService.GetAllProjectsAsync();
                Projects.Clear();

                foreach (var project in projects)
                {
                    var projectVm = new ProjectViewModel(project);

                    // Load gateways
                    foreach (var gateway in project.Gateways)
                    {
                        var gatewayVm = new GatewayViewModel(gateway);

                        // Load gateway devices
                        foreach (var device in gateway.Devices)
                        {
                            gatewayVm.Devices.Add(new DeviceViewModel(device));
                        }
                        gatewayVm.RefreshChildren();
                        projectVm.Gateways.Add(gatewayVm);
                    }

                    // Load direct devices
                    foreach (var device in project.Devices)
                    {
                        projectVm.Devices.Add(new DeviceViewModel(device));
                    }

                    projectVm.RefreshChildren();
                    Projects.Add(projectVm);
                }

                UpdateCounts();
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void UpdateCounts()
        {
            int total = 0;
            int online = 0;

            foreach (var project in Projects)
            {
                total += project.GetTotalDeviceCount();
                online += project.GetOnlineDeviceCount();
            }

            TotalDeviceCount = total;
            OnlineDeviceCount = online;

            // Update dashboard charts
            Dashboard.UpdateFromProjects(Projects);
        }

        #endregion

        #region Project Commands

        [RelayCommand]
        private async Task AddProjectAsync()
        {
            if (string.IsNullOrWhiteSpace(NewProjectName)) return;

            var project = new Project(NewProjectName.Trim(), NewProjectDescription.Trim());
            await _deviceService.AddProjectAsync(project);

            NewProjectName = string.Empty;
            NewProjectDescription = string.Empty;
        }

        [RelayCommand]
        private async Task DeleteProjectAsync()
        {
            if (SelectedProject == null) return;

            var result = MessageBox.Show(
                $"Delete project '{SelectedProject.Name}' and all its contents?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _deviceService.DeleteProjectAsync(SelectedProject.Id);
                SelectedItem = null;
            }
        }

        #endregion

        #region Gateway Commands

        [RelayCommand]
        private async Task AddGatewayAsync()
        {
            if (string.IsNullOrWhiteSpace(NewGatewayName)) return;

            // Determine parent project
            Guid projectId;
            if (SelectedProject != null)
            {
                projectId = SelectedProject.Id;
            }
            else if (SelectedGateway != null)
            {
                projectId = SelectedGateway.ProjectId;
            }
            else if (SelectedDevice != null)
            {
                projectId = SelectedDevice.ProjectId;
            }
            else if (Projects.Count > 0)
            {
                projectId = Projects[0].Id;
            }
            else
            {
                return; // No project to add to
            }

            var gateway = new Gateway(NewGatewayName.Trim(), projectId, NewGatewayEUI.Trim())
            {
                Location = NewGatewayLocation.Trim(),
                FrequencyBand = NewGatewayFrequency
            };

            await _deviceService.AddGatewayAsync(gateway);

            NewGatewayName = string.Empty;
            NewGatewayEUI = string.Empty;
            NewGatewayLocation = string.Empty;
        }

        [RelayCommand]
        private async Task DeleteGatewayAsync()
        {
            if (SelectedGateway == null) return;

            var result = MessageBox.Show(
                $"Delete gateway '{SelectedGateway.Name}' and all its devices?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _deviceService.DeleteGatewayAsync(SelectedGateway.Id);
                SelectedItem = null;
            }
        }

        [RelayCommand]
        private async Task ToggleGatewayStatusAsync()
        {
            if (SelectedGateway == null) return;
            await _deviceService.ToggleGatewayStatusAsync(SelectedGateway.Id);
        }

        #endregion

        #region Device Commands

        [RelayCommand]
        private async Task AddDeviceAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDeviceName)) return;

            // Determine parent
            Guid projectId;
            Guid? gatewayId = null;

            if (SelectedGateway != null)
            {
                projectId = SelectedGateway.ProjectId;
                gatewayId = SelectedGateway.Id;
            }
            else if (SelectedProject != null)
            {
                projectId = SelectedProject.Id;
            }
            else if (SelectedDevice != null)
            {
                projectId = SelectedDevice.ProjectId;
                gatewayId = SelectedDevice.GatewayId;
            }
            else if (Projects.Count > 0)
            {
                projectId = Projects[0].Id;
            }
            else
            {
                return;
            }

            var device = new Device(NewDeviceName.Trim(), NewDeviceType, projectId, gatewayId);
            await _deviceService.AddDeviceAsync(device);

            NewDeviceName = string.Empty;
            UpdateCounts();
        }

        [RelayCommand]
        private async Task DeleteDeviceAsync()
        {
            if (SelectedDevice == null) return;

            var result = MessageBox.Show(
                $"Delete device '{SelectedDevice.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _deviceService.DeleteDeviceAsync(SelectedDevice.Id);
                SelectedItem = null;
                UpdateCounts();
            }
        }

        [RelayCommand]
        private async Task ToggleDeviceStatusAsync()
        {
            if (SelectedDevice == null) return;
            await _deviceService.ToggleDeviceStatusAsync(SelectedDevice.Id);
        }

        #endregion

        #region Edit Modal Commands

        /// <summary>
        /// Opens the edit modal for the currently selected item.
        /// </summary>
        [RelayCommand]
        private void EditSelectedItem()
        {
            if (SelectedProject != null)
            {
                EditModal.OpenForProject(SelectedProject);
            }
            else if (SelectedGateway != null)
            {
                EditModal.OpenForGateway(SelectedGateway);
            }
            else if (SelectedDevice != null)
            {
                EditModal.OpenForDevice(SelectedDevice);
            }
        }

        /// <summary>
        /// Opens the edit modal for a specific project.
        /// </summary>
        [RelayCommand]
        private void EditProject(ProjectViewModel project)
        {
            if (project != null)
            {
                EditModal.OpenForProject(project);
            }
        }

        /// <summary>
        /// Opens the edit modal for a specific gateway.
        /// </summary>
        [RelayCommand]
        private void EditGateway(GatewayViewModel gateway)
        {
            if (gateway != null)
            {
                EditModal.OpenForGateway(gateway);
            }
        }

        /// <summary>
        /// Opens the edit modal for a specific device.
        /// </summary>
        [RelayCommand]
        private void EditDevice(DeviceViewModel device)
        {
            if (device != null)
            {
                EditModal.OpenForDevice(device);
            }
        }

        #endregion

        #region Simulation Commands

        [RelayCommand]
        private void StartSimulation()
        {
            _deviceService.StartSimulation();
            IsSimulationRunning = _deviceService.IsSimulationRunning;
            Dashboard.UpdateSimulationStatus(IsSimulationRunning);
            StatusMessage = "Simulation Running";
        }

        [RelayCommand]
        private void StopSimulation()
        {
            _deviceService.StopSimulation();
            IsSimulationRunning = _deviceService.IsSimulationRunning;
            Dashboard.UpdateSimulationStatus(IsSimulationRunning);
            StatusMessage = "Simulation Stopped";
        }

        [RelayCommand]
        private void ToggleSimulation()
        {
            if (IsSimulationRunning)
                StopSimulation();
            else
                StartSimulation();
        }

        [RelayCommand]
        private void ClearLogs()
        {
            SystemLogs.Clear();
        }

        #endregion

        #region Selection Changed

        partial void OnSelectedItemChanged(object? value)
        {
            OnPropertyChanged(nameof(SelectedProject));
            OnPropertyChanged(nameof(SelectedGateway));
            OnPropertyChanged(nameof(SelectedDevice));
            OnPropertyChanged(nameof(SelectedItemType));
        }

        #endregion

        #region Event Handlers

        private void OnProjectAdded(object? sender, ProjectEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = new ProjectViewModel(e.Project);
                Projects.Add(projectVm);
                UpdateCounts();
            });
        }

        private void OnProjectRemoved(object? sender, ProjectEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Project.Id);
                if (projectVm != null)
                {
                    Projects.Remove(projectVm);
                }
                UpdateCounts();
            });
        }

        private void OnGatewayAdded(object? sender, GatewayEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Gateway.ProjectId);
                if (projectVm != null)
                {
                    var gatewayVm = new GatewayViewModel(e.Gateway);
                    projectVm.Gateways.Add(gatewayVm);
                    projectVm.RefreshChildren();
                }
                UpdateCounts();
            });
        }

        private void OnGatewayRemoved(object? sender, GatewayEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Gateway.ProjectId);
                if (projectVm != null)
                {
                    var gatewayVm = projectVm.Gateways.FirstOrDefault(g => g.Id == e.Gateway.Id);
                    if (gatewayVm != null)
                    {
                        projectVm.Gateways.Remove(gatewayVm);
                        projectVm.RefreshChildren();
                    }
                }
                UpdateCounts();
            });
        }

        private void OnGatewayUpdated(object? sender, GatewayEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Gateway.ProjectId);
                var gatewayVm = projectVm?.Gateways.FirstOrDefault(g => g.Id == e.Gateway.Id);
                gatewayVm?.Refresh();
                UpdateCounts();
            });
        }

        private void OnDeviceAdded(object? sender, DeviceEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Device.ProjectId);
                if (projectVm != null)
                {
                    var deviceVm = new DeviceViewModel(e.Device);

                    if (e.Device.GatewayId.HasValue)
                    {
                        var gatewayVm = projectVm.Gateways.FirstOrDefault(g => g.Id == e.Device.GatewayId);
                        if (gatewayVm != null)
                        {
                            gatewayVm.Devices.Add(deviceVm);
                            gatewayVm.RefreshChildren();
                        }
                    }
                    else
                    {
                        projectVm.Devices.Add(deviceVm);
                    }
                    projectVm.RefreshChildren();
                }
                UpdateCounts();
            });
        }

        private void OnDeviceRemoved(object? sender, DeviceEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var projectVm = Projects.FirstOrDefault(p => p.Id == e.Device.ProjectId);
                if (projectVm != null)
                {
                    if (e.Device.GatewayId.HasValue)
                    {
                        var gatewayVm = projectVm.Gateways.FirstOrDefault(g => g.Id == e.Device.GatewayId);
                        var deviceVm = gatewayVm?.Devices.FirstOrDefault(d => d.Id == e.Device.Id);
                        if (deviceVm != null && gatewayVm != null)
                        {
                            gatewayVm.Devices.Remove(deviceVm);
                            gatewayVm.RefreshChildren();
                        }
                    }
                    else
                    {
                        var deviceVm = projectVm.Devices.FirstOrDefault(d => d.Id == e.Device.Id);
                        if (deviceVm != null)
                        {
                            projectVm.Devices.Remove(deviceVm);
                        }
                    }
                    projectVm.RefreshChildren();
                }
                UpdateCounts();
            });
        }

        private void OnDeviceUpdated(object? sender, DeviceEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                // Find and refresh the device
                foreach (var project in Projects)
                {
                    // Check direct devices
                    var directDevice = project.Devices.FirstOrDefault(d => d.Id == e.Device.Id);
                    if (directDevice != null)
                    {
                        directDevice.Refresh();
                        project.Refresh();
                        UpdateCounts();
                        return;
                    }

                    // Check gateway devices
                    foreach (var gateway in project.Gateways)
                    {
                        var gwDevice = gateway.Devices.FirstOrDefault(d => d.Id == e.Device.Id);
                        if (gwDevice != null)
                        {
                            gwDevice.Refresh();
                            gateway.Refresh();
                            project.Refresh();
                            UpdateCounts();
                            return;
                        }
                    }
                }
            });
        }

        private void OnLogAdded(object? sender, LogEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SystemLogs.Insert(0, e.LogEntry);
                while (SystemLogs.Count > 500)
                {
                    SystemLogs.RemoveAt(SystemLogs.Count - 1);
                }

                // Update Dashboard Log
                Dashboard.AddLogEntry(e.LogEntry.Level.ToString(), e.LogEntry.Message);
            });
        }

        #endregion
    }
}
