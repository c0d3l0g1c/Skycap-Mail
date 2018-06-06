using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Windows.UI;

namespace Skycap.Text.Converters.Html
{
    public static class TypeExtensions
    {
        private static Regex colour = new Regex(@"rgb\((\d+), (\d+), (\d+)\)");

        public static Color FromName(string name)
        {
            try
            {
                Match match = colour.Match(name);
                if (match.Success)
                {
                    return Color.FromArgb(255, byte.Parse(match.Groups[1].Value), byte.Parse(match.Groups[2].Value), byte.Parse(match.Groups[3].Value));
                }
                else
                {
                    var property = typeof(Colors).GetTypeInfo().DeclaredProperties.Single(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    return (Color)property.GetValue(null);
                }
            }
            catch
            {
                return Colors.Black;
            }
        }
    }
}
