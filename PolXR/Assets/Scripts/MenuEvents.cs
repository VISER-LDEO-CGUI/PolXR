////using Microsoft.MixedReality.Toolkit;
////using Microsoft.MixedReality.Toolkit.UI;
////using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using System;
//using System.IO;
//using UnityEngine.SceneManagement;

//public class MenuEvents : MonoBehaviour
//{
//    // The current menu type, true stands for main menu.
//    private bool isMainMenu;
//    public bool isLinePickingMode;
//    public Transform SubMenuRadar;
//    public Transform SubMenuMain;

//    public Transform SubMenuLinePicking;

//    // The workflow used for the current radarParent.
//    public int workflow;

//    // Initially set to empty objects to avoid null reference.
//    private Transform radarParent = null;
//    private Transform radargram = null; // this is the same as radarParent in workflow 2
//    public Transform RadarImageContainer;
//    public Transform DEMs;
//    public Transform CSVPicksContainer; // only for workflow 2
//    public Transform Location;

//    // The data needed for smoothing the menu movement.
//    private Vector3 targetPosition;
//    private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
//    private bool updatePosition = true;

//    // The sliders.
//    //public PinchSlider XSlider;
//    //public PinchSlider YSlider;
//    //public PinchSlider ZSlider;
//    //public PinchSlider rotationSlider;
//    //public PinchSlider transparencySlider;
//    //public PinchSlider verticalExaggerationSlider;

//    // The initial scale, rotation and position of the radar image.
//    private Vector3 originalScale;
//    private float scaleX = 1.0f;
//    private float scaleY = 1.0f;
//    private float scaleZ = 1.0f;

//    // The scale for calculating the text value
//    public float scale = 1000;

//    // Text objects
//    public TextMeshPro Title;
//    public TextMeshPro YTMP; // TMP stands for TextMeshPro
//    public TextMeshPro XTMP;
//    public TextMeshPro ZTMP;
//    public TextMeshPro RotationDegreeTMP;
//    public TextMeshPro TransparencyTMP;
//    public TextMeshPro MarkTMP;
//    public TextMeshPro MeasureModeText;

//    // Radar Menu Toggle Buttons
//    //public Interactable RadarToggle;
//    //public Interactable CSVPicksToggle;
//    //public Interactable AllRadarToggle;
//    //public Interactable AllCSVPicksToggle;
//    //public Interactable MeasurementToggle;
//    //public Interactable SecondMeasurementToggle;
//    //public Interactable SurfaceToggle;
//    //public Interactable BedToggle;
//    //public Interactable BoxToggle;

//    // The information needed for updating the selected point coordinates.
//    public GameObject MarkObj;
//    public Color MarkColor;
//    public GameObject MeasureObj;
//    public GameObject MeasureLine;
//    public string SelectionDialog = "Assets/dialog.txt";
//    private float yOrigin = 1.75f / 5.5f;

//    // The minimap plate.
//    public GameObject Minimap;

//    // The particle system for showing CSV pick lines (only workflow 2).
//    public ParticleSystem PSLine;

//    // Variables for scene-swapping
//    public string[] scenePaths;
//    readonly int HOMESCREEN_INDEX = 0;

//    // Deals with muting/unmuting sounds
//    public AudioSource audioSource;

//    void Start()
//    {
//        // Deactivate the radar menu before any selection happens; deactivate the bounding box.
//        HomeButton(true);
//        MeasureLine.SetActive(false);
//        if (SceneManager.GetActiveScene().name == "greenland") Minimap.SetActive(false); // fix later
//        Minimap.GetComponent<BoxCollider>().enabled = false;
//        MarkObj.transform.parent = Location.transform;
//        MarkObj.SetActive(false);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (!isMainMenu)
//        {

//            // Update the rotation slider value accordingly.
//            float rounded_angle = (float)(radargram.localRotation.eulerAngles.y / 360.0f);
//            rounded_angle = rounded_angle >= 0 ? rounded_angle : rounded_angle + 1.0f;
//            //if (Mathf.Abs(rotationSlider.SliderValue - rounded_angle) > 0.01f)
//            //    rotationSlider.SliderValue = rounded_angle;

