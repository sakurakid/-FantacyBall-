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
using DelegetePulg;

public abstract class DelegateEditor : Editor
{
	private bool showEvents = false;

	public override void OnInspectorGUI()
	{			
		DrawDefaultInspector();
		
		EditorGUILayout.Space();	
		
		showEvents = EditorGUILayout.Foldout(showEvents, "Delegates");
		if(showEvents)
			DrawEventInspector();	
	}
	
	public void DrawEventInspector()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(16);
		EditorGUILayout.BeginVertical();
		
		EditorDelegateGUI.EventFields(target as Component);
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}

public static class EditorDelegateGUI
{
	public static void EventFields (Component targetBehavior)
	{
		if(targetBehavior == null)
			return;
		
		foreach(FieldInfo field in targetBehavior.GetType().GetFields())
		{			
			//
			// GET/SET FIELD
			if(field.FieldType == typeof(DelegateMessage[]) || field.FieldType == typeof(Delegate))
			{
				//
				// ARRAY FIELDS
				DelegateMessage[] oldEvents = field.GetValue(targetBehavior) as DelegateMessage[];
				if(field.FieldType == typeof(Delegate))
				{
					Delegate list = (Delegate)field.GetValue(targetBehavior);
					oldEvents = list != null ? list.events:null;
				}
				else
					oldEvents = field.GetValue(targetBehavior) as DelegateMessage[];
					
				DelegateMessage[] newEvents;
				newEvents = EditorDelegateGUI.EventArrayField(field.Name, oldEvents, targetBehavior.gameObject);
				
				if(field.FieldType == typeof(Delegate))
				{
					Delegate list = (Delegate)field.GetValue(targetBehavior);
					list.events = newEvents;
				}
				else 
					field.SetValue(targetBehavior,newEvents);
			}
		}
	}
	
	public static DelegateMessage[] EventArrayField(string label, DelegateMessage[] events, GameObject dynParamSource)
	{
		List<DelegateMessage> eventList = events ==null ? new List<DelegateMessage>():new List<DelegateMessage>(events);
		
		if(events == null)
			events = new DelegateMessage[0];
		
		EditorGUILayout.PrefixLabel(label);
		EditorGUILayout.BeginVertical();
				
		DelegateMessage removeItem = null;
		for (int i = 0; i < eventList.Count; i++) 
		{
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("-", GUILayout.Width(20)))
				removeItem = eventList[i];
			else
				eventList[i] = EventField(null, eventList[i], dynParamSource);

			EditorGUILayout.EndHorizontal();
		}
		
		if(removeItem!=null)
			eventList.Remove(removeItem);
		        
		if(GUILayout.Button("+", GUILayout.Width(20)))
		{
			eventList.Add(new DelegateMessage());
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
		
		return eventList.ToArray();
	}
	
	public static DelegateMessage EventField(string label, DelegateMessage evt, GameObject dynParamSource)
	{	
		EditorGUILayout.BeginVertical();
		
		if(label != null)
			EditorGUILayout.PrefixLabel(label);

		EditorGUILayout.BeginHorizontal();

		DelegateMessage newEvt = (DelegateMessage)evt.Clone();				
		EventField_ContextPicker(newEvt);


		// SELECT EVENT TYPES
		GameObject target = (evt.targetId == -1) ? evt.overrideTarget:dynParamSource;
		DelegateDescriptor[] eventTypes = EditorDelegateUtil.FindEventsForObject(target);
		

		// SELECT EVENT & PARAMS
		DelegateDescriptor ed = EventField_EventDescriptorPopup(eventTypes, newEvt);
		EditorGUILayout.EndHorizontal();
		EventField_Parameters (ed, newEvt, dynParamSource);	
		EditorGUILayout.EndVertical();
		
		return newEvt;
	}
	
	private static void EventField_ContextPicker(DelegateMessage newEvt)
	{
		List<string> contextList = new List<string>();

		contextList.Add("Self (Default)");
		
		contextList.Add("");
		contextList.Add("override...");
		
		// SELECT TARGET
		if(newEvt.targetId == -1)
			newEvt.targetId = contextList.Count-1;
		newEvt.targetId = EditorGUILayout.Popup(newEvt.targetId, contextList.ToArray(), "popup", GUILayout.Width(80));
		if(newEvt.targetId == contextList.Count-1)
		{
			newEvt.targetId = -1;
			newEvt.overrideTarget = (GameObject)EditorGUILayout.ObjectField(newEvt.overrideTarget, typeof(GameObject), true, GUILayout.Width(80));
		}
	}

