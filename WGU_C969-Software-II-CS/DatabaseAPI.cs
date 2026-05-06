using System.IO;
using System.Data.SQLite;

namespace WGU_C969_Software_II_CS;

public static class DatabaseAPI
{
    private static string DBName = "SchedulingSoftware";

    public static void CheckCreation()
    {
        string dbPath = $"{Directory.GetCurrentDirectory()}\\SchedulingSoftwareDatabase.db";
        string connectionString = $"Data Source={dbPath};Version=3;";
        Console.WriteLine(Directory.GetCurrentDirectory());
        
        if (!File.Exists(dbPath))
        {
            SQLiteConnection.CreateFile(dbPath);

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                        CREATE TABLE country 
                                       (
                                           countryId INTEGER PRIMARY KEY, 
                                           country VARCHAR(50), 
                                           createDate DATETIME, 
                                           createdBy VARCHAR(40),
                                           lastUpdate TIMESTAMP, 
                                           lastUpdateBy VARCHAR(40)
                                       )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                    INSERT INTO country 
                        (country, createDate, createdBy, lastUpdate, lastUpdateBy)
                    VALUES 
                        (@country, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)", conn))
                {
                    cmd.Parameters.AddWithValue("@country", "USA");
                    cmd.Parameters.AddWithValue("@createDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@createdBy", "admin");
                    cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@lastUpdateBy", "admin");

                    cmd.ExecuteNonQuery();
                    
                    cmd.Parameters.AddWithValue("@country", "Spain");
                    cmd.Parameters.AddWithValue("@createDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@createdBy", "admin");
                    cmd.Parameters.AddWithValue("@lastUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@lastUpdateBy", "admin");

                    cmd.ExecuteNonQuery();
                }
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM  country", conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(
                            $"ID: {reader["countryId"]}, " +
                            $"Country: {reader["country"]}, " +
                            $"Created: {reader["createDate"]}, " +
                            $"By: {reader["createdBy"]}, " +
                            $"Updated: {reader["lastUpdate"]}, " +
                            $"By: {reader["lastUpdateBy"]}"
                        );
                    }
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE address 
                                    (
                                        addressId INTEGER PRIMARY KEY, 
                                        address VARCHAR(50), 
                                        address2 VARCHAR(50),
                                        cityId INTEGER,
                                        postalCode VARCHAR(10),
                                        phone VARCHAR(20),
                                        createDate DATETIME, 
                                        createdBy VARCHAR(40),
                                        lastUpdate TIMESTAMP, 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (cityId) REFERENCES city(cityId) 
                                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE city 
                                    (
                                        cityId INTEGER PRIMARY KEY, 
                                        city VARCHAR(50), 
                                        countryId INTEGER,
                                        createDate DATE, 
                                        createdBy VARCHAR(40),
                                        lastUpdate TIMESTAMP, 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (countryId) REFERENCES country(countryId) 
                                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE customer 
                                    (
                                        customerId INTEGER PRIMARY KEY, 
                                        customerName VARCHAR(50), 
                                        addressId INTEGER,
                                        active SMALLINT(1), 
                                        createDate DATE, 
                                        createdBy VARCHAR(40),
                                        lastUpdate TIMESTAMP, 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (addressId) REFERENCES address(addressId) 
                                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE user 
                                    (
                                        userId INTEGER PRIMARY KEY, 
                                        userName VARCHAR(50), 
                                        password VARCHAR(50),
                                        active SMALLINT(1), 
                                        createDate DATE, 
                                        createdBy VARCHAR(40),
                                        lastUpdate TIMESTAMP, 
                                        lastUpdateBy VARCHAR(40)
                                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                
                using (SQLiteCommand cmd = new SQLiteCommand(@"
                                    CREATE TABLE appointment 
                                    (
                                        appointmentId INTEGER PRIMARY KEY, 
                                        customerId INTEGER,
                                        userId INTEGER,
                                        title VARCHAR(50), 
                                        description TEXT,
                                        location TEXT,
                                        contact TEXT,
                                        type TEXT,
                                        url VARCHAR(255),
                                        start DATE,
                                        end DATE,
                                        createDate DATE, 
                                        createdBy VARCHAR(40),
                                        lastUpdate TIMESTAMP, 
                                        lastUpdateBy VARCHAR(40),
                                        FOREIGN KEY (customerId) REFERENCES customer(customerId),
                                        FOREIGN KEY (userId) REFERENCES user(userId) 
                                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        else
        {
            Console.WriteLine("Exists");
        }

        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            Console.WriteLine("Connected");
            
            string query = "SELECT name FROM sqlite_master WHERE type='table'";

            using (var cmd = new SQLiteCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("Table: " + reader["name"]);
                }
            }
            using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public static Dictionary<DataEnum, string> Pull(DataEnum type, int ID)
    {
        Dictionary<DataEnum, string> output = new Dictionary<DataEnum, string> { { DataEnum.Error, "Error" } };
        if (type == DataEnum.CustmerForm) 
        {
            if (false)  //if Id in db return data
            {
                
            }
            else //else not updating return empty
            {
                output.Remove(DataEnum.Error);
                output.Add(DataEnum.Name, "");
                output.Add(DataEnum.AddressID, "");
                output.Add(DataEnum.PhoneNumber, "");
            }
            
        }
        if (type == DataEnum.AddressForm) 
        {
            if (false)  //if Id in db return data
            {
                
            }
            else //else not updating return empty
            {
                output.Remove(DataEnum.Error);
                output.Add(DataEnum.AddressOne, "");
                output.Add(DataEnum.AddressTwo, "");
                output.Add(DataEnum.CityID, "");
                output.Add(DataEnum.PostalCode, "");
                output.Add(DataEnum.PhoneNumber, "");
            }
            
        }

        if (output.ContainsKey(DataEnum.Error))
        {
            throw new Exception("Database Error");
        }
        return output;
    }

    public static void Push(Dictionary<DataEnum, string> data)
    {
        //check if db has something with same ID mod if it does add new if it doesnt
        //if id <0 add new one to the end
        if (data.ContainsKey(DataEnum.CustmerForm))
        {
            
        }
        else if (data.ContainsKey(DataEnum.AddressForm))
        {
            
        }
        else if (data.ContainsKey(DataEnum.CityForm))
        {
            
        }
    }

    public static void Remove(DataEnum type, int ID)
    {
        Console.WriteLine(ID.ToString());
    }

    public static AdvancedList<CityForm> ReturnCities(string currentUser, App.UpdateComboBox ucb)
    {
        AdvancedList<CityForm> output = new AdvancedList<CityForm>(ucb);
        //add every city from the db into output dictionary
        
        output.Add(new CityForm(0, currentUser)
        {
            CityName = "Cool City 1",
            CountryId = 0
        });
        output.Add(new CityForm(1, currentUser)
        {
            CityName = "Cool City 2",
            CountryId = 0
        });

        return output;
    }

    public static void UpdateDB<T>(T type, Dictionary<DataEnum, string> data)
    {
        Console.WriteLine("Update DB");
    }
}


public enum DataEnum
{
    //generic
    Error,
    ID,
    CurrentUser,
    PhoneNumber,
    //CustomerForm
    CustmerForm,
    Name,
    AddressID,
    //AddressForm
    AddressForm,
    AddressOne,
    AddressTwo,
    CityID,
    PostalCode,
    //CityForm
    CityForm,
    CountryID
}