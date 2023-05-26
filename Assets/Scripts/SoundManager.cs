using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum State
    {
        idle,
        play,
        fadeIn,
        fadeOut
    }

    // AudioSource inputSource;
    // AudioClip inputClip;

    public State state = State.idle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.idle:

                break;

            case State.play:

                // gameAmbience.clip = ambience;
                // gameAmbience.loop = true;
                // gameAmbience.volume = 0;
                // gameAmbience.Play();
                // input = gameAmbience;
                // fadeIn = true;
                break;
        }
    }
}
