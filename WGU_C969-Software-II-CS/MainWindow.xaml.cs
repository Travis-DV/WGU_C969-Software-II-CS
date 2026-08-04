using System.Collections.ObjectModel;
using System.ComponentModel;
using MySqlConnector;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace WGU_C969_Software_II_CS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    public delegate void PropertyChangedDelegate(string name);

    private string CurrentUsername = "Admin";
    
    private bool CustomerMod
    {
        set
        {
            if (value)
            {
                this.CustomerModButton.Content = "mod"; //TODO Change to correct local
            }
            else if (!value)
            {
                this.CustomerModButton.Content = "add"; //TODO Change to correct local
            }
        } 
    }
    
    private int SelectedCustomerId = -1;
    public int CustomerSelectedIndex
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (this.Customers == null)
            {
                return -1;
            }
            return this.Customers.FindIndex(c => c.ID == SelectedCustomerId);
        }
        set
        {
            if (value != null && value != -1)
            {
                CustomerMod = true;
            }
            else
            {
                CustomerMod = false;
            }
            
            int i = value;
            if (value > this.Customers.Count)
            {
                Console.WriteLine("Value greater than countries");
                return;
            }
            SelectedCustomerId = this.Customers[value].ID;
            OnPropertyChanged(nameof(CustomerSelectedIndex));
        }
    }
    
    private AdvancedList<CustomerForm> Customers { get; set; }

    private void LoadCustomerNames()
    {
        ObservableCollection<CustomerForm> Customers = new ObservableCollection<CustomerForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM  customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Customers.Add(new CustomerForm(int.Parse(reader["customerId"].ToString()), this.CurrentUsername));
                }
            }
        }
        this.CustomerListView.ItemsSource = Customers;
    }
    
    public MainWindow()
    {
        this.DataContext = this;
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

        this.Customers = new AdvancedList<CustomerForm>(this.LoadCustomerNames);
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM  customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Customers.Add(new CustomerForm(int.Parse(reader["customerId"].ToString()), this.CurrentUsername));
                }
            }
        }
        //this.LoadCustomerNames();
        this.CustomerMod = false;
    }
    
    private void NewAppointmentClicked(object sender, RoutedEventArgs e)
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM appointment;", connection);
        int nextId = Convert.ToInt32(command.ExecuteScalar());
        
        AppointmentForm newAppointment = new AppointmentForm(nextId, this.CurrentUsername, 1, 0)
        {
            Owner = this
        };
        newAppointment.ShowDialog();
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
    
    private void CustomerModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.CustomerSelectedIndex == -1)
        {
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM customer;", connection);
            
            CustomerForm newCustomer = new CustomerForm(int.Parse(command.ExecuteScalar().ToString()), this.CurrentUsername)
            {
                Owner = this
            };
            newCustomer.ShowDialog();
        }
        
        List<CustomerForm> moddedCustomers = new List<CustomerForm>();
        foreach (CustomerForm selectedItem in this.CustomerListView.SelectedItems)
        {
            CustomerForm newCustomer = new CustomerForm(selectedItem.ID, this.CurrentUsername)
            {
                Owner = this
            };
            newCustomer.ShowDialog();
        }
        
        this.Customers = new AdvancedList<CustomerForm>(this.LoadCustomerNames);
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM  customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Customers.Add(new CustomerForm(int.Parse(reader["customerId"].ToString()), this.CurrentUsername));
                }
            }
        }
        //this.LoadCustomerNames();
    }
    
    private void CustomerDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        foreach (CustomerForm selectedItem in CustomerListView.SelectedItems)
        {
            
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete {selectedItem.FirstName} {selectedItem.LastName} (ID: {selectedItem.ID}) customer entry?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
                connection.Open();
                using MySqlCommand command = new MySqlCommand("DELETE FROM customer WHERE customerId = @id", connection);
                command.Parameters.AddWithValue("@id", selectedItem.ID);
                command.ExecuteNonQuery();
                this.Customers.Remove(selectedItem);
            }
        }
    }
    
    private void CustomerClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.CustomerListView.SelectedIndex = -1;
    }
    
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var listView = (ListView)sender;

        // subtract a bit for the vertical scrollbar / border so columns don't wrap
        double width = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;

        if (width > 0)
        {
            CustomerFirstNameColumn.Width = (width-26) / 2;
            CustomerLastNameColumn.Width = (width-26) / 2;
        }
    }

    public static readonly MySqlConnectionStringBuilder ConnectionBuilder = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        UserID = "sqlUser",
        Password = "Passw0rd!",
        Database = "client_schedule"
    }; 

    private static async void CheckCreation(string currentUsername)
    {
        await using (MySqlConnection testconn = new  MySqlConnection("Server=localhost;User ID=sqlUser;Password=Passw0rd!;"))
        {
            bool databaseExists = false;
            
            Console.WriteLine("Connecting to server...");
            await testconn.OpenAsync();
            Console.WriteLine("Connected");

            await using (MySqlCommand command = new MySqlCommand("SELECT VERSION();", testconn))
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"MySQL Server Version: {reader.GetString(0)}");
                }
            }
            
            await using (MySqlCommand command = new MySqlCommand("Show DATABASES", testconn))
            {
                await using var reader = await command.ExecuteReaderAsync();
            
                string databaseListString = "";
                while (reader.Read())
                {
                    Console.WriteLine($"Database Names: {reader.GetString(0)}");
                    databaseListString += $"{reader.GetString(0)},";
                }
                string[] databaseList = databaseListString.Split(',');
                if (databaseList.Contains("client_schedule"))
                {
                    Console.WriteLine("Database Exists");
                    databaseExists = true;
                }
            }

            if (!databaseExists)
            {
                Console.WriteLine("Creating database...");
            
                await using (MySqlCommand command = new MySqlCommand("CREATE DATABASE client_schedule;", testconn))
                {
                    command.ExecuteNonQuery();
                }

                await using (MySqlCommand command = new MySqlCommand("SHOW DATABASES", testconn))
                {
                    await using var reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        Console.WriteLine($"Database Names: {reader.GetString(0)}");
                    }
                }
            }
        }
        
        
        await using MySqlConnection connection = new MySqlConnection(ConnectionBuilder.ConnectionString);
        await connection.OpenAsync();
        string[] databaseTables;
        await using (MySqlCommand command = new MySqlCommand("SHOW TABLES", connection))
        {
            await using var reader = await command.ExecuteReaderAsync();
            
            string databaseTablesString = "";
            while (reader.Read())
            {
                Console.WriteLine($"TABLE Names: {reader.GetString(0)}");
                databaseTablesString += $"{reader.GetString(0)},";
            }
            databaseTables = databaseTablesString.Split(',');
        }
        
        
        if (!databaseTables.Contains("country"))
        {
            Console.WriteLine("Generating country Table");
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                        CREATE TABLE country 
                       (
                           countryId INTEGER PRIMARY KEY, 
                           country VARCHAR(50), 
                           createDate DATETIME, 
                           createdBy VARCHAR(40),
                           lastUpdate TIMESTAMP, 
                           lastUpdateBy VARCHAR(40)
                       )", 
                             connection)
                        )
            {
                command.ExecuteNonQuery();
            }
            
            await using (MySqlCommand command = new MySqlCommand(
                                     @"
                                        INSERT INTO country 
                                            (countryId, country, createDate, createdBy, lastUpdate, lastUpdateBy)
                                        VALUES 
                                            (@countryId, @country, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)",
                                     connection)
                                 )
            {
                command.Parameters.AddWithValue("@countryId", "0");
                command.Parameters.AddWithValue("@country", "USA");
                command.Parameters.AddWithValue("@createDate", DateTime.Now);
                command.Parameters.AddWithValue("@createdBy", currentUsername);
                command.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                command.Parameters.AddWithValue("@lastUpdateBy", currentUsername);
    
                command.ExecuteNonQuery();
            }
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                                        INSERT INTO country 
                                            (countryId, country, createDate, createdBy, lastUpdate, lastUpdateBy)
                                        VALUES 
                                            (@countryId, @country, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)",
                             connection)
                        )
            {
                command.Parameters.AddWithValue("@countryId", "1");
                command.Parameters.AddWithValue("@country", "Spain");
                command.Parameters.AddWithValue("@createDate", DateTime.Now);
                command.Parameters.AddWithValue("@createdBy", currentUsername);
                command.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                command.Parameters.AddWithValue("@lastUpdateBy", currentUsername);
    
                command.ExecuteNonQuery();
            }
            await using (MySqlCommand command = new MySqlCommand("SELECT * FROM  country", connection))
            {
                await using var reader = await command.ExecuteReaderAsync();
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
        }
        
        if (!databaseTables.Contains("city"))
        {
            Console.WriteLine("Generating city Table");
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                            CREATE TABLE city 
                            (
                                cityId INTEGER PRIMARY KEY, 
                                city VARCHAR(50), 
                                countryId INTEGER,
                                createDate DATETIME, 
                                createdBy VARCHAR(40),
                                lastUpdate TIMESTAMP, 
                                lastUpdateBy VARCHAR(40),
                                FOREIGN KEY (countryId) REFERENCES country(countryId) 
                            )", 
                             connection)
                        )
            {
                command.ExecuteNonQuery();
            }
        }    
        
        if (!databaseTables.Contains("address"))
        {
            Console.WriteLine("Generating address Table");
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                            CREATE TABLE address 
                            (
                                addressId INTEGER PRIMARY KEY, 
                                address VARCHAR(50), 
                                address2 VARCHAR(50),
                                cityId INTEGER,
                                postalCode VARCHAR(10),
                                phone VARCHAR(20),
                                createDate DATETIME, 
                                createdBy VARCHAR(40),
                                lastUpdate TIMESTAMP, 
                                lastUpdateBy VARCHAR(40),
                                FOREIGN KEY (cityId) REFERENCES city(cityId) 
                            )", 
                             connection)
                        )
            {
                command.ExecuteNonQuery();
            }
        }
            
        if (!databaseTables.Contains("customer"))
        {
            Console.WriteLine("Generating customer Table");
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                            CREATE TABLE customer 
                            (
                                customerId INTEGER PRIMARY KEY, 
                                customerName VARCHAR(45), 
                                addressId INTEGER,
                                active TINYINT(1), 
                                createDate DATETIME, 
                                createdBy VARCHAR(40),
                                lastUpdate TIMESTAMP, 
                                lastUpdateBy VARCHAR(40),
                                FOREIGN KEY (addressId) REFERENCES address(addressId) 
                            )", 
                             connection)
                        )
            {
                command.ExecuteNonQuery();
            }
        }
            
        if (!databaseTables.Contains("user"))
        {
            Console.WriteLine("Generating user Table");
            await using (MySqlCommand command = new MySqlCommand(
                             @"
                            CREATE TABLE user 
                            (
                                userId INTEGER PRIMARY KEY, 
                                userName VARCHAR(50), 
                                password VARCHAR(50),
                                active TINYINT(1), 
                                createDate DATETIME, 
                                createdBy VARCHAR(40),
                                lastUpdate TIMESTAMP, 
                                lastUpdateBy VARCHAR(40)
                            )", 
                             connection)
                        )
            {
                command.ExecuteNonQuery();
            }
        }
            
        if (!databaseTables.Contains("appointment"))
        {
            Console.WriteLine("Generating appointment Table");
            await using (MySqlCommand cmd = new MySqlCommand(
                             @"
                            CREATE TABLE appointment 
                            (
                                appointmentId INTEGER PRIMARY KEY, 
                                customerId INTEGER,
                                userId INTEGER,
                                title VARCHAR(255), 
                                description TEXT,
                                location TEXT,
                                contact TEXT,
                                type TEXT,
                                url VARCHAR(255),
                                start DATETIME,
                                end DATETIME,
                                createDate DATETIME, 
                                createdBy VARCHAR(40),
                                lastUpdate TIMESTAMP, 
                                lastUpdateBy VARCHAR(40),
                                FOREIGN KEY (customerId) REFERENCES customer(customerId),
                                FOREIGN KEY (userId) REFERENCES user(userId) 
                            )", 
                             connection)
                        )
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class BasicTextValidator : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cuture)
    {
        if (value == null)
        {
            return new ValidationResult(false, "Value is null");
        }
        if (value.ToString() is { Length: 0 })
        {
            return new ValidationResult(false, "Required");
        }

        return ValidationResult.ValidResult;
    }
}

