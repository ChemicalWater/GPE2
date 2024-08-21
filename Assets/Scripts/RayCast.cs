using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    private Camera cam;
    [SerializeField]
    private GameObject target;

    private Marching marchScript;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera not found. Please ensure there's a Camera tagged as 'MainCamera' in the scene.");
        }
        marchScript = target.GetComponent<Marching>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null)
        {
            return;
        }

        // Movement logic
       Vector3 moveDirection = (cam.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
       transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, Time.deltaTime * 10f);
       //
       //// Rotation logic
       //float mouseY = Input.GetAxis("Mouse Y");
       //float mouseX = Input.GetAxis("Mouse X");

        //marchScript.CameraPos(cam.transform.position);
        RaycastHit HitInfo;

        //if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out HitInfo, 100.0f))
        //    marchScript.CameraPos(HitInfo.point);

        transform.LookAt(target.transform, target.transform.up);

        //if (mouseX != 0)
        //{
        //    transform.Rotate(0, mouseX, 0);
        //}
        //
        //if (mouseY != 0)
        //{
        //    cam.transform.Rotate(-mouseY, 0, 0);
        //}

        // Raycasting logic

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Terrain"))
                {
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    //hit.transform.GetComponent<Marching>().SubdivideNode(hit.point);
                    hit.transform.GetComponent<Marching>().AddTerrain(hit.point, 0.5f);
                    //hit.transform.GetComponent<Marching>().DrawHitcube(hit.point);
                }
            }
            else
            {
                Debug.Log("No hit detected");
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Terrain"))
                {
                   Debug.DrawLine(transform.position, hit.point, Color.green);
                    Debug.Log("HIT: " + hit.point);
                   hit.transform.GetComponent<Marching>().RemoveTerrain(hit.point, 0.5f);
                   //hit.transform.GetComponent<Marching>().DrawHitcube(hit.point);
                }
            }
            else
            {
                Debug.Log("No hit detected");
            }
        }
    }
}
