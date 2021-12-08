using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSoundController : MonoBehaviour
{
    public AudioSource PropellerNoise;
    public float minTimeBetweenSound = 1.0f;
    public float maxTimeBetweenSound = 5.0f;
    private float currentSoundCountdown = 0.0f;

    public float minPitch = 0.5f;
    public float maxPitch = 1.0f;
    public float minVolume = 0.2f;
    public float maxVolume = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        currentSoundCountdown = Random.Range(minTimeBetweenSound, maxTimeBetweenSound);
    }

    // Update is called once per frame
    void Update()
    {
        currentSoundCountdown -= Time.deltaTime;
        if(currentSoundCountdown < 0)
        {
            PropellerNoise.volume = Random.Range(minVolume, maxVolume);
            PropellerNoise.pitch = Random.Range(minPitch, maxPitch);
            PropellerNoise.Play();
            currentSoundCountdown = Random.Range(minTimeBetweenSound, maxTimeBetweenSound);
        }
    }
}
