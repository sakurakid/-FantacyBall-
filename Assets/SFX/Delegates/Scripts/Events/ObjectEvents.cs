
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DelegetePulg;
[AddComponentMenu("Delegates/Events/ObjectEvents")]
public class ObjectEvents : MonoBehaviour
{
	[HideInInspector]
	public Delegate onStart;
	
	[HideInInspector]
	public Delegate onAwake;
	
	[HideInInspector]
	public Delegate onEnable;
	
	[HideInInspector]
	public Delegate onDisable;
	
	public void OnEnable() 	{onEnable.Exec(this);}
	public void OnDisable() {onDisable.Exec(this);}
	public void Start() 	{onStart.Exec(this);}
	public void Awake() 	{onAwake.Exec(this);}
}