using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class AddressFrom
{

    // ReSharper disable once InconsistentNaming
    private int ID { get; }
    private string CurrentUsername { get; }
    private string AddressOne { get; set; } = "";
    private string AddressTwo { get; set; } = "";
    private int SelectedCityId { get; set; } = -1;
    private string PostalCode { get; set; } = "";
    private PhoneClass HomePhone { get; set; }  = new PhoneClass();
    private AdvancedList<CityForm> Cities { get; set; }
    
    public AddressFrom(int addressId, string currentUsername)
    {
        InitializeComponent();
        this.DataContext = this;
        
        this.ID = addressId;
        this.CurrentUsername = currentUsername;
        this.Cities = new AdvancedList<CityForm>(this.RenderCitiesComboBox);
        this.ReadCities();

        using (SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString))
        {
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM  address WHERE addressId == {this.ID}", conn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    this.AddressOne = reader["address"].ToString() ?? "";
                    this.AddressTwo = reader["address2"].ToString() ?? "";
                    this.SelectedCityId = int.Parse(reader["cityId"].ToString() ?? "-1");
                    this.CitiesComboBox.SelectedIndex = this.SelectedCityId;
                    this.PostalCode = reader["postalCode"].ToString() ?? "";
                    this.HomePhone.Validate(reader["phone"].ToString() ?? "", CultureInfo.CurrentCulture);
                }
            }
        }
            

        this.AddressOneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressOneLabel;
        this.AddressOneTextBox.Text = this.AddressOne;
        this.AddressTwoLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.AddressOneLabel;
        this.AddressTwoTextBox.Text = this.AddressTwo;
        this.CityLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityLabel;
        this.CityAddButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityAddButton;
        this.CityModButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityModButton;
        this.CityDeleteButton.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.CityDeleteButton;
        this.PostalCodeLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.PostalCodeLabel;
        this.PostalCodeTextBox.Text = this.PostalCode;
        this.HomePhoneLabel.Content = WGU_C969_Software_II_CS.Resources.AddressFormLocal.HomePhoneLabel;
        this.HomePhoneTextBox.Text = this.HomePhone.ToString();
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }

    private void ReadCities()
    {
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM city", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            this.Cities.Add(new CityForm(int.Parse(reader["cityId"].ToString() ?? ""), this.CurrentUsername, 
                reader["city"].ToString() ?? "",
                int.Parse(reader["countryId"].ToString() ?? "")));
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
        newCity.Show();
        // DatabaseAPI.Push(new Dictionary<DataEnum, string>
        // {
        //     { DataEnum.ID, newCity.ID.ToString() },
        //     { DataEnum.Name, newCity.CityName },
        //     { DataEnum.CountryID, newCity.CountryId.ToString() }
        // });
        this.ReadCities();
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
        CityPushButtonClicked(this.CitiesComboBox.SelectedIndex);
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
                           $"DELETE FROM city WHERE cityId = {this.Cities[this.CitiesComboBox.SelectedIndex].ID};",
                           conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            this.ReadCities();
        }
    }

    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? addressOneBinding = AddressOneTextBox.GetBindingExpression(TextBox.TextProperty);
        Console.WriteLine(this.AddressOne);
        addressOneBinding?.UpdateSource();
        Console.WriteLine(this.AddressOne);
        if (addressOneBinding is not { HasError: false }) { this.AddressOne = this.AddressOneTextBox.Text; }
        
        if (this.AddressTwoTextBox.Text != "") { this.AddressTwo = this.AddressOneTextBox.Text; }
        
        Console.WriteLine(this.SelectedCityId.ToString());
        BindingExpression? citiesComboBinding = CitiesComboBox.GetBindingExpression(Selector.SelectedItemProperty);
        if (CitiesComboBox.SelectedIndex == -1)
        {
            if (citiesComboBinding != null)
            {
                Validation.MarkInvalid(citiesComboBinding,
                    new ValidationError(new ExceptionValidationRule(), citiesComboBinding));
            }
        }
        else
        {
            if (citiesComboBinding != null) { Validation.ClearInvalid(citiesComboBinding); }
        }
        
        BindingExpression? postalCodeBinding = PostalCodeTextBox.GetBindingExpression(TextBox.TextProperty);
        postalCodeBinding?.UpdateSource();
        if (postalCodeBinding is not { HasError: false }) { this.PostalCode = this.PostalCodeTextBox.Text; }

        if (HomePhoneTextBox.Text == "") return;
        BindingExpression? homePhoneBinding = HomePhoneTextBox.GetBindingExpression(TextBox.TextProperty);
        homePhoneBinding?.UpdateSource();
        if (homePhoneBinding is not { HasError: false })
        {
            this.HomePhone.Validate(HomePhoneTextBox.Text, CultureInfo.CurrentCulture);
            this.HomePhoneTextBox.Text = this.HomePhone.ToString();
        }
    }
    
    public override string ToString()
    {
        string output = $"{this.AddressOne}";
        if (this.AddressTwo != "")
        {
            output += $", {AddressTwo}";
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.Cities != null && this.Cities.Count > this.SelectedCityId && this.SelectedCityId > -1)
        {
            output += $", {this.Cities[this.SelectedCityId]}";
        }
        if (this.PostalCode != "")
        {
            output += $", {this.PostalCode}";
        }
        if (this.HomePhone.ToString() != "")
        {
            // ReSharper disable once RedundantToStringCall
            output += $", {this.HomePhone.ToString()}";
        }

        return output;
    }
}

public class PostalValidator : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cuture)
    {
        if (!int.TryParse(value?.ToString(), out _))
        {
            return new ValidationResult(false, "Must be a number");
        }

        if (value.ToString() is { Length: < 5 })
        {
            return new ValidationResult(false, "Must be a 5 digit postal code");
        }

        return ValidationResult.ValidResult;
    }
}