using UnityEngine;
using UnityEngine.InputSystem;

public class HoleManager : MonoBehaviour
{
    public GameObject[] holes;

    private int currentHoleIndex = 0;

    void Update()
    {
        // Keyboard test before Arduino is connected.
        // Press space to hide the next hole.
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ClearNextHole();
        }
    }

    public void ClearNextHole()
    {
        if (currentHoleIndex >= holes.Length)
        {
            Debug.Log("All holes cleared");
            return;
        }

        holes[currentHoleIndex].SetActive(false);
        currentHoleIndex++;

        Debug.Log("Cleared hole " + currentHoleIndex);
    }

    public void ResetHoles()
    {
        currentHoleIndex = 0;

        for (int i = 0; i < holes.Length; i++)
        {
            holes[i].SetActive(true);
        }

        Debug.Log("Reset all holes");
    }
}