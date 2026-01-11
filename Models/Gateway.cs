using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace SophicIoTManager.Models
{
    /// <summary>
    /// Represents the operational status of a gateway.
    /// </summary>
    public enum GatewayStatus
    {
        Online,
        Offline,
        Error,
        Maintenance
    }

    /// <summary>
    /// Represents a LoRaWAN Gateway that connects devices to the network.
    /// A gateway belongs to exactly one project and can have multiple devices.
    /// </summary>
    public partial class Gateway : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the gateway.
        /// </summary>
        [ObservableProperty]
        private Guid _id;

        /// <summary>
        /// Gateway EUI (Extended Unique Identifier) - 16 hex characters.
        /// For simulation, any input is allowed.
        /// </summary>
        [ObservableProperty]
        private string _gatewayEUI = string.Empty;

        /// <summary>
        /// Human-readable name of the gateway.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Physical location of the gateway (address, coordinates, or description).
        /// </summary>
        [ObservableProperty]
        private string _location = string.Empty;

        /// <summary>
        /// LoRaWAN frequency band (e.g., EU868, US915, AS923).
        /// </summary>
        [ObservableProperty]
        private string _frequencyBand = "EU868";

        /// <summary>
        /// Current operational status of the gateway.
        /// </summary>
        [ObservableProperty]
        private GatewayStatus _status;

        /// <summary>
        /// Reference to the parent project.
        /// </summary>
        [ObservableProperty]
        private Guid _projectId;

        /// <summary>
        /// Timestamp of the last status update.
        /// </summary>
        [ObservableProperty]
        private DateTime _lastSeen;

        /// <summary>
        /// Collection of devices connected through this gateway.
        /// </summary>
        public ObservableCollection<Device> Devices { get; } = new();

        /// <summary>
        /// Gets whether the gateway is currently online.
        /// </summary>
        public bool IsOnline => Status == GatewayStatus.Online;

        /// <summary>
        /// Gets a formatted display string for the gateway status.
        /// </summary>
        public string StatusDisplay => Status switch
        {
            GatewayStatus.Online => "● Online",
            GatewayStatus.Offline => "○ Offline",
            GatewayStatus.Error => "⚠ Error",
            GatewayStatus.Maintenance => "🔧 Maintenance",
            _ => Status.ToString()
        };

        /// <summary>
        /// Gets the color for the status indicator.
        /// </summary>
        public string StatusColor => Status switch
        {
            GatewayStatus.Online => "#27AE60",
            GatewayStatus.Offline => "#95A5A6",
            GatewayStatus.Error => "#E74C3C",
            GatewayStatus.Maintenance => "#F39C12",
            _ => "#95A5A6"
        };

        /// <summary>
        /// Creates a new Gateway with a generated GUID.
        /// </summary>
        public Gateway()
        {
            Id = Guid.NewGuid();
            LastSeen = DateTime.Now;
            Status = GatewayStatus.Offline;
        }

        /// <summary>
        /// Creates a new Gateway with specified properties.
        /// </summary>
        public Gateway(string name, Guid projectId, string gatewayEUI = "") : this()
        {
            Name = name;
            ProjectId = projectId;
            GatewayEUI = string.IsNullOrEmpty(gatewayEUI) 
                ? GenerateRandomEUI() 
                : gatewayEUI;
        }

        /// <summary>
        /// Generates a random EUI for simulation purposes.
        /// </summary>
        private static string GenerateRandomEUI()
        {
            var random = new Random();
            var bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
