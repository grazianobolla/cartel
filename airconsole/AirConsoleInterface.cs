using Godot;
using System;

public class AirConsoleInterface : Node
{
    public int ControllerCount = 0;

    private AirConsole _airConsole;
    private PlayerManager _playerManager;
    private Game _game;

    public override void _Ready()
    {
        _airConsole = (AirConsole)GetNode("/root/AirConsole");
        _airConsole.Connect("OnConnect", this, nameof(OnControllerConnect));
        _airConsole.Connect("OnDisconnect", this, nameof(OnControllerDisconnect));

        _playerManager = (PlayerManager)GetNode("/root/Game/PlayerManager");
        _playerManager.Connect("AddedPlayer", this, "OnPlayerAdded");

        _game = (Game)GetNode("/root/Game");
        _game.Connect("FinshedTurn", this, nameof(OnGameTurnFinish));
        _game.Connect("PlayerProcessing", this, nameof(OnGamePlayerProcessing));
    }

    public void DisplayDialog(int playerId, string text)
    {
        if (!_airConsole.ready)
            return;

        text = Marshalls.Utf8ToBase64(text); //encode text on base64, easier to handle on the controller side

        JavaScriptObject data = (JavaScriptObject)JavaScript.CreateObject("Object");
        data.Set("instruction", "dialog-view");
        data.Set("content", text);
        _airConsole.Message(_airConsole.ConvertPlayerNumberToDeviceId(playerId), data);
    }

    public string GetPlayerNickname(int playerId)
    {
        return _airConsole.GetNickname(_airConsole.ConvertPlayerNumberToDeviceId(playerId));
    }

    public void SetControllerView(int playerId, string view)
    {
        if (!_airConsole.ready)
            return;

        JavaScriptObject data = (JavaScriptObject)JavaScript.CreateObject("Object");
        data.Set("instruction", view);
        _airConsole.Message(_airConsole.ConvertPlayerNumberToDeviceId(playerId), data);
    }

    private void OnPlayerAdded(Player player)
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

    private void OnControllerConnect(int deviceId)
    {
        ControllerCount++;
        _airConsole.SetActivePlayers(ControllerCount);
    }

    private void OnControllerDisconnect(int deviceId)
    {
        ControllerCount--;
        //_airConsole.SetActivePlayers(ControllerCount);
    }

    private void OnGameTurnFinish(int nextPlayerId)
    {
        SetControllerView(nextPlayerId, "dice-view");
    }

    private void OnGamePlayerProcessing(int playerId)
    {
        SetControllerView(playerId, "panel-view");
    }
}
