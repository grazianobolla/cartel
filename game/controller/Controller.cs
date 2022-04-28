using Godot;
using System;

//TODO: remove debug stuff here
public class Controller : Node
{
    [Signal] public delegate void OnAction(int playerId, Action action, Godot.Object data);
    [Signal] public delegate void DebugShake(int index);

    public enum Action { NONE, SHAKE, BUY, BUY_HOUSE, OMIT };

    private AirConsole _airconsole;

    public override void _Ready()
    {
        ConnectAirConsole();
    }

    private void ConnectAirConsole()
    {
        _airconsole = (AirConsole)GetNode("AirConsole");

        if (!_airconsole.ready)
            return;

        _airconsole.Connect("OnMessage", this, "OnAirconsoleControllerMessage");
        _airconsole.SetActivePlayers(8); //TODO: .
    }

    private void OnAirconsoleControllerMessage(int playerNumber, Godot.Object data)
    {
        string str = (string)data.Get("instruction");
        Action action = Action.NONE;

        if (Enum.TryParse<Action>(str, out action))
            EmitSignal(nameof(OnAction), playerNumber, action);
    }

    public void SendDebugControllerMessage(int playerNumber, Action instruction)
    {
        EmitSignal(nameof(OnAction), playerNumber, instruction, null);
    }

    public void SendDebugShakeMethod(int index)
    {
        EmitSignal(nameof(DebugShake), index);
    }
}
