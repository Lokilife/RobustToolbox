using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;
using Robust.UnitTesting.Server;

namespace Robust.Benchmarks.Physics;

[Virtual]
[MediumRunJob]
public class PhysicsBoxStackBenchmark
{
    private ISimulation _sim = default!;

    [GlobalSetup]
    public void Setup()
    {
        _sim = RobustServerSimulation.NewSimulation().InitializeInstance();

        var entManager = _sim.Resolve<IEntityManager>();
        entManager.System<SharedMapSystem>().CreateMap(out var mapId);
        SetupTumbler(entManager, mapId);

        for (var i = 0; i < 30; i++)
        {
            entManager.TickUpdate(0.016f, false);
        }
    }

    [Benchmark]
    public void BoxStack()
    {
        var entManager = _sim.Resolve<IEntityManager>();

        for (var i = 0; i < 10000; i++)
        {
            entManager.TickUpdate(0.016f, false);
        }
    }

    private void SetupTumbler(IEntityManager entManager, MapId mapId)
    {
        var physics = entManager.System<SharedPhysicsSystem>();
        var fixtures = entManager.System<FixtureSystem>();

        var groundUid = entManager.SpawnEntity(null, new MapCoordinates(0, 0, mapId));
        var ground = entManager.AddComponent<PhysicsComponent>(groundUid);

        var horizontal = new EdgeShape(new Vector2(-40, 0), new Vector2(40, 0));
        fixtures.CreateFixture(groundUid, "fix1", new Fixture(horizontal, 2, 2, true), body: ground);

        var vertical = new EdgeShape(new Vector2(10, 0), new Vector2(10, 10));
        fixtures.CreateFixture(groundUid, "fix2", new Fixture(vertical, 2, 2, true), body: ground);

        var xs = new[]
        {
            0.0f, -10.0f, -5.0f, 5.0f, 10.0f
        };

        var columnCount = 1;
        var rowCount = 15;
        PolygonShape shape;

        for (var j = 0; j < columnCount; j++)
        {
            for (var i = 0; i < rowCount; i++)
            {
                var x = 0.0f;

                var boxUid = entManager.SpawnEntity(null,
                    new MapCoordinates(new Vector2(xs[j] + x, 0.55f + 1.1f * i), mapId));
                var box = entManager.AddComponent<PhysicsComponent>(boxUid);

                physics.SetBodyType(boxUid, BodyType.Dynamic, body: box);

                shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);
                physics.SetFixedRotation(boxUid, false, body: box);
                fixtures.CreateFixture(boxUid, "fix1", new Fixture(shape, 2, 2, true), body: box);

                physics.WakeBody(boxUid, body: box);
                physics.SetSleepingAllowed(boxUid, box, false);
            }
        }

        physics.WakeBody(groundUid, body: ground);
    }
}