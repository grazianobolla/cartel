using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Godot.GD;

public class Player : Spatial
{
    [Signal] public delegate void MoneyChange(int id, int money);

    public enum State { PLAYING, JAILED };

    public int Id { get; private set; } = 0;
    public int Index { get; private set; } = 0;
    public List<Tile> OwnedTiles { get; } = new List<Tile>();
    public State PlayerState { get; private set; } = State.PLAYING;
    public int JailTime { get; private set; } = 0;

    private Vector3 _posOffset = new Vector3(0, 0.25f, 0);
    private int _money = 0;

    public void Initialize(int id, int money)
    {
        this.Id = id;
        this.Money = money;
        this.Translation = Board.GetTileTransform(Index).origin + _posOffset;

        UpdateMesh(Utils.GetRandomColor());
    }

    public async Task Move(int amount)
    {
        Print("moving by ", amount, " places");
        await AnimateForward(amount);
        Index = Board.GetShiftedIndex(Index, amount);
    }

    public int GetDistanceTo(int toIndex)
    {
        if (toIndex >= Index)
            return toIndex - Index;

        return (int)(Board.Size - (Index - toIndex));
    }

    public async Task MoveTo(int toIndex)
    {
        int amount = GetDistanceTo(toIndex);
        await Move(amount);
    }

    public void AddTile(Tile tile)
    {
        if (!OwnedTiles.Contains(tile))
        {
            OwnedTiles.Add(tile);
            return;
        }

        PrintErr("player already has this tile");
    }

    public void RemoveTile(Tile tile)
    {
        OwnedTiles.Remove(tile);
    }

    public async Task Jail(int jailTileIndex)
    {
        //TODO: load from template or somewhere else
        JailTime = 3;
        PlayerState = State.JAILED;
        await MoveTo(jailTileIndex);
    }

    public void ReduceJail(int turns)
    {
        JailTime = Mathf.Max(JailTime - turns, 0);

        if (JailTime <= 0)
            PlayerState = State.PLAYING;
    }

    public bool CanPlay()
    {
        return PlayerState != State.JAILED;
    }

    public bool HasGroup(int group)
    {
        int count = OwnedTiles.FindAll(tile => tile.Data.Group == group).Count;
        return count == Board.GetTileGroupCount(group);
    }

    public int Money
    {
        get { return _money; }
        set
        {
            _money = value;
            EmitSignal(nameof(MoneyChange), Id, _money);
            if (_money < 1)
            {
                Print("player ", Id, " lost!");
            }
        }
    }

    public void UpdateMesh(Color color)
    {
        GetNode<MeshInstance>("MeshInstance").GetSurfaceMaterial(0).Set("albedo_color", color);
    }

    private async Task AnimateForward(int amount, float stepTime = .2f)
    {
        TransformInterpolator ti = (TransformInterpolator)GetNode("TransformInterpolator");

        for (int i = 0; i < amount + 1; i++)
        {
            int tileIndex = Index + i;

            //TODO: Gets tile position and interpolates player
            //probably shoudl't directly interpolate to the tile transform
            //it works for now.
            Transform tileTransform = Board.GetTileTransform(tileIndex);
            //Adjust mesh position
            tileTransform.origin += _posOffset;
            //This rotates the player 90 to make it look 'forward'
            tileTransform.basis = tileTransform.basis.Rotated(Vector3.Up, Mathf.Pi / 2);

            ti.InterpolateTransform(this, tileTransform);
            ti.Start(stepTime, Tween.TransitionType.Cubic);

            await ToSignal(ti, "FinishedInterpolation");
        }
    }
}