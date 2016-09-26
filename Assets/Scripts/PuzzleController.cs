using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleController : MonoBehaviour {

    [Header("Puzzle Variables")]
    public int columns;
    public int rows;
    public Sprite[] pieces;
    public GameObject pieceObject;

    [Header("Explosion Variables")]
    public float explosionForce;
    public Vector3 explosionPosition;
    public float randomRadius;
    public float explosionRadius;
    public float upliftModifier;

    [Header("Screen Colliders")]
    public float colThickness = 4f;
    public float zPosition = 0f;
    private Vector2 screenSize;

    

    void Start()
    {
        createScreenColliders();


        Vector2 pieceSize = pieceObject.GetComponent<BoxCollider2D>().size;

        float totalWidth = pieceSize.x * columns;
        float totalHeight = pieceSize.y * rows;

        float scaleWidth = screenSize.x * 2.0f / totalWidth;
        float scaleHeight = screenSize.y * 2.0f / totalHeight;

        pieceSize.x = pieceSize.x * scaleWidth;
        pieceSize.y = pieceSize.y * scaleHeight;
        Vector2 orig = pieceSize / 2.0f - screenSize;
        for (int x = 0; x < columns; x++)
        {
            for(int y = 0; y < rows; y++)
            {
                Vector2 offset = new Vector2(x * pieceSize.x, y * pieceSize.y);
                Debug.Log("(" + x + ", " + y + "): " + orig + offset);
                GameObject go = (GameObject)Instantiate(pieceObject, orig + offset, Quaternion.identity);
                Vector3 goP = go.transform.localPosition;
                goP.z = -0.01f * (x + y * columns);
                go.transform.localPosition = goP;
                go.GetComponent<SpriteRenderer>().sprite = pieces[x + (rows - 1 - y) * columns];
                go.GetComponent<Rigidbody2D>().isKinematic = true;
                go.transform.localScale = new Vector3(scaleWidth, scaleHeight, 1);
                go.transform.SetParent(transform.Find("Pieces"));
            }
        }

    }

    private bool explosionGizmo = false;
    private void doExplosion()
    {
        Rigidbody2D[] bodies = transform.Find("Pieces").GetComponentsInChildren<Rigidbody2D>();
        explosionPosition += (Vector3)Random.insideUnitCircle * randomRadius;
        explosionGizmo = true;
        foreach(Rigidbody2D body in bodies)
        {
            body.isKinematic = false;

            Vector3 dir = body.transform.position - explosionPosition;
            float wearoff = 1 - (dir.magnitude / explosionRadius);
            Vector3 baseForce = dir.normalized * explosionForce * wearoff;
            body.AddForce(baseForce);

            body.AddTorque(baseForce.x);

            float upliftWearoff = 1 - upliftModifier / explosionRadius;
            Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
            body.AddForce(upliftForce);

        }
    }

    private void createScreenColliders()
    {
        // Från http://forum.unity3d.com/threads/collision-with-sides-of-screen.228865/

        System.Collections.Generic.Dictionary<string, Transform> colliders = new System.Collections.Generic.Dictionary<string, Transform>();
        //Create our GameObjects and add their Transform components to the Dictionary we created above
        colliders.Add("Top", new GameObject().transform);
        colliders.Add("Bottom", new GameObject().transform);
        colliders.Add("Right", new GameObject().transform);
        colliders.Add("Left", new GameObject().transform);
        //Generate world space point information for position and scale calculations
        Vector3 cameraPos = Camera.main.transform.position;
        screenSize.x = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0))) * 0.5f; //Grab the world-space position values of the start and end positions of the screen, then calculate the distance between them and store it as half, since we only need half that value for distance away from the camera to the edge
        screenSize.y = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height))) * 0.5f;
        //For each Transform/Object in our Dictionary
        foreach (KeyValuePair<string, Transform> valPair in colliders)
        {
            valPair.Value.gameObject.AddComponent<BoxCollider2D>(); //Add our colliders. Remove the "2D", if you would like 3D colliders.
            valPair.Value.name = valPair.Key + "Collider"; //Set the object's name to it's "Key" name, and take on "Collider".  i.e: TopCollider
            valPair.Value.parent = transform.Find("Walls"); //Make the object a child of whatever object this script is on (preferably the camera)

            if (valPair.Key == "Left" || valPair.Key == "Right") //Scale the object to the width and height of the screen, using the world-space values calculated earlier
                valPair.Value.localScale = new Vector3(colThickness, screenSize.y * 2, colThickness);
            else
                valPair.Value.localScale = new Vector3(screenSize.x * 2, colThickness, colThickness);
        }
        //Change positions to align perfectly with outter-edge of screen, adding the world-space values of the screen we generated earlier, and adding/subtracting them with the current camera position, as well as add/subtracting half out objects size so it's not just half way off-screen
        colliders["Right"].position = new Vector3(cameraPos.x + screenSize.x + (colliders["Right"].localScale.x * 0.7f), cameraPos.y, zPosition);
        colliders["Left"].position = new Vector3(cameraPos.x - screenSize.x - (colliders["Left"].localScale.x * 0.7f), cameraPos.y, zPosition);
        colliders["Top"].position = new Vector3(cameraPos.x, cameraPos.y + screenSize.y + (colliders["Top"].localScale.y * 0.7f), zPosition);
        colliders["Bottom"].position = new Vector3(cameraPos.x, cameraPos.y - screenSize.y - (colliders["Bottom"].localScale.y * 0.7f), zPosition);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(explosionGizmo)
            Gizmos.DrawSphere(explosionPosition, 2);

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.E))
            doExplosion();


        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                    beginInteraction(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position));
                else if (touch.phase == TouchPhase.Moved)
                    movedInteraction(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position));
                else
                    endInteraction(touch.fingerId);
            }
        }

        if (Input.GetMouseButtonDown(0))
            beginInteraction(-1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        else if (Input.GetMouseButtonUp(0))
            endInteraction(-1);
        else if (Input.GetMouseButton(0))
            movedInteraction(-1, Camera.main.ScreenToWorldPoint(Input.mousePosition));

    }

    private Dictionary<int, TargetJoint2D> targetJoints = new Dictionary<int, TargetJoint2D>();

    void beginInteraction(int id, Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Camera.main.transform.forward);
        if(hit.collider != null)
        {
            GameObject interacted = hit.transform.gameObject;
            Vector3 iP = interacted.transform.localPosition;
            iP.z = - columns * rows;

            interacted.GetComponent<PuzzlePieceController>().pickedUp();
            TargetJoint2D joint = interacted.AddComponent<TargetJoint2D>();
            joint.anchor = hit.rigidbody.transform.InverseTransformPoint(hit.point);
            joint.target = position;
            targetJoints.Add(id, joint);
        }
    }

    void movedInteraction(int id, Vector2 position)
    {
        if (!targetJoints.ContainsKey(id))
            return;
        targetJoints[id].target = position;
        targetJoints[id].gameObject.GetComponent<PuzzlePieceController>().moved();
    }

    void endInteraction(int id)
    {
        if (!targetJoints.ContainsKey(id))
            return;
            TargetJoint2D tj = targetJoints[id];
            tj.gameObject.GetComponent<PuzzlePieceController>().dropped();
            targetJoints.Remove(id);
            Destroy(tj);
    }
}
