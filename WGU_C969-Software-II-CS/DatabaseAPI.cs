namespace WGU_C969_Software_II_CS;

public static class DatabaseAPI
{
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