using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class AirConsole : Node
{
    [Signal]
    public delegate void OnMessage(int deviceId, Godot.Object data);
    [Signal]
    public delegate void OnConnect(int deviceId);

    public bool ready = false;

    private JavaScriptObject _airconsole;
    private List<JavaScriptObject> _callbacks = new List<JavaScriptObject>();

    public override void _Ready()
    {
        if (OS.GetName() != "HTML5")
        {
            Print("can't load airconsole, not on a HTML environment");
            return;
        }

        JavaScript.Eval("airconsole = new AirConsole({device_motion: 50})");
        _airconsole = JavaScript.GetInterface("airconsole");

        ConnectCallbacks();
        Print("airconsole initialized");
        ready = true;
    }

    public int ConvertDeviceIdToPlayerNumber(int deviceId)
    {
        object playerNumber = _airconsole.Call("convertDeviceIdToPlayerNumber", deviceId);
        return (int)playerNumber;
    }

    public void SetActivePlayers(int maxPlayers)
    {
        _airconsole.Call("setActivePlayers", maxPlayers);
    }

    private void ConnectCallbacks()
    {
        _airconsole.Set("onMessage", CreateCallback("cbOnMessage"));
        _airconsole.Set("onConnect", CreateCallback("cbOnConnect"));
    }

    private JavaScriptObject CreateCallback(String func)
    {
        JavaScriptObject cb = JavaScript.CreateCallback(this, func);
        _callbacks.Add(cb);
        return cb;
    }

    private void cbOnMessage(Godot.Collections.Array args)
    {
        int playerId = ConvertDeviceIdToPlayerNumber((int)args[0]);
        EmitSignal("OnMessage", playerId, args[1]);
    }

    private void cbOnConnect(int deviceId)
    {
        EmitSignal("OnConnect", deviceId);
    }
}