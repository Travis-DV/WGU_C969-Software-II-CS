using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
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
    
    private List<DateTime> AllTimes
    {
        get
        {
            List<DateTime> output = new List<DateTime>();
            for (TimeSpan t = new TimeSpan(14, 0, 0); t < new TimeSpan(22, 0, 0); t += TimeSpan.FromMinutes(30))
            {
                output.Add(DateTime.SpecifyKind(DateTime.UtcNow.Date.Add(t), DateTimeKind.Utc));
            }

            return output;
        }
    } 
    
    private List<(DateTime start, DateTime end)> BookedRanges
    {
        get
        {
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand(
                @"SELECT start, end
                  FROM appointment
                  WHERE DATE(start) = @date
                    AND appointmentId != @appointmentId",
                connection
            );
            command.Parameters.AddWithValue("@date", DateOnly.FromDateTime((DateTime)this.SelectedDate));
            command.Parameters.AddWithValue("@appointmentId", this.ID);
            using MySqlDataReader reader = command.ExecuteReader();
            
            List<(DateTime start, DateTime end)> output = new List<(DateTime start, DateTime end)>();
            while (reader.Read())
            {
                DateTime temp_001 = reader.GetDateTime("start");
                DateTime start = DateTime.SpecifyKind(reader.GetDateTime("start"),  DateTimeKind.Utc);
                start = start.AddSeconds(-start.Second);
                DateTime end = DateTime.SpecifyKind(reader.GetDateTime("end"),  DateTimeKind.Utc);
                end = end.AddSeconds(-end.Second);
                Console.WriteLine($"start: {start}, end: {end}");
                output.Add((
                    start, end
                ));
            }

            return output;
        }
    }

    private List<DateTime> AvailableStartTimes
    {
        get
        {
            List<(DateTime start, DateTime end)> bookedRanges = this.BookedRanges;
            List<DateTime> output = this.AllTimes;
            

            foreach ((DateTime start, DateTime end) bookedTime in bookedRanges)
            {
                List<DateTime> temp = new List<DateTime>();
                for (int i = 0; i < output.Count; i++)
                {
                    if (output[i].TimeOfDay >= bookedTime.start.TimeOfDay && output[i].TimeOfDay <= bookedTime.end.TimeOfDay)
                    {
                        temp.Add(output[i]);
                    }

                    else if (output[i].TimeOfDay > bookedTime.end.TimeOfDay)
                    {
                        i = output.Count;
                    }
                }

                output = output.Except(temp).ToList();
            }

            return output;
        }
    }

    private List<string> AvailableStartTimesLocal
    {
        get
        {
            List<string> output = new List<string>();
            
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            string zoneName = localZone.IsDaylightSavingTime(DateTime.Now) 
                ? localZone.DaylightName 
                : localZone.StandardName;
            
            string abbreviation = new string(zoneName
                .Split(' ')
                .Where(word => word.Length > 0 && char.IsUpper(word[0]))
                .Select(word => word[0])
                .ToArray());

            foreach (DateTime startTime in this.AvailableStartTimes)
            {
                DateTime localStartTime = startTime.ToLocalTime();
                
                output.Add($"{localStartTime.ToString(@"hh\:mm tt")} ({abbreviation})");
            }
            
            return output;
        }
    }

    private List<DateTime> AvailableEndTimes
    {
        get
        {
            if (this.SelectedStartTime == null || this.SelectedStartTime == "" || this.SelectedStartTime == "Pick today or any future date!")
            {
                return new List<DateTime>();
            }
            
            List<DateTime> output = this.AllTimes;
            List<DateTime> temp = new List<DateTime>();
    
            DateTime selectedUtcStartTime = AvailableStartTimes[this.SelectStartTimeComboBox.SelectedIndex];
            foreach (DateTime time in output)
            {
                if (time.TimeOfDay <= selectedUtcStartTime.TimeOfDay)
                {
                    temp.Add(time);
                }
                else if (time.TimeOfDay > selectedUtcStartTime.TimeOfDay)
                {
                    break;
                }
            }
            output = output.Except(temp).ToList();
            

            List<(DateTime start, DateTime end)> bookedRanges = this.BookedRanges;
            foreach ((DateTime start, DateTime end) bookedTime in bookedRanges)
            {
                temp = new List<DateTime>();
                for (int i = 0; i < output.Count; i++)
                {
                    if (!(output[i].TimeOfDay >= bookedTime.start.TimeOfDay && output[i].TimeOfDay <= bookedTime.end.TimeOfDay))
                    {
                        temp.Add(output[i]);
                    }
                    else
                    {
                        break;
                    }
                }

                output = temp;
            }

            return output;
        }
    }
    
    private List<String> AvailableEndTimesLocal
    {
        get
        {
            List<string> output = new List<string>();
            
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            string zoneName = localZone.IsDaylightSavingTime(DateTime.Now) 
                ? localZone.DaylightName 
                : localZone.StandardName;
            
            string abbreviation = new string(zoneName
                .Split(' ')
                .Where(word => word.Length > 0 && char.IsUpper(word[0]))
                .Select(word => word[0])
                .ToArray());

            foreach (DateTime endTime in this.AvailableEndTimes)
            {
                DateTime localEndTime = endTime.ToLocalTime();
                
                output.Add($"{localEndTime.ToString(@"hh\:mm tt")} ({abbreviation})");
            }
            
            return output;
        }
    }
    
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
            Console.WriteLine($"Selected Type Change: {value}");
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
            OnPropertyChanged(nameof(SelectedDate));
            _selectedDate = value;
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
            
            this.SelectEndTimeComboBox.ItemsSource =
                this.AvailableEndTimesLocal;
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
    private DateTime[] AppointmentTime { get; set; } = new DateTime[2];
    
    private void ReadTypesAndLocations()
    {
        this.Types = new List<string>();
        this.Locations = new List<string>();
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand($"SELECT * FROM appointment", connection);
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string newType = reader.GetString("type");
            if (!this.Types.Contains(newType))
            {
                this.Types.Add(newType);
            }
            string newLocation = reader.GetString("location");
            if (!this.Locations.Contains(newLocation))
            {
                this.Locations.Add(newLocation);
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
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            PhoneClass phone = new PhoneClass();
            phone = reader.GetString("phone");
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
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                this.AppointmentTitle = reader.GetString("title");
                this.Description = reader.GetString("description");
                this.LocationComboBox.SelectedItem = reader.GetString("location");
                this.TypeComboBox.SelectedItem = reader.GetString("type");
                this.url = reader.GetString("url");
                DateTime start = DateTime.SpecifyKind(reader.GetDateTime("start"), DateTimeKind.Utc);
                this.SelectDateCalendar.SelectedDate = start;
                this.SelectStartTimeComboBox.SelectedIndex = AvailableStartTimes.FindIndex(time => time.TimeOfDay == start.TimeOfDay);
                DateTime end = DateTime.SpecifyKind(reader.GetDateTime("end"), DateTimeKind.Utc);
                this.SelectEndTimeComboBox.SelectedIndex = AvailableEndTimes.FindIndex(time => time.TimeOfDay == end.TimeOfDay);
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
        
        DateTime selectedUtcStartTime = AvailableStartTimes[this.SelectStartTimeComboBox.SelectedIndex];
        DateTime selectedUtcEndTime = AvailableEndTimes[this.SelectEndTimeComboBox.SelectedIndex];

        this.AppointmentTime[0] =
            this.SelectDateCalendar.SelectedDate.Value.Date.Add(selectedUtcStartTime.TimeOfDay);
        this.AppointmentTime[1] =
            this.SelectDateCalendar.SelectedDate.Value.Date.Add(selectedUtcEndTime.TimeOfDay);
        
        MessageBox.Show($"offset {TimeZoneInfo.Local.GetUtcOffset(AppointmentTime[0])} Selected {SelectedStartTime} Saved {AppointmentTime[0].TimeOfDay}, timezone local {TimeZoneInfo.Local} DateTimeKindLocal {DateTimeKind.Local} AvailableStartTimeIndex {selectedUtcStartTime} AvailableStartTimeLOCALIndex {AvailableStartTimesLocal[this.SelectStartTimeComboBox.SelectedIndex]}");
        
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
        command.Parameters.AddWithValue("@createDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        command.Parameters.AddWithValue("@lastUpdate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        command.ExecuteNonQuery();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        BindingExpression? dateCalandarBinding = SelectDateCalendar.GetBindingExpression(Calendar.SelectedDateProperty);
        if (dateCalandarBinding != null)
        {
            if (this.SelectedDate < DateTime.Now.Date)
            {
                Validation.MarkInvalid(dateCalandarBinding,
                    new ValidationError(new ExceptionValidationRule(), dateCalandarBinding));
                this.SelectStartTimeComboBox.ItemsSource = new List<string>() {"Pick today or a future date!"};
                this.SelectStartTimeComboBox.SelectedIndex = 0;
                this.SelectEndTimeComboBox.ItemsSource = new List<string>();
                //this.SelectEndTimeComboBox.SelectedIndex = -1;
                return;
            }
            Validation.ClearInvalid(dateCalandarBinding);
                
                
        }
        else
        {
            throw new Exception("Failed to bind SelectDateCalendar");
        }

        this.SelectStartTimeComboBox.ItemsSource = 
            this.AvailableStartTimesLocal;
        this.SelectEndTimeComboBox.ItemsSource =
            this.AvailableEndTimesLocal;
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