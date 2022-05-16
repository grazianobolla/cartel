using Godot;
using static Godot.GD;
using System.Threading.Tasks;

public class DialogManager : Node
{
    [Export] private NodePath _airConsoleInterfacePath;
    [Signal] private delegate void OnDialogInteraction();

    private Controller _controller;
    private AirConsoleInterface _airConsoleInterface;

    private int _receiverPlayerId = -1;
    private bool _dialogResponse = false;
    private bool _inUse = false;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _controller.Connect("OnAction", this, nameof(OnControllerAction));

        _airConsoleInterface = (AirConsoleInterface)GetNode(_airConsoleInterfacePath);
    }

    private void OnControllerAction(int playerId, Controller.Action action, Godot.Collections.Array arguments)
    {
        if (_receiverPlayerId != playerId)
            return;

        switch (action)
        {
            case Controller.Action.ACCEPT_TRADE:
                _dialogResponse = true;
                EmitSignal(nameof(OnDialogInteraction));
                break;
            case Controller.Action.REJECT_TRADE:
                _dialogResponse = false;
                EmitSignal(nameof(OnDialogInteraction));
                break;

            default:
                break;
        }
    }

    //Shows a request dialog on players controller,
    //first bool == error, second bool == response
    public async Task<(bool, bool)> ShowDialog(Player player, string message = "This is a dialog, will you accept?")
    {
        if (_inUse)
        {
            PrintErr("trying to send a dialog while not ready");
            return (false, false);
        }

        _inUse = true;
        Print("showing dialog to player ", player.Id);
        _receiverPlayerId = player.Id;
        _airConsoleInterface.DisplayDialog(player.Id, message);
        await ToSignal(this, nameof(OnDialogInteraction));
        _inUse = false;
        return (true, _dialogResponse);
    }
}