	#region internals
	private static DelegateDescriptor EventField_EventDescriptorPopup (DelegateDescriptor[] eventTypes, DelegateMessage oldEvent)
	{
		List<string> optionsList = new List<string>();
		foreach(DelegateDescriptor item in eventTypes)
			optionsList.Add(item.ToString());
		optionsList.Add("--NONE--");
		string[] optionsStrings = optionsList.ToArray();
		
		// loop through till we get a match, note that a value of eventTypes.Length IS valid here 
		int eventId = 0;
		for (; eventId < eventTypes.Length; eventId++) 
		{
			if(eventTypes[eventId].signature.methodName == oldEvent.name)
			{
			   	if(oldEvent.parameter.GetParameterType().IsAssignableFrom(eventTypes[eventId].signature.parameterType ))
					break;
			}
		}
			   		
		eventId = EditorGUILayout.Popup(eventId, optionsStrings, "popup");
		
		if(eventId==optionsList.Count-1)
			return null;
		
		return eventTypes[eventId];
	}
	
	private static DelegateDescriptor EventField_EventDescriptorPopup (string label, DelegateDescriptor[] eventTypes, DelegateMessage oldEvent)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(label);
		DelegateDescriptor ed = EventField_EventDescriptorPopup(eventTypes, oldEvent);
		EditorGUILayout.EndHorizontal();
		return ed;
	}
	
	private static void EventField_ParameterField (DelegateDescriptor ed, DelegateMessage newEvt, GameObject dynamicParamSource)
	{
		EditorGUILayout.BeginHorizontal();
		
		GUILayout.Label(ed.signature.parameterName, "box", GUILayout.Width(80));	
		
		// FIND DYNAMIC PARAMS
		List<string> dynParamNames = new List<string>(new string[] {"value...",""});
		List<string> memberNames = new List<string>(new string[] {"",""});
		List<Object> objects = new List<Object>(new Object[] {null, null});
		bool prevFoundOne = false;
		foreach(Component comp in dynamicParamSource.GetComponents<Component>())
		{
			if(prevFoundOne)
			{
				dynParamNames.Add("");
				memberNames.Add("");
				objects.Add(null);
			}
			prevFoundOne = false;
			
			foreach(MemberInfo item in comp.GetType().GetMembers())
			{
				if(item.MemberType != MemberTypes.Field && item.MemberType != MemberTypes.Property)
					continue;
				
				if(item.DeclaringType.IsAssignableFrom(typeof(Component)))
				   continue;
				
				System.Type itemType = null;
				
				if(item.MemberType == MemberTypes.Field)
					itemType = (item as FieldInfo).FieldType;
				else if (item.MemberType == MemberTypes.Property)
					itemType = (item as PropertyInfo).PropertyType;
				
				if(!ed.signature.parameterType.IsAssignableFrom(itemType))
					continue;
				
				prevFoundOne = true;
				dynParamNames.Add(comp.GetType().Name+"."+item.Name);
				memberNames.Add(item.Name);
				objects.Add(comp);
			}
		}
		
		int dynParamId = memberNames.IndexOf(newEvt.parameter.objectMemberName);
		if(dynParamId==-1)
			dynParamId = 0;
		
		if(dynParamNames.Count > 1)
			dynParamId = EditorGUILayout.Popup(dynParamId, dynParamNames.ToArray(), "popup", GUILayout.MinWidth(60));				
			
		if(dynParamId > 1 && dynParamNames.Count > 1)
		{
			newEvt.parameter.SetDynamicValue(objects[dynParamId], memberNames[dynParamId], ed.signature.parameterType);
		}
		else 
		{
			if(ed.signature.parameterType == typeof(string))
				newEvt.parameter.SetValue(EditorGUILayout.TextField(newEvt.parameter.stringParam), ed.signature.parameterType);
			
			if(ed.signature.parameterType == typeof(int))
				newEvt.parameter.SetValue(EditorGUILayout.IntField(newEvt.parameter.intParam), ed.signature.parameterType);
	
			if(ed.signature.parameterType == typeof(float))
				newEvt.parameter.SetValue(EditorGUILayout.FloatField(newEvt.parameter.floatParam), ed.signature.parameterType);
			
			if(ed.signature.parameterType == typeof(Vector3))
				newEvt.parameter.SetValue(EditorGUILayout.Vector3Field("", newEvt.parameter.vectorParam), ed.signature.parameterType);
	
			if(ed.signature.parameterType == typeof(bool))
				newEvt.parameter.SetValue(EditorGUILayout.Toggle(newEvt.parameter.boolParam), ed.signature.parameterType);
			
			if(typeof(UnityEngine.Object).IsAssignableFrom(ed.signature.parameterType))
				newEvt.parameter.SetValue(EditorGUILayout.ObjectField(newEvt.parameter.objectParam, ed.signature.parameterType, true), ed.signature.parameterType);
		}
		
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4);
	}
	
	private static void EventField_Parameters(DelegateDescriptor ed, DelegateMessage newEvt, GameObject dynParamSource)
	{	
		if(ed==null)
		{
			newEvt.name = "";
			if(newEvt.parameter!=null)
				newEvt.parameter.SetVoid();
			return;
		}
	
		newEvt.name = ed.signature.methodName;
			
		if(ed.signature.parameterType == typeof(void))
			newEvt.parameter.SetVoid();
		else
		{
			EventField_ParameterField (ed, newEvt, dynParamSource);
		}
	}	