//            // Set scaled dimensions text
//            YTMP.text = string.Format(
//                "Original:   {0} m \n" +
//                "Current:    {1} m \n" +
//                "Strain:     {2}",
//                (originalScale.y * scale).ToString(),
//                (radargram.localScale.y * scale).ToString(),
//                (Math.Abs(originalScale.y - radargram.localScale.y) * scale).ToString());

//            // going to need a database for this/some spreadsheet with the values
//            XTMP.text = string.Format(
//                "Original:   {0} m \n" +
//                "Current:    {1} m \n" +
//                "Strain:     {2}",
//                (originalScale.x * scale).ToString(),
//                (radargram.localScale.x * scale).ToString(),
//                (Math.Abs(originalScale.x - radargram.localScale.x) * scale).ToString());

//            if (workflow == 3)
//            {
//                ZTMP.text = string.Format(
//                    "Original:   {0} m \n" +
//                    "Current:    {1} m \n" +
//                    "Strain:     {2}",
//                    (originalScale.z * scale).ToString(),
//                    (radargram.localScale.z * scale).ToString(),
//                    (Math.Abs(originalScale.z - radargram.localScale.z) * scale).ToString());
//            }

//            // Set rotation text
//            RotationDegreeTMP.text = string.Format("ROTATION:      {0}°", radargram.localEulerAngles.y.ToString());

//            // Set transparency text
//            //TransparencyTMP.text = string.Format("Transparency:      {0}%", Mathf.Round(transparencySlider.SliderValue * 4) * 25);

//            /* This all has to do with the MarkObj */

//            // Update the selected point coordinates
//            float maxX = radargram.localScale.x * scale; // converting to radar image x in scene coords
//            float maxY = radargram.localScale.y * scale;
//            //Debug.Log("maxX and maxY:" + maxX);
//            //Debug.Log(maxY);
//            //float maxZ = radargram.localScale.z * scale;
//            float radarX = (MarkObj.transform.localPosition.x + 0.5f) * maxX;
//            float radarY = (MarkObj.transform.localPosition.y - yOrigin) * maxY;
//            //Debug.Log("radarX" + radarX);
//            //Debug.Log(radarY);
//            //float radarZ = (MarkObj.transform.localPosition.z - zOrigin) * maxZ;

//            Vector3 MarkObjFromSceneOrigin = MarkObj.transform.parent.position;
//            Vector3 MeasureObjFromSceneOrigin = MeasureObj.transform.parent.position;

//            //DistanceVectorGameUnits
//            float measure = Vector3.Distance(MarkObjFromSceneOrigin, MeasureObjFromSceneOrigin);


//            //string allSceneEPSG = File.ReadAllText(@"./epsg.json");
//            //var sceneEPSG = JsonSerializer.Deserialize<SceneCodes>(allSceneEPSG);

//            if (MarkObj.activeSelf)
//            {
//                MarkTMP.text = string.Format(
//                    "{0}: ({1}, {2})\n" +
//                    "X: {3}, Y: {4}\n",
//                    MarkObj.transform.parent.name, radarX.ToString(), radarY.ToString(), maxX.ToString(), maxY.ToString());
//                if (measureMode() != 0)
//                    MarkTMP.text += string.Format("Distance: {0}m", measure);
//            }

//            else
//                MarkTMP.text = "No selected points.";
//        }
//    }

//    // Reset the original radar image transform and re-assign the new radar image.
//    public void ResetRadarSelected(Transform newRadar, Vector3 newPosition, float newAlpha)
//    {
//        targetPosition = newPosition;

//        if (radarParent != newRadar)
//        {
//            // Switch to new radar 
//            radarParent = newRadar;

//            // Reset the values
//            if (workflow == 2)
//            {
//                radargram = newRadar;
//                originalScale = radarParent.GetComponent<RadarEvents2D>().GetScale();
//            }
//            else
//            {
//                // In 3D, we want to transform the radar parent, not the whole grouprkflow

//                radargram = newRadar.transform.GetChild(1);
//                originalScale = radarParent.GetComponent<RadarEvents3D>().GetScale();

