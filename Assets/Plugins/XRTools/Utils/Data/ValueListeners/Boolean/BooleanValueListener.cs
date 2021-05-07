using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XRTools.Utils.Data
{
	
	public class BooleanValueListener : ValueListener<BooleanValueListener, bool, BooleanValueListener.UnityEvent>
	{
		public class UnityEvent : UnityEvent<bool>
		{
		}


	}
}