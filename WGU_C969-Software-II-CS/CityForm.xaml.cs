using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CityForm : INotifyPropertyChanged
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
    // ReSharper disable once UnusedAutoPropertyAccessor.Local

    // ReSharper disable once InconsistentNaming
    private int SelectedCountryId = -1;
    public int SelectedCountryIndex
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (this.Countries == null)
            {
                return -1;
            }
            return this.Countries.FindIndex(c => c.ID == SelectedCountryId);
        }
        set
        {
            if (value > this.Countries.Count)
            {
                int i = this.Countries.FindIndex(c => c.ID == value);
                if (i >= 0)
                {
                    SelectedCountryId = value;
                    this.CountriesComboBox.SelectedIndex = i;
                    return;
                }
            }
            SelectedCountryId = this.Countries[value].ID;
            OnPropertyChanged(nameof(SelectedCountryIndex));
        }
    }
    
    private AdvancedList<CountryRecord> Countries { get; set; }
    
    public CityForm(int cityId, string currentUsername)
    {
        this.DataContext = this;
        InitializeComponent();
        
        this.ID = cityId;
        this.CurrentUsername = currentUsername;
        this.Countries = new AdvancedList<CountryRecord>(this.RenderCountriesComboBox);
        this.ReadCountries();

        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM  city WHERE cityId = @id", conn);
        cmd.Parameters.AddWithValue("@id", this.ID);
        using (SQLiteDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                this.CityName = reader["city"].ToString() ?? "";
                this.SelectedCountryId = int.Parse(reader["countryId"].ToString() ?? "-1");
                OnPropertyChanged(nameof(SelectedCountryIndex));
            }
        }

        this.CityNameLabel.Content = WGU_C969_Software_II_CS.Resources.CityFormLocal.CityNameLabel;
        this.CountryLabel.Content = WGU_C969_Software_II_CS.Resources.CityFormLocal.CountryComboBoxLabel;
        this.DoneButton.Content = WGU_C969_Software_II_CS.Resources.CustomerFormLocal.DoneButton;
    }
    
    private void ReadCountries()
    {
        this.Countries = new AdvancedList<CountryRecord>(this.RenderCountriesComboBox);
        
        using SQLiteConnection conn = new SQLiteConnection(MainWindow.ConnectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM country", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            this.Countries.Add(new CountryRecord(
                int.Parse(reader["countryId"].ToString() ?? ""), 
                reader["country"].ToString() ?? "")
            );
        }
    }
    
    private void RenderCountriesComboBox()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.Countries == null) { return; }
        this.CountriesComboBox.ItemsSource = (List<string>)this.Countries;
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
        
        this.CityName = this.CityNameTextBox.Text;
        this.SelectedCountryId = Countries[CountriesComboBox.SelectedIndex].ID;
        
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
                    INSERT INTO city 
                        (cityId, city, countryId, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@cityId, @city, @countryId, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)
                    ON CONFLICT(cityId) DO UPDATE SET
                        city = excluded.city, 
                        countryId = excluded.countryId, 
                        createDate = createDate, 
                        createdBy = createdBy, 
                        lastUpdate = excluded.lastUpdate, 
                        lastUpdateBy = excluded.lastUpdateBy", conn);
        cmd.Parameters.AddWithValue("@cityId", this.ID);
        cmd.Parameters.AddWithValue("@city", this.CityName);
        cmd.Parameters.AddWithValue("@countryId", this.SelectedCountryId);
        cmd.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@createdBy", this.CurrentUsername);
        cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@lastUpdateBy", this.CurrentUsername);

        cmd.ExecuteNonQuery();
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
        return this.CityName; //Add country code call when country added
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}