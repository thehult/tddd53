using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour {

    [SerializeField]
    private SceneState state;

    public GameObject movieObject;
    public GameObject puzzleObject;
    public GameObject cockpitObject;

    public float timeBeforeChange;
    private float lastAction;

    private MovieController movieController;
    private PuzzleController puzzleController;
    private CockpitController cockpitController;

    private bool init = true;

	// Use this for initialization
	void Start ()
    {
        movieController = movieObject.GetComponent<MovieController>();
        puzzleController = puzzleObject.GetComponent<PuzzleController>();
        cockpitController = cockpitObject.GetComponent<CockpitController>();
    }
	
	// Update is called once per frame
	void Update () {
        if(init)
        {
            changeState(SceneState.PREMOVIE);
            init = false;
        }

        if (state == SceneState.PREMOVIE)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                changeState(SceneState.PUZZLE);
                StartCoroutine(puzzleController.doExplosionAfterTime(2.0f));

            }
        } else if(state == SceneState.PUZZLE)
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                        puzzleController.beginInteraction(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position));
                    else if (touch.phase == TouchPhase.Moved)
                        puzzleController.movedInteraction(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position));
                    else
                        puzzleController.endInteraction(touch.fingerId);
                }
            }

            if (Input.GetMouseButtonDown(0))
                puzzleController.beginInteraction(-1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            else if (Input.GetMouseButtonUp(0))
                puzzleController.endInteraction(-1);
            else if (Input.GetMouseButton(0))
                puzzleController.movedInteraction(-1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        } else if(state == SceneState.COCKPIT)
        {
            if (Input.touchCount > 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    cockpitController.interaction(Camera.main.ScreenToWorldPoint(Input.touches[0].position));
                    lastAction = Time.time;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                cockpitController.interaction(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                lastAction = Time.time;
            }
            if(Time.time - lastAction > timeBeforeChange)
            {
                changeState(SceneState.PREMOVIE);
            }
        }
    }
        
    public void puzzleDone()
    {
        changeState(SceneState.COCKPIT);
    }

    private void changeState(SceneState _state)
    {
        switch (_state)
        {
            case SceneState.PREMOVIE:
                movieController.repeat = true;
                movieController.playMovie(0);
                break;
            case SceneState.PUZZLE:
                movieController.stopMovie();
                puzzleController.createPuzzle();
                break;
            case SceneState.COCKPIT:
                lastAction = Time.time;
                cockpitObject.SetActive(true);
                break;
        }
        state = _state;
    }

    public enum SceneState
    {
        PREMOVIE,
        PUZZLE,
        COCKPIT
    }
}
