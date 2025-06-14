using System.Globalization;

namespace CatSystem2;
internal static class Util
{
    internal static bool TryParseInt(string str, out uint value) 
    {

        if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) 
        {
           return uint.TryParse(str.AsSpan(2), NumberStyles.AllowHexSpecifier, null, out value);
        }
        else 
        {
           return uint.TryParse(str,out value);
        }
    }


}
