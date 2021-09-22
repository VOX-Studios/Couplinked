using UnityEngine;
using System.Collections.Generic;

public class LightningGenerator
{
	public static void SetBolt(Vector2 source, Vector2 dest, LineRenderer lineRenderer)
	{
		Vector2 tangent = dest - source;

		//get the perpendicular line so that the displacement is proper
		float temp = Mathf.Atan2(tangent.y, tangent.x);
		//Rotate clockwise
		temp += -90 * Mathf.Deg2Rad;
		Vector2 normal = new Vector2 (Mathf.Cos(temp), Mathf.Sin(temp));

		float length = tangent.magnitude;
		
		List<float> positions = new List<float>();
		
		for (int i = 0; i < length / 4; i++) 
		{
			positions.Add(Random.Range(.25f, .75f));
		}
		
		positions.Sort();

		lineRenderer.positionCount = positions.Count + 2;
		lineRenderer.SetPosition(0, source);

		for (int i = 0; i < positions.Count; i++)
		{
			float pos = positions[i];
			float displacement = Random.Range(-.5f, .5f);//<-TEMP

			Vector2 point = source + pos * tangent + displacement * normal;
			lineRenderer.SetPosition(i + 1, point);
		}

		lineRenderer.SetPosition(lineRenderer.positionCount - 1, dest);
	}
}