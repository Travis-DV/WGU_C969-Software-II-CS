using System.Windows;

namespace WGU_C969_Software_II_CS;

public partial class CityForm : Window
{
    public int ID { get; }
    public string CityName { get; set; }
    public int CountryId { get; set; }
    
    public CityForm(int cityId, string currentUsername)
    {
        InitializeComponent();
        this.ID = cityId;
    }
    
    public static implicit operator string(CityForm city)
    {
        return city.CityName; //Do more
    }
    
    public override string ToString()
    {
        if (this.CityName == null)
        {
            return "";
        }
        return this.CityName; //Add country code call when country added
    }
}