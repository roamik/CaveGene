using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using System.Collections;

public class MapGenerator : MonoBehaviour {

    public static int width = 64;
    public static int height = 64;

    public static int chankSizeX = 8;
    public static int chankSizeY = 8;

    public int mpWidth;
    public int mpHeight;
    public int mpChankX;
    public int mpChankY;

    public string seed;
    public bool useRandomSeed;

    [Range (0,100)]
    public int randomFillPercent;

    public List<Vector2> navPoints;

    int[,] map;
    public List<GameObject> Chanks; Texture2D hMap;

    
 
 //Bottom left section of the map, other sections are similar
 
         //Add each new vertex in the plane
   //hMap.GetPixel(i, j).grayscale;
    
    void Start()
    {
        mpWidth = width;
        mpHeight = height;
        mpChankX = chankSizeX;
        mpChankY = chankSizeY;
        Chanks = new List<GameObject>();
        hMap = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format("Assets/Materials/Heightmap.jpg"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(GenerateMap()); ;
        }
        
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (var v in navPoints)
        {
            Gizmos.DrawSphere(new Vector3(v.x, 10, v.y), 2);
        }
    }


    IEnumerator GenerateMap()
    {
        map = new int[width, height];
        yield return StartCoroutine(RandomFillMap());
        Debug.Log("Random map filed");
        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(SmoothMap());
            Debug.Log("Random map Smoothed "+i+"/5");
        }

        yield return StartCoroutine(ProcessMap());
        int borderSize = chankSizeX / 2;

        Debug.Log(string.Format("Start bordering map..."));
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        Debug.Log(string.Format("End bordering map..."));
        var newGuid = Guid.NewGuid().ToString();
        MeshGenerator meshGen = GetComponent<MeshGenerator>();

        var list = borderedMap.ToSquare2D(chankSizeX);
        hMap.Resize(borderedMap.GetLength(0), borderedMap.GetLength(1));
        var listheights = hMap.ToSquare2D(chankSizeX,150);
        int indexX = 0;
        var mapGO = Instantiate(new GameObject("map" + newGuid),new Vector3(0,10,0),new Quaternion()) as GameObject;
        foreach (var inserList in list)
        {
            int indexY = 0;
            foreach (var l in inserList)
            {                
                var cd = new CoroutineWithData(this, meshGen.GenerateMesh(l, 1, indexX, indexY));
                yield return cd.coroutine;
                var gm = cd.result as GameObject;
                gm.transform.parent = mapGO.transform;
                //Chanks.Add(gm);
                Debug.Log(string.Format("Plasing meshes... {0}/{1},{2}/{3}", indexX, list.Count(), indexY, inserList.Count()));
                indexY++;
            }
            indexX++;
        }
        PrefabUtility.CreatePrefab(string.Format("Assets/1/maps/map{0}.prefab", newGuid), mapGO);
        //AssetDatabase.CreateAsset(mapGO, );
    }

    IEnumerator ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);

        int wallTresholdSize = 100;

        int blaX = 0;
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallTresholdSize)
            {
                int blaY = 0;
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                    
                   // Debug.Log(string.Format("wallRegion {0}/{1}, tile {2}/{3}",  blaX, wallRegions.Count(), blaY, wallRegion.Count()));
                    blaY++;
                }
            }blaX++;
        }
        yield return null;

        List<List<Coord>> roomRegions = GetRegions(0);

        int roomTresholdSize = 55;
        List<Room> notFilteredRooms = new List<Room>(); // notFilteredRooms = rooms wich 'survived' due filtering
        int roomRegionCount = 0;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomTresholdSize)
            {
                int tileCount = 0;
                foreach (Coord tile in roomRegion)
                {                    
                    map[tile.tileX, tile.tileY] = 1;
                    //yield return null;
                   // Debug.Log(string.Format("roomRegion {0}/{1}, tile {2}/{3}", roomRegionCount, roomRegions.Count(), tileCount, roomRegion.Count()));
                    tileCount++;
                }
            }
            else
            {
                notFilteredRooms.Add(new Room(roomRegion, map));
                //yield return null;
                Debug.Log(string.Format("roomRegion {0}/{1}", roomRegionCount, roomRegions.Count()));
            }
            roomRegionCount++;
        }
        yield return null;
        Debug.Log(string.Format("Start sorting rooms..."));
        notFilteredRooms.Sort();
        Debug.Log(string.Format("End sorting rooms..."));

        notFilteredRooms[0].isMainRoom = true;
        notFilteredRooms[0].isAccessibleFromMainRoom = true;
        yield return StartCoroutine(ConnectClosestRooms(notFilteredRooms));
        Debug.Log(string.Format("Connect closest rooms..."));
        navPoints.AddRange(notFilteredRooms.Select(c => c.centerTile).ToList().ToVector());

    }

    IEnumerator ConnectClosestRooms (List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if(forceAccessibilityFromMainRoom)
        {
            int roomCount = 0;
            foreach (Room room in allRooms)
            {
                if(room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
                
                Debug.Log(string.Format("room {0}/{1}", roomCount, allRooms.Count()));                
                roomCount++;
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }
        yield return null;
        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;
        int roomACount = 0;
        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            foreach(Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB)) 
                {
                    continue;
                }
               
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];

                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                yield return StartCoroutine(CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB));
                Debug.Log(string.Format("room {0} conected", roomACount));
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            yield return StartCoroutine(CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB));
            yield return StartCoroutine(ConnectClosestRooms(allRooms, true));
        }

        if (!forceAccessibilityFromMainRoom)
        {
            yield return StartCoroutine(ConnectClosestRooms(allRooms, true));
        }
        roomACount++;
    }

    IEnumerator CreatePassage (Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {            
            DrawCircle(c, 2); // the weight of passage way (corridor)           
        }
        yield return null;
    }

    void DrawCircle (Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x*x + y*y <= r*r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;

                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine (Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign (dx);             //a value to increment x each step
        int gradientStep = Math.Sign (dy);     // value for changing Y each step 

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest)
            {
                if(inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if(IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    IEnumerator RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random psRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++ )
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1; //wall
                }
                else
                {
                    map[x, y] = (psRandom.Next(0, 100) < randomFillPercent) ? 1 : 0; //if less than randomFillPercent than in this tile = 1, else = 0
                }
            }
        }
        yield return null;
    }

    IEnumerator SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if(neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
        yield return null;
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX,neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord (int x, int y)
        {
            tileX = x;
            tileY = y;
        }
        public Vector2 ToVector()
        {
            return new Vector2(tileX, tileY);
        }
        

    }
    
    class Room : IComparable <Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public Coord centerTile;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room ()
        {

        }
        public Room (List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            int totalX = 0, totalY = 0;
            
            foreach (Coord tile in tiles)
            {
                for(int x = tile.tileX-1; x <= tile.tileX+1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if(x== tile.tileX || y == tile.tileY)
                        {
                            if(map[x,y] == 1)
                            {
                                edgeTiles.Add(tile);
                                continue;
                            }
                        }
                    }
                } 
                totalX += tile.tileX;
                totalY += tile.tileY;                
            }
            var innertiles = tiles.Except(edgeTiles).ToList();
            int centerX = totalX / tiles.Count;
            int centerY = totalY / tiles.Count;
           

            var v = new Vector2(centerX, centerY).GetClosestPoint(innertiles.ToVector());
            centerTile = new Coord((int)v.x,(int)v.y);
        }


        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;

                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo (Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}

