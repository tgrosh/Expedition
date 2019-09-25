using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : IComparable<Room>
{
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public Rect bounds;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room()
    {
    }

    public Room(List<Coord> roomTiles, int[,] map)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (map[x, y] == (int)TileType.Wall)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
        bounds = GetBounds(map);
    }

    Rect GetBounds(int[,] map)
    {
        float minX = -1, minY = -1, maxX = -1, maxY = -1;

        foreach (Coord coord in edgeTiles)
        {
            if (minX == -1 || minX > coord.tileX)
            {
                minX = coord.tileX - map.GetLength(0) / 2f;
            }
            if (minY == -1 || minY > coord.tileY)
            {
                minY = coord.tileY - map.GetLength(1) / 2f;
            }
            if (maxX == -1 || maxX < coord.tileX)
            {
                maxX = coord.tileX - map.GetLength(0) / 2f;
            }
            if (maxY == -1 || maxY < coord.tileY)
            {
                maxY = coord.tileY - map.GetLength(1) / 2f;
            }
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
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

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}
