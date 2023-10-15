
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + (cam.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal")), Time.deltaTime * 10f);
        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));
        cam.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                if (hit.transform.CompareTag("Terrain"))
                {
                    Debug.Log("HIT" + hit.point);
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    hit.transform.GetComponent<Test.Octree.CreateOctree>().DrawHitcube(hit.point);
                }
            }
        }
    }
}
