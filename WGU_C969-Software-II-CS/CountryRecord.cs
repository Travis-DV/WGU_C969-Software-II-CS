namespace WGU_C969_Software_II_CS;

// ReSharper disable twice InconsistentNaming
public record CountryRecord
{
    // ReSharper disable once InconsistentNaming
    public int ID { get; init; }
    
    private string _countryName = "";
    public string CountryName
    {
        get => _countryName;
        set => _countryName = value ?? throw new ArgumentNullException(nameof(value));
    }

    public CountryRecord(int id, string countryName)
    {
        this.ID = id;
        this._countryName = countryName;
    }

    public override string ToString()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.CountryName == null)
        {
            return "";
        }
        return this.CountryName; //Add country code call when country added
    }
}