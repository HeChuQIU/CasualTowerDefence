namespace CasualTowerDefence.World;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Meta(typeof(IAutoNode))]
public partial class MapNode : Node2D
{
    public override void _Notification(int what) => this.Notify(what);
    [Dependency] private IServiceProvider ServiceProvider => this.DependOn<IServiceProvider>();
    private Map Map => ServiceProvider.GetRequiredService<Map>();
    private ILogger Logger => ServiceProvider.GetRequiredService<ILogger>();

    public void OnResolved()
    {
        GD.Print("MapNode resolved.");
        Logger.Information("MapNode ready.");

        Map.MapChanged += OnMapChanged;
        Map.GenerateMap(42);
    }

    private List<(Vector2I Position, TileResourceId Id)> _tilesToDraw = [];

    public override void _Draw()
    {
        _tilesToDraw.GroupBy(t => t.Id)
            .Select(g => (ServiceProvider.GetKeyedService<Texture2D>(g.Key), g))
            .Where(t => t.Item1 != null)
            .Select(t => (Texture: t.Item1!, G: t.Item2))
            .ToList()
            .ForEach(t =>
            {
                var texture = t.Texture;
                t.G.ToList().ForEach(p =>
                {
                    var position = p.Position * texture.GetSize();
                    DrawTexture(texture, position);
                });
            });

        _tilesToDraw.Clear();
    }

    public void OnProcess(double delta)
    {
        if (_tilesToDraw.Count != 0)
        {
            QueueRedraw();
        }
    }

    private void OnMapChanged(object? sender, NotifyMapChangedEventArgs e)
    {
        _tilesToDraw.Add((e.Position, e.NewTileId));
    }
}
