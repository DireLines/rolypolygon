using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
//https://answers.unity.com/questions/1615363/how-to-find-connecting-mesh-triangles.html
public class MeshTriangleNeighbors {
    public class Vertex {
        public Vector3 position;
    }

    public struct Edge {
        public Vertex v1;
        public Vertex v2;
        public Edge(Vertex aV1, Vertex aV2) {
            // ensure the same order to guarantee equality
            if (aV1.GetHashCode() > aV2.GetHashCode()) {
                v1 = aV1; v2 = aV2;
            } else {
                v1 = aV2; v2 = aV1;
            }
        }
    }
    public class TrianglePair {
        public int t1 = -1;
        public int t2 = -1;
        public bool Add(int aTriangleIndex) {
            if (t1 == -1)
                t1 = aTriangleIndex;
            else if (t2 == -1)
                t2 = aTriangleIndex;
            else
                return false;
            return true;
        }

    }
    public class Neighbors {
        public int t1 = -1;
        public int t2 = -1;
        public int t3 = -1;
    }



    Dictionary<int, Vertex> verticesLookup = new Dictionary<int, Vertex>();
    Dictionary<Edge, TrianglePair> edges;

    // mesh vertex index as key
    public static List<Vertex> FindSharedVertices(Vector3[] aVertices) {
        var list = new List<Vertex>();
        for (int i = 0; i < aVertices.Length; i++) {
            Vertex v = null;
            foreach (var item in list) {
                if ((item.position - aVertices[i]).sqrMagnitude < 0.0001f) {
                    v = item;
                    break;
                }
            }
            if (v == null) {
                v = new Vertex { position = aVertices[i] };
            }
            list.Add(v);
        }
        return list;
    }
    public static Dictionary<Edge, TrianglePair> CreateEdgeList(List<Vertex> aTriangles) {
        var res = new Dictionary<Edge, TrianglePair>();
        int count = aTriangles.Count / 3;
        for (int i = 0; i < count; i++) {
            Vertex v1 = aTriangles[i * 3];
            Vertex v2 = aTriangles[i * 3 + 1];
            Vertex v3 = aTriangles[i * 3 + 2];
            TrianglePair p;
            Edge e;
            e = new Edge(v1, v2);
            if (!res.TryGetValue(e, out p)) {
                p = new TrianglePair();
                res.Add(e, p);
            }
            p.Add(i);
            e = new Edge(v2, v3);
            if (!res.TryGetValue(e, out p)) {
                p = new TrianglePair();
                res.Add(e, p);
            }
            p.Add(i);
            e = new Edge(v3, v1);
            if (!res.TryGetValue(e, out p)) {
                p = new TrianglePair();
                res.Add(e, p);
            }
            p.Add(i);
        }
        return res;
    }

