using CommunityToolkit.Mvvm.ComponentModel;
using SophicIoTManager.Models;
using System;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for Device model.
    /// Provides UI-specific formatting and LoRaWAN field bindings.
    /// </summary>
    public partial class DeviceViewModel : ObservableObject
    {
        #region Fields

        private readonly Device _device;

        #endregion

        #region Core Properties

        public Device Model => _device;
        public Guid Id => _device.Id;
        public Guid ProjectId => _device.ProjectId;
        public Guid? GatewayId => _device.GatewayId;

        public string Name
        {
            get => _device.Name;
            set
            {
                if (_device.Name != value)
                {
                    _device.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public DeviceType Type
        {
            get => _device.Type;
            set
            {
                if (_device.Type != value)
                {
                    _device.Type = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TypeDisplay));
                }
            }
        }

        public DeviceStatus Status
        {
            get => _device.Status;
            set
            {
                if (_device.Status != value)
                {
                    _device.Status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(IsOnline));
                    OnPropertyChanged(nameof(ValueDisplay));
                }
            }
        }

        public double Value
        {
            get => _device.Value;
            set
            {
                if (Math.Abs(_device.Value - value) > 0.001)
                {
                    _device.Value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ValueDisplay));
                }
            }
        }

        public DateTime LastUpdated
        {
            get => _device.LastUpdated;
            set
            {
                if (_device.LastUpdated != value)
                {
                    _device.LastUpdated = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastUpdatedDisplay));
                }
            }
        }

        public bool IsOnline => _device.IsOnline;

        #endregion

        #region LoRaWAN Properties

        public string DevEUI
        {
            get => _device.DevEUI;
            set
            {
                if (_device.DevEUI != value)
                {
                    _device.DevEUI = value;
                    OnPropertyChanged();
                }
            }
        }

        public string JoinEUI
        {
            get => _device.JoinEUI;
            set
            {
                if (_device.JoinEUI != value)
                {
                    _device.JoinEUI = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AppKey
        {
            get => _device.AppKey;
            set
            {
                if (_device.AppKey != value)
                {
                    _device.AppKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public ActivationMode ActivationMode
        {
            get => _device.ActivationMode;
            set
            {
                if (_device.ActivationMode != value)
                {
                    _device.ActivationMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActivationModeDisplay));
                }
            }
        }

        #endregion

        #region Tree Properties

        /// <summary>
        /// Whether the node is expanded in the TreeView (devices have no children).
        /// </summary>
        [ObservableProperty]
        private bool _isExpanded;

        /// <summary>
        /// Whether the node is selected in the TreeView.
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        #endregion

        #region Display Properties

        public string Icon => "📱";
        public string DisplayName => Name;
        public string TypeDisplay => _device.TypeDisplay;
        public string StatusDisplay => _device.StatusDisplay;
        public string StatusColor => _device.StatusColor;
        public string ValueDisplay => IsOnline ? _device.FormattedValue : "---";
        public string LastUpdatedDisplay => LastUpdated.ToString("HH:mm:ss");
        public string ActivationModeDisplay => ActivationMode.ToString();

        /// <summary>
        /// Available activation modes for dropdown.
        /// </summary>
        public ActivationMode[] ActivationModes => Enum.GetValues<ActivationMode>();

        /// <summary>
        /// Available device types for dropdown.
        /// </summary>
        public DeviceType[] DeviceTypes => Enum.GetValues<DeviceType>();

        #endregion

        #region Constructor

        public DeviceViewModel(Device device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes all display properties.
        /// Call this when the underlying model has been updated externally.
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(TypeDisplay));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ValueDisplay));
            OnPropertyChanged(nameof(LastUpdated));
            OnPropertyChanged(nameof(LastUpdatedDisplay));
            OnPropertyChanged(nameof(IsOnline));
            OnPropertyChanged(nameof(DevEUI));
            OnPropertyChanged(nameof(JoinEUI));
            OnPropertyChanged(nameof(AppKey));
            OnPropertyChanged(nameof(ActivationMode));
            OnPropertyChanged(nameof(ActivationModeDisplay));
        }

        #endregion
    }
}
