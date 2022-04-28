using Godot;
using System;

public class Tile : Spatial
{
    private const int MAX_HOUSE_COUNT = 4;

    public enum Type { NONE, CORNER, PROPERTY, STATE, CHANCE };
    public Type type { get; set; } = Type.NONE;
    public int index { get; private set; } = 0;
    public Player owner { get; set; } = null;

    private TileData _data;

    public void Initialize(TileData data, int index)
    {
        this._data = data;
        this.index = index;
        UpdateVisual();
    }

    public int GetFee()
    {
        return _data.houses * 10;
    }

    public int GetGroup()
    {
        return _data.group;
    }

    public bool IsBuyable()
    {
        return type == Type.PROPERTY || type == Type.STATE;
    }

    public int GetPrice()
    {
        return _data.price;
    }

    public String GetLabel()
    {
        return _data.label;
    }

    public int GetHousePrice()
    {
        return 100;
    }

    public bool AddHouse()
    {
        if (_data.houses >= MAX_HOUSE_COUNT)
            return false;

        _data.houses += 1;
        Spatial houseModel = Utils.SpawnModel(this, "res://resources/models/defaultHouseModel.tscn");
        houseModel.Translate(new Vector3((float)GD.RandRange(0.7, -0.7), 0, -1));
        UpdateVisual();

        return true;
    }

    public bool IsOwner(Player player)
    {
        //FIXME: what happens if the player loses connection and reconnects??
        //the saved player will not be the same and this will not work:
        return player == owner;
    }

    private void UpdateText(String str)
    {
        GetNode<Label>("Viewport/Label").Text = str;
        GetNode<Viewport>("Viewport").UpdateViewport();
    }

    private void UpdateMesh(Color color)
    {
        Material material = GetNode<MeshInstance>("MeshInstance").GetSurfaceMaterial(0);
        material.Set("albedo_color", color);
    }

    private void UpdateVisual()
    {
        switch (this.type)
        {
            case Type.PROPERTY:
            case Type.STATE:
                UpdateMesh(this.type == Type.PROPERTY ? Colors.Blue : Colors.Yellow);
                UpdateText($"{_data.label}\nprice: {_data.price}\ngroup: {_data.group}\nfee: {GetFee()}");
                break;

            case Type.CORNER:
                UpdateMesh(Colors.Magenta);
                UpdateText($"{_data.group}");
                break;

            case Type.CHANCE:
                UpdateMesh(Colors.White);
                UpdateText($"{_data.label}");
                break;

            case Type.NONE:
            default:
                UpdateText("Error");
                break;
        }
    }

    //TODO: debug
    private void _on_DebugArea_input_event(Node camera, InputEvent ev, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (ev is InputEventMouseButton eventButton)
        {
            if (eventButton.IsPressed() && eventButton.ButtonIndex == (int)ButtonList.Left)
            {
                GetNode<Controller>("/root/Controller").SendDebugShakeMethod(index);
            }
        }
    }

}

public struct TileData
{
    public String label { get; }
    public int price { get; }
    public int group { get; }
    public int houses { get; set; }

    public TileData(String label, int price, int group)
    {
        this.label = label;
        this.price = price;
        this.group = group;
        this.houses = 0;
    }
}