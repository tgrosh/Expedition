using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class DungeonModule : MonoBehaviour
{
    public List<Exit> exits = new List<Exit>();
    public MeshRenderer meshRenderer;
    public bool showBounds = false;

    private void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = new Color(0f, .5f, 1f, .25f);
            Bounds cubeBounds = bounds;
            Gizmos.DrawCube(bounds.center, cubeBounds.size);
        }
    }

    public Bounds bounds
    {
        get
        {
            if (meshRenderer == null) return new Bounds(Vector3.zero, Vector3.zero);

            Bounds cubeBounds = meshRenderer.bounds;
            float expandAmount = -.05f;
            cubeBounds.Expand(new Vector3(cubeBounds.size.x * expandAmount, cubeBounds.size.y * expandAmount, cubeBounds.size.z * expandAmount));
            return cubeBounds;
        }
    }
}
