using Godot;
using System.Collections.Generic;

public static class Board
{
    private static List<Tile> s_boardList;

    public static void SetList(List<Tile> list)
    {
        s_boardList = list;
    }

    public static int GetMovedIndex(int start, int steps)
    {
        int size = s_boardList.Count;
        return (steps + start) % size;
    }

    public static Vector3 GetTilePos(int index)
    {
        return s_boardList[GetMovedIndex(0, index)].GlobalTransform.origin;
    }

    public static Tile GetTile(int index)
    {
        return s_boardList[GetMovedIndex(0, index)];
    }

    public static int GetSize()
    {
        return s_boardList.Count;
    }
}
