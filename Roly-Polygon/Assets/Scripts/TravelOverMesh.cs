using System;
using System.Collections.Generic;
using UnityEngine;
public class TravelOverMesh : MonoBehaviour {
    PolygonInfo polygon;
    public LayerMask layerMask;
    MeshNavigation meshNavigation;
    public float moveSpeed = 1f;

    private void Start() {
        transform.position = UnityEngine.Random.insideUnitSphere * 50f;
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
        Vector3 lastPolygonNormal = polygon.normal;
        Vector2 planePos = meshToPlane(transform.position);
        Vector2 planePosNext = meshToPlane(GetComponent<Rigidbody>().velocity + transform.position);
        foreach (int neighbor in meshNavigation.getNeighboringFaceIndices(polygon.triangleIndex)) {
            List<Vector3> l = meshNavigation.getTriangleVertices(neighbor);
            //Vector3 neighboringNormal = PolygonMath.getNormalFromPoints(l);
            List<Vector3> commonPoints = PolygonMath.commonPoints(polygon.points, l);
            List<Vector2> edgePoints = new List<Vector2>();
            foreach (Vector3 p in commonPoints) {
                edgePoints.Add(meshToPlane(p));
            }
            if (PolygonMath.lineSegmentsIntersect(planePos, planePosNext, edgePoints[0], edgePoints[1])) {
                //switch to new polygon
                print("switching polygon");
                polygon = new PolygonInfo(transform.parent.GetComponent<MeshFilter>().mesh, neighbor, transform.parent);
                planePos = meshToPlane(transform.position);
                planePosNext = meshToPlane(GetComponent<Rigidbody>().velocity) + planePos;
                break;
            }
        }

        transform.position = planeToMesh(planePos);
        Vector3 vel = meshToPlane(GetComponent<Rigidbody>().velocity);
        Vector3 forward = (Quaternion.FromToRotation(lastPolygonNormal, polygon.normal) * vel).normalized;
        //GetComponent<Rigidbody>().velocity = moveSpeed * Time.deltaTime * forward;
    }
    void DrawTriangle(List<Vector3> corners) {
        foreach (Vector3 p in corners) {
            Gizmos.DrawSphere(p, 0.01f);
        }
        for (int i = 0; i < 3; i++) {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 3]);
        }
    }
    Vector2 meshToPlane(Vector3 point) {
        Vector3 faceCenter = (polygon.points[0] + polygon.points[1] + polygon.points[2]) / 3f;
        return removeY(Vector3.ProjectOnPlane(Quaternion.FromToRotation(polygon.normal, Vector3.up) * (point - faceCenter), Vector3.up));
    }
    Vector3 planeToMesh(Vector2 point) {
        Vector3 faceCenter = (polygon.points[0] + polygon.points[1] + polygon.points[2]) / 3f;
        return Quaternion.FromToRotation(Vector3.up, polygon.normal) * addY(point) + faceCenter;
    }
    Vector2 removeY(Vector3 point) {
        return new Vector2(point.x, point.z);
    }
    Vector3 addY(Vector2 point) {
        return new Vector3(point.x, 0f, point.y);
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
            DrawTriangle(unfoldedPoints);
            List<Vector3> planarPoints = new List<Vector3>();
            foreach (Vector3 p in unfoldedPoints) {
                planarPoints.Add(addY(meshToPlane(p)));
            }
            Gizmos.color = Color.green;
            DrawTriangle(planarPoints);
        }
    }
}
