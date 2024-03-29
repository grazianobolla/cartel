using Godot;
using System;
using System.Collections.Generic;
using static Godot.GD;

public class AirConsole : Node
{
    [Signal] public delegate void OnMessage(int deviceId, Godot.Object data);
    [Signal] public delegate void OnConnect(int deviceId);
    [Signal] public delegate void OnDisconnect(int deviceId);

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

        if (_airconsole == null)
        {
            PrintErr("can't eval airconsole");
            return;
        }

        ConnectCallbacks();
        ready = true;
        Print("airconsole initialized");
    }

    private object SafeCall(string method, params object[] args)
    {
        if (!ready)
            return null;

        return _airconsole.Call(method, args);
    }

    public int ConvertDeviceIdToPlayerNumber(int deviceId)
    {
        object playerNumber = SafeCall("convertDeviceIdToPlayerNumber", deviceId);
        return (int)playerNumber;
    }

    public int ConvertPlayerNumberToDeviceId(int playerId)
    {
        object playerNumber = SafeCall("convertPlayerNumberToDeviceId", playerId);
        return (int)playerNumber;
    }

    public Godot.Collections.Array GetActivePlayerDeviceIds()
    {
        JavaScriptObject array = (JavaScriptObject)SafeCall("getActivePlayerDeviceIds");
        return array.ToArray();
    }

    public void SetActivePlayers(int maxPlayers)
    {
        SafeCall("setActivePlayers", maxPlayers);
    }

    public void Message(int deviceId, JavaScriptObject obj)
    {
        SafeCall("message", deviceId, obj);
    }

    public void Broadcast(JavaScriptObject obj)
    {
        SafeCall("broadcast", obj);
    }

    public string GetNickname(int deviceId)
    {
        return (string)SafeCall("getNickname", deviceId);
    }

    private JavaScriptObject CreateCallback(String func)
    {
        JavaScriptObject cb = JavaScript.CreateCallback(this, func);
        _callbacks.Add(cb);
        return cb;
    }

    private void ConnectCallbacks()
    {
        _airconsole.Set("onMessage", CreateCallback("cbOnMessage"));
        _airconsole.Set("onConnect", CreateCallback("cbOnConnect"));
        _airconsole.Set("onDisconnect", CreateCallback("cbOnDisconnect"));
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

    private void cbOnDisconnect(int deviceId)
    {
        EmitSignal("OnDisconnect", deviceId);
    }
}