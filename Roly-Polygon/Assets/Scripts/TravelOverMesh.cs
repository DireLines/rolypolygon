using System;
using System.Collections.Generic;
using UnityEngine;
public class EdgeTransition {
    public float position;//0.0 to 1.0, position along edge
    public float angle;//angle as measured clockwise from the perpendicular into the polygon
    public EdgeTransition(float position, float angle) {
        this.position = position;
        this.angle = angle;
    }
    public static EdgeTransition passThrough(EdgeTransition transition) {
        return transition;
    }
    public static EdgeTransition reflect(EdgeTransition transition) {
        return new EdgeTransition(transition.position, -transition.angle);
    }
    public static EdgeTransition normal(EdgeTransition transition) {
        return new EdgeTransition(transition.position, 0);
    }
    public static EdgeTransition midpoint(EdgeTransition transition) {
        return new EdgeTransition(0.5f, transition.angle);
    }
    public static EdgeTransition random(EdgeTransition transition) {
        return new EdgeTransition(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(-90f, 90f));
    }
}

public class TravelOverMesh : MonoBehaviour {
    PolygonInfo polygon;
    public LayerMask layerMask;
    MeshNavigation meshNavigation;
    public float moveSpeed = 1f;
    Vector3 positionLF;
    Vector3 displayPoint1;
    Vector3 displayPoint2;

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
        transform.position = planeToMesh(meshToPlane(transform.position));
        positionLF = transform.position;
        GetComponent<Rigidbody>().velocity = UnityEngine.Random.onUnitSphere.normalized * Time.deltaTime * moveSpeed;
    }
    // Update is called once per frame
    void Update() {
        Vector3 lastPolygonNormal = polygon.normal;
        Vector2 planePosLF = meshToPlane(positionLF);
        Vector2 planePos = meshToPlane(transform.position);
        Func<EdgeTransition, EdgeTransition> transitionLogic = EdgeTransition.normal;
        List<Vector2> polygonPoints2D = new List<Vector2>();
        foreach (Vector3 p in polygon.points) {
            polygonPoints2D.Add(meshToPlane(p));
        }
        foreach (int neighbor in meshNavigation.getNeighboringFaceIndices(polygon.triangleIndex)) {
            List<Vector3> l = meshNavigation.getTriangleVertices(neighbor);
            List<Vector3> commonPoints = PolygonMath.commonPoints(polygon.points, l);
            List<Vector2> edgePoints = new List<Vector2>();
            foreach (Vector3 p in commonPoints) {
                edgePoints.Add(meshToPlane(p));
            }
            if (PolygonMath.lineSegmentsIntersect(planePosLF, planePos, edgePoints[0], edgePoints[1])) {
                if (PolygonMath.pointInTriangle(planePos, polygonPoints2D[0], polygonPoints2D[1], polygonPoints2D[2])) {
                    break;
                }
                //find point of intersection
                Vector2 intersection = PolygonMath.intersectionPoint(planePosLF, planePos, edgePoints[0], edgePoints[1]);
                Vector2 perpendicular = Vector2.Perpendicular(edgePoints[1] - edgePoints[0]);
                if (Vector2.Dot(planePosLF - intersection, perpendicular) < 0) {
                    perpendicular = -perpendicular;
                }
                float incidentAngle = Mathf.Rad2Deg * Vector2.SignedAngle(perpendicular, planePosLF - intersection);
                float proportion = (intersection - edgePoints[0]).magnitude / (edgePoints[1] - edgePoints[0]).magnitude;
                EdgeTransition transition = transitionLogic(new EdgeTransition(proportion, incidentAngle));
                perpendicular = -perpendicular;
                planePos = transition.position * edgePoints[1] + (1 - transition.position) * edgePoints[0];
                Vector2 vel2D = Quaternion.Euler(0, 0, Mathf.Deg2Rad * transition.angle) * perpendicular;
                Vector3 vel = GetComponent<Rigidbody>().velocity;
                bool switchingPolygon = transition.angle > -90f && transition.angle < 90f;
                if (switchingPolygon) {
                    print("switching polygon");
                    Vector3 neighboringNormal = PolygonMath.getNormalFromPoints(l);
                    Func<Vector2, Vector3> planeToNeighbor = (point) => //align with main face, then hinge to align with neighbor
                    PolygonMath.rotateAround(
                            planeToMesh(point),
                            commonPoints[0],
                            Quaternion.FromToRotation(
                                polygon.normal,
                                neighboringNormal));
                    GetComponent<Rigidbody>().velocity = planeToNeighbor(vel2D).normalized * vel.magnitude;
                    Vector3 pos3DBeforeSwitch = planeToMesh(planePos);
                    polygon = new PolygonInfo(transform.parent.GetComponent<MeshFilter>().mesh, neighbor, transform.parent);
                    planePos = meshToPlane(pos3DBeforeSwitch);
                } else {
                    GetComponent<Rigidbody>().velocity = planeToMesh(vel2D).normalized * vel.magnitude;
                }
                break;
            }
        }
        //if here, we didn't cross an edge
        transform.position = planeToMesh(planePos);
        displayPoint1 = positionLF;
        positionLF = transform.position;
        displayPoint2 = transform.position;
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
}
