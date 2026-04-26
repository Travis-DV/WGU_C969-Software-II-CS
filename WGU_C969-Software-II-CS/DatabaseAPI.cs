namespace WGU_C969_Software_II_CS;

public static class DatabaseAPI
{
    public static Dictionary<DataEnum, string> CheckID<T>(T type, int ID)
    {
        Dictionary<DataEnum, string> output = new Dictionary<DataEnum, string>();
        output.Add(DataEnum.Error, "Error");
        if (typeof(T) == typeof(CustomerForm)) 
        {
            if (false)  //if Id in db return data
            {
                
            }
            else //else not updating return empty
            {
                output.Remove(DataEnum.Error);
                output.Add(DataEnum.Updating, "False");
                output.Add(DataEnum.Name, "");
                output.Add(DataEnum.AddressID, "");
                output.Add(DataEnum.PhoneNumber, "");
            }
            
        }

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
    Updating,
    Error,
    ID,
    CurrentUser,
    //CustomerForm
    Name,
    AddressID,
    PhoneNumber
}