using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WGU_C969_Software_II_CS;

public partial class AddressFrom : Window
{

    public int ID { get; set; }
    
    public AddressFrom(int customerId, string currentUsername)
    {
        InitializeComponent();
        
    }
    
    public override string ToString()
    {
        return "Not Implemented";
    }
}

public class AddressValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        return new ValidationResult(false, "Not implemented");
    }
}