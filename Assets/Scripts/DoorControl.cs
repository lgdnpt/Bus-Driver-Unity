using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour {
    public Animator anim;
    public KeyCode openKey;
    public AudioSource DoorSound;
    //public AudioSource Backsound;
    public AudioClip openclip;
    public AudioClip closeclip;

    private void Update() {
        if (Input.GetKeyDown(openKey)) {
            if (anim.GetBool("Door_Open")) {
                anim.SetBool("Door_Open", false);
                PlayCloseSound();
            }
            else {
                anim.SetBool("Door_Open", true);
                PlayOpenSound();
            }
        }
    }

    public void PlayOpenSound() {
        DoorSound.Stop();
        DoorSound.clip = openclip;
        DoorSound.Play();
    }

    public void PlayCloseSound() {
        DoorSound.Stop();
        DoorSound.clip = closeclip;
        DoorSound.Play();
    }
}
