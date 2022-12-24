using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class BoardGenerator : Node
{
    [Export] private NodePath _tileGroup = null;
    [Export] private float _tileSize = 3.0f;

    private List<Tile> _boardList = new List<Tile>();

    public void GenerateFromTemplate(GameTemplate template)
    {
        //TODO: get tile size from model
        InstanceBoard(template.GetSideCount(), "res://board/tile/tile.tscn", (Spatial)GetNode(_tileGroup), _tileSize);
        FillBoardTypes
        (
            template.GetTileSideCount(Tile.Type.PROPERTY),
            template.GetTileSideCount(Tile.Type.STATE),
            template.GetTileSideCount(Tile.Type.CHANCE)
        );
        FillBoard(template.GenerateHandlers(_boardList));

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
            tile.Name = $"Tile{i}";

            //Rotate and place tile in place.
            int filter = (i > 0 ? 1 : 0);
            tile.Translate(pos + dir * tileSize * filter);

            tile.RotateY(-(Mathf.Pi / 2) * (int)(i / sideLenght + 1));
            pos = tile.Translation;

            _boardList.Add(tile as Tile);
            parent.AddChild(tile);

            if (i % sideLenght == 0 && i > 0)
            {
                dir = dir.Rotated(Vector3.Up, -Mathf.Pi / 2);
            }
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
            sideList[0].TileType = Tile.Type.CORNER;
            sideList.RemoveAt(0);

            var tileTypeList = GetRandomTileTypeList(propertyCount, stateCount, chanceCount);

            //Should not happen
            if (sideList.Count != tileTypeList.Count)
                PrintErr("tileTypeList and sideList count do not match");

            for (int i = 0; i < sideList.Count; i++)
            {
                sideList[i].TileType = tileTypeList[i];
            }
        }
    }

    private void FillBoard(List<TileHandler> handlerList)
    {
        //Should not happen
        if (handlerList.Count != _boardList.Count)
            PrintErr("dataList and boardList count do not match");

        for (int i = 0; i < _boardList.Count; i++)
        {
            Tile tile = _boardList[i];
            tile.Initialize(handlerList[i], i);
        }
    }

    private List<Tile.Type> GetRandomTileTypeList(int propertyCount, int stateCount, int chanceCount)
    {
        List<Tile.Type> typeList = new List<Tile.Type>();

        for (int i = 0; i < propertyCount; i++)
        {
            typeList.Add(Tile.Type.PROPERTY);
        }

        for (int i = 0; i < stateCount; i++)
        {
            typeList.Add(Tile.Type.STATE);
        }

        for (int i = 0; i < chanceCount; i++)
        {
            typeList.Add(Tile.Type.CHANCE);
        }

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
