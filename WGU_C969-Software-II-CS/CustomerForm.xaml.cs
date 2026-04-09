using System.Globalization;
using System.Resources;
using System.Windows;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : Window
{
    public CustomerForm(int customerId, string currentUsername, CultureInfo currentCulture)
    {
        InitializeComponent();

        CultureInfo culture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = culture;
        testLabel.Content = WGU_C969_Software_II_CS.Resources.Localization.test;
    }
}