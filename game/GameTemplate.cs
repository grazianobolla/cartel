using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static Godot.GD;

public class GameTemplate
{
    private JObject _template;

    public GameTemplate(String path)
    {
        String jsonString = Utils.ReadFile(path);
        _template = JObject.Parse(jsonString);
    }

    public bool Check() { return true; }

    public int GetSideCount()
    {
        var tileCount = _template["tile-count"];
        return (int)tileCount["property"] + (int)tileCount["state"] + (int)tileCount["chance"];
    }

    public int GetTileCount(Tile.Type type)
    {
        switch (type)
        {
            case Tile.Type.PROPERTY:
                return (int)_template["tile-count"]["property"];

            case Tile.Type.STATE:
                return (int)_template["tile-count"]["state"];

            case Tile.Type.CHANCE:
                return (int)_template["tile-count"]["chance"];
        }

        PrintErr("wrong tile type ", type);
        return 0;
    }

    public int GetStartingMoney()
    {
        return (int)_template["settings"]["initial-money"];
    }

    public (String text, int cost) GetRandomChanceData()
    {
        int size = ((JArray)_template["chance-data"]).Count;
        Int32 index = (Int32)(Randi() % size);
        var data = _template["chance-data"][index];
        return ((String)data["text"], (int)data["cost"]);
    }

    public List<TileData> GenerateDataList(List<Tile> boardList)
    {
        List<TileData> dataList = new List<TileData>();

        int propertyCount, groupCount, stateCount, cornerCount;
        propertyCount = groupCount = stateCount = cornerCount = 0;

        var properties = _template["properties"];

        foreach (Tile tile in boardList)
        {
            switch (tile.type)
            {
                case Tile.Type.PROPERTY:
                    {
                        var info = properties["normal"][groupCount][propertyCount];
                        TileData data = new TileData((String)info["label"], (int)info["price"], groupCount);
                        dataList.Add(data);

                        int groupSize = ((JArray)properties["normal"][groupCount]).Count - 1;

                        if (propertyCount >= groupSize)
                        {
                            groupCount = (groupCount + 1) % ((JArray)properties["normal"]).Count;
                        }

                        propertyCount = (propertyCount + 1) % 3; //TODO: magic num
                        break;
                    }
                case Tile.Type.STATE:
                    {
                        var info = properties["state"][stateCount];
                        TileData data = new TileData((String)info["label"], (int)info["price"], stateCount);
                        dataList.Add(data);
                        stateCount = (stateCount + 1) % GetTileCount(Tile.Type.STATE);
                        break;
                    }
                case Tile.Type.CHANCE:
                    {
                        dataList.Add(new TileData("Chance", 0, 0));
                        break;
                    }

                case Tile.Type.CORNER:
                    {
                        dataList.Add(new TileData("Corner", 0, cornerCount));
                        cornerCount += 1;
                        break;
                    }
                default:
                    {
                        dataList.Add(new TileData());
                        break;
                    }
            }
        }

        return dataList;
    }
}
