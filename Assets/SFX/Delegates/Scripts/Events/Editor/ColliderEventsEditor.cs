// Delegate Event Framework
// Copyright: Cratesmith (Kieran Lord)
// Created: 2010
//
// No warranty or garuntee whatsoever with this code. 
// 

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(ColliderEvents))]
public class ColliderEventsEditor : DelegateEditor 
{
	private bool showDoc = false;
	private bool showAdv = false;

	public override void OnInspectorGUI()
	{
		OnInspectorGUI_Documentation();
		OnInspectorGUI_Settings();		
		OnInspectorGUI_Advanced ();	
	}
		
	public void OnInspectorGUI_Settings()
	{
		ColliderEvents 	trigger = (ColliderEvents)target;
		trigger.tagMask = EditorGUILayout.TagField("Activator Tag", trigger.tagMask);
		if(trigger.tagMask == "Untagged")
			trigger.tagMask = "";
		
		DrawEventInspector();
	}

	private void OnInspectorGUI_Documentation ()
	{
		showDoc = EditorGUILayout.Foldout(showDoc, "Documentation");
		if(showDoc)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			
			DocumentationBox("Description", 	"An EventTriggerVolume is used to call script events when an object enters or leaves a trigger collider.");
			DocumentationBox("Activator Tag", 	"If set to Untagged (or blank), any object will fire the trigger. If a tag is set, then only objects with that tag will fire the trigger.");
			DocumentationBox("Enter Event", 	"The event fired each time an object enters the trigger.");
			DocumentationBox("Exit Event", 		"The event fired each time an object enters the trigger.");
			DocumentationBox("First In Event", 	"The event fired when an object enters the trigger, and the trigger previously was empty.");
			DocumentationBox("Last Out Event", 	"The event fired when the tigger goes from having objects inside to being empty.");
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
	}

	public void OnInspectorGUI_Advanced ()
	{
		showAdv = EditorGUILayout.Foldout(showAdv, "Advanced");
		if(showAdv)
			DrawDefaultInspector();
	}

	private static void DocumentationBox (string label, string boxText)
	{
		EditorGUILayout.BeginHorizontal();
		
		EditorGUILayout.PrefixLabel(label, "box");
		GUILayout.Box(boxText, "box");
		
		EditorGUILayout.EndHorizontal();
	}
}
