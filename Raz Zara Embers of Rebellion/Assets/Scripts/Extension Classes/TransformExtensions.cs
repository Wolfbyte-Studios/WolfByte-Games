using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtensions
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
    public static Transform FindDeepChildByTag(this Transform aParent, string tag)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.tag == tag)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
    public static List<Transform> FindDeepChildrenByTag(this Transform aParent, string tag)
    {
        List<Transform> resultList = new List<Transform>();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (current.tag == tag)
            {
                resultList.Add(current);
            }
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        return resultList;
    }


    public static List<Transform> FindDeepChildrenByType<T>(this Transform aParent) where T : Component
    {
        List<Transform> resultList = new List<Transform>();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            T component = current.GetComponent<T>();
            if (component != null)
            {
                resultList.Add(current);  // Add the Transform, not the component
            }
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        return resultList;
    }



public static void LookAtWithStrength(this Transform self, Vector3 targetPosition, float strength)
    {
        // Compute the target rotation as if the transform is looking directly at the target position
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - self.position);

        // Interpolate between the current rotation and the target rotation by the strength factor
        self.rotation = Quaternion.Slerp(self.rotation, targetRotation, strength);
    }
    /*
	//Depth-first search
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		foreach(Transform child in aParent)
		{
			if(child.name == aName )
				return child;
			var result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}
	*/
}