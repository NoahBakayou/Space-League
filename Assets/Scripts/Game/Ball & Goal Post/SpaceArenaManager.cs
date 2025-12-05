using UnityEngine;

public class SpaceArenaManager : MonoBehaviour
{
    public SpaceBall ball;
    public Transform ballSpawnPoint;

    [Header("Ball Spawn Position")]
    public float x = 0;
    public float y = 0;
    public float z = 0; 

    public int blueScore = 0;
    public int redScore = 0;

    private void Start()
    {
        foreach (var goal in FindObjectsOfType<GoalTrigger>())
        {
            goal.OnGoalScored.AddListener(HandleGoal);
        }
    }

    private void HandleGoal(SpaceBall.Team goalTeam)
    {
        // Team that scored is opposite team
        if (goalTeam == SpaceBall.Team.Blue)
        {
            redScore++;
        }
        else if (goalTeam == SpaceBall.Team.Red)
        {
            blueScore++;
        }
           
        Debug.Log($"Score: BLUE {blueScore} - RED {redScore}");

        ResetBall();
    }

    private void ResetBall()
    {
        ballSpawnPoint.position = new Vector3(x, y, z);
        ball.ResetBall(ballSpawnPoint.position);

        Physics.SyncTransforms();
    }
}