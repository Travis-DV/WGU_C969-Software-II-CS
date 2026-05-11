using System.ComponentModel;
using System.Configuration;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : INotifyPropertyChanged
{
    // ReSharper disable once InconsistentNaming
    private int ID { get; }
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
    private bool _AddressMod { get; set; }
    private bool AddressMod
    {
        // ReSharper disable once UnusedMember.Local
        get => _AddressMod;
        set
        {
            _AddressMod = value;
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
        set
        {
            Console.WriteLine("AddressString Set LMAO");
            OnPropertyChanged(nameof(AddressString));
        }
    }

    private PhoneClass PhoneNumber { get; set; } = new PhoneClass();
    public string PhoneNumberString
    {
        get
        {
            string output = PhoneNumber?.ToString() ?? "";
            if (output == "+0 (0) 0-0")
            {
                return "";
            }
            return output;
        }
        set
        {
            PhoneNumber.Validate(value.ToString(), CultureInfo.CurrentCulture);
            OnPropertyChanged(nameof(PhoneNumberString));
            Console.WriteLine($"Phone Number String Change: {value}");
        }
    }

    public CustomerForm(int customerId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = customerId;
        this.CurrentUsername = currentUsername;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        
        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            using (SQLiteCommand cmd =
                   new SQLiteCommand($"SELECT * FROM  customer WHERE customerId == @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", this.ID);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string[] customerName = (reader["customerName"].ToString() ?? "").Split(" ");
                        this.FirstName = customerName[0];
                        this.LastName = customerName[1];
                        this.AddressId = int.Parse(reader["addressId"].ToString() ?? "");
                        this.AddressMod = true;
                        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
                        this.PhoneNumber.Validate(reader["phoneNumber"].ToString() ?? "", CultureInfo.CurrentCulture);
                        OnPropertyChanged(nameof(PhoneNumberString));
                    }
                }
            }
        }
        
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.FirstNameLabel + ": ";
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.LastNameLabel + ": ";
        this.AddressLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressLabel + ": ";
        this.AddressClearButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressClearButton;
        this.PhoneNumberLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.PhoneNumberLabel + ": ";
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }

    private void AddressModButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.AddressId == -1)
        {
            using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
            conn.Open();
            using SQLiteCommand cmd = new SQLiteCommand("SELECT IFNULL(MAX(addressId), 0) + 1 FROM address;", conn);
            this.AddressId = Convert.ToInt32(cmd.ExecuteScalar());
        }
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername)
        {
            Owner = this
        };
        this.Address.ShowDialog();
        OnPropertyChanged(nameof(AddressString));
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

        BindingExpression? phoneNumberBinding = PhoneNumberTextBox.GetBindingExpression(TextBox.TextProperty);
        phoneNumberBinding?.UpdateSource();

        if (firstNameBinding is { HasError: true } || 
            lastNameBinding is { HasError: true } ||
            addressBinding is { HasError: true } ||
            phoneNumberBinding is { HasError: true })
        {
            return;
        }
        
        this.FirstName = this.FirstNameTextBox.Text;
        this.LastName = this.LastNameTextBox.Text;
        this.AddressId = this.Address.ID;
        this.PhoneNumber.Validate(PhoneNumberTextBox.Text, CultureInfo.CurrentCulture);
        this.PhoneNumberTextBox.Text = this.PhoneNumber.ToString();
        
        this.DataBaseUpdater();
        this.Close();
    }

    private void DataBaseUpdater()
    {
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using SQLiteCommand cmd = new SQLiteCommand(@"
                    INSERT INTO customer 
                        (customerId, customerName, addressId, phonenumber, active, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@customerId, @customerName, @addressId, @phoneNumber, @active, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)
                    ON CONFLICT(customerId) DO UPDATE SET
                        customerName = excluded.customerName, 
                        addressId = excluded.addressId, 
                        phoneNumber = excluded.phoneNumber,
                        active = excluded.active, 
                        createDate = createDate, 
                        createdBy = createdBy, 
                        lastUpdate = excluded.lastUpdate, 
                        lastUpdateBy = excluded.lastUpdateBy", conn);
        cmd.Parameters.AddWithValue("@customerId", this.ID);
        cmd.Parameters.AddWithValue("@customerName", $"{this.FirstName} {this.LastName}");
        cmd.Parameters.AddWithValue("@addressId", this.AddressId);
        cmd.Parameters.AddWithValue("@phoneNumber", this.PhoneNumber);
        cmd.Parameters.AddWithValue("@active", 1);
        cmd.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@createdBy", "admin");
        cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@lastUpdateBy", "admin");

        cmd.ExecuteNonQuery();
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
