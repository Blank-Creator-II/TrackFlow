using System.Drawing;
using System.IO;
using SkiaSharp;
using Svg.Skia;

namespace TrackFlow.Utils;
public static class IconLoader
{
    // the utility loads an SVG file, scales it to a square bitmap, and optionally tints it for themes.
    public static Bitmap Load(string icon_path, int size, Color? tint = null)
    {
        string path = Path.Combine(FileHelper.BASE_DIR,icon_path); // it recives a path starting from Assets/ do not send BASE_DIR when calling!!!

        var svg = new SKSvg(); // Creates an SVG loader

        svg.Load(path); // as the name says it load the SVG from Assets/

        var picture = svg.Picture;

        if (picture == null) // checks if we have icons otherwise this whole thing will go kaboom!
        {
            Console.WriteLine($"SVG could not be loaded or is empty: {path}");
        }

        var bounds = picture!.CullRect; // the compiler still thinks this can be null which is true but in debug we will see it so...

        float scale = size / Math.Max(bounds.Width, bounds.Height); // soeme math to preserv the aspect ratio

        var info = new SKImageInfo(size, size);

        using var surface = SKSurface.Create(info); // makes an offscrean drawing surface for the bitmap
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent); // let's not forget to change background to transparent

        canvas.Scale(scale);

        if (tint.HasValue) // themes baby!... if we gave it that is tinting using a color filter
        {
            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateBlendMode(
                    new SKColor(
                        tint.Value.R,
                        tint.Value.G,
                        tint.Value.B,
                        tint.Value.A),
                    SKBlendMode.SrcIn
                )
            };

            canvas.DrawPicture(picture, paint);
        }
        else
        {
            canvas.DrawPicture(picture);  // who ever did this shall be jailed; it draw SVG normally with original colors
        }

        using var image = surface.Snapshot(); // Snapshot the rendered image

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);  // finnaly it encodes it as PNG

        return new Bitmap(new MemoryStream(data.ToArray()));  // Convert to System.Drawing.Bitmap for WinForms
    }
}
