using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophicIoTManager.Models;
using SophicIoTManager.Services;
using System;
using System.Threading.Tasks;

namespace SophicIoTManager.ViewModels
{
    /// <summary>
    /// Represents the type of item being edited in the modal.
    /// </summary>
    public enum EditItemType
    {
        None,
        Project,
        Gateway,
        Device
    }

    /// <summary>
    /// ViewModel for the floating edit modal window.
    /// Handles editing of Projects, Gateways, and Devices.
    /// </summary>
    public partial class EditModalViewModel : ObservableObject
    {
        #region Fields

        private readonly IDeviceService _deviceService;
        private Guid _editingItemId;

        #endregion

        #region Observable Properties - Modal State

        /// <summary>
        /// Whether the modal is currently open/visible.
        /// </summary>
        [ObservableProperty]
        private bool _isOpen;

        /// <summary>
        /// The type of item being edited.
        /// </summary>
        [ObservableProperty]
        private EditItemType _itemType = EditItemType.None;

        /// <summary>
        /// Title displayed in the modal header.
        /// </summary>
        [ObservableProperty]
        private string _modalTitle = "Edit";

        /// <summary>
        /// Whether a save operation is in progress.
        /// </summary>
        [ObservableProperty]
        private bool _isSaving;

        /// <summary>
        /// Error message to display (if any).
        /// </summary>
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        #endregion

        #region Observable Properties - Project Fields

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private string _projectDescription = string.Empty;

        #endregion

        #region Observable Properties - Gateway Fields

        [ObservableProperty]
        private string _gatewayName = string.Empty;

        [ObservableProperty]
        private string _gatewayEUI = string.Empty;

        [ObservableProperty]
        private string _gatewayLocation = string.Empty;

        [ObservableProperty]
        private string _gatewayFrequencyBand = "EU868";

        #endregion

        #region Observable Properties - Device Fields

        [ObservableProperty]
        private string _deviceName = string.Empty;

        [ObservableProperty]
        private DeviceType _deviceType = DeviceType.Temperature;

        [ObservableProperty]
        private string _devEUI = string.Empty;

        [ObservableProperty]
        private string _joinEUI = string.Empty;

        [ObservableProperty]
        private string _appKey = string.Empty;

        [ObservableProperty]
        private ActivationMode _activationMode = ActivationMode.OTAA;

        #endregion

        #region Computed Properties

        /// <summary>
        /// Available device types for dropdown.
        /// </summary>
        public DeviceType[] DeviceTypes => Enum.GetValues<DeviceType>();

        /// <summary>
        /// Available activation modes for dropdown.
        /// </summary>
        public ActivationMode[] ActivationModes => Enum.GetValues<ActivationMode>();

        /// <summary>
        /// Available frequency bands for dropdown.
        /// </summary>
        public string[] FrequencyBands => new[] { "EU868", "US915", "AS923", "AU915", "CN470", "IN865" };

        /// <summary>
        /// Whether editing a project.
        /// </summary>
        public bool IsEditingProject => ItemType == EditItemType.Project;

        /// <summary>
        /// Whether editing a gateway.
        /// </summary>
        public bool IsEditingGateway => ItemType == EditItemType.Gateway;

        /// <summary>
        /// Whether editing a device.
        /// </summary>
        public bool IsEditingDevice => ItemType == EditItemType.Device;

        #endregion

        #region Constructor

        public EditModalViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        #endregion

        #region Public Methods - Open Modal

        /// <summary>
        /// Opens the modal to edit a project.
        /// </summary>
        public void OpenForProject(ProjectViewModel project)
        {
            if (project == null) return;

            _editingItemId = project.Id;
            ItemType = EditItemType.Project;
            ModalTitle = $"✏️ Edit Project: {project.Name}";

            // Load current values
            ProjectName = project.Name;
            ProjectDescription = project.Description;

            ErrorMessage = string.Empty;
            IsOpen = true;

            OnPropertyChanged(nameof(IsEditingProject));
            OnPropertyChanged(nameof(IsEditingGateway));
            OnPropertyChanged(nameof(IsEditingDevice));
        }

