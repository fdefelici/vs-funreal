using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace FUnreal
{
    [ContentProperty("Icon")]
    public class ImageSourceFromIconExt : MarkupExtension
{
    public Icon Icon { get; set; }

    public ImageSourceFromIconExt()
    {
    }

    public ImageSourceFromIconExt(Icon icon)
    {
        Icon = icon;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }
}
}