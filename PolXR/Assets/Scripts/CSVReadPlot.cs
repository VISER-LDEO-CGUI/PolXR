using System;
using System.IO;
using UnityEngine;

public class CSVReadPlot : MonoBehaviour
{
    // The scale factors for the coordinates.
    public float[] scaleFactor = { 1, 1, 1 };
    public int[] columnNumbers = { 2, 5, 3 };
    public string fileRoot = "SplitByLine_All_Lines_DICE_SurfElev";

    // The prefab for the data points to be instantiated.
    public ParticleSystem PSLine;
    public Color PSColor;
    public float ColorMid = -0.2f;
    public float ColorRange = 10.0f;
    public Transform Parent;
    public Transform RadarImages;
    public GameObject radarSample;
    public string radarFile;
    public int radarXIndex, radarYIndex, radarHeightIndex;

    // The parent for the DEMs.
    public Transform Dems;
    public string BedName;
    public string SurName;
    public Material BedMat;
    public Material SurMat;

    // Start is called before the first frame update
    void Start()
    {
        // Load the images here. Should have one start and one end.
        TextAsset radarPos = (TextAsset)Resources.Load(radarFile, typeof(TextAsset));

        if (radarPos != null)
        {
            string[] radarDatas = radarPos.text.Split("\n"[0]);
            Vector3 radarStart = new Vector3(0f, -1.75f, 0f);
            Vector3 radarEnd = new Vector3(0f, -1.75f, 0f);

            // Ignore the first line which is the name of the columns.
            int indexCounter = 1;

            while (indexCounter < radarDatas.Length - 1)
            {
                string[] radarData = radarDatas[indexCounter++].Split(","[0]);
                GameObject radarImage = Instantiate(radarSample, RadarImages);
                radarImage.name = radarData[0];

                // This should be the start of the radar.
                radarStart.x = float.Parse(radarData[radarXIndex]);
                //radarStart.y = float.Parse(radarData[radarHeightIndex]);
                radarStart.z = float.Parse(radarData[radarYIndex]);

                radarData = radarDatas[indexCounter++].Split(","[0]);

                // This should be the end of the radar.
                radarEnd.x = float.Parse(radarData[radarXIndex]);
                //radarEnd.y = float.Parse(radarData[radarHeightIndex]);
                radarEnd.z = float.Parse(radarData[radarYIndex]);

                radarImage.transform.localPosition = (radarStart + radarEnd) / 2;

                // Identify the prefix for x-oriented or z-oriented radar images.
                if (radarImage.name[0] == 'T')
                {
                    radarImage.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
                    radarImage.transform.localScale = new Vector3(Math.Abs(radarEnd.z - radarStart.z), 5.5f, 0.01f);
                }
                else radarImage.transform.localScale = new Vector3(Math.Abs(radarEnd.x - radarStart.x), 5.5f, 0.01f);
            }
        }

        // Loading DEM models.
        GameObject DemBed = Instantiate(Resources.Load("Prefabs/" + BedName), Dems) as GameObject;
        DemBed.transform.GetChild(0).GetComponent<Renderer>().material = BedMat;
        DemBed.layer = LayerMask.NameToLayer("Both Camera");
        DemBed.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Both Camera");
        GameObject DemSur = Instantiate(Resources.Load("Prefabs/" + SurName), Dems) as GameObject;
        DemSur.transform.GetChild(0).GetComponent<Renderer>().material = SurMat;
        DemSur.layer = LayerMask.NameToLayer("Both Camera");
        DemSur.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Both Camera");
        Debug.Log("CSV Read Plot.cs loading DEM bed called");

        // Set the default attributes for the particle system.
        var main = PSLine.main;
        main.startLifetime = 86400f;
        main.startSpeed = 0f;

        // Load all the files within the directory.
        TextAsset[] files = Array.ConvertAll(Resources.LoadAll(fileRoot, typeof(TextAsset)), asset => (TextAsset)asset);

        foreach (TextAsset file in files)
        {
            ParticleSystem newLine = Instantiate(PSLine, Parent);
            SetParticles(newLine, file);
        }
    }

    // Add a new PS Line to a radar image.
    public void AddPSLine(Transform radarImage)
    {
        ParticleSystem newLine = Instantiate(PSLine, Parent);
        radarImage.GetComponent<RadarEvents2D>().SetLine(newLine.transform, 0);
    }

    private void SetParticles(ParticleSystem line, TextAsset file)
    {
        // Split the input test by line and set the name of the line.
        string[] data = file.text.Split("\n"[0]);
        string label = data[1].Split(","[0])[0];

        // Setting the default behavior of the particle system.
        ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[data.Length - 1];
        var main = line.main;
        main.maxParticles = data.Length - 1;
        line.Emit(data.Length - 1);
        line.GetParticles(CSVPoints);
        int inRange = 0;

        // Set the particle position and the color.
        // Ignore the first line which is the name of the columns.
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
                CSVPoints[inRange].startColor = PSColor;
                // changes color based on height, but currently, heights are too similar, need to modify it if needed.
                // CSVPoints[inRange].startColor = new Color(PSColor.r, (y - ColorMid) / ColorRange, PSColor.b, 1.0f);
                inRange += 1;
            }
        }

        main.maxParticles = inRange;
        line.SetParticles(CSVPoints, inRange);

        // Assign corredponding CSV line to radar images.
        Transform radarImage = RadarImages.Find(label);
        if (radarImage != null) 
            radarImage.GetComponent<RadarEvents2D>().SetLine(line.transform, inRange);
        else line.name = label;
    }

    // Function to save the radar images' positions.
    public void SaveScene()
    {
        string radarInfo = "Name, Position, Scale, Rotation\n";
        foreach (Transform radarImage in RadarImages)
        {
            radarInfo += radarImage.name + ";";
            radarInfo += radarImage.localPosition.ToString("F3") + ";";
            radarInfo += radarImage.localScale.ToString("F3") + ";";
            radarInfo += radarImage.localEulerAngles.ToString("F3") + "\n";
        }

        var saveFile = File.CreateText("Assets/Resources/Save.txt");
        saveFile.WriteLine(radarInfo);
        saveFile.Close();
    }

    // Function to load the radar images' positions.
    public void LoadScene()
    {
        TextAsset SaveFile = (TextAsset)Resources.Load("Save", typeof(TextAsset));

        if (SaveFile != null)
        {
            string[] radaInfos = SaveFile.text.Split("\n"[0]);

            // Ignore the first line which is the name of the columns.
            int indexCounter = 1;

            while (indexCounter < radaInfos.Length - 1)
            {
                string[] radarInfo = radaInfos[indexCounter++].Split(";"[0]);
                Transform radarImage = RadarImages.Find(radarInfo[0]);
                if (radarImage != null)
                {
                    radarImage.localPosition = ToVector3(radarInfo[1]);
                    radarImage.localScale = ToVector3(radarInfo[2]);
                    radarImage.localEulerAngles = ToVector3(radarInfo[3]);
                }
            }
        }
    }

    // Parse a vector3 type.
    private Vector3 ToVector3(string input)
    {
        string[] digits = input.Substring(1, input.Length - 2).Split(',');
        return new Vector3(float.Parse(digits[0]), float.Parse(digits[1]), float.Parse(digits[2]));
    }
}
