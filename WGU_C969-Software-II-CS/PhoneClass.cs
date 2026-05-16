using System.Globalization;
using System.Windows.Controls;

namespace WGU_C969_Software_II_CS;

public class PhoneClass : ValidationRule
{
    private int _countryCode;
    private int _regionCode;
    private int _prefix;
    private int _lineNumber;

    public override ValidationResult Validate(object? value, CultureInfo cuture)
    {
        List<char> cleanString = new List<char>();
        if (value != null)
        {
            string input = value.ToString() ?? "";
        
            foreach (char c in input)
            {
                if (int.TryParse(c.ToString(), out _))
                {
                    cleanString.Add(c);
                }
            }
        }

        if (cleanString.Count < 10)
        {
            return new ValidationResult(false, Resources.CustomerFormLocal.PhoneNumberInvalidError);
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
            countryCodeString = Resources.CustomerFormLocal.PhoneNumberCountryCode;
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
    public static implicit operator PhoneClass(string phone)
    {
        PhoneClass output = new PhoneClass();
        output.Validate(phone, CultureInfo.CurrentCulture);
        return output;
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

    public override string ToString()
    {
        if (this._countryCode == null || this._countryCode <= 0 ||
            this._regionCode == null || this._regionCode <= 0 ||
            this._prefix == null || this._prefix <= 0 ||
            this._lineNumber == null || this._lineNumber <= 0)
        {
            return "";
        }
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