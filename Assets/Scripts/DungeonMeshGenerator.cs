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

    public List<GameObject> wallPropPrefabs = new List<GameObject>();
    public List<GameObject> doorPrefabs = new List<GameObject>();

    public bool debugDraw;
    public List<Material> debugMaterials = new List<Material>();

    List<Vector3> vertices;
    List<int> triangles;
    float squareSize;
    int[,] map;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize)
    {

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        this.map = map;
        this.squareSize = squareSize;
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
            Clutter();
            PlaceDoors();
        }
        
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("DebugPlane"))
        {
            Destroy(obj);
        }
        if (debugDraw)
        {
            DebugDraw(debugMaterials);
        }
    }
    
    public void DebugDraw(List<Material> debugMaterials)
    {
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                if (map[x, y] != (int)TileType.Wall)
                {
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    plane.transform.localScale = Vector3.one * (1 / squareSize) * .1f;
                    plane.transform.position = squareGrid.squares[x, y].bottomLeft.position + (Vector3.up * .1f);
                    plane.transform.SetParent(gameObject.transform);

                    plane.GetComponent<Renderer>().material = debugMaterials[map[x, y]];
                    plane.tag = "DebugPlane";
                }
            }
        }
    }

    public void PlaceDoors()
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (wallPropPrefabs.Count > 0 && map[x, y] == (int)TileType.Door)
                {
                    PlaceDoor(x, y);
                }
            }
        }
    }

    public void Clutter()
    {        
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (wallPropPrefabs.Count > 0 && map[x,y] == (int)TileType.WallProp)
                {
                    PlaceWallPrefab(x, y);
                }
            }
        }
    }

    public void PlaceDoor(int x, int y)
    {
        float rotation = -1;

        if ((x < map.GetLength(0) - 3 && map[x + 3, y] == (int)TileType.Wall) && (x > 2 && map[x - 3, y] == (int)TileType.Wall))
        {
            //wall on right AND left
            rotation = 0f;
        } else if ((y < map.GetLength(1) - 3 && map[x, y + 3] == (int)TileType.Wall) && (y > 2 && map[x, y - 3] == (int)TileType.Wall))
        {
            //wall below AND above?
            rotation = 90f;
        }

        if (rotation != -1)
        {
            GameObject door = Instantiate(doorPrefabs[UnityEngine.Random.Range(0, doorPrefabs.Count - 1)]);
            door.transform.position = new Vector3((x - map.GetLength(0) / 2f) + .5f, 0, (y - map.GetLength(1) / 2f) + .5f);
            door.transform.Rotate(new Vector3(0, rotation, 0));
            door.transform.SetParent(gameObject.transform);
        }
    }

    public void PlaceWallPrefab(int x, int y)
    {
        GameObject wallProp = Instantiate(wallPropPrefabs[UnityEngine.Random.Range(0, wallPropPrefabs.Count - 1)]);
        wallProp.transform.position = new Vector3((x - map.GetLength(0) / 2f)+.5f, wallHeight * .66f, (y - map.GetLength(1) / 2f) + .5f);
        wallProp.transform.SetParent(gameObject.transform);

        if (x < map.GetLength(0) - 1 && map[x + 1, y] == (int)TileType.Wall)
        {
            //wall on right
            wallProp.transform.Rotate(new Vector3(0, -90f, 0));
        }
        else if (x > 0 && map[x - 1, y] == (int)TileType.Wall)
        {
            //wall on left
            wallProp.transform.Rotate(new Vector3(0, 90f, 0));
        }
        else if (y < map.GetLength(1) - 1 && map[x, y + 1] == (int)TileType.Wall)
        {
            //wall below?
            wallProp.transform.Rotate(new Vector3(0, 180f, 0));
        }
        else if (y > 0 && map[x, y - 1] == (int)TileType.Wall)
        {
            //wall above?
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

        float prevVertDistance = 0;
        List<float> vertDistances = new List<float>();
        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // top left
                wallVertices.Add(vertices[outline[i + 1]]); // top right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);

                float newVertDistance = Vector3.Distance(vertices[outline[i]], vertices[outline[i + 1]]);
                vertDistances.Add(prevVertDistance);
                vertDistances.Add(prevVertDistance + newVertDistance);
                vertDistances.Add(prevVertDistance);
                vertDistances.Add(prevVertDistance + newVertDistance);
                prevVertDistance += newVertDistance;

                wallLength += newVertDistance;
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateBounds();
        wallMesh.RecalculateNormals();

        Vector2[] uvs = new Vector2[wallVertices.Count];
        for (int i = 0; i < wallVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(0, wallLength, vertDistances[i]);
            float percentY = Mathf.InverseLerp(0, wallHeight, wallVertices[i].y * -1f);
            uvs[i] = new Vector2(percentX, percentY);
        }
        wallMesh.uv = uvs;
        wallMesh.RecalculateTangents();

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

}
