using UnityEngine;
using UnityEditor;

public class UpdateRootBoneAndBounds
{
    [MenuItem("Window/Infinity PBR/Fix SMR Bounds")]
    static void FixSMRBounds()
    {
        // Fetch an array of all selected GameObjects
        GameObject[] selectedObjects = Selection.gameObjects;

        // If no GameObjects are selected, log an error
        if (selectedObjects.Length == 0) 
        {
            Debug.LogError("No GameObjects selected.");
            return;
        }

        // Process each selected GameObject
        foreach (GameObject selectedObject in selectedObjects)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = FindSkinnedMeshRenderer(selectedObject);

            if (skinnedMeshRenderer == null)
                continue; // Skip this iteration if there's no SkinnedMeshRenderer

            Transform boneRoot = FindBoneRoot(selectedObject);

            Transform closestBone = GetClosestBone(skinnedMeshRenderer);

            if (closestBone != null && skinnedMeshRenderer.rootBone != closestBone)
            {
                skinnedMeshRenderer.rootBone = closestBone;
            }

            Bounds bounds = CalculateBounds(skinnedMeshRenderer);

            skinnedMeshRenderer.localBounds = bounds;
        }
    }


    static GameObject GetSelectedObject()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null) 
        {
            Debug.LogError("No GameObject selected.");
        }
        else
        {
            Debug.Log($"Selected GameObject: {selectedObject.name}");
        }

        return selectedObject;
    }

    static SkinnedMeshRenderer FindSkinnedMeshRenderer(GameObject selectedObject)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = selectedObject?.GetComponentInChildren<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found in the selected GameObject's children.");
        }
        else
        {
            Debug.Log($"Found SkinnedMeshRenderer on {skinnedMeshRenderer.gameObject.name}");
        }

        return skinnedMeshRenderer;
    }

    static Transform FindBoneRoot(GameObject selectedObject)
    {
        Transform boneRoot = selectedObject?.transform.Find("BoneRoot");

        if (boneRoot == null)
        {
            Debug.LogError("No child named 'BoneRoot' found in the selected GameObject's children.");
        }
        else
        {
            Debug.Log($"Found 'BoneRoot' named {boneRoot.name}");
        }

        return boneRoot;
    }

    static Transform GetClosestBone(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        Transform closestBone = null;
        float closestDistance = float.MaxValue;

        foreach (Transform bone in skinnedMeshRenderer.bones)
        {
            float distance = Vector3.Distance(bone.position, skinnedMeshRenderer.gameObject.transform.position);

            if (distance < closestDistance)
            {
                closestBone = bone;
                closestDistance = distance;
            }
        }

        if (closestBone == null)
        {
            Debug.LogError("No bones found on the SkinnedMeshRenderer.");
        }
        else
        {
            Debug.Log($"Closest bone to {skinnedMeshRenderer.gameObject.name} is {closestBone.name} at a distance of {closestDistance}");
        }

        return closestBone;
    }

    static Bounds CalculateBounds(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        Mesh mesh = skinnedMeshRenderer.sharedMesh;

        // Inverse rootBone rotation
        Quaternion inverseRotation = Quaternion.Inverse(skinnedMeshRenderer.rootBone.rotation);

        // Calculate min, max and center vertices
        Vector3 min = inverseRotation * mesh.vertices[0];
        Vector3 max = inverseRotation * mesh.vertices[0];
        Vector3 center = Vector3.zero;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = inverseRotation * mesh.vertices[i];
            center += vertex;
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }

        center /= mesh.vertexCount;

        var localBonePosition = skinnedMeshRenderer.transform.InverseTransformPoint(skinnedMeshRenderer.rootBone.position);
        var rotatedBonePosition = Quaternion.Inverse(skinnedMeshRenderer.rootBone.rotation) * localBonePosition;
        var meshPosition = Vector3.zero; // Mesh local position is (0,0,0)
    
        Debug.Log($"rotatedBonePosition (local space) is {rotatedBonePosition.ToString("F6")}");
        Debug.Log($"meshPosition (local space) is {meshPosition.ToString("F6")}");
    
        Vector3 delta = meshPosition - rotatedBonePosition;
        Debug.Log($"Delta is {delta.ToString("F6")}");

        center += delta;

        Vector3 extents = max - min;
    
        Bounds bounds = new Bounds(center, extents);

        Debug.Log("Rotated bounds: Center: " + bounds.center.ToString("F6") + ", Extents: " + bounds.extents.ToString("F6"));

        return bounds;
    }



    
    

}
