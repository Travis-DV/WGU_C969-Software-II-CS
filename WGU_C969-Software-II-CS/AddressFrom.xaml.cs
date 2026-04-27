using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class AddressFrom : Window
{

    public int ID { get; }
    private string CurrentUsername { get; }
    public string AddressOne { get; set; }
    public string AddressTwo { get; set; }
    public string PostalCode { get; set; }
    public PhoneClass HomePhone { get; set; }
    public int SelectedCityId { get; set; }
    private AdvancedList<CityForm> Cities { get; set; }
    
    public AddressFrom(int addressId, string currentUsername)
    {
        InitializeComponent();
        this.DataContext = this;
        this.ID = addressId;
        this.CurrentUsername = currentUsername;
        this.HomePhone = new PhoneClass();
        
        Dictionary<DataEnum, string> databaseResults = DatabaseAPI.Pull(DataEnum.AddressForm, this.ID);

        this.AddressOneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressOneLabel;
        this.AddressOneTextBox.Text = databaseResults[DataEnum.AddressOne];
        this.AddressTwoLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressOneLabel;
        this.AddressTwoTextBox.Text = databaseResults[DataEnum.AddressOne];
        this.CityLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityLabel;
        this.Cities = DatabaseAPI.ReturnCities(this.CurrentUsername, this.RenderCitiesComboBox);
        this.Cities.Add(new CityForm(10, this.CurrentUsername)
        {
            CityName = "Something cool",
            CountryId = 1,
        });
        this.CityAddButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityAddButton;
        this.CityModButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityModButton;
        this.CityDeleteButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityDeleteButton;
        this.PostalCodeLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.PostalCodeLabel;
        this.PostalCodeTextBox.Text = databaseResults[DataEnum.PostalCode];
        this.HomePhoneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.HomePhoneLabel;
        this.HomePhoneTextBox.Text = databaseResults[DataEnum.PhoneNumber];
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }
    
    private void RenderCitiesComboBox()
    {
        if (Cities == null) { return; }
        this.CitiesComboBox.ItemsSource = (List<string>)this.Cities;
    }

    private void CityPushButtonClicked(int id)
    {
        CityForm newCity = new CityForm(id, this.CurrentUsername)
        {
            Owner = this
        };
        newCity.Show();
        DatabaseAPI.Push(new Dictionary<DataEnum, string>
        {
            { DataEnum.ID, newCity.ID.ToString() },
            { DataEnum.Name, newCity.CityName },
            { DataEnum.CountryID, newCity.CountryId.ToString() }
        });
        this.Cities = DatabaseAPI.ReturnCities(this.CurrentUsername, this.RenderCitiesComboBox);
    }
    
    public void CityAddButtonClicked(object sender, RoutedEventArgs e)
    {
        this.CityPushButtonClicked(-1);
    }
    
    public void CityModButtonClicked(object sender, RoutedEventArgs e)
    {
        CityPushButtonClicked(this.CitiesComboBox.SelectedIndex);
    }
    
    public void CityDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        Console.WriteLine(this.CitiesComboBox.SelectedIndex.ToString());
        if (this.CitiesComboBox.SelectedIndex == -1)
        {
            return;
        }
        
        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to remove {this.Cities[this.CitiesComboBox.SelectedIndex].CityName}",
            "Confirm Deletion", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            DatabaseAPI.Remove(DataEnum.CityForm, this.Cities[this.CitiesComboBox.SelectedIndex].ID);
            this.Cities = DatabaseAPI.ReturnCities(this.CurrentUsername, this.RenderCitiesComboBox);
        }
    }
    
    public void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression addressOneBinding = AddressOneTextBox.GetBindingExpression(TextBox.TextProperty);
        Console.WriteLine(this.AddressOne);
        addressOneBinding?.UpdateSource();
        Console.WriteLine(this.AddressOne);
        if (!addressOneBinding.HasError) { this.AddressOne = this.AddressOneTextBox.Text; }
        if (this.AddressTwoTextBox.Text != "") { this.AddressTwo = this.AddressOneTextBox.Text; }
        Console.WriteLine(this.SelectedCityId.ToString());
        BindingExpression citiesComboBinding = CitiesComboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
        if (CitiesComboBox.SelectedIndex == -1)
        {
            Validation.MarkInvalid(citiesComboBinding, new ValidationError(new ExceptionValidationRule(), citiesComboBinding));
        }
        else
        {
            Validation.ClearInvalid(citiesComboBinding);
        }
        BindingExpression postalCodeBinding = PostalCodeTextBox.GetBindingExpression(TextBox.TextProperty);
        postalCodeBinding?.UpdateSource();
        if (!postalCodeBinding.HasError) { this.PostalCode = this.PostalCodeTextBox.Text; }

        if (HomePhoneTextBox.Text != "")
        {
            BindingExpression homePhoneBinding = HomePhoneTextBox.GetBindingExpression(TextBox.TextProperty);
            homePhoneBinding?.UpdateSource();
            if (!homePhoneBinding.HasError)
            {
                this.HomePhone.Validate(HomePhoneTextBox.Text, CultureInfo.CurrentCulture);
                this.HomePhoneTextBox.Text = this.HomePhone.ToString();
            }
        }
    }

    
    
    public override string ToString()
    {
        return "Not Implemented";
    }
}

public class PostalValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        if (!int.TryParse(value.ToString(), out _))
        {
            return new ValidationResult(false, "Must be a number");
        }

        if (value.ToString().Length < 5)
        {
            return new ValidationResult(false, "Must be a 5 digit postal code");
        }

        return ValidationResult.ValidResult;
    }
}