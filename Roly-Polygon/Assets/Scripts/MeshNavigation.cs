using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Rigidbody))]
public class MeshNavigation : MonoBehaviour {
    public List<int> neighboringFaces;
    public Mesh mesh;
    // Start is called before the first frame update
    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        neighboringFaces = MeshTriangleNeighbors.GetNeighbors(mesh);
    }

    List<int> getNeighboringFaceIndices(int triangleIndex) {
        int triangleStart = triangleIndex * 3;
        return new List<int> { neighboringFaces[triangleStart], neighboringFaces[triangleStart + 1], neighboringFaces[triangleStart + 2] };
    }
    Vector3[] getTriangleVertices(int triangleIndex) {
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
    public List<List<Vector3>> getNeighboringFaces(int triangleIndex) {
        var result = new List<List<Vector3>>();
        var indices = getNeighboringFaceIndices(triangleIndex);
        for (int i = 0; i < 3; i++) {
            result.Add(new List<Vector3>(getTriangleVertices(indices[i])));
        }
        return result;
    }
}
