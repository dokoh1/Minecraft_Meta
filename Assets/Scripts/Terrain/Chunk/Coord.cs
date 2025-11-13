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
    
    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        if (other.X == X && other.Z == Z)
            return true;
        return false;
    }
}