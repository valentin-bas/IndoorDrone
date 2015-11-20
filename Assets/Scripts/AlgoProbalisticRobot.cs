using UnityEngine;
using System.Collections;
using System;
using ProbabilisticRobot;
using ProbabilisticRobot.MotionModel;
using ProbabilisticRobot.PerceptionModel;
using ProbabilisticRobot.Sampling;

public class AlgoProbalisticRobot : MonoBehaviour
{
    public ParticleSystem ParticlesSys;
    public GameObject LinePrefab;

    private ParticleSystem.Particle[] _particles;

    private int _currentStep = -1;

    private RobotPath _path;
    private Map _map;
    private MonteCarloLocalization _MCL;
    private MCLSimulation _MCLSimulation;

    public void Start()
    {
        Sampler.Initialize(new SamplerNormal());
        _InitializeMap();
        _InitializeRobotPath();

        VelocityModel velocityModel = new VelocityModel(0.01, 0.01, 0.01, 0.01, 0.01, 0.01);
        BeamModel beamModel = new BeamModel(3, 0.02, 1, new WeighingFactors(1, 0.1, 0, 0.1));
        _MCL = new MonteCarloLocalization(_map, new Robot(), 20000, velocityModel, beamModel);
        _MCLSimulation = new MCLSimulation(_MCL, _path.StartPose);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _Pass();
    }

    private void _Pass()
    {
        _particles = new ParticleSystem.Particle[ParticlesSys.maxParticles];
        int amount = ParticlesSys.GetParticles(_particles);

        if (_currentStep == -1)
        {
            _MCLSimulation.Drive(null);
        }
        else if (_currentStep < _path.DriveCommands.Count)
        {
            DriveCommand driveCommand = _path.DriveCommands[_currentStep];
            _MCLSimulation.Drive(driveCommand);
        }

        _DrawParticles(amount - 1);
        _DrawRobot(amount);

        ParticlesSys.SetParticles(_particles, amount);

        _currentStep++;
    }

    private void _DrawParticles(int amount)
    {
        int step = _MCL.Particles.Length / amount;
        int outIdx = 0;
        for (int i = 0; i < _MCL.Particles.Length; i += step)
        {
            Pose particle = _MCL.Particles[i];
            _particles[outIdx].position = new Vector3((float)_MCL.Particles[i].X, (float)_MCL.Particles[i].Y, 0.0f);
            _particles[outIdx].color = Color.red;
            outIdx++;
        }
    }

    private void _DrawRobot(int amount)
    {
        Pose currentPose = _path.StartPose;
        for (int i = 0; i < _currentStep + 1; i++)
        {
            if (i < _path.DriveCommands.Count)
            {
                const int sampleCount = 500;
                DriveCommand driveCommand = _path.DriveCommands[i];
                //currentPose = InsertExactPath(currentPose, velocityModel, driveCommand.Velocity, driveCommand.Duration, 500);
                long ticks = driveCommand.Duration.Ticks / sampleCount;

                Pose newPos = null;
                for (int j = 0; j < sampleCount; j++)
                {
                    newPos = _MCL.VelocityModel.MoveExact(currentPose, driveCommand.Velocity, new TimeSpan(ticks * j));
                }
                currentPose = newPos;
            }
        }
        _particles[amount - 1].position = new Vector3((float)currentPose.X, (float)currentPose.Y, 0.0f);
        _particles[amount - 1].color = Color.blue;
        _particles[amount - 1].size = 1.0f;
    }

    private void _InitializeMap()
    {
        string mapDefinition = @"
        0,4
        0,7
        2,7
        2,8
        14,8
        14,0
        10,0
        10,4
        4,4
        4,5
        2,5
        2,4
        ";
        _map = ProbabilisticRobot.Map.ParseMultiLinePointsString(mapDefinition);
        Point prev = null;
        foreach (var p in _map.Points)
        {
            Point start;
            if (prev != null)
                start = prev;
            else
                start = _map.Points[_map.Points.Length - 1];
            var go = Instantiate(LinePrefab);
            go.GetComponent<LineRenderer>().SetPosition(0, new Vector3((float)start.X, (float)start.Y, 0.0f));
            go.GetComponent<LineRenderer>().SetPosition(1, new Vector3((float)p.X, (float)p.Y, 0.0f));
            prev = p;
        }
    }

    private void _InitializeRobotPath()
    {
        RobotPath robotPath = new RobotPath(new Pose(0.5, 6, Angle.FromDegrees(0)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(2)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(-90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(-90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(-90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(3)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(3)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(90), TimeSpan.FromSeconds(1)));
        robotPath.DriveCommands.Add(new DriveCommand(0.5, Angle.FromDegrees(0), TimeSpan.FromSeconds(4)));

        this._path = robotPath;
    }

}
