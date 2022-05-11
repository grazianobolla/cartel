using Godot;
using System.Collections.Generic;

public static class Board
{
    private static List<Tile> _boardList;
    public static int Size { get; private set; }

    public static void SetBoardList(List<Tile> list)
    {
        _boardList = list;
        Size = _boardList.Count;
    }

    public static int GetShiftedIndex(int start, int steps)
    {
        int size = _boardList.Count;
        return Mathf.PosMod(start + steps, size);
    }

    public static Tile GetTile(int index)
    {
        return _boardList[GetShiftedIndex(0, index)];
    }

    public static Transform GetTileTransform(int index)
    {
        return _boardList[GetShiftedIndex(0, index)].GlobalTransform;
    }

    //TODO: maybe this can be improved by loading from template
    public static int GetTileGroupCount(int checkGroup)
    {
        int count = _boardList.FindAll(tile => (tile.TileType == Tile.Type.PROPERTY && tile.Data.Group == checkGroup)).Count;
        return count;
    }
}
