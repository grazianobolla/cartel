using Godot;
using System;

public class DebugInfo : Control
{
    [Export] private NodePath _gamePath;
    [Export] private NodePath _playerManagerPath;
    [Export] private NodePath _cameraPath;

    private Game _game;
    private PlayerManager _playerManager;
    private Font _defaultFont;
    private CameraController _camera;

    public override void _Ready()
    {
        _game = (Game)GetNode(_gamePath);
        _playerManager = (PlayerManager)GetNode(_playerManagerPath);
        _defaultFont = this.GetFont("font");
        _camera = (CameraController)GetNode(_cameraPath);
    }

    public override void _PhysicsProcess(float delta) { Update(); }

    public override void _Draw()
    {
        DrawString(_defaultFont, new Vector2(8, 16), $"current player {_game.currentPlayerId}, current state {_game.currentState}");
        DrawString(_defaultFont, new Vector2(8, 32), $"camera state {_camera.currentState}, fov {_camera.Fov}");

        for (int i = 0; i < _playerManager.playersList.Count; i++)
        {
            Player player = _playerManager.GetPlayer(i);

            DrawString(_defaultFont, new Vector2(8, 16 * i + 64),
            $"player {player.id}, index {player.index}, state {player.state}, money {player.Money}, props. {player.ownedTiles.Count}, jail {player.jailTime}, canPlay() {player.CanPlay()}"
            );
        }
    }
}
