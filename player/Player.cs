using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Godot.GD;

public class Player : Spatial
{
    public enum State { PLAYING, JAILED };

    public int id { get; private set; } = 0;
    public int index { get; private set; } = 0;
    public List<Tile> ownedTiles { get; } = new List<Tile>();
    public State state { get; private set; } = State.PLAYING;
    public int jailTime { get; private set; } = 0;

    private int _money = 0;

    public void Initialize(int id, int money)
    {
        this.id = id;
        this.Money = money;
        this.Translation = Board.GetTileTransform(index).origin;
        UpdateMesh(Utils.GetRandomColor());
    }

    public async Task Move(int amount)
    {
        Print("moving by ", amount, " places");
        await AnimateForward(amount);
        index = Board.GetMovedIndex(index, amount);
    }

    public int GetDistanceTo(int toIndex)
    {
        if (toIndex >= index)
            return toIndex - index;

        return (int)(Board.boardSize - (index - toIndex));
    }

    public async Task MoveTo(int toIndex)
    {
        int amount = GetDistanceTo(toIndex);
        await Move(amount);
    }

    public void AddTile(Tile tile)
    {
        if (!ownedTiles.Contains(tile))
        {
            ownedTiles.Add(tile);
            return;
        }

        PrintErr("player already has this tile");
    }

    public void RemoveTile(Tile tile)
    {
        ownedTiles.Remove(tile);
    }

    public async Task Jail(int jailTileIndex)
    {
        //TODO: load from template or somewhere else
        jailTime = 3;
        state = State.JAILED;
        await MoveTo(jailTileIndex);
    }

    public void ReduceJail(int turns)
    {
        jailTime = Mathf.Max(jailTime - turns, 0);

        if (jailTime <= 0)
            state = State.PLAYING;
    }

    public bool CanPlay()
    {
        return state != State.JAILED;
    }

    public bool HasGroup(int group)
    {
        int count = ownedTiles.FindAll(tile => tile.Group == group).Count;
        return count == Board.GetTileGroupCount(group);
    }

    public int Money
    {
        get { return _money; }
        set
        {
            _money = value;
            //TODO: move somewhere else (maybe Game.cs), call signal instead
            GetNode<Controller>("/root/Controller").UpdateMoneyLabel(id, _money);
            if (_money < 1)
            {
                Print("player ", id, " lost!");
            }
        }
    }

    public void UpdateMesh(Color color)
    {
        GetNode<MeshInstance>("MeshInstance").GetSurfaceMaterial(0).Set("albedo_color", color);
    }

    private async Task AnimateForward(int amount, float stepTime = .1f)
    {
        TransformInterpolator ti = (TransformInterpolator)GetNode("TransformInterpolator");

        for (int i = 0; i < amount + 1; i++)
        {
            int tileIndex = index + i;

            Transform target = Board.GetTileTransform(tileIndex);
            ti.InterpolateTransform(this, target);
            ti.Start(stepTime);

            await ToSignal(ti, "FinishedInterpolation");
        }
    }
}