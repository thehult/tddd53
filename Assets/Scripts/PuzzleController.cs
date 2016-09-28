using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleController : MonoBehaviour {

    [Header("Puzzle Variables")]
    public int columns;
    public int rows;
    public Sprite[] pieces;
    public GameObject pieceObject;
    public Vector2 pieceSize;
    public Vector2 pieceOverlap;

    [Header("Explosion Variables")]
    public float explosionForce;
    public Vector3 explosionPosition;
    public float randomRadius;
    public float explosionRadius;
    public float upliftModifier;

    [Header("Screen Shake")]
    public float shakeTime;
    public float shakeAmount;
    public float decreaseFactor;

    [Header("Screen Colliders")]
    public float colThickness = 4f;
    public float zPosition = 0f;
    private Vector2 screenSize;


    private float shaking = 0.0f;

    [Header("Misc")]
    public GameObject sceneObject;
    private SceneController sceneController;

    void Start()
    {
        sceneController = sceneObject.GetComponent<SceneController>();
        createScreenColliders();


        

    }

    public void createPuzzle()
    {

        float totalWidth = pieceSize.x * columns - pieceOverlap.x * (columns - 1);
        float totalHeight = pieceSize.y * rows - pieceOverlap.y * (rows - 1);

        Debug.Log("TOTALSIZE: " + totalWidth + ", " + totalHeight);

        float scaleWidth = screenSize.x * 2 / totalWidth;
        float scaleHeight = screenSize.y * 2f / totalHeight;

        Debug.Log("SCREENSIZE: " + screenSize);
        Debug.Log("SCALE: " + scaleWidth + ", " + scaleHeight);

        pieceSize.x = pieceSize.x * scaleWidth;
        pieceSize.y = pieceSize.y * scaleHeight;

        Debug.Log("PIECESIZE: " + pieceSize);

        Vector2 orig = pieceSize / 2.0f - screenSize;
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {                
                Vector2 offset = new Vector2(x * pieceSize.x, y * pieceSize.y);
                Vector2 overlap = new Vector2(x * pieceOverlap.x * scaleWidth,  y * pieceOverlap.y * scaleHeight);
                //Vector2 overlap = new Vector2(x * pieceOverlap.x, y * pieceOverlap.y );
                GameObject go = (GameObject)Instantiate(pieceObject, orig + offset - overlap, Quaternion.identity);
                Vector3 goP = go.transform.localPosition;
                goP.z = -0.01f * (1 + x + y * columns);
                go.transform.localPosition = goP;
                go.GetComponent<SpriteRenderer>().sprite = pieces[x + (rows - 1 - y) * columns];
                go.GetComponent<Rigidbody2D>().isKinematic = true;
                //go.GetComponent<BoxCollider2D>().size = new Vector2(pieceSize.x * scaleWidth, pieceSize.y * scaleHeight);
                go.transform.localScale = new Vector3(scaleWidth * 10f, scaleHeight * 10f, 1);
                go.transform.SetParent(transform.Find("Pieces"));
            }
        }
    }

    private bool explosionGizmo = false;
    public void doExplosion()
    {
        Rigidbody2D[] bodies = transform.Find("Pieces").GetComponentsInChildren<Rigidbody2D>();
        explosionPosition += (Vector3)Random.insideUnitCircle * randomRadius;
        explosionGizmo = true;
        foreach(Rigidbody2D body in bodies)
        {
            body.isKinematic = false;
            body.gameObject.GetComponent<PuzzlePieceController>().complete = false;

            Vector3 dir = body.transform.position - explosionPosition;
            float wearoff = 1 - (dir.magnitude / explosionRadius);
            Vector3 baseForce = dir.normalized * explosionForce * wearoff;
            body.AddForce(baseForce);

            body.AddTorque(baseForce.x);

            float upliftWearoff = 1 - upliftModifier / explosionRadius;
            Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
            body.AddForce(upliftForce);

        }
        shaking = shakeTime;
    }

    public IEnumerator doExplosionAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        doExplosion();
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
        
        if(shaking > 0)
        {
            Vector3 newPos = Random.insideUnitCircle * shakeAmount;
            newPos.z = -10;
            Camera.main.transform.localPosition = newPos;
            shaking -= Time.deltaTime * decreaseFactor;
        } else
        {
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);
        }

        

    }

    private Dictionary<int, TargetJoint2D> targetJoints = new Dictionary<int, TargetJoint2D>();


    public void beginInteraction(int id, Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Camera.main.transform.forward);
        if(hit.collider != null)
        {
            GameObject interacted = hit.transform.gameObject;
            if (!interacted.GetComponent<PuzzlePieceController>().complete)
            {

                interacted.GetComponent<PuzzlePieceController>().pickedUp();
                TargetJoint2D joint = interacted.AddComponent<TargetJoint2D>();
                joint.anchor = hit.rigidbody.transform.InverseTransformPoint(hit.point);
                joint.target = position;
                targetJoints.Add(id, joint);
                
                Vector3 iP = interacted.transform.localPosition;
                float fromZ = iP.z;
                iP.z = -0.01f * (columns * rows);
                foreach(Transform sib in interacted.transform.parent)
                {
                    if(sib.localPosition.z < fromZ)
                    {
                        Vector3 s = sib.localPosition;
                        s.z += 0.01f;
                        sib.localPosition = s;
                    }
                }
                interacted.transform.localPosition = iP;

            }
        }
    }

    public void movedInteraction(int id, Vector2 position)
    {
        if (!targetJoints.ContainsKey(id))
            return;
        targetJoints[id].target = position;
        targetJoints[id].gameObject.GetComponent<PuzzlePieceController>().moved();
    }

    public void endInteraction(int id)
    {
        if (!targetJoints.ContainsKey(id))
            return;
            TargetJoint2D tj = targetJoints[id];
            tj.gameObject.GetComponent<PuzzlePieceController>().dropped();
            targetJoints.Remove(id);
            Destroy(tj);
        int completed = 0;
        foreach (PuzzlePieceController sib in gameObject.transform.Find("Pieces").GetComponentsInChildren<PuzzlePieceController>())
        {
            if (sib.complete)
                completed++;
        }
        if (completed == columns * rows)
            sceneController.puzzleDone();
    }
}
