//#define DELEGATE_LOGGING
#define DELEGATE_VERBOSE_LOGGING 

// Delegate Event Framework
// Copyright: Cratesmith (Kieran Lord)
// Created: 2010
//
// No warranty or garuntee whatsoever with this code. 
// 

using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace DelegetePulg
{
    [System.Serializable]
    public class DelegateMessage : System.ICloneable
    {
        public string name;
        public EventParameter parameter;
        public int targetId;
        public string enumTypeName;
        public GameObject overrideTarget;

        public bool Valid { get { return name.Length > 0; } }

        struct CacheData
        {
            public CacheData(Component target, MethodInfo method) { this.target = target; this.method = method; }
            public Component target;
            public MethodInfo method;
        }
        CacheData[] cache;

        public void Exec(Component source, bool errorIfNotRecieved)
        {
            GameObject target = null;

            // OVERRIDE TARGET!
            if (targetId == -1)
                target = overrideTarget;

            if (target == null)
                target = source.gameObject;

            DelegateDebug.Log(source.gameObject, "Send target:{0} event:{1} time:{2}", target.name, name, Time.realtimeSinceStartup);
            Send(target, errorIfNotRecieved);
        }

        public void Send(GameObject target, bool errorIfNotRecieved)
        {
            if (target != null && Valid)
            {
                SendMessageOptions sendOptions = errorIfNotRecieved ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver;
                if (parameter.paramType == EventParameter.ParamType.Void)
                    target.SendMessage(name, sendOptions);
                else
                {
                    object param = parameter.GetParameterValue();
                    target.SendMessage(name, param, sendOptions);
                }
            }
        }

        public object Clone()
        {
            DelegateMessage newEvt = (DelegateMessage)MemberwiseClone();
            if (parameter != null)
                newEvt.parameter = (EventParameter)parameter.Clone();

            return newEvt;
        }
    }

    [System.Serializable]
    public class Delegate
    {
        [HideInInspector]
        public DelegateMessage[] events;

        [HideInInspector]
        System.Action onExec;
        public System.Action OnExec { get { return this.onExec; } set { onExec = value; } }

        public void Exec(Component source)
        {
            Exec(source, false);
        }

        public void Exec(Component source, bool errorIfNotRecieved)
        {
            if (onExec != null)
                onExec();

            foreach (DelegateMessage item in events)
                item.Exec(source, errorIfNotRecieved);
        }
    }

    [System.Serializable]
    public class EventParameter : System.ICloneable
    {
        public enum ParamType
        {
            Void = 0,
            Int = 1,
            Float = 2,
            String = 3,
            Object = 4,
            Vector3 = 5,
            Bool = 6
        }

        public ParamType paramType;
        public int intParam;
        public float floatParam;
        public string stringParam;
        public Vector3 vectorParam;
        public Object objectParam;
        public bool boolParam;

        public Object dynamicSource;
        public string objectMemberName;

        public void SetVoid()
        {
            paramType = ParamType.Void;
            intParam = 0;
            floatParam = 0.0f;
            boolParam = false;
            stringParam = "";
            vectorParam = Vector3.zero;
            objectParam = null;
            objectMemberName = null;
            dynamicSource = null;
        }

        public void SetDynamicValue(Object source, string memberName, System.Type parameterType)
        {
            SetVoid();
            dynamicSource = source;
            objectMemberName = memberName;
            SetType(parameterType);
        }

        public void SetValue(object parameter, System.Type parameterType)
        {
            SetVoid();
            SetType(parameterType);
            if (parameter == null || typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
            {
                objectParam = (Object)parameter;
                return;
            }

            if (parameterType == typeof(string))
            {
                stringParam = (string)parameter;
                return;
            }

            if (parameterType == typeof(int))
            {
                intParam = (int)parameter;
                return;
            }

            if (parameterType == typeof(bool))
            {
                boolParam = (bool)parameter;
                return;
            }

            if (parameterType == typeof(float))
            {
                floatParam = (float)parameter;
                return;
            }

            if (parameterType == typeof(Vector3))
            {
                vectorParam = (Vector3)parameter;
                return;
            }
        }

        private void SetType(System.Type parameterType)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
                paramType = ParamType.Object;
            else if (parameterType == typeof(string))
                paramType = ParamType.String;
            else if (parameterType == typeof(int))
                paramType = ParamType.Int;
            else if (parameterType == typeof(float))
                paramType = ParamType.Float;
            else if (parameterType == typeof(Vector3))
                paramType = ParamType.Vector3;
            else if (parameterType == typeof(bool))
                paramType = ParamType.Bool;
            else
                paramType = ParamType.Object;

        }

        public object GetParameterValue()
        {
            if (objectMemberName != null && objectMemberName.Length > 0 && dynamicSource != null)
            {
                // Dynamic param
                MemberInfo[] items = dynamicSource.GetType().GetMember(objectMemberName);
                foreach (MemberInfo item in items)
                {
                    if (item.MemberType == MemberTypes.Field)
                        return ((FieldInfo)item).GetValue(dynamicSource);
                    else if (item.MemberType == MemberTypes.Property)
                        return ((PropertyInfo)item).GetValue(dynamicSource, null);
                }
                return null;
            }
            else
            {
                switch (paramType)
                {
                    case ParamType.Int: return intParam;
                    case ParamType.Float: return floatParam;
                    case ParamType.String: return stringParam;
                    case ParamType.Object: return objectParam;
                    case ParamType.Vector3: return vectorParam;
                    case ParamType.Bool: return boolParam;
                }
            }
            return null;
        }

        public System.Type GetParameterType()
        {
            switch (paramType)
            {
                case ParamType.Int: return typeof(int);
                case ParamType.Float: return typeof(float);
                case ParamType.String: return typeof(string);
                case ParamType.Object: return objectParam != null ? objectParam.GetType() : typeof(Object);
                case ParamType.Vector3: return typeof(Vector3);
                case ParamType.Bool: return typeof(bool);
            }
            return typeof(void);
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

    public static class DelegateDebug
    {
        public struct Entry
        {
            public int context;
            public string message;

            public Entry(int context, string message)
            {
                this.context = context;
                this.message = message;
            }
        }

        public static LinkedList<Entry> logItems = new LinkedList<Entry>();

        public static void Log(GameObject context, string format, params object[] args)
        {
#if DELEGATE_LOGGING
		int contextId = context.GetInstanceID();
		string message = string.Format(format,args);
		logItems.AddLast(new Entry(contextId, message));		

#if DELEGATE_VERBOSE_LOGGING
		Debug.Log(context.name + ": " + message);
#endif
#endif
        }

    }


}
