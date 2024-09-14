namespace CasualTowerDefence.Util;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Resource;
using World;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTextureKeyList(this IServiceCollection services)
    {
        services.AddSingleton<List<TextureResourceId>>();
        return services;
    }

    public static IServiceCollection LoadTextures(this IServiceCollection services)
    {
        var textureFolderPath = "res://Texture";
        var textureFolder = DirAccess.Open(textureFolderPath);
        if (textureFolder is null) return services;

        List<string> filePaths = [];

        List<string> curFoldPaths = [""];
        while (curFoldPaths.Count > 0)
        {
            var ts = curFoldPaths
                .Select(path =>
                {
                    var (fs, ds) = GetFilesAndSubdirs(textureFolderPath, path);
                    var fullPathFiles = fs.Select(f => Path.Combine(textureFolderPath, path, f).Replace("\\", "/"));
                    var fullPathDirs = ds.Select(d => Path.Combine(path, d).Replace("\\", "/"));
                    return (fullPathFiles, fullPathDirs);
                });

            var (files, subdirs) = ts.Aggregate((a, b) => (a.Item1.Concat(b.Item1), a.Item2.Concat(b.Item2)));
            filePaths.AddRange(files);
            curFoldPaths = subdirs.ToList();
        }

        filePaths
            .ForEach(path =>
            {
                GD.Print($"Loading texture: {path}");
                var texture = ResourceLoader.Load<Texture2D>(path);
                if (texture is null)
                {
                    return;
                }

                var p = path["res://Texture/".Length..];
                var td = Path.GetDirectoryName(p);
                var tn = Path.GetFileNameWithoutExtension(p);
                var textureResourceName = $"{td}/{tn}".Replace(@"\", "/");

                var textureResourceId = new TextureResourceId(ResourceId.BuiltinModName,
                    new PathString(textureResourceName));

                services.AddKeyedSingleton<Texture2D>(textureResourceId, (provider, _) =>
                {
                    var keyList = provider.GetRequiredService<List<ResourceIdBase>>();
                    return texture;
                });

                services.BuildServiceProvider().GetRequiredService<List<ResourceIdBase>>().Add(textureResourceId);

                GD.Print($"Texture loaded: {textureResourceId}");
            });

        return services;

        (string[] files, string[] subdirs) GetFilesAndSubdirs(string root, string path)
        {
            var folder = DirAccess.Open(Path.Combine(root, path));
            if (folder is null) return ([], []);
            folder.IncludeHidden = false;
            return (folder.GetFiles().Where(s => s.EndsWith("png", StringComparison.OrdinalIgnoreCase)).ToArray(),
                folder.GetDirectories());
        }
    }

    public static IServiceCollection AddMap(this IServiceCollection services)
    {
        services.AddSingleton<Map>(provider =>
        {
            var textureResourceIdList = provider.GetRequiredService<List<ResourceIdBase>>();
            var tileResourceIdList = textureResourceIdList
                .Where(id => id.Path.StartsWith("Tile"))
                .ToList();

            var map = ActivatorUtilities.CreateInstance<Map>(provider);
            tileResourceIdList
                .ToList()
                .ForEach(id =>
                {
                    var texture = provider.GetRequiredKeyedService<Texture2D>(id);
                    map.AddTileTexture(new TileResourceId(ResourceId.BuiltinModName, new PathString(id.Path["Tile/".Length..])),
                        texture);
                });

            return map;
        });
        return services;
    }

    public static IServiceCollection AddTileImage(this IServiceCollection services)
    {
        // ((IEnumerable<string>) ["0", "1", "2", "3", "4"])
        //     .Select(s => new TileResourceId(ResourceId.BUILTIN_MOD_NAME, s))
        //     .ToList()
        //     .ForEach(id =>
        //     {
        //         var colorFromString = GetColorFromString(id.ToString());
        //         services.AddKeyedSingleton<Image>(id, (provider, n) =>
        //         {
        //             var requiredService = provider.GetRequiredService<TileGenerator>();
        //             var resourceIdList = provider.GetRequiredService<List<ResourceId>>();
        //
        //             var generateTileTexture = requiredService.GenerateTileImage(id.ToString(), colorFromString);
        //             resourceIdList.Add(id);
        //
        //             return generateTileTexture;
        //         });
        //     });
        // services.AddKeyedSingleton<Image>(  );
        // {
        //     var resourceIds = provider.GetRequiredService<List<ResourceId>>();
        //     var images = resourceIds.Select(id =>
        //     {
        //         var colorFromString = GetColorFromString(id.ToString());
        //         var requiredService = provider.GetRequiredService<TileGenerator>();
        //         var resourceIdList = provider.GetRequiredService<List<ResourceId>>();
        //
        //         var generateTileTexture = requiredService.GenerateTileImage(id.ToString(), colorFromString);
        //         resourceIdList.Add(id);
        //
        //         return generateTileTexture;
        //     }).ToList();
        //     return images;
        // });
        return services;
    }

    public static IServiceCollection AddResourceIds(this IServiceCollection services)
    {
        services.AddSingleton<List<ResourceIdBase>>(((IEnumerable<string>) ["0", "1", "2", "3", "4"])
            .Select(s => new TileResourceId(ResourceId.BuiltinModName, new PathString(s)) as ResourceIdBase)
            .ToList());
        return services;
    }

    [SuppressMessage("Security", "CA5350:不要使用弱加密算法")]
    private static Color GetColorFromString(string name)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(name));
        var r = (hash[0] & 0xFF) / 255f;
        var g = (hash[1] & 0xFF) / 255f;
        var b = (hash[2] & 0xFF) / 255f;
        return new Color(r, g, b);
    }
}
