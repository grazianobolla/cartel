using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class BoardGenerator : Node
{
    [Export] private NodePath _tileGroup = null;
    private List<Tile> _boardList = new List<Tile>();

    public void GenerateFromTemplate(GameTemplate template)
    {
        InstanceBoard(template.GetSideCount(), "res://board/tile/tile.tscn", (Spatial)GetNode(_tileGroup), 3.3f);
        FillBoardTypes
        (
            template.GetTileCount(Tile.Type.PROPERTY),
            template.GetTileCount(Tile.Type.STATE),
            template.GetTileCount(Tile.Type.CHANCE)
        );
        FillBoard(template.GenerateDataList(_boardList));

        Board.SetBoardList(_boardList);
    }

    private void InstanceBoard(int sideLenght, String tileModelPath, Spatial parent, float tileSize)
    {
        sideLenght += 1;

        Print("creating board side lenght ", sideLenght);

        PackedScene tileScene = (PackedScene)Load(tileModelPath);
        Vector3 pos = Vector3.Zero;
        Vector3 dir = Vector3.Forward;

        for (int i = 0; i < sideLenght * 4; i++)
        {
            Spatial tile = (Spatial)tileScene.Instance();

            //Rotate and place tile in place.
            int filter = (i > 0 ? 1 : 0);
            tile.Translate(pos + dir * tileSize * filter);
            tile.RotateY(-(Mathf.Pi / 2) * (int)(i / sideLenght + 1));
            pos = tile.Translation;

            _boardList.Add(tile as Tile);
            parent.AddChild(tile);

            if (i % sideLenght == 0 && i > 0)
                dir = dir.Rotated(Vector3.Up, -Mathf.Pi / 2);
        }

        parent.Translate(new Vector3(-sideLenght * tileSize, 0, sideLenght * tileSize) / 2);
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

    private Godot.Collections.Array<Tile.Type> GenerateTileArray(int propertyCount, int stateCount, int chanceCount)
    {
        Godot.Collections.Array<Tile.Type> typeArray = new Godot.Collections.Array<Tile.Type>();

        for (int i = 0; i < propertyCount; i++)
            typeArray.Add(Tile.Type.PROPERTY);

        for (int i = 0; i < stateCount; i++)
            typeArray.Add(Tile.Type.STATE);

        for (int i = 0; i < chanceCount; i++)
            typeArray.Add(Tile.Type.CHANCE);

        typeArray.Shuffle();

        return typeArray;
    }
}
