using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JBCStyle
{
	
	Dictionary<ConsoleLevel, GUIStyle> levelToStyle;
	
	GUISkin skin;
	
	public JBCStyle()
	{
		skin = GUISkin.CreateInstance<GUISkin>();
		var tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
		tex.Apply();
		skin.box.normal.background = tex;
		
		levelToStyle = new Dictionary<ConsoleLevel, GUIStyle>();
		
		GUIStyle style = MakeLogStyle();
		style.normal.textColor = new Color(0.62f, 0.82f, 0.62f);
		levelToStyle[ConsoleLevel.Debug] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.87f, 0.87f);
		levelToStyle[ConsoleLevel.Info] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.47f, 0.47f);
		levelToStyle[ConsoleLevel.Warn] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.133f, 0.133f);
		levelToStyle[ConsoleLevel.Error] = style;
		levelToStyle[ConsoleLevel.Fatal] = style;
	}
	
	GUIStyle MakeLogStyle()
	{
		var style = new GUIStyle();
		style.richText = true;
		return style;
	}
	
	public float GetLogLineHeight()
	{
		return levelToStyle[ConsoleLevel.Debug].lineHeight;
	}
	
	public GUISkin Skin
	{
		get { return skin; }
	}
	
	public GUIStyle GetStyleForLogLevel(ConsoleLevel level)
	{
		return levelToStyle[level];
	}
}
