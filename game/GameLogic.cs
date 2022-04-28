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

    private GameTemplate _template;
    private PlayerManager _playerManager;

    public override void _Ready()
    {
        Randomize();
        GetNode("/root/Controller").Connect("OnAction", this, nameof(OnReceiveAction));
        GetNode("/root/Controller").Connect("DebugShake", this, nameof(OnDebugShake));
        _playerManager = (PlayerManager)GetNode("PlayerManager");
        CreateGame();
    }

    public void CreateGame(String templatePath = "res://templates/template0.json")
    {
        _template = new GameTemplate(templatePath);

        if (!_template.Check())
            PrintErr("could not validate template");

        int startingMoney = _template.GetStartingMoney();

        GetNode<BoardGenerator>("BoardGenerator").GenerateFromTemplate(_template);

        _playerManager.AddPlayer(startingMoney);
        _playerManager.AddPlayer(startingMoney);
    }

    private uint GetDiceNumber()
    {
        return Randi() % 12 + 1;
    }

    private void OnDebugShake(int index)
    {
        
        Player player = GetCurrentPlayer();
        StartCycle(player.GetDistanceTo(index));
    }

    private void OnReceiveAction(int playerId, Controller.Instruction instruction)
    {
        if (playerId != currentPlayerId)
            return;

        Player player = GetCurrentPlayer();

        switch (instruction)
        {
            case Controller.Instruction.SHAKE:
                StartCycle((int)GetDiceNumber());
                break;

            case Controller.Instruction.BUY:
                if (currentState != State.INTERACTING)
                    return;

                BuyTile(player);
                break;

            case Controller.Instruction.BUY_HOUSE:
                if (currentState != State.INTERACTING)
                    return;

                BuyHouse(player);
                break;

            case Controller.Instruction.OMIT:
                if (currentState != State.INTERACTING)
                    return;

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

        currentPlayerId = _playerManager.GetNextId(currentPlayerId);

        while (GetCurrentPlayer().CanPlay() == false)
            currentPlayerId = _playerManager.GetNextId(currentPlayerId);

        currentState = State.WAITING;
    }

    private async Task MoveState(Player player, int moveAmount)
    {
        int initialIndex = player.index;
        await player.Move(moveAmount);
        if (player.index <= initialIndex)
        {
            player.money += 200;
            Print("player ", player.id, " get start bonus");
        }
    }

    private Player GetCurrentPlayer()
    {
        return _playerManager.GetPlayer(currentPlayerId);
    }
}