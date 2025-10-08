using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.Views.Shared;

public partial class ListServiceBox : UserControl
{
    public ListServiceBox()
    {
        InitializeComponent();
    }

    private void ServiceItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.ServiceItem_PreviewMouseLeftButtonDown(sender, e);
        }
    }

    private void ServiceItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.ServiceItem_PreviewMouseMove(sender, e);
        }
    }

    private void ServiceItem_Drop(object sender, DragEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.ServiceItem_Drop(sender, e);
        }
    }

    private void RenameServiceMenu_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.RenameServiceMenu_Click(sender, e);
        }
    }

    private void DeleteServiceMenu_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.DeleteServiceMenu_Click(sender, e);
        }
    }

    private void ChangeColorMenu_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainView mainView)
        {
            mainView.ChangeColorMenu_Click(sender, e);
        }
    }
}
