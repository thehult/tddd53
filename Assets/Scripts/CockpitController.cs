using UnityEngine;
using System.Collections;
using System;

public class CockpitController : MonoBehaviour
{

    public Interactable[] interactables;
    public GameObject movieObject;
    private MovieController movieController;

    // Use this for initialization
    void Start()
    {
        movieController = movieObject.GetComponent<MovieController>();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void interaction(Vector3 pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Camera.main.transform.forward);
        if(hit.collider != null)
        {
            foreach(Interactable i in interactables)
            {
                if(i.collider.GetInstanceID() == hit.collider.GetInstanceID())
                {
                    movieController.repeat = false;
                    movieController.playMovie(i.movieId);
                }
            }
        }
    }



    [Serializable]
    public class Interactable
    {
        public Collider2D collider;
        public int movieId;
        public string info;
    }
}
