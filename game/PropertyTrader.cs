using Godot;
using System.Threading.Tasks;
using static Godot.GD;

public class PropertyTrader : Node
{
    [Export] private NodePath _controllerPath;
    [Export] private NodePath _airConsoleInterfacePath;

    [Signal] private delegate void PlayerInteractedWithTrade();
    private Controller _controller;
    private AirConsoleInterface _airConsoleInterface;

    private int _currentTradeReceiverId = -1;

    public override void _Ready()
    {
        _airConsoleInterface = (AirConsoleInterface)GetNode(_airConsoleInterfacePath);

        _controller = (Controller)GetNode(_controllerPath);
        _controller.Connect("OnAction", this, "OnControllerAction");
    }

    private void OnControllerAction(int playerId, Controller.Action action, Godot.Collections.Array arguments)
    {
        if (playerId == _currentTradeReceiverId)
        {

        }
    }

    public async Task TradeProperty(Tile property, Player from, Player to, int price)
    {
        if (property.TileType != Tile.Type.PROPERTY || property.TileType != Tile.Type.STATE)
        {
            PrintErr("bad tile type");
            return;
        }

        if (!property.IsOwner(from))
        {
            PrintErr("property trade wrong owner");
            return;
        }

        if (property.IsOwner(to))
        {
            PrintErr("target player already owns property");
            return;
        }

        if (to.Money < price)
        {
            //TODO: send notification
            Print("target player doesn't have enough money");
            return;
        }


    }

    private async void SendTradeRequest(Player to, Tile property, int price)
    {
        currentTradeReceiverId = to.Id;
        _airConsoleInterface.ShowTradeDialog(to.Id);
        await ToSignal(this, nameof(PlayerInteractedWithTrade));
    }
}