    public static List<int> GetNeighbors(Dictionary<Edge, TrianglePair> aEdgeList, List<Vertex> aTriangles) {
        var res = new List<int>();
        int count = aTriangles.Count / 3;
        for (int i = 0; i < count; i++) {
            Vertex v1 = aTriangles[i * 3];
            Vertex v2 = aTriangles[i * 3 + 1];
            Vertex v3 = aTriangles[i * 3 + 2];
            TrianglePair p;
            if (aEdgeList.TryGetValue(new Edge(v1, v2), out p)) {
                if (p.t1 == i)
                    res.Add(p.t2);
                else
                    res.Add(p.t1);
            } else
                res.Add(-1);
            if (aEdgeList.TryGetValue(new Edge(v2, v3), out p)) {
                if (p.t1 == i)
                    res.Add(p.t2);
                else
                    res.Add(p.t1);
            } else
                res.Add(-1);
            if (aEdgeList.TryGetValue(new Edge(v3, v1), out p)) {
                if (p.t1 == i)
                    res.Add(p.t2);
                else
                    res.Add(p.t1);
            } else
                res.Add(-1);
        }
        return res;
    }
    public static List<int> GetNeighbors(Mesh aMesh) {
        var vertexList = FindSharedVertices(aMesh.vertices);
        var tris = aMesh.triangles;
        var triangles = new List<Vertex>(tris.Length);
        foreach (var t in tris)
            triangles.Add(vertexList[t]);
        var edges = CreateEdgeList(triangles);
        return GetNeighbors(edges, triangles);
    }
}
public struct PolygonInfo {
    public PolygonInfo(Mesh mesh, int triangleIndex, Transform transform) {
        this.mesh = mesh;
        this.triangleIndex = triangleIndex;
        points = new List<Vector3>(getTriangleVertices(mesh, triangleIndex, transform));
        normal = PolygonMath.getNormalFromPoints(points);
    }
    static Vector3[] getTriangleVertices(Mesh mesh, int triangleIndex, Transform transform) {
        int triangleStart = triangleIndex * 3;
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] corners = { verts[tris[triangleStart]], verts[tris[triangleStart + 1]], verts[tris[triangleStart + 2]] };
        Vector3[] result = corners;
        for (int i = 0; i < 3; i++) {
            result[i] = transform.TransformPoint(corners[i]);
        }
        return result;
    }
    public List<Vector3> points { get; }
    public Mesh mesh { get; }
    public int triangleIndex { get; }
    public Vector3 normal { get; }
}
public class PolygonMath {
    public static bool rayIntersectLineSegment(Vector2 rayStart, Vector2 rayDirection, Vector2 p1, Vector2 p2) {
        return true;
    }
    public static int hitEdgeInPolygon(List<Vector2> vertices, Vector2 position, Vector2 velocity) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 p1 = vertices[i];
            Vector2 p2 = vertices[(i + 1) % vertices.Count];
        }
        return 0;
    }
    public static List<Vector3> commonPoints(List<Vector3> l1, List<Vector3> l2, double epsilon = 0.0001f) {
        List<Vector3> results = new List<Vector3>();
        foreach (Vector3 p1 in l1) {
            foreach (Vector3 p2 in l2) {
                if (Vector3.SqrMagnitude(p2 - p1) < epsilon) {
                    results.Add(p1);
                    continue;
                }
            }
        }
        return results;
    }
    public static Vector3 getNormalFromPoints(List<Vector3> points) {
        Vector3 p1 = points[0], p2 = points[1], p3 = points[2];
        return Vector3.Cross(p2 - p3, p2 - p1).normalized;
    }
    public static Vector3 rotateAround(Vector3 point, Vector3 pivot, Quaternion rotation) {
        return rotation * (point - pivot) + pivot;
    }
    // https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    static bool onSegment(Vector2 p, Vector2 q, Vector2 r) {
        if (q.x <= Mathf.Max(p.x, r.x) &&
            q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) &&
            q.y >= Mathf.Min(p.y, r.y)) {
            return true;
        }

        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    static int orientation(Vector2 p, Vector2 q, Vector2 r) {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0;  // collinear

        return (val > 0) ? 1 : 2; // clockwise or counterclockwise
    }

    // The main function that returns true if line segment 'a1a2'
    // and 'b1b2' intersect.
    public static bool lineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(a1, a2, b1);
        int o2 = orientation(a1, a2, b2);
        int o3 = orientation(b1, b2, a1);
        int o4 = orientation(b1, b2, a2);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // a1, a2 and b1 are collinear and b1 lies on segment a1a2
        if (o1 == 0 && onSegment(a1, b1, a2)) return true;

        // a1, a2 and b2 are collinear and b2 lies on segment a1a2
        if (o2 == 0 && onSegment(a1, b2, a2)) return true;

        // b1, b2 and a1 are collinear and a1 lies on segment b1b2
        if (o3 == 0 && onSegment(b1, a1, b2)) return true;

        // b1, b2 and a2 are collinear and a2 lies on segment b1b2
        if (o4 == 0 && onSegment(b1, a2, b2)) return true;

        return false; // Doesn't fall in any of the above cases
    }

    public static Vector2 intersectionPoint(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        float dxa = a2.x - a1.x;
        float dxb = b2.x - b1.x;
        Func<float, float> formulaB = (x) =>
        {
            float m = (b2.y - b1.y) / (b2.x - b1.x);
            return m * (x - b1.x) + b1.y;
        };
        Func<float, float> formulaA = (x) =>
        {
            float m = (a2.y - a1.y) / (a2.x - a1.x);
            return m * (x - a1.x) + a1.y;
        };
        if (dxa == 0 && dxb == 0) {
            //parallel vertical lines - no single intersection point
            Debug.Log("hit a weird case");
            return new Vector2(0, 0);
        }
        if (dxa == 0) {
            //a is vertical
            return new Vector2(a1.x, formulaB(a1.x));
        }
        if (dxb == 0) {
            //b is vertical
            return new Vector2(b1.x, formulaA(b1.x));
        }
        //normal case
        float ma = (a2.y - a1.y) / (a2.x - a1.x);
        float mb = (b2.y - b1.y) / (b2.x - b1.x);
        if (ma == mb) {
            //lines are parallel - no single intersection point
            Debug.Log("hit a weird case");
            return new Vector2(0, 0);
        }
        float intersectionX = (b1.y - a1.y + ma * a1.x - mb * b1.x) / (ma - mb);
        return new Vector2(intersectionX, formulaA(intersectionX));
    }
    //is p inside the triangle t1-t2-t3?
    //https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    public static bool pointInTriangle(Vector2 p, Vector2 t1, Vector2 t2, Vector2 t3) {
        var s = (t1.x - t3.x) * (p.y - t3.y) - (t1.y - t3.y) * (p.x - t3.x);
        var t = (t2.x - t1.x) * (p.y - t1.y) - (t2.y - t1.y) * (p.x - t1.x);

        if ((s < 0) != (t < 0) && s != 0 && t != 0)
            return false;

        var d = (t3.x - t2.x) * (p.y - t2.y) - (t3.y - t2.y) * (p.x - t2.x);
        return d == 0 || (d < 0) == (s + t <= 0);
    }
}