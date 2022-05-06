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
    private TileInteractor _tileInteractor;
    private CameraController _camera;

    public override void _Ready()
    {
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
        GetNode("/root/Controller").Connect("OnAction", this, nameof(OnReceiveAction));
        //TODO: remove
        GetNode("/root/Controller").Connect("DebugShake", this, nameof(OnDebugShake));
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
        if (currentPlayerId != playerId)
            return;

        switch (action)
        {
            case Controller.Action.SHAKE:
                StartCycle((int)GetDiceNumber());
                break;

            default:
                if (currentState != State.INTERACTING)
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

        if (currentState != State.WAITING)
            return;

        //Iterates players and checks for things like
        //jail time reduction etc.
        _playerManager.CheckTurn();

        //Camera focus the player
        _camera.Focus(player);

        await ToSignal(GetTree().CreateTimer(1), "timeout");

        //Player moves
        currentState = State.MOVING;
        await MoveState(player, diceNumber);

        //Process landing, does things like
        //charge player depending on landing tile,
        //sending the player to prision etc.
        currentState = State.PROCESSING;
        await _tileInteractor.ProcessLanding(player, _template);

        await ToSignal(GetTree().CreateTimer(2), "timeout");

        //If the player is allowed to play, it is presented
        //with an intreaction menu.
        if (player.CanPlay())
        {
            _camera.Overview();
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
            //TODO: load from template
            player.Money += 200;
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