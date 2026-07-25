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
    
    private DateTime AppointmentTime { get; set; }
    
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
        
        URLLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.URL;
        AppointmentTitleLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.AppointmentTitle;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        ContactLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Contact;
        TypeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Type;
        LocationLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Location;
        DescriptionLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.Description;
        SelectTimeLabel.Content = WGU_C969_Software_II_CS.Resources.AppointmentForm.SelectTime;
    }

    private void LoadAvailibleTimes()
    {
        
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
        
        if (titleBinding is { HasError: true } ||
            descriptionBinding is { HasError: true } ||
            locationComboBinding is { HasError: true } ||
            contactBinding is { HasError: true } ||
            typeComboBinding is { HasError: true } ||
            urlBinding is { HasError: true })
        {
            return;
        }
        
        this.AppointmentTitle = this.AppointmentTitleTextbox.Text;
        this.Description = this.DescriptionTextBox.Text;
        this.SelectedLocation = this.LocationComboBox.SelectedItem.ToString();
        this.Contact = this.ContactTextBox.Text;
        this.SelectedType = this.TypeComboBox.SelectedItem.ToString();
        this.url = this.URLTextBox.Text;
        
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