using UnityEngine;


public class AudioController : MonoBehaviour
{
   AudioSource audioSource;
   public float startTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.time = startTime;
        if (gameObject.name == "Background Music")
        {
            audioSource.Play();
        }
    }

    public void PlayAudio()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
    
    public void StopAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
