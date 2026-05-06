using System.Data.SQLite;
using System.Globalization;
using System.Windows;

namespace WGU_C969_Software_II_CS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        if (CultureInfo.CurrentCulture.Name.Contains("en"))
        {
            this.LanguagesComboBoxItemEn.IsSelected = true;
        }
        else if (CultureInfo.CurrentCulture.Name.Contains("es"))
        {
            this.LanguagesComboBoxItemEs.IsSelected = true; 
        }
        
        DatabaseAPI.CheckCreation();
    }

    private void NewCustomerClicked(object sender, RoutedEventArgs e)
    {
        int nextId = 0;
        using (SQLiteConnection conn = new SQLiteConnection(DatabaseAPI.connectionString))
        using (SQLiteCommand cmd = new SQLiteCommand("SELECT IFNULL(MAX(cityId), 0) + 1 FROM city;", conn))
        {
            conn.Open();
            nextId = Convert.ToInt32(cmd.ExecuteScalar());
        }
        CustomerForm newCustomer = new CustomerForm(nextId, "Test")
        {
            Owner = this
        };
        newCustomer.Show();
    }
    
    private void LanguageSelectedChanged(object sender, RoutedEventArgs e)
    {
        
        CultureInfo culture;
        if (this.LanguagesComboBox.SelectedItem.Equals(this.LanguagesComboBoxItemEs))
        {
            culture = new CultureInfo("es-ES");
        } 
        else
        {
            culture = new CultureInfo("en-US");
        } 
        
        Thread.CurrentThread.CurrentUICulture = culture;
        
    }
}

