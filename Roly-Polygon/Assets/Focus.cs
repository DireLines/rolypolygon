using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour {
    public Transform target;
    public float viewDist = 1f;
    // Start is called before the first frame update
    void Start() {
        transform.position = target.position;
    }

    // Update is called once per frame
    void Update() {
        transform.position = target.position + target.gameObject.GetComponent<TravelOverMesh>().polygon.normal * viewDist;
        transform.LookAt(target.position);
        transform.rotation = Quaternion.FromToRotation(transform.up, target.GetComponent<Rigidbody>().velocity) * transform.rotation;
    }
    void OnPreRender() {
        GL.wireframe = true;
    }

    void OnPostRender() {
        GL.wireframe = false;
    }
}
