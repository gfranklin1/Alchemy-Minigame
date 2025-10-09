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
       audioSource.Play();
   }
}
