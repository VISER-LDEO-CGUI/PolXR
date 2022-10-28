using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public GameObject gridLine;
    public Vector3 startPoint;
    public int xNum;
    public int yNum;
    public int xGap;
    public int yGap;

    // Start is called before the first frame update
    void Start()
    {
        // Draw the grid.
        Vector3 point0 = startPoint;
        Vector3 point1 = startPoint;
        point1.x += (xNum - 1) * xGap;
        for (int i = 0; i < yNum; i++)
        {
            GameObject newLine = Instantiate(gridLine, this.transform);
            newLine.GetComponent<LineRenderer>().SetPosition(0, point0);
            newLine.GetComponent<LineRenderer>().SetPosition(1, point1);
            newLine.GetComponent<LineRenderer>().startWidth = 0.03f;
            newLine.GetComponent<LineRenderer>().endWidth = 0.03f;
            point0.z += yGap;
            point1.z += yGap;
        }
        
        point0 = startPoint;
        point1 = startPoint;
        point1.z += (yNum - 1) * yGap;
        for (int i = 0; i < xNum; i++)
        {
            GameObject newLine = Instantiate(gridLine, this.transform);
            newLine.GetComponent<LineRenderer>().SetPosition(0, point0);
            newLine.GetComponent<LineRenderer>().SetPosition(1, point1);
            newLine.GetComponent<LineRenderer>().startWidth = 0.03f;
            newLine.GetComponent<LineRenderer>().endWidth = 0.03f;
            point0.x += xGap;
            point1.x += xGap;
        }

        Destroy(gridLine);
    }
}