//                // Update the flightline colors
//                foreach (Transform child in RadarImageContainer)
//                {
//                    child.GetComponent<RadarEvents3D>().TogglePolyline(true, false);
//                }
//                radarParent.GetComponent<RadarEvents3D>().TogglePolyline(true, true);
//            }

//            // Set the title of the menu to the current radar.
//            Title.text = radarParent.name;
//        }

//        isMainMenu = false;
//        //RadarToggle.IsToggled = true;
//        if (isLinePickingMode)
//        {
//            SubMenuRadar.gameObject.SetActive(false);
//        }
//        else
//        {
//            SubMenuRadar.gameObject.SetActive(true);
//        }

//        SubMenuMain.gameObject.SetActive(false);
//    }

//    // Just resets the radar to its original transform but leaves it selected
//    public void ResetRadarTransform()
//    {
//        radarParent.GetComponent<RadarEvents3D>().ResetTransform();
//    }

//    // The reset button for the radarParent transform; resets all radar.
//    public void ResetButton()
//    {
//        if (isMainMenu)
//        {
//            AllRadarToggle.IsToggled = true;
//            if (workflow == 2)
//            {
//                AllCSVPicksToggle.IsToggled = true;
//                foreach (Transform child in RadarImageContainer) child.GetComponent<RadarEvents2D>().ResetRadar(true);
//                MainCSVToggling();
//            }
//            else
//            {
//                foreach (Transform child in RadarImageContainer)
//                {
//                    child.GetComponent<RadarEvents3D>().ResetRadar();
//                    child.GetComponent<RadarEvents3D>().TogglePolyline(true, false);
//                }
//            }
//            MarkObj.transform.parent = Location.transform;
//            MarkObj.SetActive(false);
//        }
//        // The snap function.
//        else if (measureMode() != 0)
//        {
//            if (MeasureObj.activeSelf && MeasureObj.transform.parent != MarkObj.transform.parent)
//            {
//                MeasureObj.transform.parent.rotation = MarkObj.transform.parent.rotation;

//                // Compute the offset and merge the measuring image to the marked radar image.
//                Vector3 snapOffset = (MeasureObj.transform.position - MarkObj.transform.position);

//                // If they are too close and user wants to, reset the image gap.
//                if (snapOffset.magnitude < 0.001f) snapOffset = MeasureObj.transform.forward * 0.1f;

//                MeasureObj.transform.parent.position -= snapOffset;

//                // Set transparency for better comparisons: only for images not so transparent.
//                MeasureObj.transform.parent.gameObject.GetComponent<RadarEvents>().SetAlpha(0.5f, true);
//                MarkObj.transform.parent.gameObject.GetComponent<RadarEvents>().SetAlpha(0.5f, true);
//                transparencySlider.SliderValue = 0.5f;
//            }
//        }
//        else
//        {
//            // Reset radar menu and radar attributes.
//            RadarToggle.IsToggled = true;
//            if (workflow == 2)
//            {
//                CSVPicksToggle.IsToggled = true;
//                radarParent.GetComponent<RadarEvents2D>().ResetRadar(false);
//                radarParent.GetComponent<RadarEvents2D>().ToggleLine(true);
//            }
//            else
//            {
//                foreach (Transform child in RadarImageContainer)
//                {
//                    child.GetComponent<RadarEvents3D>().ResetRadar();
//                    child.GetComponent<RadarEvents3D>().TogglePolyline(true, false);
//                }
//            }
//        }
//    }

//    // The write button for writting the coordinates into a file.
//    // Reference https://forum.unity.com/threads/how-to-write-a-file.8864/
//    // Be aware of the file path issue! And try to keep a history...
//    public void WriteButton()
//    {
//        if (isMainMenu) Location.GetComponent<CSVReadPlot>().SaveScene();
//        else
//        {
//            if (workflow == 2)
//            {
//                RadarToggle.IsToggled = true;
//                RadarToggling();
//                CSVPicksToggle.IsToggled = true;
//                CSVToggling();

