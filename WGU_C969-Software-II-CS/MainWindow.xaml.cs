using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WGU_C969_Software_II_CS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NewCustomerClicked(object sender, RoutedEventArgs e)
    {
        CultureInfo culture = new CultureInfo("es-ES");
        
        CustomerForm newCustomer = new CustomerForm(0, "Test", culture);
        newCustomer.Owner = this;
        newCustomer.Show();
    }
}