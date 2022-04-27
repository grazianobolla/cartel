using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class AirConsole : Node
{
    [Signal]
    public delegate void OnMessage(int deviceId, Godot.Collections.Dictionary data);
    [Signal]
    public delegate void OnConnect(int deviceId);
    [Signal]
    public delegate void OnDisconnect(int deviceId);
    [Signal]
    public delegate void OnActivePlayersChange(int playerNumber);

    private JavaScriptObject airconsole;
    private List<JavaScriptObject> callbacks = new List<JavaScriptObject>();
    public bool ready = false;

    private JavaScriptObject CreateCallback(String func)
    {
        JavaScriptObject cb = JavaScript.CreateCallback(this, func);
        callbacks.Add(cb);
        return cb;
    }

    public override void _Ready()
    {
        if (OS.GetName() != "HTML5")
        {
            Print("can't load airconsole, not on a HTML environment");
            return;
        }

        JavaScript.Eval("airconsole = new AirConsole({device_motion: 50})");
        airconsole = JavaScript.GetInterface("airconsole");

        ConnectCallbacks();
        Print("airconsole initialized");
        ready = true;
    }

    private void ConnectCallbacks()
    {
        airconsole.Set("onMessage", CreateCallback("cbOnMessage"));
        airconsole.Set("onConnect", CreateCallback("cbOnConnect"));
        airconsole.Set("onDisconnect", CreateCallback("cbOnDisconnect"));
        airconsole.Set("onActivePlayersChange", CreateCallback("cbOnActivePlayersChange"));
    }

    private void cbOnMessage(Godot.Collections.Array args)
    {
        EmitSignal(nameof(OnMessage), args[0], args[1]);
    }

    private void cbOnConnect(int deviceId)
    {
        EmitSignal(nameof(OnDisconnect), deviceId);
    }

    private void cbOnDisconnect(int deviceId)
    {
        EmitSignal(nameof(OnDisconnect), deviceId);
    }

    private void cbOnActivePlayersChange(int playerNumber)
    {
        EmitSignal(nameof(OnActivePlayersChange), playerNumber);
    }
}