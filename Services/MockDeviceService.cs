using SophicIoTManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SophicIoTManager.Services
{
    /// <summary>
    /// Mock implementation of IDeviceService with hierarchical IoT data.
    /// Simulates real IoT device behavior for testing and demonstration.
    /// </summary>
    public class MockDeviceService : IDeviceService, IDisposable
    {
        #region Fields

        private readonly List<Project> _projects;
        private readonly List<Gateway> _gateways;
        private readonly List<Device> _devices;
        private readonly Timer _simulationTimer;
        private readonly Random _random;
        private readonly object _lockObject = new();
        private bool _isSimulationRunning;
        private bool _isDisposed;

        private const int SIMULATION_INTERVAL_MS = 2000;
        private const int MIN_CONNECTION_DELAY_MS = 500;
        private const int MAX_CONNECTION_DELAY_MS = 1500;

        #endregion

        #region Events

        public event EventHandler<ProjectEventArgs>? ProjectAdded;
        public event EventHandler<ProjectEventArgs>? ProjectUpdated;
        public event EventHandler<ProjectEventArgs>? ProjectRemoved;
        public event EventHandler<GatewayEventArgs>? GatewayAdded;
        public event EventHandler<GatewayEventArgs>? GatewayUpdated;
        public event EventHandler<GatewayEventArgs>? GatewayRemoved;
        public event EventHandler<DeviceEventArgs>? DeviceUpdated;
        public event EventHandler<DeviceEventArgs>? DeviceAdded;
        public event EventHandler<DeviceEventArgs>? DeviceRemoved;
        public event EventHandler<LogEventArgs>? LogAdded;

        #endregion

        #region Properties

        public bool IsSimulationRunning => _isSimulationRunning;

        #endregion

        #region Constructor

        public MockDeviceService()
        {
            _projects = new List<Project>();
            _gateways = new List<Gateway>();
            _devices = new List<Device>();
            _random = new Random();
            _simulationTimer = new Timer(SimulationCallback, null, Timeout.Infinite, Timeout.Infinite);

            InitializeSampleData();
        }

        #endregion

        #region Initialization

        private void InitializeSampleData()
        {
            // Create sample project
            var project1 = new Project("Smart Factory Floor", "Industrial IoT monitoring for manufacturing plant");
            _projects.Add(project1);

            // Create gateways for project 1
            var gw1 = new Gateway("Gateway-North", project1.Id) 
            { 
                Location = "Building A, Floor 1",
                FrequencyBand = "EU868",
                Status = GatewayStatus.Online
            };
            var gw2 = new Gateway("Gateway-South", project1.Id)
            {
                Location = "Building A, Floor 2",
                FrequencyBand = "EU868",
                Status = GatewayStatus.Online
            };
            _gateways.Add(gw1);
            _gateways.Add(gw2);

            // Create devices under gateway 1
            var dev1 = new Device("Temp-Sensor-01", DeviceType.Temperature, project1.Id, gw1.Id, DeviceStatus.Online) { Value = 23.5 };
            var dev2 = new Device("Vibration-Monitor-01", DeviceType.Vibration, project1.Id, gw1.Id, DeviceStatus.Online) { Value = 45.8 };
            _devices.Add(dev1);
            _devices.Add(dev2);

            // Create devices under gateway 2
            var dev3 = new Device("Pressure-Gauge-01", DeviceType.Pressure, project1.Id, gw2.Id, DeviceStatus.Online) { Value = 101.3 };
            var dev4 = new Device("Humidity-Sensor-01", DeviceType.Humidity, project1.Id, gw2.Id, DeviceStatus.Offline) { Value = 0 };
            _devices.Add(dev3);
            _devices.Add(dev4);

            // Create device directly under project (no gateway)
            var dev5 = new Device("Standalone-Temp-01", DeviceType.Temperature, project1.Id, null, DeviceStatus.Online) { Value = 21.0 };
            _devices.Add(dev5);

            // Create second project
            var project2 = new Project("Warehouse Monitoring", "Environmental monitoring for storage facility");
            _projects.Add(project2);

            var gw3 = new Gateway("Gateway-Warehouse", project2.Id)
            {
                Location = "Warehouse B",
                FrequencyBand = "US915",
                Status = GatewayStatus.Offline
            };
            _gateways.Add(gw3);

            var dev6 = new Device("Temp-WH-01", DeviceType.Temperature, project2.Id, gw3.Id, DeviceStatus.Offline) { Value = 0 };
            _devices.Add(dev6);

            RaiseLogAdded(LogEntry.Info("System initialized with sample IoT data"));
        }

        #endregion

        #region Project Operations

        public Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            lock (_lockObject)
            {
                // Populate navigation properties
                foreach (var project in _projects)
                {
                    project.Gateways.Clear();
                    project.Devices.Clear();

                    var projectGateways = _gateways.Where(g => g.ProjectId == project.Id);
                    foreach (var gw in projectGateways)
                    {
                        gw.Devices.Clear();
                        var gwDevices = _devices.Where(d => d.GatewayId == gw.Id);
                        foreach (var dev in gwDevices)
                        {
                            gw.Devices.Add(dev);
                        }
                        project.Gateways.Add(gw);
                    }

                    var directDevices = _devices.Where(d => d.ProjectId == project.Id && d.GatewayId == null);
                    foreach (var dev in directDevices)
                    {
                        project.Devices.Add(dev);
                    }
                }

                return Task.FromResult<IEnumerable<Project>>(_projects.ToList());
            }
        }

        public Task<Project?> GetProjectByIdAsync(Guid projectId)
        {
            lock (_lockObject)
            {
                return Task.FromResult(_projects.FirstOrDefault(p => p.Id == projectId));
            }
        }

        public Task<bool> AddProjectAsync(Project project)
        {
            if (project == null)
            {
                RaiseLogAdded(LogEntry.Error("Cannot add null project"));
                return Task.FromResult(false);
            }

            lock (_lockObject)
            {
                if (_projects.Any(p => p.Id == project.Id))
                {
                    RaiseLogAdded(LogEntry.Warning($"Project with ID '{project.Id}' already exists"));
                    return Task.FromResult(false);
                }
                _projects.Add(project);
            }

            RaiseProjectAdded(project);
            RaiseLogAdded(LogEntry.Success($"Project '{project.Name}' created"));
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProjectAsync(Project project)
        {
            if (project == null) return Task.FromResult(false);

            lock (_lockObject)
            {
                var existing = _projects.FirstOrDefault(p => p.Id == project.Id);
                if (existing == null)
                {
                    RaiseLogAdded(LogEntry.Error($"Project '{project.Id}' not found"));
                    return Task.FromResult(false);
                }

                existing.Name = project.Name;
                existing.Description = project.Description;
            }

            RaiseProjectUpdated(project);
            RaiseLogAdded(LogEntry.Info($"Project '{project.Name}' updated"));
            return Task.FromResult(true);
        }

        public Task<bool> DeleteProjectAsync(Guid projectId)
        {
            Project? project;
            lock (_lockObject)
            {
                project = _projects.FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    RaiseLogAdded(LogEntry.Warning($"Project '{projectId}' not found"));
                    return Task.FromResult(false);
                }

                // Delete all devices under this project
                _devices.RemoveAll(d => d.ProjectId == projectId);
                
                // Delete all gateways under this project
                _gateways.RemoveAll(g => g.ProjectId == projectId);
                
                // Delete the project
                _projects.Remove(project);
            }

            RaiseProjectRemoved(project);
            RaiseLogAdded(LogEntry.Success($"Project '{project.Name}' and all contents deleted"));
            return Task.FromResult(true);
        }

        #endregion

        #region Gateway Operations

        public Task<IEnumerable<Gateway>> GetGatewaysByProjectAsync(Guid projectId)
        {
            lock (_lockObject)
            {
                var gateways = _gateways.Where(g => g.ProjectId == projectId).ToList();
                foreach (var gw in gateways)
                {
                    gw.Devices.Clear();
                    var devices = _devices.Where(d => d.GatewayId == gw.Id);
                    foreach (var dev in devices)
                    {
                        gw.Devices.Add(dev);
                    }
                }
                return Task.FromResult<IEnumerable<Gateway>>(gateways);
            }
        }

        public Task<Gateway?> GetGatewayByIdAsync(Guid gatewayId)
        {
            lock (_lockObject)
            {
                return Task.FromResult(_gateways.FirstOrDefault(g => g.Id == gatewayId));
            }
        }

        public Task<bool> AddGatewayAsync(Gateway gateway)
        {
            if (gateway == null)
            {
                RaiseLogAdded(LogEntry.Error("Cannot add null gateway"));
                return Task.FromResult(false);
            }

            lock (_lockObject)
            {
                if (!_projects.Any(p => p.Id == gateway.ProjectId))
                {
                    RaiseLogAdded(LogEntry.Error($"Project '{gateway.ProjectId}' not found"));
                    return Task.FromResult(false);
                }

                _gateways.Add(gateway);
            }

            RaiseGatewayAdded(gateway);
            RaiseLogAdded(LogEntry.Success($"Gateway '{gateway.Name}' added"));
            return Task.FromResult(true);
        }

        public Task<bool> UpdateGatewayAsync(Gateway gateway)
        {
            if (gateway == null) return Task.FromResult(false);

            lock (_lockObject)
            {
                var existing = _gateways.FirstOrDefault(g => g.Id == gateway.Id);
                if (existing == null)
                {
                    RaiseLogAdded(LogEntry.Error($"Gateway '{gateway.Id}' not found"));
                    return Task.FromResult(false);
                }

                existing.Name = gateway.Name;
                existing.GatewayEUI = gateway.GatewayEUI;
                existing.Location = gateway.Location;
                existing.FrequencyBand = gateway.FrequencyBand;
                existing.LastSeen = DateTime.Now;
            }

            RaiseGatewayUpdated(gateway);
            RaiseLogAdded(LogEntry.Info($"Gateway '{gateway.Name}' updated"));
            return Task.FromResult(true);
        }

        public Task<bool> DeleteGatewayAsync(Guid gatewayId)
        {
            Gateway? gateway;
            lock (_lockObject)
            {
                gateway = _gateways.FirstOrDefault(g => g.Id == gatewayId);
                if (gateway == null)
                {
                    RaiseLogAdded(LogEntry.Warning($"Gateway '{gatewayId}' not found"));
                    return Task.FromResult(false);
                }

                // Delete all devices under this gateway
                _devices.RemoveAll(d => d.GatewayId == gatewayId);
                _gateways.Remove(gateway);
            }

            RaiseGatewayRemoved(gateway);
            RaiseLogAdded(LogEntry.Success($"Gateway '{gateway.Name}' and all devices deleted"));
            return Task.FromResult(true);
        }

        public async Task<bool> ToggleGatewayStatusAsync(Guid gatewayId)
        {
            Gateway? gateway;
            lock (_lockObject)
            {
                gateway = _gateways.FirstOrDefault(g => g.Id == gatewayId);
            }

            if (gateway == null)
            {
                RaiseLogAdded(LogEntry.Error($"Gateway '{gatewayId}' not found"));
                return false;
            }

            RaiseLogAdded(LogEntry.Info($"Toggling gateway '{gateway.Name}'..."));
            await Task.Delay(_random.Next(MIN_CONNECTION_DELAY_MS, MAX_CONNECTION_DELAY_MS));

            gateway.Status = gateway.Status == GatewayStatus.Online 
                ? GatewayStatus.Offline 
                : GatewayStatus.Online;
            gateway.LastSeen = DateTime.Now;

            RaiseGatewayUpdated(gateway);
            RaiseLogAdded(LogEntry.Success($"Gateway '{gateway.Name}' is now {gateway.Status}"));
            return true;
        }

        #endregion

        #region Device Operations

        public Task<IEnumerable<Device>> GetDevicesByProjectAsync(Guid projectId)
        {
            lock (_lockObject)
            {
                return Task.FromResult<IEnumerable<Device>>(
                    _devices.Where(d => d.ProjectId == projectId).ToList());
            }
        }

        public Task<IEnumerable<Device>> GetDirectDevicesByProjectAsync(Guid projectId)
        {
            lock (_lockObject)
            {
                return Task.FromResult<IEnumerable<Device>>(
                    _devices.Where(d => d.ProjectId == projectId && d.GatewayId == null).ToList());
            }
        }

        public Task<IEnumerable<Device>> GetDevicesByGatewayAsync(Guid gatewayId)
        {
            lock (_lockObject)
            {
                return Task.FromResult<IEnumerable<Device>>(
                    _devices.Where(d => d.GatewayId == gatewayId).ToList());
            }
        }

        public Task<Device?> GetDeviceByIdAsync(Guid deviceId)
        {
            lock (_lockObject)
            {
                return Task.FromResult(_devices.FirstOrDefault(d => d.Id == deviceId));
            }
        }

        public Task<bool> AddDeviceAsync(Device device)
        {
            if (device == null)
            {
                RaiseLogAdded(LogEntry.Error("Cannot add null device"));
                return Task.FromResult(false);
            }

            lock (_lockObject)
            {
                if (!_projects.Any(p => p.Id == device.ProjectId))
                {
                    RaiseLogAdded(LogEntry.Error($"Project '{device.ProjectId}' not found"));
                    return Task.FromResult(false);
                }

                if (device.GatewayId.HasValue && !_gateways.Any(g => g.Id == device.GatewayId.Value))
                {
                    RaiseLogAdded(LogEntry.Error($"Gateway '{device.GatewayId}' not found"));
                    return Task.FromResult(false);
                }

                _devices.Add(device);
            }

            RaiseDeviceAdded(device);
            RaiseLogAdded(LogEntry.Success($"Device '{device.Name}' added"));
            return Task.FromResult(true);
        }

        public Task<bool> UpdateDeviceAsync(Device device)
        {
            if (device == null) return Task.FromResult(false);

            lock (_lockObject)
            {
                var existing = _devices.FirstOrDefault(d => d.Id == device.Id);
                if (existing == null)
                {
                    RaiseLogAdded(LogEntry.Error($"Device '{device.Id}' not found"));
                    return Task.FromResult(false);
                }

                existing.Name = device.Name;
                existing.Type = device.Type;
                existing.DevEUI = device.DevEUI;
                existing.JoinEUI = device.JoinEUI;
                existing.AppKey = device.AppKey;
                existing.ActivationMode = device.ActivationMode;
                existing.LastUpdated = DateTime.Now;
            }

            RaiseDeviceUpdated(device);
            RaiseLogAdded(LogEntry.Info($"Device '{device.Name}' updated"));
            return Task.FromResult(true);
        }

        public Task<bool> DeleteDeviceAsync(Guid deviceId)
        {
            Device? device;
            lock (_lockObject)
            {
                device = _devices.FirstOrDefault(d => d.Id == deviceId);
                if (device == null)
                {
                    RaiseLogAdded(LogEntry.Warning($"Device '{deviceId}' not found"));
                    return Task.FromResult(false);
                }
                _devices.Remove(device);
            }

            RaiseDeviceRemoved(device);
            RaiseLogAdded(LogEntry.Success($"Device '{device.Name}' deleted"));
            return Task.FromResult(true);
        }

        public async Task<bool> ToggleDeviceStatusAsync(Guid deviceId)
        {
            Device? device;
            lock (_lockObject)
            {
                device = _devices.FirstOrDefault(d => d.Id == deviceId);
            }

            if (device == null)
            {
                RaiseLogAdded(LogEntry.Error($"Device '{deviceId}' not found"));
                return false;
            }

            RaiseLogAdded(LogEntry.Info($"Toggling device '{device.Name}'..."));
            await Task.Delay(_random.Next(MIN_CONNECTION_DELAY_MS, MAX_CONNECTION_DELAY_MS));

            device.Status = device.Status == DeviceStatus.Online 
                ? DeviceStatus.Offline 
                : DeviceStatus.Online;
            
            if (device.Status == DeviceStatus.Offline)
            {
                device.Value = 0;
            }
            
            device.LastUpdated = DateTime.Now;

            RaiseDeviceUpdated(device);
            RaiseLogAdded(LogEntry.Success($"Device '{device.Name}' is now {device.Status}"));
            return true;
        }

        #endregion

        #region Simulation

        public void StartSimulation()
        {
            if (_isSimulationRunning) return;

            _isSimulationRunning = true;
            _simulationTimer.Change(0, SIMULATION_INTERVAL_MS);
            RaiseLogAdded(LogEntry.Success("IoT Simulation STARTED"));
        }

        public void StopSimulation()
        {
            if (!_isSimulationRunning) return;

            _isSimulationRunning = false;
            _simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            RaiseLogAdded(LogEntry.Info("IoT Simulation STOPPED"));
        }

        private void SimulationCallback(object? state)
        {
            if (!_isSimulationRunning) return;

            List<Device> onlineDevices;
            lock (_lockObject)
            {
                onlineDevices = _devices.Where(d => d.Status == DeviceStatus.Online).ToList();
            }

            foreach (var device in onlineDevices)
            {
                UpdateDeviceValue(device);
            }
        }

        private void UpdateDeviceValue(Device device)
        {
            double newValue = device.Type switch
            {
                DeviceType.Temperature => GenerateTemperatureValue(device.Value),
                DeviceType.Vibration => GenerateVibrationValue(device.Value),
                DeviceType.Pressure => GeneratePressureValue(device.Value),
                DeviceType.Humidity => GenerateHumidityValue(device.Value),
                _ => device.Value
            };

            device.Value = Math.Round(newValue, 2);
            device.LastUpdated = DateTime.Now;

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                RaiseDeviceUpdated(device);
            });

            // Random error (2% chance)
            if (_random.NextDouble() < 0.02)
            {
                device.Status = DeviceStatus.Error;
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    RaiseLogAdded(LogEntry.Error($"Connection lost to '{device.Name}'"));
                    RaiseDeviceUpdated(device);
                });
            }
        }

        private double GenerateTemperatureValue(double current) =>
            Math.Clamp(current + (_random.NextDouble() - 0.5) * 2, 15, 45);

        private double GenerateVibrationValue(double current) =>
            Math.Clamp(current + (_random.NextDouble() - 0.5) * 10, 0, 100);

        private double GeneratePressureValue(double current) =>
            Math.Clamp(current + (_random.NextDouble() - 0.5) * 2, 90, 120);

        private double GenerateHumidityValue(double current) =>
            Math.Clamp(current + (_random.NextDouble() - 0.5) * 4, 30, 90);

        #endregion

        #region Event Helpers

        private void RaiseProjectAdded(Project project) =>
            ProjectAdded?.Invoke(this, new ProjectEventArgs(project));

        private void RaiseProjectUpdated(Project project) =>
            ProjectUpdated?.Invoke(this, new ProjectEventArgs(project));

        private void RaiseProjectRemoved(Project project) =>
            ProjectRemoved?.Invoke(this, new ProjectEventArgs(project));

        private void RaiseGatewayAdded(Gateway gateway) =>
            GatewayAdded?.Invoke(this, new GatewayEventArgs(gateway));

        private void RaiseGatewayUpdated(Gateway gateway) =>
            GatewayUpdated?.Invoke(this, new GatewayEventArgs(gateway));

        private void RaiseGatewayRemoved(Gateway gateway) =>
            GatewayRemoved?.Invoke(this, new GatewayEventArgs(gateway));

        private void RaiseDeviceUpdated(Device device) =>
            DeviceUpdated?.Invoke(this, new DeviceEventArgs(device));

        private void RaiseDeviceAdded(Device device) =>
            DeviceAdded?.Invoke(this, new DeviceEventArgs(device));

        private void RaiseDeviceRemoved(Device device) =>
            DeviceRemoved?.Invoke(this, new DeviceEventArgs(device));

        private void RaiseLogAdded(LogEntry log)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.Invoke(() => LogAdded?.Invoke(this, new LogEventArgs(log)));
            }
            else
            {
                LogAdded?.Invoke(this, new LogEventArgs(log));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;
            StopSimulation();
            _simulationTimer.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
