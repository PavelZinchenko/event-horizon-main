# Combat Simulation

`Assets/ModulesShared/CombatSimulation/` is the new battle simulator. It will replace `Assets/Modules/BattleSimulator/` ([battle simulator](battle_simulator.md)). Event Horizon and Frontier share one copy through the `ModulesShared` submodule ([Gui: Shared code constraint](gui.md#shared-code-constraint)).

Status: in progress. Only the physics library exists. Stage P1 is closed (2026-10-17, with the known limits below). It has the stage P1 features: torus broad phase, circles, convex polygons, a soft-step contact solver with speculative contacts, and a free-position query. Stage P2 is closed (2026-10-18, with the known limits below): capsules, convex decomposition, compound bodies, whole-body depenetration, inner-edge contacts and the time-of-impact check of speculative contacts. Swept bullets, ray casts and joints come in stages P3–P4. The plan and its progress are in `TODO/battle_simulator_implementation.md`. The findings and decisions are in `TODO/battle_simulator_investigation.md`. The `TODO/` folder is local and is not part of this repository.

## Assemblies

| Assembly | Folder | References |
| --- | --- | --- |
| `CombatSimulation.Physics` | `Physics/` | `Utilites` |
| `CombatSimulation.Physics.Tests` (Editor) | `Physics/Tests/` | `CombatSimulation.Physics`, `Utilites` |

Rules:

1. `CombatSimulation.Physics` is engine-free (`noEngineReferences: true`). Follow the [engine-free module rules](architecture.md#engine-free-modules).
2. The new core and the old `BattleSimulator` do not reference each other, in either direction.
3. The core may reference `Database` and `ShipConstructor` by assembly name, as `ShipEditor` does. Each game supplies its own copy, so the core uses only types and fields that both games have.
4. Unit tests go into `Physics/Tests/` with an Editor-only asmdef, and `Tests~/EngineFree.Tests.csproj` compiles them.

## Physics library

`PhysicsWorld` owns the body state. The simulation reads and writes bodies through `BodyId` handles and keeps no copy (investigation 1.7). One `Step(dt)` runs these phases in order:

1. Compute the motion margin of each body from its current velocity.
2. Rebuild the torus grid and find the pairs.
3. Merge the pairs into the persistent contacts.
4. Update the manifolds.
5. Continue, end and start whole-body depenetrations.
6. Run the soft-step solver.
7. Set the touching flags and the begin/end events.
8. Write back the poses.

Conventions:

| Item | Rule |
| --- | --- |
| Coordinates | Canonical torus coordinates in `[0, Width) × [0, Height)`. `Torus.Delta` gives the shortest offset across the wrap; every pair test and every impulse point uses it. |
| Units | Game units and seconds. Angles are in radians (the game uses degrees, so the core converts). |
| Position | `GetPosition` is the pivot (body origin), as in Unity. Velocity and impulses act on the centre of mass, which follows from the shapes. |
| Mass | Set by code (`BodyDef.Mass`, `SetMass`). Inertia comes from the shapes, scaled to that mass. |
| Flags | `FixedPosition`: infinite mass, and the origin is never written, so it stays exact to the last bit. The body turns about its origin. `FixedRotation`: infinite inertia. |
| Material | `ShapeDef.Default`: friction 0.2, restitution 0.2. Mixing: friction √(a·b), restitution max(a, b). |
| Speculative contacts | A pair gets manifold points up to `4 × LinearSlop + motion of both bodies in the step`. The solver lets a positive gap close in the step, and no more (owner decision 8). A time of impact clears the points of a pair that does not meet in the step ("Ghost passes" below). |
| Manifold timing | `Step` computes manifolds before the solve, from the poses at the start of the step. After a step, `GetContact` describes the previous poses, not the new ones. |
| Touching | A contact touches when a point is closer than `LinearSlop`, or when the solver pushed in the step. A fast hit can bounce on a speculative point before the gap closes, and it still counts. |
| Allocation | The step allocates nothing once its buffers have grown to the scene size. |
| Determinism | The same inputs on the same build give bit-identical results. Cross-platform equality is not a goal (investigation 6.2). Measured in P1 and P2 (P2-4: 358 scene hashes): the gate scenes give bit-identical results on desktop .NET (x64) and Android IL2CPP (ARM64); a compiler change can break this. |

Shapes:

| Shape | Use | Notes |
| --- | --- | --- |
| `Circle` | Small drones, shields, bullets | |
| `Polygon` | Convex, counter-clockwise, at most 8 vertices | `FromPoints` builds the convex hull. `Radius` is the rounding radius: 0, except for a capsule core. |
| `Capsule` | Segment plus radius (Unity `CapsuleCollider2D`; `Capsule.FromSize` converts its size and direction) | The polygon manifold treats it as a rounded 2-vertex polygon. |

A compound body is a body with several shapes. Mass, centre of mass and inertia sum over the shapes, and the broad phase never pairs two shapes of one body. Contacts and events are per shape pair, so one hull-to-hull hit can give several begin events.

`ConvexDecomposition.Decompose` splits a concave outline into convex parts once, at load ("load-time split", owner decision 1). Each part becomes one polygon shape of the body.

1. Input: one outline path in body units. Scale it first, because `Polygon` has an absolute minimum edge length (0.001). A collider with several paths is decomposed path by path.
2. Cleanup: welds near-duplicate points, drops collinear points, accepts either winding. A self-intersecting outline throws.
3. Bayazit's split at each reflex vertex, then a Hertel–Mehlhorn merge of neighbouring parts while the union stays convex and within 8 vertices. The parts cover the outline exactly.
4. Outline data: each part knows which of its edges are cuts (inside the outline, between two parts) and the outline normals at each vertex. `ConvexDecomposition.AttachOutline` adds the same data to parts made by hand. A part edge that continues a cut along the outline in one straight line counts as outer.
5. Deterministic: plain arithmetic in a fixed order, so the same outline gives the same parts on every run of the same build.
6. It allocates; it is for loading, not for the step.
7. Holes: a clockwise path inside another path is a hole in Unity. The hull loader must skip it, so the outer path makes the hole solid (the harness does this; 2 holes in `mothership`). Decomposing a hole as its own path puts overlapping parts on top of the outer path.

Measured on the 124 ship outline paths (ship prefabs, scaled × 2): all decompose; parts per path median 4, 90th percentile 9, max 33 (`mothership`); area error at most 0.005%.

Polygon manifold (`Collide.Polygons`, adapted from Box2D v3, with radius support). When the shapes are apart:

1. If the incident edge overlaps the reference face along the face, it is clipped against the side planes of the face, widened by the speculative distance. A corner past the end of the face keeps its own point, measured against the face plane. Box2D v3 does not widen. Here the speculative distance grows with the motion, so without the widening both points of a corner hit near the end of a face fall on the face end, and the one-sided push spins the body (seen in P2: a drone hitting an octagon vertex at 40 u/s spun at 134 rad/s and sank in 123%).
2. If the incident edge lies wholly beside the face (corner past corner), one point lies between the closest features of the two edges, with the true gap.

Inner edges (`Collide`, "Outline parts"). A contact with a part of a concave outline never takes its normal from a cut. A body that slides across a seam between two parts would otherwise catch on the cut: in P2-1 a probe lost 90–102% of its speed on 4-part bars. Allowed normals are the outer face normals and, at a vertex, the directions between the two outline normals there. Polygons without cut edges take the plain code path, unchanged.

1. Polygon against circle: an outer face works as for any polygon. At a vertex, the direction to the centre is clamped into the vertex's outline normals; on a straight seam this gives the neighbour's face normal. A centre beyond a cut gets no point, because the neighbour part holds the whole cut. A centre inside the part gets the outline axis with the largest separation.
2. Polygon against polygon: the separating-axis test still culls pairs that are too far apart. When its axis is an outline direction of both polygons, the plain manifold follows. When a cut faces the other polygon, the outline axis with the largest separation replaces it (outer faces and outline vertex normals of both, measured with the support points of both). Apart across a cut, one point between the true closest points stays when their direction is an outline direction of both; otherwise the neighbour part across the cut is at least as near. An outline axis is not used as a speculative normal there: beyond a cut its gap along a sideways outer face is small, and such a point stopped f6s1 sideways at 140 u/s, which then sank 154% into the mothership (P2-3 trial).
3. Result (P2-3): 0.0% loss on the 4-part bars with straight and 45° cuts, for box and circle probes at 5, 40 and 140 u/s.
4. Not covered: seams between different colliders of one hull (polygon and circle, capsule or box). Their edges are not cuts. On the test hulls every such junction is a concave corner (52–90°), a real wall.

`WorldSettings` holds the torus size and the solver tuning: cell size, substeps, contact hertz and damping ratio, push speed, slop, restitution threshold, and the depenetration trigger. The defaults are the Box2D v3 values, except three that are tuned for drone crowds at the database thrust caps: 8 substeps (Box2D: 4), contact 100 Hz (30) and push speed 5 u/s (3). The solver caps the contact hertz at a quarter of the substep rate, so fewer substeps also make the contacts softer. The game sets these values in the project configuration at build time, not in the database (owner decision); the core passes them to `PhysicsWorld`.

`TryFindFreePosition` places spawns and teleports without overlap (owner decision 9). It tests every shape (no grid), because spawns and teleports are rare.

Whole-body depenetration (`Separate`, `Dynamics/Separation.cs`) separates two overlapping bodies as whole bodies. Inside a compound hull, each part pushes along its own normal, and the pushes can cancel each other: a captured drone stays wedged. Depenetration replaces all contacts of the pair with one push along the shortest way out.

1. Start: the core calls `Separate(a, b)` when it knows about the overlap, for example on capture (owner decision 2026-10-18: no free-position jump on capture). It also starts on its own when a contact stays deeper than `SeparationDepth` (25% of the smaller shape's bound radius) for more than `SeparationSteps` (10) steps, and one of the two bodies is compound. These are the fast-hit limit values. A pair of single convex shapes is left to its contacts: it has one consistent normal, and in the P1 crowd such pairs are deep only because other drones press them together.
2. Direction: 16 fixed directions in the frame of the heavier body, plus the line between the centres. For each, the clearance is the shortest move of the lighter body that frees the pair. Each shape pair overlaps in one interval of the move; separating axes give the interval (exact for polygons, a little long for round shapes), and the clearance is the first move that no interval covers. The shortest clearance wins; a tie goes to the lower candidate.
3. Push: the solver skips the pair's contacts and solves one constraint instead: normal along the direction, anchors at the centres of mass (no torque), no friction, no bounce, separation minus the clearance. It pushes at most at `PushMaxVelocity`, and the relax pass removes the push velocity, so the body slides out and does not launch. The constraint also blocks motion back into the hull.
4. End: when the pair overlaps by 5% of the smaller body's radius or less; the contacts handle the rest. When the clearance does not drop for 1 s, the direction is chosen again once; then it ends and `FailedSeparationCount` grows.
5. Cost (desktop): start 0.16 ms for a drone in the mothership, 4.9 ms for a mothership inside another one (40 × 40 parts); per step while it runs, up to about 0.3 ms for the hull pair.

Ghost passes (`Dynamics/SpeculativeContacts.cs`, `Collision/Distance.cs`, `Collision/TimeOfImpact.cs`). A speculative point takes its normal from the closest features at the start of the step, and the solver stops any approach along that normal. A body that passes close by, but does not touch, would stop too: in P2-1 passes at a gap of 0.05 lost 31–113% of their speed. Before the solve, a time of impact checks such contacts and clears their points for the step when the shapes do not come within the speculative distance (owner decision 2026-10-18: keep the motion-based margin, add a time of impact only where needed).

1. Which contacts: all points speculative (gap above `4 × LinearSlop`), and the gap can close within the step plus one substep, from the velocities, forces, damping and spin of both bodies. The extra substep is the relax pass after the last substep, which already stops an approach into the next substep. Other contacts keep their points, so the solver result does not change for them.
2. Free motion only: the time of impact assumes that no other contact acts on the two bodies. An island is a group of movable bodies linked by contacts with points; a body that can neither move nor turn links nothing. When an island holds a contact that the solver acts on (touching, deep, a time-of-impact hit, a depenetration), no contact in it is cleared. Without this rule, crowds pushed drones into neighbours whose contacts were cleared (2 blow-ups to 76% in the 40-position crowd sweep), and a hull that hit with one part turned another part into the target.
3. Time of impact: GJK distance and conservative advancement on separating axes (adapted from Box2D, `b2ShapeDistance`, `b2TimeOfImpact`). The path is the solver's own free motion: the poses after each substep, a straight line between them and a normalized lerp of the rotation. A pair that only translates on one line is checked over the whole step in one go; the separation along a fixed axis is then concave, so the ends of the interval bound it. Otherwise the step is split at the substeps, and up to 4 times more while the bodies turn; the largest dip that the turn can add within an interval is added to the target.
4. Result (P2-4): every ghost-pass row passes (0.000% speed change, no begin event, 8 directions × 24 hull pairs, 8 reference passes). A brute-force check of about 16,000 cleared contacts (2,000 distance samples each, fast hit, fast spin, ghost pass, crowd, impact and cost scenes) found no pair that came closer than the target.
5. Cost (desktop): the step-cost scenes check 0 contacts per step at the median, 5 at most; step time unchanged (0.145 ms median). `TimeOfImpactCount` and `DroppedSpeculativeCount` give the last step's numbers.

Known limits:

1. A polygon against a circle gives one manifold point. A polygon that spins fast after a glancing hit can swing another corner into the circle for one step (P1 impact results).
2. When an incident edge overlaps the reference face only partly, the point of its corner past the face end measures against the face plane. This is conservative: a body can stop slightly early (ghost collision), never late. Corner past corner measures the true gap. The time of impact removes these early stops for bodies in free motion; in an island with an active contact they stay. Example: a ship that passes close to a hull while drones rest on that hull is still braked.
3. An AABB must be smaller than half the torus on each axis.
4. Soft contacts give way under load: penetration grows with the force (P1 crowd results).
5. The solver handles each contact once per substep, in a fixed order. When other contacts push the same body in the same substep, a fast pair can pass its speculative limit and overlap deeply for 1–3 steps; it then separates at no more than the push speed. Seen when crowd drones bounce off a target into drones that still arrive (P1 arrival peak).
6. Fast spin: a hull whose tip moves at 140 u/s (f6s1 at 233 rad/s, flagship4 at 47 rad/s) can sink 31–45% for one step. The manifold point comes from the start of the step, and a fast turn brings another feature in. Before P2-4 the ghost stop of the previous step braked some of these spins early (flagship4: 0%).

Third-party code: [THIRD_PARTY.md](../Assets/ModulesShared/CombatSimulation/Physics/THIRD_PARTY.md) (Box2D, MIT).

## Gate harness

`Experimental/CombatSimulationHarness/` is a throwaway .NET console project. It compiles the physics sources directly, runs the gate scenes of the physics stages and checks their limits. It is outside `Assets/` and is not in Git. Game code never references it.

```
dotnet run -c Release --project Experimental/CombatSimulationHarness
```

It prints a table of measured values against the limits, and it writes the same table to `Results/P1-<date>.md` with the harness version and a state hash per scene. Two runs of the same build give the same hashes. Each scene runs in the arena middle and on the wrap corner. With `-- --replay` it also records each scene, and `Experimental/CombatSimulationHarness/Viewer/index.html` plays the recordings step by step (harness README).
