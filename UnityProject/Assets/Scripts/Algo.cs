using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct Part
{
    public ParticleSystem.Particle P;
    public float Weight;
}

public struct ReusableParticle
{
    public ParticleSystem.Particle Particle;
    public int Index;
}

public class Algo
{
    private static float e = 2.718281828459045f;
    private static float sigma2 = Mathf.Pow(0.9f, 2);

    public ParticleSystem Particles;
    public Drone drone;
    private Part[] parts;
    public float Filtering;
    public LayerMask Mask;
    public Vector3 RelMove;
    public int ParticleCount;

    private List<ReusableParticle> reuse = new List<ReusableParticle>();
    private List<ReusableParticle> best = new List<ReusableParticle>();

    public void Reset()
    {
        reuse = new List<ReusableParticle>();
        best = new List<ReusableParticle>();
        parts = null;
        Particles.Clear();
    }

    public float WGauss(float a, float b)
    {
        float error = a - b;
        return Mathf.Pow(e, -Mathf.Pow(error, 2) / (2 * sigma2));
    }

    public void SetZ(float z)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Particles.maxParticles];
        int amount = Particles.GetParticles(particles);
        for (int i = 0; i < amount; ++i)
        {
            particles[i].position = new Vector3(particles[i].position.x, particles[i].position.y, z);
        }
        Particles.SetParticles(particles, amount);
    }

    private void SpawnMissingParticle()
    {
        if (Particles.particleCount == 0)
        {
            Particles.Emit(ParticleCount);
        }
        else
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Particles.maxParticles];
            int amount = Particles.GetParticles(particles);

            List<ParticleSystem.Particle> r = new List<ParticleSystem.Particle>();
            List<ParticleSystem.Particle> b = new List<ParticleSystem.Particle>();

            for (int i = 0; i < best.Count; ++i )
            {
                b.Add(best[i].Particle);
            }

            for (int i = 0; i < reuse.Count; ++i)
            {
                ReusableParticle p2 = best[Random.Range(0, best.Count - 1)];
                var v = reuse[i];
                v.Particle.position = new Vector2(p2.Particle.position.x + Random.Range(-0.05f, 0.05f), p2.Particle.position.y + Random.Range(-0.05f, 0.05f));
                reuse[i] = v;
                r.Add(reuse[i].Particle);
            }

            //int particleUnit = amount / reuse.Count;
            //for (int i = 0; i < amount; ++i)
            //{
            //    for (int j = 0; j < particleUnit; ++j)
            //    {
            //        reuse[i * particleUnit + j].Particle.position 
            //        //ParticleSystem.Particle p = new ParticleSystem.Particle();
            //        //p.position = new Vector2(particles[i].position.x + Random.Range(-1.0f, 1.0f), particles[i].position.y + Random.Range(-1.0f, 1.0f));
            //        //p.lifetime = 500000;
            //        //p.size = 0.1f;
            //        //p.color = Color.white;
            //        //p.velocity = Vector3.zero;
            //    }
            //}

            var arr1 = b.ToArray();
            var arr2 = r.ToArray();
            ParticleSystem.Particle[] newParts = new ParticleSystem.Particle[Particles.maxParticles];
            System.Array.Copy(arr1, 0, newParts, 0, arr1.Length);
            System.Array.Copy(arr2, 0, newParts, arr1.Length, arr2.Length);

            Particles.SetParticles(newParts, arr1.Length + arr2.Length);

            best.Clear();
            reuse.Clear();

            //int diff = ParticleCount - Particles.particleCount;
            //if (diff > 0)
            //{
            //    Particles.Emit(diff);
            //}
        }
    }

    public void Pass()
    {
        SpawnMissingParticle();

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Particles.maxParticles];
        int amount = Particles.GetParticles(particles);
        parts = new Part[amount];

        float avg = 0.0f;
        for (int i = 0; i < amount; ++i)
        {
            Part part = new Part();
            part.P = particles[i];
            //particles[i].position += RelMove;

            RaycastHit2D hit;
            hit = Physics2D.Raycast(part.P.position, drone.transform.right, Mask);
            if (hit.collider != null && drone.left > 0.0f)
            {
                //Debug.DrawLine(part.P.position, hit.point);
                part.Weight += WGauss(drone.left, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, -drone.transform.right, Mask);
            if (hit.collider != null && drone.right > 0.0f)
            {
                //Debug.DrawLine(part.P.position, hit.point);
                part.Weight += WGauss(drone.right, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, -drone.transform.up, Mask);
            if (hit.collider != null && drone.up > 0.0f)
            {
                //Debug.DrawLine(part.P.position, hit.point);
                part.Weight += WGauss(drone.up, hit.distance);
            }
            hit = Physics2D.Raycast(part.P.position, drone.transform.up, Mask);
            if (hit.collider != null && drone.down > 0.0f)
            {
                //Debug.DrawLine(part.P.position, hit.point);
                part.Weight += WGauss(drone.down, hit.distance);
            }

            parts[i] = part;

            avg += part.Weight;
            particles[i].size = part.Weight / 10.0f;
        }
        avg /= amount;


        float filter = avg;
        for (int i = 0; i < amount; ++i)
        {
            if (parts[i].Weight >= filter)
            {
                particles[i].color = Color.red;
                best.Add(new ReusableParticle { Particle = particles[i], Index = i });
                //newParticles.Add(particles[i]);
            }
            else
            {
                //particles[i].color = Color.black;
                particles[i].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                reuse.Add(new ReusableParticle { Particle = particles[i], Index = i });
            }
        }
        Particles.SetParticles(particles, amount);
        //Particles.SetParticles(particles, amount);
    }
}