        /// <summary>
        /// Opens the modal to edit a gateway.
        /// </summary>
        public void OpenForGateway(GatewayViewModel gateway)
        {
            if (gateway == null) return;

            _editingItemId = gateway.Id;
            ItemType = EditItemType.Gateway;
            ModalTitle = $"✏️ Edit Gateway: {gateway.Name}";

            // Load current values
            GatewayName = gateway.Name;
            GatewayEUI = gateway.GatewayEUI;
            GatewayLocation = gateway.Location;
            GatewayFrequencyBand = gateway.FrequencyBand;

            ErrorMessage = string.Empty;
            IsOpen = true;

            OnPropertyChanged(nameof(IsEditingProject));
            OnPropertyChanged(nameof(IsEditingGateway));
            OnPropertyChanged(nameof(IsEditingDevice));
        }

        /// <summary>
        /// Opens the modal to edit a device.
        /// </summary>
        public void OpenForDevice(DeviceViewModel device)
        {
            if (device == null) return;

            _editingItemId = device.Id;
            ItemType = EditItemType.Device;
            ModalTitle = $"✏️ Edit Device: {device.Name}";

            // Load current values
            DeviceName = device.Name;
            DeviceType = device.Type;
            DevEUI = device.DevEUI;
            JoinEUI = device.JoinEUI;
            AppKey = device.AppKey;
            ActivationMode = device.ActivationMode;

            ErrorMessage = string.Empty;
            IsOpen = true;

            OnPropertyChanged(nameof(IsEditingProject));
            OnPropertyChanged(nameof(IsEditingGateway));
            OnPropertyChanged(nameof(IsEditingDevice));
        }

        #endregion

        #region Commands

        /// <summary>
        /// Saves the current changes.
        /// </summary>
        [RelayCommand]
        private async Task SaveAsync()
        {
            // Validation
            if (!Validate())
            {
                return;
            }

            IsSaving = true;
            ErrorMessage = string.Empty;

            try
            {
                bool success = ItemType switch
                {
                    EditItemType.Project => await SaveProjectAsync(),
                    EditItemType.Gateway => await SaveGatewayAsync(),
                    EditItemType.Device => await SaveDeviceAsync(),
                    _ => false
                };

                if (success)
                {
                    IsOpen = false;
                }
                else
                {
                    ErrorMessage = "Failed to save changes. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Cancels and closes the modal.
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            IsOpen = false;
            ErrorMessage = string.Empty;
        }

        #endregion

        #region Private Methods

        private bool Validate()
        {
            return ItemType switch
            {
                EditItemType.Project => ValidateProject(),
                EditItemType.Gateway => ValidateGateway(),
                EditItemType.Device => ValidateDevice(),
                _ => false
            };
        }

        private bool ValidateProject()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                ErrorMessage = "Project name is required.";
                return false;
            }
            return true;
        }

        private bool ValidateGateway()
        {
            if (string.IsNullOrWhiteSpace(GatewayName))
            {
                ErrorMessage = "Gateway name is required.";
                return false;
            }
            return true;
        }

        private bool ValidateDevice()
        {
            if (string.IsNullOrWhiteSpace(DeviceName))
            {
                ErrorMessage = "Device name is required.";
                return false;
            }
            return true;
        }

        private async Task<bool> SaveProjectAsync()
        {
            var project = await _deviceService.GetProjectByIdAsync(_editingItemId);
            if (project == null) return false;

            project.Name = ProjectName.Trim();
            project.Description = ProjectDescription.Trim();

            return await _deviceService.UpdateProjectAsync(project);
        }

        private async Task<bool> SaveGatewayAsync()
        {
            var gateway = await _deviceService.GetGatewayByIdAsync(_editingItemId);
            if (gateway == null) return false;

            gateway.Name = GatewayName.Trim();
            gateway.GatewayEUI = GatewayEUI.Trim();
            gateway.Location = GatewayLocation.Trim();
            gateway.FrequencyBand = GatewayFrequencyBand;

            return await _deviceService.UpdateGatewayAsync(gateway);
        }

        private async Task<bool> SaveDeviceAsync()
        {
            var device = await _deviceService.GetDeviceByIdAsync(_editingItemId);
            if (device == null) return false;

            device.Name = DeviceName.Trim();
            device.Type = DeviceType;
            device.DevEUI = DevEUI.Trim();
            device.JoinEUI = JoinEUI.Trim();
            device.AppKey = AppKey.Trim();
            device.ActivationMode = ActivationMode;

            return await _deviceService.UpdateDeviceAsync(device);
        }

        #endregion
    }
}
