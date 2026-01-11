using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SophicIoTManager.Models
{
    /// <summary>
    /// Represents the operational status of an IoT device.
    /// </summary>
    public enum DeviceStatus
    {
        Online,
        Offline,
        Error,
        Maintenance
    }

    /// <summary>
    /// Represents the type of IoT device sensor.
    /// </summary>
    public enum DeviceType
    {
        Temperature,
        Vibration,
        Pressure,
        Humidity
    }

    /// <summary>
    /// Represents the LoRaWAN activation mode.
    /// </summary>
    public enum ActivationMode
    {
        OTAA,  // Over-The-Air Activation
        ABP    // Activation By Personalization
    }

    /// <summary>
    /// Represents an IoT device entity with observable properties for real-time UI updates.
    /// Includes LoRaWAN-specific fields for TTN/ChirpStack compatibility.
    /// </summary>
    public partial class Device : ObservableObject
    {
        #region Core Properties

        /// <summary>
        /// Unique identifier for the device (internal GUID).
        /// </summary>
        [ObservableProperty]
        private Guid _id;

        /// <summary>
        /// Human-readable name of the device.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// The type of sensor this device represents.
        /// </summary>
        [ObservableProperty]
        private DeviceType _type;

        /// <summary>
        /// Current operational status of the device.
        /// </summary>
        [ObservableProperty]
        private DeviceStatus _status;

        /// <summary>
        /// The last recorded sensor value.
        /// </summary>
        [ObservableProperty]
        private double _value;

        /// <summary>
        /// Timestamp of the last value update.
        /// </summary>
        [ObservableProperty]
        private DateTime _lastUpdated;

        #endregion

        #region LoRaWAN Properties

        /// <summary>
        /// Device EUI (Extended Unique Identifier) - 16 hex characters.
        /// For simulation, any input is allowed.
        /// </summary>
        [ObservableProperty]
        private string _devEUI = string.Empty;

        /// <summary>
        /// Join EUI / Application EUI - 16 hex characters.
        /// For simulation, any input is allowed.
        /// </summary>
        [ObservableProperty]
        private string _joinEUI = string.Empty;

        /// <summary>
        /// Application Key for OTAA activation - 32 hex characters.
        /// For simulation, any input is allowed.
        /// </summary>
        [ObservableProperty]
        private string _appKey = string.Empty;

        /// <summary>
        /// LoRaWAN activation mode (OTAA or ABP).
        /// </summary>
        [ObservableProperty]
        private ActivationMode _activationMode = ActivationMode.OTAA;

        #endregion

        #region Parent References

        /// <summary>
        /// Reference to the parent project (always set).
        /// </summary>
        [ObservableProperty]
        private Guid _projectId;

        /// <summary>
        /// Reference to the parent gateway (null if device is directly under project).
        /// </summary>
        [ObservableProperty]
        private Guid? _gatewayId;

        #endregion

        #region Computed Properties

        /// <summary>
        /// Indicates whether the device is currently online.
        /// </summary>
        public bool IsOnline => Status == DeviceStatus.Online;

        /// <summary>
        /// Gets the display unit based on device type.
        /// </summary>
        public string Unit => Type switch
        {
            DeviceType.Temperature => "°C",
            DeviceType.Vibration => "Hz",
            DeviceType.Pressure => "PSI",
            DeviceType.Humidity => "%",
            _ => ""
        };

        /// <summary>
        /// Gets a formatted string representation of the current value with unit.
        /// </summary>
        public string FormattedValue => $"{Value:F2} {Unit}";

        /// <summary>
        /// Gets a formatted display string for the device type.
        /// </summary>
        public string TypeDisplay => Type switch
        {
            DeviceType.Temperature => "🌡️ Temperature",
            DeviceType.Vibration => "📳 Vibration",
            DeviceType.Pressure => "⚙️ Pressure",
            DeviceType.Humidity => "💧 Humidity",
            _ => Type.ToString()
        };

        /// <summary>
        /// Gets a formatted display string for the device status.
        /// </summary>
        public string StatusDisplay => Status switch
        {
            DeviceStatus.Online => "● Online",
            DeviceStatus.Offline => "○ Offline",
            DeviceStatus.Error => "⚠ Error",
            DeviceStatus.Maintenance => "🔧 Maintenance",
            _ => Status.ToString()
        };

        /// <summary>
        /// Gets the color for the status indicator.
        /// </summary>
        public string StatusColor => Status switch
        {
            DeviceStatus.Online => "#27AE60",
            DeviceStatus.Offline => "#95A5A6",
            DeviceStatus.Error => "#E74C3C",
            DeviceStatus.Maintenance => "#F39C12",
            _ => "#95A5A6"
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Device with a generated GUID and random LoRaWAN identifiers.
        /// </summary>
        public Device()
        {
            Id = Guid.NewGuid();
            LastUpdated = DateTime.Now;
            DevEUI = GenerateRandomEUI();
            JoinEUI = GenerateRandomEUI();
            AppKey = GenerateRandomAppKey();
        }

        /// <summary>
        /// Creates a new Device with specified properties.
        /// </summary>
        public Device(string name, DeviceType type, Guid projectId, Guid? gatewayId = null, DeviceStatus status = DeviceStatus.Offline)
            : this()
        {
            Name = name;
            Type = type;
            Status = status;
            ProjectId = projectId;
            GatewayId = gatewayId;
        }

        /// <summary>
        /// Legacy constructor for backwards compatibility.
        /// </summary>
        public Device(string name, DeviceType type, DeviceStatus status = DeviceStatus.Offline)
            : this()
        {
            Name = name;
            Type = type;
            Status = status;
        }

        #endregion

        #region Helper Methods

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

        /// <summary>
        /// Generates a random AppKey for simulation purposes.
        /// </summary>
        private static string GenerateRandomAppKey()
        {
            var random = new Random();
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        #endregion
    }
}
