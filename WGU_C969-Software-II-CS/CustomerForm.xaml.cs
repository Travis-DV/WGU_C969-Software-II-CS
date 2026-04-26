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
    private string currentUsername { get; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    private int AddressId { get; set; }
    private bool _AddressMod { get; set; }
    private bool AddressMod
    {
        get
        {
            return _AddressMod;
        }
        set
        {
            _AddressMod = value;
            if (value)
            {
                this.AddressModButton.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.AddressModButton;
            }
            else if (!value)
            {
                this.AddressModButton.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.AddressAddButton;
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
        this.currentUsername = currentUsername;
        this.PhoneNumber = new PhoneClass();
        
        Dictionary<DataEnum, string> databaseResults = DatabaseAPI.CheckID(this, this.ID);

        string[] customerName = databaseResults[DataEnum.Name].Split(" ");
        this.FirstName = customerName[0];
        this.LastName = customerName[1];
        int tempAID = -1;
        this.AddressMod = false;
        if (int.TryParse(databaseResults[DataEnum.AddressID], out tempAID))
        {
            this.AddressId = tempAID;
            this.AddressMod = true;
        }
        this.Address = new AddressFrom(this.AddressId, this.currentUsername);
        this.AddressTextBox.Text = this.Address.ToString();
        this.PhoneNumber.Validate(databaseResults[DataEnum.PhoneNumber], CultureInfo.CurrentCulture);
        
        
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.FirstNameLabel + ": ";
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.LastNameLabel + ": ";
        this.AddressLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.AddressLabel + ": ";
        this.AddressClearButton.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.AddressClearButton;
        this.PhoneNumberLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.PhoneNumberLabel + ": ";
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.DoneButton;
    }

    public void AddressModButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Address = new AddressFrom(this.AddressId, this.currentUsername)
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
        this.Address = new AddressFrom(this.AddressId, this.currentUsername);
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
            Console.WriteLine(this.PhoneNumber.ToString());
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
        Dictionary<DataEnum, string> data = new Dictionary<DataEnum, string>();
        data.Add(DataEnum.ID, this.ID.ToString());
        data.Add(DataEnum.Name, this.FirstName + " " + this.LastName);
        data.Add(DataEnum.AddressID, this.Address.ID.ToString());
        data.Add(DataEnum.PhoneNumber, this.PhoneNumber);
        DatabaseAPI.UpdateDB(this, data);
    }
}

public class NameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        if (value.ToString().Length == 0)
        {
            return new ValidationResult(false, "Required");
        }

        return ValidationResult.ValidResult;
    }
}
