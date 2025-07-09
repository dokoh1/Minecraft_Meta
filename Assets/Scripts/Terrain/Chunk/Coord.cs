using UnityEngine;

public class Coord
{
    public int X;
    public int Z;
    
    public Coord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public Coord()
    {
        X = 0;
        Z = 0;
    }

    public Coord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);
        X = Mathf.FloorToInt(xCheck) /  VoxelData.ChunkWidth;
        Z = Mathf.FloorToInt(zCheck) / VoxelData.ChunkDepth;
    }

    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z)
            return true;
        else
            return false;
    }
}