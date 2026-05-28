using UnityEngine;

public class SalmonMovement : MonoBehaviour
{
    public float speed = 2f;

    private bool canMove = true;

    void Update()
    {
        if (canMove)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    public void StopMoving()
    {
        canMove = false;
    }

    public void StartMoving()
    {
        canMove = true;
    }
}