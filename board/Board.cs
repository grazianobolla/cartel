using Godot;
using System.Collections.Generic;

public static class Board
{
    private static List<Tile> _boardList;

    public static void SetList(List<Tile> list)
    {
        _boardList = list;
    }

    public static int GetMovedIndex(int start, int steps)
    {
        int size = _boardList.Count;
        return (steps + start) % size;
    }

    public static Vector3 GetTilePos(int index)
    {
        return _boardList[GetMovedIndex(0, index)].GlobalTransform.origin;
    }

    public static Tile GetTile(int index)
    {
        return _boardList[GetMovedIndex(0, index)];
    }

    public static int GetSize()
    {
        return _boardList.Count;
    }

    //TODO: maybe this can be improved by loading from template
    public static int GetTileGroupCount(int checkGroup)
    {
        int count = _boardList.FindAll(tile => (tile.type == Tile.Type.PROPERTY && tile.Group == checkGroup)).Count;
        return count;
    }
}
