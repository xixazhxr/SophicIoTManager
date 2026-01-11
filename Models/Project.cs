using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace SophicIoTManager.Models
{
    /// <summary>
    /// Represents an IoT Project/Application container (similar to TTN Application).
    /// A project can contain multiple gateways and devices.
    /// </summary>
    public partial class Project : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the project.
        /// </summary>
        [ObservableProperty]
        private Guid _id;

        /// <summary>
        /// Human-readable name of the project.
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// Description of the project.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// When the project was created.
        /// </summary>
        [ObservableProperty]
        private DateTime _createdAt;

        /// <summary>
        /// Collection of gateways belonging to this project.
        /// </summary>
        public ObservableCollection<Gateway> Gateways { get; } = new();

        /// <summary>
        /// Collection of devices directly under this project (not under a gateway).
        /// </summary>
        public ObservableCollection<Device> Devices { get; } = new();

        /// <summary>
        /// Gets the total number of devices (direct + under gateways).
        /// </summary>
        public int TotalDeviceCount
        {
            get
            {
                int count = Devices.Count;
                foreach (var gateway in Gateways)
                {
                    count += gateway.Devices.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the number of online devices.
        /// </summary>
        public int OnlineDeviceCount
        {
            get
            {
                int count = 0;
                foreach (var device in Devices)
                {
                    if (device.Status == DeviceStatus.Online) count++;
                }
                foreach (var gateway in Gateways)
                {
                    foreach (var device in gateway.Devices)
                    {
                        if (device.Status == DeviceStatus.Online) count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Creates a new Project with a generated GUID.
        /// </summary>
        public Project()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Creates a new Project with specified properties.
        /// </summary>
        public Project(string name, string description = "") : this()
        {
            Name = name;
            Description = description;
        }
    }
}
