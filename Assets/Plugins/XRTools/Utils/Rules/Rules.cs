using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Rules
{
	interface iRule
	{
		bool Accepts(object target);
	}

	public class Rule : MonoBehaviour
	{
		public static bool ActiveInHierarchy(GameObject targetGameObject)
		{
			return targetGameObject.activeInHierarchy;

		}

		public static bool LayerCheck(GameObject targetGameObject, LayerMask LayerMask)
		{
			return (LayerMask & (1 << targetGameObject.layer)) != 0;
		}

		public static bool Accepts(GameObject targetGameObject, string[] tags)
		{
			if (tags == null)
				return false;

			foreach (string testedTag in tags)
				if (targetGameObject.CompareTag(testedTag))
					return true;

			return false;
		}

		public static bool Accepts(object target, Object[] objects)
		{
			if ( objects == null)
				return false;
			
			Object targetObject = target as Object;
			return targetObject != null && objects.Contains(targetObject);
		}
	}
	

	public class AnyBehaviourEnabledRule
	{
		//public SerializableTypeBehaviourObservableList BehaviourTypes { get; set; }

		public static bool Accepts(GameObject targetGameObject)
		{
			//if (BehaviourTypes == null)
			//{
			//	return false;
			//}

			//foreach (SerializableType serializedType in BehaviourTypes.NonSubscribableElements)
			//{
			//	if (serializedType.ActualType != null && IsEnabled(targetGameObject.TryGetComponent(serializedType)))
			//	{
			//		return true;
			//	}
			//}

			return false;
		}

		bool IsEnabled(Component component)
		{
			if (component == null)
			{
				return false;
			}

			Behaviour checkBehaviour = component as Behaviour;
			return checkBehaviour != null && checkBehaviour.enabled;
		}
	}
	
}
