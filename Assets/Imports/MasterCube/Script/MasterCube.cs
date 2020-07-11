using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterCube : MonoBehaviour 
{
    public Mesh mesh;
    public Face front, back, right, left, top, bottom;
    public Vector2 offset = Vector2.zero, scale = Vector2.one;
    public float rotation;
	[HideInInspector] public Vector3 oldScale;

     [System.Serializable]
    public class Face
    {
        public FaceSide side;
        public Vector2 offset;
        public Vector2 scale;
        public float rotation;
        public List<UV> uvs;
        public Face(FaceSide side)
        {
            this.side = side;
            this.scale = Vector2.one;
            this.uvs = new List<UV>();
            this.rotation = 0;
        }
    }

     public enum FaceSide
     {
         Front,Back,Left,Right,Top,Bottom
     }
}

[System.Serializable]
public class UV
{
    public int index;
    public Vector2 position;
    public UV(int index, Vector2[] position)
    {
        this.index = index;
        this.position = position[index];
    }        
}