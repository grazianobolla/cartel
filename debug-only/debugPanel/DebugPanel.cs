using Godot;

public class DebugPanel : Control
{
    private Game _game;
    private Controller _controller;
    private PlayerManager _playerManager;
    private CameraController _camera;
    private DialogManager _dialog;
    private AirConsole _airconsole;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _game = (Game)GetParent();
        _playerManager = (PlayerManager)GetNode("/root/Game/PlayerManager");
        _camera = (CameraController)GetNode("/root/Game/GameCamera");
        _dialog = (DialogManager)GetNode("/root/Game/DialogManager");
        _airconsole = (AirConsole)GetNode("/root/AirConsole");

        // if (OS.GetName() == "HTML5")
        // {
        //     Visible = false;
        // }
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
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.BUY_HOUSE, null);
    }

    private void _on_OmitButton_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.OMIT, null);
    }

    private void _on_StartGame_pressed()
    {
        _game.CreateGame();
    }

    private void _on_CameraOverview_pressed()
    {
        _camera.Overview();
    }

    private void _on_CameraFocus_pressed()
    {
        _camera.Focus(PlayerManager.GetPlayer(0));
    }

    private void _on_Left_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.TILE_SELECTOR_BKW, null);
    }

    private void _on_Right_pressed()
    {
        _controller.SendDebugControllerMessage(_game.CurrentPlayerId, Controller.Action.TILE_SELECTOR_FWD, null);
    }

    private int _pc = 0;
    private void _on_AddPlayer_pressed()
    {
        _playerManager.AddPlayer(_pc, 1500, "Jhonny");
        _pc++;
    }

    private void _on_DialogCancel_pressed()
    {
        int id = GetNode<TextEdit>("VBoxContainer/HBoxContainer5/TextEdit").Text.ToInt();
        _controller.SendDebugControllerMessage(id, Controller.Action.DIALOG_CANCEL, null);
    }

    private void _on_DialogAccept_pressed()
    {
        int id = GetNode<TextEdit>("VBoxContainer/HBoxContainer5/TextEdit").Text.ToInt();
        _controller.SendDebugControllerMessage(id, Controller.Action.DIALOG_ACCEPT, null);
    }
}
