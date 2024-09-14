namespace CasualTowerDefence.World;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Resource;
using Serilog;

[Meta(typeof(IAutoNode))]
public partial class MapNode : Node2D
{
    public override void _Notification(int what) => this.Notify(what);
    [Dependency] private IServiceProvider ServiceProvider => this.DependOn<IServiceProvider>();
    private Map Map => ServiceProvider.GetRequiredService<Map>();
    private ILogger Logger => ServiceProvider.GetRequiredService<ILogger>().ForContext<MapNode>();

    public void OnResolved()
    {
        GD.Print("MapNode resolved.");

        // Map.MapChanged += OnMapChanged;
        Map.GenerateMap(42);
        AddChild(Map.TileMap);

        Logger.Information("MapNode ready.");
    }

    private List<(Vector2I Position, TileResourceId Id)> _tilesToDraw = [];

    public override void _Draw()
    {
        var startTime = DateTime.Now;
        Logger.Information("Drawing tiles.");

        // _tilesToDraw.GroupBy(t => t.Id)
        //     .Select(g => (ServiceProvider.GetKeyedService<Texture2D>(g.Key), g))
        //     .Where(t => t.Item1 != null)
        //     .Select(t => (Texture: t.Item1!, G: t.Item2))
        //     .ToList()
        //     .ForEach(t =>
        //     {
        //         var texture = t.Texture;
        //         t.G.ToList().ForEach(p =>
        //         {
        //             var position = p.Position * texture.GetSize();
        //             DrawTexture(texture, position);
        //         });
        //     });

        Logger.Debug("Drawing map with {ChunkCount} chunks.", Map.Chunks.Count);

        Map.ToList().ForEach(pc =>
        {
            var (chunkPosition, chunk) = pc;
            Logger.Debug("Drawing chunk at {Position}.", chunkPosition);

            chunk.ToList().ForEach(pt =>
            {
                var (tilePosition, tileId) = pt;
                // Logger.Debug("Drawing tile at {Position} with ID {TileId}.", tilePosition, tileId);

                var texture = ServiceProvider.GetKeyedService<Texture2D>(tileId);
                if (texture == null)
                {
                    Logger.Warning("Texture for tile ID {TileId} not found.", tileId);
                    return;
                }

                var position = tilePosition * texture.GetSize();
                DrawTexture(texture, position);
            });
        });

        // _tilesToDraw.Clear();

        var endTime = DateTime.Now;
        Logger.Information("Tiles drawn in {TimeSpan}.", endTime - startTime);
    }

    public void OnProcess(double delta)
    {
        // if (_tilesToDraw.Count != 0)
        // {
        //     QueueRedraw();
        // }
    }

    // private void OnMapChanged(object? sender, NotifyMapChangedEventArgs e) =>
    //     _tilesToDraw.Add((e.Position, e.NewTileId));
}
