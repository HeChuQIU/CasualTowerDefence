namespace CasualTowerDefence.World;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Resource;
using Serilog;
using Util;
using FastNoiseLite = Godot.FastNoiseLite;

public class Map : INotifyMapChanged, IEnumerable<(Vector2I Position, Chunk Chunk)>
{
    public Map(ILogger logger, [FromKeyedServices(FastNoiseLite.NoiseTypeEnum.Perlin)] FastNoiseLite noiseGenerator)
    {
        Logger = logger.ForContext<Map>();
        NoiseGenerator = noiseGenerator;
        TileMap.TileSet = new TileSet { TileSize = new Vector2I(64, 64) };

        Logger.Debug("Map created.");
    }

    public TileMapLayer TileMap { get; set; } = new();
    private Dictionary<TileResourceId, int> TileIdMap { get; } = new();
    private int GetTileId(TileResourceId id) => TileIdMap[id];

    public void AddTileTexture(TileResourceId id, Texture2D texture)
    {
        var tileSetAtlasSource = new TileSetAtlasSource();
        tileSetAtlasSource.Texture = texture;
        tileSetAtlasSource.TextureRegionSize = (Vector2I)texture.GetSize();
        tileSetAtlasSource.CreateTile(Vector2I.Zero);
        var intId = TileMap.TileSet.AddSource(tileSetAtlasSource);
        TileIdMap.Add(id, intId);
    }

    public void SetTile(Vector2I position, TileResourceId id) =>
        TileMap.SetCell(position, GetTileId(id), Vector2I.Zero);

    public void GenerateMap(int seed)
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
                var tileId = new TileResourceId(ResourceId.BuiltinModName, new PathString(tileName));
                SetTile(position, tileId);
            }
        }

        var endTime = DateTime.Now;
        Logger.Information("Map generated in {TimeSpan}.", endTime - startTime);
    }

    // private async Task SetTile(Vector2I position, TileResourceId id)
    // {
    //     var chunkPosition = new Vector2I(position.X / Chunk.SIZE, position.Y / Chunk.SIZE);
    //     var chunk = Chunks.Find(c => c.Position == chunkPosition)?.Chunk;
    //     if (chunk == null)
    //     {
    //         chunk = new Chunk(new TileResourceId());
    //         chunk.ChunkChanged += OnChunkChanged;
    //         Chunks.Add(new ChunkWithPosition(chunkPosition, chunk));
    //     }
    //
    //     chunk[position.X % Chunk.SIZE, position.Y % Chunk.SIZE] = id;
    // }

    private void OnChunkChanged(object? sender, NotifyChunkChangedEventArgs e)
    {
        // GD.Print($"Chunk changed at {e.Position} from {e.OldTileId} to {e.NewTileId}");
        // Logger.Debug("Chunk changed at {Position} from {OldTileId} to {NewTileId}", e.Position, e.OldTileId,
        //     e.NewTileId);
        MapChanged?.Invoke(this, new NotifyMapChangedEventArgs(e.Position, e.OldTileId, e.NewTileId));
    }

    public ILogger Logger { get; set; }

    public TileGenerator TileGenerator { get; set; }

    public FastNoiseLite NoiseGenerator { get; set; }

    public record ChunkWithPosition(Vector2I Position, Chunk Chunk);

    public List<ChunkWithPosition> Chunks { get; } = new();
    public event EventHandler<NotifyMapChangedEventArgs>? MapChanged;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<(Vector2I Position, Chunk Chunk)> GetEnumerator() =>
        Chunks.Select(c => (c.Position, c.Chunk)).GetEnumerator();
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
