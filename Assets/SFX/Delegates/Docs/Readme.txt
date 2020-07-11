Unity Editor Delegates 

by Cratesmith (Kieran Lord)
www.cratesmith.com 
cratesmith@cratesmith.com 
====================================

--- What are editor delegates? ---

	Editor delegates let you link events in one script call another function
	using the inspector window instead of writing the link in code.
	
	A good example is having a trigger that will open a door, traditionally you would
	make a script called DoorTrigger... however this is largely redundant. 
	
	A better way is to have a CollisionEvents script that is hooked up by the level 
	designer to call the Open door function on the door object.
	
	I actually made this tool several years ago and have used it in many projects. But seeing as this feature
	was demoed in the Unite 2012 keynote as a future feature... I thought it would be good to let everyone 
	use it to see why it's so neat :)
	
--- How fast are editor delegates? ---

	Short answer: pretty fast, but don't use them for things called every frame.
	
	Long answer... it depends on what features you use. Internally editor delegates uses 
	SendMessage to call functions, which is pretty fast. 
	
	However if you are calling a function using them with a value that is coming from a
	variable in one of your scripts... that's much slower, but still not THAT slow if you
	aren't using it often.
	
	Basically, editor delegates are perfectly fast enough for sending events around. 
	Just don't use them for events that are called every single frame.


--- How to use delegates --- 

= Step 0 - Check out the example scripts in the "Events" folder
	
	These are common use scripts for creating things like
	- triggers
	- timers
	- object events
	- mouse events
	

= Step 1 - Add a editor script for your monobehaviour

	For exmaple if you have: 	"Assets/MyAmazingBehaviour.cs"
	You need to create a file: 	"Assets/Editor/MyAmazingBehaviourEditor.cs"

	And here's the contents of "Assets/Editor/MyAmazingBehaviourEditor.cs"

	[UnityEditor.CustomEditor(typeof(MyAmazingBehavior))]
	public class MyAmazingBehaviourEditor : DelegateEditor {}

	Just make one of these for each monobehaviour you have that uses editor delegates.


= Step 2 - Add "Delegate" parameters to your behavior

	For example, here's an example behaviour using a delegate on start.

	using UnityEngine;

	public class MyAmazingBehaviour 
	{
		[SerializeField] Delegate onStart;

		public void Start() 
		{
			onStart.Exec(this)
		}
	}

	Done! You now have a delegate parameter in your inspector called "onStart" 
	To call a delegate you just need to type "myDelegate.Exec(this)" 


= Step 3 - Hook your delegate up in the editor.

	1. Click on the foldout called "Delegates" in the inspector to see the list of delegates you've exposed in a behaviour.

	2. Click the plus button to create a new event to trigger on a delegate.
	
	3. Select "Self" to call functions on the object that owns the delegate or "Override" to supply any other object as a target to receive this event.

	4. Select the function you want to call from the drop down list. 
		- The delegate calls all funcitons that match the one you select 
		- internally it uses "SendMessage("yourFunctionName", parameter)" to work.
	
	5. You can also use public variables & properties from your monobehaviours as parameters for these functions
		- It can only handle functions with a single parameter 
		- Supported parameter types are:
			. Int
			. Float
			. String
			. Vector3
			. Bool
			. Unity Object & Asset Types




--- Troubleshooting --- 

= My function isn't appearing in the drop down list.
	- A few possible causes:
		. Your function has more than one parameter
		. Your function has a parameter of an unsupported type
		. Your function isn't public
		. You haven't hooked the delegate up to an object with a behaviour that exposes that function 

= I'm calling my delegate from another monobehaviour
	- I strongly advise that only the object owning the delegate should call it
	- If you HAVE to, then pass in the delegate's owner eg. myHorribleHack.myDelegate(myHorribleHack); (but seriously don't)

= Something else is going wrong.
	- If you think it's actual bug contact me via cratesmith@cratesmith.com 
	- I don't have time to actively support this extension by email, but I will try to answer questions in the Unity3d forums
	- If possible, try to figure out what's going wrong and suggest a fix. 
	
	
	
--- About the developer --- 

I'm a freelance/independent developer with a penchant for making tools like this as I'm working.

Occasionally I will polish these up for other people to use, one example of which is Autopilot which does
iOS testflight integration which you can buy on the Asset store.

