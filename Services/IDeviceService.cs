using SophicIoTManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SophicIoTManager.Services
{
    /// <summary>
    /// Event arguments for project-related events.
    /// </summary>
    public class ProjectEventArgs : EventArgs
    {
        public Project Project { get; }
        public string Message { get; }

        public ProjectEventArgs(Project project, string message = "")
        {
            Project = project;
            Message = message;
        }
    }

    /// <summary>
    /// Event arguments for gateway-related events.
    /// </summary>
    public class GatewayEventArgs : EventArgs
    {
        public Gateway Gateway { get; }
        public string Message { get; }

        public GatewayEventArgs(Gateway gateway, string message = "")
        {
            Gateway = gateway;
            Message = message;
        }
    }

    /// <summary>
    /// Event arguments for device-related events.
    /// </summary>
    public class DeviceEventArgs : EventArgs
    {
        public Device Device { get; }
        public string Message { get; }

        public DeviceEventArgs(Device device, string message = "")
        {
            Device = device;
            Message = message;
        }
    }

    /// <summary>
    /// Event arguments for log-related events.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; }

        public LogEventArgs(LogEntry logEntry)
        {
            LogEntry = logEntry;
        }
    }

    /// <summary>
    /// Interface defining the contract for IoT management services.
    /// Supports hierarchical structure: Project → Gateway → Device
    /// </summary>
    public interface IDeviceService
    {
        #region Events

        /// <summary>
        /// Raised when a project is added.
        /// </summary>
        event EventHandler<ProjectEventArgs>? ProjectAdded;

        /// <summary>
        /// Raised when a project is updated.
        /// </summary>
        event EventHandler<ProjectEventArgs>? ProjectUpdated;

        /// <summary>
        /// Raised when a project is removed.
        /// </summary>
        event EventHandler<ProjectEventArgs>? ProjectRemoved;

        /// <summary>
        /// Raised when a gateway is added.
        /// </summary>
        event EventHandler<GatewayEventArgs>? GatewayAdded;

        /// <summary>
        /// Raised when a gateway is updated.
        /// </summary>
        event EventHandler<GatewayEventArgs>? GatewayUpdated;

        /// <summary>
        /// Raised when a gateway is removed.
        /// </summary>
        event EventHandler<GatewayEventArgs>? GatewayRemoved;

        /// <summary>
        /// Raised when a device's data is updated.
        /// </summary>
        event EventHandler<DeviceEventArgs>? DeviceUpdated;

        /// <summary>
        /// Raised when a device is added.
        /// </summary>
        event EventHandler<DeviceEventArgs>? DeviceAdded;

        /// <summary>
        /// Raised when a device is removed.
        /// </summary>
        event EventHandler<DeviceEventArgs>? DeviceRemoved;

        /// <summary>
        /// Raised when a new log entry is created.
        /// </summary>
        event EventHandler<LogEventArgs>? LogAdded;

        #endregion

        #region Project Operations

        /// <summary>
        /// Retrieves all projects.
        /// </summary>
        Task<IEnumerable<Project>> GetAllProjectsAsync();

        /// <summary>
        /// Retrieves a project by ID.
        /// </summary>
        Task<Project?> GetProjectByIdAsync(Guid projectId);

        /// <summary>
        /// Adds a new project.
        /// </summary>
        Task<bool> AddProjectAsync(Project project);

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        Task<bool> UpdateProjectAsync(Project project);

        /// <summary>
        /// Deletes a project and all its gateways/devices.
        /// </summary>
        Task<bool> DeleteProjectAsync(Guid projectId);

        #endregion

        #region Gateway Operations

        /// <summary>
        /// Retrieves all gateways for a project.
        /// </summary>
        Task<IEnumerable<Gateway>> GetGatewaysByProjectAsync(Guid projectId);

        /// <summary>
        /// Retrieves a gateway by ID.
        /// </summary>
        Task<Gateway?> GetGatewayByIdAsync(Guid gatewayId);

        /// <summary>
        /// Adds a new gateway to a project.
        /// </summary>
        Task<bool> AddGatewayAsync(Gateway gateway);

        /// <summary>
        /// Updates an existing gateway.
        /// </summary>
        Task<bool> UpdateGatewayAsync(Gateway gateway);

        /// <summary>
        /// Deletes a gateway and all its devices.
        /// </summary>
        Task<bool> DeleteGatewayAsync(Guid gatewayId);

        /// <summary>
        /// Toggles gateway connection status.
        /// </summary>
        Task<bool> ToggleGatewayStatusAsync(Guid gatewayId);

        #endregion

        #region Device Operations

        /// <summary>
        /// Retrieves all devices for a project (including those under gateways).
        /// </summary>
        Task<IEnumerable<Device>> GetDevicesByProjectAsync(Guid projectId);

        /// <summary>
        /// Retrieves devices directly under a project (not under a gateway).
        /// </summary>
        Task<IEnumerable<Device>> GetDirectDevicesByProjectAsync(Guid projectId);

        /// <summary>
        /// Retrieves all devices for a gateway.
        /// </summary>
        Task<IEnumerable<Device>> GetDevicesByGatewayAsync(Guid gatewayId);

        /// <summary>
        /// Retrieves a device by ID.
        /// </summary>
        Task<Device?> GetDeviceByIdAsync(Guid deviceId);

        /// <summary>
        /// Adds a new device.
        /// </summary>
        Task<bool> AddDeviceAsync(Device device);

        /// <summary>
        /// Updates an existing device.
        /// </summary>
        Task<bool> UpdateDeviceAsync(Device device);

        /// <summary>
        /// Deletes a device.
        /// </summary>
        Task<bool> DeleteDeviceAsync(Guid deviceId);

        /// <summary>
        /// Toggles device connection status.
        /// </summary>
        Task<bool> ToggleDeviceStatusAsync(Guid deviceId);

        #endregion

        #region Simulation Control

        /// <summary>
        /// Starts the IoT simulation service.
        /// </summary>
        void StartSimulation();

        /// <summary>
        /// Stops the IoT simulation service.
        /// </summary>
        void StopSimulation();

        /// <summary>
        /// Gets whether the simulation is currently running.
        /// </summary>
        bool IsSimulationRunning { get; }

        #endregion
    }
}
