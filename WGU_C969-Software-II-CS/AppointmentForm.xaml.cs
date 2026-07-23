using System.ComponentModel;
using System.Windows;

namespace WGU_C969_Software_II_CS;

public partial class AppointmentForm : INotifyPropertyChanged
{
    // ReSharper disable once InconsistentNaming
    private int ID { get; init; }
    private string CurrentUsername { get; }
    
    private string _appointmentTitle = "";
    public string AppointmentTitle
    {
        get => _appointmentTitle;
        set
        {
            _appointmentTitle = value;
            OnPropertyChanged(nameof(AppointmentTitle));
            Console.WriteLine($"Appointment Title Change: {value}");
        } 
    }
    
    private string _description = "";
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
            Console.WriteLine($"Description Change: {value}");
        } 
    }
    
    // ReSharper disable once InconsistentNaming
    private List<string> Locations { get; set; }
    private string _selectedLocation = "";
    public string SelectedLocation
    {
        get => _selectedLocation;
        set
        {
            _selectedLocation = value;
            OnPropertyChanged(nameof(SelectedLocation));
            Console.WriteLine($"Selected Location Change: {value}");
        } 
    }
    
    private string _contact = "";
    public string Contact
    {
        get => _contact;
        set
        {
            _contact = value;
            OnPropertyChanged(nameof(Contact));
            Console.WriteLine($"Contact Change: {value}");
        } 
    }
    
    private List<string> Types { get; set; }
    private string _selectedType = "";
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged(nameof(SelectedType));
            Console.WriteLine($"Selected Location Change: {value}");
        } 
    }
    
    private string _url = "";
    public string url
    {
        get => _url;
        set
        {
            _url = value;
            OnPropertyChanged(nameof(url));
            Console.WriteLine($"Description Change: {value}");
        } 
    }
    
    private DateTime AppointmentTime { get; set; }
    
    public AppointmentForm(int customerId, string currentUsername)
    {
        InitializeComponent();
    }
    
    private void MyTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged)
        {
            double newHeight = e.NewSize.Height;
            DoneButton.Width = newHeight * 2;
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public enum AppointmentParts
{
    CustomerId,
    Title,
    Description,
    Location,
    Type,
    DateTime
}