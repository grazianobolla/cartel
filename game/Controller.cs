using Godot;
using System;

public class Controller : Node
{
    [Signal]
    public delegate void OnAction(int playerId, Instruction instruction);
    public enum Instruction { NONE, SHAKE, BUY, BUY_HOUSE, OMIT };

    private AirConsole _airconsole;

    public override void _Ready()
    {
        _airconsole = (AirConsole)GetNode("/root/AirConsole");

        if (!_airconsole.ready)
            return;

        _airconsole.Connect("OnMessage", this, "AirconsoleControllerMessage");
    }

    private void AirconsoleControllerMessage(int deviceId, Godot.Object data)
    {
        string str = (string)data.Get("instruction");
        Instruction instruction = Instruction.NONE;
        Enum.TryParse<Instruction>(str, out instruction);
        EmitSignal(nameof(OnAction), deviceId, instruction); //FIXME: convert device id!!!!
    }
}
