using System.ComponentModel;
using System.Runtime.InteropServices.Marshalling;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MySqlConnector;

namespace WGU_C969_Software_II_CS;

public partial class AppointmentForm : INotifyPropertyChanged
{
    // ReSharper disable once InconsistentNaming
    private int ID { get; init; }
    private string CurrentUsername { get; }
    
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
    
    private void ReadContact(int customerId)
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
        command.Parameters.AddWithValue("@customerId", customerId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            PhoneClass phone = new PhoneClass();
            phone = reader["phone"].ToString();
            this.Contact = $"{reader["customerName"].ToString()} ({phone.ToString()}). {reader["address"].ToString()}, {reader["city"]}";
        }
    }
    
    private List<TimeSpan> LoadAvailibleTimes()
    {
        List<TimeSpan> allTimes = new List<TimeSpan>();
        for (var t = new TimeSpan(9, 0, 0); t < new TimeSpan(17, 0, 0); t += TimeSpan.FromMinutes(30))
        {
            allTimes.Add(t);
        }
        
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand(
            @"SELECT TIME(start) as startTime
              FROM appointment
              WHERE DATE(start) = @date",
            connection
        );
        command.Parameters.AddWithValue("@date", SelectDateCalendar.SelectedDate);
        using var reader = command.ExecuteReader();
        
        var bookedTimes = new List<TimeSpan>();
        while (reader.Read())
        {
            TimeSpan time = (TimeSpan)reader["startTime"];
            Console.WriteLine(time.ToString());
            bookedTimes.Add(time);
        }
        
        return allTimes
            .Except(bookedTimes
                .Select(t => new TimeSpan(t.Hours, t.Minutes, 0))
                .ToList())
            .ToList();
    }
    
    public AppointmentForm(int appointmentId, string currentUsername, int customerId)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = appointmentId;
        this.CurrentUsername = currentUsername;
        
        this.ReadTypesAndLocations();
        TypeComboBox.ItemsSource = Types;
        LocationComboBox.ItemsSource = Locations;
        
        this.ReadContact(customerId);
        this.ContactTextBox.Text = this.Contact;

        this.SelectDateCalendar.SelectedDate = DateTime.Now.Date;
        
        URLLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.URL;
        AppointmentTitleLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.AppointmentTitle;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        ContactLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Contact;
        TypeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Type;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        DescriptionLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Description;
        SelectStartTimeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.SelectStartTime;
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
        
        BindingExpression? timeComboBinding = SelectStartTimeComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (timeComboBinding != null)
        {
            if (TypeComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(timeComboBinding,
                    new ValidationError(new ExceptionValidationRule(), timeComboBinding));
            }
            else
            {
                Validation.ClearInvalid(timeComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind SelectTimeComboBox");
        }
        
        if (titleBinding is { HasError: true } ||
            descriptionBinding is { HasError: true } ||
            locationComboBinding is { HasError: true } ||
            contactBinding is { HasError: true } ||
            typeComboBinding is { HasError: true } ||
            urlBinding is { HasError: true } || 
            timeComboBinding is { HasError: true })
        {
            return;
        }
        
        this.AppointmentTitle = this.AppointmentTitleTextbox.Text;
        this.Description = this.DescriptionTextBox.Text;
        this.SelectedLocation = this.LocationComboBox.SelectedItem.ToString();
        this.Contact = this.ContactTextBox.Text;
        this.SelectedType = this.TypeComboBox.SelectedItem.ToString();
        this.url = this.URLTextBox.Text;
        
        //this.AppointmentTime[0] = new DateTime(this.SelectDateCalendar.SelectedDate, SelectedTime.Split())
        
        //this.DataBaseUpdater();
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
        this.SelectStartTimeComboBox.ItemsSource = this.LoadAvailibleTimes().Select(t => t.ToString(@"hh\:mm")).ToList();
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