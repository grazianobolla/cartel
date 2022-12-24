using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static Godot.GD;

public class GameTemplate
{
    private JObject _template;
    private JToken _settings;

    public GameTemplate(String path)
    {
        String jsonString = Utils.ReadFile(path);
        _template = JObject.Parse(jsonString);
        _settings = _template["settings"];
    }

    public bool Check() { return true; }

    public int GetSideCount()
    {
        var tileCount = _template["side-count"];
        return (int)tileCount["property"] + (int)tileCount["state"] + (int)tileCount["chance"];
    }

    public int GetTileSideCount(Tile.Type type)
    {
        switch (type)
        {
            case Tile.Type.PROPERTY:
                return (int)_template["side-count"]["property"];

            case Tile.Type.STATE:
                return (int)_template["side-count"]["state"];

            case Tile.Type.CHANCE:
                return (int)_template["side-count"]["chance"];
        }

        PrintErr("wrong tile type ", type);
        return 0;
    }

    public int GetStartingMoney()
    {
        return (int)_settings["initial-money"];
    }

    public (String text, int cost) GetRandomChanceData()
    {
        int size = ((JArray)_template["chance-data"]).Count;
        Int32 index = (Int32)(Randi() % size);
        var data = _template["chance-data"][index];
        return ((String)data["text"], (int)data["cost"]);
    }

    public List<TileHandler> GenerateHandlers(List<Tile> tileList)
    {
        List<TileHandler> dataList = new List<TileHandler>();

        int groupIndex = 0;
        int propertyIndex = 0;

        int stateCount = 0;
        int cornerCount = 0;

        var properties = _template["properties"];

        foreach (Tile tile in tileList)
        {
            switch (tile.TileType)
            {
                case Tile.Type.PROPERTY:
                    {
                        int currentPropertiesGroupSize = 3;

                        // get group data
                        var groupData = properties["normal"][groupIndex][0];

                        //get tile info
                        propertyIndex = (propertyIndex % currentPropertiesGroupSize) + 1;
                        var tileInfo = properties["normal"][groupIndex][propertyIndex];

                        // define group color based on group data
                        Godot.Color groupColor = new Godot.Color((string)groupData["color"]);

                        // generate handler and fill
                        TradeableHandler handler = new TradeableHandler(
                            tile,
                            (String)tileInfo["label"],
                            (int)tileInfo["price"],
                            groupIndex,
                            groupColor);

                        dataList.Add(handler);

                        // if propertyIndex >= groupSize, we advance to the next group
                        if (propertyIndex >= currentPropertiesGroupSize)
                        {
                            groupIndex = (groupIndex + 1) % ((JArray)properties["normal"]).Count;
                        }

                        break;
                    }

                case Tile.Type.STATE:
                    {
                        var tileInfo = properties["state"][stateCount];
                        Godot.Color groupColor = new Godot.Color((string)tileInfo["color"]);

                        TradeableHandler data = new TradeableHandler(
                            tile,
                            (String)tileInfo["label"],
                            (int)tileInfo["price"],
                            stateCount,
                            groupColor);

                        dataList.Add(data);

                        int stateTilesPerSide = GetTileSideCount(Tile.Type.STATE);
                        stateCount = (stateCount + 1) % stateTilesPerSide;
                        break;
                    }

                case Tile.Type.CHANCE:
                    {
                        dataList.Add(new ChanceHandler(tile, "Chance"));
                        break;
                    }

                case Tile.Type.CORNER:
                    {
                        dataList.Add(new CornerHandler(tile, "Corner", cornerCount));
                        cornerCount += 1;
                        break;
                    }

                default:
                    {
                        PrintErr("unkown tile type");
                        break;
                    }
            }
        }

        return dataList;
    }
}