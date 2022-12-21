using SixLabors.ImageSharp.PixelFormats;

namespace SaladimWpf;

public static class Extensions
{
    public static SixLabors.ImageSharp.Color ToISColor(this System.Drawing.Color sysColor)
    {
        return new SixLabors.ImageSharp.Color(new Rgba32(sysColor.R, sysColor.G, sysColor.B, sysColor.A));
    }
}
