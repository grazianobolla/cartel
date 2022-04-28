using Godot;
using System.Collections.Generic;
using static Godot.GD;

public class PlayerManager : Node
{
    public List<Player> playersList { get; } = new List<Player>();

    private PackedScene _playerScene = (PackedScene)GD.Load("res://player/player.tscn");

    public void AddPlayer(int startingMoney)
    {
        Player player = (Player)_playerScene.Instance();
        AddChild(player);
        playersList.Add(player);
        player.Initialize(playersList.Count - 1, startingMoney);
    }

    public int GetNextId(int currentId)
    {
        int id = (currentId + 1) % playersList.Count;
        Print("turn of player ", currentId);
        return id;
    }

    public Player GetPlayer(int id)
    {
        return playersList[id]; //TODO: might have to change this if a player disconnects
    }

    public void TransferMoney(Player from, Player to, int amount)
    {
        //TODO: check if the player has money
        from.Money -= amount;
        to.Money += amount;
    }

    public void CheckTurn()
    {
        foreach (Player player in playersList)
        {
            if (player.state == Player.State.JAILED)
                player.ReduceJail(1);
        }
    }
}
