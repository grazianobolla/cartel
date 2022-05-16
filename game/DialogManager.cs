using Godot;
using static Godot.GD;
using System.Threading.Tasks;

public class DialogManager : Node
{
    [Export] private NodePath _airConsoleInterfacePath;
    [Signal] private delegate void OnDialogInteraction(bool response);

    private Controller _controller;
    private AirConsoleInterface _airConsoleInterface;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _airConsoleInterface = (AirConsoleInterface)GetNode(_airConsoleInterfacePath);
    }

    //Shows a request dialog on players controller,
    //return bool indicates player response
    public async Task<bool> ShowDialog(Player player, string message = "This is a dialog, will you accept?")
    {
        Print("showing dialog to player ", player.Id);
        _airConsoleInterface.DisplayDialog(player.Id, message);

        while (true)
        {
            var args = await ToSignal(_controller, "OnAction");
            if ((int)args[0] != player.Id)
                continue;

            Controller.Action action = (Controller.Action)args[1];

            if (action == Controller.Action.DIALOG_ACCEPT)
            {
                return true;
            }
            else if (action == Controller.Action.DIALOG_CANCEL)
            {
                return false;
            }
        }
    }
}
