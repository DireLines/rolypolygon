using System;
using System.Collections.Generic;
using UnityEngine;
public class TravelOverMesh : MonoBehaviour {
    PolygonInfo polygon;
    public LayerMask layerMask;
    MeshNavigation meshNavigation;
    public float moveSpeed = 1f;

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
        GetComponent<Rigidbody>().velocity = UnityEngine.Random.onUnitSphere;
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
            List<Vector3> planarPoints = new List<Vector3>();
            foreach (Vector3 p in unfoldedPoints) {
                planarPoints.Add(meshToPlane(p));
            }
            transform.position = planeToMesh(meshToPlane(transform.position));
            Vector3 vel = GetComponent<Rigidbody>().velocity;
            Vector3 forward = Vector3.ProjectOnPlane(vel, polygon.normal).normalized;
            Vector3 right = Vector3.Cross(forward, polygon.normal).normalized;
            GetComponent<Rigidbody>().velocity = moveSpeed * Time.deltaTime * (forward + right * Input.GetAxisRaw("Horizontal")).normalized;


        }
    }
    void DrawTriangle(List<Vector3> corners) {
        foreach (Vector3 p in corners) {
            Gizmos.DrawSphere(p, 0.01f);
        }
        for (int i = 0; i < 3; i++) {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 3]);
        }
    }
    Vector3 meshToPlane(Vector3 point) {
        Vector3 faceCenter = (polygon.points[0] + polygon.points[1] + polygon.points[2]) / 3f;
        return Vector3.ProjectOnPlane(Quaternion.FromToRotation(polygon.normal, Vector3.up) * (point - faceCenter), Vector3.up);
    }
    Vector3 planeToMesh(Vector3 point) {
        Vector3 faceCenter = (polygon.points[0] + polygon.points[1] + polygon.points[2]) / 3f;
        return Quaternion.FromToRotation(Vector3.up, polygon.normal) * point + faceCenter;
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        DrawTriangle(polygon.points);
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
            //DrawTriangle(unfoldedPoints);
            List<Vector3> planarPoints = new List<Vector3>();
            foreach (Vector3 p in unfoldedPoints) {
                planarPoints.Add(meshToPlane(p));
            }
            Gizmos.color = Color.green;
            //DrawTriangle(planarPoints);
        }
    }
}
