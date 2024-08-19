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
        ServiceCollection services = new();
        services.AddSerilog((provider, configuration) =>
            {
                configuration.WriteTo.Console()
                    .MinimumLevel.Information();
            })
            .AddSingleton<Game>(this)
            .AddSingleton<TileGenerator>()
            .AddSingleton<Map>()
            .AddKeyedSingleton<FastNoiseLite>(FastNoiseLite.NoiseTypeEnum.Perlin,
                (_, o) => new FastNoiseLite { NoiseType = (FastNoiseLite.NoiseTypeEnum)o! });

        GenerateTileTexture(services);

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

    private static void GenerateTileTexture(ServiceCollection services) =>
        ((IEnumerable<string>)["0", "1", "2", "3", "4"])
        .Select(s => new TileResourceId(ResourceId.BUILTIN_MOD_NAME, s))
        .ToList()
        .ForEach(id =>
        {
            var colorFromString = GetColorFromString(id.ToString());
            services.AddKeyedSingleton<Texture2D>(id , (provider,n) =>
            {
                var requiredService = provider.GetRequiredService<TileGenerator>();
                return requiredService.GenerateTileTexture(id.ToString(), colorFromString);
            });
        });
}
