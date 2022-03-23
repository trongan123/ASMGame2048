using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound2048 : MonoBehaviour
{
    public static AudioClip MergingSound;
    static AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        MergingSound = Resources.Load<AudioClip>("Merging_2048");

        audioSource = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public static void MergeSound(string clip)
    {
        switch (clip)
        {
            case "merge":
                audioSource.PlayOneShot(MergingSound);
                break;

        }
    }
}
