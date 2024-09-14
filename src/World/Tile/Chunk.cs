namespace CasualTowerDefence.World;

using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Resource;

public class Chunk : INotifyChunkChanged, IEnumerable<(Vector2I Position, TileResourceId Id)>
{
    public const int SIZE = 64;
    private readonly TileResourceId[,] _tileIds = new TileResourceId[SIZE, SIZE];

    public Chunk(TileResourceId defaultTileId)
    {
        for (var x = 0; x < SIZE; x++)
        {
            for (var y = 0; y < SIZE; y++)
            {
                _tileIds[x, y] = defaultTileId;
            }
        }
    }

    public TileResourceId this[int x, int y]
    {
        get => _tileIds[x, y];
        set
        {
            var oldTileId = _tileIds[x, y];
            _tileIds[x, y] = value;
            ChunkChanged?.Invoke(this, new NotifyChunkChangedEventArgs(new Vector2I(x, y), oldTileId, value));
        }
    }

    public TileResourceId this[Vector2I position]
    {
        get => this[position.X, position.Y];
        set => this[position.X, position.Y] = value;
    }

    public event EventHandler<NotifyChunkChangedEventArgs>? ChunkChanged;
    public IEnumerator<(Vector2I Position, TileResourceId Id)> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator : IEnumerator<(Vector2I Position, TileResourceId Id)>
    {
        private readonly Chunk _chunk;
        private int _x = -1;
#pragma warning disable CA1805
        private int _y = 0;
#pragma warning restore CA1805

        public Enumerator(Chunk chunk)
        {
            _chunk = chunk;
        }

        public (Vector2I Position, TileResourceId Id) Current => (new Vector2I(_x, _y), _chunk[_x, _y]);

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_x == SIZE - 1)
            {
                _x = 0;
                _y++;
            }
            else
            {
                _x++;
            }

            return _y < SIZE;
        }

        public void Reset()
        {
            _x = -1;
            _y = 0;
        }
    }
}

public interface INotifyChunkChanged
{
    event EventHandler<NotifyChunkChangedEventArgs> ChunkChanged;
}

public class NotifyChunkChangedEventArgs : EventArgs
{
    public NotifyChunkChangedEventArgs(Vector2I position, TileResourceId oldTileId, TileResourceId newTileId)
    {
        Position = position;
        OldTileId = oldTileId;
        NewTileId = newTileId;
    }

    public Vector2I Position { get; }
    public TileResourceId OldTileId { get; }
    public TileResourceId NewTileId { get; }
}
