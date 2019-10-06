using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ModularDungeon : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public DungeonModule startingRoom;
    public List<DungeonModule> rooms = new List<DungeonModule>();
    public List<DungeonModule> halls = new List<DungeonModule>();
    public List<DungeonModule> enders = new List<DungeonModule>();
    public int moduleLimit;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 1)]
    public float roomChance;
    public int overlapRetries;
    public int dungeonRetries;

    System.Random pseudoRandom;
    bool generationComplete;
    bool invalidDungeon;

    int currentOverlapRetries = 0;
    int currentDungeonRetries = 0;

    GameObject player;
    GameObject enemy;

    int branchModuleLimit;

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

    IEnumerator BuildNavMesh()
    {
        yield return new WaitForEndOfFrame();
        foreach (NavMeshSurface surface in GetComponents<NavMeshSurface>())
        {
            Debug.Log("Building navmesh");
            surface.BuildNavMesh();
        }
    }

    void GenerateDungeon()
    {
        invalidDungeon = false;
        ClearDungeon();
        DungeonModule startModule = Instantiate(startingRoom, Vector3.zero, Quaternion.identity, gameObject.transform);
        currentModules.Add(startModule);
        branchModuleLimit = moduleLimit / startModule.exits.Count;
        PopulateExits(startModule, false);

        if (invalidDungeon)
        {
            if (currentDungeonRetries < dungeonRetries)
            {
                Debug.Log("Retrying dungeon");
                currentDungeonRetries++;
                GenerateDungeon();
            }
            else
            {
                Debug.Log("Dungeon Generation Failed. Too many retries");
            }
            return;
        }

        generationComplete = true;
        Debug.Log("Dungeon Complete after " + (currentDungeonRetries + 1) + " attempt(s)");
        currentDungeonRetries = 0;

        StartCoroutine("BuildNavMesh");
        player = Instantiate(playerPrefab, startingRoom.transform.position, Quaternion.identity);
        DungeonModule enemyModule = GetRandomModule(currentModules);
        enemy = Instantiate(enemyPrefab, enemyModule.transform.position, Quaternion.identity);
    }

    void ClearDungeon()
    {
        Destroy(player);
        Destroy(enemy);
        foreach (DungeonModule module in GetComponentsInChildren<DungeonModule>())
        {
            Destroy(module.gameObject);
        }
        currentModules.Clear();
        currentOverlapRetries = 0;
        generationComplete = false;
    }

    void PopulateExits(DungeonModule module, bool endersOnly)
    {
        List<DungeonModule> modules;

        for (int i = 0; i < module.exits.Count; i++)
        {
            Exit exit = module.exits[i];

            if (exit.available)
            {
                modules = (roomChance > pseudoRandom.Next(0, 100) / 100f) ? rooms : halls;

                if (endersOnly)
                {
                    modules = enders;
                }

                if (!PlaceRandomModule(modules, exit))
                {
                    if (currentOverlapRetries < overlapRetries)
                    {
                        i--; //continue with this exit until it gets populated
                        currentOverlapRetries++;
                    }
                    else
                    {
                        currentOverlapRetries = 0;
                        invalidDungeon = true;
                        Debug.LogWarning("Too many retries.. Invalid dungeon");
                    }
                }
            }
        }
    }

    DungeonModule GetRandomModule(List<DungeonModule> modules)
    {
        int randomModule = pseudoRandom.Next(0, modules.Count);
        return modules[randomModule];
    }

    bool PlaceRandomModule(List<DungeonModule> modules, Exit target)
    {
        DungeonModule module = Instantiate(GetRandomModule(modules));
        module.gameObject.name += "-" + (currentModules.Count);
        Exit exit = GetRandomAvailableExit(module);

        if (exit == null)
        {
            Debug.LogWarning("No exit found for module " + module.gameObject.name);
        }

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
            PopulateExits(module, currentModules.Count >= branchModuleLimit);
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

        foreach(DungeonModule dungeonModule in currentModules)
        {
            if (dungeonModule != module && dungeonModule.bounds.Intersects(moduleBounds))
            {
                Debug.Log("Overlap " + module.name + " and " + dungeonModule.name);
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
