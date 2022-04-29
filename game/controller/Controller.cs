using Godot;
using System;

//TODO: remove debug stuff here
public class Controller : Node
{
    [Signal] public delegate void OnAction(int playerId, Controller.Action action, Godot.Collections.Array arguments);
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
    }

    private void OnAirconsoleControllerMessage(int playerNumber, Godot.JavaScriptObject data)
    {
        var argumentArray = data.ToArray();

        string actionStr = (string)argumentArray[0];
        Action action = Action.NONE;
        if (!Enum.TryParse<Action>(actionStr, out action))
            return;

        var arguments = data.GetAllButFirst().ToArray();

        EmitSignal(nameof(OnAction), playerNumber, action, arguments);
    }

    public void SetActivePlayers(int amount)
    {
        _airconsole.SetActivePlayers(amount);
    }

    public void SendDebugControllerMessage(int playerNumber, Action instruction, Godot.Collections.Array data)
    {
        EmitSignal(nameof(OnAction), playerNumber, instruction, data);
    }

    public void SendDebugShakeMethod(int index)
    {
        EmitSignal(nameof(DebugShake), index);
    }
}
