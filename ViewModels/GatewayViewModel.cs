using CommunityToolkit.Mvvm.ComponentModel;
using SophicIoTManager.Models;
using System;
using System.Collections.ObjectModel;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for Gateway model.
    /// Provides tree structure with child devices.
    /// </summary>
    public partial class GatewayViewModel : ObservableObject
    {
        #region Fields

        private readonly Gateway _gateway;

        #endregion

        #region Properties

        public Gateway Model => _gateway;
        public Guid Id => _gateway.Id;
        public Guid ProjectId => _gateway.ProjectId;

        public string Name
        {
            get => _gateway.Name;
            set
            {
                if (_gateway.Name != value)
                {
                    _gateway.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string GatewayEUI
        {
            get => _gateway.GatewayEUI;
            set
            {
                if (_gateway.GatewayEUI != value)
                {
                    _gateway.GatewayEUI = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Location
        {
            get => _gateway.Location;
            set
            {
                if (_gateway.Location != value)
                {
                    _gateway.Location = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FrequencyBand
        {
            get => _gateway.FrequencyBand;
            set
            {
                if (_gateway.FrequencyBand != value)
                {
                    _gateway.FrequencyBand = value;
                    OnPropertyChanged();
                }
            }
        }

        public GatewayStatus Status
        {
            get => _gateway.Status;
            set
            {
                if (_gateway.Status != value)
                {
                    _gateway.Status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(IsOnline));
                }
            }
        }

        public DateTime LastSeen => _gateway.LastSeen;
        public bool IsOnline => _gateway.IsOnline;

        /// <summary>
        /// Child devices for TreeView display.
        /// </summary>
        public ObservableCollection<DeviceViewModel> Devices { get; } = new();

        /// <summary>
        /// Combined children for TreeView binding.
        /// </summary>
        public ObservableCollection<object> Children => new(Devices);

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

        public string Icon => "📡";
        public string DisplayName => Name;
        public string StatusDisplay => _gateway.StatusDisplay;
        public string StatusColor => _gateway.StatusColor;
        public string DeviceCountDisplay => $"{Devices.Count} Device(s)";
        public string LastSeenDisplay => LastSeen.ToString("HH:mm:ss");

        #endregion

        #region Constructor

        public GatewayViewModel(Gateway gateway)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        #endregion

        #region Methods

        public void RefreshChildren()
        {
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(DeviceCountDisplay));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(GatewayEUI));
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(FrequencyBand));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(IsOnline));
            OnPropertyChanged(nameof(LastSeenDisplay));
            OnPropertyChanged(nameof(DeviceCountDisplay));
        }

        #endregion
    }
}
