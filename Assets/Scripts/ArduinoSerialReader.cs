using UnityEngine;
using System.IO.Ports;

public class ArduinoSerialReader : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 115200;

    public PlayerBlockageManager player1BlockageManager;
    public PlayerBlockageManager player2BlockageManager;

    public float clearCooldown = 0.5f;

    private SerialPort serialPort;

    private bool lastFSRPressedP1 = false;
    private bool lastFSRPressedP2 = false;

    private float lastClearTimeP1 = 0f;
    private float lastClearTimeP2 = 0f;

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();

            Debug.Log("Connected to Arduino on " + portName);
        }
        catch
        {
            Debug.LogWarning("Could not open serial port " + portName);
        }
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            return;
        }

        try
        {
            string line = serialPort.ReadLine();
            string[] values = line.Split(',');

            if (values.Length < 7)
            {
                return;
            }

            bool touchingFoil = values[0] == "1";
            bool timerActive = values[1] == "1";
            float secondsLeft = float.Parse(values[2]);

            bool currentFSRPressedP1 = values[3] == "1";
            int fsrValueP1 = int.Parse(values[4]);

            bool currentFSRPressedP2 = values[5] == "1";
            int fsrValueP2 = int.Parse(values[6]);

            // Player 1: detect weight lifted.
            if (lastFSRPressedP1 && !currentFSRPressedP1)
            {
                if (Time.time - lastClearTimeP1 > clearCooldown)
                {
                    player1BlockageManager.ClearCurrentBlockage();
                    lastClearTimeP1 = Time.time;
                }
            }

            // Player 2: detect weight lifted.
            if (lastFSRPressedP2 && !currentFSRPressedP2)
            {
                if (Time.time - lastClearTimeP2 > clearCooldown)
                {
                    player2BlockageManager.ClearCurrentBlockage();
                    lastClearTimeP2 = Time.time;
                }
            }

            lastFSRPressedP1 = currentFSRPressedP1;
            lastFSRPressedP2 = currentFSRPressedP2;

            Debug.Log(
                "P1 FSR: " + fsrValueP1 + " Pressed: " + currentFSRPressedP1 +
                " | P2 FSR: " + fsrValueP2 + " Pressed: " + currentFSRPressedP2
            );
        }
        catch
        {
            // Ignore messy serial reads.
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}