using Godot;
using System;

public class TileData
{
    public String Label { get; private set; } = "No Label";
    public int Group { get; private set; } = -1;
    public Color Color { get; private set; } = Colors.White;

    private int _price = 0;
    public int _houseCount = 0;

    public TileData() { }

    public TileData(String label, int price, int group, Color color)
    {
        this.Label = label;
        this.Group = group;
        this.Color = color;

        _price = price;
        _houseCount = 0;
    }

    public int HouseCount
    {
        get { return _houseCount; }
        set { _houseCount = value; }
    }

    public int Price
    {
        get { return _price; }
    }

    public int Fee
    {
        get { return _houseCount * 10; }
    }

    public int HousePrice
    {
        get { return 10; }
    }
}