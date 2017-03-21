using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    public bool is2D;
    List<Vector3> vertices;
    List<int> triangles;

    public Material wallmat;
    public Material topmat;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

//<<<<<<< HEAD
   public IEnumerator<GameObject> GenerateMesh (int[,] map, float[,] heigtmap, float squareSize, int indexX, int indexY)
////=======
//    public void GenerateMesh(int[,] map, float squareSize, List<Vector2> roomCenters )
//>>>>>>> origin/DevValuta
    {
        var chankGO = new GameObject(indexX.ToString() + indexY.ToString());
        var topGO = new GameObject("top" + indexX.ToString()+ indexY.ToString());
        var wallsGO = new GameObject("walls" + indexX.ToString() + indexY.ToString());

        MeshFilter top = topGO.gameObject.AddComponent<MeshFilter>();
        MeshFilter walls = wallsGO.gameObject.AddComponent<MeshFilter>();

        MeshRenderer wallsRender = topGO.gameObject.AddComponent<MeshRenderer>();
        MeshRenderer topRender = wallsGO.gameObject.AddComponent<MeshRenderer>();

        wallsRender.material = wallmat;
        topRender.material = topmat;
        topGO.transform.parent = chankGO.transform;
        wallsGO.transform.parent = chankGO.transform;
        chankGO.transform.localPosition = new Vector3((map.GetLength(0)-3) * squareSize * indexX, 10, (map.GetLength(1)-3) * squareSize * indexY);

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, heigtmap, squareSize);

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
        top.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0)/2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;
        var wallMesh = new Mesh();
        if(!is2D)
        {
            wallMesh = CreateWallMesh((map.GetLength(0)));
        }
        var newGuid = Guid.NewGuid().ToString();

        walls.mesh = wallMesh;
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;

        MeshCollider wallCollidertop = top.gameObject.AddComponent<MeshCollider>();
        wallCollidertop.sharedMesh = mesh;


        AssetDatabase.CreateAsset(wallMesh, string.Format("Assets/1/wals{0}.prefab", newGuid));
        AssetDatabase.CreateAsset(mesh, string.Format("Assets/1/cave{0}.prefab", newGuid));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var gmcave = AssetDatabase.LoadAssetAtPath(string.Format("Assets/1/cave{0}.prefab", newGuid), typeof(Mesh)) as Mesh;
        var gmwals = AssetDatabase.LoadAssetAtPath(string.Format("Assets/1/wals{0}.prefab", newGuid), typeof(Mesh)) as Mesh;
        
        top.mesh = gmcave;
        walls.mesh = gmwals;
//<<<<<<< HEAD
        AssetDatabase.SaveAssets();

        //var emptyPrefab = PrefabUtility.CreateEmptyPrefab(string.Format("Assets/1/GO{0}.prefab", newGuid));
        yield return chankGO;
//=======
//        var wallPoints = new List<Vector2>();
//        //var emptyPrefab = PrefabUtility.CreateEmptyPrefab(string.Format("Assets/1/GO{0}.prefab", newGuid));
//        for(int i= 0; i < map.GetLength(0); i++)
//        {
//            for (int j = 0; j < map.GetLength(1); j++)
//            {
//                if(map[i,j] == 1)
//                {
//                    wallPoints.Add(new Vector2(i, j));
//                }
//            }
//        }
//        var newMap = gm.AddComponent<Map>() as Map;
//        newMap.RoomCenters = roomCenters;
//        newMap.RoomBorders = wallPoints;

//        PrefabUtility.CreatePrefab(string.Format("Assets/1/GO{0}.prefab", newGuid), gm);
        
