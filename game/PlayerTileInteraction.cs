using System.Threading.Tasks;
using Godot;
using static Godot.GD;

//Handles interaction between tiles-players.
public class PlayerTileInteraction : Node
{
    [Export] private NodePath _dialogManagerPath;
    [Export] private NodePath _playerInteractionPath;

    private TileSelector _tileSelector;
    private DialogManager _dialogManager;
    private PlayerInteraction _playerInteraction;

    public override void _Ready()
    {
        _tileSelector = (TileSelector)GetNode("TileSelector");
        _dialogManager = (DialogManager)GetNode(_dialogManagerPath);
        _playerInteraction = (PlayerInteraction)GetNode(_playerInteractionPath);
    }

    //Called when a player interacts with the game (in his turn)
    public async Task<bool> ProcessInteraction(Player player, Controller.Action action, Godot.Collections.Array arguments)
    {
        switch (action)
        {
            case Controller.Action.BUY:
                BuyTile(player, player.Index);
                return false;

            //[tileIndex]
            case Controller.Action.BUY_HOUSE:
                BuyHouse(player, _tileSelector.CurrentIndex);
                return false;

            case Controller.Action.OMIT:
                return true;

            case Controller.Action.TILE_SELECTOR_FWD:
                _tileSelector.Move(true);
                return false;

            case Controller.Action.TILE_SELECTOR_BKW:
                _tileSelector.Move(false);
                return false;

            case Controller.Action.SELECT_TILE:
                _tileSelector.MoveTo((int)arguments[0]);
                return false;

            //[tileIndex, playerId, price]
            case Controller.Action.TRADE:
                try
                {
                    int tileIndex = (int)arguments[0];
                    int playerId = (int)arguments[1];
                    int price = (int)arguments[2];

                    Tile tile = Board.GetTile(tileIndex);
                    Player targetPlayer = PlayerManager.GetPlayer(playerId);

                    await _playerInteraction.TradeProperty(tile, player, targetPlayer, price);
                }
                catch { }

                return false;

            default:
                //PrintErr("unkown action");
                return false;
        }
    }

    //Called when a player lands on a tile, this happens automatically
    public async Task ProcessLanding(Player player, GameTemplate template)
    {
        //Current tile under the player
        Tile tile = Board.GetTile(player.Index);

        switch (tile.TileType)
        {
            case Tile.Type.PROPERTY:
            case Tile.Type.STATE:
                if (tile.PlayerOwner == player || tile.PlayerOwner == null)
                    return;

                Print("landed on someones property, paying ", tile.Data.Fee);
                PlayerInteraction.TransferMoney(player, tile.PlayerOwner, tile.Data.Fee);
                break;

            case Tile.Type.CHANCE:
                var chanceData = template.GetRandomChanceData();
                player.Money += chanceData.cost;
                string message = $"{chanceData.text}\nCost: {chanceData.cost}";
                await _dialogManager.ShowDialog(player, message);
                break;

            case Tile.Type.CORNER:
                //3 is always the jail corner
                if (tile.Data.Group != 3)
                    return;

                int jailIndex = Board.Size / 4;
                await player.Jail(jailIndex);
                break;

            default:
                PrintErr("unknown tile type");
                break;
        }
    }

    public bool BuyTile(Player player, int tileIndex)
    {
        Tile tile = Board.GetTile(tileIndex);

        if (!tile.IsBuyable())
        {
            Print("you cant buy this tile");
            return false;
        }

        if (tile.PlayerOwner != null)
        {
            Print("already owned tile");
            return false;
        }

        if (player.Money < tile.Data.Price)
        {
            Print("you dont have enough money");
            return false;
        }

        player.Money -= tile.Data.Price;
        AssignTile(player, tile);

        Print("player ", player.Index, " purchased tile ", tile.Data.Label, " at ", tile.Data.Price);
        return true;
    }

    public bool BuyHouse(Player player, int tileIndex)
    {
        Tile tile = Board.GetTile(tileIndex);
        int price = tile.Data.HousePrice;

        if (tile.TileType != Tile.Type.PROPERTY)
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
            return false;
        }

        if (!player.HasGroup(tile.Data.Group))
        {
            Print("you dont have a full tile group yet");
            return false;
        }

        if (tile.AddHouse())
        {
            player.Money -= price;
            return true;
        }

        Print("you cant buy more houses");
        return false;
    }

    public static void AssignTile(Player player, Tile tile)
    {
        if (tile.PlayerOwner != null)
        {
            tile.PlayerOwner.RemoveTile(tile);
        }

        tile.PlayerOwner = player;
        player.AddTile(tile);

        Print(tile.Data.Label, " is now owned by player ", player.Id);
    }

    public void EnableTileSelection(int index = 0)
    {
        _tileSelector.Enable(index);
    }

    public void DisableTileSelection()
    {
        _tileSelector.Disable();
    }
}
