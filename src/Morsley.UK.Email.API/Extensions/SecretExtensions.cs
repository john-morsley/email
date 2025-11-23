namespace Morsley.UK.Email.API.Extensions;

public static class SecretExtensions
{
    public static string ToMaskedSecret(this string? secret)
    {
        if (string.IsNullOrEmpty(secret)) return "[0]";

        /*
         * Length: 0 to 9 --> [length]
         * Length: 10 to 19 --> x[length - 2]x
         * Length: 20 to 29 --> xx[length - 4]xx
         * Length: 30 to 39 --> xxx[length - 6]xxx
         * etc.
         */

        int length = secret.Length;

        //if (length >= 1 && length < 10) return $"[{length}]";

        int show = length / 10;

        var sb = new StringBuilder();

        for (var i = 0; i < show; i++)
        {
            sb.Append(secret[i]);
        }

        sb.AppendFormat("[{0}]", length - (show * 2));

        for (var i = length - show; i < length; i++)
        {
            sb.Append(secret[i]);
        }

        return sb.ToString();
    }
}