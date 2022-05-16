using Godot;
using static Godot.GD;
using System.Threading.Tasks;

//TODO: alert instead of print
//Handles interaction between player-player.
public class PlayerInteraction : Node
{
    [Export] private NodePath _dialogManagerPath;
    private DialogManager _dialogManager;

    public override void _Ready()
    {
        _dialogManager = (DialogManager)GetNode(_dialogManagerPath);
    }

    public async Task<bool> TradeProperty(Tile property, Player playerGiver, Player playerReceiver, int price)
    {
        if (playerGiver.Id == playerReceiver.Id)
        {
            Print("you cant trade with yourself");
            return false;
        }

        if (property.TileType != Tile.Type.PROPERTY && property.TileType != Tile.Type.STATE)
        {
            Print("wrong tile type");
            return false;
        }

        if (!property.IsOwner(playerGiver))
        {
            Print("you are not the owner of this tile");
            return false;
        }

        if (property.IsOwner(playerReceiver))
        {
            Print("player already owns this tile");
            return false;
        }

        //TODO: maybe check if it has houses on it?
        //see what the rules say.

        string message = $"{playerGiver.Id} wants to trade {property.Data.Label} for ${price}.\nDo you accept?";
        bool response = await _dialogManager.ShowDialog(playerReceiver, message);

        //target accepted deal
        if (response)
        {
            if (SafeTransferMoney(playerReceiver, playerGiver, price))
            {
                PlayerTileInteraction.AssignTile(playerReceiver, property);
                return true;
            }

            Print("receiver doesnt have enough money");
            return false;
        }

        Print("player rejected offer!");
        return false;
    }

    public static void TransferMoney(Player from, Player to, int amount)
    {
        //TODO: check if the player has money
        from.Money -= amount;
        to.Money += amount;
        Print($"player {from.Id} transfered ${amount} to player {to.Id}");
    }

    public static bool SafeTransferMoney(Player from, Player to, int amount)
    {
        if (from.Money < amount)
            return false;

        TransferMoney(from, to, amount);
        return true;
    }

}
