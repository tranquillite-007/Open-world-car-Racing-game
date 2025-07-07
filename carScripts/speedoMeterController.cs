using UnityEngine;
using UnityEngine.UI;

public class SpeedometerController : MonoBehaviour
{
    [SerializeField] private DriftCarController carController; // Reference to the car controller
    [SerializeField] private Transform speedometerNeedle; // Reference to the speedometer needle
    [SerializeField] private Text speedText; // Reference to the UI Text for speed display

    private const float maxSpeed = 180f; // Maximum speed in km/h
    private const float needleRotationOffset = 225f; // Initial rotation value of the needle at 0 speed

    void Update()
    {
        UpdateSpeedometer();
    }

    private void UpdateSpeedometer()
    {
        float currentSpeed = carController.GetCurrentSpeed(); 
        speedText.text = Mathf.RoundToInt(currentSpeed).ToString() + " km/h";

        float needleRotation = needleRotationOffset - (currentSpeed / maxSpeed) * 244f; 
        needleRotation = Mathf.Clamp(needleRotation, 0, needleRotationOffset);

        speedometerNeedle.localEulerAngles = new Vector3(0, 0, needleRotation);
    }
}