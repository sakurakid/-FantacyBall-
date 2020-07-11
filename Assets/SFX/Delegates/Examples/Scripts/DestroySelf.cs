using UnityEngine;
using System.Collections;

public class DestroySelf : MonoBehaviour 
{
	public void DoDestroy() 				{GameObject.Destroy(gameObject);}
	public void DoDestroyAtTime(float time) {GameObject.Destroy(gameObject, time);}
}
