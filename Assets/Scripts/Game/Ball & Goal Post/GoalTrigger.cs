using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Add this script to an invisible goal volume to marked as goal 

public class GoalTrigger : MonoBehaviour
{
    public SpaceBall.Team goalBelongsToTeam;

    [System.Serializable]
    public class GoalScoredEvent : UnityEvent<SpaceBall.Team> { }
    public GoalScoredEvent OnGoalScored;

    private bool goalLocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (goalLocked)
        {
            return;
        }

        if (other.TryGetComponent<SpaceBall>(out var ball))
        {
            goalLocked = true;
            // Prevent scoring on your own goal
            if (ball.lastTouchedTeam == goalBelongsToTeam)
            {
                return;
            }
            
            OnGoalScored?.Invoke(goalBelongsToTeam);

            StartCoroutine(UnlockGoal());
        }
    }

    private IEnumerator UnlockGoal()
    {
        yield return new WaitForSeconds(1f);
        goalLocked = false;
    }
}