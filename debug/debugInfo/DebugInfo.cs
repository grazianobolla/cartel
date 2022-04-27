using Godot;
using System;

public class DebugInfo : Control
{
    [Export]
    private NodePath gameLogicPath;

    private GameLogic gameLogic;
    private PlayerManager playerManager;
    private Font defaultFont;

    public override void _Ready()
    {
        gameLogic = (GameLogic)GetNode(gameLogicPath);
        playerManager = (PlayerManager)GetNode("/root/GameLogic/PlayerManager");
        defaultFont = this.GetFont("font");
    }

    public override void _PhysicsProcess(float delta) { Update(); }

    public override void _Draw()
    {
        DrawString(defaultFont, new Vector2(8, 16), $"current player {gameLogic.currentPlayerId}, current state {gameLogic.currentState}");

        for (int i = 0; i < playerManager.playersList.Count; i++)
        {
            Player player = playerManager.GetPlayer(i);

            DrawString(defaultFont, new Vector2(8, 16 * i + 32),
            $"player {player.id}, index {player.index}, state {player.state}, money {player.Money}, props. {player.ownedTiles.Count}, jail {player.jailTime}, canPlay() {player.CanPlay()}"
            );
        }
    }
}
