using UnityEngine;

public class DebrisSweep : MonoBehaviour{
    public static DebrisSweep Instance;

    private void Awake(){
        // Simple Singleton pattern so the UDP script can find this easily
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ClearLeftPollutants(){
        Debug.Log("⚡ Executing Visual Polish: LEFT Current Sweep!");
        
        // Spawn a temporary primitive cube to act as a placeholder visual effect
        GameObject sweepVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sweepVisual.transform.position = new Vector3(-4f, 0f, 0f); // Left side of screen
        sweepVisual.GetComponent<Renderer>().material.color = Color.cyan;
        
        // Make it slide across the screen and disappear
        Destroy(sweepVisual, 1.5f); 
    }

    public void ClearRightPollutants(){
        Debug.Log("⚡ Executing Visual Polish: RIGHT Current Sweep!");
        
        // Spawn a temporary primitive cube on the right side
        GameObject sweepVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sweepVisual.transform.position = new Vector3(4f, 0f, 0f); // Right side of screen
        sweepVisual.GetComponent<Renderer>().material.color = Color.blue;
        
        Destroy(sweepVisual, 1.5f);
    }
}