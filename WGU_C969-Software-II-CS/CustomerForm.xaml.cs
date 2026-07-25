using MySqlConnector;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : INotifyPropertyChanged, IDatabaseInteraction
{
    // ReSharper disable once InconsistentNaming
    private int ID { get; init; }
    private string CurrentUsername { get; }

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
            Console.WriteLine($"First Name Change: {value}");
        } 
    }

    private string _lastName = "";
    public string LastName {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
            Console.WriteLine($"Last Name Change: {value}");
        } 
    }
    
    private int AddressId { get; set; }  = -1;
    // ReSharper disable once InconsistentNaming
    //private bool _AddressMod { get; set; }
    private bool AddressMod
    {
        // ReSharper disable once UnusedMember.Local
        //get => _AddressMod;
        set
        {
            //_AddressMod = value;
            if (value)
            {
                this.AddressModButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressModButton;
            }
            else if (!value)
            {
                this.AddressModButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressAddButton;
            }
        } 
    }

    private AddressFrom _addressFrom;
    private AddressFrom Address
    {
        get => _addressFrom;
        set
        {
            _addressFrom = value;
            OnPropertyChanged(nameof(AddressString));
        }
    }
    public string AddressString
    {
        get => Address?.ToString() ?? "";
        set => OnPropertyChanged(nameof(AddressString));
    }

    public CustomerForm(int customerId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = customerId;
        this.CurrentUsername = currentUsername;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT * FROM  customer WHERE customerId = @id", connection))
            {
                command.Parameters.AddWithValue("@id", this.ID);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string[] customerName = (reader["customerName"].ToString() ?? "").Split(" ");
                    this.FirstName = customerName[0];
                    this.LastName = customerName[1];
                    this.AddressId = int.Parse(reader["addressId"].ToString() ?? "");
                    this.AddressMod = true;
                    this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
                }
            }
        }
        
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.FirstNameLabel + ": ";
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.LastNameLabel + ": ";
        this.AddressLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressLabel + ": ";
        this.AddressClearButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressClearButton;
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }

    private void AddressModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.AddressId == -1)
        {
            using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
            connection.Open();
            using MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(addressId), 0) + 1 FROM address;", connection);
            this.AddressId = Convert.ToInt32(command.ExecuteScalar());
        }
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername)
        {
            Owner = this
        };
        this.Address.ShowDialog();
        
        if (this.Address is {DialogResult: true})
        {
            OnPropertyChanged(nameof(AddressString));
            this.AddressMod = true;
        }
    }
    
    private void AddressClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.AddressId = -1;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        this.AddressTextBox.Text = "";
    }
    
    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? firstNameBinding = FirstNameTextBox.GetBindingExpression(TextBox.TextProperty);
        firstNameBinding?.UpdateSource();

        BindingExpression? lastNameBinding = LastNameTextBox.GetBindingExpression(TextBox.TextProperty);
        lastNameBinding?.UpdateSource();

        BindingExpression? addressBinding = AddressTextBox.GetBindingExpression(TextBox.TextProperty);
        if (addressBinding != null)
        {
            if (this.AddressTextBox.Text == "")
            {
                Validation.MarkInvalid(addressBinding,
                    new ValidationError(new ExceptionValidationRule(), addressBinding));
            }
            else
            {
                Validation.ClearInvalid(addressBinding);
            }
        }

        if (firstNameBinding is { HasError: true } || 
            lastNameBinding is { HasError: true } ||
            addressBinding is { HasError: true })
        {
            return;
        }
        
        this.FirstName = this.FirstNameTextBox.Text;
        this.LastName = this.LastNameTextBox.Text;
        this.AddressId = this.Address.ID;
        
        this.DataBaseUpdater();
        this.DialogResult = true;
        this.Close();
    }

    private void DataBaseUpdater()
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand(@"
                    INSERT INTO customer 
                        (customerId, customerName, addressId, active, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@customerId, @customerName, @addressId, @active, @createDate, @createdBy, @lastUpdate, @lastUpdateBy) AS new
                    ON DUPLICATE KEY UPDATE
                        customerName = new.customerName,
                        addressId = new.addressId,
                        active = new.active,
                        lastUpdate = new.lastUpdate,
                        lastUpdateBy = new.lastUpdateBy", connection);
        command.Parameters.AddWithValue("@customerId", this.ID);
        command.Parameters.AddWithValue("@customerName", $"{this.FirstName} {this.LastName}");
        command.Parameters.AddWithValue("@addressId", this.AddressId);
        command.Parameters.AddWithValue("@active", 1);
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
