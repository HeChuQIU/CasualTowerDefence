namespace CasualTowerDefence;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Resource;
using Serilog;
using Util;
using World;
using FastNoiseLite = Godot.FastNoiseLite;

[Meta(typeof(IAutoNode))]
public partial class Game : Control, IProvide<IServiceProvider>
{
    public override void _Notification(int what) => this.Notify(what);

    public Button TestButton { get; private set; } = default!;
    public int ButtonPresses { get; private set; }
    public TextureRect TestTextureRect { get; private set; } = default!;

    public override void _Ready()
    {
        DataGenerator.DataGenerator.Main();

        ServiceCollection services = new();
        services.AddSerilog((_, configuration) =>
            {
                configuration.WriteTo.Console()
                    .MinimumLevel.Debug();
            })
            .AddSingleton<Game>(this)
            .AddSingleton<List<ResourceIdBase>>([])
            .LoadTextures()
            .AddSingleton<TileGenerator>()
            .AddKeyedSingleton<FastNoiseLite>(FastNoiseLite.NoiseTypeEnum.Perlin,
                (_, o) => new FastNoiseLite { NoiseType = (FastNoiseLite.NoiseTypeEnum)o! })
            .AddTileImage()
            .AddMap();

        // GenerateTileImage(services);

        ServiceProvider = services.BuildServiceProvider();

        this.Provide();

        var Logger = ServiceProvider.GetRequiredService<ILogger>();
        Logger.Information("Game started.");

        TestButton = GetNode<Button>("%TestButton");
    }

    public void OnTestButtonPressed() => ButtonPresses++;
    IServiceProvider IProvide<IServiceProvider>.Value() => ServiceProvider;
    public IServiceProvider ServiceProvider { get; set; } = null!;

    [SuppressMessage("Security", "CA5350:不要使用弱加密算法")]
    private static Color GetColorFromString(string name)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(name));
        var r = (hash[0] & 0xFF) / 255f;
        var g = (hash[1] & 0xFF) / 255f;
        var b = (hash[2] & 0xFF) / 255f;
        return new Color(r, g, b);
    }

    private static void GenerateTileImage(ServiceCollection services) =>
        ((IEnumerable<string>) ["0", "1", "2", "3", "4"])
        .Select(s => new TileResourceId(ResourceId.BuiltinModName, new PathString(s)))
        .ToList()
        .ForEach(id =>
        {
            var colorFromString = GetColorFromString(id.ToString());
            services.AddKeyedSingleton<Image>(id, (provider, n) =>
            {
                var requiredService = provider.GetRequiredService<TileGenerator>();
                var resourceIdList = provider.GetRequiredService<List<ResourceIdBase>>();

                var generateTileTexture = requiredService.GenerateTileImage(id.ToString(), colorFromString);
                resourceIdList.Add(id);

                return generateTileTexture;
            });
        });

    private static void GenerateTileSet(ServiceCollection services)
    {
        // TileSetAtlasSource tileSetAtlasSource = new TileSetAtlasSource();
        // tileSetAtlasSource.Texture =
        services.AddSingleton<TileSet>(provider =>
        {
            var tileSet = new TileSet();
            var resourceIdList = provider.GetRequiredService<List<ResourceId>>();
            resourceIdList
                .Select(id => provider.GetRequiredKeyedService<Texture2D>(id))
                .ToList()
                .ForEach(texture =>
                {
                    TileSetAtlasSource tileSetAtlasSource = new() { Texture = texture };
                });
            return tileSet;
        });
    }
}
