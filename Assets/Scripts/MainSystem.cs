using UnityEngine;
using System.Collections;

public class MainSystem {
#if UNITY_EDITOR
	public static int ScreenHeight = Screen.height;
	public static int ScreenWidth = Screen.width;
#else
	// Need to flip these for horizontal display
	public static int ScreenHeight = Screen.width;
	public static int ScreenWidth = Screen.height;
#endif
}
