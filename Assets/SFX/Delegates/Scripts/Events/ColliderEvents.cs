// Delegate Event Framework
// Copyright: Cratesmith (Kieran Lord)
// Created: 2010
//
// No warranty or garuntee whatsoever with this code. 
// 

using UnityEngine;
using System.Collections.Generic;
using DelegetePulg;

[AddComponentMenu("Delegates/Events/ColliderEvents")]
public class ColliderEvents : MonoBehaviour
{
	public string				tagMask;
	public LayerMask 			layerMask = -1;
	
	[HideInInspector]
	public Delegate				firstInEvent;
	
	[HideInInspector]
	public Delegate				lastOutEvent;
	
	[HideInInspector]
	public Delegate				enterEvent;
	
	[HideInInspector]
	public Delegate				exitEvent;
	
	private List<GameObject> 	contains = new List<GameObject>();
	public List<GameObject> 	Contains {get {return contains;} }
	
	private GameObject			lastEventCollider;
	public GameObject 			LastEventCollider { get {return lastEventCollider;} }
	
	public void EnableColliderEvents()
	{
		enabled = true;
	}
	
	public void DisableColliderEvents()
	{
		enabled = false;
	}
	
	public void OnCollisionEnter(Collision col)
	{
		OnTriggerEnter(col.collider);
	}
	
	public void OnCollisionExit(Collision col)
	{
		OnTriggerExit(col.collider);
	}
	
	public void OnTriggerEnter(Collider other)
	{	
		if(!enabled)
			return;
		
		lastEventCollider = other.attachedRigidbody==null ? other.gameObject:other.attachedRigidbody.gameObject;
		
		if(layerMask.value != -1 && ((layerMask.value & (1<<lastEventCollider.layer))==0))
			return;
		
		if(tagMask.Length > 0 && tagMask!=lastEventCollider.tag)
			return;
								
		contains.Add(lastEventCollider);
		contains.RemoveAll(m => m==null);
		
		enterEvent.Exec(this);
		if(contains.Count == 1)
			firstInEvent.Exec(this);
	}
	
	public void OnTriggerExit(Collider other)
	{
		if(!enabled)
			return;
		
		lastEventCollider = other.attachedRigidbody==null ? other.gameObject:other.attachedRigidbody.gameObject;
		
		contains.RemoveAll(m => m==null);
		if(!contains.Remove(lastEventCollider))
			return;
	
		exitEvent.Exec(this);
		if(contains.Count == 0)
			lastOutEvent.Exec(this);
	}
}