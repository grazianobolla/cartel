using Godot;

public class DebugPanel : Control
{
    private Game _game;
    private Controller _controller;
    private PlayerManager _playerManager;
    private CameraController _camera;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _game = (Game)GetParent();
        _playerManager = (PlayerManager)GetNode("/root/Game/PlayerManager");
        _camera = (CameraController)GetNode("/root/Game/GameCamera");
    }

    private void _on_ShakeButton_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.SHAKE, null);
    }

    private void _on_BuyButton_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.BUY, null);
    }

    private void _on_BuyHouseButton_pressed()
    {
        int index = 0;
        int.TryParse(GetNode<TextEdit>("VBoxContainer/HBoxContainer2/TextEdit").Text, out index);
        Godot.Collections.Array dataArray = new Godot.Collections.Array { index };
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.BUY_HOUSE, dataArray);
    }

    private void _on_OmitButton_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.OMIT, null);
    }

    private void _on_AddPlayer_pressed()
    {
        _playerManager.AddPlayer(1000);
    }

    private void _on_CameraOverview_pressed()
    {
        _camera.Overview();
    }

    private void _on_CameraFocus_pressed()
    {
        _camera.Focus(_playerManager.GetPlayer(0));
    }

    private void _on_Left_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.BUTTON_LEFT, null);
    }

    private void _on_Right_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.BUTTON_RIGHT, null);
    }
}
