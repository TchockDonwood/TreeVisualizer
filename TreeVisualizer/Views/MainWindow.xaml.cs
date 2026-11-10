using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TreeVisualizer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        DataContext = vm;

        vm.Logs.CollectionChanged += Logs_CollectionChanged;
    }

    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            LogScrollViewer.ScrollToEnd();
    }

    private void LeftNull_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.Text == "NULL" && tb.DataContext is VisualNode vNode && DataContext is MainViewModel vm && !vm.IsRunning)
        {
            vm.AddNode(vNode.Model, true);
            e.Handled = true;
        }
    }

    private void RightNull_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.Text == "NULL" && tb.DataContext is VisualNode vNode && DataContext is MainViewModel vm && !vm.IsRunning)
        {
            vm.AddNode(vNode.Model, false);
            e.Handled = true;
        }
    }

    private void NodeValue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is VisualNode vNode && DataContext is MainViewModel vm && !vm.IsRunning)
        {
            if (vNode.IsLeaf)
            {
                vm.DeleteNode(vNode.Model);
                e.Handled = true;
            }
        }
    }
}