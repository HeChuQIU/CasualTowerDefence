namespace CasualTowerDefence.Util;

using System.Drawing;
using Godot;
using SkiaSharp;
using Bitmap = System.Drawing.Bitmap;
using Color = Godot.Color;
using Font = System.Drawing.Font;
using Image = Godot.Image;

public class TileGenerator
{
    public Texture2D GenerateTileTexture(string name, Color color, int size = 32)
    {
        var bitmap = CreateImage(size, size, color, name);
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, bitmap);
        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }

    public byte[] CreateImage(int width, int height, Color backgroundColor, string text)
    {
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
}
