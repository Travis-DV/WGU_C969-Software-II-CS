using MySqlConnector;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CityForm : INotifyPropertyChanged, IDatabaseInteraction
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string CurrentUsername { get; set; }
    
    private string _cityName = "";
    public string CityName
    {
        get => _cityName;
        set
        {
            _cityName = value;
            OnPropertyChanged(nameof(CityName));
        }
    }

    private CountryRecord _selectedCountry;
    public CountryRecord SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            _selectedCountry = value;
            Console.WriteLine($"SelectedCountry Changed {value}");
            OnPropertyChanged(nameof(SelectedCountry));
        }
    }
    
    private List<CountryRecord> Countries { get; set; }
    
    public CityForm(int cityId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = cityId;
        this.CurrentUsername = currentUsername;
        this.Countries = new List<CountryRecord>();
        this.ReadCountries();
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand($"SELECT * FROM  city WHERE cityId = @id", connection);
        command.Parameters.AddWithValue("@id", this.ID);
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                this.CityName = reader.GetString("city");
                this.SelectedCountry = this.Countries.Find(i => i.ID == reader.GetInt16("countryId"));
                OnPropertyChanged(nameof(SelectedCountry));
            }
        }

        this.CityNameLabel.Content = WGU_C969_Software_II_CS.Resources.CityFormLocal.CityNameLabel;
        this.CountryLabel.Content = WGU_C969_Software_II_CS.Resources.CityFormLocal.CountryComboBoxLabel;
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }
    
    private void ReadCountries()
    {
        this.Countries = new List<CountryRecord>();
        
        using MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString);
        connection.Open();
        using MySqlCommand command = new MySqlCommand("SELECT * FROM country", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            this.Countries.Add(new CountryRecord(
                reader.GetInt16("countryId"), 
                reader.GetString("country"))
            );
        }
        this.CountriesComboBox.ItemsSource = this.Countries;
    }
    
    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression? cityBinding = CityNameTextBox.GetBindingExpression(TextBox.TextProperty);
        cityBinding?.UpdateSource();
        
        BindingExpression? countriesComboBinding = CountriesComboBox.GetBindingExpression(Selector.SelectedIndexProperty);
        if (countriesComboBinding != null)
        {
            if (CountriesComboBox.SelectedIndex < 0)
            {
                Validation.MarkInvalid(countriesComboBinding,
                    new ValidationError(new ExceptionValidationRule(), countriesComboBinding));
            }
            else
            {
                Validation.ClearInvalid(countriesComboBinding);
            }
        }
        else
        {
            throw new Exception("Failed to bind CountriesComboBox");
        }
        
        if (cityBinding is { HasError: true } ||
            countriesComboBinding is { HasError: true })
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
                    INSERT INTO city 
                        (cityId, city, countryId, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@cityId, @city, @countryId, @createDate, @createdBy, @lastUpdate, @lastUpdateBy) AS new
                    ON DUPLICATE KEY UPDATE
                        city = new.city,
                        countryId = new.countryId,
                        lastUpdate = new.lastUpdate,
                        lastUpdateBy = new.lastUpdateBy", connection);
        command.Parameters.AddWithValue("@cityId", this.ID);
        command.Parameters.AddWithValue("@city", this.CityName);
        command.Parameters.AddWithValue("@countryId", this.SelectedCountry.ID);
        command.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        command.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        command.ExecuteNonQuery();
    }
    
    public static implicit operator string(CityForm city)
    {
        return city.ToString();
    }
    
    public override string ToString()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.CityName == null)
        {
            return "";
        }
        return $"{this.CityName}, {this.SelectedCountry.CountryName}"; //Add country code call when country added
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}