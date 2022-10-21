using UnityEngine;

public class MarkObj : MonoBehaviour
{
    private Vector3 Original_Scale;
    public LineRenderer circleRenderer;
    public int steps;
    public float radius;

    // Start is called before the first frame update
    void Start()
    {
        // Record the starting scale.
        Original_Scale = this.transform.lossyScale;
        this.gameObject.SetActive(false);

        // Draw circle. https://pastebin.com/ZHG0crvP
        circleRenderer.positionCount = steps;
        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float currentRadian = (float)currentStep / (steps - 1) * 2 * Mathf.PI;
            Vector3 currentPosition = new Vector3(radius * Mathf.Cos(currentRadian), radius * Mathf.Sin(currentRadian), 0);
            circleRenderer.SetPosition(currentStep, currentPosition);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust the scale according to new parent.
        Vector3 Global_Scale = this.transform.parent.transform.lossyScale;
        this.transform.localScale = new Vector3(Original_Scale.x / Global_Scale.x, Original_Scale.y / Global_Scale.y, Original_Scale.z / Global_Scale.z);
    }
}