//                /*if (File.Exists(SelectionDialog))
//                {
//                    List<string> tempList = new List<string> { MarkTMP.text };
//                    File.AppendAllLines(SelectionDialog, tempList);
//                }
//                else
//                {
//                    var sr = File.CreateText(SelectionDialog);
//                    sr.WriteLine(MarkTMP.text);
//                    sr.Close();
//                }*/

//                // Trying to find or add a new particle system for the radar image.

//                if (radarParent.Find("Line") == null) Location.GetComponent<CSVReadPlot>().AddPSLine(radarParent);

//                radarParent.GetComponent<RadarEvents2D>().AddNewPoint(MarkColor);
//            }
//        }
//    }

//    // The close button, make the menu disappear and deactivated.
//    public void CloseButton(bool shutDown)
//    {
//        if (shutDown) targetScale = new Vector3(0.0f, 0.0f, 0.0f);
//        else
//        {
//            targetScale = new Vector3(1.0f, 1.0f, 1.0f);
//            targetPosition = Camera.main.transform.position + Camera.main.transform.forward;
//            if (this.transform.localScale.x < 0.1f) this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//            this.gameObject.SetActive(true);
//        }
//    }

//    // Switch between different sub menus.
//    public void HomeButton(bool home)
//    {
//        isMainMenu = home;
//        Scene currentScene = SceneManager.GetActiveScene();
//        string sceneName = string.Concat(currentScene.name[0].ToString().ToUpper(), currentScene.name.Substring(1));
//        Title.text = home || (radarParent == null) ? sceneName : radarParent.name;
//        SubMenuRadar.gameObject.SetActive(!home);
//        SubMenuMain.gameObject.SetActive(home);
//    }

//    // Switch to line picking sub menu
//    public void LinePickingMode(bool linePicking)
//    {
//        Debug.Log("Line Picking Mode turned on in menu");
//        isLinePickingMode = linePicking;

//        // turn on line picking sub menu
//        SubMenuLinePicking.gameObject.SetActive(isLinePickingMode);

//        // turn off other menus
//        SubMenuRadar.gameObject.SetActive(false);
//        SubMenuMain.gameObject.SetActive(false);

//        //turn off object manipulators on all radar images
//        foreach (Transform child in RadarImageContainer)
//        {
//            child.GetChild(1).gameObject.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = false;
//        }
//    }

//    // Switch to radar menu
//    public void returnToRadarMode(bool radar)
//    {
//        isLinePickingMode = false;
//        // turn on radar sub menu
//        SubMenuRadar.gameObject.SetActive(radar);

//        // turn off other menus
//        SubMenuLinePicking.gameObject.SetActive(false);
//        SubMenuMain.gameObject.SetActive(false);

//        //turn on object manipulators on all radar images
//        foreach (Transform child in RadarImageContainer)
//        {
//            child.GetChild(1).gameObject.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = true;
//        }
//    }

//    public void saveRadargram(bool save)
//    {
//        radarParent.GetComponent<RadarEvents3D>().saveRadargram();
//    }

//    // The four slider update interface.
//    public void OnYSliderUpdated(SliderEventData eventData)
//    {
//        if (radarParent && YSlider.gameObject.tag == "Active")
//            radargram.localScale = new Vector3(
//                radargram.localScale.x,
//                originalScale.y * scaleY * (0.5f + eventData.NewValue),
//                radargram.localScale.z
//            );
//    }

//    public void OnXSliderUpdated(SliderEventData eventData)
//    {
//        if (radarParent && XSlider.gameObject.tag == "Active")
//            radargram.localScale = new Vector3(
//                originalScale.x * scaleX * (0.5f + eventData.NewValue),
//                radargram.localScale.y,
//                radargram.localScale.z
//            );
//    }

//    public void OnZSliderUpdated(SliderEventData eventData)
//    {
//        if (radarParent && ZSlider.gameObject.tag == "Active")
//            radargram.localScale = new Vector3(
//                radargram.localScale.x,
//                radargram.localScale.y,
//                originalScale.z * scaleZ * (0.5f + eventData.NewValue)
//            );
//    }

//    public void OnRotateSliderUpdated(SliderEventData eventData)
//    {
//        float rotate = (float)(360.0f * eventData.NewValue);
//        if (radarParent) radargram.localRotation = Quaternion.Euler(0, rotate, 0);
//    }

