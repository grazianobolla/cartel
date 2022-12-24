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

            //[tileIndex, playerId, price]
            case Controller.Action.TRADE:
                try
                {
                    int tileIndex = (int)arguments[0];
                    int playerId = (int)arguments[1];
                    int price = (int)arguments[2];

                    Print("trading tile:", tileIndex, " to:", playerId, " price:", price);

                    Tile tile = Board.GetTile(tileIndex);
                    Player targetPlayer = PlayerManager.GetPlayer(playerId);

                    await _playerInteraction.TradeProperty(tile, player, targetPlayer, price);
                }
                catch { GD.PrintErr("error while executing trade interaction"); }

                return false;

            default:
                PrintErr("unkown action");
                return false;
        }
    }

    //Called when a player lands on a tile, this happens automatically
    public async Task ProcessLanding(Player player, GameTemplate template)
    {
        //Current tile under the player
        Tile tile = Board.GetTile(player.Index);

        //TODO: emit signals for each case
        switch (tile.TileType)
        {
            case Tile.Type.PROPERTY:
            case Tile.Type.STATE:
                {
                    TradeableHandler handler = tile.Handler as TradeableHandler;
                    if (handler.PlayerOwner == player || handler.PlayerOwner == null)
                        return;

                    Print("landed on someones property, paying ", handler.LandingFee);
                    _playerInteraction.SafeTransferMoney(player, handler.PlayerOwner, handler.LandingFee);
                    break;
                }
            case Tile.Type.CHANCE:
                {
                    var chanceData = template.GetRandomChanceData();
                    player.Money += chanceData.cost;
                    string message = $"{chanceData.text}\nCost: {chanceData.cost}";
                    await _dialogManager.ShowDialog(player, message);
                    break;
                }
            case Tile.Type.CORNER:
                {
                    CornerHandler handler = tile.Handler as CornerHandler;

                    //3 is always the jail corner
                    if (handler.CornerNumber != 3)
                        return;

                    int jailIndex = Board.Size / 4;
                    await player.Jail(jailIndex);
                    break;
                }
            default:
                PrintErr("unknown tile type");
                break;
        }
    }

    public bool BuyTile(Player player, int tileIndex)
    {
        Tile tile = Board.GetTile(tileIndex);
        TradeableHandler handler = tile.Handler as TradeableHandler;

        if (!tile.IsBuyable())
        {
            Print("you cant buy this tile");
            return false;
        }

        if (handler.PlayerOwner != null)
        {
            Print("already owned tile");
            return false;
        }

        if (player.Money < handler.Price)
        {
            Print("you dont have enough money");
            return false;
        }

        player.Money -= handler.Price;
        AssignTile(player, tile);

        Print("player ", player.Index, " purchased tile ", handler.Label, " at ", handler.Price);
        return true;
    }

    //TODO: show error dialogs
    public bool BuyHouse(Player player, int tileIndex)
    {
        Tile tile = Board.GetTile(tileIndex);
        TradeableHandler handler = tile.Handler as TradeableHandler;

        int price = handler.HousePrice;

        if (tile.TileType != Tile.Type.PROPERTY)
        {
            Print("you can only buy houses on property tiles");
            return false;
        }

        if (!handler.IsOwner(player))
        {
            Print("this is not your property");
            return false;
        }

        if (player.Money < price)
        {
            Print("you dont have enough money");
            return false;
        }

        if (!player.HasGroup(handler.Group))
        {
            Print("you dont have a full tile group yet");
            return false;
        }

        if (handler.AddHouse())
        {
            player.Money -= price;
            return true;
        }

        Print("you cant buy more houses");
        return false;
    }

    //TODO: print signals
    public static void AssignTile(Player player, Tile tile)
    {
        TradeableHandler handler = tile.Handler as TradeableHandler;

        if (handler.PlayerOwner != null)
        {
            handler.PlayerOwner.RemoveTile(tile);
        }

        handler.PlayerOwner = player;
        player.AddTile(tile);

        Print(handler.Label, " is now owned by player ", player.Id);
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
