using Godot;
using System.Threading.Tasks;
using static Godot.GD;

public partial class GameLogic : Spatial
{
    public async Task ProcessLanding(Player player)
    {
        Tile tile = Board.GetTile(player.index);

        switch (tile.type)
        {
            case Tile.Type.PROPERTY:
            case Tile.Type.STATE:
                if (tile.owner == player || tile.owner == null)
                    return;

                Print("landed on someones property, paying ", tile.GetFee());
                playerManager.TransferMoney(player, tile.owner, tile.GetFee());
                break;

            case Tile.Type.CHANCE:
                var chanceData = template.GetRandomChanceData();
                //TODO: show player message
                Print("landed on chance tile (", chanceData.text, ")");
                player.Money += chanceData.cost;
                break;

            case Tile.Type.CORNER:
                if (tile.GetGroup() != 3) //always the jail corner
                    return;

                int jailIndex = Board.GetSize() / 4;
                await player.Jail(jailIndex);
                break;

            default:
                PrintErr("unknown tile type");
                break;
        }
    }

    public bool BuyTile(Player player)
    {
        Tile tile = Board.GetTile(player.index);

        if (!tile.IsBuyable())
        {
            Print("you cant buy this tile");
            return false;
        }

        if (tile.owner != null)
        {
            Print("already owned tile");
            return false;
        }

        if (player.Money < tile.GetPrice())
        {
            Print("you dont have enough money");
            return false;
        }

        player.Money -= tile.GetPrice();
        AssignTile(player, tile);
        Print("player ", player.id, " purchased tile ", tile.GetLabel(), " at ", tile.GetPrice());
        return true;
    }

    public bool BuyHouse(Player player)
    {
        Tile tile = Board.GetTile(player.index);
        int price = tile.GetHousePrice();

        if (tile.type != Tile.Type.PROPERTY)
        {
            Print("you can only buy houses on property tiles");
            return false;
        }

        if (!tile.IsOwner(player))
        {
            Print("this is not your property");
            return false;
        }

        if (player.Money < price)
        {
            Print("you dont have enough money");
        }

        if (tile.AddHouse())
        {
            player.Money -= price;
            return true;
        }

        Print("you cant buy more houses");
        return false;
    }

    private void AssignTile(Player player, Tile tile)
    {
        if (tile.owner != null)
            tile.owner.RemoveTile(tile);

        tile.owner = player;
        player.AddTile(tile);
    }
}
