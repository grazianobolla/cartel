using Godot;
using System;

public class AirConsoleInterface : Node
{
    private AirConsole _airConsole;
    private PlayerManager _playerManager;

    public override void _Ready()
    {
        _airConsole = (AirConsole)GetNode("/root/AirConsole");

        _playerManager = (PlayerManager)GetNode("/root/Game/PlayerManager");
        _playerManager.Connect("AddedPlayer", this, "OnPlayerAdded");
    }

    public void DisplayDialog(int playerId, string text)
    {
        if (!_airConsole.ready)
            return;

        JavaScriptObject data = (JavaScriptObject)JavaScript.CreateObject("Object");
        data.Set("instruction", "display-dialog");
        data.Set("content", text);
        _airConsole.Message(_airConsole.ConvertPlayerNumberToDeviceId(playerId), data);
    }

    private void OnPlayerAdded(Player player, int playerCount)
    {
        player.Connect("MoneyChange", this, "DisplayUpdateMoney");
    }

    private void DisplayUpdateMoney(int playerId, int value)
    {
        if (!_airConsole.ready)
            return;

        JavaScriptObject data = (JavaScriptObject)JavaScript.CreateObject("Object");
        data.Set("instruction", "update-money");
        data.Set("content", value);
        _airConsole.Message(_airConsole.ConvertPlayerNumberToDeviceId(playerId), data);
    }
}
