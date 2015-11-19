using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
    public ParticleSystem System;
    public float Distance = 2.0f;
    public float left;
    public float right;
    public float up;
    public float down;
    public float Filtering = 2.0f;

    private Algo algo;

    void Start()
    {
        algo = new Algo();
        algo.drone = this;
        algo.Particles = System;
    }

    void Update()
    {
        Sensor();

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        algo.Filtering = Filtering;
            algo.Pass();
        //}
    }

    private void Sensor()
    {
        RaycastHit2D cleft = Physics2D.Raycast(transform.position, -transform.right, Distance);
        RaycastHit2D cright = Physics2D.Raycast(transform.position, transform.right, Distance);
        RaycastHit2D cup = Physics2D.Raycast(transform.position, transform.up, Distance);
        RaycastHit2D cdown = Physics2D.Raycast(transform.position, -transform.up, Distance);

        if (cleft.collider != null)
        {
            left = cleft.distance;
            Debug.DrawLine(transform.position, cleft.point, Color.red);
        }
        else
        {
            left = -1.0f;
        }
        if (cright.collider != null)
        {
            right = cright.distance;
            Debug.DrawLine(transform.position, cright.point, Color.red);
        }
        else
        {
            right = -1.0f;
        }
        if (cup.collider != null)
        {
            up = cup.distance;
            Debug.DrawLine(transform.position, cup.point, Color.red);
        }
        else
        {
            up = -1.0f;
        }
        if (cdown.collider != null)
        {
            down = cdown.distance;
            Debug.DrawLine(transform.position, cdown.point, Color.red);
        }
        else
        {
            down = -1.0f;
        }
    }
}
