using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
	public List<T> activeObjects, inactiveObjects;
	public int maxObjects;
	DeactivateObjectDelegate DeactiveObjectFunction;
	ActivateObjectDelegate ActivateObjectFunction;
	public void Initialize(int max, InstantiateObjectDelegate instantiateObjectFunction,
	                   DeactivateObjectDelegate deactiveObjectFunction,
	                   ActivateObjectDelegate activateObjectFunction) 
	{
		activeObjects = new List<T>();
		inactiveObjects = new List<T>();
		maxObjects = max;

		for(int i = 0; i < maxObjects; i++)
		{
			T obj = instantiateObjectFunction();
			inactiveObjects.Add(obj);
		}

		DeactiveObjectFunction = deactiveObjectFunction;
		ActivateObjectFunction = activateObjectFunction;
	}

	public delegate T InstantiateObjectDelegate();
	public delegate void DeactivateObjectDelegate(ref T obj);
	public delegate void ActivateObjectDelegate(ref T obj);

	public T ActivateObject()
	{
		int inactiveCount = inactiveObjects.Count;
		if(inactiveCount > 0)
		{
			T obj = inactiveObjects[inactiveCount - 1];

			ActivateObjectFunction(ref obj);

			inactiveObjects.RemoveAt(inactiveCount - 1);
			activeObjects.Add(obj);
			return obj;
		}

		return null;
	}

	public void DeactivateObject(int index)
	{
		T obj = activeObjects[index];
		DeactiveObjectFunction(ref obj);
		activeObjects.RemoveAt(index);
		inactiveObjects.Add(obj);
	}
	
	public void DeactivateObject(T obj)
	{
		if(activeObjects.Contains(obj))
		{
			DeactiveObjectFunction(ref obj);
			activeObjects.Remove(obj);
			inactiveObjects.Add(obj);
		}
	}
}
