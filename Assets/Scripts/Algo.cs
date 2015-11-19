using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct Part
{
    public ParticleSystem.Particle P;
    public float Weight;
}

public class Algo
{
    private static float e = 2.718281828459045f;
    private static float sigma2 = Mathf.Pow(0.9f, 2);

    public ParticleSystem Particles;
    public Drone drone;
    private Part[] parts;
    public float Filtering;

    public float WGauss(float a, float b)
    {
        float error = a - b;
        return Mathf.Pow(e, -Mathf.Pow(error, 2) / (2 * sigma2));
    }

    public void Pass()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Particles.maxParticles];
        int amount = Particles.GetParticles(particles);
        parts = new Part[amount];

        float avg = 0.0f;
        for (int i = 0; i < amount; ++i)
        {
            Part part = new Part();
            part.P = particles[i];

            RaycastHit2D hit;
            hit = Physics2D.Raycast(part.P.position, -drone.transform.right);
            if (hit.collider != null && drone.left > 0.0f)
            {
                part.Weight += WGauss(drone.left, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, drone.transform.right);
            if (hit.collider != null && drone.right > 0.0f)
            {
                part.Weight += WGauss(drone.right, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, drone.transform.up);
            if (hit.collider != null && drone.up > 0.0f)
            {
                part.Weight += WGauss(drone.up, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, -drone.transform.up);
            if (hit.collider != null && drone.down > 0.0f)
            {
                part.Weight += WGauss(drone.down, hit.distance);
            }

            parts[i] = part;

            avg += part.Weight;
            particles[i].size = part.Weight / 10.0f;
        }
        avg /= amount;

        float filter = avg * Filtering;
        for (int i = 0; i < amount; ++i)
        {
            if (parts[i].Weight >= filter)
            {
                particles[i].color = Color.red;
            }
            else
            {
                particles[i].color = Color.white;
            }
        }
        Particles.SetParticles(particles, amount);
    }
}