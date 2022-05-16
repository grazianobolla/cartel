using Godot;
using System.Collections.Generic;
using static Godot.GD;

public class PlayerManager : Node
{
    [Signal] public delegate void AddedPlayer(Player player, int playerCount);

    private static List<Player> _playersList { get; } = new List<Player>();
    private PackedScene _playerScene = (PackedScene)GD.Load("res://player/player.tscn");

    public override void _Ready()
    {
        GetNode("/root/Game").Connect("StartedTurn", this, "OnGameTurnStart");
    }

    public void AddPlayer(int startingMoney)
    {
        Player player = (Player)_playerScene.Instance();
        EmitSignal(nameof(AddedPlayer), player, _playersList.Count + 1);

        AddChild(player);
        _playersList.Add(player);
        //TODO: id shouldn't be the index of the player
        player.Initialize(_playersList.Count - 1, startingMoney);
        Print("added player id ", player.Id, " player count ", _playersList.Count);
    }

    public int GetNextId(int currentId)
    {
        int id = (currentId + 1) % _playersList.Count;
        return id;
    }

    public static Player GetPlayer(int id)
    {
        //TODO: might have to change this if a player disconnects
        return _playersList[id];
    }

    private void OnGameTurnStart()
    {
        CheckTurn();
    }

    //Called at the start of every turn.
    private void CheckTurn()
    {
        foreach (Player player in _playersList)
        {
            if (player.PlayerState == Player.State.JAILED)
                player.ReduceJail(1);
        }

        Print("checking turns");
    }

    //TODO: remove or improve, used only by DebugInfo.cs
    public List<Player> PlayerList
    {
        get { return _playersList; }
    }
}
