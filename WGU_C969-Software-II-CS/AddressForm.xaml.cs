using MySqlConnector;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class AddressForm : INotifyPropertyChanged, IDatabaseInteraction
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; }
    private string CurrentUsername { get; }
    
    private string _addressOne = "";
    public string AddressOne
    {
        get => _addressOne;
        set
        {
            _addressOne = value;
            Console.WriteLine($"AddressOne Changed {value}");
            OnPropertyChanged(nameof(AddressOne));
        }
    }
    
    private string AddressTwo = "";
    
    // ReSharper disable once InconsistentNaming
    private CityRecord _selectedCity;
    public CityRecord SelectedCity
    {
        get => _selectedCity;
        set
        {
            _selectedCity = value;
            Console.WriteLine($"SelectedCity Changed {value}");
            OnPropertyChanged(nameof(SelectedCity));
        }
    }
    private List<CityForm> Cities { get; set; }
    
    private string _postalCode = "";
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
            Console.WriteLine($"PostalCode Changed {value}");
            OnPropertyChanged(nameof(PostalCode));
        }
    }
    
    private PhoneClass HomePhone { get; set; }  = new PhoneClass();
    public string PhoneNumberString
    {
        get => HomePhone?.ToString() ?? "";
        set
        {
            HomePhone.Validate(value.ToString(), CultureInfo.CurrentCulture);
            Console.WriteLine($"Phone Number String Change: {value}");
            OnPropertyChanged(nameof(PhoneNumberString));
        }
    }
    
    public AddressForm(int addressId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = addressId;
        this.CurrentUsername = currentUsername;
        this.Cities = new List<CityForm>();
        this.ReadCities();

        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand($"SELECT * FROM  address WHERE addressId = @id", connection))
            {
                command.Parameters.AddWithValue("@id", this.ID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        this.AddressOne = reader.GetString("address");
                        this.AddressTwo = reader.GetString("address2");
                        this.AddressTwoTextBox.Text = this.AddressTwo;
                        this.SelectedCity = this.Cities.Find(i => i.ID == reader.GetInt16("cityId")).ToCityRecord();
                        this.PostalCode = reader.GetString("postalCode");
                        this.HomePhone.Validate(reader.GetString("phone"), CultureInfo.CurrentCulture);
                        OnPropertyChanged(nameof(PhoneNumberString));
                    }
                }
            }
        }
            

        this.AddressOneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressOneLabel;
        this.AddressTwoLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressTwoLabel;
        this.CityLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityLabel;
        this.CityAddButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityAddButton;
        this.CityModButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityModButton;
        this.CityDeleteButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityDeleteButton;
        this.PostalCodeLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.PostalCodeLabel;
        this.HomePhoneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.HomePhoneLabel;
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }

    private void ReadCities()
    {
        this.Cities = new List<CityForm>();
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using var command = new MySqlCommand("SELECT * FROM city", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            this.Cities.Add(new CityForm(reader.GetInt16("cityId"), this.CurrentUsername));
        }

        List<CityRecord> cities = new List<CityRecord>();
        foreach (CityForm cityForm in this.Cities)
        {
            cities.Add(cityForm.ToCityRecord());
        }
        
        this.CitiesComboBox.ItemsSource = cities;
    }

    private void CityPushButtonClicked(int id)
    {
        CityForm newCity = new CityForm(id, this.CurrentUsername);
        newCity.ShowDialog();
        this.ReadCities();
        this.CitiesComboBox.SelectedIndex = this.Cities.FindIndex(c => c.ID == newCity.ID);
    }

    private void CityAddButtonClicked(object sender, RoutedEventArgs e)
    {
        int cityId;
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand("SELECT IFNULL(MAX(cityId), 0) + 1 FROM city;", connection))
            {
                cityId = Convert.ToInt32(command.ExecuteScalar());
            }
        }
        this.CityPushButtonClicked(cityId);
    }

    private void CityModButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? citiesComboBinding = CitiesComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (citiesComboBinding != null)
        {
            if (CitiesComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(citiesComboBinding,
                    new ValidationError(new ExceptionValidationRule(), citiesComboBinding));
                return;
            }
            else
            {
                Validation.ClearInvalid(citiesComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind CitiesComboBox");
        }
        CityPushButtonClicked(this.Cities[this.CitiesComboBox.SelectedIndex].ID);
    }

    private void CityDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        if (this.CitiesComboBox.SelectedIndex == -1)
        {
            return;
        }
        
        MessageBoxResult result = MessageBox.Show(
            $"{WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirm} {this.Cities[this.CitiesComboBox.SelectedIndex]}",
            WGU_C969_Software_II_CS.Resources.MainWindow.DeleteConfirmTitle, 
            MessageBoxButton.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(
                           "DELETE FROM city WHERE cityId = @id;",
                           connection))
                {
                    command.Parameters.AddWithValue("@id", this.Cities[this.CitiesComboBox.SelectedIndex].ID);
                    command.ExecuteNonQuery();
                }
            }
            this.ReadCities();
        }
    }

    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? addressOneBinding = AddressOneTextBox.GetBindingExpression(TextBox.TextProperty);
        addressOneBinding?.UpdateSource();

        this.AddressTwo = this.AddressTwoTextBox.Text;
        
        BindingExpression? citiesComboBinding = CitiesComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (citiesComboBinding != null)
        {
            if (SelectedCity == null)
            {
                Validation.MarkInvalid(citiesComboBinding,
                    new ValidationError(new ExceptionValidationRule(), citiesComboBinding));
            }
            else
            {
                Validation.ClearInvalid(citiesComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind CitiesComboBox");
        }
        
        BindingExpression? postalCodeBinding = PostalCodeTextBox.GetBindingExpression(TextBox.TextProperty);
        postalCodeBinding?.UpdateSource();
        
        BindingExpression? homePhoneBinding = HomePhoneTextBox.GetBindingExpression(TextBox.TextProperty);
        homePhoneBinding?.UpdateSource();

        ValidationResult phoneValidation = this.HomePhone.Validate(HomePhoneTextBox.Text, CultureInfo.CurrentCulture);
        if (!phoneValidation.IsValid)
        {
            return;
        }
        this.HomePhoneTextBox.Text = this.HomePhone.ToString();

        if (addressOneBinding is { HasError: true } ||
            citiesComboBinding is { HasError: true } ||
            postalCodeBinding is { HasError: true } ||
            this.HomePhone.ToString() == "")
        {
            return;
        }
        
        this.DataBaseUpdater();
        this.DialogResult = true;
        this.Close();
    }
    
    private void DataBaseUpdater()
    {
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        // ReSharper disable once UseRawString
        using MySqlCommand command = new MySqlCommand(@"
                    INSERT INTO address 
                        (addressId, address, address2, cityId, postalCode, phone, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@addressId, @address, @address2, @cityId, @postalCode, @phone, @createDate, @createdBy, @lastUpdate, @lastUpdateBy) AS new
                    ON DUPLICATE KEY UPDATE
                        address = new.address,
                        address2 = new.address2,
                        cityId = new.cityId,
                        postalCode = new.postalCode,
                        phone = new.phone,
                        lastUpdate = new.lastUpdate,
                        lastUpdateBy = new.lastUpdateBy", connection);
        command.Parameters.AddWithValue("@addressId", this.ID);
        command.Parameters.AddWithValue("@address", this.AddressOne.Trim());
        command.Parameters.AddWithValue("@address2", this.AddressTwo.Trim());
        command.Parameters.AddWithValue("@cityId", this.SelectedCity.ID);
        command.Parameters.AddWithValue("@postalCode", this.PostalCode.Trim());
        command.Parameters.AddWithValue("@phone", (string)this.HomePhone);
        command.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        command.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        command.ExecuteNonQuery();
    }
    
    public override string ToString()
    {
        string output = $"{this.AddressOne}";
        if (this.AddressTwo != "")
        {
            output += $", {AddressTwo}";
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.SelectedCity != null && this.PostalCode != "")
        {
            output += $", {SelectedCity} {this.PostalCode}";
        }
        if (this.HomePhone.ToString() != "")
        {
            // ReSharper disable once RedundantToStringCall
            output += $", {this.HomePhone.ToString()}";
        }

        return output;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class PostalValidator : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cuture)
    {
        int code = 0;
        if (!int.TryParse(value?.ToString(), out code))
        {
            return new ValidationResult(false, "Must be a number");
        }

        if (code.ToString().ToList() is not { Count: 5 })
        {
            return new ValidationResult(false, "Must be a 5 digit postal code");
        }

        return ValidationResult.ValidResult;
    }
}