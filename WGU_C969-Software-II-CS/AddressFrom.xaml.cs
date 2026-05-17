using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class AddressFrom : INotifyPropertyChanged, IDatabaseInteraction
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; init; }
    private string CurrentUsername { get; }
    
    private string _addressOne = "";
    public string AddressOne
    {
        get => _addressOne;
        set
        {
            _addressOne = value;
            OnPropertyChanged(nameof(AddressOne));
        }
    }
    
    private string _addressTwo = "";
    public string AddressTwo
    {
        get => _addressTwo;
        set
        {
            _addressTwo = value;
            OnPropertyChanged(nameof(AddressTwo));
        }
    }
    
    // ReSharper disable once InconsistentNaming
    private int SelectedCityId = -1;
    public int SelectedCityIndex
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (this.Cities == null)
            {
                return -1;
            }
            return this.Cities.FindIndex(c => c.ID == SelectedCityId);
        }
        set
        {
            int i = value;
            if (value > this.Cities.Count)
            {
                i = this.Cities.FindIndex(c => c.ID == value);
                Console.WriteLine("Value greater than cities");
            }
            SelectedCityId = this.Cities[i].ID;
            OnPropertyChanged(nameof(SelectedCityIndex));
        }
    }
    
    private string _postalCode = "";
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
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
            OnPropertyChanged(nameof(PhoneNumberString));
            Console.WriteLine($"Phone Number String Change: {value}");
        }
    }
    private AdvancedList<CityForm> Cities { get; set; }
    
    public AddressFrom(int addressId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = addressId;
        this.CurrentUsername = currentUsername;
        this.Cities = new AdvancedList<CityForm>(this.RenderCitiesComboBox);
        this.ReadCities();

        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM  address WHERE addressId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", this.ID);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        this.AddressOne = reader["address"].ToString() ?? "";
                        this.AddressTwo = reader["address2"].ToString() ?? "";
                        this.SelectedCityId = int.Parse(reader["cityId"].ToString() ?? "-1");
                        OnPropertyChanged(nameof(SelectedCityIndex));
                        this.PostalCode = reader["postalCode"].ToString() ?? "";
                        this.HomePhone.Validate(reader["phone"].ToString() ?? "", CultureInfo.CurrentCulture);
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
        this.Cities = new AdvancedList<CityForm>(this.RenderCitiesComboBox);
        
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM city", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            this.Cities.Add(new CityForm(int.Parse(reader["cityId"].ToString() ?? ""), this.CurrentUsername));
        }
    }
    
    private void RenderCitiesComboBox()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.Cities == null) { return; }
        this.CitiesComboBox.ItemsSource = (List<string>)this.Cities;
    }

    private void CityPushButtonClicked(int id)
    {
        CityForm newCity = new CityForm(id, this.CurrentUsername)
        {
            Owner = this
        };
        newCity.ShowDialog();
        this.ReadCities();
        this.CitiesComboBox.SelectedIndex = this.Cities.FindIndex(c => c.ID == newCity.ID);
    }

    private void CityAddButtonClicked(object sender, RoutedEventArgs e)
    {
        int cityId;
        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT IFNULL(MAX(cityId), 0) + 1 FROM city;", conn))
            {
                cityId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        this.CityPushButtonClicked(cityId);
    }

    private void CityModButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? citiesComboBinding = CitiesComboBox.GetBindingExpression(Selector.SelectedIndexProperty);
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
            $"Are you sure you want to remove {this.Cities[this.CitiesComboBox.SelectedIndex]}",
            "Confirm Deletion", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(
                           $"DELETE FROM city WHERE cityId = @id;",
                           conn))
                {
                    cmd.Parameters.AddWithValue("@id", this.Cities[this.CitiesComboBox.SelectedIndex].ID);
                    cmd.ExecuteNonQuery();
                }
            }
            this.ReadCities();
        }
    }

    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? addressOneBinding = AddressOneTextBox.GetBindingExpression(TextBox.TextProperty);
        addressOneBinding?.UpdateSource();
        
        BindingExpression? citiesComboBinding = CitiesComboBox.GetBindingExpression(Selector.SelectedIndexProperty);
        if (citiesComboBinding != null)
        {
            if (CitiesComboBox.SelectedIndex < 0)
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
        
        if (HomePhoneTextBox.Text != "")
        {
            BindingExpression? homePhoneBinding = HomePhoneTextBox.GetBindingExpression(TextBox.TextProperty);
            homePhoneBinding?.UpdateSource();
            if (homePhoneBinding is { HasError: false })
            {
                this.HomePhone.Validate(HomePhoneTextBox.Text, CultureInfo.CurrentCulture);
                this.HomePhoneTextBox.Text = this.HomePhone.ToString();
            }
            else
            {
                this.HomePhone.Validate("0 000 000 0000", CultureInfo.CurrentCulture);
            }
        }

        if (addressOneBinding is { HasError: true } ||
            citiesComboBinding is { HasError: true } ||
            postalCodeBinding is { HasError: true } ||
            this.HomePhone.ToString() == "")
        {
            return;
        }
        
        this.AddressOne = this.AddressOneTextBox.Text;
        this.AddressTwo = this.AddressTwoTextBox.Text;
        this.SelectedCityId = Cities[CitiesComboBox.SelectedIndex].ID;
        this.PostalCode = this.PostalCodeTextBox.Text;
        
        this.DataBaseUpdater();
        this.DialogResult = true;
        this.Close();
    }
    
    private void DataBaseUpdater()
    {
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        // ReSharper disable once UseRawString
        using SQLiteCommand cmd = new SQLiteCommand(@"
                    INSERT INTO address 
                        (addressId, address, address2, cityId, postalCode, phone, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@addressId, @address, @address2, @cityId, @postalCode, @phone, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)
                    ON CONFLICT(addressId) DO UPDATE SET
                        address = excluded.address, 
                        address2 = excluded.address2, 
                        cityId = excluded.cityId, 
                        postalCode = excluded.postalCode, 
                        phone = excluded.phone, 
                        createDate = createDate, 
                        createdBy = createdBy, 
                        lastUpdate = excluded.lastUpdate, 
                        lastUpdateBy = excluded.lastUpdateBy", conn);
        cmd.Parameters.AddWithValue("@addressId", this.ID);
        cmd.Parameters.AddWithValue("@address", this.AddressOne);
        cmd.Parameters.AddWithValue("@address2", this.AddressTwo);
        cmd.Parameters.AddWithValue("@cityId", this.SelectedCityId);
        cmd.Parameters.AddWithValue("@postalCode", this.PostalCode);
        cmd.Parameters.AddWithValue("@phone", this.HomePhone);
        cmd.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        cmd.ExecuteNonQuery();
    }
    
    public override string ToString()
    {
        string output = $"{this.AddressOne}";
        if (this.AddressTwo != "")
        {
            output += $", {AddressTwo}";
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.Cities != null && this.Cities.Count > this.SelectedCityIndex && this.SelectedCityIndex > -1 && this.PostalCode != "")
        {
            output += $", {this.Cities[this.SelectedCityIndex]} {this.PostalCode}";
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

        if (code.ToString() is { Length: < 5 })
        {
            return new ValidationResult(false, "Must be a 5 digit postal code");
        }

        return ValidationResult.ValidResult;
    }
}