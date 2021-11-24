using System;
using System.Collections.Generic;
using UnityEngine;
public class GetCurrentPolygon : MonoBehaviour {
    PolygonInfo polygon;
    public LayerMask layerMask;
    MeshNavigation meshNavigation;

    private void Start() {
        meshNavigation = GetComponentInParent<MeshNavigation>();

        //get initial polygon
        //get center of mass of parent object
        Vector3 centerOfMass = transform.parent.TransformPoint(transform.parent.GetComponent<Rigidbody>().centerOfMass);
        //find first face in parent's mesh hit by (center of mass - transform.position)
        Vector3 rayDir = centerOfMass - transform.position;
        if (Physics.Raycast(transform.position, rayDir, out RaycastHit hit, 1000f, layerMask)) {
            polygon = new PolygonInfo(transform.parent.GetComponent<MeshFilter>().mesh, hit.triangleIndex, transform.parent);
        }
    }
    // Update is called once per frame
    void Update() {
        //get center of mass of parent object
        Vector3 centerOfMass = transform.parent.TransformPoint(transform.parent.GetComponent<Rigidbody>().centerOfMass);
        //find first face in parent's mesh hit by (center of mass - transform.position)
        Vector3 rayDir = centerOfMass - transform.position;
        if (Physics.Raycast(transform.position, rayDir, out RaycastHit hit, 1000f, layerMask)) {
            polygon = new PolygonInfo(transform.parent.GetComponent<MeshFilter>().mesh, hit.triangleIndex, transform.parent);
        }
        //Vector3 targetPos = Vector3.ProjectOnPlane(transform.position - polygon.points[0], polygon.normal) + polygon.points[0] + polygon.normal.normalized * 0.001f;
        //update velocity to be within the plane of the face
        //GetComponent<Rigidbody>().velocity = 50 * Time.deltaTime * (targetPos - transform.position);
    }
    void DrawTriangle(List<Vector3> corners) {
        foreach (Vector3 p in corners) {
            Gizmos.DrawSphere(p, 0.01f);
        }
        for (int i = 0; i < 3; i++) {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 3]);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (Vector3 p in polygon.points) {
            Gizmos.DrawSphere(p, 0.015f);
        }
        Gizmos.DrawLine(transform.position, transform.position + polygon.normal);
        foreach (List<Vector3> l in meshNavigation.getNeighboringFaces(polygon.triangleIndex)) {
            Vector3 neighboringNormal = PolygonMath.getNormalFromPoints(l);
            List<Vector3> commonPoints = PolygonMath.commonPoints(polygon.points, l);
            List<Vector3> unfoldedPoints = new List<Vector3>();
            foreach (Vector3 p in l) {
                unfoldedPoints.Add(PolygonMath.rotateAround(
                        p,
                        commonPoints[0],
                        Quaternion.FromToRotation(
                            neighboringNormal,
                            polygon.normal)));
            }
            Gizmos.color = Color.blue;
            DrawTriangle(unfoldedPoints);
            Func<Vector3, Vector3> rotateOntoPlane = (point) => Vector3.ProjectOnPlane(Quaternion.FromToRotation(polygon.normal, Vector3.up) * point, Vector3.up);
            List<Vector3> planarPoints = new List<Vector3>();
            foreach (Vector3 p in unfoldedPoints) {
                planarPoints.Add(rotateOntoPlane(p) - rotateOntoPlane(transform.position));
            }
            Gizmos.color = Color.green;
            DrawTriangle(planarPoints);
            Gizmos.DrawSphere(rotateOntoPlane(transform.position) - rotateOntoPlane(transform.position), 0.04f);
        }
    }
}
