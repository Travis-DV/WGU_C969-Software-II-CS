using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Windows;

namespace WGU_C969_Software_II_CS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public delegate void PropertyChangedDelegate(string name);

    private string CurrentUsername = "Admin";
    
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
        
        MainWindow.CheckCreation(this.CurrentUsername);
    }

    private void NewCustomerClicked(object sender, RoutedEventArgs e)
    {
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using SQLiteCommand cmd = new SQLiteCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM customer;", conn);
        int nextId = Convert.ToInt32(cmd.ExecuteScalar());
        
        CustomerForm newCustomer = new CustomerForm(1, this.CurrentUsername)
        {
            Owner = this
        };
        newCustomer.ShowDialog();
    }
    
    private void LanguageSelectedChanged(object sender, RoutedEventArgs e)
    {
        
        CultureInfo culture;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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
    
    private static readonly string DbPath = $"{Directory.GetCurrentDirectory()}\\SchedulingSoftwareDatabase.db";
    public static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

    private static void CheckCreation(string currentUsername)
    {
        
        Console.WriteLine(Directory.GetCurrentDirectory());
        
        if (!File.Exists(DbPath))
        {
            SQLiteConnection.CreateFile(DbPath);

            using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
            conn.Open();
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                        CREATE TABLE country 
                                       (
                                           countryId INTEGER PRIMARY KEY, 
                                           country VARCHAR(50), 
                                           createDate VARCHAR(20), 
                                           createdBy VARCHAR(40),
                                           lastUpdate VARCHAR(20), 
                                           lastUpdateBy VARCHAR(40)
                                       )", conn))
            {
                cmd.ExecuteNonQuery();
            }
                
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                    INSERT INTO country 
                        (country, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@country, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)", conn))
            {
                cmd.Parameters.AddWithValue("@country", "USA");
                cmd.Parameters.AddWithValue("@createDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@createdBy", currentUsername);
                cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@lastUpdateBy", currentUsername);

                cmd.ExecuteNonQuery();
                    
                cmd.Parameters.AddWithValue("@country", "Spain");
                cmd.Parameters.AddWithValue("@createDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@createdBy", currentUsername);
                cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@lastUpdateBy", currentUsername);

                cmd.ExecuteNonQuery();
            }
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM  country", conn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(
                        $@"ID: {reader["countryId"]}, " +
                        $@"Country: {reader["country"]}, " +
                        $@"Created: {reader["createDate"]}, " +
                        $@"By: {reader["createdBy"]}, " +
                        $@"Updated: {reader["lastUpdate"]}, " +
                        $@"By: {reader["lastUpdateBy"]}"
                    );
                }
            }
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE address 
                                    (
                                        addressId INTEGER PRIMARY KEY, 
                                        address VARCHAR(50), 
                                        address2 VARCHAR(50),
                                        cityId INTEGER,
                                        postalCode VARCHAR(10),
                                        phone VARCHAR(20),
                                        createDate VARCHAR(20), 
                                        createdBy VARCHAR(40),
                                        lastUpdate VARCHAR(20), 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (cityId) REFERENCES city(cityId) 
                                    )", conn))
            {
                cmd.ExecuteNonQuery();
            }
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE city 
                                    (
                                        cityId INTEGER PRIMARY KEY, 
                                        city VARCHAR(50), 
                                        countryId INTEGER,
                                        createDate VARCHAR(20), 
                                        createdBy VARCHAR(40),
                                        lastUpdate VARCHAR(20), 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (countryId) REFERENCES country(countryId) 
                                    )", conn))
            {
                cmd.ExecuteNonQuery();
            }
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE customer 
                                    (
                                        customerId INTEGER PRIMARY KEY, 
                                        customerName VARCHAR(50), 
                                        addressId INTEGER,
                                        phoneNumber VARCHAR(20),
                                        active SMALLINT(1), 
                                        createDate VARCHAR(20), 
                                        createdBy VARCHAR(40),
                                        lastUpdate VARCHAR(20), 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (addressId) REFERENCES address(addressId) 
                                    )", conn))
            {
                cmd.ExecuteNonQuery();
            }
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE user 
                                    (
                                        userId INTEGER PRIMARY KEY, 
                                        userName VARCHAR(50), 
                                        password VARCHAR(50),
                                        active SMALLINT(1), 
                                        createDate VARCHAR(20), 
                                        createdBy VARCHAR(40),
                                        lastUpdate VARCHAR(20), 
                                        lastUpdateBy VARCHAR(40)
                                    )", conn))
            {
                cmd.ExecuteNonQuery();
            }
                
            using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE appointment 
                                    (
                                        appointmentId INTEGER PRIMARY KEY, 
                                        customerId INTEGER,
                                        userId INTEGER,
                                        title VARCHAR(50), 
                                        description TEXT,
                                        location TEXT,
                                        contact TEXT,
                                        type TEXT,
                                        url VARCHAR(255),
                                        start DATE,
                                        end DATE,
                                        createDate VARCHAR(20), 
                                        createdBy VARCHAR(40),
                                        lastUpdate VARCHAR(20), 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (customerId) REFERENCES customer(customerId),
                                        FOREIGN KEY (userId) REFERENCES user(userId) 
                                    )", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        else
        {
            // ReSharper disable once LocalizableElement
            Console.WriteLine("Exists");
        }

        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            // ReSharper disable once LocalizableElement
            Console.WriteLine("Connected");
            
            string query = "SELECT name FROM sqlite_master WHERE type='table'";

            using (var cmd = new SQLiteCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // ReSharper disable once LocalizableElement
                    Console.WriteLine("Table: " + reader["name"]);
                }
            }
            using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}