//    public void OnTransparencySliderUpdated(SliderEventData eventData)
//    {
//        //Round the result to nearest levels.
//        transparencySlider.SliderValue = Mathf.Round(eventData.NewValue * 4) / 4;
//        Debug.Log("DATA " + eventData.NewValue);

//        // Set the transparency
//        if (radarParent)
//        {
//            Debug.Log("WE HERE!! : " + radarParent.GetComponent<RadarEvents3D>().GetType());
//            if (workflow == 2) radarParent.GetComponent<RadarEvents2D>().SetAlpha(1 - eventData.NewValue);
//            else radarParent.GetComponent<RadarEvents3D>().SetAlpha(1 - eventData.NewValue);
//        }
//    }

//    // Main Menu Vertical Exaggeration Slider
//    public void OnVerticalExaggerationSliderUpdated(SliderEventData eventData)
//    {
//        if (workflow == 2)
//        {
//            foreach (Transform child in DEMs)
//            {
//                child.localScale = new Vector3(
//                    child.localScale[0],
//                    0.1f + (4.9f * eventData.NewValue),
//                    child.localScale[2]
//                );
//            }
//        }
//        else
//        {
//            foreach (Transform child in DEMs)
//            {
//                child.localScale = new Vector3(
//                    child.localScale.x,
//                    child.localScale.y,
//                    (child.localScale.x / 2) + (child.localScale.x * eventData.NewValue)
//                );
//            }
//            RadarImageContainer.transform.localScale = new Vector3(1f, 0.5f + eventData.NewValue, 1f);
//        }
//    }

//    // Main Menu Toggling CSV and radar images.
//    public void MainCSVToggling()
//    {
//        Vector3 newScale = AllCSVPicksToggle.IsToggled ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
//        CSVPicksToggle.IsToggled = AllCSVPicksToggle.IsToggled;
//        foreach (Transform child in CSVPicksContainer) child.localScale = newScale;
//        foreach (Transform child in RadarImageContainer)
//            child.GetComponent<RadarEvents2D>().ToggleLine(AllCSVPicksToggle.IsToggled);
//    }

//    // Toggles all radar.
//    public void MainRadarToggling()
//    {
//        foreach (Transform child in RadarImageContainer)
//        {
//            child.GetComponent<RadarEvents2D>().ToggleRadar(AllRadarToggle.IsToggled);
//        }
//    }

//    // Single radar toggling.
//    public void CSVToggling() { radarParent.GetComponent<RadarEvents2D>().ToggleLine(CSVPicksToggle.IsToggled); } // nullref
//    public void RadarToggling()
//    {
//        if (workflow == 2) radarParent.GetComponent<RadarEvents2D>().ToggleRadar(RadarToggle.IsToggled);
//        else radarParent.GetComponent<RadarEvents3D>().ToggleRadar(RadarToggle.IsToggled);
//    }

//    // Find the dem according to name.
//    public void DemToggle(string name)
//    {
//        GameObject targetDem = DEMs.Find(name).gameObject;
//        targetDem.SetActive(!targetDem.activeSelf);
//    }

//    // Switch between two states of the bounding box.
//    public void BoundingBoxToggle()
//    {
//        if (!SurfaceToggle.IsToggled)
//        {
//            DemToggle(Location.GetComponent<CSVReadPlot>().SurName + "(Clone)");
//            SurfaceToggle.IsToggled = true;
//        }

//        if (!BedToggle.IsToggled)
//        {
//            DemToggle(Location.GetComponent<CSVReadPlot>().BedName + "(Clone)");
//            BedToggle.IsToggled = true;
//        }
//        bool originalState = Location.GetComponent<BoxCollider>().enabled;
//        BoxToggle.IsToggled = !originalState;
//        Location.GetComponent<BoxCollider>().enabled = !originalState;
//        Location.GetComponent<BoundsControl>().enabled = !originalState;
//    }

