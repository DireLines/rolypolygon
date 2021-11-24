using System.Collections;
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
}
