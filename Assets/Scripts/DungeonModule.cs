using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class DungeonModule : MonoBehaviour
{
    public List<Exit> exits = new List<Exit>();
    public MeshRenderer meshRenderer;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, .5f, 1f, .25f);
        Bounds cubeBounds = bounds;
        cubeBounds.Expand(-.1f);
        Gizmos.DrawCube(bounds.center, cubeBounds.size);
    }

    public Bounds bounds
    {
        get
        {
            return meshRenderer.bounds;
        }
    }
}
