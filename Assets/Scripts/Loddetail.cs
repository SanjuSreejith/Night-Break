/*using UnityEngine;

public class AutoLOD : MonoBehaviour
{
    public float LOD1Distance = 20f;  // Distance to switch to LOD1
    public float LOD2Distance = 40f;  // Distance to switch to LOD2
    public float cullingDistance = 60f; // Distance at which the object is disabled

    private LODGroup lodGroup;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    void Start()
    {
        lodGroup = gameObject.AddComponent<LODGroup>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        if (meshRenderer == null || meshFilter == null)
        {
            Debug.LogError("AutoLOD: No MeshRenderer or MeshFilter found!");
            return;
        }

        // Create LOD levels
        LOD[] lods = new LOD[3];

        // LOD 0 (Full Detail)
        lods[0] = new LOD(1f, new Renderer[] { meshRenderer });

        // LOD 1 (Lower Detail)
        GameObject lod1 = InstantiateLOD("LOD1", 0.5f);
        lods[1] = new LOD(0.5f, new Renderer[] { lod1.GetComponent<MeshRenderer>() });

        // LOD 2 (Lowest Detail)
        GameObject lod2 = InstantiateLOD("LOD2", 0.2f);
        lods[2] = new LOD(0.2f, new Renderer[] { lod2.GetComponent<MeshRenderer>() });

        // Apply LODs to the LODGroup
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }

    GameObject InstantiateLOD(string name, float scaleFactor)
    {
        GameObject lodObject = new GameObject(name);
        lodObject.transform.SetParent(transform);
        lodObject.transform.localPosition = Vector3.zero;
        lodObject.transform.localRotation = Quaternion.identity;
        lodObject.transform.localScale = Vector3.one * scaleFactor;

        MeshFilter mf = lodObject.AddComponent<MeshFilter>();
        mf.mesh = meshFilter.mesh; // Assign the same mesh for now

        MeshRenderer mr = lodObject.AddComponent<MeshRenderer>();
        mr.material = meshRenderer.material; // Assign the same material

        return lodObject;
    }
}
*/ 