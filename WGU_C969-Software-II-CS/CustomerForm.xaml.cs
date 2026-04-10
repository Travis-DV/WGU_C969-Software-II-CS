using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : Window
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public PhoneClass phoneNumber { get; set; }

    public CustomerForm(int customerId, string currentUsername)
    {
        InitializeComponent();
        //testLabel.Content = WGU_C969_Software_II_CS.Resources.Localization.test;
        this.DataContext = this;
        this.FirstName = "";
        this.LastName = "";
        this.phoneNumber = new PhoneClass();
        this.phoneNumber.ImportFromString("+100 (314) 478-9031");
        this.phoneNumber.ImportFromString("13144789031");
        this.phoneNumber.ImportFromString("+34 (314) 478-9031 asdf sf");
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.FirstNameLabel + ": ";
        this.LastNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.LastNameLabel + ": ";
    }

    public void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression firstNameBinding = FirstNameTextBox.GetBindingExpression(TextBox.TextProperty);
        firstNameBinding?.UpdateSource();

        BindingExpression lastNameBinding = LastNameTextBox.GetBindingExpression(TextBox.TextProperty);
        lastNameBinding?.UpdateSource();
    }
}

public class NameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        if (value.ToString().Length == 0)
        {
            return new ValidationResult(false, "Required");
        }

        return ValidationResult.ValidResult;
    }
}
