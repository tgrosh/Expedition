using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularDungeon : MonoBehaviour
{
    public DungeonModule startingRoom;
    public List<DungeonModule> rooms = new List<DungeonModule>();
    public List<DungeonModule> halls = new List<DungeonModule>();

    public string seed;
    public bool useRandomSeed;

    [Range(0, 1)]
    public float roomChance;
    public int overlapRetries;

    System.Random pseudoRandom;
    bool generationComplete;

    int currentOverlapRetries = 0;

    List<DungeonModule> currentModules = new List<DungeonModule>();

    public void Start()
    {
        if (useRandomSeed)
        {
            seed = DateTime.Now.Ticks.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateDungeon();        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            GenerateDungeon();
        }
    }

    void GenerateDungeon()
    {
        ClearDungeon();
        DungeonModule startModule = Instantiate(startingRoom, Vector3.zero, Quaternion.identity, gameObject.transform);
        currentModules.Add(startModule);
        PopulateExits(startModule);
        Debug.Log("Dungeon Complete");
    }

    void ClearDungeon()
    {
        foreach (DungeonModule module in GetComponentsInChildren<DungeonModule>())
        {
            Destroy(module.gameObject);
        }
        currentModules.Clear();
        currentOverlapRetries = 0;
    }

    void PopulateExits(DungeonModule module)
    {
        List<DungeonModule> modules;

        for (int i = 0; i < module.exits.Count; i++)
        {
            Exit exit = module.exits[i];

            if (exit.available)
            {
                modules = (roomChance > pseudoRandom.Next(0, 100) / 100f) ? rooms : halls;

                if (!PlaceRandomModule(modules, exit))
                {
                    if (currentOverlapRetries < overlapRetries)
                    {
                        i--; //continue with this exit until it gets populated
                        currentOverlapRetries++;
                    } else
                    {
                        currentOverlapRetries = 0;
                        Debug.LogWarning("Too many retries.. Invalid dungeon");
                    }
                }
            }
        }
    }

    bool PlaceRandomModule(List<DungeonModule> modules, Exit target)
    {
        int randomModule = pseudoRandom.Next(0, modules.Count);
        DungeonModule module = Instantiate(modules[randomModule]);
        module.gameObject.name += "-" + (currentModules.Count+1);
        Exit exit = GetRandomAvailableExit(module);

        //reset position and rotation
        module.transform.position = Vector3.zero;
        module.transform.rotation = Quaternion.identity;

        //set room rotation
        Vector3 targetRotation = target.transform.eulerAngles;
        Vector3 exitRotation = exit.transform.eulerAngles;
        float angle = Mathf.DeltaAngle(exitRotation.y, targetRotation.y);
        Quaternion nextHallRotation = Quaternion.AngleAxis(angle, Vector3.up);
        module.transform.rotation = nextHallRotation * Quaternion.Euler(0, 180f, 0);

        //set room position
        Vector3 nextHallPositionOffset = exit.transform.position - module.transform.position;
        module.transform.position = target.transform.position - nextHallPositionOffset;

        module.transform.SetParent(gameObject.transform);
        
        if (!CheckOverlap(module))
        {
            target.available = false;
            exit.available = false;
            currentModules.Add(module);
            PopulateExits(module);
            return true;
        } else
        {
            Destroy(module.gameObject);
            return false;
        }
    }

    Exit GetRandomAvailableExit(DungeonModule module)
    {
        if (!module.exits.Exists((x => x.available == true))) { return null; }

        Exit exit = null;

        while (exit == null || exit.available == false) {
            exit = module.exits[pseudoRandom.Next(0, module.exits.Count)];
        }

        return exit;
    }

    bool CheckOverlap(DungeonModule module)
    {
        Bounds moduleBounds = module.bounds;
        moduleBounds.Expand(-.1f);

        foreach(DungeonModule dungeonModule in currentModules)
        {
            if (dungeonModule != module && dungeonModule.bounds.Intersects(moduleBounds))
            {
                //Debug.Log("Overlap " + module.name + " and " + dungeonModule.name);
                return true;
            }
        }
        return false;
    }
    
    private void MyHallPlacementLogic()
    {
        //nextHall.transform.position = target.position + exit.transform.localPosition;
        //nextHall = Instantiate(halls[randomHall]);
        //exits = nextHall.GetComponentsInChildren<Exit>();
        //exit = exits[pseudoRandom.Next(0, exits.Length)];
        //nextHall.transform.position = target.position + exit.transform.localPosition;

        //float angle = exit.transform.localRotation.eulerAngles.y + target.localRotation.eulerAngles.y;
        //nextHall.transform.RotateAround(exit.transform.position, Vector3.up, angle);
    }
}
