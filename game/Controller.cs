using Godot;
using System;

public class Controller : Node
{
    [Signal]
    public delegate void OnAction(int playerId, Instruction instruction);
    [Signal]
    public delegate void DebugShake(int index);

    public enum Instruction { NONE, SHAKE, BUY, BUY_HOUSE, OMIT };

    private AirConsole _airconsole;

    public override void _Ready()
    {
        ConnectAirConsole();
    }

    private void ConnectAirConsole()
    {
        _airconsole = (AirConsole)GetNode("/root/AirConsole");

        if (!_airconsole.ready)
            return;

        _airconsole.Connect("OnMessage", this, "AirconsoleControllerMessage");
        _airconsole.SetActivePlayers(2); //TODO: this
    }

    private void AirconsoleControllerMessage(int playerNumber, Godot.Object data)
    {
        string str = (string)data.Get("instruction");
        Instruction instruction = Instruction.NONE;

        if (Enum.TryParse<Instruction>(str, out instruction))
            EmitSignal(nameof(OnAction), playerNumber, instruction);
    }

    public void DebugControllerMessage(int playerNumber, Instruction instruction)
    {
        EmitSignal(nameof(OnAction), playerNumber, instruction);
    }

    public void DebugShakeMethod(int index)
    {
        EmitSignal(nameof(DebugShake), index);
    }
}
