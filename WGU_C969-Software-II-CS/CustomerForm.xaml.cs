using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm
{
    // ReSharper disable once InconsistentNaming
    private int ID { get; }
    private string CurrentUsername { get; }
    private string FirstName { get; set; } = "";
    private string LastName { get; set; } = "";
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
    private AddressFrom Address { get; set; }
    private PhoneClass PhoneNumber { get; set; }  = new PhoneClass();

    public CustomerForm(int customerId, string currentUsername)
    {
        InitializeComponent();
        this.DataContext = this;
        
        this.ID = customerId;
        this.CurrentUsername = currentUsername;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        this.PhoneNumber.Validate("", CultureInfo.CurrentCulture);
        
        
        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            using (SQLiteCommand cmd =
                   new SQLiteCommand($"SELECT * FROM  customer WHERE customerId == {this.ID}", conn))
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
                }
            }
        }
        
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.FirstNameLabel + ": ";
        this.FirstNameTextBox.Text = this.FirstName;
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.LastNameLabel + ": ";
        this.LastNameTextBox.Text = this.LastName;
        this.AddressLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressLabel + ": ";
        this.AddressTextBox.Text = this.Address.ToString();
        this.AddressClearButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressClearButton;
        this.PhoneNumberLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.PhoneNumberLabel + ": ";
        this.PhoneNumberTextBox.Text = this.PhoneNumber.ToString();
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
        this.Address.Show();
        this.AddressTextBox.Text = this.Address.ToString();
    }
    
    private void AddressClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.AddressId = -1;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        this.AddressTextBox.Text = this.Address.ToString();
    }
    
    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? firstNameBinding = FirstNameTextBox.GetBindingExpression(TextBox.TextProperty);
        firstNameBinding?.UpdateSource();

        BindingExpression? lastNameBinding = LastNameTextBox.GetBindingExpression(TextBox.TextProperty);
        lastNameBinding?.UpdateSource();

        BindingExpression? addressBinding = AddressTextBox.GetBindingExpression(TextBox.TextProperty);
        if (this.AddressTextBox.Text == "")
        {
            if (addressBinding != null)
            {
                Validation.MarkInvalid(addressBinding,
                    new ValidationError(new ExceptionValidationRule(), addressBinding)); 
            }
        }
        else
        {
            if (addressBinding != null)
            {
                Validation.ClearInvalid(addressBinding);
            }
        }

        BindingExpression? phoneNumberBinding = PhoneNumberTextBox.GetBindingExpression(TextBox.TextProperty);
        phoneNumberBinding?.UpdateSource();
        if (phoneNumberBinding is { HasError: false })
        {
            this.PhoneNumber.Validate(PhoneNumberTextBox.Text, CultureInfo.CurrentCulture);
            this.PhoneNumberTextBox.Text = this.PhoneNumber.ToString();
        }

        if (firstNameBinding is not { HasError: false } || 
            lastNameBinding is not { HasError: false } ||
            addressBinding is not { HasError: false } ||
            phoneNumberBinding is not { HasError: false })
        {
            return;
        }
        
        this.FirstName = this.FirstNameTextBox.Text;
        this.LastName = this.LastNameTextBox.Text;
        this.DataBaseUpdater();
    }

    private void DataBaseUpdater()
    {
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using SQLiteCommand cmd = new SQLiteCommand(@"
                    INSERT INTO customer 
                        (customerId, customerName, addressId, active, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@customerId, @customerName, @addressId, @active, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)
                    ON CONFLICT(customerId) DO UPDATE SET
                        customerName = excluded.customerName, 
                        addressId = excluded.addressId, 
                        active = excluded.active, 
                        createDate = createDate, 
                        createdBy = createdBy, 
                        lastUpdate = excluded.lastUpdate, 
                        lastUpdateBy = excluded.lastUpdateBy", conn);
        cmd.Parameters.AddWithValue("@customerId", this.ID);
        cmd.Parameters.AddWithValue("@customerName", this.Name);
        cmd.Parameters.AddWithValue("@addressId", this.AddressId);
        cmd.Parameters.AddWithValue("@active", 1);
        cmd.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@createdBy", "admin");
        cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@lastUpdateBy", "admin");

        cmd.ExecuteNonQuery();
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