#endregion
}

public static class EditorDelegateUtil
{	
	public static DelegateDescriptor[] FindEventsForObject(GameObject evtSource)
	{
		return ProcessGameObjectForEvents(evtSource).ToArray();
	}
	
	public static string[] FindEventStrings(GameObject evtSource)
	{
		List<DelegateDescriptor> events = ProcessGameObjectForEvents(evtSource);
		List<string> eventStrings = new List<string>();
		foreach(DelegateDescriptor item in events)
			eventStrings.Add(item.signature.methodName);
		
		return eventStrings.ToArray();
	}
	
	#region internals and stuff
	private static List<DelegateDescriptor> ProcessGameObjectForEvents(GameObject evtSource)
	{
		List<DelegateDescriptor> eventDescriptors = new List<DelegateDescriptor>();
		if(evtSource!=null)
		{
			MonoBehaviour[] behaviours = evtSource.GetComponents<MonoBehaviour>();
			
			foreach(MonoBehaviour behavior in behaviours)
				ProcessMonobehaviourForEvents(eventDescriptors, behavior.GetType());
		}
		
		return eventDescriptors;
	}

	private static void ProcessMonobehaviourForEvents (List<DelegateDescriptor> eventDescriptors, System.Type behaviorType)
	{
		string[] eventNameExclusions = new string[] {"Update", "FixedUpdate",  "LateUpdate", "Start", "Awake", "OnEnable", "OnDisable", "OnDrawGizmos"};

		MethodInfo[] methods = behaviorType.GetMethods();
		foreach(MethodInfo method in methods)
		{
			System.Reflection.ParameterInfo[] parameters = method.GetParameters();
			// events are currently restrected to public, zero parameter functions, that aren't constructors
			if(parameters.Length > 1 || method.IsConstructor || !method.IsPublic)
				continue;
		
			// return type void only
			if(method.ReturnType != typeof(void))
				continue;
			
			// skip all base class methods from monodevelop, component and UnityEngine.Object
			if(method.DeclaringType.IsAssignableFrom(typeof(MonoBehaviour)))
			 	continue;
			
			// don't allow unity callbacks to be used as events (Update, Awake, etc)				
			if(System.Array.IndexOf(eventNameExclusions, method.Name)!=-1)
				continue;
	
			if(parameters.Length>0 && !IsSupportedParamType(parameters[0].ParameterType))
				continue;
				
			DelegateDescriptor.EventSignature signature;
			
			if(parameters.Length>0)
				signature = new DelegateDescriptor.EventSignature(method.Name, parameters[0].Name, parameters[0].ParameterType);
			else 
				signature = new DelegateDescriptor.EventSignature(method.Name);
			
			foreach(DelegateDescriptor item in eventDescriptors)
			{
				if(item.signature.ToString() == signature.ToString())
					continue;
			}
			
			eventDescriptors.Add(new DelegateDescriptor(signature));
		}
	}
	
	private static void ParseAssetsForEvents_Recursive(Dictionary<string, DelegateDescriptor> events, System.IO.DirectoryInfo dir, System.IO.DirectoryInfo root)
	{
		System.IO.FileInfo[] files = dir.GetFiles();
		foreach (System.IO.FileInfo item in files) 
		{
			string path = EvaluateRelativePath(root.FullName, item.FullName);			
			
			if(System.IO.Path.GetExtension(path) == ".prefab")
				ParseAssetsForEvents_Prefab(events, path);
			else if(System.IO.Path.GetExtension(path) == ".cs")
				ParseAssetsForEvents_Script(events, path);
		}
		
		System.IO.DirectoryInfo[] dirs = dir.GetDirectories();
		foreach (System.IO.DirectoryInfo item in dirs) 
		{
			ParseAssetsForEvents_Recursive(events, item, root);	
		}
	}
	
	private static void ParseAssetsForEvents_Prefab (Dictionary<string, DelegateDescriptor> events, string path)
	{
		GameObject gameobject = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
		if(gameobject != null)
		{
			ParseAssetsForEvents_GameObject(events,gameobject);
		}
	}
	
	private static void ParseAssetsForEvents_GameObject(Dictionary<string, DelegateDescriptor> events, GameObject gameobject)
	{
		List<DelegateDescriptor> eventList = ProcessGameObjectForEvents(gameobject);
		foreach(DelegateDescriptor item in eventList)
		{
			string eventStr = item.signature.ToString();
			if(!events.ContainsKey(eventStr))
				events.Add(eventStr, item);

			item.usedByPrefabs.Add(gameobject);
		}
	}

