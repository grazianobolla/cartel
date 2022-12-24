using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class Game : Spatial
{
    [Signal] public delegate void StartedTurn();
    [Signal] public delegate void PlayerProcessing(int playerId);
    [Signal] public delegate void PlayerInteracting(int nextPlayerId);
    [Signal] public delegate void FinshedTurn(int nextPlayerId);
    [Signal] public delegate void BeginCreateGame();
    [Signal] public delegate void CreatedGame(int firstPlayerId);
    [Signal] public delegate void CrossedStart(int playerId);

    [Signal] private delegate void FinishedInteraction();

    public enum State { WAITING, MOVING, PROCESSING, INTERACTING };
    public State CurrentState { get; private set; } = State.WAITING;
    public int CurrentPlayerId { get; private set; } = 0;
    public bool AwaitingInteraction { get; private set; } = false;

    private Controller _controller;
    private GameTemplate _template;
    private PlayerManager _playerManager;
    private PlayerTileInteraction _tileInteractor;
    private CameraController _camera;
    private Board _board;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _tileInteractor = (PlayerTileInteraction)GetNode("TileInteractor");
        _playerManager = (PlayerManager)GetNode("PlayerManager");
        _camera = (CameraController)GetNode("GameCamera");
        _board = (Board)GetNode("Board");

        Randomize();
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        _controller.Connect("OnAction", this, nameof(OnReceiveAction));
        _controller.Connect("DebugShake", this, nameof(OnDebugShake));
    }

    public void CreateGame(String templatePath = "res://templates/template0.json")
    {
        EmitSignal(nameof(BeginCreateGame));

        _template = new GameTemplate(templatePath);

        if (_template.Check())
        {
            _board.GenerateFromTemplate(_template);
            SpawnPlayers();
            _camera.Overview();
            EmitSignal(nameof(CreatedGame), CurrentPlayerId);
            return;
        }

        PrintErr("could not validate template");
    }

    //Called when a player requests an interaction with the game
    private async void OnReceiveAction(int playerId, Controller.Action action, Godot.Collections.Array arguments)
    {
        if (AwaitingInteraction)
            return;

        if (CurrentPlayerId != playerId)
            return;

        switch (action)
        {
            case Controller.Action.SHAKE:
                StartCycle((int)GetDiceNumber());
                break;

            default:
                if (CurrentState == State.INTERACTING)
                {
                    AwaitingInteraction = true;
                    bool finished = await _tileInteractor.ProcessInteraction(GetCurrentPlayer(), action, arguments);

                    if (finished)
                        EmitSignal(nameof(FinishedInteraction));

                    AwaitingInteraction = false;
                }
                break;
        }
    }

    //Starts a game cycle with the current player
    private async void StartCycle(int diceNumber)
    {
        Player player = GetCurrentPlayer();

        if (CurrentState != State.WAITING)
            return;

        //First state of the cycle
        EmitSignal(nameof(StartedTurn));
        CurrentState = State.MOVING;

        //Camera focus the player
        _camera.Focus(player);
        await ToSignal(GetTree().CreateTimer(1), "timeout");

        //Player moves
        await MoveState(player, diceNumber);

        //Second state of the cycle
        EmitSignal(nameof(PlayerProcessing), CurrentPlayerId);
        CurrentState = State.PROCESSING;

        //Process landing, does things like
        //charge player depending on landing tile,
        //sending the player to prision etc.
        await _tileInteractor.ProcessLanding(player, _template);
        await ToSignal(GetTree().CreateTimer(1), "timeout");

        //If the player is allowed to play, it is presented
        //with an intreaction menu.
        if (player.CanPlay())
        {
            //Third state of the cycle
            EmitSignal(nameof(PlayerInteracting), CurrentPlayerId);
            CurrentState = State.INTERACTING;

            _camera.Overview();
            _playerManager.ToggleNameTags(true);

            _tileInteractor.EnableTileSelection(player.Index);
            await ToSignal(this, nameof(FinishedInteraction));
            _tileInteractor.DisableTileSelection();

            _playerManager.ToggleNameTags(false);
        }

        _camera.Overview();

        NextPlayerTurn();

        //TODO: if all players land in jail this enters
        //a infinite loop!
        while (GetCurrentPlayer().CanPlay() == false)
            NextPlayerTurn();

        CurrentState = State.WAITING;

        EmitSignal(nameof(FinshedTurn), CurrentPlayerId);
    }

    //Moves the player and checks for start bonus
    private async Task MoveState(Player player, int moveAmount)
    {
        int initialIndex = player.Index;
        await player.Move(moveAmount);
        if (player.Index <= initialIndex)
        {
            //TODO: load from template
            player.Money += 200;
            EmitSignal(nameof(CrossedStart));
            Print("player ", player.Id, " get start bonus");
        }
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < _controller.GetControllerCount(); i++)
        {
            int startingMoney = _template.GetStartingMoney();
            string nickname = _controller.GetPlayerNickname(i);
            _playerManager.AddPlayer(i, startingMoney, nickname);
        }
    }

    private Player GetCurrentPlayer()
    {
        return PlayerManager.GetPlayer(CurrentPlayerId);
    }

    private void NextPlayerTurn()
    {
        CurrentPlayerId = _playerManager.GetNextId(CurrentPlayerId);
        Print("turn of player ", CurrentPlayerId);
    }

    private uint GetDiceNumber()
    {
        return (Randi() % 12) + 1;
    }

    private void OnDebugShake(int index)
    {
        Player player = GetCurrentPlayer();
        StartCycle(player.GetDistanceTo(index));
    }
}