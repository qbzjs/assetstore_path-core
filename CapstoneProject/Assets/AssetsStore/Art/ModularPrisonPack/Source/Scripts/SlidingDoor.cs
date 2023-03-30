using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SlidingDoor : MonoBehaviour {

    const string OPEN = "open";

    public AudioClip openSound;
    public AudioClip closeSound;

    public void InteractWithSlidingDoor() {
        Animator a = GetComponent<Animator>();
        bool b = a.GetBool(OPEN) ? false : true;
        a.SetBool(OPEN, b);
        if (openSound != null || closeSound != null)
        {
            if (b)
                AudioSource.PlayClipAtPoint(openSound, a.transform.position);
            else
                AudioSource.PlayClipAtPoint(closeSound, a.transform.position);
        }
    }
}
