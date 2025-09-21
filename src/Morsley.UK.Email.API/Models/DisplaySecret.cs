namespace Morsley.UK.Email.API.Models;

public class DisplaySecret
{
    public string Comment { get; set; } = string.Empty;

    private string _value = string.Empty;

    public string Value 
    {
        get
        {
            if (Mask) return PartiallyMaskSecret(_value);            
            return _value;
        } 
        set
        {
            _value = value;;
        }
    }

    public bool Mask { get; set; } = true;

    private string PartiallyMaskSecret(string value)
    {
        /*
         * Length: 1 --> [1]
         * Length: 2 --> x[1]
         * Length: 3 --> x[2]
         * 
         * Length: 4 to 9 --> x[length - 2]x
         * 
         * Length: 10 to 19 --> xx[6]xx
         * Length: 20 to 29 --> xxx[14]xxx
         * Length: 30 to 39--> xxxx[22]xxxx
         * etc.
         */

        var length = value.Length;
        var first = 0;
        var last = value.Length - 1;
        var show = 0;
        if (length > 4) { 
            show = (length % 10) + 1;
        }

        if (length == 1) return "[1]";
        if (length == 2 || length == 3) return $"{_value[first]}[{length - 1}]";
        if (length >= 4 && length < 10) return $"{_value[first]}[{length - 2}]{_value[last]}";

        var sb = new StringBuilder();
        
        for (var i = 0; i < show; i++)
        {
            sb.Append(_value[i]);
        }

        sb.AppendFormat("[{0}]", length - (show * 2));

        for (var i = length - show; i < length; i++)
        {
            sb.Append(_value[i]);
        }

        return sb.ToString();
    }
}