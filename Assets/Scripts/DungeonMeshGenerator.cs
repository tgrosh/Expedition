using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter ground;
    public MeshFilter ceiling;
    public MeshFilter walls;
    public MeshFilter roomOutline;
    public int wallHeight = 5;
    public bool is2D;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize)
    {

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        roomOutline.mesh = mesh;

        roomOutline.transform.position = new Vector3(0, wallHeight, 0);
        walls.transform.position = new Vector3(0, wallHeight, 0);
        ceiling.transform.position = new Vector3(0, wallHeight, 0);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;


        if (is2D)
        {
            Generate2DColliders();
        }
        else
        {
            CreateWallMesh(map, squareSize);
        }
    }

    void CreateWallMesh(int[,] map, float squareSize)
    {

        MeshCollider currentCollider = GetComponent<MeshCollider>();
        Destroy(currentCollider);

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallLength = 0;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);

                wallLength += Vector3.Distance(vertices[outline[i]], vertices[outline[i + 1]]);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateBounds();
        wallMesh.RecalculateNormals();

        //int tileAmount = 1;
        //Vector2[] uvs = new Vector2[wallVertices.Count];
        //for (int i = 0; i < wallVertices.Count; i++)
        //{
        //    float percentX = Mathf.InverseLerp(0, wallLength, Vector3.Distance(wallVertices[0], wallVertices[i])) * tileAmount;
        //    float percentY = Mathf.InverseLerp(wallHeight, 0, wallVertices[i].y) * tileAmount;
        //    uvs[i] = new Vector2(percentX, percentY);
        //}
        //wallMesh.uv = uvs;
        BoxUV(wallMesh, walls.transform);

        walls.mesh = wallMesh;

        MeshCollider wallCollider = gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    void Generate2DColliders()
    {

        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);
            }
            edgeCollider.points = edgePoints;
        }

    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }

    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }

        SimplifyMeshOutlines();
    }

    void SimplifyMeshOutlines()
    {
        for (int outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex++)
        {
            List<int> simplifiedOutline = new List<int>();
            Vector3 dirOld = Vector3.zero;
            for (int i = 0; i < outlines[outlineIndex].Count; i++)
            {
                Vector3 p1 = vertices[outlines[outlineIndex][i]];
                Vector3 p2 = vertices[outlines[outlineIndex][(i + 1) % outlines[outlineIndex].Count]];
                Vector3 dir = p1 - p2;
                if (dir != dirOld)
                {
                    dirOld = dir;
                    simplifiedOutline.Add(outlines[outlineIndex][i]);
                }
            }
            outlines[outlineIndex] = simplifiedOutline;
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }




    public Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).normalized;
    }

    public int GetBoxDir(Vector3 v)
    {
        float x = Mathf.Abs(v.x);
        float y = Mathf.Abs(v.y);
        float z = Mathf.Abs(v.z);
        if (x > y && x > z)
        {
            return v.x < 0 ? -1 : 1;
        }
        else if (y > z)
        {
            return v.y < 0 ? -2 : 2;
        }
        return v.z < 0 ? -3 : 3;
    }

    public Vector2 GetBoxUV(Vector3 vertex, int boxDir)
    {
        if (boxDir == -1 || boxDir == 1)
        {
            // X - axis
            return new Vector2(vertex.z * Mathf.Sign(boxDir), vertex.y);
        }
        else if (boxDir == -2 || boxDir == 2)
        {
            // Y - axis
            return new Vector2(vertex.x, vertex.z * Mathf.Sign(boxDir));
        }
        else
        {
            // Z - axis
            return new Vector2(vertex.x * -Mathf.Sign(boxDir), vertex.y);
        }
    }

    // This can easily be generalized to support any configuration of planar projection
    // Instead of mapping to box directions, you could supply an array of directions or planes
    // and map to them.
    public void BoxUV(Mesh mesh, Transform tform)
    {
        // Matrix 
        Matrix4x4 matrix = tform.localToWorldMatrix;

        // TODO: transfer vertex colors, etc.
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;

        Vector3[] worldVerts = new Vector3[verts.Length];
        for (int i = 0; i < worldVerts.Length; i++)
        {
            worldVerts[i] = matrix.MultiplyPoint(verts[i]);
        }

        // Lists for new mesh..
        List<Vector3> newVerts = new List<Vector3>(verts.Length);
        List<Vector3> newNormals = new List<Vector3>(verts.Length);
        List<Vector2> newUVs = new List<Vector2>(verts.Length);
        List<List<int>> newTris = new List<List<int>>();

        // Prepare a map to vertices to box directions
        Dictionary<int, int[]> vertexMap = new Dictionary<int, int[]>();
        for (int i = -3; i <= 3; i++)
        {
            if (i == 0)
                continue;

            int[] vmap = new int[verts.Length];
            for (int v = 0; v < vmap.Length; v++)
            {
                vmap[v] = -1;
            }
            vertexMap.Add(i, vmap);
        }

        // Compute triangle normal for each tri, and rebuild it with unique verts
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            int[] tris = mesh.GetTriangles(s);
            newTris.Add(new List<int>());

            for (int t = 0; t < tris.Length; t += 3)
            {
                int v0 = tris[t];
                int v1 = tris[t + 1];
                int v2 = tris[t + 2];

                Vector3 triNormal = TriangleNormal(worldVerts[v0], worldVerts[v1], worldVerts[v2]);

                int boxDir = GetBoxDir(triNormal);

                // Remap triangle verts
                for (int i = 0; i < 3; i++)
                {
                    int v = tris[t + i];

                    // If vertex doesn't already exist in boxDir vertex map,
                    // we'll add a copy of it with the correct UV
                    if (vertexMap[boxDir][v] < 0)
                    {
                        // Compute UV
                        Vector2 vertexUV = GetBoxUV(worldVerts[v], boxDir);

                        vertexMap[boxDir][v] = newVerts.Count;
                        newVerts.Add(verts[v]);
                        newNormals.Add(normals[v]);
                        newUVs.Add(vertexUV);
                    }

                    // Use remapped vertex index
                    newTris[s].Add(vertexMap[boxDir][v]);
                }
            }
        }

        mesh.vertices = newVerts.ToArray();
        mesh.normals = newNormals.ToArray();
        mesh.uv = newUVs.ToArray();

        // TODO: Recalculate tangents

        for (int s = 0; s < newTris.Count; s++)
        {
            mesh.SetTriangles(newTris[s].ToArray(), s);
        }
    }
}
