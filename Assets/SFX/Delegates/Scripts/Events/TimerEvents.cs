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

[AddComponentMenu("Delegates/Events/EventTimer")]
public class TimerEvents : MonoBehaviour
{
	public float			time=1.0f;
	public bool				startOnLoad=false;
	
	[HideInInspector]
	public Delegate			onTimerStart;
	
	[HideInInspector]
	public Delegate			onTimerComplete;
	
	public void Start()
	{
		if(startOnLoad)
			StartTimer();
	}
	
	public void StartTimer()
	{
		StartTimerWithDuration(time);
	}
	
	public void StartTimerWithDuration(float time)
	{		
		StopAllCoroutines();
		onTimerStart.Exec(this, true);
		
		StartCoroutine(TimerCoroutine(time));
	}
	
	public void StopTimer()
	{
		StopAllCoroutines();
	}
	
	IEnumerator TimerCoroutine(float time)
	{
		yield return new WaitForSeconds(time);
		onTimerComplete.Exec(this, true);
	}
}
