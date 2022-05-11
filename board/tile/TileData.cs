using Godot;
using System;

public class TileData
{
    public String Label { get; private set; }
    public int Group { get; private set; }
    public Color Color { get; private set; }

    private int _price;
    public int _houseCount;

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