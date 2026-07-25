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
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM customer;", connection);
        int nextId = Convert.ToInt32(command.ExecuteScalar());
        
        CustomerForm newCustomer = new CustomerForm(1, this.CurrentUsername)
        {
            Owner = this
        };
        newCustomer.ShowDialog();
    }
    private void NewAppointmentClicked(object sender, RoutedEventArgs e)
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM appointment;", connection);
        int nextId = Convert.ToInt32(command.ExecuteScalar());
        
        AppointmentForm newAppointment = new AppointmentForm(1, this.CurrentUsername, 1)
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

