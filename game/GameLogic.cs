using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class GameLogic : Spatial
{
    [Signal]
    delegate void FinishedInteraction();

    public enum State { WAITING, MOVING, PROCESSING, INTERACTING };
    public State currentState = State.WAITING;
    public int currentPlayerId = 0;

    private GameTemplate template;
    private PlayerManager playerManager;

    public override void _Ready()
    {
        Randomize();
        GetNode("DebugPanel").Connect("key_pressed", this, "ProcessKey");
        playerManager = (PlayerManager)GetNode("PlayerManager");
        CreateGame();
    }

    public void CreateGame(String templatePath = "res://templates/template0.json")
    {
        template = new GameTemplate(templatePath);

        if (!template.Check())
            PrintErr("could not validate template");

        int startingMoney = template.GetStartingMoney();

        GetNode<BoardGenerator>("BoardGenerator").GenerateFromTemplate(template);

        playerManager.AddPlayer(startingMoney);
        playerManager.AddPlayer(startingMoney);
    }

    private uint GetDiceNumber()
    {
        return Randi() % 12 + 1;
    }

    private void ProcessKey(int playerId, Godot.Collections.Dictionary data)
    {
        if (playerId != currentPlayerId)
            return;

        switch (data["instruction"])
        {
            case "shake":
                StartCycle((int)GetDiceNumber());
                break;

            case "interact":
                Interact((String)data["action"]);
                break;
        }
    }

    private void Interact(String action)
    {
        if (currentState != State.INTERACTING)
            return;

        Player player = GetCurrentPlayer();

        switch (action)
        {
            case "buy":
                BuyTile(player);
                break;

            case "buy-house":
                if (BuyHouse(player))
                {
                    EmitSignal(nameof(FinishedInteraction));
                }
                break;

            case "omit":
                EmitSignal(nameof(FinishedInteraction));
                break;
        }
    }

    private async void StartCycle(int diceNumber)
    {
        if (currentState != State.WAITING)
            return;

        Player player = GetCurrentPlayer();

        if (!player.CanPlay())
            return;

        currentState = State.MOVING;
        await MoveState(player, diceNumber);
        currentState = State.PROCESSING;
        await ProcessLanding(player);

        if (player.CanPlay())
        {
            currentState = State.INTERACTING;
            await ToSignal(this, nameof(FinishedInteraction));
        }

        currentPlayerId = playerManager.GetNextId(currentPlayerId);

        while (GetCurrentPlayer().CanPlay() == false)
            currentPlayerId = playerManager.GetNextId(currentPlayerId);

        currentState = State.WAITING;
    }

    private async Task MoveState(Player player, int moveAmount)
    {
        int initialIndex = player.index;
        await player.Move(moveAmount);
        if (player.index <= initialIndex)
        {
            player.Money += 200;
            Print("player ", player.id, " get start bonus");
        }
    }

    private Player GetCurrentPlayer()
    {
        return playerManager.GetPlayer(currentPlayerId);
    }
}