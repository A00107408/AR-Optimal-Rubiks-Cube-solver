using UnityEngine;
using System.Collections;

public class CubeLocationBeep: MonoBehaviour{

    public AudioSource beep;
   
    private float Pitch = 1f;

    private bool AudioPlaying = false;

	public void PlayBeep(int ConseqFaces)
    {
        if (ConseqFaces == 1) //Pitch && ConseqFaces < 10)
        {
            beep.pitch = 1f;
        }
        else if (ConseqFaces == 2)
        {
            beep.pitch = 1.2f;
        }
        else if(ConseqFaces == 3)
        {
            beep.pitch = 1.3f;
        }
        else if(ConseqFaces == 4)
        {
            beep.pitch = 1.4f;
        }
        else if (ConseqFaces == 5)
        {
            beep.pitch = 1.5f;
        }
        else if (ConseqFaces == 6)
        {
            beep.pitch = 1.6f;
        }
        else if (ConseqFaces == 7)
        {
            beep.pitch = 1.7f;
        }
        else if (ConseqFaces == 8)
        {
            beep.pitch = 1.8f;
        }
        else if (ConseqFaces == 9)
        {
            beep.pitch = 1.9f;
        }
        else if (ConseqFaces == 10)
        {
            beep.pitch = 2.0f;
        }
        

        StartCoroutine(PlayIt());     
    }

    IEnumerator PlayIt()
    {
        if (AudioPlaying == false) {
            beep.Play();
            AudioPlaying = true;
            yield return new WaitForSeconds(0.5f);
            AudioPlaying = false;
        }       
    }
}
