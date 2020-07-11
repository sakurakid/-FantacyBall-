// Delegate Event Framework
// Copyright: Cratesmith (Kieran Lord)
// Created: 2010
//
// No warranty or garuntee whatsoever with this code. 
// 

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DelegetePulg;
[AddComponentMenu("Delegates/Events/MouseEvents")]
public class MouseEvents : MonoBehaviour
{
	[HideInInspector]
	public Delegate onMouseEnter;
	
	[HideInInspector]
	public Delegate onMouseOver;
	
	[HideInInspector]
	public Delegate onMouseExit;
	
	[HideInInspector]
	public Delegate onMouseDown;
	
	[HideInInspector]
	public Delegate onMouseUp;
	
	[HideInInspector]
	public Delegate onMouseDrag;
	
	public void OnMouseEnter()
	{
		onMouseEnter.Exec(this);
	}
	
	public void OnMouseOver()
	{
		onMouseOver.Exec(this);
	}
	
	public void OnMouseExit()
	{
		onMouseExit.Exec(this);
	}
	
	public void OnMouseDown()
	{
		onMouseDown.Exec(this);
	}
	
	public void OnMouseUp()
	{
		onMouseUp.Exec(this);
	}
	
	public void OnMouseDrag()
	{
		onMouseDrag.Exec(this);
	}
}
