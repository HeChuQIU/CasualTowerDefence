namespace CasualTowerDefence.World;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Util;
using FastNoiseLite = Godot.FastNoiseLite;

public class Map : INotifyMapChanged
{
    public Map(ILogger logger, [FromKeyedServices(FastNoiseLite.NoiseTypeEnum.Perlin)] FastNoiseLite noiseGenerator)
    {
        Logger = logger;
        NoiseGenerator = noiseGenerator;

        Logger.Debug("Map created.");
    }

    public async Task GenerateMap(int seed)
    {
        var startTime = DateTime.Now;
        NoiseGenerator.Seed = seed;
        NoiseGenerator.Frequency = 0.1f;
        for (var x = 0; x < 1024; x++)
        {
            for (var y = 0; y < 1024; y++)
            {
                var position = new Vector2I(x, y);
                var noise = NoiseGenerator.GetNoise2D(x, y);
                var tileName = noise switch
                {
                    < -0.5f => "0",
                    < -0.25f => "1",
                    < 0.25f => "2",
                    < 0.5f => "3",
                    _ => "4"
                };
                var tileId = new TileResourceId(ResourceId.BUILTIN_MOD_NAME, tileName);
                await SetTile(position, tileId);
            }
        }
        var endTime = DateTime.Now;
        Logger.Information("Map generated in {TimeSpan}.", endTime - startTime);
    }

    private async Task SetTile(Vector2I position, TileResourceId id)
    {
        var chunkPosition = new Vector2I(position.X / Chunk.SIZE, position.Y / Chunk.SIZE);
        var chunk = Chunks.Find(c => c.Position == chunkPosition)?.Chunk;
        if (chunk == null)
        {
            chunk = new Chunk(TileResourceId.Default);
            chunk.ChunkChanged += OnChunkChanged;
            Chunks.Add(new ChunkWithPosition(chunkPosition, chunk));
        }

        chunk[position.X % Chunk.SIZE, position.Y % Chunk.SIZE] = id;
    }

    private void OnChunkChanged(object? sender, NotifyChunkChangedEventArgs e)
    {
        // GD.Print($"Chunk changed at {e.Position} from {e.OldTileId} to {e.NewTileId}");
        Logger.Debug("Chunk changed at {Position} from {OldTileId} to {NewTileId}", e.Position, e.OldTileId,
            e.NewTileId);
        MapChanged?.Invoke(this, new NotifyMapChangedEventArgs(e.Position, e.OldTileId, e.NewTileId));
    }

    public ILogger Logger { get; set; }

    public TileGenerator TileGenerator { get; set; }

    public FastNoiseLite NoiseGenerator { get; set; }

    public record ChunkWithPosition(Vector2I Position, Chunk Chunk);

    public List<ChunkWithPosition> Chunks { get; } = new();
    public event EventHandler<NotifyMapChangedEventArgs>? MapChanged;
}

public interface INotifyMapChanged
{
    event EventHandler<NotifyMapChangedEventArgs>? MapChanged;
}

public class NotifyMapChangedEventArgs(Vector2I position, TileResourceId oldTileId, TileResourceId newTileId)
    : EventArgs
{
    public Vector2I Position { get; } = position;
    public TileResourceId OldTileId { get; } = oldTileId;
    public TileResourceId NewTileId { get; } = newTileId;
}
