using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public partial class CustomerForm : Window
{
    public string FirstName { get; set; }

    public CustomerForm()
    {
        InitializeComponent();
        this.DataContext = this;
        this.FirstName = "";
    }

    public CustomerForm(int customerId, string currentUsername)
    {
        InitializeComponent();
        //testLabel.Content = WGU_C969_Software_II_CS.Resources.Localization.test;
        this.DataContext = this;
        this.FirstName = "";
        this.FirstNameLabel.Content = WGU_C969_Software_II_CS.Resources.CustomerForm.FirstNameLabel;
    }

    public void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        BindingExpression firstNameBinding = FirstNameTextBox.GetBindingExpression(TextBox.TextProperty);
        firstNameBinding?.UpdateSource();

        if (Validation.GetHasError(FirstNameTextBox))
        {
            Console.WriteLine("Failed");
        }
        else if (!Validation.GetHasError(FirstNameTextBox))
        {
            Console.WriteLine("Passed");
        }
    }
}

public class NameValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        if (value.ToString().Length == 0)
        {
            return new ValidationResult(false, "Name Required");
        }

        return ValidationResult.ValidResult;
    }
}
