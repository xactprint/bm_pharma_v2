using System.Windows;
using System.Windows.Data;

namespace BMPharma.UI.Converters;

public class ConverterResourceDictionary : ResourceDictionary
{
    public ConverterResourceDictionary()
    {
        Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
        Add("InverseBoolToVisibilityConverter", new InverseBoolToVisibilityConverter());
        Add("ColorToBrushConverter", new ColorToBrushConverter());
    }
}
