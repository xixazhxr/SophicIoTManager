using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SophicIoTManager.Views
{
    /// <summary>
    /// Converts a non-empty string to Visibility.Visible.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }
    }

    /// <summary>
    /// Code-behind for EditModal.xaml
    /// </summary>
    public partial class EditModal : UserControl
    {
        public EditModal()
        {
            // Add converters to resources
            Resources.Add("StringToVisConverter", new StringToVisibilityConverter());
            Resources.Add("InverseBoolConverter", new InverseBoolConverter());
            
            InitializeComponent();
        }

        /// <summary>
        /// Closes modal when clicking on the dark overlay background.
        /// </summary>
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Optional: Close on overlay click
            // Uncomment below if you want clicking outside to close the modal
            // if (DataContext is ViewModels.EditModalViewModel vm)
            // {
            //     vm.CancelCommand.Execute(null);
            // }
        }
    }
}
