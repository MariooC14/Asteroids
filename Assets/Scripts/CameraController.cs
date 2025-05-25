using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the camera effects, such as camera shake
/// </summary>
public class CameraController : MonoBehaviour
{
    // Inspector stuff 
    public static readonly int CAMERA_HEIGHT = 30;
    public static bool cameraShakeEnabled = true;
    public float shakeStrength = 0.5f;
    public float shakeDuration = 0.25f;

    // This is a toggle button found in the options panel
    public Toggle cameraShakeToggle;

    private void Start()
    {
        cameraShakeToggle.isOn = cameraShakeEnabled;
        transform.position = new Vector3(0, CAMERA_HEIGHT, 0);
        transform.LookAt(Vector3.zero, Vector3.up);

        cameraShakeToggle.onValueChanged.AddListener(delegate
        {
            ToggleCameraShake(cameraShakeToggle);
        });
    }

    /// <summary>
    /// Aplies a shake effect to the camera (if enabled)
    /// </summary>
    public void ShakeCamera()
    {
        StartCoroutine(Shake());
    }

    /// <summary>
    /// Coroutine that applies the shake effect
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shake()
    {
        if (!cameraShakeEnabled) yield break;

        var initialPos = transform.position;
        var animationTime = shakeDuration;

        while (animationTime > 0)
        {
            // If the game is paused, wait until it's unpaused
            if (Time.timeScale == 0) yield break;
            animationTime -= Time.deltaTime;
            transform.position = initialPos + Random.insideUnitSphere * shakeStrength;
            yield return null;
        }

        transform.position = initialPos;
    }

    /// <summary>
    /// Setter for camera shake option in options panel
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleCameraShake(Toggle toggle)
    {
        cameraShakeEnabled = toggle.isOn;
    }
}