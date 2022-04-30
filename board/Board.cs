using Godot;
using System.Collections.Generic;

public static class Board
{
    private static List<Tile> _boardList;
    public static int boardSize { get; private set; }

    public static void SetBoardList(List<Tile> list)
    {
        _boardList = list;
        boardSize = _boardList.Count;
    }

    public static int GetMovedIndex(int start, int steps)
    {
        int size = _boardList.Count;
        return (steps + start) % size;
    }

    public static Tile GetTile(int index)
    {
        return _boardList[GetMovedIndex(0, index)];
    }

    public static Transform GetTileTransform(int index)
    {
        return _boardList[GetMovedIndex(0, index)].GlobalTransform;
    }

    //TODO: maybe this can be improved by loading from template
    public static int GetTileGroupCount(int checkGroup)
    {
        int count = _boardList.FindAll(tile => (tile.type == Tile.Type.PROPERTY && tile.Group == checkGroup)).Count;
        return count;
    }
}
