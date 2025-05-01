using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveHistory : MonoBehaviour
{
    private List<SpaceBody> moveHistory;
    private int moveIndex;

    public MoveHistory()
    {
        moveHistory = new() { null };
        moveIndex = 0;
    }

    public void AddMove(SpaceBody target)
    {
        if (!(moveIndex == moveHistory.Count - 1))
        {
            moveHistory.RemoveRange(moveIndex + 1, (moveHistory.Count - (moveIndex + 1)));
        }
        moveHistory.Add(target);
        moveIndex++;
    }

    public void UndoMove()
    {
        if (moveIndex > 0)
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.GAME_BUTTON);
            moveIndex--;
            MoveToTarget();
        }
        else SoundFX.PlayAudioClip(SoundFX.AudioType.WARNING);
    }

    public void RedoMove()
    {
        if (moveIndex < moveHistory.Count - 1)
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.GAME_BUTTON);
            moveIndex++;
            MoveToTarget();
        }
        else SoundFX.PlayAudioClip(SoundFX.AudioType.WARNING);
    }

    private void MoveToTarget()
    {
        SpaceBody target = moveHistory.ElementAt(moveIndex);
        if (target == null) UniverseHandler.MoveToUniverseView();
        else if (target.IsStar()) UniverseHandler.MoveToStar((Star)target);
        else if (target.IsPlanet()) UniverseHandler.MoveToPlanet((Planet)target);
    }
}
