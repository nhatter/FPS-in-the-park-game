using UnityEngine;
using System;
using System.Collections.Generic;

public class ComponentUtils {
	public static Transform FindTransformInChildren(GameObject gameObject, string name) {
        foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>()) {
            if(transform.gameObject.name == name) {
                  return transform;
			}
		}

        return null;
    }
}

