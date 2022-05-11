using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class Game : Spatial
{
    [Signal] private delegate void FinishedInteraction();

    public enum State { WAITING, MOVING, PROCESSING, INTERACTING };
    public State CurrentState { get; private set; } = State.WAITING;
    public int CurrentPlayerId { get; private set; } = 0;

    private Controller _controller;
    private GameTemplate _template;
    private PlayerManager _playerManager;
    private TileInteractor _tileInteractor;
    private CameraController _camera;

    public override void _Ready()
    {
        _controller = (Controller)GetNode("/root/Controller");
        _tileInteractor = (TileInteractor)GetNode("TileInteractor");
        _playerManager = (PlayerManager)GetNode("PlayerManager");
        _camera = (CameraController)GetNode("GameCamera");

        Randomize();
        CreateGame();
        ConnectSignals();

        _camera.Overview();
    }

    private void ConnectSignals()
    {
        _controller.Connect("OnAction", this, nameof(OnReceiveAction));
        _controller.Connect("DebugShake", this, nameof(OnDebugShake));

        _playerManager.Connect("AddedPlayer", this, "OnPlayerAdded");
    }

    private void OnPlayerAdded(Player player, int playerCount)
    {
        GetNode<AirConsole>("/root/AirConsole").SetActivePlayers(playerCount);
    }

    public void CreateGame(String templatePath = "res://templates/template0.json")
    {
        _template = new GameTemplate(templatePath);

        if (!_template.Check())
            PrintErr("could not validate template");

        GetNode<BoardGenerator>("BoardGenerator").GenerateFromTemplate(_template);
    }

    //Called when a player requests an interaction with the game
    private void OnReceiveAction(int playerId, Controller.Action action, Godot.Collections.Array arguments)
    {
        if (CurrentPlayerId != playerId)
            return;

        switch (action)
        {
            case Controller.Action.SHAKE:
                StartCycle((int)GetDiceNumber());
                break;

            default:
                if (CurrentState != State.INTERACTING)
                    return;

                if (_tileInteractor.ProcessInteraction(GetCurrentPlayer(), action, arguments))
                    EmitSignal(nameof(FinishedInteraction));

                break;
        }
    }

    //Starts a game cycle with the current player
    private async void StartCycle(int diceNumber)
    {
        Player player = GetCurrentPlayer();

        if (CurrentState != State.WAITING)
            return;

        //Iterates players and checks for things like
        //jail time reduction etc.
        _playerManager.CheckTurn();

        //Camera focus the player
        _camera.Focus(player);

        await ToSignal(GetTree().CreateTimer(1), "timeout");

        //Player moves
        CurrentState = State.MOVING;
        await MoveState(player, diceNumber);

        //Process landing, does things like
        //charge player depending on landing tile,
        //sending the player to prision etc.
        CurrentState = State.PROCESSING;
        await _tileInteractor.ProcessLanding(player, _template);

        await ToSignal(GetTree().CreateTimer(2), "timeout");

        //If the player is allowed to play, it is presented
        //with an intreaction menu.
        if (player.CanPlay())
        {
            CurrentState = State.INTERACTING;
            _tileInteractor.EnableTileSelection();
            await ToSignal(this, nameof(FinishedInteraction));
            _tileInteractor.DisableTileSelection();
        }

        NextPlayerTurn();

        //TODO: if all players land in jail this enters
        //a infinite loop!
        while (GetCurrentPlayer().CanPlay() == false)
            NextPlayerTurn();

        CurrentState = State.WAITING;
    }

    private async Task MoveState(Player player, int moveAmount)
    {
        int initialIndex = player.Index;
        await player.Move(moveAmount);
        if (player.Index <= initialIndex)
        {
            //TODO: load from template
            player.Money += 200;
            Print("player ", player.Id, " get start bonus");
        }
    }

    private Player GetCurrentPlayer()
    {
        return _playerManager.GetPlayer(CurrentPlayerId);
    }

    private void NextPlayerTurn()
    {
        CurrentPlayerId = _playerManager.GetNextId(CurrentPlayerId);
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