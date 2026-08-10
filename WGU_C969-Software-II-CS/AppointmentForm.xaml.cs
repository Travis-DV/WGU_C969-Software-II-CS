using System.ComponentModel;
using System.Runtime.InteropServices.Marshalling;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MySqlConnector;

namespace WGU_C969_Software_II_CS;

public partial class AppointmentForm : INotifyPropertyChanged, IDatabaseInteraction
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; init; }
    private string CurrentUsername { get; init; }
    public int CustomerId { get; init; }
    public int UserId { get; init; }
    
    private string _appointmentTitle = "";
    public string AppointmentTitle
    {
        get => _appointmentTitle;
        set
        {
            _appointmentTitle = value;
            OnPropertyChanged(nameof(AppointmentTitle));
            Console.WriteLine($"Appointment Title Change: {value}");
        } 
    }
    
    private string _description = "";
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
            Console.WriteLine($"Description Change: {value}");
        } 
    }
    
    // ReSharper disable once InconsistentNaming
    private List<string> Locations { get; set; }
    private string _selectedLocation = "";
    public string SelectedLocation
    {
        get => _selectedLocation;
        set
        {
            _selectedLocation = value;
            OnPropertyChanged(nameof(SelectedLocation));
            Console.WriteLine($"Selected Location Change: {value}");
        } 
    }
    
    private string _contact = "";
    public string Contact
    {
        get => _contact;
        set
        {
            _contact = value;
            OnPropertyChanged(nameof(Contact));
            Console.WriteLine($"Contact Change: {value}");
        } 
    }
    
    private List<string> Types { get; set; }
    private string _selectedType = "";
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged(nameof(SelectedType));
            Console.WriteLine($"Selected Location Change: {value}");
        } 
    }
    
    private string _url = "";
    public string url
    {
        get => _url;
        set
        {
            _url = value;
            OnPropertyChanged(nameof(url));
            Console.WriteLine($"Description Change: {value}");
        } 
    }
    
    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged(nameof(SelectedDate));
            Console.WriteLine($"Selected Date Change: {value}");
        }
    }
    private string _selectedStartTime = "";
    public string SelectedStartTime
    {
        get => _selectedStartTime;
        set
        {
            _selectedStartTime = value;
            OnPropertyChanged(nameof(SelectedStartTime));
            Console.WriteLine($"Selected Start Time Change: {value}");
            if (value == "")
            {
                //this.SelectedEndTime = "";
                this.SelectEndTimeComboBox.SelectedIndex = -1;
                return;
            }
            this.SelectEndTimeComboBox.ItemsSource = this.AvailableEndTimes.Select(t => t.ToString(@"hh\:mm")).ToList();
        } 
    }
    private string _selectedEndTime = "";
    public string SelectedEndTime
    {
        get => _selectedEndTime;
        set
        {
            _selectedEndTime = value;
            OnPropertyChanged(nameof(SelectedEndTime));
            Console.WriteLine($"Selected End Time Change: {value}");
        } 
    }
    private DateTime[] AppointmentTime { get; set; } = new  DateTime[2];

    private List<TimeSpan> AllTimes
    {
        get
        {
            List<TimeSpan> output = new List<TimeSpan>();
            for (var t = new TimeSpan(9, 0, 0); t < new TimeSpan(17, 0, 0); t += TimeSpan.FromMinutes(30))
            {
                output.Add(t);
            }

            return output;
        }
    } 
    
    private List<(TimeSpan start, TimeSpan end)> BookedRanges
    {
        get
        {
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand(
                @"SELECT TIME(start) as startTime, TIME(end) as endTime
                  FROM appointment
                  WHERE DATE(start) = @date",
                connection
            );
            command.Parameters.AddWithValue("@date", SelectDateCalendar.SelectedDate);
            using var reader = command.ExecuteReader();
            
            List<(TimeSpan start, TimeSpan end)> output = new List<(TimeSpan start, TimeSpan end)>();
            while (reader.Read())
            {
                TimeSpan start = (TimeSpan)reader["startTime"];
                TimeSpan end = (TimeSpan)reader["endTime"];
                Console.WriteLine($"start: {start.ToString()}, end: {end.ToString()}");
                output.Add((
                    new TimeSpan(start.Hours, start.Minutes, 0), 
                    new TimeSpan(end.Hours, end.Minutes, 0)
                ));
            }

            return output;
        }
    }

    private List<TimeSpan> AvailableStartTimes
    {
        get
        {
            return this.AllTimes
                .Where(slot => !this.BookedRanges.Any(r => slot >= r.start && slot <= r.end))
                .ToList();
        }
    }

    private List<TimeSpan> AvailableEndTimes
    {
        get
        {
            if (this.SelectedStartTime == null || this.SelectedStartTime == "" || this.SelectedStartTime == "Pick today or a future date!")
            {
                return new List<TimeSpan>();;
            }


            return this.AllTimes
                .Where(t => t > TimeSpan.Parse(this.SelectedStartTime))
                .TakeWhile(timeSpan => !this.BookedRanges.Any(r => timeSpan >= r.start && timeSpan <= r.end))
                .ToList();
        }
    }


    private void ReadTypesAndLocations()
    {
        this.Types = new List<string>();
        this.Locations = new List<string>();
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand($"SELECT * FROM appointment", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (!this.Types.Contains(reader["type"].ToString()))
            {
                this.Types.Add(reader["type"].ToString());
            }
            if (!this.Locations.Contains(reader["type"].ToString()))
            {
                this.Locations.Add(reader["location"].ToString());
            }
        }
    }
    
    private void ReadContact()
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand(
            @"SELECT customer.customerName, address.phone, address.address, city.city
              FROM customer 
              JOIN address ON customer.addressId = address.addressId
              JOIN city ON address.cityId = city.cityId
              WHERE customerId = @customerId",
            connection
        );
        command.Parameters.AddWithValue("@customerId", this.CustomerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            PhoneClass phone = new PhoneClass();
            phone = reader["phone"].ToString();
            this.Contact = $"{reader["customerName"].ToString()} ({phone.ToString()}). {reader["address"].ToString()}, {reader["city"]}";
        }
    }
    
    public AppointmentForm(int appointmentId, string currentUsername, int customerId, int userId)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = appointmentId;
        this.CurrentUsername = currentUsername;
        this.UserId = userId;
        
        this.ReadTypesAndLocations();
        TypeComboBox.ItemsSource = Types;
        LocationComboBox.ItemsSource = Locations;

        this.CustomerId = customerId;
        this.ReadContact();
        this.ContactTextBox.Text = this.Contact;

        this.SelectDateCalendar.SelectedDate = DateTime.Now.Date;
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand($"SELECT * FROM appointment WHERE appointmentId = @id", connection);
        command.Parameters.AddWithValue("@id", this.ID);
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                this.AppointmentTitle = reader["title"].ToString() ?? "";
                this.Description = reader["description"].ToString() ?? "";
                this.LocationComboBox.SelectedItem = reader["location"].ToString();
                this.TypeComboBox.SelectedItem = reader["type"].ToString();
                this.url = reader["url"].ToString();
                DateTime start = DateTime.Parse(reader["start"].ToString());
                this.SelectDateCalendar.SelectedDate = start;
                this.SelectStartTimeComboBox.SelectedItem = start.TimeOfDay.ToString();
                DateTime end = DateTime.Parse(reader["end"].ToString());
                this.SelectEndTimeComboBox.SelectedItem = start.TimeOfDay.ToString();
            }
        }
        
        URLLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.URL;
        AppointmentTitleLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.AppointmentTitle;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        ContactLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Contact;
        TypeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Type;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        DescriptionLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Description;
        SelectStartTimeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.SelectStartTime;
        SelectEndtTimeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.SelectEndTime;
    }
    
    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? titleBinding = AppointmentTitleTextbox.GetBindingExpression(TextBox.TextProperty);
        titleBinding?.UpdateSource();
        BindingExpression? descriptionBinding = DescriptionTextBox.GetBindingExpression(TextBox.TextProperty);
        descriptionBinding?.UpdateSource();
        
        BindingExpression? locationComboBinding = LocationComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (locationComboBinding != null)
        {
            if (LocationComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(locationComboBinding,
                    new ValidationError(new ExceptionValidationRule(), locationComboBinding));
            }
            else
            {
                Validation.ClearInvalid(locationComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind LocationComboBox");
        }
        
        BindingExpression? contactBinding = ContactTextBox.GetBindingExpression(TextBox.TextProperty);
        contactBinding?.UpdateSource();
        
        BindingExpression? typeComboBinding = TypeComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (typeComboBinding != null)
        {
            if (TypeComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(typeComboBinding,
                    new ValidationError(new ExceptionValidationRule(), typeComboBinding));
            }
            else
            {
                Validation.ClearInvalid(typeComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind TypeComboBox");
        }
        
        BindingExpression? urlBinding = URLTextBox.GetBindingExpression(TextBox.TextProperty);
        urlBinding?.UpdateSource();
        
        BindingExpression? startTimeComboBinding = SelectStartTimeComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (startTimeComboBinding != null)
        {
            if (SelectStartTimeComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(startTimeComboBinding,
                    new ValidationError(new ExceptionValidationRule(), startTimeComboBinding));
            }
            else
            {
                Validation.ClearInvalid(startTimeComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind SelectTimeComboBox");
        }
        
        BindingExpression? endTimeComboBinding = SelectEndTimeComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (endTimeComboBinding != null)
        {
            if (SelectEndTimeComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(endTimeComboBinding,
                    new ValidationError(new ExceptionValidationRule(), endTimeComboBinding));
            }
            else
            {
                Validation.ClearInvalid(endTimeComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind SelectEndTimeComboBox");
        }
        
        if (titleBinding is { HasError: true } ||
            descriptionBinding is { HasError: true } ||
            locationComboBinding is { HasError: true } ||
            contactBinding is { HasError: true } ||
            typeComboBinding is { HasError: true } ||
            urlBinding is { HasError: true } || 
            startTimeComboBinding is { HasError: true } || 
            endTimeComboBinding is { HasError: true })
        {
            return;
        }
        
        this.AppointmentTitle = this.AppointmentTitleTextbox.Text;
        this.Description = this.DescriptionTextBox.Text;
        this.SelectedLocation = this.LocationComboBox.SelectedItem.ToString();
        this.Contact = this.ContactTextBox.Text;
        this.SelectedType = this.TypeComboBox.SelectedItem.ToString();
        this.url = this.URLTextBox.Text;
        
        this.AppointmentTime[0] =
            this.SelectDateCalendar.SelectedDate.Value.Date.Add(TimeSpan.Parse(SelectedStartTime));
        this.AppointmentTime[1] =
            this.SelectDateCalendar.SelectedDate.Value.Date.Add(TimeSpan.Parse(SelectedEndTime));
        
        this.DataBaseUpdater();
        this.DialogResult = true;
        this.Close();
    }
    
    private void MyTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged)
        {
            double newHeight = e.NewSize.Height;
            DoneButton.Width = newHeight * 2;
        }
    }

    private void SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        BindingExpression? dateCalandarBinding = SelectDateCalendar.GetBindingExpression(Calendar.SelectedDateProperty);
        if (dateCalandarBinding != null)
        {
            if (this.SelectDateCalendar.SelectedDate < DateTime.Now.Date)
            {
                Validation.MarkInvalid(dateCalandarBinding,
                    new ValidationError(new ExceptionValidationRule(), dateCalandarBinding));
                this.SelectStartTimeComboBox.ItemsSource = new List<string>() {"Pick today or a future date!"};
                this.SelectStartTimeComboBox.SelectedIndex = 0;
                return;
            }
            Validation.ClearInvalid(dateCalandarBinding);
        }
        else
        {
            throw new Exception("Failed to bind SelectDateCalendar");
        }
        this.SelectStartTimeComboBox.ItemsSource = this.AvailableStartTimes.Select(t => t.ToString(@"hh\:mm")).ToList();
        this.SelectEndTimeComboBox.ItemsSource = this.AvailableEndTimes.Select(t => t.ToString(@"hh\:mm")).ToList();
    }

    private void DataBaseUpdater()
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        // ReSharper disable once UseRawString
        using MySqlCommand command = new MySqlCommand(@"
                    INSERT INTO appointment 
                        (appointmentId, customerId, userId, title, description, location, contact, type, url, start, end, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@appointmentId, @customerId, @userId, @title, @description, @location, @contact, @type, @url, @start, @end, @createDate, @createdBy, @lastUpdate, @lastUpdateBy) AS new
                    ON DUPLICATE KEY UPDATE
                        customerId = new.customerId,
                        userId = new.userId,
                        title = new.title,
                        description = new.description,
                        location = new.location,
                        contact = new.contact,
                        type = new.type,
                        url = new.url,
                        start = new.start,
                        end = new.end,
                        lastUpdate = new.lastUpdate,
                        lastUpdateBy = new.lastUpdateBy", connection);
        command.Parameters.AddWithValue("@appointmentId", this.ID);
        command.Parameters.AddWithValue("@customerId", this.CustomerId);
        command.Parameters.AddWithValue("@userId", this.UserId);
        command.Parameters.AddWithValue("@title", this.AppointmentTitle);
        command.Parameters.AddWithValue("@description", this.Description);
        command.Parameters.AddWithValue("@location", this.SelectedLocation);
        command.Parameters.AddWithValue("@contact", this.Contact);
        command.Parameters.AddWithValue("@type", this.SelectedType);
        command.Parameters.AddWithValue("@url", this.url);
        command.Parameters.AddWithValue("@start", this.AppointmentTime[0]);
        command.Parameters.AddWithValue("@end", this.AppointmentTime[1]);
        command.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        command.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        command.ExecuteNonQuery();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public enum AppointmentParts
{
    CustomerId,
    Title,
    Description,
    Location,
    Type,
    DateTime
}