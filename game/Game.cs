using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class Game : Spatial
{
    [Signal] private delegate void FinishedInteraction();

    public enum State { WAITING, MOVING, PROCESSING, INTERACTING };
    public State currentState { get; private set; } = State.WAITING;
    public int currentPlayerId { get; private set; } = 0;

    private GameTemplate _template;
    private PlayerManager _playerManager;
    private TileInteraction _tileInteraction;

    public override void _Ready()
    {
        _tileInteraction = new TileInteraction();
        _playerManager = (PlayerManager)GetNode("PlayerManager");

        Randomize();
        CreateGame();
        ConnectSignals();
    }

    public void CreateGame(String templatePath = "res://templates/template0.json")
    {
        _template = new GameTemplate(templatePath);

        if (!_template.Check())
            PrintErr("could not validate template");

        GetNode<BoardGenerator>("BoardGenerator").GenerateFromTemplate(_template);
    }

    private void ConnectSignals()
    {
        GetNode("/root/Controller").Connect("OnAction", this, nameof(OnReceiveAction));
        GetNode("/root/Controller").Connect("DebugShake", this, nameof(OnDebugShake));
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

                _tileInteraction.BuyTile(player);
                break;

            case Controller.Instruction.BUY_HOUSE:
                if (currentState != State.INTERACTING)
                    return;

                _tileInteraction.BuyHouse(player);
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
        await _tileInteraction.ProcessLanding(player, _playerManager, _template);

        if (player.CanPlay())
        {
            currentState = State.INTERACTING;
            await ToSignal(this, nameof(FinishedInteraction));
        }

        NextPlayerTurn();

        //TODO: if all players land in jail this enters
        //a infinite loop!
        while (GetCurrentPlayer().CanPlay() == false)
            NextPlayerTurn();

        currentState = State.WAITING;
    }

    private async Task MoveState(Player player, int moveAmount)
    {
        int initialIndex = player.index;
        await player.Move(moveAmount);
        if (player.index <= initialIndex)
        {
            player.money += 200; //TODO: load from template
            Print("player ", player.id, " get start bonus");
        }
    }

    private Player GetCurrentPlayer()
    {
        return _playerManager.GetPlayer(currentPlayerId);
    }

    private void NextPlayerTurn()
    {
        currentPlayerId = _playerManager.GetNextId(currentPlayerId);
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
}