using SophicIoTManager.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SophicIoTManager
{
    /// <summary>
    /// Converts null to Visibility.Visible and non-null to Visibility.Collapsed.
    /// Used to show content when nothing is selected.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts non-null to Visibility.Visible and null to Visibility.Collapsed.
    /// Used to show content when something is selected.
    /// </summary>
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles TreeView selection changed to update the ViewModel's SelectedItem.
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedItem = e.NewValue;
            }
        }

        /// <summary>
        /// Handles TreeViewItem selection to update the ViewModel's SelectedItem.
        /// </summary>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem)
            {
                e.Handled = true;
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SelectedItem = treeViewItem.DataContext;
                }
            }
        }

        /// <summary>
        /// Handles FAB button click to open the context menu.
        /// </summary>
        private void FabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Find the parent Grid that has the ContextMenu
                var parent = button.Parent as System.Windows.Controls.Grid;
                if (parent?.ContextMenu != null)
                {
                    parent.ContextMenu.PlacementTarget = button;
                    parent.ContextMenu.IsOpen = true;
                }
                e.Handled = true;
            }
        }
    }
}
