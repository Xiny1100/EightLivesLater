using UnityEngine;

[RequireComponent(typeof(Light))]

public class LightFlicker : MonoBehaviour
{

    public float minIntensity = 0f;       // Can go completely dark
    public float maxIntensity = 1.5f;     // Normal light intensity
    public float flickerSpeed = 0.1f;     // How fast intensity changes
    public float flickerChance = 0.2f;    // Chance to flicker each check
    public float flickerInterval = 0.05f; // How often to check (seconds)

    private Light myLight;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myLight = GetComponent<Light>();
        myLight.intensity = maxIntensity;
        timer = flickerInterval;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = flickerInterval;

            // Random chance to flicker
            if (Random.value < flickerChance)
            {
                // Flicker: randomly dim or turn off
                myLight.intensity = Random.Range(minIntensity, maxIntensity);
            }
            else
            {
                // Mostly stay at max intensity
                myLight.intensity = maxIntensity;
            }
        }
    }
}
