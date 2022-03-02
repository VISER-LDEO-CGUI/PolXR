using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CSVReadPlot : MonoBehaviour
{
    // The scale factors for the coordinates.
    public float[] scaleFactor = { 1, 1, 1 };
    public int[] columnNumbers = { 2, 5, 3 };
    public string fileRoot = "Assets/Resources/SplitByLine_All_Lines_DICE_SurfElev/";

    // The prefab for the data points to be instantiated.
    public ParticleSystem PSLine;
    public Color PSColor;
    public float ColorMid = -0.2f;
    public float ColorRange = 10.0f;
    public Transform Parent;

    // Start is called before the first frame update
    void Start()
    {
        // Set the default attributes for the particle system.
        var main = PSLine.main;
        main.startLifetime = 86400f;
        main.startSpeed = 0f;

        // Get the file names.
        // Need to fix the file path problem.
        string[] files = System.IO.Directory.GetFiles(fileRoot);

        foreach(var file in files)
        {
            if (file[file.Length - 1] == 'v')
            {
                // Create lines and parent objects if needed.
                ParticleSystem newLine = Instantiate(PSLine, Parent);
                newLine.name = file.Substring(fileRoot.Length);

                SetParticles(newLine, file);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetParticles(ParticleSystem line, string file)
    {
        // Need to fix the file path problem.
        string[] data = System.IO.File.ReadAllText(file).Split("\n"[0]);

        // Setting the default behavior of the particle system.
        ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[data.Length - 1];
        var main = line.main;
        main.maxParticles = data.Length - 1;
        line.Emit(data.Length - 1);
        line.GetParticles(CSVPoints);
        int inRange = 0;

        // Set the particle position and the color.
        for (int i = 1; i < data.Length - 1; i++)
        {
            // Get and compute the coordinates.
            string[] coords = data[i].Split(","[0]);
            float x = float.Parse(coords[columnNumbers[0]]) * scaleFactor[0];
            float y = float.Parse(coords[columnNumbers[1]]) * scaleFactor[1];
            float z = float.Parse(coords[columnNumbers[2]]) * scaleFactor[2];

            // Set individual particles.
            if (x > -9000 & y > -9000 & z > -9000)
            {
                CSVPoints[inRange].position = new Vector3(x, y, z);
                CSVPoints[inRange].startColor = new Color(PSColor.r, (y - ColorMid) / ColorRange, PSColor.b, 1.0f);
                inRange += 1;
            }
        }

        line.SetParticles(CSVPoints, inRange);
    }
}
