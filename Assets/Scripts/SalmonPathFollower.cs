using UnityEngine;

public class SalmonPathFollower : MonoBehaviour
{
    public Transform[] pathPoints;
    public Transform[] stopPoints;

    public float speed = 2f;
    public float reachDistance = 0.08f;

    public RaceManager raceManager;
    public int playerNumber = 1;

    private int currentPathIndex = 0;
    private int currentStopIndex = 0;

    private bool stoppedAtBlockage = false;
    private bool finished = false;

    void Start()
    {
        if (pathPoints.Length > 0)
        {
            transform.position = pathPoints[0].position;
            currentPathIndex = 1;
        }
    }

    void Update()
    {
        if (finished || stoppedAtBlockage)
        {
            return;
        }

        if (raceManager != null && !raceManager.IsRaceActive())
        {
            return;
        }

        MoveToNextPoint();
    }

    void MoveToNextPoint()
    {
        if (currentPathIndex >= pathPoints.Length)
        {
            finished = true;

            Debug.Log(gameObject.name + " finished!");

            if (raceManager != null)
            {
                raceManager.PlayerFinished(playerNumber);
            }

            return;
        }

        Transform target = pathPoints[currentPathIndex];

        Vector3 moveDirection = target.position - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (Vector3.Distance(transform.position, target.position) <= reachDistance)
        {
            CheckIfStopPoint(target);
            currentPathIndex++;
        }
    }

    void CheckIfStopPoint(Transform reachedPoint)
    {
        if (currentStopIndex >= stopPoints.Length)
        {
            return;
        }

        if (reachedPoint == stopPoints[currentStopIndex])
        {
            stoppedAtBlockage = true;
            Debug.Log(gameObject.name + " stopped at blockage");
        }
    }

    public void ContinueAfterBlockage()
    {
        if (!stoppedAtBlockage)
        {
            return;
        }

        stoppedAtBlockage = false;
        currentStopIndex++;

        Debug.Log(gameObject.name + " continued");
    }

    public bool IsStoppedAtBlockage()
    {
        return stoppedAtBlockage;
    }

    public bool IsFinished()
    {
        return finished;
    }
}