public class CoroutineWithData
{ 
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
public static class LinqHelper
{
    public static List<Vector2> ToVector(this List<MapGenerator.Coord> coords)
    {
        var list = new List<Vector2>();
        foreach (var c in coords)
        {
            list.Add(new Vector2(c.tileX, c.tileY));
        }
        return list;
    }
    public static Vector2 GetClosestPoint(this Vector2 v1, IEnumerable<Vector2> points)
    {
        var sorted = points.OrderBy(v2 => Vector2.Distance(v1,v2));
        return sorted.First();
    }
    public static List<List<T[,]>> ToSquare2D<T>(this T[,] array, int size)
    {
        var blaX = ((int)array.GetLength(0) / size);
        var blaY = ((int)array.GetLength(1) / size);
        var sizeX=  array.GetLength(0);
        var sizeY = array.GetLength(1);
        var returned = new List<List<T[,]>>(blaX);
        for (var kx = 0; kx < sizeX; kx += size)
        {
            var inner = new List<T[,]>(blaY);
            for (var ky = 0; ky < sizeY; ky += size)
            {
                var buffer = new T[size+2, size+2];
                int bufX = 1;
                for (var i = kx; i < kx + size; i++)
                {
                    int bufY = 1;
                    for (var j = ky; j < ky + size; j++)
                    {                        
                        buffer[bufX, bufY] = array[i, j];
                        bufY++;                        
                    }
                    bufX++;
                }
                inner.Add(buffer);
            }
            returned.Add(inner);
        }
        return returned;
    }
    public static List<List<float[,]>> ToSquare2D(this Texture2D array, int size, int heigth)
    {
        var blaX = ((int)array.height / size);
        var blaY = ((int)array.width / size);
        var sizeX = array.height;
        var sizeY = array.width;
        var returned = new List<List<float[,]>>(blaX);
        for (var kx = 0; kx < sizeX; kx += size)
        {
            var inner = new List<float[,]>(blaY);
            for (var ky = 0; ky < sizeY; ky += size)
            {
                var buffer = new float[size + 2, size + 2];
                int bufX = 1;
                for (var i = kx; i < kx + size; i++)
                {
                    int bufY = 1;
                    for (var j = ky; j < ky + size; j++)
                    {
                        if (i > 0 && j > 0 && i < sizeX && j < sizeY)
                        {
                            buffer[bufX, bufY] = array.GetPixel(i, j).grayscale * heigth;
                        }
                        //else
                        //{
                        //    if (i < 0 && j < 0)
                        //    {
                        //        buffer[bufX, bufY] =( array.GetPixel(i+1, j+1).grayscale +1) * heigth;
                        //    }
                        //    else if (i < 0 && j < sizeY)
                        //    {
                        //        buffer[bufX, bufY] =( array.GetPixel(i+1, j).grayscale + 1 )* heigth;
                        //    }
                        //    else if (j < 0 && i < sizeX)
                        //    {
                        //        buffer[bufX, bufY] = (array.GetPixel(i, j + 1).grayscale + 1) * heigth;
                        //    }
                        //    if (i > sizeX && j > sizeY)
                        //    {
                        //        buffer[bufX, bufY] = (array.GetPixel(i - 1, j - 1).grayscale + 1 )* heigth;
                        //    }
                        //    else if (i > sizeY && j >= 0)
                        //    {
                        //        buffer[bufX, bufY] =( array.GetPixel(i-1, j).grayscale + 1) * heigth;
                        //    }
                        //    else if (j > sizeX && i >= 0)
                        //    {
                        //        buffer[bufX, bufY] = (array.GetPixel(i, j-1).grayscale + 1) * heigth;
                        //    }
                        //}
                        
                        bufY++;
                    }
                    bufX++;
                }
                inner.Add(buffer);
            }
            returned.Add(inner);
        }
        return returned;
    }
}