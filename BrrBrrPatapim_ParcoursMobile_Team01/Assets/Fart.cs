using System;
using UnityEngine;

public class Fart : MonoBehaviour
{

    [SerializeField] AudioClip fartSound; // Reference to the fart sound clip
    private AudioSource audioSource; // Reference to the AudioSource component
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // Add an AudioSource component to the GameObject
        audioSource.clip = fartSound; // Set the AudioSource clip to the fart sound

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Check if the space key is pressed
        {
            PlayFartSound(); // Call the method to play the fart sound
        }
    }

    private void PlayFartSound()
    {
        AudioSource fartSource = gameObject.AddComponent<AudioSource>(); // Create a new AudioSource for the fart sound
        fartSource.clip = fartSound; // Set the clip to the fart sound
        fartSource.Play(); // Play the fart sound
    }
}
