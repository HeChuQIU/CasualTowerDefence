namespace CasualTowerDefence.World;

using System;
using System.Collections.Generic;
using Godot;
using Resource;

public class TileMapHelper()
{
    public TileMapLayer TileMap { get; } = new();

    private Dictionary<TileResourceId, int> TileIdMap { get; } = new();
    private int GetTileId(TileResourceId id) => TileIdMap[id];

    public void AddTileTexture(TileResourceId id, Texture2D texture)
    {
        var tileSetAtlasSource = new TileSetAtlasSource();
        tileSetAtlasSource.Texture = texture;
        tileSetAtlasSource.CreateTile(Vector2I.Zero, (Vector2I?)texture.GetSize() ?? throw new InvalidOperationException());
        var intId = TileMap.TileSet.AddSource(tileSetAtlasSource);
        TileIdMap.Add(id, intId);
    }

    public void SetTile(Vector2I position, TileResourceId id) => TileMap.SetCell(position, GetTileId(id));
}