	private static void ParseAssetsForEvents_Script (Dictionary<string, DelegateDescriptor> events, string path)
	{
		MonoScript monoScript = (MonoScript)AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
		if(monoScript != null && typeof(MonoBehaviour).IsAssignableFrom(monoScript.GetClass()))
		{
			List<DelegateDescriptor> eventList = new List<DelegateDescriptor>();
			ProcessMonobehaviourForEvents(eventList, monoScript.GetClass());
			foreach(DelegateDescriptor item in eventList)
			{
				string eventStr = item.signature.ToString();
				if(!events.ContainsKey(eventStr))
					events.Add(eventStr, item);

				item.usedByBehaviour.Add(monoScript.GetClass());
			}
		}
	}
	
	private static bool IsSupportedParamType(System.Type parameterType)
	{
		if(parameterType == typeof(string))
		   return true;
		   
		if(parameterType == typeof(int))
			return true;

		if(parameterType == typeof(float))
			return true;
		
		if(parameterType == typeof(Vector3))
			return true;
		
		if(parameterType == typeof(bool))
			return true;
		
		if(typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
			return true;
		   
		return false;
	}
	#endregion
	
	public static string EvaluateRelativePath(string mainDirPath, string absoluteFilePath)
	{
	    string[] firstPathParts = mainDirPath.Trim(System.IO.Path.DirectorySeparatorChar).Split(System.IO.Path.DirectorySeparatorChar);
	    string[] secondPathParts = absoluteFilePath.Trim(System.IO.Path.DirectorySeparatorChar).Split(System.IO.Path.DirectorySeparatorChar);
	
	    int sameCounter = 0;
	    for (int i = 0; i < System.Math.Min(firstPathParts.Length, secondPathParts.Length); i++)
	    {
	        if (!firstPathParts[i].ToLower().Equals(secondPathParts[i].ToLower()))
	        {
	            break;
	        }
	        sameCounter++;
	    }
	
	    if (sameCounter == 0)
	    {
	        return absoluteFilePath;
	    }
	
	    string newPath = System.String.Empty;
	    for (int i = sameCounter; i < firstPathParts.Length; i++)
	    {
	        if (i > sameCounter)
	        {
	            newPath += System.IO.Path.DirectorySeparatorChar;
	        }
	        newPath += "..";
	    }

	    for (int i = sameCounter; i < secondPathParts.Length; i++)
	    {
			if (newPath.Length > 0)
	        	newPath += System.IO.Path.DirectorySeparatorChar;
	        newPath += secondPathParts[i];
	    }
	    return newPath;
	} 
}

public class DelegateDescriptor
{	
	public struct EventSignature
	{
		public EventSignature (string name)
		{
			methodName = name;
			parameterName = "";
			parameterType = typeof(void);
		}
		
		public EventSignature (string name, string param, System.Type paramType)
		{
			methodName = name;
			parameterName = param;
			parameterType = paramType;
		}
	
		public override string ToString ()
		{
			if(parameterType == typeof(void))
				return string.Format("{0}", methodName);						
			else
				return string.Format("{0}({1} {2})", methodName, parameterType, parameterName);					
		}
		
		public string 		methodName;
		public string		parameterName;
		public System.Type 	parameterType;
	}
	
	public EventSignature 		signature;
	public List<GameObject> 	usedByPrefabs = new List<GameObject>();
	public List<System.Type> 	usedByBehaviour = new List<System.Type>();
	
	public DelegateDescriptor (EventSignature signature)
	{
		this.signature = signature;	
	}

	public override string ToString()
	{
		string str = string.Format("{0}", signature.ToString());
		
		int tabCounter = 5 - (int)((float)signature.ToString().Length/4+0.5f);
		for(int i=0;i<tabCounter;++i)
			str += "\t";
			
		if(usedByPrefabs.Count>0 || usedByBehaviour.Count>0)
			str += "\t- ";
		
		if(usedByPrefabs.Count>0)
		{
			str += "prefabs[";
			for (int i = 0; i < usedByPrefabs.Count; i++) 
				str += string.Format("{0}{1}", usedByPrefabs[i].name, ((i+1)<usedByPrefabs.Count?", ":"") );	
			str += "]\t";
		}
		
		if(usedByBehaviour.Count>0)
		{
			str += "behaviors[";
			for (int i = 0; i < usedByBehaviour.Count; i++) 
				str += string.Format("{0}{1}", usedByBehaviour[i].Name, ((i+1)<usedByPrefabs.Count?", ":"") );		
			str += "]";
		}
		
		return str;
	}
}