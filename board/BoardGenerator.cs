using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class BoardGenerator : Node
{
    private List<Tile> _boardList = new List<Tile>();

    public void GenerateFromTemplate(GameTemplate templ)
    {
        InstanceBoard(templ.GetSideCount(), "res://board/tile/tile.tscn", 3, (Spatial)GetNode("/root/GameLogic/TileGroup"));
        FillBoardTypes
        (
            templ.GetTileCount(Tile.Type.PROPERTY),
            templ.GetTileCount(Tile.Type.STATE),
            templ.GetTileCount(Tile.Type.CHANCE)
        );
        FillBoard(templ.GenerateDataList(_boardList));

        Board.SetList(_boardList);
    }

    private void InstanceBoard(int sideLenght, String tileModelPath, int tileGap, Spatial parent)
    {
        sideLenght += 1;

        Print("creating board side lenght ", sideLenght);

        PackedScene tileScene = (PackedScene)Load(tileModelPath);
        Vector3 pos = Vector3.Zero;
        Vector3 dir = Vector3.Forward;

        for (int i = 0; i < sideLenght * 4; i++)
        {
            Spatial tile = (Spatial)tileScene.Instance();
            tile.Translate(pos + dir * tileGap * (i > 0 ? 1 : 0));
            tile.Rotate(Vector3.Up, -(Mathf.Pi / 2) * (int)(i / sideLenght + 1));

            pos = tile.Translation;

            _boardList.Add(tile as Tile);
            parent.AddChild(tile);

            if (i % sideLenght == 0 && i > 0)
                dir = dir.Rotated(Vector3.Up, -Mathf.Pi / 2);
        }

        parent.Translate(new Vector3(-sideLenght * tileGap, 0, sideLenght * tileGap) / 2);
    }

    private void FillBoardTypes(int propertyCount, int stateCount, int chanceCount)
    {
        int boardSideLenght = (_boardList.Count / 4);

        Print("filling board types lenght ", boardSideLenght, " (of ", _boardList.Count, ")");

        for (int side = 0; side < 4; side++)
        {
            int sideArrayStart = side * boardSideLenght;
            var sideList = _boardList.GetRange(sideArrayStart, boardSideLenght);

            //sets the first element of the array to a corner type
            sideList[0].type = Tile.Type.CORNER;
            sideList.RemoveAt(0);

            var tileTypeList = GenerateTileArray(propertyCount, stateCount, chanceCount);

            for (int i = 0; i < sideList.Count; i++)
                sideList[i].type = tileTypeList[i];
        }
    }

    private void FillBoard(List<TileData> dataList)
    {
        if (dataList.Count != _boardList.Count)
            PrintErr("dataList and _boardList count do not match");

        for (int i = 0; i < _boardList.Count; i++)
        {
            Tile tile = _boardList[i];
            tile.Initialize(dataList[i], i);
        }
    }

    private List<Tile.Type> GenerateTileArray(int propertyCount, int stateCount, int chanceCount)
    {
        List<Tile.Type> typeList = new List<Tile.Type>();

        for (int i = 0; i < propertyCount; i++)
            typeList.Add(Tile.Type.PROPERTY);

        for (int i = 0; i < stateCount; i++)
            typeList.Add(Tile.Type.STATE);

        for (int i = 0; i < chanceCount; i++)
            typeList.Add(Tile.Type.CHANCE);

        ShuffleArray(typeList);

        return typeList;
    }

    private void ShuffleArray<T>(List<T> array)
    {
        //TODO: check this function
        var rng = new Random();

        int n = array.Count - 1;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
