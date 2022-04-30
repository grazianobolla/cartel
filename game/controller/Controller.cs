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
        Godot.Collections.Array argumentArray = data.ToArray();

        string actionStr = (string)argumentArray[0];
        Action action = Action.NONE;
        if (!Enum.TryParse<Action>(actionStr, out action))
            return;

        //remove instruction and leave only instruction arguments
        argumentArray.RemoveAt(0);

        EmitSignal(nameof(OnAction), playerNumber, action, argumentArray);
    }

    //TODO: move this to a specific AirConsole interface
    public void UpdateMoneyLabel(int playerId, int value)
    {
        if (!_airconsole.ready)
            return;

        JavaScriptObject data = (JavaScriptObject)JavaScript.CreateObject("Object");
        data.Set("instruction", "update-money");
        data.Set("content", value);
        _airconsole.Message(_airconsole.ConvertPlayerNumberToDeviceId(playerId), data);
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