//    // Synchronize the measurement toggle function.
//    // 0 for not turned on, 1 for measure object (end), 2 for mark object (start).
//    public int measureMode(bool voiceSync = false)
//    {
//        if (voiceSync)
//        {
//            SecondMeasurementToggle.IsToggled = MeasurementToggle.IsToggled && !SecondMeasurementToggle.IsToggled;
//            MeasurementToggle.IsToggled = !(MeasurementToggle.IsToggled && !SecondMeasurementToggle.IsToggled);
//        }

//        if (!MeasurementToggle.IsToggled)
//        {
//            SecondMeasurementToggle.IsToggled = false;
//            SecondMeasurementToggle.gameObject.SetActive(false);
//            MeasureModeText.text = "MEASUREMENT MODE";
//            return 0;
//        }
//        else
//        {
//            SecondMeasurementToggle.gameObject.SetActive(true);
//            MeasureModeText.text = "MEASUREMENT MODE\nCHANGE START";
//            return SecondMeasurementToggle.IsToggled ? 2 : 1;
//        }
//    }

//    // Synchronize the sliders.
//    public void syncScaleSlider()
//    {
//        if (radarParent)
//        {
//            scaleX = radargram.localScale.x / originalScale.x;
//            scaleY = radargram.localScale.y / originalScale.y;
//            if (workflow == 3) scaleZ = radargram.localScale.z / originalScale.z;
//        }
//    }

//    // Move the user to somewhere near the selected radar.
//    public void TeleportationButton()
//    {
//        if (MarkObj.activeSelf && (Camera.main.transform.position - MarkObj.transform.position).magnitude > 1)
//        {
//            Vector3 tlpOffset = (Camera.main.transform.position - MarkObj.transform.position).normalized;
//            MixedRealityPlayspace.Transform.Translate(-tlpOffset);
//        }
//    }

//    // Turn on/off the minimap.
//    public void MinimapButton()
//    {
//        Minimap.GetComponent<BoxCollider>().enabled = !Minimap.GetComponent<BoxCollider>().enabled;
//        Minimap.transform.localPosition = new Vector3(0.04f, -0.03f, 0);
//    }

//    // The voice command version for the close button.
//    public void MenuVoice()
//    {
//        if (!this.gameObject.activeSelf) CloseButton(false);
//        else if (!isMainMenu) HomeButton(true);
//        else CloseButton(true);
//    }

//    // The voice command version for the toggles.
//    public void ToggleVoice(string keyword)
//    {
//        if (keyword == "measure") measureMode(true);
//        else if (keyword == "box") BoundingBoxToggle();
//        else if (keyword == "model")
//        {
//            DemToggle("Bedmap2_surface_RIS");
//            DemToggle("Bedmap2_bed");
//            SurfaceToggle.IsToggled = !SurfaceToggle.IsToggled;
//            BedToggle.IsToggled = !BedToggle.IsToggled;
//        }
//        else if (keyword == "line")
//        {
//            if (isMainMenu)
//            {
//                AllCSVPicksToggle.IsToggled = !AllCSVPicksToggle.IsToggled;
//                MainCSVToggling();
//            }
//            else
//            {
//                CSVPicksToggle.IsToggled = !CSVPicksToggle.IsToggled;
//                CSVToggling();
//            }
//        }
//        else if (keyword == "image")
//        {
//            if (isMainMenu)
//            {
//                AllRadarToggle.IsToggled = !AllRadarToggle.IsToggled;
//                MainRadarToggling();
//            }
//            else
//            {
//                if (transparencySlider.SliderValue < 0.5) transparencySlider.SliderValue = 0.5f;
//                else
//                {
//                    RadarToggle.IsToggled = !RadarToggle.IsToggled;
//                    RadarToggling();
//                    if (RadarToggle.IsToggled) transparencySlider.SliderValue = 0;
//                }
//            }
//        }
//        else if (keyword == "delete one" || keyword == "delete all")
//        {
//            if (radarParent && workflow == 2) radarParent.GetComponent<RadarEvents2D>().UndoAddPoint(keyword == "delete all");
//        }
//    }

//    public void returnToHomeScreen()
//    {
//        SceneManager.LoadScene(scenePaths[HOMESCREEN_INDEX], LoadSceneMode.Single);
//    }

//    public void muteButton()
//    {

//    }
//}
