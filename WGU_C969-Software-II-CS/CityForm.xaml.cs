namespace WGU_C969_Software_II_CS;

public partial class CityForm
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string CurrentUsername { get; set; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string CityName { get; set; } = "";
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private int CountryId { get; set; }
    
    public CityForm(int cityId, string currentUsername)
    {
        InitializeComponent();
        this.ID = cityId;
        this.CurrentUsername = currentUsername;
    }
    
    public CityForm(int cityId, string currentUsername, string cityName, int countryId)
    {
        InitializeComponent();
        this.ID = cityId;
        this.CurrentUsername = currentUsername;

        this.CityName = cityName;
        this.CountryId = countryId;
    }
    
    public static implicit operator string(CityForm city)
    {
        return city.CityName; //Do more
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
}