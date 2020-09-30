using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUtilities{

	public static CanvasGroup GetCanvasGroup(GameObject go)
	{
		if (go.GetComponent<CanvasGroup> ())
			return go.GetComponent<CanvasGroup> ();
		else {
			CanvasGroup _g = go.AddComponent<CanvasGroup> ();
			return _g;
		}
	}
}
