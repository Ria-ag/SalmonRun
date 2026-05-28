using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WaveReceiver : MonoBehaviour{
    [Header("Network Configuration")]
    [SerializeField] private int port = 5005;
    // Background thread management
    private Thread receiveThread;
    private UdpClient client;
    private bool isRunning = false;

    // Thread-safe state variables to pass data to the Main Unity Thread
    private bool pendingLeftWave = false;
    private bool pendingRightWave = false;
    private readonly object lockObject = new object();

    void Start(){
        // Start the background network thread on game launch
        isRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log($"[UDP] Listening for Python wave gestures on port {port}...");
    }

    void Update(){
        // Check for triggers on the main thread every frame (Thread-Safe check)
        lock (lockObject){
            if (pendingLeftWave){
                TriggerLeftWaveAction();
                pendingLeftWave = false;
            }
            if (pendingRightWave){
                TriggerRightWaveAction();
                pendingRightWave = false;
            }
        }
    }

    // This runs completely in the background so it never hitches your frame rate
    private void ReceiveData(){
        try{
            client = new UdpClient(port);
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            while (isRunning){
                // This blocks the background thread until a packet arrives from Python
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data).Trim();

                // Lock and set flags so the Update loop can see them safely
                lock (lockObject){
                    if (text == "WAVE_LEFT"){
                        pendingLeftWave = true;
                    }
                    else if (text == "WAVE_RIGHT"){
                        pendingRightWave = true;
                    }
                }
            }
        }
        catch (Exception e){
            // Thread closing down handles throw exceptions gracefully on exit
            if (isRunning) Debug.LogError($"[UDP Error] {e.Message}");
        }
    }

    private void TriggerLeftWaveAction(){
        Debug.Log("[GAMEPLAY] Left Wave Received! Spawning Left Ecosystem Sweep Current.");
        
        // Call the placeholder visual script
        if (DebrisSweep.Instance != null){
            DebrisSweep.Instance.ClearLeftPollutants();
        }
    }

    private void TriggerRightWaveAction(){
        Debug.Log("[GAMEPLAY] Right Wave Received! Spawning Right Ecosystem Sweep Current.");
        
        // Call the placeholder visual script
        if (DebrisSweep.Instance != null){
            DebrisSweep.Instance.ClearRightPollutants();
        }
    }

    // CRITICAL: Clean up sockets when exiting or stopping the game in the editor!
    private void OnDisable(){
        CloseSockets();
    }

    private void OnApplicationQuit(){
        CloseSockets();
    }

    private void CloseSockets(){
        isRunning = false;
        if (client != null){
            client.Close();
        }
        if (receiveThread != null && receiveThread.IsAlive){
            receiveThread.Abort();
        }
    }
}