//>>>>>>> origin/DevValuta

        //PrefabUtility.ReplacePrefab(gm, emptyPrefab);
    } 

    Mesh CreateWallMesh(int size)
    {
        CalculateMeshOutlines();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 10;
        var veclist = new List<Vector3>()
                {
                    new Vector3(size/4, size/4),
                    new Vector3((size/4)*3 , size/4),
                    new Vector3(size/4,(size/4)*3),
                    new Vector3((size/4)*3 , (size/4)*3),
                };

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); //top left vertex (0)
                wallVertices.Add(vertices[outline[i+1]]); //top right vertex (1)

                //if (vertices[outline[i]].x != 0 && vertices[outline[i]].x != size - 1 && vertices[outline[i]].z != 0 && vertices[outline[i]].z != size - 1)
                //{
                    var vector = veclist.OrderBy(c => Vector3.Distance(vertices[outline[i]], c)).First();
                    wallVertices.Add(Vector3.MoveTowards(vertices[outline[i]], vector, 0f) - Vector3.up * (wallHeight / 2)); //middle left vertex (2)
                    var vector2 = veclist.OrderBy(c => Vector3.Distance(vertices[outline[i+1]], c)).First();
                    wallVertices.Add(Vector3.MoveTowards(vertices[outline[i + 1]] , vector2, 0f) - Vector3.up * (wallHeight / 2));
                //}
                //else
                //{
                //    wallVertices.Add(vertices[outline[i]] - Vector3.up * (wallHeight / 2)); //middle left vertex (2)
                //    wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * (wallHeight / 2)); //middle right vertex (3)
                //}
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); //bottom left vertex (4)
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); //bottom right vertex (5)

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);

                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 4);
                wallTriangles.Add(startIndex + 5);

                wallTriangles.Add(startIndex + 5);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 2);

                //for (int j = 0; j <= florsubs; j = +2)
                //{
                //    wallTriangles.Add(startIndex + j + 0);
                //    wallTriangles.Add(startIndex + j + 2);
                //    wallTriangles.Add(startIndex + j + 3);

                //    wallTriangles.Add(startIndex + j + 3);
                //    wallTriangles.Add(startIndex + j + 1);
                //    wallTriangles.Add(startIndex + j + 0);
                //}

                //wallTriangles.Add(startIndex + florsubs + 0);
                //wallTriangles.Add(startIndex + florsubs + 2);
                //wallTriangles.Add(startIndex + florsubs + 3);

                //wallTriangles.Add(startIndex + florsubs + 3);
                //wallTriangles.Add(startIndex + florsubs + 1);
                //wallTriangles.Add(startIndex + florsubs + 0);
            }
        }
        
        wallMesh.vertices = wallVertices.ToArray();
        Vector2[] uvs = new Vector2[wallMesh.vertices.Length];
        for (int i = 0; i < wallMesh.vertices.Length; i++)
        {
            float x = wallMesh.vertices[i].x;
            if (i + 1 < wallMesh.vertices.Length && x == wallMesh.vertices[i + 1].x && i > 1 && wallMesh.vertices[i - 1].x == x)
            {
                //Fix bug texture
                x = wallMesh.vertices[i].z;
            }
            uvs[i] = new Vector2(x, wallMesh.vertices[i].y);
        }
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateNormals();
        
        wallMesh.uv = uvs;
        
        return wallMesh;
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            // 1 points selected in square
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

            // 2 points selected in square
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

            // 3 points selected in square
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

            // 4 points selected in square

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

        if(points.Length >=3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }

        if(points.Length >=4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }

        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }

        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node [] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1) //if the point has vertexindex (default index = -1) we know that this particular point hasn't been assigned yet
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
        if (triangleDictionary.ContainsKey (vertexIndexKey))
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
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++)
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
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if(nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for(int i = 0; i < trianglesContainingVertex.Count; i++)
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
            if (trianglesContainingVertexA [i].Contains(vertexB))
            {
                sharedTriangleCount++;

                if(sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle (int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this [int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains (int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float[,] height, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1); 
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int x = 0; x < nodeCountX; x++)
            {
                for(int y = 0; y <nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, height[x,y], -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square (ControlNode topLeft1, ControlNode topRight1, ControlNode bottomRight1, ControlNode bottomLeft1)
        {
            topLeft = topLeft1;
            topRight = topRight1;
            bottomLeft = bottomLeft1;
            bottomRight = bottomRight1;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
            {
                configuration += 8;
            }
            if(topRight.active)
            {
                configuration += 4;
            }
            if(bottomRight.active)
            {
                configuration += 2;
            }
            if(bottomLeft.active)
            {
                configuration += 1;
            }
        }
    }

	public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node (Vector3 pos)
        {
            position = pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active; // active = wall , not active = not a wall
        public Node above, right;

        public ControlNode (Vector3 pos1, bool active1, float squareSize) : base (pos1)
        {
            active = active1;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
