using UnityEngine;

public class Coord
{
    private int _x;
    private int _z;

    public int X_int { get { return _x; } set { _x = value; } }
    public int Z_int { get { return _z; } set { _z = value; } }
    public float X
    {
        get => _x;
        set => _x = Mathf.FloorToInt(value);
    }

    public float Z
    {
        get => _z;
        set => _z = Mathf.FloorToInt(value);
    }
    public Coord(int x, int z)
    {
        _x = x;
        _z = z;
    }

    public Coord()
    {
        _x = 0;
        _z = 0;
    }

    public Coord(Vector3 pos)
    {
        _x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        _z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;
    }

    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        else if (other._x == _x && other._z == _z)
            return true;
        else
            return false;
    }
}