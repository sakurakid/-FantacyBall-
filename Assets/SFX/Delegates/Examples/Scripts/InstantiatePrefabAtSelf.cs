using UnityEngine;

public class InstantiatePrefabAtSelf : MonoBehaviour
{
	public void DoInstantiatePrefab(GameObject prefab)
	{
		GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
	}
}