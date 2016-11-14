using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class JBCKeyBinding : MonoBehaviour
{
	readonly List<Binding> bindings = new List<Binding>();

	public delegate bool KeyBindCanTriggerDelegate();

	public KeyBindCanTriggerDelegate CanTriggerHandle;
	public bool ShowingMap {get; private set;}
	bool showingMapAlias;

	void Enable()
	{
		if (!Application.isEditor)
			enabled = false;
	}
	
	void Update ()
	{
		if (!Application.isEditor)
			return;
		
		if(CanTriggerHandle != null && !CanTriggerHandle()) return;

		Binding binding;
		for(var i = bindings.Count - 1; i >= 0; i--)
		{
			binding = bindings[i];
			var nowPressed = binding.IsKeysPressed();
			if(binding.WasPressed)
			{
				if(!nowPressed)
				{
					binding.WasPressed = false;
					if(binding.type == BindType.Up)
					{
						binding.Call();
					}
				}
				else if(binding.type == BindType.Hold)
				{
					binding.Call();
				}
			}
			else if(nowPressed)
			{
				binding.WasPressed = true;
				if(binding.type == BindType.Down && 
				   !bindings.Exists(b => b != binding && 
				                 b.type == BindType.Down &&
				                 b.WasPressed && 
				                 Array.Exists(b.keysCombo, k => Array.IndexOf(binding.keysCombo, k) >= 0))
				   )
				{
					binding.Call();
					return;
				}
			}
		}
	}

	bool wasConsoleVisibleBeforeToggle;
	public void ToggleShowBindings()
	{
		if (!Application.isEditor)
			return;
		
		if(!ShowingMap || !JBConsole.instance.Visible)
		{
			wasConsoleVisibleBeforeToggle = JBConsole.instance.Visible;
			ShowBindings();
		}
		else
		{
			HideBindings();
			JBConsole.instance.Visible = wasConsoleVisibleBeforeToggle;
		}
	}

	public void ShowBindings(bool alias = false)
	{
		if (!Application.isEditor)
			return;
		
		ShowingMap = true;
		showingMapAlias = alias;
		JBConsole.instance.Visible = true;
		JBConsole.instance.Focus(DrawBindings);
	}

	public void HideBindings()
	{
		if (!Application.isEditor)
			return;
		
		if(ShowingMap)
		{
			ShowingMap = false;
			JBConsole.instance.Defocus();
		}
	}

	Vector2 scrollPosition;
	void DrawBindings(float width, float height, float scale = 1)
	{
		GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);
		var console = JBConsole.instance;

		if(GUILayout.Button("CLOSE KEY BINDINGS MAP", console.style.MenuStyle))
		{
			HideBindings();
		}
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		foreach(var binding in bindings)
		{
			if(showingMapAlias || !binding.Alias)
			{
				GUILayout.Label(binding.ComboString, maxwidthscreen);
			}
		}
		GUILayout.EndScrollView ();
	}
	
	Binding FindBinding(BindType type, KeyCode[] keysCombo)
	{
		Binding binding;
		KeyCode[] keys;
		var len = keysCombo.Length;
		bool found;
		for(var i = bindings.Count - 1; i >= 0; i--)
		{
			binding = bindings[i];
			keys = binding.keysCombo;
			if(binding.type == type && keys.Length == len)
			{
				found = true;
				for(var j = keys.Length - 1; j >= 0; j--)
				{
					if(keys[j] != keysCombo[j])
					{
						found = false;
						break;
					}
				}
				if(found)
				{
					return binding;
				}
			}
		}
		return null;
	}

	Binding FindOrCreateBinding(BindType type, KeyCode[] keysCombo)
	{
		var binding = FindBinding(type, keysCombo);
		if(binding == null)
		{
			binding = new Binding(type, keysCombo);
			bindings.Add(binding);

			if(keysCombo.Length > 1)
			{
				// those with more combo gets picked up earlier.
				bindings.Sort((a, b) => a.keysCombo.Length - b.keysCombo.Length);
			}
		}
		return binding;
	}

	void Unbind(BindType type, Action callback)
	{
		Binding binding;
		for(var i = bindings.Count - 1; i >= 0; i--)
		{
			binding = bindings[i];
			if(binding.callback == callback)
			{
				bindings.RemoveAt(i);
			}
		}
	}

	public static void BindDown(KeyCode[] keysCombo, Action callback, string description = null, params KeyCode[][] alias)
	{
		if (!Application.isEditor)
			return;

		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(BindType.Down, keysCombo).Set(callback, description, false);
			foreach(var a in alias)
			{
				instance.FindOrCreateBinding(BindType.Down, a).Set(callback, description, true);
			}
		}
	}

	public static void BindUp(KeyCode[] keysCombo, Action callback, string description = null, params KeyCode[][] alias)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(BindType.Up, keysCombo).Set(callback, description, false);
			foreach(var a in alias)
			{
				instance.FindOrCreateBinding(BindType.Up, a).Set(callback, description, true);
			}
		}
	}

	public static void BindHold(KeyCode[] keysCombo, Action callback, string description = null, params KeyCode[][] alias)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.FindOrCreateBinding(BindType.Hold, keysCombo).Set(callback, description, false);
		}
		foreach(var a in alias)
		{
			instance.FindOrCreateBinding(BindType.Hold, a).Set(callback, description, true);
		}
	}

	public static void UnbindDown(Action callback)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(BindType.Down, callback);
		}
	}

	public static void UnbindUp(Action callback)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(BindType.Up, callback);
		}
	}
	
	public static void UnbindHold(Action callback)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(BindType.Hold, callback);
		}
	}
	
	public static void Unbind(Action callback)
	{
		if (!Application.isEditor)
			return;
		
		var instance = Instance;
		if(instance)
		{
			instance.Unbind(BindType.Up, callback);
			instance.Unbind(BindType.Down, callback);
			instance.Unbind(BindType.Hold, callback);
		}
	}

	static JBCKeyBinding _instance;
	static JBCKeyBinding Instance
	{
		get
		{
			if (!Application.isEditor)
				return null;
			
			if(!_instance && JBConsole.Exists)	
			{
				_instance = JBConsole.instance.RegisterPlugin<JBCKeyBinding>();
			}
			return _instance;
		}
	}

	enum BindType
	{
		Down,
		Hold,
		Up
	}

	class Binding
	{
		public BindType type;
		public KeyCode[] keysCombo;
		public Action callback;
		public string desc;
		public bool Alias;

		public bool WasPressed;

		public Binding(BindType type, KeyCode[] keysCombo)
		{
			if (!Application.isEditor)
				return;
			
			this.type = type;
			this.keysCombo = keysCombo;
			if(keysCombo.Length == 0) throw new System.Exception("Key combo can not be blank.");
		}

		public void Set(Action callback, string desc, bool alias)
		{
			if (!Application.isEditor)
				return;
			
			this.callback = callback;
			this.desc = desc;
			this.Alias = alias;
		}

		public bool IsKeysPressed()
		{
			if (!Application.isEditor)
				return false;
			
			for(var i = keysCombo.Length - 1; i >= 0; i--)
			{
				if(!Input.GetKey(keysCombo[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void Call()
		{
			if (!Application.isEditor)
				return;
			
			if(callback != null)
			{
				callback();
			}
		}

		string comboString;
		public string ComboString
		{
			get
			{
				if (!Application.isEditor)
					return "";
				
				if(comboString == null)
				{
					comboString = "";
					if(type == BindType.Up) comboString += "UP ";
					else if(type == BindType.Down) comboString += "DOWN ";
					else if(type == BindType.Hold) comboString += "HOLD ";
					var l = keysCombo.Length - 1;
					for(var i = 0; i <= l; i++)
					{
						comboString += keysCombo[i];
						if(i < l) comboString += "+";
					}
					if(!string.IsNullOrEmpty(desc))
					{
						comboString += " = " + desc;
					}
				}
				return comboString;
			}
		}
	}
}