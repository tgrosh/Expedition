using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonBuilder : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;

    public int minRoomSize;
    public int maxRoomSize;

    public int minRoomCount;
    public int maxRoomCount;

    public int hallwayWidth;
    public int borderSize = 5;

    public GameObject player;
    public GameObject ground;

    public string seed;
    public bool useRandomSeed;
    System.Random pseudoRandom;

    int[,] map;
    List<Room> rooms = new List<Room>();

    void Start()
    {
        if (useRandomSeed)
        {
            seed = DateTime.Now.Ticks.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMap();
        Populate();
        StartPlayer();
    }

    void StartPlayer()
    {
        foreach (NavMeshSurface surface in ground.GetComponents<NavMeshSurface>())
        {
            surface.BuildNavMesh();
        }

        Rect startingRoomBounds = rooms[pseudoRandom.Next(0, rooms.Count)].bounds;
        GameObject objPlayer = Instantiate(player, new Vector3(startingRoomBounds.center.x, 0, startingRoomBounds.center.y), Quaternion.identity);
    }

    void Populate()
    {
        GetComponent<DungeonPopulator>().Populate(rooms);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            GenerateMap();
            StartPlayer();
        }
    }

    void GenerateMap()
    {
        map = new int[mapWidth, mapHeight];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y] = (int)TileType.Wall;
            }
        }

        rooms = CreateRooms();
        ConnectRooms(rooms);

        int[,] borderedMap = new int[mapWidth + borderSize * 2, mapHeight + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < mapWidth + borderSize && y >= borderSize && y < mapHeight + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = (int)TileType.Wall;
                }
            }
        }

        DungeonMeshGenerator meshGen = GetComponent<DungeonMeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

    List<Room> CreateRooms()
    {
        List<List<Coord>> roomRegions;
        List<Room> rooms = new List<Room>();        

        for (int counter = 0; counter < pseudoRandom.Next(minRoomCount, maxRoomCount); counter++)
        {
            CreateRoom();
        }

        //rooms are created, but many may overlap, creating new room structure
        // the final list of rooms isnt official until all rooms are created and allowed to overlap
        roomRegions = GetRegions((int)TileType.Room);
        foreach (List<Coord> roomRegion in roomRegions)
        {
            rooms.Add(new Room(roomRegion, map));
        }
        rooms.Sort();

        return rooms;
    }

    void CreateRoom()
    {
        int roomWidth = pseudoRandom.Next(minRoomSize, maxRoomSize);
        int roomHeight = pseudoRandom.Next(minRoomSize, maxRoomSize);

        int roomStartX = pseudoRandom.Next(1, map.GetLength(0) - 1 - roomWidth);
        int roomStartY = pseudoRandom.Next(1, map.GetLength(1) - 1 - roomHeight);

        List<Coord> roomCoords = new List<Coord>();
        for (int x = roomStartX; x < roomStartX + roomWidth; x++)
        {
            for (int y = roomStartY; y < roomStartY + roomHeight; y++)
            {
                roomCoords.Add(new Coord(x, y));
                map[x, y] = (int)TileType.Room;
            }
        }
    }

    void ConnectRooms(List<Room> rooms)
    {
        if (rooms.Count > 0)
        {
            rooms[0].isMainRoom = true;
            rooms[0].isAccessibleFromMainRoom = true;

            ConnectClosestRooms(rooms);
        }
    }
    
    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
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
                CreateCorridor(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreateCorridor(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreateCorridor(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Coord> corridor = GetCorridorCoords(tileA, tileB);

        foreach (Coord c in corridor)
        {
            DrawCorridorCoord(c, hallwayWidth);
        }
    }

    void DrawCorridorCoord(Coord c, int size)
    {
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                map[c.tileX + x, c.tileY + y] = (int)TileType.Corridor;
            }
        }
    }

    List<Coord> GetCorridorCoords(Coord from, Coord to)
    {
        List<Coord> corridorCoords = new List<Coord>();
        Coord currentCoord = from;
        int vDirection = 0;
        int hDirection = 0;

        vDirection = GetDirection(from.tileY, to.tileY);
        hDirection = GetDirection(from.tileX, to.tileX);

        if (from.tileY != to.tileY)
        {
            //vertically different
            while (currentCoord.tileY != to.tileY)
            {
                corridorCoords.Add(currentCoord);
                currentCoord = new Coord(currentCoord.tileX, currentCoord.tileY + vDirection);
            }
        }

        if (from.tileX != to.tileX)
        {
            //horizontally different
            while (currentCoord.tileX != to.tileX)
            {
                corridorCoords.Add(currentCoord);
                currentCoord = new Coord(currentCoord.tileX + hDirection, currentCoord.tileY);
            }
        }

        return corridorCoords;
    }

    int GetDirection(int num1, int num2)
    {
        return num2 > num1 ? 1 : -1;
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
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
        int[,] mapFlags = new int[mapWidth, mapHeight];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
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
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }
}
