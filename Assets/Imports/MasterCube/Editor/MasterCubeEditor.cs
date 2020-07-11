using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(MasterCube))]
public class MasterCubeEditor : Editor
{
    Vector3 oldScale;
    MasterCube cCube;
    bool customTransforme;   

    void OnScene(SceneView view)
    {
        if (Application.isPlaying) return;
        
        foreach (Object target in targets)
        {
            cCube = (MasterCube)target;
            if (cCube == null) continue;
            try
            {                
                if (cCube.transform.localScale == cCube.oldScale && cCube.GetComponent<MeshFilter>().sharedMesh!=null) return;
                else cCube.oldScale = cCube.transform.localScale;
                meshTranform(cCube.front);
                meshTranform(cCube.back);
                meshTranform(cCube.right);
                meshTranform(cCube.left);
                meshTranform(cCube.top);
                meshTranform(cCube.bottom);                
            }
            catch { }
        }
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
        SceneView.onSceneGUIDelegate += OnScene;
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying) return;
        foreach (Object target in targets)
        {
            cCube = (MasterCube)target;
            if (cCube.mesh == null)
                CreateCustomMesh();

           // cCube.mesh = (Mesh)EditorGUILayout.ObjectField("Base Mesh", cCube.mesh, typeof(Mesh), false);
			if(cCube.mesh==null)
			{
				EditorGUILayout.HelpBox("Can't find the Master Cube Mesh",MessageType.Error);
			}

            cCube.offset = EditorGUILayout.Vector2Field("UV Off Set", cCube.offset);
            cCube.scale = EditorGUILayout.Vector2Field("UV Scale", cCube.scale);
			cCube.scale.x = Mathf.Clamp(cCube.scale.x, -float.MaxValue, float.MaxValue);
			cCube.scale.y = Mathf.Clamp(cCube.scale.y, -float.MaxValue, float.MaxValue);
            cCube.rotation = EditorGUILayout.Slider("UV Rotation", cCube.rotation, -180, 180);
            customTransforme = EditorGUILayout.Foldout(customTransforme, "Face transform");
            if (customTransforme)
            {
                cCube.front = SideParameter("Front", cCube.front);
                cCube.back = SideParameter("Back", cCube.back);
                cCube.right = SideParameter("Right", cCube.right);
                cCube.left = SideParameter("Left", cCube.left);
                cCube.top = SideParameter("Top", cCube.top);
                cCube.bottom = SideParameter("Bottom", cCube.bottom);
            }
            if (GUI.changed)
            {
                meshTranform(cCube.front);
                meshTranform(cCube.back);
                meshTranform(cCube.right);
                meshTranform(cCube.left);
                meshTranform(cCube.top);
                meshTranform(cCube.bottom);

				EditorUtility.SetDirty(cCube);
            }
            if (GUILayout.Button("Refresh Base UV"))
                RefreshBaseUV();
        }       
    }
    
    void meshTranform(MasterCube.Face faceSide)
    {        
        var scaleFactor = Vector2.one;
        switch (faceSide.side)
        {
            case MasterCube.FaceSide.Front:
                scaleFactor.x = cCube.transform.localScale.x;
                scaleFactor.y = cCube.transform.localScale.y;
                break;
            case MasterCube.FaceSide.Back:
                scaleFactor.x = cCube.transform.localScale.x;
                scaleFactor.y = cCube.transform.localScale.y;
                break;
            case MasterCube.FaceSide.Left:
                scaleFactor.x = cCube.transform.localScale.z;
                scaleFactor.y = cCube.transform.localScale.y;
                break;
            case MasterCube.FaceSide.Right:
                scaleFactor.x = cCube.transform.localScale.z;
                scaleFactor.y = cCube.transform.localScale.y;
                break;
            case MasterCube.FaceSide.Top:
                scaleFactor.x = cCube.transform.localScale.x;
                scaleFactor.y = cCube.transform.localScale.z;
                break;
            case MasterCube.FaceSide.Bottom:
                scaleFactor.x = cCube.transform.localScale.x;
                scaleFactor.y = cCube.transform.localScale.z;
                break;
        }

        if (cCube.GetComponent<MeshFilter>().sharedMesh == null)
        {
            cCube.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(cCube.mesh);
        }

        Vector2[] uvs = cCube.GetComponent<MeshFilter>().sharedMesh.uv;
        foreach (UV uv in faceSide.uvs)
        {
            if (cCube.mesh == null) break;
            var rot = Quaternion.Euler(0, 0, cCube.rotation + faceSide.rotation);

            var newuv = new Vector2(((cCube.mesh.uv[uv.index].x + (faceSide.offset.x + cCube.offset.x)) * scaleFactor.x) / (faceSide.scale.x * cCube.scale.x),
                                    ((cCube.mesh.uv[uv.index].y + (faceSide.offset.y + cCube.offset.y)) * scaleFactor.y) / (faceSide.scale.y * cCube.scale.y));

            newuv = rot*newuv;
            uvs[uv.index] = newuv;          
        }
        if (cCube.mesh != null)
        {
            cCube.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(cCube.mesh);
            cCube.GetComponent<MeshFilter>().sharedMesh.uv = uvs;
        }
    }
    
    MasterCube.Face SideParameter(string name, MasterCube.Face faceSide)
    {       
        GUILayout.BeginVertical(name, "window");
        faceSide.offset = EditorGUILayout.Vector2Field("UV off Set", faceSide.offset);
        faceSide.scale = EditorGUILayout.Vector2Field("UV scale", faceSide.scale);
		faceSide.scale.x = Mathf.Clamp(faceSide.scale.x,-float.MaxValue, float.MaxValue);
		faceSide.scale.y = Mathf.Clamp(faceSide.scale.y, -float.MaxValue, float.MaxValue);
        faceSide.rotation = EditorGUILayout.Slider("UV rotation", faceSide.rotation, -180, 180);
        GUILayout.EndVertical();
        EditorGUILayout.Space();       
       
        return faceSide;
    }
   
    void CreateCustomMesh()
    {
        //  GameObject cube = new GameObject("New Cube", typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider), typeof(CustomCube));
        if (cCube.gameObject.GetComponent<MeshFilter>() == null) cCube.gameObject.AddComponent<MeshFilter>();
        if (cCube.gameObject.GetComponent<MeshRenderer>() == null) cCube.gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.Clear();

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh = (Mesh)Instantiate(cube.GetComponent<MeshFilter>().sharedMesh);

        Mesh _mesh = new Mesh();
		if (!System.IO.Directory.Exists(Application.dataPath + "/MasterCube/MasterCubeMesh"))
			System.IO.Directory.CreateDirectory(Application.dataPath + "/MasterCube/MasterCubeMesh");
		_mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/CustomCube/MasterCubeMesh/masterCube.asset", typeof(Mesh));
        if (_mesh == null)
        {

			AssetDatabase.CreateAsset(mesh, "Assets/MasterCube/MasterCubeMesh/masterCube.asset");
			_mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/MasterCube/MasterCubeMesh/masterCube.asset", typeof(Mesh));
        }

        if (_mesh == null)
        {
            Debug.Log("Can't find or create a master cube mesh");
            return;
        }
        cCube.mesh = _mesh;
        cCube.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(_mesh);
        if (cCube.GetComponent<Renderer>().sharedMaterial == null)
            cCube.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        DestroyImmediate(cube);
    }

	void RefreshBaseUV()
	{
		foreach (Object target in targets)
		{
			cCube = (MasterCube)target;
			if (cCube.mesh == null)
				CreateCustomMesh();
			var uvs = cCube.mesh.uv;
			
			cCube.bottom = new MasterCube.Face(MasterCube.FaceSide.Bottom);
			// BOTTOM   15   13   12   14
			cCube.bottom.uvs.Add(new UV(15, uvs));
			cCube.bottom.uvs.Add(new UV(13, uvs));
			cCube.bottom.uvs.Add(new UV(12, uvs));
			cCube.bottom.uvs.Add(new UV(14, uvs));
			
			cCube.left = new MasterCube.Face(MasterCube.FaceSide.Left);
			// LEFT   19   17   16   18
			cCube.left.uvs.Add(new UV(19, uvs));
			cCube.left.uvs.Add(new UV(17, uvs));
			cCube.left.uvs.Add(new UV(16, uvs));
			cCube.left.uvs.Add(new UV(18, uvs));
			
			cCube.front = new MasterCube.Face(MasterCube.FaceSide.Front);
			// FRONT    2    3    0    1
			cCube.front.uvs.Add(new UV(2, uvs));
			cCube.front.uvs.Add(new UV(3, uvs));
			cCube.front.uvs.Add(new UV(0, uvs));
			cCube.front.uvs.Add(new UV(1, uvs));
			
			
			cCube.back = new MasterCube.Face(MasterCube.FaceSide.Back);
			// BACK    6    7   10   11
			cCube.back.uvs.Add(new UV(6, uvs));
			cCube.back.uvs.Add(new UV(7, uvs));
			cCube.back.uvs.Add(new UV(10, uvs));
			cCube.back.uvs.Add(new UV(11, uvs));
			
			
			cCube.right = new MasterCube.Face(MasterCube.FaceSide.Right);
			// RIGHT   23   21   20   22
			cCube.right.uvs.Add(new UV(23, uvs));
			cCube.right.uvs.Add(new UV(21, uvs));
			cCube.right.uvs.Add(new UV(20, uvs));
			cCube.right.uvs.Add(new UV(22, uvs));
			
			cCube.top = new MasterCube.Face(MasterCube.FaceSide.Top);
			// TOP    4    5    8    9
			cCube.top.uvs.Add(new UV(4, uvs));
			cCube.top.uvs.Add(new UV(5, uvs));
			cCube.top.uvs.Add(new UV(8, uvs));
			cCube.top.uvs.Add(new UV(9, uvs));
		}
	}


	[MenuItem("GameObject/3D Object/Master Cube")]
	static void CreateMasterCube()
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.name = "MasterCube";
		SceneView view =  SceneView.lastActiveSceneView;
		if (SceneView.lastActiveSceneView == null)
			throw new UnityException("The Scene View can't be access");

		Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
		cube.transform.position = spawnPos;
        mesh = (Mesh)Instantiate(cube.GetComponent<MeshFilter>().sharedMesh);
        MasterCube cCube = cube.AddComponent<MasterCube>();

        Mesh _mesh = new Mesh();
        if (!System.IO.Directory.Exists(Application.dataPath + "/MasterCube/MasterCubeMesh"))
			System.IO.Directory.CreateDirectory(Application.dataPath + "/MasterCube/MasterCubeMesh");
		_mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/MasterCube/MasterCubeMesh/masterCube.asset", typeof(Mesh));
        if (_mesh == null)
        {

			AssetDatabase.CreateAsset(mesh, "Assets/MasterCube/MasterCubeMesh/masterCube.asset");
			_mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/MasterCube/MasterCubeMesh/masterCube.asset", typeof(Mesh));
        }

        if (_mesh == null)
        {
            Debug.Log("Can't find or create a master cube mesh");
            return;
        }
        cCube.mesh = _mesh;
        cCube.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(_mesh);
        if (cCube.GetComponent<Renderer>().sharedMaterial == null)
            cCube.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));

        #region UVs

        var uvs = cCube.mesh.uv; 
        cCube.bottom = new MasterCube.Face(MasterCube.FaceSide.Bottom);
        // BOTTOM   15   13   12   14
        cCube.bottom.uvs.Add(new UV(15, uvs));
        cCube.bottom.uvs.Add(new UV(13, uvs));
        cCube.bottom.uvs.Add(new UV(12, uvs));
        cCube.bottom.uvs.Add(new UV(14, uvs));

        cCube.left = new MasterCube.Face(MasterCube.FaceSide.Left);
        // LEFT   19   17   16   18
        cCube.left.uvs.Add(new UV(19, uvs));
        cCube.left.uvs.Add(new UV(17, uvs));
        cCube.left.uvs.Add(new UV(16, uvs));
        cCube.left.uvs.Add(new UV(18, uvs));
       
        cCube.front = new MasterCube.Face(MasterCube.FaceSide.Front);
        // FRONT    2    3    0    1
        cCube.front.uvs.Add(new UV(2, uvs));
        cCube.front.uvs.Add(new UV(3, uvs));
        cCube.front.uvs.Add(new UV(0, uvs));
        cCube.front.uvs.Add(new UV(1, uvs));
       

        cCube.back = new MasterCube.Face(MasterCube.FaceSide.Back);
        // BACK    6    7   10   11
        cCube.back.uvs.Add(new UV(6, uvs));
        cCube.back.uvs.Add(new UV(7, uvs));
        cCube.back.uvs.Add(new UV(10, uvs));
        cCube.back.uvs.Add(new UV(11, uvs));
      
        
        cCube.right = new MasterCube.Face(MasterCube.FaceSide.Right);
        // RIGHT   23   21   20   22
        cCube.right.uvs.Add(new UV(23, uvs));
        cCube.right.uvs.Add(new UV(21, uvs));
        cCube.right.uvs.Add(new UV(20, uvs));
        cCube.right.uvs.Add(new UV(22, uvs));
       
        cCube.top = new MasterCube.Face(MasterCube.FaceSide.Top);
        // TOP    4    5    8    9
        cCube.top.uvs.Add(new UV(4, uvs));
        cCube.top.uvs.Add(new UV(5, uvs));
        cCube.top.uvs.Add(new UV(8, uvs));
        cCube.top.uvs.Add(new UV(9, uvs));
        
        #endregion
        Selection.activeGameObject = cCube.gameObject;
    }    
}
