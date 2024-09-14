namespace CasualTowerDefence.Util;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Godot;
using SkiaSharp;
using Bitmap = System.Drawing.Bitmap;
using Color = Godot.Color;
using Font = System.Drawing.Font;
using Image = Godot.Image;

public class TileGenerator
{
    public Image GenerateTileImage(string name, Color color, int size = 32)
    {
        var bitmap = CreateImage(size, size, name, color);
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, bitmap);
        return image;
    }

    public byte[] CreateImage(int width, int height, string text, Color backgroundColor = default)
    {
        if (backgroundColor == default)
        {
            backgroundColor = GetColorFromString(text);
        }

        // Create a new bitmap with specified width and height
        using var bitmap = new SKBitmap(width, height);
        // Create a canvas from the bitmap
        using var canvas = new SKCanvas(bitmap);
        // Fill the background with the specified color
        canvas.Clear(new SKColor(backgroundColor.ToRgba32()));

        // Set up the font and paint for drawing the text
        using var paint = new SKPaint();
        paint.Color = SKColors.Black;
        paint.TextSize = 16;
        paint.IsAntialias = true;

        // Draw the text at the top-left corner
        canvas.DrawText(text, 0, 16, paint);

        return bitmap.Bytes;
    }

    [SuppressMessage("Security", "CA5350:不要使用弱加密算法")]
    public static Color GetColorFromString(string name)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(name));
        var r = (hash[0] & 0xFF) / 255f;
        var g = (hash[1] & 0xFF) / 255f;
        var b = (hash[2] & 0xFF) / 255f;
        return new Color(r, g, b);
    }
}
