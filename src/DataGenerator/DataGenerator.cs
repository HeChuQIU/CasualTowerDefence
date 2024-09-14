namespace CasualTowerDefence.DataGenerator;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Resource;
using Util;

public class DataGenerator
{
    public static void Main()
    {
        List<TileResourceId> tileResourceIds = ((string[]) ["0", "1", "2", "3", "4"])
            .Select(s => new TileResourceId(ResourceId.BuiltinModName, new PathString(s)))
            .ToList();
        tileResourceIds.ForEach(id =>
        {
            TileGenerator tileGenerator = new();
            const int size = 64;
            var generateTileTexture = tileGenerator.CreateImage(size, size, id.Path);
            var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, generateTileTexture);
            var path = ProjectSettings.GlobalizePath($"user://datagen/tiles");
            DirAccess.MakeDirRecursiveAbsolute(path);
            image.SavePng(Path.Combine(path, $"{id.Path}.png"));
        });
    }
}
