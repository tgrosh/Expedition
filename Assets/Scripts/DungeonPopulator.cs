using System.Collections.Generic;
using UnityEngine;

public class DungeonPopulator : MonoBehaviour
{
    public GameObject enemyPrefab;

    public void Populate(List<Room> rooms)
    {
        List<Room> availableRooms = rooms.FindAll(room => !room.isMainRoom);
        Room enemyRoom = availableRooms[new System.Random().Next(0, availableRooms.Count - 1)];

        GameObject objEnemy = Instantiate(enemyPrefab, new Vector3(enemyRoom.bounds.center.x, 0, enemyRoom.bounds.center.y), Quaternion.identity);
    }

}
