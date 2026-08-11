using System.Collections.ObjectModel;
using System.ComponentModel;
using MySqlConnector;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace WGU_C969_Software_II_CS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    public delegate void PropertyChangedDelegate(string name);

    private string CurrentUsername { get; init; }
    private int ID { get; init; } = -1;
    
    private bool CustomerMod
    {
        set
        {
            if (value)
            {
                this.CustomerModButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerModifyButton;
            }
            else if (!value)
            {
                this.CustomerModButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerAddButton;
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
                Console.WriteLine("Value greater than Customers");
                return;
            }

            if (value == -1)
            {
                SelectedCustomerId = -1;
            }
            else if (value != -1)
            {
                SelectedCustomerId = this.Customers[value].ID;
            }
            this.LoadAppointmentDisplay();
            Console.WriteLine($"CustomerSelectedIndex Changed {value}");
            OnPropertyChanged(nameof(CustomerSelectedIndex));
        }
    }
    
    private AdvancedList<CustomerForm> Customers { get; set; }
    
    private bool AppointmentMod
    {
        set
        {
            if (value)
            {
                this.AppointmentModButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentModifyButton;
            }
            else if (!value)
            {
                this.AppointmentModButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentAddButton;
            }
        } 
    }
    
    private int SelectedAppointmentId = -1;
    public int AppointmentSelectedIndex
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (this.Appointments == null)
            {
                return -1;
            }
            return this.Appointments.FindIndex(c => c.ID == SelectedAppointmentId);
        }
        set
        {
            if (value != null && value != -1)
            {
                AppointmentMod = true;
            }
            else
            {
                AppointmentMod = false;
            }
            
            int i = value;
            if (value > this.Appointments.Count)
            {
                Console.WriteLine("Value greater than Appointments");
                return;
            }

            if (value == -1)
            {
                SelectedAppointmentId = -1;
            }
            else if (value != -1)
            {
                SelectedAppointmentId = this.Appointments[value].ID;
            }
            Console.WriteLine($"SelectedAppointmentId Changed {value}");
            OnPropertyChanged(nameof(AppointmentSelectedIndex));
        }
    }
    
    private AdvancedList<AppointmentForm> Appointments { get; set; }
    
    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            this.LoadAppointmentDisplay();
            OnPropertyChanged(nameof(SelectedDate));
            Console.WriteLine($"Selected Date Change: {value}");
        }
    }

    private void LoadCustomerDisplay()
    {
        ObservableCollection<CustomerForm> customers = new ObservableCollection<CustomerForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    customers.Add(new CustomerForm(int.Parse(reader["customerId"].ToString()), this.CurrentUsername));
                }
            }
        }
        this.CustomerListView.ItemsSource = customers;
    }
    
    private void LoadAppointmentDisplay()
    {
        /*BindingExpression? customerListBinding = this.CustomerListView.GetBindingExpression(Selector.SelectedIndexProperty);
        BindingExpression? selectDateBinding = this.DatePickCalendar.GetBindingExpression(Selector.SelectedIndexProperty);
        if (customerListBinding != null)
        {
            Validation.ClearInvalid(customerListBinding);
        }
        else
        {
            throw new Exception("Failed to bind CustomerListView");
        }
        if (selectDateBinding != null)
        {
            Validation.ClearInvalid(selectDateBinding);
        }
        else
        {
            throw new Exception("Failed to bind DatePickCalendar");
        }
        
        if (this.SelectedCustomerId == null || this.SelectedCustomerId == -1 || !this.SelectedDate.HasValue)
        {
            
            Validation.MarkInvalid(customerListBinding,
                new ValidationError(new ExceptionValidationRule(), customerListBinding));

            Validation.MarkInvalid(selectDateBinding,
                new ValidationError(new ExceptionValidationRule(), customerListBinding));
            
            return;
        }*/
        if ((this.SelectedCustomerId == null || this.SelectedCustomerId == -1) && !this.SelectedDate.HasValue)
        {
            return;
        }

        int? customerId = null;
        if (this.SelectedCustomerId != -1)
        {
            customerId  = this.SelectedCustomerId;
        }

        ObservableCollection<DisplayAppointment> appointments = new ObservableCollection<DisplayAppointment>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand(@"
SELECT 
    appointment.appointmentId, 
    appointment.customerId, 
    appointment.userId, 
    appointment.start,
    appointment.title,
    appointment.description,
    customer.customerName
FROM appointment
JOIN customer ON appointment.customerId = customer.customerId
WHERE (@date IS NULL OR DATE(appointment.start) = @date)
  AND (@customerId IS NULL OR appointment.customerId = @customerId);", connection))
            {
                command.Parameters.AddWithValue("@date", this.SelectedDate);
                command.Parameters.AddWithValue("@customerId", customerId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime start = DateTime.Parse(reader["start"].ToString());
                    string detail = "";
                    if (this.SelectedCustomerId == -1 && this.SelectedDate.HasValue)
                    {
                        detail = reader["customerName"].ToString();
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerNameDetail;
                    }
                    else if (this.SelectedCustomerId != -1 && !this.SelectedDate.HasValue)
                    {
                        detail = DateOnly.FromDateTime(start).ToString();
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.DateDetail;
                    }
                    else if (this.SelectedCustomerId != -1 && this.SelectedDate.HasValue)
                    {
                        detail = reader["description"].ToString();
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.DescriptionDetail;
                    }
                    
                    appointments.Add(new DisplayAppointment()
                    {
                        ID = int.Parse(reader["appointmentId"].ToString()),
                        CustomerId =  int.Parse(reader["customerId"].ToString()),
                        UserId = int.Parse(reader["userId"].ToString()),
                        StartTime = start.TimeOfDay,
                        AppointmentTitle = reader["title"].ToString(),
                        Detail = detail
                    });
                }
            }
        }
        this.AppointmentListView.ItemsSource = appointments;
    }
    
    public MainWindow()
    {
        if (this.ID == -1)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.DialogResult != true)
            {
                this.Close();
                return;
            }
            this.ID = loginWindow.ID;
            this.CurrentUsername = loginWindow.Username;
        }
        
        this.DataContext = this;
        InitializeComponent();
        
        MainWindow.CheckCreation(this.CurrentUsername);

        this.Customers = new AdvancedList<CustomerForm>(this.LoadCustomerDisplay);
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
        this.CustomerMod = false;
        
        
        this.Appointments = new AdvancedList<AppointmentForm>(this.LoadAppointmentDisplay);
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT appointmentId, customerId, userId FROM appointment", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Appointments.Add(new AppointmentForm(int.Parse(reader["appointmentId"].ToString()),
                        this.CurrentUsername, int.Parse(reader["customerId"].ToString()),
                        int.Parse(reader["userId"].ToString())));
                }
            }
        }
        this.AppointmentMod = false;
        
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand(@"
SELECT 
    appointment.appointmentId, 
    appointment.customerId, 
    appointment.userId, 
    appointment.start,
    appointment.title,
    appointment.description,
    customer.customerName
FROM appointment
JOIN customer ON appointment.customerId = customer.customerId
WHERE appointment.start BETWEEN NOW() and DATE_ADD(NOW(), INTERVAL 15 MINUTE);", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string appointmentTitle = reader.GetString("title");
                    string customerName = reader.GetString("customerName");
                    DateTime appointmentStartTime = reader.GetDateTime("start");
                    MessageBox.Show($"{WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentGeneric} {appointmentTitle} {WGU_C969_Software_II_CS.Resources.MainWindow.PossessiveGeneric} {customerName} {WGU_C969_Software_II_CS.Resources.MainWindow.StartsIn}! ({appointmentStartTime.TimeOfDay})");
                }
            }
        }
        
        this.CustomerIDColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.ItemIDColumn;
        this.CustomerFirstNameColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerFirstNameColumn;
        this.CustomerLastNameColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerLastNameColumn;
        this.CustomerDeleteButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerDeleteButton;
        this.CustomerClearButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.ClearSelectButton;
        
        this.DateClearButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.ClearSelectButton;
        
        this.AppointmentIDColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.ItemIDColumn;
        this.AppointmentStartTimeColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentStartTimeColumn;
        this.AppointmentTitleColumn.Header = WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentTitleColumn;
        this.AppointmentDeleteButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.AppointmentDeleteButton;
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
        
        this.Customers = new AdvancedList<CustomerForm>(this.LoadCustomerDisplay);
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
                $"{WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirm} {selectedItem.FirstName} {selectedItem.LastName} (ID: {selectedItem.ID}) {WGU_C969_Software_II_CS.Resources.MainWindow.CustomerEntry}?",
                WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirm,
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
        this.CustomerSelectedIndex = -1;
        this.CustomerListView.SelectedIndex = -1;
    }
    
    private void DateClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.DatePickCalendar.SelectedDate = null;
    }
    
    private void AppointmentModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.AppointmentSelectedIndex == -1)
        {
            BindingExpression? customerListBinding = CustomerListView.GetBindingExpression(Selector.SelectedIndexProperty);
            if (customerListBinding != null)
            {
                if (CustomerListView.SelectedIndex < 0)
                {
                    Validation.MarkInvalid(customerListBinding,
                        new ValidationError(new ExceptionValidationRule(), customerListBinding));
                    return;
                }
                else
                {
                    Validation.ClearInvalid(customerListBinding);
                }
            }
            else
            {
                throw new Exception("Failed to bind CustomerListView");
            }
            
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(appointmentId), 0) + 1 FROM appointment;", connection);
            
            AppointmentForm newCustomer = new AppointmentForm(int.Parse(command.ExecuteScalar().ToString()), this.CurrentUsername, this.SelectedCustomerId, this.ID)
            {
                Owner = this
            };
            newCustomer.ShowDialog();
        }
        
        List<AppointmentForm> moddedAppointments = new List<AppointmentForm>();
        foreach (DisplayAppointment selectedItem in this.AppointmentListView.SelectedItems)
        {
            AppointmentForm newAppointment = new AppointmentForm(selectedItem.ID, this.CurrentUsername, selectedItem.CustomerId, selectedItem.UserId)
            {
                Owner = this
            };
            newAppointment.ShowDialog();
        }
        
        this.Appointments = new AdvancedList<AppointmentForm>(this.LoadAppointmentDisplay);
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT appointmentId, customerId, userId FROM appointment", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Appointments.Add(new AppointmentForm(int.Parse(reader["appointmentId"].ToString()), this.CurrentUsername, int.Parse(reader["customerId"].ToString()), int.Parse(reader["userId"].ToString())));
                }
            }
        }
    }
    
    private void AppointmentDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        foreach (AppointmentForm selectedItem in AppointmentListView.SelectedItems)
        {
            string customerName = "";
            using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand command =
                       new MySqlCommand($"SELECT customerName FROM customer WHERE customerId = @customerId", connection))
                {
                    command.Parameters.AddWithValue("@customerId", selectedItem.ID);
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        customerName = reader["customerName"].ToString();
                    }
                }
            }
            
            MessageBoxResult result = MessageBox.Show(
                $"{WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirm} {selectedItem.Title} {WGU_C969_Software_II_CS.Resources.MainWindow.PossessiveGeneric} {customerName} (ID: {selectedItem.ID}) {WGU_C969_Software_II_CS.Resources.MainWindow.CustomerEntry}?",
                WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirmTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
                connection.Open();
                using MySqlCommand command = new MySqlCommand("DELETE FROM appointment WHERE appointmentId = @id", connection);
                command.Parameters.AddWithValue("@id", selectedItem.ID);
                command.ExecuteNonQuery();
                this.Appointments.Remove(selectedItem);
            }
        }
    }
    
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var listView = (ListView)sender;

        if (listView.View is not GridView gridView || gridView.Columns.Count < 3)
        {
            return;
        }
        
        // subtract a bit for the vertical scrollbar / border so columns don't wrap
        double width = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;

        if (width > 0)
        {
            double colWidth = (width - 13) / (gridView.Columns.Count-1);
            for (int i = 1; i < gridView.Columns.Count; i++)
            {
                gridView.Columns[i].Width = colWidth;
                gridView.Columns[i].Width = colWidth; 
            }
        }
    }

    public static readonly MySqlConnectionStringBuilder ConnectionBuilder = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        UserID = "sqlUser",
        Password = "Passw0rd!",
        Database = "client_schedule"
    }; 

    private static void CheckCreation(string currentUsername)
    {
        using (MySqlConnection testconn = new  MySqlConnection("Server=localhost;User ID=sqlUser;Password=Passw0rd!;"))
        {
            bool databaseExists = false;
            
            Console.WriteLine("Connecting to server...");
            testconn.Open();
            Console.WriteLine("Connected");

            using (MySqlCommand command = new MySqlCommand("SELECT VERSION();", testconn))
            {
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"MySQL Server Version: {reader.GetString(0)}");
                }
            }
            
            using (MySqlCommand command = new MySqlCommand("Show DATABASES", testconn))
            {
                using var reader = command.ExecuteReader();
            
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
            
                using (MySqlCommand command = new MySqlCommand("CREATE DATABASE client_schedule;", testconn))
                {
                    command.ExecuteNonQuery();
                }

                using (MySqlCommand command = new MySqlCommand("SHOW DATABASES", testconn))
                {
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine($"Database Names: {reader.GetString(0)}");
                    }
                }
            }
        }
        
        
        using MySqlConnection connection = new MySqlConnection(ConnectionBuilder.ConnectionString);
        connection.Open();
        string[] databaseTables;
        using (MySqlCommand command = new MySqlCommand("SHOW TABLES", connection))
        {
            using var reader = command.ExecuteReader();
            
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
            using (MySqlCommand command = new MySqlCommand(
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
            
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM  country", connection))
            {
                using var reader = command.ExecuteReader();
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
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand command = new MySqlCommand(
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
            using (MySqlCommand cmd = new MySqlCommand(
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

public class DisplayAppointment
{
    public int ID { get; init; }
    public int CustomerId { get; init; } 
    public int UserId { get; init; } 
    public TimeSpan StartTime { get; init; }
    public string AppointmentTitle { get; init; }
    public string Detail { get; init; }
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

