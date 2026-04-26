using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WGU_C969_Software_II_CS;

public class PhoneClass : ValidationRule
{
    private int _countryCode;
    private int _regionCode;
    private int _prefix;
    private int _lineNumber;

    public override ValidationResult Validate(object value, CultureInfo cuture)
    {
        List<char> cleanString = new List<char>();
        string input = value.ToString();
        
        foreach (char c in input)
        {
            if (int.TryParse(c.ToString(), out _))
            {
                cleanString.Add(c);
            }
        }
        
        if (cleanString.Count < 10)
        {
            return new ValidationResult(false, WGU_C969_Software_II_CS.Resources.CustomerForm.PhoneNumberInvalidError);
        }
        
        string countryCodeString = "";
        for (int i = 0; i < (cleanString.Count - 10); i++)
        {
            countryCodeString += cleanString[i];
        }

        for (int i = 0; i < countryCodeString.Length; i++)
        {
            cleanString.RemoveAt(0);
        }

        if (countryCodeString == "")
        {
            countryCodeString = WGU_C969_Software_II_CS.Resources.CustomerForm.PhoneNumberCountryCode;
        }

        this._countryCode = int.Parse(countryCodeString);

        string regionCodestring = cleanString[0].ToString() + cleanString[1].ToString() + cleanString[2].ToString();
        this._regionCode = int.Parse(regionCodestring);

        string prefixString = cleanString[3].ToString() + cleanString[4].ToString() + cleanString[5].ToString();
        this._prefix = int.Parse(prefixString);
        
        string lineNumberString = cleanString[6].ToString() +cleanString[7].ToString() + cleanString[8].ToString() + cleanString[9].ToString();
        this._lineNumber = int.Parse(lineNumberString);
        
        return new ValidationResult(true, this.ToString());
    }

    public static implicit operator string(PhoneClass phone)
    {
        return phone._countryCode.ToString() + phone._regionCode.ToString() + phone._prefix.ToString() + phone._lineNumber.ToString();
    }

    public int this[PhoneParts phonePart]
    {
        get
        {
            switch (phonePart)
            {
                case PhoneParts.CountryCode:
                    return this._countryCode;
                case PhoneParts.RegionCode:
                    return this._regionCode;
                case PhoneParts.Prefix:
                    return this._prefix;
                case PhoneParts.LineNumber:
                    return this._lineNumber;
                default:
                    return -1;
            }
        }
    }
    // Returns greater than int32
    // Why did I think I needed this?
    // public static implicit operator int(PhoneClass phone)
    // {
    //     return int.Parse((string)phone);
    // }

    public override string ToString()
    {
        return $"+{this._countryCode} ({this._regionCode}) {this._prefix}-{this._lineNumber}";
    }
}

public enum PhoneParts
{
    CountryCode,
    RegionCode,
    Prefix,
    LineNumber
}