using System.Data.SQLite;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : Window
{
    private int ID { get; }
    private string CurrentUsername { get; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    private int AddressId { get; set; }
    private bool _AddressMod { get; set; }
    private bool AddressMod
    {
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
    public AddressFrom Address { get; set; }
    public PhoneClass PhoneNumber { get; set; }

    public CustomerForm(int customerId, string currentUsername)
    {
        InitializeComponent();
        //testLabel.Content = WGU_C969_Software_II_CS.Resources.Localization.test;
        this.DataContext = this;
        this.ID = customerId;
        this.CurrentUsername = currentUsername;
        this.PhoneNumber = new PhoneClass();
        
        using (SQLiteConnection conn = new SQLiteConnection(DatabaseAPI.connectionString))
        using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM  customer WHERE customerId == {this.ID}", conn))
        using (SQLiteDataReader reader = cmd.ExecuteReader())
        {
            conn.Open();

            if (!reader.Read())
            {
                this.FirstName = "";
                this.LastName = "";
                this.AddressMod = false;
                this.AddressId = -1;
                this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
                this.AddressTextBox.Text = this.Address.ToString();
                this.PhoneNumber.Validate("", CultureInfo.CurrentCulture);
                return;
            }
            
            string[] customerName = reader["customerName"].ToString().Split(" ");
            this.FirstName = customerName[0];
            this.LastName = customerName[1];
            this.AddressId = int.Parse(reader["addressId"].ToString());
            this.AddressMod = true;
            this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
            this.AddressTextBox.Text = this.Address.ToString();
            this.PhoneNumber.Validate(reader["phoneNumber"].ToString(), CultureInfo.CurrentCulture);
        }
        
        
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.FirstNameLabel + ": ";
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.LastNameLabel + ": ";
        this.AddressLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressLabel + ": ";
        this.AddressClearButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.AddressClearButton;
        this.PhoneNumberLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.PhoneNumberLabel + ": ";
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }

    public void AddressModButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername)
        {
            Owner = this
        };
        this.Address.Show();
        this.AddressTextBox.Text = this.Address.ToString();
    }
    
    public void AddressClearButtonClicked(object sender, RoutedEventArgs e)
    {
        this.AddressId = -1;
        this.AddressMod = false;
        this.Address = new AddressFrom(this.AddressId, this.CurrentUsername);
        this.AddressTextBox.Text = this.Address.ToString();
    }
    
    public void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression firstNameBinding = FirstNameTextBox.GetBindingExpression(TextBox.TextProperty);
        firstNameBinding?.UpdateSource();

        BindingExpression lastNameBinding = LastNameTextBox.GetBindingExpression(TextBox.TextProperty);
        lastNameBinding?.UpdateSource();

        BindingExpression addressBinding = AddressTextBox.GetBindingExpression(TextBox.TextProperty);
        if (this.AddressTextBox.Text == "")
        {
            Validation.MarkInvalid(addressBinding, new ValidationError(new ExceptionValidationRule(), addressBinding));
        }
        else
        {
            Validation.ClearInvalid(addressBinding);
        }

        BindingExpression phoneNumberBinding = PhoneNumberTextBox.GetBindingExpression(TextBox.TextProperty);
        phoneNumberBinding?.UpdateSource();
        if (!phoneNumberBinding.HasError)
        {
            this.PhoneNumber.Validate(PhoneNumberTextBox.Text, CultureInfo.CurrentCulture);
            this.PhoneNumberTextBox.Text = this.PhoneNumber.ToString();
        }

        if (!firstNameBinding.HasError && !lastNameBinding.HasError && !addressBinding.HasError &&
            !phoneNumberBinding.HasError)
        {
            this.FirstName = this.FirstNameTextBox.Text;
            this.LastName = this.LastNameTextBox.Text;
            this.DataBaseUpdater();
        }
    }

    private void DataBaseUpdater()
    {
        using (SQLiteConnection conn = new SQLiteConnection(DatabaseAPI.connectionString))
        using (SQLiteCommand cmd = new SQLiteCommand(@"
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
                    lastUpdateBy = excluded.lastUpdateBy", conn))
        {
            conn.Open();
            
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
}

public class BasicTextValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        if (value == null)
        {
            return new ValidationResult(false, "Value is null");
        }
        if (value.ToString().Length == 0)
        {
            return new ValidationResult(false, "Required");
        }

        return ValidationResult.ValidResult;
    }
}
