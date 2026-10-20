# Combat Simulation

`Assets/ModulesShared/CombatSimulation/` is the new battle simulator. It will replace `Assets/Modules/BattleSimulator/` ([battle simulator](battle_simulator.md)). Event Horizon and Frontier share one copy through the `ModulesShared` submodule ([Gui: Shared code constraint](gui.md#shared-code-constraint)).

Status: in progress. Only the physics library exists. Stage P1 is closed (2026-10-17, with the known limits below). It has the stage P1 features: torus broad phase, circles, convex polygons, a soft-step contact solver with speculative contacts, and a free-position query. Stage P2 is closed (2026-10-18, with the known limits below): capsules, convex decomposition, compound bodies, whole-body depenetration, inner-edge contacts and the time-of-impact check of speculative contacts. Stage P3 is closed (2026-10-19, with the known limits below): collision filters, bullets with swept hits and pass-through, ray casts and circle overlaps. Stage P4 is closed (2026-10-19, with the known limits below): hinges, ropes, bullets attached to bodies, group teleport, and contact and joint impulses. The plan and its progress are in `TODO/battle_simulator_implementation.md`; stage results in `TODO/battle_simulator_physics.md`. The findings and decisions are in `TODO/battle_simulator_investigation.md`. The `TODO/` folder is local and is not part of this repository.

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
6. Prepare the joints, then run the soft-step solver (joints, then contacts, in each stage).
7. Set the touching flags and the begin/end events.
8. Write back the poses.
9. Sweep the bullets against the bodies' motion of this step and against each other, then move them (`BulletHits`); attached bullets move with their parent. Skipped without bullets.

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
| Determinism | The same inputs on the same build give bit-identical results. Cross-platform equality is not a goal (investigation 6.2). Measured in P1 and P2 (P2-4: 358 scene hashes): the gate scenes give bit-identical results on desktop .NET (x64) and Android IL2CPP (ARM64); a compiler change can break this. P3-1: 454 of 460 match; the energy-shield bullet scene and the bullet cost scene differ (same verdicts). |

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
2. Free motion only: the time of impact assumes that no other contact acts on the two bodies. An island is a group of movable bodies linked by contacts with points and by joints; a body that can neither move nor turn links nothing. When an island holds a contact that the solver acts on (touching, deep, a time-of-impact hit, a depenetration) or a joint, no contact in it is cleared. Without this rule, crowds pushed drones into neighbours whose contacts were cleared (2 blow-ups to 76% in the 40-position crowd sweep), and a hull that hit with one part turned another part into the target.
3. Time of impact: GJK distance and conservative advancement on separating axes (adapted from Box2D, `b2ShapeDistance`, `b2TimeOfImpact`). The path is the solver's own free motion: the poses after each substep, a straight line between them and a normalized lerp of the rotation. A pair that only translates on one line is checked over the whole step in one go; the separation along a fixed axis is then concave, so the ends of the interval bound it. Otherwise the step is split at the substeps, and up to 4 times more while the bodies turn; the largest dip that the turn can add within an interval is added to the target.
4. Result (P2-4): every ghost-pass row passes (0.000% speed change, no begin event, 8 directions × 24 hull pairs, 8 reference passes). A brute-force check of about 16,000 cleared contacts (2,000 distance samples each, fast hit, fast spin, ghost pass, crowd, impact and cost scenes) found no pair that came closer than the target.
5. Cost (desktop): the step-cost scenes check 0 contacts per step at the median, 5 at most; step time unchanged (0.145 ms median). `TimeOfImpactCount` and `DroppedSpeculativeCount` give the last step's numbers.

Bullets, filters and queries (P3; `Dynamics/Bullet.cs`, `Dynamics/BulletSweep.cs`, `Collision/ShapeCast.cs`, `Queries/Queries.cs`).

| Item | Rule |
| --- | --- |
| Filter | `ShapeDef.Filter` and `BulletDef.Filter`: `Categories` and `Ignore` (64 bits each; the core puts side × class in them). Two filters pair unless one ignores a category of the other. All zero pairs with everything, so a `ShapeDef` without a filter behaves as before filters existed (the inverse of the Box2D mask, where a zero mask pairs with nothing). The filter applies to solid contacts (in the broad phase), bullet hits and queries. `SetShapeFilter` switches a shape at run time (a shield on and off). |
| Bullets | `CreateBullet` with one circle, polygon or capsule. Not bodies: the solver never sees them (about 2,000 of the 2,130 R2 bodies are bullets, and they need detection only). The core sets the velocity (homing); the world moves a bullet on a straight line and never turns it. The `Owner` body is the bullet's broad-phase group, so a bullet never hits its own ship, and bullets of one ship never hit each other. The core destroys bullets. |
| Sweep | After the solve, each bullet is cast along its motion relative to each candidate: a body keeps its start rotation and moves its centre on a line from its start to its end pose (P3 owner decision 3); two bullets both move on their lines. Shapes that overlap at the start hit at fraction 0. |
| Target unit | Hits are counted per unit: the shapes of one body with the same filter categories are one target (the parts of a hull), a shield shape with other categories is another. A bullet without pass-through reports its first hit only; a pass-through bullet reports the first hit on each unit in the step. `Begin` is set when the bullet did not hit that unit in the previous step. |
| Order | `BulletHits` is sorted by fraction, then bullet index, then target (bodies before bullets, by index), then shape index (contact ordering, investigation 1.5; the core maps indices to entity IDs). Each bullet of a hit pair gets its own hit on the other. |
| Shape cast | Newton steps on the GJK distance of the cores. For a pure translation the distance is convex in time, so each step lands at or before the first contact and never passes through. A flat face converges in one step. Tolerance: a gap of 1e-5 u plus the float resolution of the cast frame (computed near the origin, not at map coordinates); a tolerance relative to the bullet size let large bullets report a graze, early, as their hit. A step below 1e-6 of the translation counts as converged, so float noise never turns a contact into a miss. |
| Contact normal | The direction between the closest core points while they are apart (always for circles and capsules). When a vertex lands exactly on a face, the cores touch and that direction is noise; the face normal with the largest separation then gives the normal (separating axis test, cut edges excluded). Polygon manifolds are not used for this: they prefer a face of A within a tolerance, also when a vertex of A touches a face of B. Shapes that overlap at the start take the manifold normal and its deepest point. |
| Ray cast | `RayCast`: a point cast through the query grid (grid walk in unwrapped cell coordinates). The ray is clipped to `min(Width, Height)`, each target is tested in each torus image that its cells cover and keeps its nearest hit, and the `ignore` body (the shooter) is never hit. Without pass-through it returns the first hit; with pass-through up to `hits.Length` (8 in the game), the nearest, and an overflow flag. A ray that starts inside a target hits it at 0, with the reverse ray direction as the normal. |
| Overlap | `OverlapCircle`: exact distance to each convex part (not a bounding circle), each unit once at its nearest image, nearest surface first, up to `results.Length` (32) with an overflow flag. |
| Query grid | Tight boxes of the current poses of shapes and bullets, rebuilt on the first query after a step or after a change of shapes, poses or bullets. |
| Allocation | The step and the queries allocate nothing once their buffers have grown. A new peak still grows a buffer once (P3 cost scene: the sweep pair buffer, 2,048 → 4,096 pairs, 32 KB, in one of 1,000 steps). |

Joints (P4; `Dynamics/Joint.cs`, `Dynamics/JointSolver.cs`).

| Item | Rule |
| --- | --- |
| Hinge | `CreateHinge(HingeDef)`: a shared point of two bodies (Box2D v3 revolute joint, without spring and motor; P4 owner decision 2). Anchors are body-local and relative to the body origin, as Unity anchors. An optional limit holds the joint angle within `[LowerAngle, UpperAngle]`, both within [-π, π]. |
| Rope | `CreateRope(RopeDef)`: the distance between two anchors never exceeds `Length` (Box2D v3 distance joint with only the upper limit: no spring, no lower limit, never rigid). It only pulls. While slack it acts as a speculative constraint: the ends may reach the rope length within the substep, but no more; it applies no impulse otherwise. `SetRopeLength` reels in or out; a shorter rope pulls the ends together at the joint softness. `GetJointKind` tells hinges and ropes apart. |
| Joint impulse | `GetJointImpulse`: the sum of the linear impulses that the joint applied to body B in the last step, through its anchor (warm start and solve; body A got the negative). Hinge limits apply angular impulses only, which the sum leaves out. |
| Joint angle | Angle of B relative to A minus `ReferenceAngle`, in (-π, π]. The caller always passes the reference angle; the library never takes it from the current poses (P4 owner decision 3). Today's `HingeJoint2D` takes it at creation, before the rigidbody sync, which is a candidate cause of the tail that coils at an impossible angle. |
| Solver | Soft step, as the contacts: the point constraint and the limits are soft with bias (`JointHertz` 200, `JointDampingRatio` 2, the hertz capped at half the substep rate; Box2D v3 solves joints with twice the contact hertz), and the relax pass removes the bias velocity. A limit that is not reached acts as a speculative constraint: the angle may reach the limit within the substep, but no more. Joints come before contacts in each stage. |
| Seam | Prepared from `Torus.Delta` of the two centres at the start of the step; within the step only the motion of the two bodies counts. A chain across the wrap line needs no special case. Each offset across the line rounds at the precision of coordinates near the map size (about 1.5e-5 u on map 200), so a chaotic chain across the line drifts from the same chain in the middle (unit test: 5e-3 u after 60 steps, 3e-4 u away from the line). |
| Contacts | `CollideConnected` off (the default, as in Unity): the two bodies get no contact. `CreateHinge` drops the pair's existing contacts at once; touching ones end in the next step's events. |
| Islands | A joint links its bodies into one island and marks it: no contact in that island is cleared by the time of impact, because a joined body never moves freely. |
| Teleport | `TeleportGroup(body, position)` moves the body and every body joined to it by hinges (P4 owner decision 4); rotations and velocities are not written. A rope never moves its other end. `TeleportGroup(body, position, angle)` also turns the group about the body's origin and turns the velocities. Relative positions keep their value within 2 ulp of the coordinates (P4 owner decision 5). |
| Lifetime | `DestroyJoint`; `DestroyBody` destroys the body's joints. |
| Contact impulse | `ManifoldPoint.TotalNormalImpulse` and `TotalTangentImpulse`: the impulse the point applied in the whole step (Box2D v3.1 `totalNormalImpulse`), for momentum checks and ramming damage. Body B received it. |

Attachment (P4; `Dynamics/Bullet.cs`, `Dynamics/BulletSweep.cs`): a bullet that follows a body (harpoon hook, sticky mine, `AttachedToParent` ammunition). It is not a joint.

| Item | Rule |
| --- | --- |
| Pose | `AttachBullet(bullet, parent, local)` stores the pose in the parent's frame, relative to its origin; `AttachBullet(bullet, parent)` takes the current world pose. The world pose is the parent pose times the local pose. The local pose is never written back, so it stays exact to the last bit. `SetBulletPose` throws for an attached bullet. |
| Motion | Each step sweeps the bullet from its pose on the parent's start pose to its pose on the parent's end pose, with the parent's start rotation (as bodies in the sweep), and never against its parent. The velocity is that of the parent's point. A teleport of the parent (`SetPose`, `TeleportGroup`) moves it at once. |
| Lifetime | `DetachBullet` keeps the world pose and the point velocity. When the parent is destroyed, the bullet comes free at the next step with its last pose and velocity. |
| Rope to a hook | The core anchors the rope on the target at the hook's local point (`GetBulletLocalPose`); the rope anchor and the hook are then at the same point to the last bit. |

Known limits:

1. A polygon against a circle gives one manifold point. A polygon that spins fast after a glancing hit can swing another corner into the circle for one step (P1 impact results).
2. When an incident edge overlaps the reference face only partly, the point of its corner past the face end measures against the face plane. This is conservative: a body can stop slightly early (ghost collision), never late. Corner past corner measures the true gap. The time of impact removes these early stops for bodies in free motion; in an island with an active contact or a joint they stay. Example: a ship that passes close to a hull while drones rest on that hull is still braked; so is a ship that passes close to a worm.
3. An AABB must be smaller than half the torus on each axis.
4. Soft contacts give way under load: penetration grows with the force (P1 crowd results).
5. The solver handles each contact once per substep, in a fixed order. When other contacts push the same body in the same substep, a fast pair can pass its speculative limit and overlap deeply for 1–3 steps; it then separates at no more than the push speed. Seen when crowd drones bounce off a target into drones that still arrive (P1 arrival peak).
6. Fast spin: a hull whose tip moves at 140 u/s (f6s1 at 233 rad/s, flagship4 at 47 rad/s) can sink 31–45% for one step. The manifold point comes from the start of the step, and a fast turn brings another feature in. Before P2-4 the ghost stop of the previous step braked some of these spins early (flagship4: 0%).
7. Bullet sweeps ignore the target's spin within the step (P3 owner decision 3). A hull tip that turns through a bullet in one step is missed; the bullet is then inside the hull at the next step and hits it at fraction 0 there, one step late.
8. A bullet, its swept box and the boxes of its targets must stay below half the torus (limit 3). On map 50 the P3 sweep scene skips its 30 u bullets for that reason.
9. Joints have no friction or motor. A worm tail sways for seconds after the head stops; only the core's sideways damping rule removes energy.
10. Warm start after a sudden stop: in the first step after a strong turn kick ends, the warm-started joint impulses can raise the kinetic energy by a few percent (6% at the ship engine caps). Box2D v3 behaves the same.
11. A chain far faster than the database worms (engine caps: 1,200°/s) opens its joints by up to 18% and whips the tail tip past 120 u/s. The core caps the worm's turn rate.
12. A rope is stiff and inelastic: a jerk stops the far end at the rope length without a bounce.
13. Chaotic joint scenes (a swinging chain) drift apart between any two positions, also away from the wrap line, through float rounding. Seam checks compare the first steps only.

Third-party code: [THIRD_PARTY.md](../Assets/ModulesShared/CombatSimulation/Physics/THIRD_PARTY.md) (Box2D, MIT).

## Gate harness

`Experimental/CombatSimulationHarness/` is a throwaway .NET console project. It compiles the physics sources directly, runs the gate scenes of the physics stages and checks their limits. It is outside `Assets/` and is not in Git. Game code never references it.

```
dotnet run -c Release --project Experimental/CombatSimulationHarness
```

It prints a table of measured values against the limits, and it writes the same table to `Results/P1-<date>.md` with the harness version and a state hash per scene. Two runs of the same build give the same hashes. Each scene runs in the arena middle and on the wrap corner. With `-- --replay` it also records each scene, and `Experimental/CombatSimulationHarness/Viewer/index.html` plays the recordings step by step (harness README).
