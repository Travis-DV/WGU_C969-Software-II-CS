using System.Collections.ObjectModel;
using System.ComponentModel;
using MySqlConnector;
using System.Globalization;
using System.IO;
using System.Reflection;
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
    
    private CustomerForm _selectedCustomer;
    public CustomerForm SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (value != null)
            {
                CustomerMod = true;
            }
            else if (value == null)
            {
                CustomerMod = false;
            }

            _selectedCustomer = value;
            
            this.LoadAppointmentDisplay();
            Console.WriteLine($"SelectedCustomer Changed {value}");
            OnPropertyChanged(nameof(SelectedCustomer));
        }
    }
    
    private List<CustomerForm> Customers { get; set; }
    
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
    
    private DisplayAppointment _selectedAppointment;
    public DisplayAppointment SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (value != null)
            {
                AppointmentMod = true;
            }
            else
            {
                AppointmentMod = false;
            }

            _selectedAppointment = value;
            
            Console.WriteLine($"SelectedAppointment Changed {value}");
            OnPropertyChanged(nameof(SelectedAppointment));
        }
    }
    
    private List<AppointmentForm> Appointments { get; set; }
    
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
        List<CustomerForm> customers = new List<CustomerForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    customers.Add(new CustomerForm(reader.GetInt16("customerId"), this.CurrentUsername));
                }
            }
        }

        customers = customers.OrderBy(customer => customer.ID).ToList();
        this.CustomerListView.ItemsSource = customers;
    }
    
    private void LoadAppointmentDisplay()
    {
        List<DisplayAppointment> appointments = new List<DisplayAppointment>();
        
        if (this.SelectedCustomer == null && !this.SelectedDate.HasValue)
        {
            this.AppointmentListView.ItemsSource = appointments;
            return;
        }

        int? customerId = null;
        if (this.SelectedCustomer != null)
        {
            customerId = this.SelectedCustomer.ID;
        }
        
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
                    DateTime start = reader.GetDateTime("start");
                    string detail = "";
                    if (customerId == null && this.SelectedDate.HasValue)
                    {
                        detail = reader.GetString("customerName");
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.CustomerNameDetail;
                    }
                    else if (customerId != null && !this.SelectedDate.HasValue)
                    {
                        detail = DateOnly.FromDateTime(start).ToString();
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.DateDetail;
                    }
                    else if (customerId != null && this.SelectedDate.HasValue)
                    {
                        detail = reader.GetString("description");
                        this.Detail.Header = WGU_C969_Software_II_CS.Resources.MainWindow.DescriptionDetail;
                    }
                    
                    appointments.Add(new DisplayAppointment()
                    {
                        ID = reader.GetInt16("appointmentId"),
                        CustomerId =  reader.GetInt16("customerId"),
                        UserId = reader.GetInt16("userId"),
                        StartTime = start.ToLocalTime().TimeOfDay,
                        AppointmentTitle = reader.GetString("title"),
                        Detail = detail
                    });
                }
            }
        }

        appointments = appointments.OrderBy(appointment => appointment.ID).ToList();
        
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

        /*this.ID = 0;
        this.CurrentUsername = "Admin";*/
        
        this.DataContext = this;
        InitializeComponent();
        
        MainWindow.CheckCreation(this.CurrentUsername);

        this.Customers = new List<CustomerForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM  customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Customers.Add(new CustomerForm(reader.GetInt16("customerId"), this.CurrentUsername));
                }
            }
        }
        this.CustomerMod = false;
        
        
        this.Appointments = new List<AppointmentForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT appointmentId, customerId, userId FROM appointment", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Appointments.Add(new AppointmentForm(reader.GetInt16("appointmentId"),
                        this.CurrentUsername, reader.GetInt16("customerId"),
                        reader.GetInt16("userId")));
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
        
        this.LoadCustomerDisplay();
        this.LoadAppointmentDisplay();

        this.ReportComboBox.ItemsSource = new List<string>
        {
            WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_TypesPerMonth, //Number of appointment types per-month
            WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_UserSchedules, //All user schedules 
            WGU_C969_Software_II_CS.Resources.MainWindow.ReportComboBox_AverageAppointments //Average Number of appointments per-month
        };

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

        this.ReportButton.Content = WGU_C969_Software_II_CS.Resources.MainWindow.GenerateReportButton;
    }
    
    private void CustomerModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.SelectedCustomer == null)
        {
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(customerId), 0) + 1 FROM customer;", connection);
            
            CustomerForm newCustomer = new CustomerForm(int.Parse(command.ExecuteScalar().ToString()), this.CurrentUsername);
            newCustomer.ShowDialog();
        }
        
        List<CustomerForm> moddedCustomers = new List<CustomerForm>();
        foreach (CustomerForm selectedItem in this.CustomerListView.SelectedItems)
        {
            CustomerForm newCustomer = new CustomerForm(selectedItem.ID, this.CurrentUsername);
            newCustomer.ShowDialog();
        }
        
        this.Customers = new List<CustomerForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT customerId FROM  customer", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Customers.Add(new CustomerForm(reader.GetInt16("customerId"), this.CurrentUsername));
                }
            }
        }
        
        LoadCustomerDisplay();
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

        LoadCustomerDisplay();
    }
    
    private void CustomerClearButtonClicked(object sender, RoutedEventArgs e)
    {
        //this.SelectedCustomer = null;
        this.CustomerListView.SelectedIndex = -1;
        Console.WriteLine(this.SelectedCustomer);
    }
    
    private void DateClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.DatePickCalendar.SelectedDate = null;
    }
    
    private void AppointmentModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.SelectedAppointment == null)
        {
            BindingExpression? customerListBinding = CustomerListView.GetBindingExpression(Selector.SelectedItemProperty);
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
            
            AppointmentForm newAppointment = new AppointmentForm(int.Parse(command.ExecuteScalar().ToString()), this.CurrentUsername, this.SelectedCustomer.ID, this.ID);
            newAppointment.ShowDialog();
        }
        
        List<AppointmentForm> moddedAppointments = new List<AppointmentForm>();
        foreach (DisplayAppointment selectedItem in this.AppointmentListView.SelectedItems)
        {
            AppointmentForm newAppointment = new AppointmentForm(selectedItem.ID, this.CurrentUsername, selectedItem.CustomerId, selectedItem.UserId);
            newAppointment.ShowDialog();
        }
        
        this.Appointments = new List<AppointmentForm>();
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT appointmentId, customerId, userId FROM appointment", connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    this.Appointments.Add(new AppointmentForm(reader.GetInt16("appointmentId"), this.CurrentUsername, reader.GetInt16("customerId"), reader.GetInt16("userId")));
                }
            }
        }
        
        this.LoadAppointmentDisplay();
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
                        customerName = reader.GetString("customerName");
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
        this.LoadAppointmentDisplay();
    }
    
    private void ReportButtonClicked(object sender, RoutedEventArgs e)
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        
        
        if ((string)ReportComboBox.SelectedItem == WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_TypesPerMonth) //Number of appointment types per-month
        {
            List<(string month, int count)> outputs = new List<(string month, int count)>();
            
            using (MySqlCommand command = new MySqlCommand(@"
SELECT
    DATE_FORMAT(start, '%Y-%m') AS month,
    COUNT(DISTINCT type) AS total
FROM appointment
GROUP BY month
ORDER BY month;", connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        outputs.Add((reader.GetString("month"), reader.GetInt32("total"))); 
                    }
                }
            }
            
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"TypesPerMonth.csv");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                File.AppendAllText(filePath, $"Month, Total" + Environment.NewLine);
            }
            Console.WriteLine(filePath);
            
            outputs.ForEach(output => File.AppendAllText(filePath, $"{output.month}, {output.count}" + Environment.NewLine));

            MessageBox.Show($"{WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_SuccessfullySaved}: {filePath}");
        }
        else if ((string)ReportComboBox.SelectedItem == WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_UserSchedules) //All user schedules
        {
            List<(string name, string title, DateTime start, DateTime end)> outputs = new List<(string name, string title, DateTime start, DateTime end)>();
            
            using (MySqlCommand command = new MySqlCommand(@"
SELECT
    customer.customerName AS Name,
    appointment.title AS Title,
    appointment.start AS Start,
    appointment.end AS End
FROM customer
INNER JOIN appointment
    ON customer.customerId = appointment.customerId
GROUP BY customer.customerName, appointment.title, appointment.start, appointment.end
ORDER BY Name, Start
", connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        outputs.Add(
                            (
                                reader.GetString("Name"), 
                                reader.GetString("Title"), 
                                reader.GetDateTime("Start"),
                                reader.GetDateTime("End")
                            )
                        ); 
                    }
                }
            }
            
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"ClientSchedule.csv");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                File.AppendAllText(filePath, $"ClientName, Title, Start, End" + Environment.NewLine);
            }
            Console.WriteLine(filePath);
            
            outputs.ForEach(output => File.AppendAllText(filePath, $"{output.name}, {output.title}, {output.start}, {output.end}" + Environment.NewLine));

            MessageBox.Show($"{WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_SuccessfullySaved}: {filePath}");
        }
        else if ((string)ReportComboBox.SelectedItem == WGU_C969_Software_II_CS.Resources.MainWindow.ReportComboBox_AverageAppointments) //Average Number of appointments per-month
        {
            List<int> outputs = new List<int>();
            
            using (MySqlCommand command = new MySqlCommand(@"
SELECT AVG(monthly_count) AS avg_appointments_per_month
FROM (
    SELECT
        DATE_FORMAT(start, '%Y-%m') AS month,
        COUNT(*) AS monthly_count
    FROM appointment
    GROUP BY month
    ORDER BY month
) AS monthly_totals;", connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        outputs.Add(reader.GetInt32("avg_appointments_per_month")); 
                    }
                }
            }
            
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"AverageAppointments.csv");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                File.AppendAllText(filePath, $"Average Per Month" + Environment.NewLine);
            }
            Console.WriteLine(filePath);
            
            outputs.ForEach(output => File.AppendAllText(filePath, $"{output.ToString()}" + Environment.NewLine));

            MessageBox.Show($"{WGU_C969_Software_II_CS.Resources.MainWindow.ReportsComboBox_SuccessfullySaved}: {filePath}");
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
                        $@"ID: {reader.GetString("countryId")}, " +
                        $@"Country: {reader.GetString("country")}, " +
                        $@"Created: {reader.GetString("createDate")}, " +
                        $@"By: {reader.GetString("createdBy")}, " +
                        $@"Updated: {reader.GetString("lastUpdate")}, " +
                        $@"By: {reader.GetString("lastUpdateBy")}"
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

