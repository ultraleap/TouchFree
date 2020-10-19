using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class DataLogger : MonoBehaviour
{

    public string fileName;
    public int bufferSize = 1;

    Queue<List<float>> dataPoints;

    System.IO.StreamWriter fileStream;
    bool fileOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        dataPoints = new Queue<List<float>>();
        fileOpened = false;   
    }

    public void LogData(List<float> dataPoint)
    {
        dataPoints.Enqueue(dataPoint);
        
        if (dataPoints.Count >= bufferSize)
        {
            WriteBuffer();
        }

    }

    void WriteBuffer()
    {
        if(!fileOpened)
        {
            string filePath = System.IO.Directory.GetCurrentDirectory() + "/" + fileName;
            fileStream = File.CreateText(filePath);
            fileOpened = true;
        }
        while (dataPoints.Count > 0)
        {
            WriteLine(dataPoints.Dequeue());
        }
    }
    
    void WriteLine(List<float> data)
    {
        string lineString = "";
        foreach(float element in data)
        {
            lineString += element.ToString() + ",";
        }
        fileStream.WriteLine(lineString);
    }

    void OnDestroy()
    {
        WriteBuffer();
        fileStream.Close();
    }

}
