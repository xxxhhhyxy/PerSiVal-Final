using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerNormal : MonoBehaviour
{
    public GameObject start;
    public GameObject end;
    public Transform shoulder;
    public Transform elbow;
    public Transform wrist;
    public MeshFilter Lower;
    public MeshFilter Upper;
    Vector3 S2E;
    Vector3 E2W;
    Vector3 lowerNormal;
    Vector3 upperNormal;
    Ray ray;
    RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        S2E = elbow.position - shoulder.position;
        E2W = wrist.position - elbow.position;
        lowerNormal = Vector3.ProjectOnPlane(-S2E, E2W).normalized;
        upperNormal = Vector3.ProjectOnPlane(E2W, S2E).normalized;
        //Debug.Log(Lower.mesh.bounds.center);

        Debug.Log(lowerNormal.ToString("F3"));
        Debug.Log(upperNormal.ToString("F3"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ray = new Ray(Lower.mesh.bounds.center + lowerNormal * 100, Lower.mesh.bounds.center);
        start.transform.position = Lower.mesh.bounds.center;
        end.transform.position = Lower.mesh.bounds.center + lowerNormal * 100;

        start.transform.position = Upper.mesh.bounds.center;
        end.transform.position = Upper.mesh.bounds.center + upperNormal * 100;
        //Debug.DrawLine(Lower.mesh.bounds.center + lowerNormal * 200, hit.point, Color.red, 100000);
        //if (Physics.Raycast(ray, out hit,10000))
        //{
        //    Debug.DrawLine(Lower.mesh.bounds.center + lowerNormal * 200, hit.point, Color.red, 100000);
        //    //start.transform.position = hit.point;
        //}
        //Debug.DrawLine(Vector3.zero, Vector3.forward, Color.red, 1000);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.transform.position);
    }
}
