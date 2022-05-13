using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class Player
{
    private bool _isMoving = false;

    public void StickToCurrentTile()
    {
        //If the player is still, make it stick to the current tile.
        if (!_isMoving)
        {
            this.Translation = Board.GetTileTransform(Index).origin + _posOffset;
        }
    }
    public async Task Move(int amount)
    {
        Print("moving by ", amount, " places");
        _isMoving = true;
        await AnimateForward(amount);
        Index = Board.GetShiftedIndex(Index, amount);
        _isMoving = false;
    }

    public async Task MoveTo(int toIndex)
    {
        int amount = GetDistanceTo(toIndex);
        await Move(amount);
    }

    //TODO: public for debug
    public int GetDistanceTo(int toIndex)
    {
        if (toIndex >= Index)
            return toIndex - Index;

        return (int)(Board.Size - (Index - toIndex));
    }
}
