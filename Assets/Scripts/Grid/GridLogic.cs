using System.Collections.Generic;
using UnityEngine;

public class GridLogic
{
    Spring[] _springs;
    public PointMass[,] Points;

    public GridLogic(float width, float height, int gridDensity)
    {
        if(gridDensity == 0)
        {
            _springs = new Spring[0];
            Points = new PointMass[0, 0];
            return;
        }    

        float spacing = 1f / gridDensity;

        List<Spring> springList = new List<Spring>();

        int numColumns = (int)Mathf.Ceil(gridDensity * width) + 1; // +1 for extra point due to endpoints, round up to cover whole area
        int numRows = (int)Mathf.Ceil(gridDensity * height) + 1;// +1 for extra point due to endpoints, round up to cover whole area

        float startX = -((numColumns - 1) * spacing) / 2f;
        float startY = -((numRows - 1) * spacing) / 2f;

        Points = new PointMass[numColumns, numRows];

        // create the point masses
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                float xPos = startX + (spacing * x);
                float yPos = startY + (spacing * y);

                float invMass = 1;

                if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1) // anchor the border of the grid 
                {
                    invMass = 0;
                }

                Points[x, y] = new PointMass(
                    new Vector3(xPos, yPos, 0), 
                    invMass
                    );
            }
        }

        // link the point masses with springs
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1) // anchor the border of the grid 
                {
                    springList.Add(new Spring(Points[x, y], Points[x, y], 0.9f, 0.9f));
                }
                //else if (x % 3 == 0 && y % 3 == 0) // loosely anchor 1/9th of the point masses 
                //{
                //    springList.Add(new Spring(Points[x, y], Points[x, y], 0.01f, 0.9f));
                //}

                const float stiffness = .15f;
                const float damping = 0.06f;
                if (x > 0)
                {
                    springList.Add(new Spring(Points[x - 1, y], Points[x, y], stiffness, damping));
                }
                if (y > 0)
                {
                    springList.Add(new Spring(Points[x, y - 1], Points[x, y], stiffness, damping));
                }
            }
        }

        _springs = springList.ToArray();
    }

    public void Update()
    {
        for (int i = 0; i < _springs.Length; i++)
        {
            _springs[i].Update();
        }

        foreach (PointMass mass in Points)
        {
            mass.Update();
        }
    }

    public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
    {
        foreach (PointMass mass in Points)
        {
            if (Vector3.SqrMagnitude(position - mass.Position) < radius * radius)
            {
                mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
            }
        }
    }

    public void ApplyImplosiveForce(float force, Vector3 position, float radius)
    {
        foreach (PointMass mass in Points)
        {
            float dist2 = Vector3.SqrMagnitude(position - mass.Position);
            if (dist2 < radius * radius)
            {
                //mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));

                Vector3 delta = position - mass.Position;

                //have the force magnitude depend on distance from center
                float forceMagnitude = 1f; // (radius - delta.magnitude) / radius;
                mass.ApplyForce((forceMagnitude * force * delta.normalized) / 10f);
            }
        }
    }

    public void ApplyExplosiveForce(float force, Vector3 position, float radius)
    {
        foreach (PointMass mass in Points)
        {
            float dist2 = Vector3.SqrMagnitude(position - mass.Position);
            if (dist2 < radius * radius)
            {
                Vector3 delta = mass.Position - position;

                //have the force magnitude depend on distance from center
                float forceMagnitude = (radius - delta.magnitude) / radius;
                mass.ApplyForce((forceMagnitude * force * delta.normalized) / 10f);
            }
        }
    }
}
