using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CombineMeshEditor : EditorWindow 
{
    [MenuItem("GameObject/Combine Meshs", false,0)]
    public static void Combine()
    {
        var transform = Selection.activeGameObject.transform;
        var meshFilters = transform.GetComponentsInChildren<MeshFilter>();
        Debug.Log(meshFilters.Length);
        if (meshFilters.Length <2)
        {
            if(meshFilters.Length ==0)
                EditorUtility.DisplayDialog("Combine Mesh", "The " + transform.name + " Don't have a Mesh Filter\nPlease put the gameobjects child in " + transform.name + " to combine all", "OK");
            if (meshFilters.Length > 0)
                EditorUtility.DisplayDialog("Combine Mesh", "The " + transform.name + " have just one Mesh Filter\nPlease put the gameobjects child in "+transform.name+" to combine all", "OK");
                return;
        }
        if (!EditorUtility.DisplayDialog("Combine Mesh", "Are you sure if want to combine meshs?","YES", "NO")) return;

        var position = transform.position;
        var rotation = transform.rotation;
        var scale = transform.localScale;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        Dictionary<int, List<CombineInstance>> combinedMeshs = new Dictionary<int, List<CombineInstance>>();
        List<Material> combinedMaterials = new List<Material>();
        List<string> names = new List<string>();
       
        foreach (MeshFilter msF in meshFilters)
        {
            int indexMaterial = 0;

            if (!names.Contains(msF.GetComponent<MeshRenderer>().sharedMaterial.name))
            {
                names.Add(msF.GetComponent<Renderer>().sharedMaterial.name);
                combinedMaterials.Add(msF.GetComponent<Renderer>().sharedMaterial);

                indexMaterial = names.IndexOf(msF.GetComponent<Renderer>().sharedMaterial.name);
                combinedMeshs.Add(indexMaterial, new List<CombineInstance>());
            }

            Debug.Log(indexMaterial);
            indexMaterial = names.IndexOf(msF.GetComponent<Renderer>().sharedMaterial.name);

            CombineInstance msCombine = new CombineInstance();
            msCombine.transform = msF.transform.localToWorldMatrix;
            msCombine.subMeshIndex = 0;
            msCombine.mesh = msF.sharedMesh;

            combinedMeshs[indexMaterial].Add(msCombine);
        }

        List<CombineInstance> combineInstance = new List<CombineInstance>();
        foreach (int index in combinedMeshs.Keys)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.CombineMeshes(combinedMeshs[index].ToArray(), true, true);
            CombineInstance msCombine = new CombineInstance();

            msCombine.mesh = mesh;
            msCombine.subMeshIndex = 0;

            Debug.Log(index);
            combineInstance.Add(msCombine);
        }

        if (transform.GetComponent<MeshFilter>() == null)
            transform.gameObject.AddComponent<MeshFilter>();
        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();

        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combineInstance.ToArray(), false, false);
        var o_78_8_636677155633336930 = transform.GetComponent<MeshFilter>().sharedMesh;
        transform.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
        MeshRenderer meshRendererCombine = transform.GetComponent<MeshRenderer>();
        if (!meshRendererCombine)
            meshRendererCombine = transform.gameObject.AddComponent<MeshRenderer>();

        meshRendererCombine.materials = combinedMaterials.ToArray();
        transform.position = position;       
        transform.rotation = rotation;
        transform.localScale = scale;
        if (EditorUtility.DisplayDialog("Combine Mesh", "Destroy old meshs?", "YES", "NO"))
        {
            foreach (MeshFilter msF in meshFilters)
            {
                if(msF.gameObject!= Selection.activeGameObject)
                DestroyImmediate(msF.gameObject);
            }
        }
		string filePath = Application.dataPath + "/MasterCube/CombinedMeshs";
        if (!System.IO.Directory.Exists(filePath))
            System.IO.Directory.CreateDirectory(filePath);

        var info = new System.IO.DirectoryInfo(filePath);
        var fileInfo = info.GetFiles();
        int nameCount = 0;
        foreach (FileInfo file in fileInfo)
        {
            if (file.Name.Contains(transform.name))
                nameCount++;
        }
        Mesh _mesh = new Mesh();

		string path = "Assets/MasterCube/CombinedMeshs/" + transform.name + (nameCount + 1).ToString("00") + ".asset";
        AssetDatabase.CreateAsset(transform.GetComponent<MeshFilter>().sharedMesh, path);
        _mesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
        transform.GetComponent<MeshFilter>().sharedMesh = _mesh;
        Selection.activeObject = transform.gameObject;
    }

    // [MenuItem("GameObject/Combine Meshs", true, 0)]
    //private static bool NewMenuOptionValidation()
    //{
    //   var msf = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();

    //   return msf.Length > 0;
    //}
}
