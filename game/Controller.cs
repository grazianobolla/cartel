using Godot;
using System;
using static Godot.GD;

//TODO: remove debug stuff here
public class Controller : Node
{
    [Signal] public delegate void OnAction(int playerId, Controller.Action action, Godot.Collections.Array arguments);
    [Signal] public delegate void DebugShake(int index);

    //FIXME: change duplicated actions for arguments
    public enum Action
    {
        NONE, SHAKE, BUY, BUY_HOUSE, OMIT,
        TILE_SELECTOR_FWD, TILE_SELECTOR_BKW,
        DIALOG_ACCEPT, DIALOG_CANCEL,
        TRADE
    };

    private AirConsole _airConsole;
    private AirConsoleInterface _airConsoleInterface;

    public override void _Ready()
    {
        _airConsole = (AirConsole)GetNode("/root/AirConsole");
        _airConsoleInterface = (AirConsoleInterface)GetNode("/root/Game/AirConsoleInterface");

        if (!_airConsole.ready)
            return;

        _airConsole.Connect("OnMessage", this, "OnAirconsoleControllerMessage");
        Print("airconsole connected");
    }

    //handles airconsole received messages
    private void OnAirconsoleControllerMessage(int playerNumber, Godot.JavaScriptObject data)
    {
        Godot.Collections.Array argumentArray = data.ToArray();

        string actionStr = (string)argumentArray[0];
        Action action = Action.NONE;

        if (!Enum.TryParse<Action>(actionStr, out action))
        {
            PrintErr("action ", actionStr, " unknown");
            return;
        }

        //remove instruction and leave only instruction arguments
        argumentArray.RemoveAt(0);

        EmitSignal(nameof(OnAction), playerNumber, action, argumentArray);
    }

    public int GetControllerCount()
    {
        return _airConsoleInterface.ControllerCount;
    }

    public string GetPlayerNickname(int playerId)
    {
        return _airConsoleInterface.GetPlayerNickname(playerId);
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
