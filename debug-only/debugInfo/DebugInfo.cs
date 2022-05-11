using Godot;
using System;

public class DebugInfo : Control
{
    [Export] private NodePath _gamePath;
    [Export] private NodePath _playerManagerPath;
    [Export] private NodePath _cameraPath;
    [Export] private NodePath _tileSelectorPath;

    private Game _game;
    private PlayerManager _playerManager;
    private Font _defaultFont;
    private CameraController _camera;
    private TileSelector _tileSelector;

    public override void _Ready()
    {
        _game = (Game)GetNode(_gamePath);
        _playerManager = (PlayerManager)GetNode(_playerManagerPath);
        _defaultFont = this.GetFont("font");
        _camera = (CameraController)GetNode(_cameraPath);
        _tileSelector = (TileSelector)GetNode(_tileSelectorPath);
    }

    public override void _PhysicsProcess(float delta) { Update(); }

    public override void _Draw()
    {
        DrawString(_defaultFont, new Vector2(8, 16), $"current player {_game.CurrentPlayerId}, current state {_game.CurrentState}");
        DrawString(_defaultFont, new Vector2(8, 32), $"camera state {_camera.currentState}, fov {_camera.Fov}");
        DrawString(_defaultFont, new Vector2(8, 48), $"tile selector index {_tileSelector.CurrentIndex} enabled {_tileSelector.Enabled}");

        for (int i = 0; i < _playerManager.PlayerList.Count; i++)
        {
            Player player = _playerManager.GetPlayer(i);

            DrawString(_defaultFont, new Vector2(8, 16 * i + 64),
            $"player {player.Id}, index {player.Index}, state {player.PlayerState}, money {player.Money}, props. {player.OwnedTiles.Count}, jail {player.JailTime}, canPlay() {player.CanPlay()}"
            );
        }
    }
}
