using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophicIoTManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for Project model.
    /// Provides tree structure with child gateways and devices.
    /// </summary>
    public partial class ProjectViewModel : ObservableObject
    {
        #region Fields

        private readonly Project _project;

        #endregion

        #region Properties

        public Project Model => _project;
        public Guid Id => _project.Id;

        public string Name
        {
            get => _project.Name;
            set
            {
                if (_project.Name != value)
                {
                    _project.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _project.Description;
            set
            {
                if (_project.Description != value)
                {
                    _project.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CreatedAt => _project.CreatedAt;

        /// <summary>
        /// Child gateways for TreeView display.
        /// </summary>
        public ObservableCollection<GatewayViewModel> Gateways { get; } = new();

        /// <summary>
        /// Devices directly under the project (no gateway).
        /// </summary>
        public ObservableCollection<DeviceViewModel> Devices { get; } = new();

        /// <summary>
        /// All devices in this project (from gateways + direct).
        /// Used for binding in dashboard.
        /// </summary>
        public IEnumerable<DeviceViewModel> AllDevices
        {
            get
            {
                foreach (var dev in Devices)
                    yield return dev;
                foreach (var gw in Gateways)
                    foreach (var dev in gw.Devices)
                        yield return dev;
            }
        }

        /// <summary>
        /// Combined children for TreeView (Gateways + Direct Devices).
        /// </summary>
        public ObservableCollection<object> Children { get; } = new();

        /// <summary>
        /// Whether the node is expanded in the TreeView.
        /// </summary>
        [ObservableProperty]
        private bool _isExpanded = true;

        /// <summary>
        /// Whether the node is selected in the TreeView.
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        #endregion

        #region Display Properties

        public string Icon => "📁";
        public string DisplayName => Name;
        public string GatewayCountDisplay => $"{Gateways.Count} Gateway(s)";
        public string DeviceCountDisplay => $"{GetTotalDeviceCount()} Device(s)";
        public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd");

        #endregion

        #region Constructor

        public ProjectViewModel(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
        }

        #endregion

        #region Methods

        public int GetTotalDeviceCount()
        {
            int count = Devices.Count;
            foreach (var gw in Gateways)
            {
                count += gw.Devices.Count;
            }
            return count;
        }

        public int GetOnlineDeviceCount()
        {
            int count = 0;
            foreach (var dev in Devices)
            {
                if (dev.IsOnline) count++;
            }
            foreach (var gw in Gateways)
            {
                foreach (var dev in gw.Devices)
                {
                    if (dev.IsOnline) count++;
                }
            }
            return count;
        }

        public void RefreshChildren()
        {
            Children.Clear();
            foreach (var gw in Gateways)
            {
                Children.Add(gw);
            }
            foreach (var dev in Devices)
            {
                Children.Add(dev);
            }
            OnPropertyChanged(nameof(GatewayCountDisplay));
            OnPropertyChanged(nameof(DeviceCountDisplay));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(GatewayCountDisplay));
            OnPropertyChanged(nameof(DeviceCountDisplay));
        }

        #endregion
    }
}
