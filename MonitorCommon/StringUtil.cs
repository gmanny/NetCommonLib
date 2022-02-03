using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonitorCommon;

public static class StringUtil
{
    // from https://stackoverflow.com/a/36845864/579817
    public static int GetStableHashCode(this string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i+1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
            }

            return hash1 + (hash2*1566083941);
        }
    }

    public static string FirstLetterToUpper(this string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    // taken from https://www.30secondsofcode.org/c-sharp/s/to-camel-case
    public static string ToCamelCase(this string str, bool firstLetterUpper = true) 
    {
        Regex pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
        return new string(
            CultureInfo.InvariantCulture
                .TextInfo
                .ToTitleCase(
                    string.Join(" ", pattern.Matches(str)).ToLower()
                )
                .Replace(@" ", "")
                .Select((x, i) => i == 0 
                    ? firstLetterUpper 
                        ? char.ToUpperInvariant(x) 
                        : char.ToLowerInvariant(x) 
                    : x)
                .ToArray()
        );
    }
}