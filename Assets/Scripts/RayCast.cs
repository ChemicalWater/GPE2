using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    private Camera cam;
    [Header("Target & Systems")]
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private ParticleSystem pSystem;

    private LineRenderer lnRender;

    [Header("Laser Settings")]
    [SerializeField]
    [Range(.02f,1)]
    private float laserStartRadius = .1f;
    [SerializeField]
    [Range(.02f,1)]
    private float laserEndRadius = .02f;
    [SerializeField]
    private float laserStrength = .5f;
    private bool isFiring = false;
    [SerializeField]
    [Range(.5f, 2f)]
    private float laserOffset = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera not found. Please ensure there's a Camera tagged as 'MainCamera' in the scene.");
        }

        lnRender = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null)
        {
            return;
        }

       Vector3 moveDirection = (cam.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
       transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, Time.deltaTime * 10f);

        transform.LookAt(target.transform, target.transform.up);

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Terrain"))
                {
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    hit.transform.GetComponent<Marching>().AddTerrain(hit.point, laserStrength);
                }
            }
            else
            {
                Debug.Log("No hit detected");
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(!isFiring)
                isFiring = true;
        }
        if(Input.GetMouseButtonUp(0))
        {
            if(isFiring)
            {
                isFiring = false;
                lnRender.enabled = false;
                pSystem.Stop();  
            }
        }

        if(isFiring)
        {
            FireLaser();
        }
    }

    void FireLaser()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Terrain"))
            {
                lnRender.startWidth = laserStartRadius;
                lnRender.endWidth = laserEndRadius;

                lnRender.enabled = true;
                Vector3 laserSpot = new Vector3(cam.transform.position.x, cam.transform.position.y - laserOffset, cam.transform.position.z);
                lnRender.SetPosition(0, laserSpot);

                lnRender.SetPosition(1, hit.point);

                pSystem.transform.position = hit.point;
                pSystem.transform.LookAt(cam.transform);
                if (!pSystem.isPlaying)
                {
                    pSystem.Play();
                }

                Debug.DrawLine(cam.transform.position, hit.point, Color.green);
                hit.transform.GetComponent<Marching>().RemoveTerrain(hit.point, laserStrength);
            }
        }
        else
        {
            lnRender.enabled = false;
            pSystem.Stop();
        }
    }

}
