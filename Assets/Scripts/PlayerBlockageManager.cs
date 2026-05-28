using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlockageManager : MonoBehaviour
{
    public GameObject[] blockages;
    public SalmonPathFollower salmon;

    public Key testKey = Key.Space;

    private int currentBlockageIndex = 0;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            ClearCurrentBlockage();
        }
    }

    public void ClearCurrentBlockage()
    {
        if (currentBlockageIndex >= blockages.Length)
        {
            Debug.Log(gameObject.name + " has no blockages left");
            return;
        }

        blockages[currentBlockageIndex].SetActive(false);
        currentBlockageIndex++;

        salmon.ContinueAfterBlockage();

        Debug.Log(gameObject.name + " cleared blockage");
    }
}