using System.Windows;

namespace TreeVisualizer.Views;

public partial class InputDialog : Window
{
    public string Result { get; private set; } = string.Empty;

    public InputDialog(string prompt)
    {
        InitializeComponent();
        PromptText.Text = prompt;
        Loaded += (s, e) => ValueTextBox.Focus();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Result = ValueTextBox.Text;
        DialogResult = true;
    }
}