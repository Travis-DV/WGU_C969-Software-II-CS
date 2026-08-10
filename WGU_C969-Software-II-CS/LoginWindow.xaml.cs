using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MySqlConnector;
using System.IO;
using System.Reflection;

namespace WGU_C969_Software_II_CS;

public partial class LoginWindow : Window
{
    public int ID { get; set; }
    public string Username;
    
    public LoginWindow()
    {
        InitializeComponent();
        
        this.UsernameLabel.Content = WGU_C969_Software_II_CS.Resources.MainWindow.UsernameLabel;
        this.PasswordLabel.Content = WGU_C969_Software_II_CS.Resources.MainWindow.PasswordLabel;
    }
    
    private void MyTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged)
        {
            double newHeight = e.NewSize.Height;
            DoneButton.Width = newHeight * 2;
        }
    }

    private void DoneButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Username = this.UsernameTextBox.Text;
        string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Login_History.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }
        Console.WriteLine(filePath);
        
        
        using (MySqlConnection connection = new MySqlConnection(MainWindow.ConnectionBuilder.ConnectionString))
        {
            connection.Open();
            using (MySqlCommand command =
                   new MySqlCommand($"SELECT userId FROM user WHERE userName = @userName AND password = @password", connection))
            {
                command.Parameters.AddWithValue("@username", Username);
                command.Parameters.AddWithValue("@password", this.PasswordTextBox.Password);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    this.ID = reader.GetInt32("userId");
                    this.DialogResult = true;
                }
            }

            if (DialogResult == true)
            {
                using (MySqlCommand command2 = new MySqlCommand(@"
                    UPDATE user 
                    SET 
                        active = 1, 
                        lastUpdate = @lastUpdate, 
                        lastUpdateBy = @lastUpdateBy
                    WHERE userId = @userId",
                           connection)
                      )
                {
                    command2.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                    command2.Parameters.AddWithValue("@lastUpdateBy", this.Username);
                    command2.Parameters.AddWithValue("@userId", this.ID);
    
                    command2.ExecuteNonQuery();
                }
            }
        }
        
        if (DialogResult == true)
        {
            File.AppendAllText(filePath, $"{this.Username}, ({DateTime.Now})" + Environment.NewLine); 
            this.Close();
            return;
        }
        
        
        MessageBox.Show("Incorrect username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}