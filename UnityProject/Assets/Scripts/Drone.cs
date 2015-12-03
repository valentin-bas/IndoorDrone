using UnityEngine;
using System.Collections;
using System.Text;

public class Drone : MonoBehaviour
{
    public ZumoControl Zumo;
    public ParticleSystem System;
    public float Distance = 2.0f;
    public float left;
    public float right;
    public float up;
    public float down;
    public float Filtering = 2.0f;
    public float MoveSpeed = 5.0f;
    public LayerMask Mask;
    public int ParticleCount = 2000;
    public float Noise = 0.1f;

    private Algo algo;

    private new Rigidbody2D rigidbody;
    private Vector3 oldPosition;

    void Start()
    {
        oldPosition = transform.position;

        algo = new Algo();
        algo.drone = this;
        algo.Particles = System;
        algo.SetZ(0.0f);

        rigidbody = GetComponent<Rigidbody2D>();

        StartCoroutine(Pass());
        StartCoroutine(S());
    }

    private int toast = 0;
    IEnumerator S()
    {
        while (true)
        {
            //int angle = toast++;
            //
            //transform.rotation = Quaternion.AngleAxis(angle, -Vector3.forward);

            if (Zumo._client != null && Zumo._client.Connected)
            {
                byte[] data = Encoding.ASCII.GetBytes("a\n");
                Zumo._client.Send(data);
                data = Encoding.ASCII.GetBytes("s\n");
                Zumo._client.Send(data);
            }
            yield return new WaitForSeconds(0.25f);

        }
    }

    void Update()
    {
        DoInput();

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            algo.Reset();
        }
        //Sensor();

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Vector3 relMove = transform.position - oldPosition;
        //    algo.Filtering = Filtering;
        //    algo.Mask = Mask;
        //    algo.RelMove = relMove;
        //    algo.ParticleCount = ParticleCount;
        //    algo.Pass();

        //    oldPosition = transform.position;
        //}
    }

    private IEnumerator Pass()
    {
        while (true)
        {
            //if (Dirty)
            //{
                Vector3 relMove = transform.position - oldPosition;

                algo.Filtering = Filtering;
                algo.Mask = Mask;
                algo.RelMove = relMove;
                algo.ParticleCount = ParticleCount;
                algo.Pass();

                oldPosition = transform.position;
                Dirty = false;
            //}

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void DoInput()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector2 moveDirection = new Vector2(0.0f, MoveSpeed * Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection) * Time.deltaTime * 10.0f;
        rigidbody.velocity = moveDirection;

        //transform.Rotate(transform.forward * -horizontal * Time.deltaTime * 360.0f);
    }

    private void Sensor()
    {
        RaycastHit2D cleft = Physics2D.Raycast(transform.position, -transform.right, Distance, Mask);
        RaycastHit2D cright = Physics2D.Raycast(transform.position, transform.right, Distance, Mask);
        RaycastHit2D cup = Physics2D.Raycast(transform.position, transform.up, Distance, Mask);
        RaycastHit2D cdown = Physics2D.Raycast(transform.position, -transform.up, Distance, Mask);

        if (cleft.collider != null)
        {
            left = cleft.distance + Random.Range(-Noise, Noise);
            Debug.DrawLine(transform.position, cleft.point, Color.red);
        }
        else
        {
            left = -1.0f;
        }
        if (cright.collider != null)
        {
            right = cright.distance + Random.Range(-Noise, Noise);
            Debug.DrawLine(transform.position, cright.point, Color.red);
        }
        else
        {
            right = -1.0f;
        }
        if (cup.collider != null)
        {
            up = cup.distance + Random.Range(-Noise, Noise);
            Debug.DrawLine(transform.position, cup.point, Color.red);
        }
        else
        {
            up = -1.0f;
        }
        if (cdown.collider != null)
        {
            down = cdown.distance + Random.Range(-Noise, Noise);
            Debug.DrawLine(transform.position, cdown.point, Color.red);
        }
        else
        {
            down = -1.0f;
        }

        // override left and back
        left = -1.0f;
        down = -1.0f;
    }

    public bool Dirty { get; set; }
}
