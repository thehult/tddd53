using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MovieController : MonoBehaviour {

    public Movie[] movies;
    private AudioSource audioSource;
    private RawImage rawImage;

    [HideInInspector]
    public bool repeat = false;

    private bool isPlaying = false;
    private int playingId = 0;

	// Use this for initialization
	void Start () {
        rawImage = GetComponent<RawImage>();
        audioSource = GetComponent<AudioSource>();
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (isPlaying && !(rawImage.texture as MovieTexture).isPlaying)
            onStop();
	}

    public void playMovie (int id)
    {
        isPlaying = true;
        playingId = id;
        gameObject.SetActive(true);
        rawImage.texture = movies[id].movieTexture as MovieTexture;
        audioSource.clip = movies[id].audioClip;
        movies[id].movieTexture.Play();
        audioSource.Play();
    }

    public void stopMovie()
    {
        (rawImage.texture as MovieTexture).Stop();
        audioSource.Stop();
        audioSource.time = 0.0f;
        gameObject.SetActive(false);
    }

    private void onStop()
    {
        if (repeat)
        {
            stopMovie();
            playMovie(playingId);
        }
        else
            stopMovie();
    }

    [Serializable]
    public class Movie
    {
        public MovieTexture movieTexture;
        public AudioClip audioClip;
    }
}
