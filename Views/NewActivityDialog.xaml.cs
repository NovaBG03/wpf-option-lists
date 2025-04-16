using System.Windows;

namespace WpfApp.Views;

public partial class NewItemDialog : Window
{
    public string ItemName { get; private set; } = string.Empty;

    public NewItemDialog(string title = "Add New Item")
    {
        InitializeComponent();
        Title = title;
        NameTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ItemName = NameTextBox.Text.Trim();
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter a name.", "Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameTextBox.Focus();
        }
    }
}
