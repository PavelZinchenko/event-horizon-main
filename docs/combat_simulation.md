# Combat Simulation

`Assets/ModulesShared/CombatSimulation/` is the new battle simulator. It will replace `Assets/Modules/BattleSimulator/` ([battle simulator](battle_simulator.md)). Event Horizon and Frontier share one copy through the `ModulesShared` submodule ([Gui: Shared code constraint](gui.md#shared-code-constraint)).

Status: in progress. Physics and the phase 2 core (steps 1–11) exist; phase 2 is complete: a scripted battle runs on the core rules without Unity. Migration phase 3c is complete: quick combat runs on the core in a debug scene ([Unity integration](#unity-integration)). Migration phase 4 is complete (2026-11-01): original ships, ammunition, hulls and behaviour trees run on the core, and campaign battles reach the new scene when `CombatSettings.UseNewCombatSimulator` is true. Core provides the fixed tick loop, spawn identity, the core-owned roster, roster commands, the battle result, the timer, ships with an engine, resources and control frames, the published snapshot and event batch, the contact pipeline with damage and ramming, the bullet store with the cannon, homing missile and beam, point defense, drone bays with persistent inventory, and the per-tick target grid; Runtime runs pilots in locked mode, including drone self-destruct requests. Stage P1 is closed (2026-10-17, with the known limits below). It has the stage P1 features: torus broad phase, circles, convex polygons, a soft-step contact solver with speculative contacts, and a free-position query. Stage P2 is closed (2026-10-18, with the known limits below): capsules, convex decomposition, compound bodies, whole-body depenetration, inner-edge contacts and the time-of-impact check of speculative contacts. Stage P3 is closed (2026-10-19, with the known limits below): collision filters, bullets with swept hits and pass-through, ray casts and circle overlaps. Stage P4 is closed (2026-10-19, with the known limits below): hinges, ropes, bullets attached to bodies, group teleport, and contact and joint impulses. The plan and its progress are in `TODO/battle_simulator_implementation.md`; stage results in `TODO/battle_simulator_physics.md`. The findings and decisions are in `TODO/battle_simulator_investigation.md`. The `TODO/` folder is local and is not part of this repository.

## Assemblies

| Assembly | Folder | References |
| --- | --- | --- |
| `CombatSimulation.Physics` | `Physics/` | `Utilites` |
| `CombatSimulation.Physics.Tests` (Editor) | `Physics/Tests/` | `CombatSimulation.Physics`, `Utilites` |
| `CombatSimulation.Core` | `Core/` | `CombatSimulation.Physics`, `Utilites`, `Diagnostics` |
| `CombatSimulation.Core.Tests` (Editor) | `Core/Tests/` | `CombatSimulation.Core`, `CombatSimulation.Physics`, `CombatSimulation.Runtime`, `Utilites` |
| `CombatSimulation.Runtime` | `Runtime/` | `CombatSimulation.Core`, `CombatSimulation.Physics`, `CombatSimulation.Network` |
| `CombatSimulation.Runtime.Tests` (Editor) | `Runtime/Tests/` | `CombatSimulation.Core`, `CombatSimulation.Physics`, `CombatSimulation.Network`, `CombatSimulation.Runtime` |
| `CombatSimulation.Network` | `Network/` | `Utilites` |
| `CombatSimulation.Network.Tests` (Editor) | `Network/Tests/` | `CombatSimulation.Network`, `Utilites` |

Rules:

1. Physics, Core, Runtime and Network are engine-free (`noEngineReferences: true`). Follow the [engine-free module rules](architecture.md#engine-free-modules).
2. The new core and the old `BattleSimulator` do not reference each other, in either direction.
3. Phase 2 does not reference `Database` or `ShipConstructor`. Phase 4 adds the database path using contracts shared with Frontier.
4. Unit tests go into `Physics/Tests/`, `Core/Tests/`, `Network/Tests/` or `Runtime/Tests/` with an Editor-only asmdef. `Tests~/EngineFree.Tests.csproj` compiles all four. Existing pilot tests remain in `Core/Tests/`.
5. Core grants internal access only to Runtime and Core tests. Core does not reference Network or the application.
6. Physics, Core and Runtime are `autoReferenced`, so the application adapter in Assembly-CSharp can use them. An asmdef cannot reference Assembly-CSharp, where the application contracts live.

## Network library

Migration phase 3b, step 1 is implemented. Network has no reference to Core, Physics, Runtime or Unity.
Runtime uses it through `NetworkSession` (step 6). Core remains independent of Network.

### Peer and transport contracts

1. Create one `PeerConnection` per remote peer and match. Both endpoints use the same maximum datagram size, default 1,200 bytes.
2. Supply an `ITransport` that sends and receives whole datagrams without a delivery guarantee. The caller owns its lifetime.
3. `TrySend` consumes or copies the supplied bytes before returning. A false return means local backpressure, not packet loss.
4. `TryReceive` consumes one datagram and reports its original length, even when the receive buffer is too small. The peer rejects oversized packets.
5. Call `Update(now)` regularly with finite, nondecreasing monotonic seconds. It receives packets, then sends one datagram with everything queued since the last call:
   input and unreliable sections, acknowledgements, clock probes, reliable retries and new reliable data.
6. All state belongs to the instance. Use one thread; receiver callbacks must not reenter the peer or throw.
7. Queue methods copy their payloads. `IPeerReceiver` callbacks expose borrowed spans that expire when the callback returns.
8. Use a fresh transport and peer for each match. Session negotiation, authentication, reconnects and disconnect policy remain phase 7 work.

### Channels and encoding

`ByteWriter` and `ByteReader` encode integers and floating-point bits explicitly in little-endian order.
Failed reads and writes leave their position unchanged. The codec uses no streams or per-operation heap objects.
`BitWriter` and `BitReader` pack fixed-width fields and `VarUInt` values, least significant bit first.
`VarUInt` is the Utilites `EliasGammaEncoder` code with section size 4: 0 takes 1 bit, 1–15 take 6, 16–255 take 11.
The reader rejects non-minimal codes. `Finish` zeroes the padding of the last byte.
The byte codec also has a byte-aligned `VarUInt`: unsigned LEB128, 7 bits per byte. 0–127 take 1 byte; non-minimal codes are rejected.

**One datagram per update (G3, packet format version 3).** A datagram is the version byte, then one or more sections to its end.
A section is a LEB128 header (payload length × 8 + kind), then its payload: 1 byte for payloads under 16 bytes, 2 bytes under 2,048.
Kinds are input (1), reliable data (2), acknowledgement (3), clock probe (4), clock reply (5), and plain unreliable data (6).
The receiver checks the framing of the whole datagram first, so a malformed datagram delivers none of its sections.
Then it dispatches the sections in order. `InvalidPackets` counts malformed datagrams and sections with invalid contents.
Payload contents remain opaque to Network.

1. Input and unreliable sections are queued until the next `Update`. A section that does not fit sends the datagram before it.
2. `Update` adds the clock reply, the probe and the acknowledgement, then reliable retries, then new reliable data in the space left.
   More data than one datagram holds continues in more datagrams.
3. A datagram of only acknowledgements, probes and replies waits up to 1.5 ticks (30 ms) for data to join it. Runtime sends data
   about every tick, so these sections rarely travel alone. A reply carries its own receive and send times, so the wait does not bias
   the clock estimate. Cost: an acknowledgement can arrive up to one tick later.
4. A lost datagram loses all its sections. Input copies, reliable resends and periodic corrections cover the loss.
5. A refused send (local backpressure) drops the datagram. Its reliable packets keep no sequence and wait for the next update.
6. Measured, dogfight sender A, Perfect (`Results/N-bandwidth-2026-10-25-g3.md`): 137 → 49 datagrams/s; payload 5.95 → 5.39 KB/s;
   7.2 KB/s with the 38-byte Relay header. At cap: 53 datagrams/s, 7.4 KB/s with the Relay header.

Channels:

1. **Inputs:** `TrySendInput(payload)` queues one input section; `OnInput(payload)` receives it whole. `MaxInputPayload` gives its size limit.
   Network adds no history, redundancy or record framing. Runtime owns the input encoding, change detection and copies ("Controls").
2. **Reliable messages:** the reliable channel is one ordered byte stream. Each message is a LEB128 length, then its bytes.
   The stream is cut into packets; each reliable section carries a 32-bit sequence number, then stream bytes. A message can span packets.
   A packet is cut at its first send, to the space its datagram has left, and resent unchanged. It takes all unsent bytes, or at
   least 64, or it starts a datagram. Messages queued between two `Update` calls share a packet. The unsent stream keeps growing
   while the window is full. So the window (64 packets) limits packets in flight, not messages.
   The receiver holds gaps, appends data in sequence order and delivers each complete message once, in submission order.
   Empty messages are valid. Sequence arithmetic wraps at 32 bits; fewer than half the sequence space may remain outstanding.
   Measured reason (step 9a): one datagram per message filled the window at At cap; each loss at the window base stopped sending.
3. **Acknowledgements:** the next contiguous receive sequence acknowledges all earlier packets; a mask acknowledges buffered packets after it.
   The mask travels as a `VarUInt` without its always-clear first bit: usually 1 byte, so an acknowledgement section is 6 bytes.
   The sender retries only missing packets. Duplicate packets trigger another acknowledgement, including during receive-only traffic.
   The initial retry timeout is 200 ms. Later it is twice the smoothed round-trip time, clamped to 100–1,000 ms.
   A packet sent before one the peer has acknowledged is probably lost. It is retried after 1.25 round trips instead,
   at least 50 ms and at most the normal timeout (`TickClock.FastResendSeconds`). Reordering can cause a harmless duplicate.
4. **Plain unreliable data:** `TrySendUnreliable` queues one opaque payload without history or fragmentation. `MaxUnreliablePayload` gives its size limit.
   Runtime uses it for corrections. Input and plain unreliable delivery do not wait for reliable gaps.

Packet buffers and reliable reassembly storage retain their capacities for reuse.
Capacity growth may allocate; warmed steady traffic, including retries and reassembly, allocates zero bytes in the tests.
Reliable queues survive blackouts without dropping messages. Long-blackout queue limits remain phase 7 work.
An acknowledged stream with a malformed length prefix faults the connection (`IsFaulted`); it never silently discards an application message.
A reliable section without data, or with more data than a datagram can carry, is invalid and consumes no sequence.

### Tick clock

One peer is the reference clock, selected at construction. Both peers exchange probes every 250 ms.
Four timestamps measure round-trip time excluding remote processing time and estimate the reference clock's offset.
The offset uses the lowest-round-trip sample among the latest 16 samples. Round-trip smoothing uses a weight of 1/8 for each new sample.
Duplicate or expired probe replies cannot refresh the estimate.

1. Wait for the follower's `Clock.HasEstimate`. Match setup supplies both peers with the same start epoch in reference-clock seconds.
2. Call `Clock.Start(localTime, referenceStartTime)` once. A future epoch is allowed; tick zero becomes due at that epoch.
3. After start, `PeerConnection.Update` advances the clock. `TickPosition` and `IsTickDue(nextTick)` tell Runtime which ticks to execute.
4. Runtime must execute every due tick in order with the fixed 0.02-second simulation step. Network never calls the core.
5. The reference runs at rate 1. The follower adjusts its rate toward the estimated reference time over a one-second horizon.
   The rate stays between 0.98 and 1.02. New estimates never snap or rewind the running tick position.

**Asymmetric delay:** round-trip synchronization cannot distinguish clock offset from directional latency imbalance.
The 40/120 ms test exposes a 40 ms bias, or two actual ticks, despite a stable estimate.
The owner kept the one-tick gate limit unchanged (2026-10-22). The gate must measure actual peer tick differences and report failures.

Validation (2026-10-22): 45 Network tests and all 617 engine-free tests pass.
Tests cover loopback, seeded loss, duplication, reordering, delay, a 500 ms blackout, sequence wraparound, fragmentation and zero warmed allocations.
Clock tests cover distinct clock origins, jitter, oscillator drift, pacing limits and asymmetric bias.
Network and Network.Tests compile separately with Unity 6000.0.75f1's compiler, .NET Standard 2.1 references and warnings treated as errors.
The scripted battle still ends at tick 3506; its four existing acceptance tests pass. Unity metadata is supplied; Editor import remains a separate check.

## Runtime network session

Migration phase 3b, step 6 is implemented (2026-10-23). `NetworkSession` wraps the existing runner and uses the production Network channels.
The session supports two fixed owners. Match setup supplies identical battle definitions, per-owner delays, and datagram limits.
Authentication, negotiation, disconnect handling, and Unity Relay integration remain phase 7 work.

### Session API and tick order

1. Construct `NetworkSession(setup, pilots, transport, options)`. `NetworkSessionOptions` binds local and remote owners and selects the reference clock.
   Each roster owner must match that binding. The local owner must be local in `BattleSetup`; the remote owner must be remote.
2. The session exposes `BattleId`, `Tick`, `Snapshot`, `IsCompleted`, `Clock`, and the existing roster, self-destruct, vanish and allegiance command methods.
   Those commands retain their explicit target ticks. `SubmitControls(entity, originalTick, controls)` accepts player controls without an `IPilot`.
3. `Pump(now)` exchanges network data without executing simulation ticks. It works before clock start and after completion.
   Wait for the follower's `Clock.HasEstimate`, then call `StartClock(now, referenceStartTime)` on both endpoints with the same reference epoch.
4. `Update(now)` executes every tick due under the existing paced clock. `Step(now)` explicitly executes one tick without requiring clock start.
   Both APIs take finite, nondecreasing monotonic seconds. The transport belongs to the caller.
5. Receive callbacks copy payloads into reusable queues. Before each step, Runtime decodes queued records, sends due local controls,
   and submits due controls to Core. After stepping, it queues the tick's reliable batch (hits, owner operations) and any due correction.
6. The final step also sends its batch. Continue calling `Pump` or `Update` after completion to deliver final records and acknowledgements.
   `PendingReliablePackets` exposes the remaining reliable queue. `IsFaulted` reflects the existing reliable-channel fault state.

### Controls

1. Existing pilot delay remains separate from network delay (owner decision, 2026-10-23).
   A pilot reading snapshot N schedules controls for `t = N + PilotSchedule.Delay`. Runtime sends them at t.
   Both copies apply them at `t + BattleSetup.OwnerDelay(sender)`. Pilot periods and offsets remain unchanged.
2. Player input through `SubmitControls` uses its supplied original tick directly. Controls carry course, throttle, and system bits.
   Self-destruction remains an owner command at the delayed apply tick; the resulting removal travels reliably.
3. Runtime runs pilots only for local entities. The remote copy keeps its last applied controls between frames.
   Each remote entity tracks the original tick of its last applied frame. Duplicate and older frames are discarded.
4. Future frames wait for their own apply ticks, regardless of arrival order. Among overdue frames, only the newest applies at the present tick.
   A frame scheduled for the present tick takes precedence over overdue frames. A wholly overdue press/release pair can disappear remotely.
5. Runtime assigns fresh core command IDs locally and checks the sender against the target owner.
   An inbound roster deployment can receive controls in the same tick. Missing entities cannot be controlled.
   An entity whose received removal or roster exit is overdue takes no controls: the exit applies in stage 1, before controls.
6. `BattleRunner` remains the single-player entry point, with no peer, correction traffic, or added network delay.
   The same runner performs simulation and pilot dispatch in both modes. Existing pilot timing and the single-player digest remain unchanged.
7. **Quantized, change-only input (owner decision, 2026-10-24; network sessions only).** Bandwidth step 9a measured raw input at 83% of the traffic.
   - The course is rounded to a whole degree, the throttle to 1/127 steps. The owner applies the rounded frame too, so both copies fly the same controls.
   - Hysteresis: the held degree stays while the raw course is within 0.75° of it. A course near a half-degree boundary does not alternate.
   - Runtime applies and sends a frame only when its rounded value changes. The core keeps the last frame in force, so an equal frame is a no-op.
   - A change goes into the next 3 input datagrams. Each held frame is repeated once per 25 ticks, staggered by control index.
     The repeat keeps its original tick, so a receiver that already applied it drops it. It repairs a change whose 3 copies were all lost.
   - A control index names a ship or drone. Each peer counts the sender's spawn operations (deployments, drone launches) in sequence order.
     The reliable stream is ordered, so both counts agree; no index travels on the wire. Inputs drain after the batch's reliable records.
     A record whose index has no spawn yet is dropped; a copy or the repeat delivers it later.
   - Self-destruction stays local: a frame that only sets it is applied by the owner and not sent.
   - Measured (dogfight, owner A): input 119.8 → 2.5 KB/s, 149 changes per second. Late changes now move the remote copy by a whole degree.
     At cap, correction errors grow about 50–80% and still pass; shooter advantage, already failing, grows (see the network plan).

### Session wire format and corrections

1. Runtime messages are bit-packed (`BitWriter`, `VarUInt`): input and corrections are unreliable sections, tick batches are reliable messages.
   Network sends all of one tick's messages in one datagram (G3).
2. Input uses its own bit-packed section (`InputCodec`, version 1), not this header: version (8 bits), battle ID low 32 bits, base tick,
   then records until zero padding. A record holds control index, base tick − original tick, course (9 bits; 511: none), throttle (7 bits)
   and systems. Index, tick offset and systems are `VarUInt`. The sender is the owner of every record.
   **Tick batch (reliable, G2, `SessionCodec` version 11).** One reliable message per tick holds the tick's operations in sequence
   order, then its hits. Header: version (8 bits), tick and last operation sequence (`VarUInt`). No battle ID: the connection is
   bound to one battle. Then records until zero padding: kind (4 bits from version 8, from 1), record tick − batch tick (zigzag `VarUInt`; a beam
   end record is one tick ahead), then the record fields. The sender is the owner of every record.
   - **Names by stream count.** The stream is ordered and exactly-once, so both peers count its records the same way (`StreamNames`).
     A ship or drone is named by its control index (spawn-record order). A bullet is named by its age: the latest bullet index minus
     its own; each bullet-spawn record takes the next index. A removed bullet keeps its index until the end of its batch, so a hit of
     the same tick can still name it. Operation sequences are consecutive and end at the header's value; a hit's sequence is its
     position in the stream.
   - Bullet spawn: parent (1 kind bit: control index or bullet age), device, barrel and spawn sequence (`VarUInt`), launch flag
     (1 bit). With the flag (plain projectiles, 9d P1): muzzle position per axis on the correction grid (1/1024 unit, bit count
     from the arena) and launch angle (14 bits); 51 bits in a 200-unit arena. The receiver derives the entity ID with
     `SpawnIdentity.CreateChild`; Core checks it. A child bullet (phase 5) names its parent by bullet age, has the trigger index
     as its device and barrel 0, and always has the launch flag. Its tick can be the batch tick + 1 (see Bullet triggers).
     Then a charge flag (1 bit; version 10, phase 5 step 4f) and, with it, the charge ticks of a charging weapon's shot
     (`VarUInt`; 1000 ticks take 16 bits). A child takes its parent's charge, so it never sends one.
   - Removal: 1 kind bit, reason (3 bits; `Vanish` from version 8), control index or bullet age. A drone's bay comes from its launch record.
   - Hit: attacker (1 kind bit plus index or age), target (1 kind bit, then a control index in the receiver's stream or, for a
     bullet, its 64-bit entity ID: the sender may know only a predicted copy, which no stream names), shape and device + 1
     (`VarUInt`), contact (2 bits), impact (3 bits), amount (32-bit float: the target owner commits damage from it), damage
     type (`VarUInt`, below `DamageTypeTable.MaxTypes`; version 6, D4). The target's core rejects a type its table lacks.
     Then an impulse flag (1 bit) and, with the flag, the push impulse (two 32-bit floats; version 7, phase 4 E4). Only a
     projectile's start record on a ship carries one; the target's core rejects it elsewhere and applies it on arrival.
     Then the effect (3 bits, `HitEffect`; version 11, phase 5 step 5a): `Damage`, or a drain (`EnergyDrain`, `ShieldDrain`)
     that names a ship target, with damage type 0 and no impulse.
   - Retreat: ship control index. Roster changes and drone launches are rare and keep full-width IDs, records and poses.
   - Missile target (kind 7, 9d P3): missile bullet age, a segment flag (1 bit; version 11, phase 5 step 5a), then the target
     control index + 1 in the receiver's stream (`VarUInt`; 0: no target), or with the flag a sticky bullet's tail segment as
     its 64-bit entity ID. About 1.5 bytes in a real batch.
   - Allegiance (kind 8, version 8, phase 4 D13): ship control index, then the allegiance (3 bits). About 1 byte.
   - Device use (kind 9, version 9, phase 5 step 7a): ship control index, device index (`VarUInt`, below 64), started (1 bit).
     About 2 bytes. Its tick can be the batch tick + 1 (an InfinityStone use that lethal damage started).
   - The sender throws on a record its streams cannot name or on a sequence gap: that is a session bug.
   - The receiver decodes the whole batch before it submits any record, so a malformed batch submits nothing. Its records named
     entities by count, so the stream stops there: every later batch is dropped too. Its operation sequences stay missing, so the
     result cannot finalize.
   - Measured (dogfight, owner A, Perfect): bullet spawn 39 → 1.9 bytes, removal 21 → 3.0, hit 44 → 8.2, header with length prefix
     10.5 → 7.2. Reliable channel 4.05 → 0.86 KB/s, most of it datagram framing; G3 removed that framing.
   **The batch is the watermark.** Its header says "all my records through this tick are sent; the last operation is N".
   A tick without hits or operations sends a batch only every 5 ticks (100 ms heartbeat), and every tick while a candidate
   or final result exists. Cost: a remote bullet the owner never fired leaves up to 4 ticks later in quiet periods.
   Measured reason (step 9a): a separate watermark every tick was 49 messages/s, and each message used its own datagram.
3. Owner-operation payloads preserve roster records, spawn poses and velocities, drone bay identities, removal reasons, fire identity inputs,
   retreat starts (entity and tick only), missile targets (missile, target, tick) and allegiance changes (ship, allegiance, tick). Hit payloads preserve contact boundaries, authority fields, and resolved damage.
4. **Corrections (G1 and 9d P3, `CorrectionCodec`, version 2).** One unreliable section per batch: version (8 bits), battle ID low
   32 bits, batch tick, applied hit sequence and missile count (`VarUInt`), the missiles, then bodies in increasing control index
   order until zero padding.
   The configured interval is 100, 150, or 200 milliseconds; default 100. At 50 Hz, 150 milliseconds alternates eight and seven ticks.
   - A body is named by its control index, as the gap to the previous index (`VarUInt`). No counts travel: both peers take the
     definition from the spawn record of that index.
   - Pose: position on a 1/1024-unit grid (18 bits per axis in the 200-unit arena; the bit count follows the arena size), angle 14 bits,
     linear velocity 15 bits per axis on a 1/128 unit/s grid, angular velocity 14 bits on a 1/512 rad/s grid. Velocity field 0 escapes to a 32-bit float.
   - Armor, shield, energy: no bits for a maximum of 0; 1 bit full; if not full, 1 bit "delay running" (only for a resource that recharges) and an 8-bit level.
   - Point-defense timers only, stop then query: 1 bit ready, otherwise the age in ticks (`VarUInt`). Other devices and barrels send nothing.
   - Missile (P3), in increasing bullet index order: the absolute bullet index of the sender's stream as the gap to the previous
     one (`VarUInt`; the stream age does not work on an unreliable channel), position on the body grid, angle 14 bits, a velocity
     flag and, with it, velocity as for a body. Without the flag the missile flies at `Speed` along its angle: the owner sends the
     velocity only while it differs from that by more than 1/64 unit/s. 52 bits without velocity, about 83 with it. The receiver
     skips an index it does not know.
   - Missile budget: `NetworkSessionOptions.MissileCorrectionBytes`, default 300 (a placeholder; the owner sets it from a playable
     scene), at most half of the datagram payload. Runtime takes Core's missiles in priority order while their bits fit, counting
     each with its absolute index. The missiles go in the batch's first section only.
   - A batch that does not fit continues in more self-contained sections. Both endpoints use the same datagram size, default 1,200 bytes.
     The batch byte limit defaults to 1 MiB and is configurable. An empty batch is not sent.
5. Each section is submitted on its own. Core drops a body that is not newer than the correction it holds or applied, so a late or
   repeated section changes nothing. Reading stops at the first index without a spawn record: later indices are unknown too.
   A lost section is repaired by a later correction, without reliable retransmission.
6. Measured, dogfight sender A (`Results/N-bandwidth-2026-10-25-g1.md`): corrections 13.3 → 1.8 KB/s, 273 bytes per batch for 21 bodies.
7. Invalid versions, headers, lengths, owners, enum values, booleans, or numeric payloads are rejected before submission and counted in `InvalidMessages`.
   Core retains its semantic authority checks. `SentCorrectionBatches` counts sent batches; `ReceivedCorrectionBatches` counts newer decoded batch ticks.
8. Buffers, receive slots, and scheduling lists retain capacity. Stable warmed sessions with active pilots and corrections allocate zero bytes.

Validation: 37 added cases cover codecs, input selection, delay combinations, correction fragmentation, clocks, result agreement, and actual combat traffic.
The lossless combat test includes cannon fire, homing missiles, beams, point defense, drone launches and deaths, and compares every owner operation
through the result tick with its received copy. Full gate scenes and network profiles remain steps 7-9.
All 752 engine-free tests pass. The single-player log retains SHA-256 `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`.
Physics, Network, Core, Runtime, Core.Tests, and Runtime.Tests compile separately with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production warnings are errors. Core.Tests retains its existing CS0649 fixture warning. Unity metadata is supplied; live Editor import is not verified.

## Core foundation

`BattleSetup` holds immutable battle ID, match seed, roster, arena dimensions, time limit and the active-ship limit per side. Dimensions default to 200 × 200 and must be finite and positive.
The roster is a read-only copy of the caller's slot list. Each `RosterSlotSetup` has a unique roster ID, a side (0 or 1), an owner (player index, 0 in phase 2) and a `ShipDefinition`. Each side needs at least one slot.
`ShipDefinition` holds the hull, mass, the engine, an optional engine without energy, maximum armor, the shield and energy `ResourceDefinition` (maximum, recharge rate, recharge delay), barrels and devices.

The hull is a list of `HullShape` (phase 4, D5): 1 point is a circle, 2 points a capsule, 3 or more an outline. Only circles and capsules have a radius. The `HullShape` constructor validates the shape and splits an outline with `ConvexDecomposition`, so an invalid shape throws at setup. `HullParts` lists the physics parts (`HullPart`: circle, capsule or convex polygon) in shape order, each shape's parts in decomposition order. Mass is set by the definition; the centre of mass and the inertia come from the parts. `HullRadius` is the farthest hull point from the body origin. `HullHash` hashes every part's geometry bit by bit, in order; the phase 7 handshake compares it so both duel peers build the same hull. A second constructor takes a radius and extra circles, for code fixtures.
`EngineDefinition` holds derived stats in core units: propulsion (u/s²), maximum velocity (u/s), turn rate (rad/s²) and maximum angular velocity (rad/s). Phase 4 derives them from engine power and weight.
Each `Battle` owns a separate `PhysicsWorld`. Physics state is internal; the public surface exposes setup, battle ID, tick, `IsCompleted`, `Snapshot`, `Submit(RosterCommand)`, `Submit(ControlFrame)` and `Step()`.

`Tick` identifies the next tick to execute. It starts at zero. The first `Step()` processes tick zero and leaves `Tick` at one.
The stage order is commands, systems, modifiers, motion, spawn requests, queries, physics, contacts, response, damage, deaths, structure, battle flow, publish.
Stage 3 collects the device statuses (see Device statuses). Stage 6 queries beam rays and point-defense overlaps before physics advances by `0.02f` seconds with eight substeps.
Publish writes the snapshot and event batch of the current tick, then increments it.
Runtime owns pilot scheduling and network pacing through `TickClock`.

### Core authority and outgoing hits

Migration phase 3b, step 2 is implemented. Core remains transport-free.

1. `BattleSetup` accepts an optional `IReadOnlyCollection<int> localOwners`. It copies and deduplicates nonnegative player indices.
   Omission makes every roster owner local. An empty collection makes none local. `IsLocalOwner(int)` queries the frozen set.
   Owner and side remain separate; local status never changes during a battle. Allegiance (D13) is a third, separate property: see "Allegiance and ship commands".
2. For ships and drones, the attacker's owner decides the contact. A local attacker damages a local target, shield first.
   Against a remote target, it emits a resolved outgoing hit. Since step 5 the copy also takes the damage as a prediction (see "Corrections and predicted damage").
   Remote attackers produce presentation predictions without damaging ships or drones from local contacts.
3. A bullet's owner commits every change of that bullet. A contact between two bullets, or between point defense and a
   bullet, counts if either owner's device sees it (network plan 9d; owner, 2026-10-26). Before, only the bullet owner's view
   counted, so one device could lose its missile in a collision the other never saw while the other's missile flew on.
   - Local bullets take damage, stop or disarm normally.
   - Resolver calculations remove a remote bullet by prediction, without changing its hit points. Several weak hits in one
     tick can predict a removal; weak hits across ticks do not accumulate remote damage.
   - When a contact changes a remote bullet, the device that saw it sends an outgoing hit with `TargetIsBullet` to that
     bullet's owner: a local projectile's or ray's damage (`Impact`, `Energy`; a ray sends one `DamageOverTime` record per
     tick), a local point defense's `Disarm`, or `Stop` for a remote projectile that stopped at a local bullet. The attacker is
     always the sender's bullet or ship.
   - The owner applies the record at stage 10 if its bullet is still alive: `Disarm` removes it with `Disposal`, `Stop` with
     `Death`, damage reduces its hit points. A projectile's damage counts once per attacker and bullet (a ray's once per
     tick), whether the owner's own contact or the record supplied it. A record for a bullet that is gone has no effect.
   - Accepted limits (owner, 2026-10-26): the record arrives one one-way delay later. If the bullet hits a ship before that,
     the hit stands, and the device that saw the collision takes a hit from a bullet it no longer shows. Each device runs every
     ship's point defense; a zap that only a ship's copy makes costs the real ship no energy and starts no cooldown until the
     next correction resets the copy's energy and timers.
4. Only local ships and drones commit deaths, retreat completion, forced destruction, and drone inventory losses.
   Public commands cannot deploy, retreat, or self-destruct remote entities. Side destruction affects only local slots.
   Remote drone bays do not launch automatically. Owner records supply remote deployments and launches (step 3).
5. Remote movement, weapon activation, energy, and cooldowns continue to run. Remote armor and shield do not regenerate locally.
   Owner corrections (step 5) repair them.
   Remote projectiles and beams can spawn, stop, expire, or deactivate by prediction.
   `BattleRunner` creates and runs pilots only for local ships and drones. Remote control frames remain accepted.
6. `BattleEvent.Authority` is `Committed` (zero) or `Predicted`.
   A ship-target hit follows attacker authority; a bullet-target hit or disarm follows target authority.
   Bullet spawn and removal events follow that bullet's owner, independently of the hit event.
   A committed hit against a remote ship commits the contact, not the target's health or death.
   Single-player events retain their ordering, text, and field meanings, and all are committed.

`BattleSnapshot.OutgoingHits` is a separate `ReadOnlySpan<ResolvedHit>` with the same borrowed lifetime as the other snapshot spans.
It is empty in single player. Each record copies battle ID, tick, attacker and target IDs and owners, source device index,
target shape order, contact state, impact kind, and amount. No record depends on a live attacker, device, or physics handle.
The source device index is retained on bullets and beams; ramming uses -1. Presentation `BattleEvent.DeviceIndex` keeps its existing point-defense meaning.

1. `HitImpactKind` distinguishes `Impact` (missile and ramming), `Energy` (bolt and point defense), and `DamageOverTime` (beam).
   Discrete hits carry damage and `HitContactState.Start`. Beam records carry damage per second.
   The target owner applies shield and armor rules; the sender resolves ramming damage from pre-step velocities and masses.
2. Beam contacts are keyed by attacker, device, target, and target shape. `Start` includes its tick; `End` excludes its tick.
   A held contact emits no repeated outgoing record. `Stay` is reserved; the current devices do not emit it.
   A changed target or shape ends the old interval before starting the new one. Missing contact or release ends at the current tick.
   Attacker removal after damage ends at the next tick, even when the end record is published in the removal tick's snapshot.
   Per-tick presentation hits remain unchanged.
3. Outgoing buffers reuse their capacity. Tests cover zero allocations after warm-up during beam contact churn.
   Snapshot byte measurements include outgoing records.

Validation (2026-10-22): 35 authority cases and two added scripted-battle checks; all 654 engine-free tests pass.
The complete 8,050-line single-player event/state log matches the pre-change baseline byte for byte.
Its SHA-256 is `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`; side 0 wins at tick 3506.
Physics, Core, Runtime, and Core.Tests compile separately with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production assemblies treat warnings as errors. Core.Tests retains the existing CS0649 warning for `PilotSchedulingTests.Provider.PilotedSide`.
Live Editor import verified (owner, 2026-10-27).

Step 6 adds Runtime scheduling and result agreement. The relay simulator, gate matrix, and platform run remain steps 7-11.
The existing single-player adapter needs no change because all its owners remain local.

### Inbound owner records

Migration phase 3b, step 3 is implemented. Step 2 is committed as `db162db` in the current submodule history.

1. `BattleSetup.ownerDelays` copies fixed delays by owner. Known roster owners accept 0-5 ticks; omitted values default to zero.
   `OwnerDelay(int)` exposes the value. The target uses the attacker's delay for damage over time.
   Runtime schedules input at original tick + sender delay. Core control frames carry an already-selected target tick.
2. `Battle.Submit(InboundHit)` accepts a resolved hit with a sender owner and a positive sequence number.
   Runtime supplies the sender from its peer binding. Core checks battle, owner, target authority, and numeric fields.
   A live attacker must belong to the sender; an absent attacker does not invalidate the hit.
   Bullet targets are ignored. Missing or removed ship/drone targets take no damage.
   Future hits wait for their source tick. Discrete late hits apply during the receipt tick's stage 10, shield first.
3. Damage-over-time intervals use attacker, device, target, and target shape as the contact key.
   They apply over `[start + D, end + D)`. Start/end boundaries available in the receive batch are processed before damage.
   A late start catches up through the receipt tick, or the known exclusive end, as one impact at stage 10.
   Any resulting death commits at that receipt tick. A late end stops further damage without refunding earlier extra ticks.
   Multiple completed intervals received after a blackout remain separate. Held contact needs no repeated records.
4. `Battle.Submit(senderOwner, OwnerOperation)` consumes records from the reliable ordered event stream.
   `RosterChange` includes the related entity spawn/removal in one atomic operation. `DroneLaunch` and `Remove` cover other entity changes.
   Operations carry battle ID, owner, sequence, original tick, and entity identity. Sequence numbers start at one per owner.
   Hit and operation sequences are separate. Duplicate sequences are ignored; sparse sequence tracking retains gaps without retaining the whole history.
   Roster revisions add another guard: duplicates do nothing, and later revisions wait for missing revisions before changing entities or slots.
5. Roster records copy status, reason, deployment count/order, retained armor/shield deficits, and committed damage fraction.
   The receiver preserves the owner's deployment order instead of assigning arrival order. It does not infer committed facts from predicted health.
   A destroyed slot cannot deploy. Reserve destruction creates no entity or removal event.
6. Root and drone spawn records capture the pose at creation, including linear and angular velocity and the original spawn tick.
   Drone records also carry the persistent bay and parent identity. The receiver validates derived identities and uses the agreed definitions.
   Owner choice: late ships and drones start at the recorded pose and velocity, without extrapolation or a new free-position search.
   On-time root deployments apply at stage 1, drone launches at stage 5, and removals at stage 12. Late operations apply before commands.
   Drone launches/losses update persistent inventory once. Mothership removal preserves drones, and redeployment restores their mothership association.
7. `BattleSnapshot.OutgoingOperations` contains local commits captured when they happen, including spawn/removal within one tick.
   `AppliedOperations` contains received commits applied this step, with their original owner ticks for later result agreement.
   Both spans have the snapshot's borrowed lifetime. Received operations produce committed presentation events at the local application tick and never echo outbound.
   A bullet already removed by prediction still publishes its owner's removal in `AppliedOperations`, without repeating the visual removal.
8. Reused buffers keep steady hit and beam-interval processing allocation-free after warm-up.
   Snapshot byte accounting includes both new operation spans. Single-player operation batches remain empty.

Validation (2026-10-22): 32 inbound cases; all 686 engine-free tests pass. Tests include direct exchange between two cores,
late beam boundaries, revision gaps, retained resources, drone inventory, bullet removals, and zero warmed allocations.
The single-player event/state digest remains `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`.
Core, Runtime, and Core.Tests compile separately with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production warnings are errors. Live Editor import verified (owner, 2026-10-27).

### Fire confirmations

Migration phase 3b, step 4 is committed as `ab26117`; step 3 as `60a0ac4`.
The receiver's bullet and beam set converges on the owner's set. Its copies of remote weapons still fire from predicted input and energy.

1. The owner captures one `OwnerOperation.BulletFired` per projectile and beam, in `OutgoingOperations`.
   It carries tick, entity ID, parent ID, device index, barrel index, and spawn sequence: the inputs of the derived ID.
   A plain projectile also carries its launch (`HasLaunchPose`, 9d P1): the muzzle position and launch angle in `Pose`.
   Beams and homing projectiles carry no pose.
   It shares the per-owner operation sequence, so it stays ordered with removals. Single player captures nothing.
2. `Battle.Submit(senderOwner, OwnerWatermark)` states: "all my operations through tick W are sent; the last has sequence N".
   W takes effect only when every operation sequence up to N has been submitted. Older or repeated watermarks are ignored.
   Runtime sends it as the header of each tick batch (step 6, revised in step 9b). Result agreement in step 6 reuses the same watermark.
3. Each remote ship and drone keeps 16 spawn samples: position, rotation, angle, and linear velocity at stage 5, before spawns.
   Stage 4 changes velocity, so this is the exact muzzle state the owner used. Samples exist only when the battle has remote owners.
4. A fire record applies at the end of stage 5 of its tick, after local spawns; an overdue one applies at stage 1.
   It must name a live parent of the sender and a cannon or beam device with the same barrel. The ID must match the derivation.
   A matching live copy becomes `Confirmed`. A bullet this device removed by prediction, an expired projectile,
   or a bullet whose owner removal is in the same batch is not created. The record still enters `AppliedOperations`.
5. A missing projectile starts at the muzzle of the parent's sample for its spawn tick, with spread from that tick.
   With a launch pose it starts at the owner's muzzle and angle; the inherited velocity still comes from the sample.
   **Re-aim (P1).** A live copy with a launch pose moves to the owner's line: launch pose, sample velocity, advanced from the
   spawn tick to the present without hit tests. Contacts the copy already made stay. The re-aim adds no event.
   It advances on a line, one physics step per missed tick, without hit tests or homing steering. A spawn older than the
   samples uses the oldest one. A missing continuous beam attaches to the remote beam device; a missing shot beam that has
   expired here is skipped. Its spawn event is committed.
   **Adoption (P2).** Before adding a missing bullet, the receiver looks for an unconfirmed copy of the same shot fired at
   another tick (an overdue fire input fires the copy late, with another derived ID). The copy must have the same parent,
   device, barrel and spawn sequence.
   - Continuous beam: the device's attached beam, at any start tick. The owner stopped any earlier beam before this one, so
     the attached beam is a prediction.
   - Projectile or shot beam (phase 5, step 4d): spawn tick within the weapon's shortest shot spacing − 1 ticks (max of
     cooldown and barrel cooldown), so the copy can be no other owner shot. The nearest spawn tick wins, then the earlier one. A wider window would shift every
     copy of a held burst.
   The copy takes the owner's ID, spawn tick and expiry and becomes `Confirmed`; a plain projectile is then re-aimed (P1).
   The body, beam attachment and contacts so far stay. The battle emits a committed `Rename` event (`PreviousId` → `EntityId`);
   the scene moves the bullet view to the new ID. Confirmation changes no recoil or energy; corrections own those (step 5).
   **Timer re-sync (G1).** A confirmed shot at tick T sets the copy's cannon (or shot beam) last shot to T, unless the copy's last shot is at or after
   T + cooldown (a later predicted shot). The barrel shot tick becomes the later of its value and T.
   Reason: an overdue fire input shifts a held weapon's cooldown phase for the whole burst.
6. At the end of stage 5, each live remote bullet whose spawn tick is at or below min(W, present) and is not confirmed
   leaves at once with `RemovalReason.Unconfirmed` and a predicted event. Owners never send this reason.
7. A remote bullet removed locally before confirmation (contact, point defense, expiry, beam stop) enters a predicted-removal set.
   The entry ends when min(W, present) passes its spawn tick: any confirmation for it has applied by then.
8. Owner removal of a live remote beam detaches it from the receiver's beam device. Its cooldown starts at the removal record's tick (G1).

Validation (2026-10-22): 13 fire-confirmation cases; all 699 engine-free tests pass. Tests include matching IDs and positions
on two cores, late additions at the owner's pose, the history fallback, watermark gaps, beam replacement, and zero warmed allocations.
The single-player event/state digest remains `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`.
Physics, Core, Runtime, and Core.Tests compile separately with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production warnings are errors. Live Editor import verified (owner, 2026-10-27).

### Corrections and predicted damage

Migration phase 3b, step 5 is implemented. Core still has no wire format and no send interval; Runtime adds both in step 6.

1. `Battle.WriteCorrections(owner, peerOwner, CorrectionBatch)` runs after `Step` and describes the last completed tick t.
   It writes each ship and drone of the local owner: pose, linear and angular velocity (`SpawnPose`), armor, shield and energy
   (`ResourceLevel`, G1), and one `PointDefenseTimers` per point-defense device.
   The header carries `AppliedHitSequence`: the last hit sequence from peerOwner that this core has applied by tick t.
2. `ResourceLevel`: full, or an 8-bit level (Level / 256 of the maximum) and whether the recharge delay runs. Armor rounds up
   (at most level 255), so a living ship never reads as empty; shield and energy round to the nearest level. A resource with
   maximum 0 is full; only a recharging resource has a delay.
   `PointDefenseTimers`: ages in ticks at t since the activation stop and since the last query start, or `Ready` when that timer
   no longer blocks any tick after t. A ready import only unblocks; an age sets the timer and marks the device started.
   Enabled is not sent: the copy derives it from the owner's controls and its energy. Cannons, beams and barrels follow fire
   confirmations and removals; a drone bay runs only on its owner. Retreat is the reliable `OwnerOperation.RetreatStarted`.
3. `Battle.Submit(senderOwner, CorrectionBatch)` validates and holds one correction per remote entity. A correction must name a live entity of the sender,
   carry one timer per point-defense device with ages below their ready thresholds, a finite pose, and levels valid for the resource.
   Own entities are never corrected.
   A correction that is not newer than the held or the last applied one is dropped, so a receive batch applies only the newest.
   It applies at stage 1, after received operations and before commands, once its tick has run locally.
4. Each remote ship and drone keeps 16 end-of-tick body states (stage 14) and the engine input of each tick (stage 4).
   Movement error = sent state − local state at tick t. Position uses the minimum image, the angle wraps to ±π, and velocities subtract.
   C1 (owner, 2026-10-27): the error grows while the correction waits, for example when owner and copy turn in opposite
   directions near the opposite heading. Both states at t are re-run to the last completed tick with the copy's engine input and
   free motion (the solver's integration without contacts, force or damping). The difference of the two runs is the error at each
   later tick: it is added to the present body, the body states from t and the spawn samples after t, so a later correction
   measures only the remaining error. The copy keeps its own contact and recoil response; only the error is carried.
   A correction older than the history skips movement and still applies the gameplay part.
5. Resources (G1). Each gets a candidate: the owner's level at t, recharged for the ticks from t + 1 to the last completed tick,
   then minus the replayed damage (item 8). The candidate replaces the copy's value only when they differ by more than 1/256 of
   the maximum. The delay always follows the flag: 0 clears it; 1 keeps a running local delay, or starts the full delay at t.
   A full resource has delay flag 0. Point-defense timers are set from the ages at t.
   Retreat (G1): the owner sends "retreat started" (entity, tick) on the reliable tick batch; it applies at stage 1. The copy sets the
   absolute completion tick; an overdue record turns off its engine and devices from receipt. The owner's roster record completes it.
6. Prediction (N3). A local attacker's hit on a remote ship or drone also damages the copy at once, shield first.
   A predicted armor of 0 never kills; the owner's records decide deaths and roster facts. Remote shield and armor still do not regenerate locally.
7. `ResolvedHit.Sequence` numbers outgoing records per attacker owner from 1; `InboundHit` uses it and has no own sequence.
   The predicted-damage ledger keeps each sent discrete hit and each beam interval until the target owner acknowledges it.
8. After a correction with acknowledgement A at tick t, the copy replays the damage and drains the owner has not applied:
   - A discrete hit replays while its sequence is above A.
   - A beam interval [s, e) shows rate × (min(e, present) − s) locally. The owner applies [s + D, e + D) with the shooter's delay D.
     The replay is the shown ticks minus the owner's ticks through t; the owner counts only the boundaries it has acknowledged.
   - Entries leave when acknowledged (a beam: when its end is acknowledged and t + 1 ≥ e + D) or when the target is removed.
9. Without remote owners, nothing is recorded or replayed. Correction, ledger and history buffers allocate nothing after warm-up.

Validation (2026-10-22): 16 new cases; all 715 engine-free tests pass. Tests cover the seam, a history shift that applies an error once,
stale and held corrections, out-of-history corrections, recharge after a correction, retreat, device timers, rejected records,
late-input drift, discrete and beam replay with D = 3, and zero warmed allocations.
The single-player event/state digest remains `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`.
Physics, Core, Runtime, and Core.Tests compile separately with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production warnings are errors. Live Editor import verified (owner, 2026-10-27).

### Homing missile sync (9d P3)

Plain projectiles fly straight, so their spawn and launch pose fix their path (P1). A homing copy steers on the receiver's
world, so its target choice and its path drift from the owner's. Measured without P3 (`Results/N-homing-drift-2026-10-26-p3a.md`,
At cap): position error p95 1-3 units by 2-4 s of flight, and 1-7% of the samples on another target, with errors of 10-100 units.

1. **Target records.** Each change of a homing projectile's target at an owner search (every 2 s, and at once after a target loss)
   captures `OwnerOperation.MissileTargetChanged` (missile, target or null, tick). Single player captures nothing.
   On the receiver it applies before the steering of its tick (stage 4); an overdue one at stage 1. The copy takes the target and
   the owner's schedule: next search at tick + 2 s. Between records the copy searches locally. A record for a missile that is
   already gone commits without effect; a target that is gone here makes the copy search at once.
2. **Search phase on adoption.** A copy adopted under P2 moves its next search by the same amount as its spawn tick.
3. **Corrections.** `WriteCorrections` lists every live homing projectile of the owner (`CorrectionBatch.Missiles`), highest priority
   first. A missile never corrected comes first, older first: the early correction after spawn. Otherwise the priority is the ticks
   since its last sent correction times its weight: 4 within one second of flight of a ship of the peer or inside that ship's
   point-defense range, 1 elsewhere. Equal weights give round-robin. Runtime reports each sent missile back
   (`MissileCorrectionSent`). Ships and drones keep every-batch corrections; the interval is shared.
4. **Replacement.** A held missile correction applies at stage 1, after ship corrections, once its tick has run locally. The copy
   takes the owner's position, angle and velocity at tick t, then the homing steering runs again from t + 1 to the last completed
   tick toward the target's present position, without searches or hit tests (`HomingSteering.Advance`). There is no per-missile
   history, so a correction cannot overshoot. Contacts the copy made stay; no event. An older or repeated correction is dropped.
5. The observer reports a missile correction with `IsMissile`; the position and angle errors are the change of the present state.

Measured (`Results/N9d-P3.log`, At cap, seeds 1-3): Homing shooter advantage 0.003-0.018 (P2: up to 0.069), hit position p95
0.20-0.24 radius (P2: 0.51-0.84). The HomingDrones scene (fixture N8-5, 20 drones per side as extra targets; not in the gate
matrix): shooter advantage 0.11-0.18 → 0.030-0.066, hit position p95 1.4-3.0 → 0.36-0.50. Copies older than 0.26 s: target
mismatch 0%, position p95 0.05-0.20 units. About +0.4 KB/s in HomingDrones (6 live missiles). Dogfight gameplay rows are unchanged.
The MissileSwarm stress scene (fixture N8-6, about 100 live missiles per side; not in the matrix) saturates the 300-byte budget at
19-24 missiles per batch; position error p95 stays 0.05-0.32 units up to 6 s of age, and hit position p95 is 0.45-0.49 radius.
Missile-against-missile contacts: before authority rule 3 counted a contact seen on either device, a receiver removed a remote
copy at a contact with its own missile while the owner's missile flew on and hit (0.3-0.7% of the hits in MissileSwarm). With the
rule, 2-13 such hits remain per peer and seed, all within 1-14 ticks of the contact: the owner's hit came before the record.

### Identity

`BattleId` and `EntityId` wrap unsigned 64-bit values. `RosterId` wraps an unsigned 32-bit value.
They provide value equality, unsigned comparison and allocation-free hash codes. Entity ID zero means no entity.
Battle and roster ID zero remain valid. The caller supplies battle IDs; the core does not generate them from a clock.

Internal `SpawnIdentity` hashes these fields using 64-bit FNV-1a, then applies the Murmur3 `fmix64` finalizer:

| Spawn | Fields, in order |
| --- | --- |
| Root | Tag byte 1, battle ID (64 bits), roster ID (32 bits), deployment number (32 bits), spawn tick (64 bits) |
| Child | Tag byte 2, parent entity ID (64 bits), spawn tick (64 bits), source index (32 bits), slot (32 bits), sequence (32 bits) |

All numeric fields use little-endian byte order. The offset basis is `14695981039346656037`; the prime is `1099511628211`.
Arithmetic wraps at 64 bits. A zero result becomes one.
FNV-1a alone leaves each low output bit a function of the same low input bits. The finalizer is a bijection that mixes every bit, so `Value mod P` is safe for think offsets.
The source index is the device index on a ship parent and the trigger index on a bullet parent. The slot is the barrel or bay index; a bullet parent uses 0.
The sequence counts spawns with the same parent, tick, source and slot, in creation order. It covers a device that fires more than once in a tick.
On a bullet parent the sequence counts all children of the parent with the same spawn tick, across its triggers (phase 5): unique, and simpler to keep than one count per trigger. Spawn ticks must be nonnegative; a child requires a nonzero parent ID.
A roster deployment creates one root, so its spawn slot is implicit. The caller changes the deployment number for each redeployment. The spawn tick is redundant with the deployment number; it stays so that roots and children use the same tick rule.
The hash uses no allocation history, shared random sequence, or runtime hash code. Match seed affects future random behavior, not identity.
Fixed test vectors pin the byte layout. The network gate must still validate this derivation.
The `StableHash` helper holds the FNV-1a and `fmix64` code; spawn identity and counter-based random share it.

Ships are pooled `ShipEntity` objects in a `ShipStore` with a LIFO free list. The internal storage handle is kind, index and generation. `EntityRegistry` maps each live `EntityId` to its handle.
Registering an ID that is already alive is a defect. It throws when the internal `ThrowOnDefect` flag is set (tests set it). Otherwise it logs through `GameDiagnostics.Trace` and the spawn fails: the deployment is rejected and the slot stays `Ready`.
A removed entity's ID leaves the registry and never returns to life. Stages visit roster ships in setup order, then drones in entity ID order. Storage allocation order does not determine processing order.

### Counter-based random

`CounterRandom.Create(matchSeed, entityId, tick, purpose)` returns a `Utilites.RandomState` seeded with `PcgRandomAlgorithm.InitializeState` of a `StableHash` over tag byte 3, seed (64 bits), entity ID (64 bits), tick (64 bits) and purpose (32 bits).
The caller draws a fixed sequence from that state. A draw never depends on how many draws other entities made. Purposes are appended to `RandomPurpose`; existing values never change.

### Roster

A slot is `Ready`, `Active` or `Destroyed`. Its status is stored; it is never inferred from whether an entity exists. A pending deployment is a command, not a status.
Each slot counts its deployments, records its battle-wide deployment order (the last activated ship has the highest) and increments a revision on every transition.

| Transition | When | Entity |
| --- | --- | --- |
| `Ready` → `Active` | Deploy, stage 1 | Spawn with root ID `(battle, roster, deployment count + 1, tick)` |
| `Active` → `Ready` | Retreat completion, stage 12 of tick `start + 350` | Removed; the slot keeps the armor and shield deficits (B4) |
| `Active` → `Destroyed` | Death, stage 12 | Removed |
| Any → `Destroyed` | Surrender (own side) or kill-all (other side), stage 12 | Only active entities removed; reserves get no entity |

Stage 12 applies one batch in roster order: deaths, then retreat completions, then surrender and kill-all. A death in the completion tick wins over the retreat. A repeated destruction is a no-op.
A deployment in the surrender tick is destroyed in the same batch. Death has no damage source yet: the internal `QueueDeath(EntityId)` feeds stage 11, and the damage stage will use the same list.

Spawn placement follows legacy `Scene.FindFreePlace` and `ShipInfo.Create`, with counter-based random (purpose `SpawnPlacement`, the new root ID and the tick):

1. Rotation: a whole degree from 0 to 359.
2. Up to 100 uniform candidates over the torus. The first candidate at 40 units or more from every ship of the other side wins; otherwise the candidate farthest from them.
3. `TryFindFreePosition` moves the body up to 40 units to clear any overlap, allies included.

### Roster commands

`RosterCommand` carries battle ID, command ID, target tick and kind: `Deploy(rosterId)`, `Retreat(entityId)`, `Surrender(side)` or `KillAll(requestingSide)`.
`Submit` rejects a wrong battle ID, a past tick, an unknown kind, an invalid side and a command ID that is already pending. It returns `false` and logs. It throws after the result.
The buffer stays sorted by target tick and command ID. Stage 1 applies the commands of its tick in command ID order and validates them against committed state:

1. Deploy: the slot is `Ready` and its side is below `MaxActiveShipsPerSide` (default 1).
2. Retreat: the entity is a live roster ship with no retreat running. The completion tick is fixed at `tick + 350`.
3. Surrender and kill-all: the side is marked for stage 12.

A rejection logs a warning and increments the internal `RejectedCommandCount`. Phase 2 has no rejection event.

### Control frames

`ControlFrame` carries battle ID, command ID, target tick, entity ID, a course in radians or none, a throttle and 64 system bits (bit = device index).
Command IDs share one pending namespace with roster commands and self-destruct commands.
`Submit` also rejects a null entity, a throttle outside [0, 1] or NaN, a non-finite course and a second frame for the same entity and tick.
The entity is checked at stage 1, after the roster commands: a frame can name the derivable root ID of a ship that deploys in the same tick.
A frame for an unknown or removed entity is dropped, logged and counted in `RejectedCommandCount`.

1. A frame stays in force until the next frame for that entity. A deployment starts with empty controls.
2. At stage 1, before the frames, each ship copies its system bits to `PreviousSystems`. `PressedSystems` and `ReleasedSystems` give the edges; the driver sends no events.

### Ship resources

`ResourcePool` ports legacy `Energy` (`IResourcePoints.cs`) with absolute values. A pool starts full with no delay.

1. Stage 2 updates energy, armor and shield, in that order, before any device. Only the part of the step after the delay recharges.
2. A spend with a positive rate restarts the full delay. A failed `TryConsume` changes nothing. `Consume` clamps and returns the removed amount; a negative amount adds.
3. A negative rate drains (net consumption). A zero maximum means no resource.
4. Armor has rate 0: it never regenerates and never starts a delay (Q7).
5. B4: stage 12 stores the absolute armor and shield deficits on the slot at retreat completion. Deploy applies them as a spend, as legacy `ShipInfo.Create`, so a shield deficit starts the shield delay at deployment. Energy starts full.

### Engine

`ShipEngine` ports legacy `ShipEngine` in radians. The legacy formulas are linear in the angle unit, so the conversion changes no result. Stage 4 runs it for active roster ships, then drones in entity ID order.

1. The engine reads `Energy > 0` sampled at the start of stage 2, before the recharge. Without energy it uses the engine without energy; without one, the ship neither thrusts nor turns.
2. Thrust only above throttle 0.01. It steers the velocity toward `MaxVelocity` along the heading: a soft cap that keeps faster forward motion. The velocity changes by `acceleration × dt` before the physics step.
3. With a course, the legacy turn controller brakes inside 120° when the spin would overshoot, and drops to a tenth of the turn rate above the maximum angular velocity. Without a course it brakes the spin.
4. A retreating ship has its engine off and drifts (legacy `ShipRetreatEffect`).
5. Body damping is 0, as the legacy prefab drag.

### Contacts and damage

Stages 8–11 run one pipeline for every contact source. Physics only reports; the core selects the valid hits. Ramming, bullet sweeps, beam rays and point-defense overlaps share this pipeline.

1. **Shape table.** Physics shapes carry no user data. `ShapeTable` maps each live `ShapeId` to its entity, its hit priority (`Shield` before `Hull`) and its order in the definition. A lookup checks the shape generation. A stale `ShapeId` therefore resolves to nothing: the delayed end event of a destroyed body, or a reused index. Deploy adds the hull parts in `ShipDefinition.HullParts` order. That order is the shape order of hit records. Removal releases them.
2. **Candidates.** Each source adds `ContactCandidate`s to `ContactResolver` at stage 8. The sort key is a tuple, not one packed number, so the 64-bit IDs stay intact. The key order is: time (a fraction of the step), kind (`Disarm` before `Hit`), attacker ID, attacker device index (point defense; -1 otherwise), ray distance fraction, target shape priority, target ID, target shape order. Non-ray sources use zero for ray distance. Beam time stays zero; distance selects the nearest valid target within that ray. Without this field, the global sort could select a farther target by ID. Two candidates with the same key are a defect: the battle throws when `ThrowOnDefect` is set, and logs otherwise.
3. **Walk (stage 9).** The resolver walks the sorted candidates. It keeps per-bullet state for the step: stop time, disarmed, consumed own hit, damage taken. A candidate is skipped when:
   - its attacker is disarmed, has already stopped at its own hit, or stopped at an earlier time;
   - its target is a disarmed bullet, or a bullet that stopped at an earlier time.

   A target that stops at the same time stays valid, so two bullets that hit each other both take the hit. A hit on a bullet with `TargetHitPoints` above 0 adds its damage to the target's damage taken. The hit that brings it to the hit points stops the target at that time. Several weak hits in one step therefore stop a missile at the lethal one. A candidate with `OncePerTargetEntity` (damage over time and area bullets, D16) counts once per attacker and target entity per step. Ships never become invalid within a step.
4. **Ramming (Q8).** Stage 4 stores each active ship's linear velocity before the step (`PreStepVelocity`). Physics reports a touch only after the solve, when the velocities have already changed. `RammingTracker` counts the touching shape pairs per enemy ship pair. At stage 8 it handles begin events first, then end events. When a count goes from 0 to 1, it adds one candidate per direction at time 1. The damage is `0.1 × |vA − vB| × min(mass A, mass B) × the attacker's RammingDamageMultiplier`, and it applies only above 1 (legacy `DefaultCollisionBehaviour`). The candidate's target shape is the smallest (priority, order) among the pair's begin events in that step. A removal forgets every pair of the removed ship. Ally pairs are not tracked. A hostile worm tail segment pairs with a ship too; only the ship deals damage (Device statuses, worm tail).
5. **Damage (stage 10).** Stage 9 adds each accepted hit on a ship or a missile to its `PendingDamage` and emits a `Hit` event. A ship keeps one pending amount per damage type. Stage 10 applies the totals in one pass, as legacy `Impact.ApplyShield` and `ShipStats.ApplyDamage` do (see "Damage types"). A missile loses hit points (legacy `MissileDamageHandler`). The resolver and stage 10 add the same values in the same order, so they agree on a missile's death. Stage 10 applies damage only to local targets; the authority rules above decide which contacts supply that damage.
6. **Deaths (stage 11).** A ship with armor 0 or less is marked; stage 12 removes it with reason `Death` and a `Killed` transition. A missile with hit points 0 or less gets removal reason `Death`; it wins over an expiry in the same tick. `QueueDeath` remains as a test hook.

### Damage types

Phase 4 step 1 (D4, owner 2026-10-28): the legacy formula, with every type rule as data. Examples: `TODO/battle_simulator_phase4.md`, "Damage examples (D4)"; tests: `Core/Tests/DamageTypeTests.cs`.

1. **Table.** `BattleSetup.DamageTypes` is a `DamageTypeTable`: one `DamageTypeDefinition` per type (its shield order and its resistance ceiling, Device statuses item 9) and a `RammingDamageType`. A damage type is an index into it. The default is `LegacyDamageModel.Types`: impact, energy, heat, corrosive in database enum order; ramming deals impact.
2. **Sources.** `BulletDefinition`, `BeamDefinition` and `PointDefenseDefinition` carry `DamageType` (point defense defaults to energy, legacy `PointDefenseCollisionAction`). `BattleSetup` rejects a type its table lacks, on roster ships and their drones.
3. **Resistances.** `ShipDefinition.Resistances` holds one `DamageResistance` per type, or none (every type in full). `Armor` is the fraction removed before armor, `Shield` the fraction removed before the shield, `EnergyGain` the energy per point that passes the shield. `Armor` and `Shield` are at most 1, so no damage heals. `LegacyDamageModel.Resistances` builds them from the legacy percentages: corrosive takes half of the smallest armor resistance and the shield corrosive resistance; energy absorption over the armor multiplier is the energy gain.
4. **Stage 10.** The shield meets the types group by group in `ShieldOrder` (legacy: corrosive first). A type's load is its damage times `1 − Shield`. A group within the shield left is absorbed; a larger group keeps the same share of each type, and the shield empties. The shield restarts its delay. Armor then takes each remaining amount times `1 − Armor`. Energy gains each remaining amount times `EnergyGain`, up to its maximum, without restarting its delay.
5. **Bullet targets** take damage without types or resistances (legacy `MissileDamageHandler`).
6. **Network.** A hit record carries its damage type. Prediction and the correction replay use the copy's resistances and the same function.

### Result and timer

Stage 13 evaluates elimination first: a side whose slots are all `Destroyed` loses; both sides in one tick give a draw. A `Ready` reserve keeps its side alive.
Then the timer: it advances one tick only while both sides have an active slot (B5). `TimeLimitTicks` 0 means no timer. `BattleSetup.TimerMode` decides the resets and the expiry (phase 4 step 8; `TODO/battle_simulator_phase4.md`, "Timer modes and reinforcements (8)"):
- `Draw` (default): any ship deployment or removal on either side resets the timer to zero (owner, 2026-10-20). At the limit the battle ends in a draw with reason `Timeout`.
- The legacy `TimeOutMode` values use `PlayerSide`; the other side is the enemy. An enemy deployment resets. An enemy removal resets, except in `CallNextEnemyOrDraw` with no ready enemy slot. A player removal resets only in `DrainPlayerHp`. A player deployment never resets. A removal is a death, a retreat or a vanish; a reserve destroyed without an entity is not. The rules read the counts after every transition of the tick.
- `CallNextEnemy` never draws: the timer counts on past the limit, and the adapter deploys the next enemy. `CallNextEnemyOrDraw` does the same while the enemy side has a ready slot (alive minus active); without one, 3 × the limit is a draw. `DrainPlayerHp` never draws: from the limit on, stage 10 drains each active player roster ship of a local owner by k % of its maximum armor and shield per second, k being the whole overtime seconds of the previous tick's timer. The drain ignores resistances and restarts both delays.

The tick of a reset counts when both sides then have a ship. `BattleSetup.MaxActiveShips(side)` is the deployment limit per side (`maxActiveShipsBySide`, else `maxActiveShipsPerSide` for both).
`BattleSetup.OnlyOneShip` copies legacy `PlayerShipSelectionMode.OnlyOneShip` (phase 4 step 9; `TODO/battle_simulator_phase4.md`, "Ship selection modes (9)"). The first `Active` → `Destroyed` transition of a `PlayerSide` slot sets a deadline `Battle.ShipLostTicks` (150, the legacy 3 s) later. At the deadline the enemy side wins with reason `ShipLost`; the player reserves stay `Ready`. Elimination at or before the deadline comes first, so an enemy that dies inside the window gives a player victory. A retreat does not set the deadline. After the death the core rejects player deployments. A duel never sets the flag.
`BattleResult` holds outcome (`Victory` or `Draw`), winning side, reason (`Elimination`, `Timeout` or `ShipLost`) and the original commit tick.
`CommittedBattleFlow` evaluates roster transitions by their original ticks. It stores a confirmed prefix and replays only pending transitions.
A delayed deployment or removal can replace a pending timeout. All transitions at one tick apply before elimination and timer evaluation.
The first eliminated side loses; elimination of both sides at the same tick is a draw. Drones and predicted health never enter this evaluator.
`BattleSnapshot.CandidateResult` exposes the provisional result. `Result` and `IsCompleted` stay unset until the local core executes its tick
and every remote watermark covers it. Missing operation sequences and pending roster revisions block the confirmed prefix.
The core continues stepping and accepting commands while a candidate is pending. After finalization, `Step()` and `Submit()` throw.
One final result event uses the publication tick; its `Result.Tick` retains the historical result tick.
Single player uses the same evaluator with an immediately confirmed local frontier, preserving its event log and timer behavior.

### Bullets and cannon

Phase 2 steps 6–7. Devices are explicit code per device (`ShipDevice`); the data form waits for the data-model gate (A4).

1. **Definitions.** `ShipDefinition.Devices` lists at most 64 `DeviceDefinition`s; the list index is the device index and its control bit. `ShipDefinition.Barrels` lists `BarrelDefinition`s: the muzzle point and angle in the ship frame (the legacy platform offset included). `CannonDefinition` holds barrel index, cooldown (`1 / FireRate`), energy cost, spread (full angle, radians) and a `BulletDefinition` (radius, speed, range, damage and damage type, extra damage of other types, weight, `ParentVelocityEffect`, hit points, homing, `StartingVelocityMultiplier`, recoil, push). Deployment creates or resets cached devices for the definition. A missile launcher is a cannon with homing ammunition: legacy launchers are ordinary weapons, and the ammunition decides the behaviour.
2. **Whole ticks.** `TickTime` converts legacy seconds once, at setup, with exact arithmetic. A legacy `>=` check takes ceil(seconds × 50) ticks; a legacy `>` check takes floor(seconds × 50) + 1. A value within 0.001 tick of a boundary counts as on it. Legacy accumulates float time per step, so at an exact multiple it can differ by one tick.
3. **Cannon (Q1, legacy `CommonCannon` on a `FixedPlatform`).** At stage 2, after the resources recharge, each device of a ship that is not retreating runs in index order (legacy `ShipRetreatEffect` blocks every system). A cannon fires when its bit is held, its cooldown is over (`CooldownTicks`, a `>=` check; it starts ready), its barrel is ready, and `Energy.TryConsume(cost)` succeeds. A failed check spends nothing.
4. **Barrels.** Weapons on one barrel share a 0.04 s cooldown (legacy weapon platform, a `>` check): 3 ticks. A deployment counts as a barrel shot in the tick before, so the first shot comes 2 ticks after the deploy tick, as in legacy.
5. **Spawn (stage 5).** A device writes a `SpawnRequest` at stage 2; stage 5 creates the bullet. The ID is `CreateChild(ship, tick, device index, barrel index, sequence)`. The angle is the ship angle + the barrel angle + (r − 0.5) × spread, with r from `CounterRandom` (`WeaponSpread`, the bullet ID). The velocity is speed × `StartingVelocityMultiplier` along the angle + the ship velocity × `ParentVelocityEffect`; the ship velocity is the one after stage 4.
6. **Recoil.** An impulse of −`Recoil` × the bullet velocity relative to the ship (legacy `BulletFactory.ApplyRecoil`), at the muzzle point; an off-centre barrel also spins the ship. `Recoil` comes from `Body.Recoil` only, never from the weight (phase 4 E1, owner 2026-10-31); the mapper sets it to 0 for a bullet attached to its parent. `PreStepVelocity` keeps the stage 4 value.
7. **Store.** `BulletStore` is a struct array with a LIFO free list and generations. The free list grows with the array, so a tick that frees many bullets late in a battle allocates nothing; `EntityRegistry` maps bullet IDs too. A physics bullet maps back to its entry by physics index and `BulletId`. Physics bullets are pass-through, so physics reports every target; the core keeps the first valid hit (contact ordering). The bullet's `Owner` body is its ship, so it never hits that ship.
8. **Filters.** `CollisionFilters` gives one category bit per allegiance group and class, following the legacy layer matrix (phase 4, D13: four groups, Player and Ally share one). Ship hulls are `Ship(allegiance)` and ignore nothing. An energy bolt is `Energy(allegiance)` and ignores friendly ships and drones and all energy bolts. It hits hostile ships, drones and missiles. A missile is `Missile(allegiance)` and ignores friendly ships, drones, bolts and missiles. It meets hostile ships, drones, missiles and bolts. A test checks every pair against the legacy matrix.
9. **Hits.** At stage 8 each physics hit on a live ship becomes a candidate at its sweep fraction (`BulletSweep`). Q1: a cannon bullet stops at its first valid hit, so stage 9 marks it for removal with reason `Death`. The damage then follows the ship pipeline: shield first, then armor. A ship takes each damage effect (`Damage` and `ExtraDamage`) with its own type; a missile and the hit event take the total. A remote ship gets one hit record per damage effect. A hit on a bullet becomes a candidate only when the target is a missile, with the missile's hit points as `TargetHitPoints` (see Missiles and target grid).
10. **Lifetime.** Range / speed, with a `>` check: `LifetimeTicks`. A bullet spawned at tick s flies in the physics of ticks s to s + `LifetimeTicks` − 1. Stage 11 of the last tick marks it `Expiry`.
11. **Removal (stage 12).** Bullets leave after the ship changes, in entity ID order, so the events do not depend on storage. A bullet outlives the ship that fired it; physics ignores its stale owner body.
12. **Push (phase 4 E4).** A hit on a ship or drone adds `Push` × the bullet velocity as an impulse at the target's origin (legacy `PushAction`: no spin; negative pulls). Missiles are never pushed: legacy `MissileDamageHandler` drops impulses. On a remote target the attacker's copy moves at once as a prediction; the first hit record carries the impulse, and the target owner applies it when the record arrives.

### Missiles and target grid

Phase 2 step 7 (Q2a, Q2b). The rules port legacy `HomingController` on new ammunition, with `W = 1 / (0.2 + 2 × weight)`.

1. **Missile.** A bullet with hit points above 0 is a missile (legacy `UnitClass.Missile`). It stops at its first valid hit on a ship or a missile (legacy `DetonateAtTargetAction`): impact damage only. Area explosions come with triggers in phase 5 (owner, 2026-10-20).
2. **Bullet contacts.** A bolt that hits a missile deals its damage and stops. Two missiles that meet both detonate, whatever their hit points: the candidate of each one stops it. A missile that meets a bolt deals nothing and does not stop (legacy `BulletDamageHandler`); only the hit of the bolt counts.
3. **Lifetime.** Range / speed, as for every bullet (Q2a; the legacy 1.2 factor is not kept).
4. **Target grid (R9).** At the start of stage 4, `TargetGrid` takes every active ship: entity ID, allegiance, position and hull radius, at the poses at the start of the step. It is a uniform torus grid with 32 u cells, rebuilt once per tick. It allocates nothing once its arrays fit the battle. Drones join it on the tick after launch: stage 4 precedes the stage 5 spawn.
5. **Target rule.** Range is the centre distance at the nearest torus image minus both radii. The search covers 1.3 × the range of the missile. Inside it, the nearest enemy within 30° of the heading wins. Without one, the enemy with the smallest deviation wins. Equal keys go to the lower `EntityId`, so the result does not depend on grid order. Beyond the radius there is no target (owner, 2026-10-20). Legacy chases an enemy at any distance, and its choice depends on list order. Target priority, decoys and `ChanceToAvoidMissile` come later.
6. **Search timing.** Legacy runs no search at creation, so the first search is in the tick after the launch. The next one comes 101 ticks later (`TickTime.After(2 s)`). A removed target starts a search at once, at stage 4 of the next tick (redesign, owner, 2026-10-20). Every search restarts the period.
7. **Steering (stage 4, `HomingSteering`).** Below full speed along the heading, the velocity changes by `normalize(speed × heading − v) × 0.5 × speed × W × dt`. This also removes sideways velocity. The turn rate is `120 × W` deg/s toward the target, and 0 inside ±5° or without a target. The rate is set, not accelerated. Physics never turns a bullet, so the core sets the heading after the turn before the physics step. The legacy body has the same heading after its step. Smart aim is deferred (phase 5, H4).
8. **Magnetic homing (phase 5, step 4c).** `IgnoreRotation` maps to `BulletDefinition.IsMagnetic` (legacy `MagneticController`). It is a homing bullet, so the search, the target records and the corrections of the homing rules apply. With a target, each tick the velocity changes by `normalize(speed × normalize(L) − v) × speed × W × dt`. `L` is the target delta plus the target velocity × |delta| / speed. The thrust has full size every tick, so the speed jitters around `speed`. Without a target there is no thrust. A bullet with a direction (`HasDirection`, legacy `BulletShape.HasDirection`) searches in the 30° cone and turns its heading by `10 × angle to its velocity × dt`, with or without a target. A `Spark` or `Mine` searches all around (the nearest enemy wins) and never turns. Legacy measures the delta without the torus wrap; the core uses the nearest image. A correction re-runs the magnetic steering (`HomingSteering.Advance`).

### Bullet triggers

Phase 5, step 2. `BulletDefinition.Triggers` lists the gameplay triggers in legacy order (`BulletTriggerDefinition`): `Spawn` creates child bullets, `Detonate` makes the bullet detonate. Sound and visual triggers stay in the view (owner, 2026-10-29). A trigger has one or more `BulletTriggerConditions`.

1. **Who runs them (H8).** Only the owner of a bullet runs its triggers. A receiver never derives a child: it adds each child from the owner's spawn record. A remote copy that this device removed by prediction (N2) therefore gets no children of its own.
2. **Conditions (legacy `Bullet`).** `Created` runs when the bullet is made. Every other condition runs at stage 12, before the bullet's removal:
   - `Hit` runs once per contact of the tick: the bullet's own stopping hit, a point-defense zap, or a hit that damages it (a missile). A contact that a remote device claimed counts once.
   - A `Death` removal (a stopping hit, or a missile shot down) runs `Destroyed`, then `Detonated` if `DetonateWhenDestroyed` (legacy default true).
   - An `Expiry` runs `Expired`, then `Destroyed`. A `Detonate` trigger among the `Expired` ones adds `Detonated`.
   - A `Disposal` (point defense) runs `Disarmed`.
   - A `Detonate` trigger on `Hit` stops a bullet that survives the contact (`Death`). On other conditions legacy ignores its result.
3. **Children.** Child i of a `Spawn` trigger turns from the parent's heading by `Rotation + RotationSpan × r` and starts at its pattern offset in that frame (`BulletSpawnPattern`). Its velocity is speed × `StartingVelocityMultiplier` along its heading + the parent's velocity × the child's `ParentVelocityEffect`. The trigger's `RandomFactor` varies speed and lifetime by (r − 0.5) × factor each. All draws come from `CounterRandom` (`ChildSpawn`, the child's ID) in a fixed order: rotation, offset X, offset Y, speed, lifetime.
4. **Identity and timing.** The ID is `CreateChild(parent bullet, spawn tick, trigger index, 0, sequence)`. A `Created` child of a stage 5 shot spawns at that tick and moves in it. A stage 12 child has the next tick as its spawn tick: it is created at stage 12, appears in that snapshot, and moves from the next tick, as a shot does. Homing steering starts after the first move. A child's own `Created` triggers run at once, so nesting stays in one tick; the mapper bounds the depth.
5. **Inherited fields.** A child takes owner, side, allegiance, the ship that fired its first ancestor (`ShipId`, so allegiance changes reach it) and that ship's device index (hits name it). `ParentId` is the parent bullet.
6. **Records.** The owner captures each child's spawn record before the parent's removal record, so the stream still names the parent. The record carries the launch pose. The receiver takes the parent's definition and velocity from its live copy, or from a store of remote bullets that can spawn children. An entry lives from its spawn record until the tick after its owner removal record applies. A late batch applies the removal (tick T) before the children (tick T + 1). The receiver moves a late child on a line from its spawn tick, as a late shot. The inherited velocity comes from the receiver's copy of the parent, as the shot velocity comes from its copy of the ship (P1).
7. **Mapper.** `CoreWeaponMapper` maps legacy `BulletTriggerBuilder`: triggers in order, then the `OutOfAmmo` spawn triggers with the conditions of the spawn triggers that reached `MaxNestingLevel` (default 100). A child takes the legacy `CreateFactory` modifiers (power and size multipliers, trigger colour); its speed and range also scale by the area multiplier. Each child definition is built once per weapon. The rotation and offset expressions are sampled per child index: a `RANDOM` call becomes a span. Inert reasons: a child the core cannot run gives its own reason; `SpawnBullet on Cooldown`, `SpawnBullet on Hit with cooldown` (a missile), `spawn expression with RANDOM` (several calls, `RANDOM_INT`, not linear), `too many child bullets` (over 1000 per weapon), `beam with trigger …`. A bullet with a gameplay trigger needs no damage effect (a carrier).
8. **Behaviour change (owner, 2026-11-02).** Legacy builds an `OutOfAmmo` `Detonate` with the `OutOfAmmo` condition itself, which runs once at creation, so it does nothing. The core gives it the same conditions as the `OutOfAmmo` spawn triggers. `ClusterMissile` (record 112) now detonates when it expires at its last nesting level. The old simulator keeps the legacy result.

### Area and over-time impact

Phase 5, step 3. `BulletDefinition.Impact` copies legacy `BulletImpactType`. A bullet that is neither a missile nor a `FirstTarget` bolt is a legacy area of effect (`IsArea`): it has the energy filter, point defense ignores it (legacy `PointDefenseCollisionAction`), and nothing damages it.

1. **`AllTargets`** (legacy `HitAllTargets`, `OnTriggerEnter`): a target unit counts when it enters the bullet (physics `Begin`), so a target that stays inside is hit once and a target that leaves and comes back is hit again. The bullet never stops at a hit. A missile with `AllTargets` flies through its targets (legacy builds no `DetonateAtTargetAction` for it).
2. **`OverTime`** (legacy `DamageOverTime`, `OnTriggerStay`): every step the bullet touches a target, the target takes `Damage × 0.02` once per bullet and target entity (D16). The definition allows one damage type and no push or falloff, because a remote ship takes it as one interval at a fixed rate (network plan, decision 2); the record names shape 0, so the interval stays one contact while the bullet crosses the hull parts. A remote bullet target gets one record per tick, as a ray.
3. **Falloff (E6).** A `DamageAmount` with `Falloff` (legacy `DamageWithFalloff`) and `RadialPushWithFalloff` scale by `clamp01((R + target scale − d) / R)`: R is half the bullet `Scale`, d the torus distance between the centres after the physics step, and the target scale is `ShipDefinition.Scale` (legacy `ModelScale`) or the target bullet's `Scale`. Stage 8 stores the factor in the candidate; the candidate damage is the total after the falloff.
4. **Radial push** (legacy `RadialPushAction`: `PushFromCenter`, `PullToCenter`, `PushFromCenterWithFalloff`): an impulse along the line from the bullet centre to the target's origin, negative toward the centre, added to `Push`. Legacy applies it at the bullet centre; that point lies on the same line, so applying it at the origin gives the same spin. Missiles are never pushed. A remote target gets it in the first hit record (phase 4 E4).
5. **Lifetime.** A positive `lifetime` (legacy `Body.Lifetime`) replaces range / speed; speed and range may then be 0. A child's random factor varies it as before.
6. **Attached bullets** (`AttachedToParent`): the bullet rides on its ship at the barrel pose (physics `AttachBullet`), without spread, recoil or velocity of its own. Each device attaches its copy to its own ship copy; a fire confirmation does not re-aim it. When the ship leaves (death, retreat, vanish), stage 12 removes the bullet with `Disposal` and runs none of its triggers (owner, 2026-11-02; behaviour changes). A child cannot be attached: physics attaches bullets to bodies only.
7. **Mapper.** `CoreWeaponMapper` maps `HitAllTargets` and `DamageOverTime`, `DamageWithFalloff`, the three radial push effects, `Body.Lifetime` and `AttachedToParent`. A bullet without a prefab is the legacy area bullet: a circle of radius 0.5 × size. An area with effects scales its size by `AoeRadiusMultiplier` (legacy `BulletStats.BodySize`); `Scale` is that size. Inert reasons: `push over time`, `falloff over time`, `several damage types over time`, `bullet attached to a bullet`, `homing bullet attached to the parent`, `beam with falloff`. No built-in ammunition uses them.
8. **Views.** A bullet without a prefab has no look of its own in legacy (its trigger effects draw it), so `CoreBulletVisuals` draws the legacy flash glow at the bullet's `Scale`, in its colour at half alpha, until H11 decides the area views.

### Drains and siphon

Phase 5, step 5a. Examples: `TODO/battle_simulator_phase5.md`, "Drains and siphon (5a)"; tests: `Core/Tests/DrainTests.cs`.

1. **Model.** `BulletDefinition.Effects` and `BeamDefinition.Effects` hold one `ImpactEffects`: `EnergyDrain`, `ShieldDrain` and the
   `Siphon` factor. A hit applies each drain once at full power, without falloff (legacy actions); an `OverTime` bullet and a
   beam apply it per second, × 0.02 per tick. A charge scales the drains, not the siphon factor. Only ship and drone targets take
   them; a missile or a tail segment takes the damage only.
2. **Stage 9 and 10.** Stage 9 adds the drains to the target's `PendingEffects` beside its pending damage. Stage 10 (legacy
   `Impact.ApplyShield`, `ShipStats.ApplyDamage`) takes the shield drain from the shield first; the shield left absorbs damage,
   so a drain at least as large as the shield lets all damage reach the armor. The shield loses drain plus absorbed. The armor
   takes what passed less the repair. Energy gains its share first, then loses the drain, clamped at 0; the drain restarts its
   recharge delay. Several hits in one tick sum, so the order holds for the sum.
3. **Siphon** (legacy `SiphonHitPointsAction`). The siphon heals from the first damage effect: the attacker's ship (`ShipId`, also
   for a child bullet) regains that damage × (1 − the target's armor resistance of its type, with its resistance share) ×
   `Siphon`, as a repair at stage 10, capped at its maximum. The target's shield does not reduce it (finding 7a, kept). Only the
   attacker's owner heals its own ship; a copy follows corrections.
4. **Network.** A drain on a remote target is its own hit record beside the damage records of the contact (`HitEffect`), with
   the drain as amount; over time, its own interval with the rate as amount, because the interval key includes the effect. The
   target owner applies it like damage; the attacker's copy takes it at once as a prediction, and the correction replay
   includes it. A drain makes no `Hit` event on the receiver.
5. **Mapper.** `DrainEnergy` (× `EffectPowerMultiplier`), `DrainShield` (× `DamageMultiplier`) and `SiphonHitPoints` (a damage
   effect × `DamageMultiplier`, with its `Factor`). A bullet or beam with only drains maps with damage 0. Inert reason: `siphon
   with another damage effect` (the core heals from one damage effect; no built-in ammunition has it).

### Slow-down and roll

Phase 5, step 5b. Examples: `TODO/battle_simulator_phase5.md`, "Slow-down and roll (5b)".
Tests extend `Core/Tests/ImpactEffectTests.cs`, `DrainTests.cs`, and `Runtime/Tests/SessionCodecTests.cs`.

1. **Mapping and weight.** `ImpactEffects.SlowDown` and `Roll` use `EffectPowerMultiplier` and scale with charge.
   Neither uses damage falloff. Effects without damage map with damage 0. `ShipDefinition.Mass` comes from the same
   `stats.Weight` as legacy bodies, for ships and drones. Its 0.01 minimum is below the built-in weight floor of 0.05.
2. **Slow-down.** Stage 9 scales velocity and angular velocity by `1 - min(1, power / sqrt(mass))`, before push.
   Over-time bullets and beams use `power * 0.02` each tick. Separate contacts multiply separately.
   A web of power 4 on mass 16 applies 0.98 each tick: about 36.4% remains after 50 ticks without engine input.
3. **Roll.** On contact entry, add `power / sqrt(mass) * 180` degrees/s in the current spin direction.
   Zero spin starts counter-clockwise. Physics receives radians through `SetAngularVelocity`.
   Two reused sets of attacker/target pairs prevent repeated kicks during sustained contact. Leaving and returning
   starts a new contact. Both motion effects ignore missiles and tail segments (the approved minimal Roll port).
4. **Network.** Version 11's existing three-bit `HitEffect` field holds `SlowDown` and `Roll`.
   A slow-down record precedes the damage record carrying push. Its amount is power, or power per second for an interval.
   Roll is a discrete record carrying a positive degrees/s change; the receiver chooses its own spin direction.
   The target owner applies both on arrival. Missed slow-down ticks compound the per-tick multiplier against current
   velocity; they do not replay intervening engine inputs. Motion effects make no remote damage event and stay outside
   resource correction replay. Existing velocity corrections handle motion; they do not reconstruct contact history.
5. **Unsupported combinations.** The mapper reports `several slow-down effects` or `slow-down after roll` instead of
   merging non-equivalent action sequences into one power. No built-in ammunition uses either combination.

### Weapon classes and rockets

Phase 5, steps 4a, 4b, 4f and 4g. `CannonDefinition.Mode` copies the legacy weapon classes that fire projectiles from a held key; `Magazine` is the bullet count (multi-shot) or the burst length (machine gun). Examples: phase 5 plan, "Weapon classes and controllers (4)".

1. **Multi-shot** (legacy `MultishotCannon`). One shot needs the cooldown, the barrel and `Magazine × cost` of energy, spent at once. It requests `Magazine` bullets from one spawn slot in one tick (sequences 0 to n − 1), each with its own spread draw and recoil. One barrel shot.
2. **Machine gun** (legacy `MachineGun`). While the key is held, the magazine is not empty, the cooldown is over and the energy is above the cost (a strict check), the gun fires one bullet whenever the barrel is ready (every 3 ticks). After the last shot, the next update without a shot refills the magazine and starts the cooldown. A released key keeps the count. A confirmed shot re-syncs only the barrel, because the weapon cooldown follows bursts; the copy window of a shot is the barrel cooldown.
3. **Random factor.** Legacy `WeaponFactory` sets `RandomFactor` 0.2 (machine gun) and 0.3 (multi-shot): each bullet's speed and lifetime vary by (r − 0.5) × factor. `BattleWorld.DrawShot` draws them from `CounterRandom` (`ShotVariation`, the bullet ID): speed, then lifetime. A copy that adopts the owner's ID takes the owner's draws. Homing thrust and the speed cap scale with the speed; the turn rate does not (legacy).
4. **Rocket** (legacy `RocketController`, `BulletDefinition.IsRocket`). From the tick after its spawn tick, stage 4 changes the velocity by `normalize(max(forward speed, speed) × heading − v) × speed / (0.1 + weight) × dt`. It also removes the sideways speed. The thrust keeps full size until the velocity matches: the forward speed ends up to one tick of thrust above the speed, and a sideways remainder flips sign each tick (legacy). It never turns. A rocket on an attached bullet does nothing, so the mapper maps it as a plain bullet.
5. **Late spawns.** A late spawn record applies at stage 1, before stage 4. `RocketSteering.Advance` moves a late projectile from its spawn tick to the start of the present tick, with the thrust of the ticks between; a plain projectile moves on a line. A shot copy, a confirmed launch (P1) and a late child use it, so a remote rocket matches its owner without corrections.
6. **Mapper.** `CoreWeaponMapper` maps `MultiShot` and `MashineGun` weapons to cannons with the mode and `Magazine`; a magazine below 1 is inert (`weapon class … without magazine`). `Rocket` controllers take `StartingVelocityModifier`. Step 4b: `Manageable` weapons map to the manageable mode; `StickyMine` controllers set `StickyLifetime` (inert: `sticky bullet attached to the parent`, `sticky bullet without lifetime`).
7. **Manageable** (step 4b, legacy `ManageableCannon`). A shot needs the key, the cooldown, the barrel and the energy, as a cannon; it records no barrel shot. Stage 5 hands the bullet to the device (`CannonDevice.Hold`), and the device state names it (`DeviceState.ActiveBullet`). While the bullet flies, the device is active and ready while the key is held, whatever the barrel and the energy; a released key makes the owner mark the bullet's detonation at stage 2 (`BulletStore.MarkDetonation`). Legacy also needs a ready barrel, so another weapon's shot on that barrel detonated the bullet; the core fixes this defect (owner, 2026-11-03). A retreat counts as a released key. The update that finds the bullet gone records the barrel shot and starts the weapon cooldown; it fires nothing. A dead ship's bullet flies on, and nobody detonates it.
8. **Detonation** (legacy `Bullet.Detonate`). A bullet marked before the physics step takes no contacts in that tick and is no target. Stage 12 removes it with `Death` and runs `Destroyed`, then `Detonated` whatever `DetonateWhenDestroyed` (`BulletEntity.Detonates`). The children start at the bullet's pose after that tick's move: one tick of flight later than legacy.
9. **Sticky mine** (step 4b, legacy `StickyController`, `StickyMine`). Stage 8 records the first ship, drone or tail segment a sticky bullet touches. At the next stage 4 the bullet rides on that body (physics `AttachBullet`) at its pose in the body frame, with the offset × 0.9. Its expiry moves by `StickyTicks`, and the owner detonates it at stage 4 of the tick `StickyTicks − 1` after the stick tick. When the target leaves, the bullet leaves at the next stage 12 with `Disposal` and no triggers. Physics never hits an attached bullet's parent, but legacy keeps the stuck bullet inside its target's collider: each step, `BulletStore.CollectStuckHits` adds one time-0 overlap contact of a stuck `OverTime` bullet on its target's first shape, so its damage over time goes on.
10. **Network** (H8). Only the owner detonates and sticks. A copy takes the weapon cooldown from the owner's removal record: the tick after the record's tick, however late it arrives (`CannonDevice.OnRemoteRemoval`); a confirmed shot of a manageable cannon re-syncs neither cooldown nor barrel. The owner sends a stick as a `MissileTarget` record; the copy sticks to that target at its own pose when the record applies. A stick to a tail segment sets the record's segment flag (step 5a, `SessionCodec` version 11): the segment travels as its entity ID.
11. **Charging** (step 4f, H7, legacy `ChargeableCannon`; `CannonDefinition.Charged`, mode `Charging`, and `BeamDefinition.Charged`). `WeaponCharge` holds the rule for both devices. A held key starts the charge when the barrel is ready and the energy is at least half the full cost; the start spends nothing and counts charge tick 1. Each later tick with the key held spends `EnergyCost × 0.02 / ChargeTime` and adds a tick; from `TickTime.After(ChargeTime)` ticks on, the key holds the charge for free. The charge c is the ticks over `ChargeTime × 50`, 1 from `TickTime.AtLeast(ChargeTime)` ticks. A released key, a blocked bit, a retreat or energy that runs out fires the charge at that tick when it has at least 6 ticks (more than 0.1 s), else drops it with its energy. The shot records a barrel shot; there is no weapon cooldown. `DeviceState.Charge` shows c, `Ready` is legacy `CanBeActivated` (a running charge, or the barrel and half the cost), `Cooldown` is the barrel only, and `ActiveBullet` is the last shot's bullet while it lives. A charging weapon is not a shot weapon: a warp lets it charge and fire.
12. **Charged definitions.** The shot uses `BulletDefinition.WithCharge(c)` or `BeamDefinition.WithCharge(c)`, cached per device and charge tick count (`CannonDevice.BulletFor`, `BeamDevice.BeamFor`); a full charge is the device's own definition. Legacy `BulletStats` with `PowerLevel` c: damage, push and radial push × c; range × c, or 0 at c ≤ 0.1 (a beam then hits nothing; a bullet without a body lifetime expires after its first step); body size, collider radius, weight, recoil and beam-bolt length × (0.5 + 0.5c). A body lifetime stays, except a charged beam's (G9, owner 2026-11-03: × (0.5 + 0.5c)). Each child bullet takes its parent's charge, recursively (behaviour change "Charged child bullets"; legacy keeps the charge of the weapon's first shot). `Uncharged` and `Charge` name the source; the scene looks up the view by `Uncharged` and scales its size.
13. **Charge network.** The owner's spawn record carries the shot's charge ticks (`OwnerOperation.ChargeTicks`). The receiver builds the bullet or beam of that charge; a copy that its own control frames fired takes the owner's definition and expiry (`TakeCharge`), but its body keeps the radius of its predicted charge. A confirmed shot ends the copy's own charge that started at or before the shot (`WeaponCharge.EndAt`), so a missed release frame fires no second shot. The copy window of a charging weapon is the barrel cooldown plus 6 ticks, minus 1. Corrections carry no charge.
14. **Charging mapper and AI.** `RequiredCharging` with a beam controller maps to a charged beam (the shot-beam limits of [Beams](#beams), item 12), any other controller to a charging cannon; the fire-rate cooldown is the charge time. A fire rate of 0 or below is inert (`charging weapon without fire rate`). AI: `IsReadyToFire` needs c = 1; `Activate` releases the key; `SustainAim` tracks a charging weapon while its `ActiveBullet` lives; `ChargeWeaponsNode` (legacy `ChargeWeaponsNode`) holds the key of each charging weapon whose remaining cost `EnergyCost × (1 − c)` the free energy covers, or while the energy is at least 99%, and reserves that cost; Running while one charges, Success when all held ones are charged. `ChargeTests` covers the plan's rows 1-9 and 11, the definitions and three network cases.
15. **Continuous cannon** (step 4g, legacy `ContinuousCannon` with a bullet that is not a beam; mode `Continuous`). The device holds one bullet, as a manageable cannon does. A shot needs the key, the weapon cooldown and a ready barrel, and costs `EnergyCost` (legacy `ActivationCost`: `AuraEmitter` is no continuous controller, so no × lifetime). While the bullet lives, each tick with the key and the cooldown over pays `EnergyCost × 0.02` and renews its lifetime from that tick (`Auras.Renew`, legacy `Lifetime.Restore`); any other tick, also one without the energy, restarts the cooldown (legacy `TimeFromLastUse = 0`). The update that finds the bullet gone may fire the next one. `Ready` needs the cooldown, and the barrel only without a bullet; no energy check. A retreat counts as a released key; a warp does not block it.
16. **Aura** (step 4g, legacy `AuraController`, `BulletDefinition.IsAura`). `Radius` is the full radius. At the end of stage 5 the aura grows one tick, to at most `AuraGrowthTicks` (the body lifetime in ticks), and never above the ticks of lifetime left before this tick's step: g = min(g + 1, G, G − (tick − renewal tick)). Physics takes the radius R × g / G (`PhysicsWorld.SetBulletRadius`). At g = 0, the last tick of its lifetime, it takes no contacts. So a held aura grows in G ticks and stays; a released one shrinks to 0 over its lifetime; one released during growth grows until it meets the falling share. The snapshot shows g / G (`BulletState.AuraPower`); the scene scales the area sprite by it. The mapper maps `AuraEmitter` on any weapon (inert: `aura without lifetime`) at legacy full size 2 × `BodySize`, and a `Continuous` weapon with it to a continuous cannon.
17. **Aura network.** Only the owner expires an aura: its weapon's renewals follow the owner's keys. A copy grows and shrinks its aura by the control frames it has and leaves it with the owner's removal record (`BulletStore.MarkDeaths`); a late record adds the aura whatever its spawn tick. A copy adopts its device's held aura at any spawn tick, as a continuous beam; a record of another aura removes the held one, a prediction. No codec change. `AuraTests` covers the plan's rows 1-4, a renewal while the aura shrinks, damage while it grows and shrinks, and the network rule.

### Beams

Phase 2 step 8 (Q3). Immediate shutdown replaces the legacy lifetime tail (owner decision, 2026-10-20).

1. **Definition.** `BeamDefinition` is a device with barrel index, cooldown seconds, activation cost, energy per second, range and damage per second. Values must be finite; range is positive and costs, cooldown and damage are nonnegative. Phase 2 uses a fixed muzzle direction without spread or recoil; phase 5 adds the spread and tracking (items 10 and 11).
2. **Activation.** Stage 2 checks the held bit, weapon cooldown, shared barrel cooldown and activation payment. The first active tick pays only activation energy. Later ticks pay `EnergyPerSecond × 0.02` after resource recharge. Each successful tick deals `DamagePerSecond × 0.02`. Failed payment spends nothing and gives no partial damage. The existing resource rule requires positive available energy even for a zero cost.
3. **Shutdown.** Release, failed payment or retreat start stops the ray before stage 6 and starts cooldown on that tick. A held bit can restart after cooldown and energy recovery, but never on the shutdown tick. A beam records one barrel shot on activation; maintaining it does not occupy the barrel.
4. **Lifetime and identity.** Stage 5 creates one entity in `BulletStore` using the normal child ID. It has no physics body or collider, cannot be hit, and has no projectile expiry. The same ID lasts through the activation. Stage 12 removes stopped beams with `Disposal`. Parent removal marks attached beams before releasing parent state; projectiles still outlive their parent.
5. **Queries.** Stage 6 samples the current muzzle pose and performs one pass-through ray cast with eight reusable hit slots. It uses the energy collision filter and ignores the parent body. Enemy ships and missiles block the beam; friendly ships, friendly missiles and energy bolts do not. The ray and its no-hit endpoint use the physics limit of the smaller arena dimension.
6. **Selection.** Stage 8 collects the retained ray candidates; stage 9 selects the nearest valid target after disarms. A hit consumes this tick's ray, not the beam entity. Shield priority breaks equal-distance ties before entity ID and shape order. Damage-over-time deduplication limits each attacker/target pair to one hit per tick.
7. **Overflow.** If all eight returned targets are invalid and physics reports overflow, that tick has no hit. `BeamQueries.ExhaustedRayCount` increments and a warning identifies the battle, tick and beam. Overflow with an accepted target does not increment it.
8. **Validation.** `BeamTests` covers legacy LaserBeam_S_1 values, energy boundaries, cooldowns, retreat, identity, query ordering, shields, missiles, wrapping, snapshots and exhausted buffers. `PointDefenseTests` also checks real disarms through complete battle steps, including exhausted ray buffers. Sustained hits and repeated activations allocate zero bytes after warm-up.
9. **Shot beam** (phase 5, step 4d, legacy `CommonCannon` with a beam controller). `BeamDefinition.Shot` has a lifetime (`IsShot`); `ActivationCost` is the energy per shot and `EnergyPerSecond` is 0. The device fires as a single-mode cannon: the held bit, the weapon cooldown from the last shot, the barrel and the energy, then one barrel shot. It holds no beam (`DeviceState.ActiveBullet` is empty) and is a shot weapon, so a warp blocks it. The beam rides on its barrel and expires after `LifetimeTicks` (`TickTime.After`) at stage 11, as a projectile does; stage 12 removes it with `Expiry`. Ship removal removes a live shot beam with `Disposal`, without triggers (legacy `BulletCompositeDisposable`).
10. **Spread** (legacy `BeamController`). Each tick stage 6 turns the ray from the barrel by (r − 0.5) × `Spread`, for a continuous or a shot beam. `r` comes from `CounterRandom` (`BeamJitter`, the beam ID and tick), so a remote copy with the owner's ID draws the same angle. A beam without spread draws nothing.
11. **Tracking** (legacy `LookAtTargetController`, `TrackingAngle` > 0). Stage 6 points the ray at the position of the target the beam touched in the previous tick (a ship, a tail segment or a missile; the nearest torus image), at most `TrackingAngle` from the barrel. Without a target it turns by (2r − 1) × `Spread`: the full spread either way (legacy).
12. **Mapper.** A `Common` weapon with a beam controller maps to a shot beam with `Body.Lifetime` (inert: `shot beam without lifetime`); `RequiredCharging` maps to a charged beam (step 4f, [Weapon classes and rockets](#weapon-classes-and-rockets), item 11); other classes that fire projectiles keep the reason `cannon with controller Beam`. Both forms take the weapon `Spread` and the controller `TrackingAngle`, and the same limits: one damage effect over time, no trigger, push or falloff, a positive range, and `AttachedToParent` (inert: `beam not attached to the parent`; legacy leaves such a beam at its spawn pose).
13. **Beam bolt** (phase 5, step 4e, legacy `MovingBeamController` and `RayCastCollider`). A cannon bullet with `BeamLength` > 0 is a ray of up to that length from its pose along its heading (`BeamBolts`). Its physics body pairs with nothing, and point defense skips it (the legacy prefab has no collider). It launches with `ParentVelocityEffect` × the ship velocity only. Its ray is 0 in the spawn tick, then speed × age × 0.02, at most the length and the range. In the tick the ray reaches the length (`GrowthTicks`, `TickTime.AtLeast(length / speed)`), stage 4 adds speed × heading to its velocity. Its lifetime starts after the growth, so `LifetimeTicks` and the random-factor draws add the growth ticks. In flight the ray is the remaining lifetime × speed when that is shorter than the length; otherwise the length, capped by the range (the legacy collider keeps its growth cap). A late spawn record replays the launch in `RocketSteering.Advance`.
14. **Bolt contacts.** Stage 6 casts the ray on the poses at the start of the step, with the energy filter, ignoring the bolt's ship. Each hit becomes a projectile contact (`BulletSweep`, time 0, the ray fraction orders the targets), so the projectile rules apply: a `FirstTarget` bolt stops at its first valid hit and runs its `Hit` triggers; an `AllTargets` bolt hits a unit when the unit enters its ray (it was not on the ray in the tick before); an `OverTime` bolt hits every tick. Only missiles among bullets take its hits. A ray without pass-through (`Piercing` false: every beam shape but `PiercingLaser`) gives an `AllTargets` or `OverTime` bolt its first unit only. Stage 9 cuts such a ray at its first accepted hit for the view: the snapshot draws it from the bolt's pose after the step (`BulletState.HasRay`, `BeamEndPoint`), 0 once the bolt has passed the hit. The network needs no new record: a bolt is a plain projectile (spawn record with its launch pose).
15. **Bolt mapper.** A beam shape on a cannon with the `Projectile` controller maps to a beam bolt; its length is `Body.Length`, or the body size when 0 (a child scales it by `AoeRadiusMultiplier`). Inert: `beam bolt with controller …`, `beam bolt with hit points`, `beam bolt attached to the parent`, `beam bolt without speed`. `BeamBoltTests` covers growth, launch, lifetime, shrink, the range cap, first-hit and piercing rays, damage over time, point defense and a late remote copy.

### Point defense

Phase 2 steps 9 and 10 (Q4). The owner chose payment before each zap and legacy checking while ready. Step 10 adds drone energy damage.

1. **Definition.** `PointDefenseDefinition` supplies radius, cooldown seconds, energy cost and optional nonnegative `droneDamage` (default 0). Radius is positive; cooldown and cost are nonnegative. All values are finite. The device needs no barrel. The legacy fixture uses radius 4 (size 3 plus hull radius 1), cooldown 0.3 s and cost 1.
2. **Activation.** Stage 2 checks the held bit and pays the energy cost once. Release or retreat disables the device before queries. Failed zap payment also disables it. Reactivation needs payment and `TickTime.AtLeast(cooldown)` ticks since shutdown. Zero-cost activation and zaps retain the existing resource rule requiring positive available energy.
3. **Query timing.** The first query occurs at first activation tick plus `TickTime.After(cooldown)`. A successful query processes all affordable targets, then the next query waits the same delay. At 0.3 s the delay is 16 ticks. Empty queries leave the device ready, so it queries again next tick. Disable/re-enable preserves the query deadline; redeployment creates fresh state.
4. **Overlap.** Stage 6 queries a circle at the ship's current position, before physics. It includes enemy missiles and energy bolts spawned at stage 5. Enemy drones are also included. Allies, roster ships, beams and bullets pending removal are excluded. Physics measures distance to the target surface and handles torus wrapping. The reusable buffer starts at 32 entries and grows on overflow until every target fits.
5. **Payment and ordering.** Projectile overlaps become time-zero `Disarm` candidates. Drone overlaps become time-zero `Hit` candidates, one per device and drone, including compound hulls. Disarms sort before hits, so projectiles receive payment first. Drone damage reduces shield, then armor at stage 10; it never drains energy. Drones remain valid targets throughout the contact walk, even after lethal accumulated damage. The ship ID and device index identify the attacker. Stage 9 checks contact validity before payment. A failed payment spends nothing, leaves the target valid and stops this device's remaining zaps. A target already disarmed by another device costs nothing and does not reset the skipped device's query deadline. Accepted zaps restart the energy recharge delay.
6. **Removal and events.** An accepted disarm immediately invalidates the bullet within the contact walk, before any time-zero hit. Projectiles and beams can then select valid targets behind it. Stage 9 emits `BattleEventKind.Disarm` with attacker ID, target ID, `DeviceIndex` and the target position sampled at stage 6. Stage 12 removes the bullet with `Disposal`. The device has no separate entity.
   An area bullet is never a target (phase 5, step 3). A bullet with `CanBeDisarmed` false (legacy `Body.CanBeDisarmed`, phase 5) is zapped the same way: payment, delay, `Disarm` event and its `Hit` triggers. It stays a valid attacker and target and flies on, on both devices. Its owner counts each zap once (attacker, bullet, tick), whether its own copy or a remote claim reported it.
7. **Authority.** Defense energy and cooldown are simulated on both copies. The bullet owner commits destruction; the other copy removes the bullet by prediction.
8. **Validation.** All 534 engine-free tests pass, including 29 point-defense cases. Coverage includes payment, cooldowns, retreat, redeployment, filtering, wrapping, same-tick spawns, overlapping devices, allocation order, real beam/projectile continuation and 70-target salvos. Repeated interception and activation allocate zero bytes after warm-up. The large-salvo test also fixed a resolver bug: array growth must finish before the disarm array reference is fetched.

Core and Core.Tests compile separately with Unity 6000.0.75f1's compiler and assembly references. Unity Editor imported the new sources and compiled both assemblies.

### Drone bays and self-destruction

Phase 2 step 10 (Q5/Q6). A drone is a full `ShipEntity` without a roster slot. Its bay belongs to `(RosterId, device index)`.

1. **Definition and inventory.** `DroneBayDefinition` supplies positive capacity, nonnegative finite launch energy cost and a drone `ShipDefinition`. A launched ship launches none: a drone or clone with a bay or a clonning center is rejected, as legacy excludes them on drones (`DeviceDefinition.ChildShip`). A clonning center (phase 5, step 7c; [Device statuses](#device-statuses)) has the same inventory without a capacity, so its clone is a drone of the slot. Each roster slot owns persistent bay inventory. Launches consume hangar stock; deaths never restore it. Surviving drones and unlaunched stock remain through retreat and redeployment. Replication is deferred.
2. **Launch timing.** Held controls launch while stock, energy and the shared mothership bay timer permit. The timer is separate from weapon barrels. It uses `TickTime.After(0.4f)`: first launch at deployment tick + 20, subsequent launches at least 21 ticks apart. Device index resolves competition between bays. Failed payment spends nothing. Release preserves readiness; retreat blocks launch; redeployment resets the timer.
3. **Intentional timing correction (owner, 2026-10-21).** Legacy `ShipFactory.CreateDroneBays` registers the same platform twice. `ShipSystems` therefore advances its 0.4-second timer twice per tick. The new core advances it once and uses the declared duration.
4. **Spawn.** Stage 2 pays and queues; stage 5 creates at the mothership position and heading, with zero velocity and empty controls. The child ID uses the current mothership ID, tick, device index, lifetime launch slot and sequence. The free-position query searches up to 40 units. On failure it still launches at the returned least-overlapping position and retains the payment (owner, 2026-10-21).
5. **Lifecycle.** Mothership retreat or destruction does not remove drones. Redeployment exposes the new mothership ID through the same bay without resetting drone resources, controls or pilots. Surrender and kill-all destroy roster slots; surviving drones remain in the frozen result snapshot. Drones do not consume roster deployment limits, prevent elimination, prevent timer pauses or reset the timer (owner, 2026-10-21).
6. **Combat.** Drones use the ship engine, resources, devices, damage and ramming pipeline. They touch all drones and enemy roster ships, but pass through allied roster ships. Enemy projectiles and beams hit them. Newly launched drones participate in same-tick point-defense queries and physics. Their first systems/motion update and target-grid entry occur next tick.
7. **Public state.** Ship snapshots and spawn/removal events include nullable `BayKey` and `IsDrone`; a drone's `RosterId` is default. The snapshot supplies the current `MothershipId`; events use `ParentId`. An absent mothership has a null entity ID. No drone removal creates a roster transition. Both snapshot buffers grow to include drones.
8. **Pilot command.** `SelfDestructCommand` carries battle ID, command ID, target tick and entity ID. `Submit` rejects wrong battles, past ticks, null IDs and duplicate pending command IDs. Stage 1 processes it after roster commands and before frames, rejecting targets that are not live drones. Stage 11 marks death; stage 12 removes the drone normally. Runtime exposes `SelfDestruct` and forwards `PilotControls.SelfDestruct` at the locked delay. Providers select `PilotSchedule.Drone` (delay 2, period 2). Pilots decide whether an absent mothership warrants self-destruction.
9. **Allocation.** Pooled ships cache device instances per definition and reset their transient state on reuse. The ship free list reserves capacity as the pool grows. Repeated launches, drone deaths and weapon fire allocate zero bytes after warm-up. Sustained drone zaps also allocate zero bytes.

### Allegiance and ship commands

Phase 4, step 7e (D12, D13). The side stays the team: result, timer, surrender, kill-all and spawn placement read it.

1. **Allegiance.** `Allegiance` copies legacy `UnitSide`: `Undefined`, `Player`, `Ally`, `Enemy`, `Neutral`. `Allegiances.IsHostile` copies `IsEnemy`: equal allegiances and the `Player`-`Ally` pair are friends. A ship and its drones start with `Allegiances.OfSide` (side 0 `Player`, side 1 `Enemy`); a bullet or beam takes its ship's allegiance at spawn. Collision filters, the missile target search, point defense and ramming damage use hostility, not the side. Ship and bullet snapshots carry it.
2. **Ship commands.** `ShipCommand` carries battle ID, command ID, target tick, entity ID and kind: `Vanish` or `ChangeAllegiance(allegiance)`. `Submit` rejects wrong battles, past ticks, null IDs, unknown kinds, invalid allegiances and duplicate pending command IDs. Stage 1 runs them after self-destruct requests and before frames, in command ID order; it rejects unknown and remote entities.
3. **Vanish (D12).** Stage 1 marks the ship; stage 12 removes it with `RemovalReason.Vanish`: no death, no explosion. A death of the same tick wins. A roster ship gets the `Retreated` transition at once and keeps its deficits for a redeployment. A drone is lost to its bay.
4. **Allegiance change (D13).** Stage 1 sets the ship's allegiance, its hull filters, and the allegiance and filters of its live bullets and beams: legacy bullets read their owner's side. A drone that turns hostile gives no kill credit and earns none (the core keeps no kill credit).
5. **Network.** The owner sends an `Allegiance` operation (stage 1). The receiver applies it to the copy and the copy's bullets, predicted copies included. A vanished drone travels as a removal with reason `Vanish`; a vanished roster ship travels as its `Retreated` roster record, so the copy shows a retreat removal.
6. **Runtime.** `PilotControls` carries `Vanish` and an optional `Allegiance`. The runner submits them as ship commands for the pilot's own ship at the frame's target tick, in locked and network mode; they never enter the input stream.

### Device statuses

Phase 5, steps 7a to 7d (data-model gate, H2). Examples: `TODO/battle_simulator_phase5.md`, "Data-model gate (7)"; tests:
`Core/Tests/GateDeviceTests.cs` and `Core/Tests/WormTailTests.cs`. Each device stays a small `ShipDevice` class that composes
these parts:

1. **Activation policy.** `ActivationPolicy` is data in the device definition: energy cost, cooldown, when the cooldown starts
   (`AtStart` or `AtEnd`), duration, and an optional release time after which a released bit ends the use. `DeviceActivation`
   runs it: the cooldown timer (ready at once, as legacy `TimeFromLastUse` 3600 s), the start tick and the running flag. A
   free use needs no energy (legacy `CanBeActivated` has no energy check without a cost).
2. **Ship status.** Stage 3 asks every device of every ship for its status (`ShipStatusFlags`, a mask of blocked devices).
   `NoContacts` keeps the hull categories and ignores every other category, so no shape pairs with the ship: no hits, no
   ramming, no solid contact. The point-defense query has no category; it skips a drone without contacts itself. Homing and
   the AI still see the ship. `EngineOff` gives the engine throttle 0 and no course (the spin brake stays); the engine input
   with it is kept for the correction re-run. `Drift` gives no engine at all: no thrust, no turn and no spin brake, so the
   ship keeps its velocity and spin. `Faded` is for the view (10% opacity). The ship snapshot carries the flags, and
   `ShipState.Tint` (0 to 1, the largest of the devices) for the view's colour change.
3. **Blocked devices.** The mask of stage 3 holds through stage 2 of the next tick: a blocked device sees its bit released, the
   same for every device of the ship (approved change: legacy blocks the devices after the blocking one at once).
4. **Lethal damage.** At the end of stage 10, after the timer drain, a local ship without armor asks its devices in index
   order; the first that saves it ends the round. Every hit of the tick counts first (H3). Stage 11 then finds no death.
5. **Retreat.** `UpdateRetreating` replaces `Stop` at stage 2 of a retreating ship. Devices stop as before; a running use goes
   on as with a released bit (legacy `ShipRetreatEffect` blocks activation only).
6. **Network (H8).** Only the owner starts or ends a use. It sends a `DeviceUse` operation (ship, device, started) that names
   the use's first status tick; the copy applies it at stage 1 of that tick, or at once when late. A copy never starts a use
   from its own control bits or damage. Hits a remote attacker commits before the record arrives still count (attacker
   authority; one network delay).
7. **Ship-centre spawn.** A device may request a bullet at the ship centre (`SpawnRequest.ShipCentre`): rotation 0 and no
   spread (legacy `CreateArea`). Its identity has the device index as source and
   spawn slot 0. The owner sends a `BulletSpawn` record with the launch pose; the receiver adds the bullet from it and never
   derives it (H8), so it appears one network delay late, and not at all once it has expired there.
8. **Child ship.** A device with a `ChildShip` launches through the drone path (`SpawnRequest.Launch`): the slot's inventory,
   the child identity with the lifetime launch slot, a `DroneSpawn` record and drone collision. `OnShipLaunched` tells the
   device its child, on the owner at stage 5 and on a copy from the launch record. `OnShipLost` tells the device of the
   slot's active ship that a child of its index left; the device compares the ID, so a redeployed ship ignores the child of a
   former deployment. `DeviceActivation.Reset(policy, tick, share)` starts a timer partly elapsed.
9. **Resistance share.** A status may carry `ResistanceShare` from 0 to 1. Stage 10 moves each armor resistance r of the ship
   to r + share × (ceiling − r), where `DamageTypeDefinition.ResistanceCeiling` is 1, and 0.5 for legacy corrosive damage
   (half of the smallest resistance). Two shares p and q give 1 − (1 − p)(1 − q), as legacy applies modifications in turn.
   The correction replay uses the copy's share. `ShipState.ResistanceShare` carries it to the view: the combat HUD shows
   heat, energy and kinetic resistance with the share, as legacy `Stats.Resistance` does.

Devices:

1. **InfinityStone** (`TimeMachine`). Cooldown starts at the use; `NoContacts` for `Lifetime`. A use restores energy, armor
   and shield to their maximum without a recharge delay. It starts while the bit is held and the device is ready, then
   `NoContacts` runs from that tick. Lethal damage starts it when ready; then `NoContacts` runs from the next tick, as legacy
   turns the collider off at the next frame, and the cooldown counts from the damage tick. Lethal damage during `NoContacts`
   restores again without a new cooldown.
2. **WarpDrive** (`TeleporterV2`). Energy cost at the start; cooldown at the end. The start sets the velocity to heading ×
   `Power` and the spin to 0. While it runs: `NoContacts`, `EngineOff`, `Faded`, and cannons blocked (legacy: weapons other
   than `Continuous` and `RequiredCharging`; energy shields join when they run). It ends after `Range / Power`, or at a released
   bit once more than 0.1 s has passed; the velocity drops to 0. A copy ends at the full duration when the end record is late.
   The `Chase` node holds the first warp drive at full throttle (legacy `ChaseNode.UseWarpDrive`). Gap: the centre distance
   minus the selected weapons' minimum range. A warp starts within 5° of the course and a gap of 5 up to the warp range; a
   running warp is held while the gap is above 0 and within the range.
3. **Detonator** (`Detonator`, step 7b). Energy cost at the start; cooldown at the end. The lock runs for `Lifetime`, with or
   without the key and during a retreat: `Drift`, every other device blocked, and `Tint` from 0 to 1 (the view moves the
   ship to the device colour). In the first tick after the lock the owner spawns the explosion at the ship centre and takes
   all armor (legacy `CollisionEffect.Destroy`), so the ship dies unless a ready InfinityStone saves it. A ship that dies
   during the lock does not explode. The explosion (`DetonatorDefinition.CreateExplosion`) is an `AllTargets` area for
   0.3 s, radius `Range` × √model scale; heat `Power` × cells × damage multiplier and a push radius × √mass, both with
   falloff; allies are not hit. The lock restarts at each use (approved change: legacy never resets its timer). The copy
   runs the lock from the start record and ends it at the full time; the explosion comes from the owner's spawn record
   (item 7), the death from the owner's roster record. The `DetonateShip` node holds the first detonator; with `InRange`
   only when the ship and its target, each moved on by its velocity for the lock time, touch (centre distance up to the sum
   of half their model scales).
4. **Clonning center** (`ClonningCenter`, step 7c). Energy cost at the start; the use lasts while its clone lives, so the
   cooldown starts at the clone's loss. Deployment starts the timer 80% elapsed (first clone after 6, 9, 12 s for cooldowns
   of 30, 45, 60 s). While no clone lives and the timer has passed, a held bit launches one: at the mothership position plus
   its model scale in a counter-based direction of whole degrees (`RandomPurpose.CloneSpawn`), facing that direction, with
   the mothership velocity. The clone is a drone (legacy `UnitClass.Drone`): it stays when the mothership dies or retreats
   and does not keep its side alive. While it lives the center is not ready, its cooldown is 1, and it counts one active
   drone; without it, one drone in the hangar (the `SpawnDrones` and `HasDrones` AI rules). The mapper builds the clone from
   legacy `ShipSpecDecorator(0.9, 0.9, 0.9)`: armor, shield and shield recharge × 0.9, and no bays and no clonning
   centers. It also scales the weapon damage multiplier and the ramming damage multiplier by 0.9, which legacy combat never
   reads from the decorator (approved change). Devices keep the ship's damage multiplier.
5. **Worm tail** (`WormTailV2`, step 7d). A passive device; `Battle.Tails` runs its chain of `TailSegment` entities
   (`EntityKind.Segment`). Segment 1, the head of the tail, hangs on the ship; each next one on the segment before it.
   - *Growth* (stage 2, owner only). The head grows at the deployment tick, then each segment grows the next one after
     0.05 s (3 ticks), up to `Size` (legacy `RoundToInt`). A segment that replaces a lost one, and each segment after it,
     waits 0.5 s instead (legacy factory counter). Scale × 0.95 per segment from the ship's model scale; mass ship / `Size`
     for the head, then × 0.95; a circle of the prefab radius. The segment appears where its hinge holds it, straight
     behind the parent at the parent's angle, without velocity. A growth waits while the place overlaps a body the segment
     would touch (P4 rule): an `OverlapCircle` with the segment filter, bullets excluded.
   - *Hinges.* Anchor (−offset × parent scale, 0) on the parent, with the device `Offset.x` plus 0.15 at the ship, and
     (`Offset.y` × segment scale, 0) on the segment; reference angle 0; limits ±75° at the ship, ±40° between segments.
   - *Collision.* The missile class of its allegiance (legacy `UnitClass.Limb` on the missile layer): hostile ships,
     drones, missiles and bolts touch it; its own ship, its own segments and allies never do. Beams and point defense
     follow from the filter; point defense ignores segments (legacy targets bolts, missiles and drones only).
   - *Motion* (stage 4, both sides). An attached segment loses 5 × its sideways speed per second. A segment at a squared
     distance of 5 × its scale or more from its parent, or turned 90° or more from it, goes back where the hinge holds it,
     with the parent's velocity and no spin, and skips the damping that tick.
   - *Damage* (stage 9 queues, stage 10 applies, owner only, before the ship pass). A segment takes the total of every
     damage type, without resistances, and a tenth of the push. Segments 1 and 2 never break while attached: a hit goes to
     the tip of the tail first, and the damage the whole tail cannot take passes to the ship as impact damage. From segment
     3 on, recent damage above the hit points tears the segment off (recent damage falls by 5 × the hit points per second);
     otherwise the hit goes to the tip first, and a segment that cannot take the rest dies and passes it on. A segment that
     absorbs a hit restarts its 5 s regrowth wait. Hit points: `Power` × the ship's armor.
   - *Loss.* The segments behind a lost one detach at once: they keep their hinges to each other, take hits on their own
     hit points and destroy themselves after 5 s. The parent regrows the lost segment `Cooldown` later.
   - *Status.* Stage 3 gives the ship the resistance share of the attached segments over `Size` (item 9).
   - *Ship leaves.* A ship that dies takes segment 1 with it; the segments behind it detach and destroy themselves after
     5 s, as after a tear (legacy: the head finds its ship destroyed first). A ship that retreats or vanishes takes every
     attached segment (legacy `Vanish` detaches nothing). Detached segments run out their own timers; on a copy they leave
     by the owner's removal records.
   - *Ramming.* A hostile ship rams a segment with the ship rule (Contacts and damage, item 4); a segment rams nothing.
   - *Turn cap* (P4). A ship with a tail turns at most 942°/s with 2,447°/s² (the fastest database worm, where the hinge gate
     passed); `ShipDefinition` caps both engines.
   - *Network (H8).* Identity `CreateChild(ship, tick, device, segment number, 0)`. The owner sends a `BulletSpawn` record
     per grown segment (barrel = segment number) and a removal record per segment that leaves, also with its ship; the copy
     builds each segment behind its own chain and never grows, damages or times out a segment itself. A contact on a segment
     travels as a contact on a bullet (`TargetIsBullet`, network plan 9d): it counts once, whether the attacker's or the
     owner's device saw it; a projectile once per segment, a ray, damage over time and a ram (device −1) once per tick. A
     remote projectile that stops at a local segment stops on its owner too. No codec change.

### Snapshots and events

Stage 14 writes a `BattleSnapshot` of the tick it completed, then increments `Battle.Tick`. `Battle.Snapshot` returns the published one. The constructor publishes tick −1: every slot `Ready`, no ships, no events.

1. Contents: battle ID, tick, timer, result, `Ships` (active roster ships in roster order, then drones by entity ID), `Bullets` (live bullets in storage order; the order has no meaning), `Segments` (live worm tail segments in storage order: entity, ship, side, owner, definition, number, detached, scale and pose), `Roster` (every slot in setup order) and `Events` (the tick's presentation batch), plus `OutgoingHits` (resolved contacts for remote ship and drone owners, and contacts on remote bullets for their owners). The lists are `ReadOnlySpan` views over arrays the battle reuses.
2. `ShipState`: entity, roster ID, side, allegiance, owner, definition, pose and velocities from the physics body, armor, shield and energy (value and maximum), the controls in force and the retreat completion tick.
3. `BulletState`: entity, parent entity, side, owner, definition, pose, velocity and expiry tick. Projectiles retain `Definition` (`BulletDefinition`). Beams set `BeamDefinition` and `IsBeam`, with `Definition` null. Their pose is the sampled ray origin and angle; `BeamEndPoint` and `BeamTargetId` describe the accepted hit or full range. Beam velocity is zero and expiry is `long.MaxValue` because the device owns lifetime.
4. `RosterSlotState`: roster ID, side, owner, status, deployment count and order, revision, active entity (null unless active), the retained B4 deficits and `DamageFraction` (B2).
5. `DeviceState` (phase 4, step 3b): `GetDevices(ship)` returns one entry per device of a `ShipState` from the same snapshot, in control-bit order. `Ready` is legacy `CanBeActivated` for the next tick with the end-of-tick resources; the bit does not count. `Active` is legacy `SystemBase.Active`: the bit was held and the device's checks passed before it spent energy. `Cooldown` is the HUD fraction (0 ready, 1 blocked). Drone bays add `InHangar` and `ActiveDrones`; beams, manageable cannons and charging weapons add `ActiveBullet` (the attached beam, the bullet in flight, or the last shot's live bullet; phase 5, steps 4b and 4f); charging weapons add `Charge` (step 4f). Inert devices are never ready or active and show cooldown 1. No peer sends it: each derives it from its own simulation. The rules per device are in [the phase 4 plan](../TODO/battle_simulator_phase4.md#device-state-3b).
6. Two buffers: the battle writes one while readers use the other. A reader finishes with a snapshot before the next `Step()` returns; the snapshot of tick `t` stays intact while tick `t + 1` runs. A worker-thread reader needs a third buffer.
7. Publishing allocates nothing once the arrays fit the battle (unit tests: ships, cannon hits, missile salvos, sustained beam hits and repeated beam activations, and the full fleet of the scripted battle). The copy size is measured in [Scripted battle](#scripted-battle).

`BattleEvent` is one flat struct per event, tagged by `Kind`. Its identity is battle ID, tick and `Sequence` (the position in the batch). Beam spawn/removal events carry `BeamDefinition`, `ParentId`, `IsBeam` and `IsBullet`; `BulletDefinition` is null. A beam emits one spawn and removal per activation and a `Hit` for each accepted tick. Events follow stage order. Roster transitions follow roster order; drone removals follow entity ID order; hits follow contact order. An entity event comes before the roster transition of the same operation:

| Operation | Stage | Events |
| --- | --- | --- |
| Deploy | 1 | `Spawn` (with the `ShipDefinition`), then `RosterTransition` Ready → Active, `Deployed` |
| Shot | 5 | `Spawn` of a bullet: `BulletDefinition`, `ParentId`, side and owner; `IsBullet` is true |
| Hit | 9 | `Hit` per accepted hit on a ship or a missile, in contact order: attacker (a ship or a bullet), target, damage before the shield, point (zero for a ship touch) |
| Point-defense zap | 9 | `Disarm`: attacking ship ID, device index, target bullet ID and query position |
| Death | 12 | `Removal` (`Death`), then `RosterTransition` → Destroyed, `Killed` |
| Retreat completion | 12 | `Removal` (`Retreat`), then `RosterTransition` → Ready, `Retreated`, with the retained deficits |
| Surrender, kill-all | 12 | For an active slot `Removal` (`Death`); for every slot `RosterTransition` → Destroyed, `ForcedDestruction`. A reserve has no entity. |
| Bullet adoption (network only) | 5 (an overdue record: 1) | `Rename`: a predicted copy of a remote bullet takes its owner's ID; `PreviousId` is the copy's ID (fire confirmations, P2) |
| Missile target or correction (network only) | 4 or 1 | None: the copy's target and motion change without an event (P3) |
| Child bullet | 5 (`Created`) or 12 | `Spawn` of a bullet whose `ParentId` is a bullet; at stage 12 before its parent's `Removal` (bullet triggers) |
| Bullet removal | 12 | `Removal` of a bullet (`Death` at a hit, `Expiry`, or `Disposal` after disarm or beam shutdown), after the ship changes, in entity ID order |
| Result | 13 | `Result` with the `BattleResult` |

A repeated destruction emits nothing. `RosterTransitionReason` has the same values as the application's `RosterChangeReason`. Every single-player event is committed; mixed-ownership events carry explicit authority as described above.

B2 participation facts: `DamageFraction` is the share of a slot's armor that is lost. It is 1 when the slot is destroyed (also without an entity), the armor deficit over the maximum while active or after a retreat, and 0 when never deployed. The core emits no separate checkpoint event. A `Retreated` or `Killed` transition and the `Result` event are the checkpoints. The consumer reads the fractions from the same snapshot, after the tick's roster updates. It credits the increase of the enemy sum, as legacy `CombatModel` does. A `ForcedDestruction` transition is not a checkpoint. At completion, the consumer credits the slot with the highest `DeploymentOrder` of its side.

### Pilots and scheduling

`BattleRunner` (Runtime) owns a `Battle`, assigns command IDs and runs pilots. Callers submit roster commands through it, so command IDs never clash.

1. `IPilot.Think(snapshot, ship, out controls)` reads the snapshot and returns course, throttle, system bits and optional `SelfDestruct`. The runner submits self-destruction with the same `N + Delay` as the control frame. `false` keeps the last frame in force; the runner submits nothing.
2. `IPilotProvider.CreatePilot` runs once per local ship `Spawn` event (bullets and remote owners are skipped) and returns a pilot and a `PilotSchedule`, or null. A redeployment is a new spawn, so the provider is asked again. A `Removal` event drops the pilot.
3. Locked mode: after each step, a pilot thinks on the snapshot of tick `N` when `N mod Period` equals `EntityId mod Period`. Its frame targets `N + Delay`; the battle's command buffer holds it until then. Pilots run in spawn order.
4. Presets (Q9): `Ship` (delay 2, period 1), `Drone` (2, 2), `PlayerShip` (1, 1).
5. Pilots run synchronously on the calling thread, so a pilot is never late. Free-running mode is deferred to the worker thread (owner, 2026-10-20): without one it equals locked mode with delay 1.
6. No pilot thinks on the result snapshot. Frames already queued for a ship that leaves are dropped at stage 1 and counted as rejected.

Core tests cover tick advancement, fixed physics timing, independent battle instances, setup validation, identity derivation and value semantics, the entity registry, counter-based random, roster transitions, roster commands, control frames, spawn placement, the result, the timer, resources, B4 deficits, the engine, snapshots, events, pilot scheduling, contact ordering and selection, ramming, damage, B2 facts, the cannon (Q1) and bullets: hits, expiry, filters, identity, events, the snapshot and allocation, the target grid, and the homing missile (Q2): launch, thrust, turn, search timing, retarget, hits on ships, bolts and missiles, expiry and allocation.
Beam (Q3), point-defense (Q4) and drone-bay (Q5/Q6) tests cover full battle steps and allocation. The scripted battle tests cover all of it together. All 654 engine-free tests pass, including Network and the step 2 authority checks. Core, Runtime and Core.Tests compile separately with Unity 6000.0.75f1's compiler.

### Scripted battle

Phase 2 step 11. `Core/Tests/ScriptedBattle.cs` is test code: the core knows nothing of it.

1. **Fleets.** Each side has three slots with 600 armor, in this order: a gunship (cannon and point defense), a striker (homing missile launcher and beam on separate barrels) and a carrier (a bay with four cannon drones). Two ships per side are active; the carrier is the reserve. Arena 200 × 200, time limit 6000 ticks.
2. **Pilots.** One `ChasePilot` per ship and drone, through `BattleRunner` with the Q9 schedules. It flies at the nearest enemy by the shortest torus vector and only turns inside 6 units. It holds a weapon bit when the enemy is in range and inside the aim cone: 20° for bolts, 60° for missiles, 10° for the beam. Point defense and bays are always held. A drone self-destructs when its bay slot is `Destroyed`; after a retreat it waits for the redeployment (Q6).
3. **Commander.** After each step it deploys `Ready` slots in roster order while a side is below its active limit. It orders one retreat per gunship slot below 60 % armor, so the ship survives the 7 s charge and redeploys.
4. **Result (seed 7).** Side 0 wins by elimination at tick 3506 (70 s). The battle has 4 roster deaths, 3 drone deaths, 2 retreats and 2 redeployments. It spawns 8 ships, 8 drones, 1119 bolts, 34 missiles and 46 beams. It has 2053 hits (1222 beam, 8 ramming) and 54 disarms.
5. **Determinism.** The log holds every event and a per-tick digest of ship and bullet poses, velocities and resources as raw float bits. One run alone and two runs stepped interleaved in one process give the same log.
6. **Allocation.** The same fleet with huge armor and energy, all three slots active, steps 3000 ticks after a 1500-tick warm-up with zero bytes allocated, pilots included. The window has bolts, missiles, beam hits, point-defense zaps and eight live drones. Buffers grow until about tick 510, at the first salvo peak; afterwards nothing allocates.
7. **Snapshot size.** Element sizes on .NET 8 x64, measured as the allocation of a 1024-element array: `ShipState` 128 B, `BulletState` 88 B, `RosterSlotState` 64 B, `BattleEvent` 160 B, `DeviceState` 24 B (2026-10-29; `ShipState` kept its size with the device offset). The scripted battle copies 1.8 KB per tick on average and 3.6 KB at most (peak 12 ships, 16 bullets). At R2 (2,000 bullets, 130 ships) the ship and bullet copy is 192,640 B per tick, about 9.6 MB/s at 50 Hz. That is 3.2 times the design estimate of 60 KB, which assumed 24-byte bullets. Timed on R1 (2026-10-28), `BattleSnapshot.Write` takes 0.24 ms per tick at R2 (0.27 ms with `DeviceState`, 2026-10-31); the byte copy is about a tenth of it ([core design](../TODO/battle_simulator_core.md), risk 3).

## Unity integration

Migration phases 3c and 4. Quick combat and campaign battles run on the core in a separate scene when the database selects it; the old scene stays the default. The plans and decisions are in `TODO/battle_simulator_scene.md` (S1–S5) and `TODO/battle_simulator_phase4.md`.

| Part | Location |
| --- | --- |
| Adapter, ship mapper, entity ID map, pilots, keyboard driver | `Assets/Scripts/Combat/Integration/Core/` |
| Views | `Assets/Modules/CombatView/` (references Core and Physics; the core never references it) |
| Installer | `Assets/Scripts/Installers/CoreCombatSceneInstaller.cs` |
| Scene | `Assets/Scenes/CoreCombatScene.unity` (`GameScene.CoreCombat`) |
| Editor entry | `Assets/Editor/CoreCombatLauncher.cs`: menu `Tools/Combat/Start Core Quick Combat` |

### Scene selection

`CombatSettings.UseNewCombatSimulator` (default false) selects the new scene (phase 4, D10). `CombatState`, `QuickCombatState` and `CombatRewardState` read it in their constructors, before the scene loads, and their `RequiredScenes` return `GameScene.CoreCombat`; the reward window has its own instance in `CoreCombatScene`. A database without the field, such as a mod, reads false and keeps the old scene; a partial mod without its own `CombatSettings` record inherits the built-in value. For tests, `@` in the `TestShipId` debug setting also selects the new scene for quick combat, with or without ship IDs (`@34`); outside the Editor it needs an editable database. Each state binds the same `ICombatModel` for both scenes.
`CoreCombatSceneInstaller` binds only the adapter, its scene presentation, the HUD presenters, the HUD panels and input. It binds no legacy `CombatManager`, `Scene`, AI or factories.

### Adapter

`CoreCombatAdapter` owns the `BattleRunner` and implements the application contracts: `IRosterView`, `IBattleView`, `IShipViews`, `IShipControlSource` and `ICombatCommands`. `CoreScenePresentation` implements `IScenePresentation` over it. The HUD reads these contracts and works unchanged.

1. **Setup.** Player slots get roster IDs 1…n, then enemy slots. The arena is `BattleMapSize` square. The seed comes from `UnityEngine.Random`. The rules map to `BattleSetup`: `TimeLimit` × 50 ticks, `TimeOutMode` as the timer mode, one active player ship, max(`MaxEnemyShips`, `InitialEnemyShips`) active enemies, and `OnlyOneShip` ([Result and timer](#result-and-timer)).
2. **Driver.** `Tick` adds `Time.deltaTime` to an accumulator and steps every `0.02` s, at most 5 steps per frame. A longer frame drops the rest. The adapter handles each tick's events right after its `Step()`, so no event is lost.
3. **Projection.** Slots and ship views copy what they need from the snapshot. They keep no spans. `EntityIdMap` gives each core entity a sequential application `EntityId` at its spawn event and releases it after the removal. The 64-bit core ID is never truncated.
4. **Commands.** Deploy, retreat, surrender and kill-all go to the runner. One retreat request per ship. Surrender steps once at once, because the menu pauses the game.
5. **Battle flow (legacy `CombatManager`).** The first `InitialEnemyShips` ready enemies deploy at tick 0; after that the first ready enemy deploys 3 s after the last active enemy leaves. When the timer expires in `CallNextEnemy` or `CallNextEnemyOrDraw` and the next-enemy rule allows it, the next enemy deploys one tick later; the menu button calls it the same way. When an enemy is active and the player has none, the selection panel opens: at once at the start, 3 s after a player ship leaves. `ByOrder` deploys the first ready ship without the panel. In `OnlyOneShip` the player commander stops after the first death. The open panel pauses the game, so a deployment applies when it closes.
6. **HUD values.** The timer panel shows while both sides have an active ship, with the legacy digits and alarm: seconds left, 0 in `CallNextEnemy` while no next enemy can come (the panel then hides the text), 3 × the limit in `CallNextEnemyOrDraw` without a ready enemy with the alarm in its last 15 s, and the alarm during the `DrainPlayerHp` overtime. `CanChangeShip` and `CanCallReinforcement` follow the legacy rules. `CoreShipControls.GetCooldown` returns the device `Cooldown`; the adapter copies the device states after each step.
7. **Completion.** At the result, stepping stops and the last frame stays on screen. After 3 s of real time the adapter fires `ExitSignal` once; a `ShipLost` result exits at once, because the core already waited the legacy 3 s. Real time, because surrender comes from the open menu, which pauses the game. `QuickCombatState` returns to the main menu without rewards; `CombatState` gives the reward, runs the completion action and opens the reward window over `CoreCombatScene`.
8. **Results (phase 4, step 12).** After each step, before the events, the adapter writes every roster slot to its `ShipInfo` (`ApplyCoreState`: status, `DamageFraction`, `DeploymentOrder`). `IsVictory()`, loot, `LootCondition`, `ExpCondition` and the B2 experience then work as in legacy; `CombatExperienceRecorder` is bound in this scene too. At the result the adapter compares the core winner with `IsVictory()` and logs an error on a mismatch, without rewriting either.
9. **Unload.** `Dispose` drops the runner and the views without roster events. The results stay in `ShipInfo`.

### Ships and pilots

`CoreShipMapper` maps each ship build (`IShipSpecification`) to a `ShipDefinition` (phase 4, step 3a). The rules and examples are in [the phase 4 plan](../TODO/battle_simulator_phase4.md#ship-mapping-3a). Armor, shield, energy, weight, ramming and resistances come from the ship stats. The engine uses the legacy `EngineStats` formula, with degrees converted to radians. The device index is the legacy system index: weapons by platform, then devices, then clonning centers, the replicator and the drone bays. Hull barrels become `BarrelDefinition`s; satellite weapons have no barrel (D9). `CoreWeaponMapper` maps a weapon with new-format ammunition to a cannon or a beam. Point defense and drone bays map to their core devices; a drone is mapped from its build with the bay multipliers. A clonning center maps to its core device with a clone mapped from the ship through legacy `ShipSpecDecorator` (phase 5, step 7c). Every other part becomes an `InertDeviceDefinition`: it keeps its index and does nothing (D1). Weapons with obsolete ammunition stay inert; the built-in catalog has none since the conversion (step 4c).
The mapper also returns a `CoreShipMapping` per ship and drone: key binding, button icon, activation cost and inert reason per device. The adapter logs the inert devices once per battle.
`CoreHullBuilder` builds the hull from `Ship.Collider` times `ModelScale`. It skips an invalid shape with one warning per ship. A ship with no valid shape gets the legacy sprite collider: a circle around the sprite for `SizeClass.Undefined`, otherwise the sprite physics shape reduced with `ColliderTolerance`. It runs on the main thread; the core receives shapes only.
Editor menus: *Tools/Combat/Check Ship Hulls* builds the hull of every built-in ship from the data and from the sprite and reports parts, inertia and radius. *Tools/Combat/Check Ship Builds* maps every built-in build and reports the armed builds, the behaviour trees they use and the inert reasons with counts.

`CorePilotProvider` picks each pilot by the legacy `ShipFactory` and `ControllerFactory` rules (phase 4, step 7f). A player ship gets a new `PlayerPilot` (`PilotSchedule.PlayerShip`); with the autopilot component it also gets the autopilot tree through `AutopilotPilot`. An enemy ship runs its own tree or the `CombatSettings` enemy tree at the fleet's AI level; an AI level below 0 gives no tree (legacy `EmptyController`). A starbase runs its own or the starbase tree; a drone runs the tree its bay selects, with the bay range. A clone runs its own or the clone tree at `AiSettings.Default`, with `PilotSchedule.Ship` (`CoreShipMapping.IsClone`). The mapping records the tree choice (`AiTree`, `DroneRange`, `HasAutopilot`). A null tree idles the ship and logs once per mapping. `CorePilotProvider.CreateTrees` builds the battle's `BehaviorTreeFactory` from the mapper: profiles by definition, bullet scales from the bullet sources and the broadcast callback.
`AutoActivationPilot` wraps every tree pilot and applies `CoreAutoActivation` to its frame. A tree that keeps failing submits nothing (T7), so the wrapper submits the kept frame again when the automatic bits change; one-shot commands are never sent twice.
`PlayerPilot` implements `IShipCommandSink` and submits the stored throttle, course and system bits each tick. `CoreAutoActivation` gives every pilot the legacy rule for devices with a key below 0: the pilot holds the device while energy fraction + 0.1 × (key + 1) ≥ 0, plus activation cost ÷ maximum energy while the device is `Active`. `CoreShipControls` groups the other devices by key, as legacy `GetKeyBindings`. A new deployment gets a new pilot, so it starts with empty controls.
`CoreKeyboardDriver` applies the legacy `KeyboardController` rules over `IKeyboard` and `IMouse`. It writes a value to the sink only when its own value changes, so HUD buttons keep theirs. A key held across a ship change acts on the new ship, as in legacy. In every frame with input it fires the legacy `PlayerInputSignal`, as the HUD does; the adapter then calls `BehaviorTreeFactory.OnPlayerInput` with the tick the pilots think on, so the autopilot tree waits.
ShowMessage texts reach the adapter during the step. After the step the adapter localizes each text and `BattleRenderer.AddText` draws it at the ship: legacy `RadioTransmitter` and the `MessageText` effect, 2 s, the ship's velocity plus 1 unit per second in a random direction, fading. Only the font comes from the legacy prefab.
Editor menu *Tools/Combat/Check AI Replay* runs every armed built-in build against the next one, both on their trees at the hard AI level, twice with the same seed and interleaved. It compares the logs: every event, a per-tick digest as in the scripted battle, and the texts.

### Views

`BattleRenderer` (`CombatView` module) draws ships and bullets as sprites under one root in the combat scene. Scenes load additively and the active scene never changes, so the root is moved into the combat scene.

1. Ships and drones use the model sprite at scale `ModelScale`. Worm tail segments use the body sprite of their prefab at their body scale, below the ships; a view lives while the snapshot lists its segment. Bullets and beams use their `BulletPrefab` image, size and colour (phase 4 step 5; rules in `TODO/battle_simulator_phase4.md`, "Views (5)"); a bullet without view data gets the generic sprite and one warning. Hits by bullets flash.
2. A removal by death fades the ship out in 0.4 s. There are no other effects or sounds.
3. Each view keeps the previous and current pose and interpolates with alpha = accumulator / 0.02 s.
4. Torus wrap: the controlled ship goes to its image nearest to the anchor, and the anchor follows it. Every other view goes to its image nearest to the new anchor. `IShipView.VisualPosition` is that image, so the radar and the camera agree. Drawn coordinates can leave the base map; simulation positions are never written back.
5. The camera uses the legacy `Combat.Scene.ViewRect` framing of the player and the target, and zooms in on each player deployment.
6. `CorePlanet` instantiates the legacy planet prefab unless `DisablePlanet`: presentation only, colour, position and size from a `System.Random` seeded from the battle seed. Asteroids are phase 5.

### Known limits

1. Many devices and ammunition effects stay inert until phase 5; the adapter lists them once per battle. Auto-aiming barrels fire straight; satellites are not attached.
2. The built-in database keeps `UseNewCombatSimulator` false until phase 5 coverage passes. Exploration and the controls scene still use the old simulator until phase 6.
3. A new player ship spawns at a random map point, so the camera whips toward it (legacy spawns the same way).
4. The Editor pauses the game when it loses focus with an Android build target (`MobileApplication`). Play-mode checks from the CLI must force focus.

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
7. Holes: a clockwise path inside another path is a hole in Unity. The hull loader must skip it, so the outer path makes the hole solid (`ConvexDecomposition.IsHole`; the collider export and the sprite fallback use it; 2 holes in `mothership`). Decomposing a hole as its own path puts overlapping parts on top of the outer path.

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
| Allocation | The step and the queries allocate nothing once their buffers have grown. Free lists grow with their pools, so many removals in one late step allocate nothing. A new peak still grows a buffer once (P3 cost scene: the sweep pair buffer, 2,048 → 4,096 pairs, 32 KB, in one of 1,000 steps). |

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

`Experimental/CombatSimulationHarness/` is a throwaway .NET console project. It compiles the engine-free Physics, Core, Network, Runtime, Diagnostics, and Utilites sources directly. The default command runs the physics gate scenes and checks their limits. It is outside `Assets/`, in its own private repository, not in the game repository. Game code never references it.

```
dotnet run -c Release --project Experimental/CombatSimulationHarness
```

It prints a table of measured values against the limits, and it writes the same table to `Results/P1-<date>.md` with the harness version and a state hash per scene. Two runs of the same build give the same hashes. Each scene runs in the arena middle and on the wrap corner. With `-- --replay` it also records each scene, and `Experimental/CombatSimulationHarness/Viewer/index.html` plays the recordings step by step (harness README).

### Network relay and virtual clock

Migration phase 3b, step 7 is implemented (2026-10-23). Run the infrastructure checks with:

```powershell
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-smoke
```

1. The relay provides two `ITransport` endpoints, pooled datagrams, and independent seeded `PcgRandom` streams per direction.
2. All six gate profiles use the documented latency, uniform jitter, loss, and sender delays. Selected reordered packets receive 100 ms extra delay.
3. Blackout drops sends and in-flight arrivals in both directions during each 500 ms outage, starting every 30 seconds after match start.
4. The runner advances virtual time by 1 ms without sleeping. It uses separate local clock origins and production `NetworkSession.Update` pacing.
5. Clock probes warm up for four seconds. Both clocks start against a reference epoch at five seconds. Missing synchronization fails explicitly.
6. Samples retain actual peer ticks and pacing rates. The no-jitter asymmetric check exposes the existing two-tick bias.
7. Completed sessions pump until reliable packets are acknowledged. The relay then stops new sends and drains queued traffic, with bounded deadlines.
8. Validation: 22 harness self-checks pass. They cover relay semantics, deterministic replay, reliable fragmentation and recovery, clock drift, and result agreement.
9. Each of the six profiles runs twice for three virtual minutes with a small moving two-ship fixture. Per-peer snapshot and packet hashes repeat.
10. All 752 engine-free tests pass, including the unchanged single-player log. The harness retains five existing build warnings.

Production APIs and sources are unchanged. The harness README specifies scheduling, counters, and timeout diagnostics.
The step 7 fixture has no weapons. Step 8 adds combat scenes and measurements below; the 168-run matrix remains step 9.
Android integration remains step 10. Phase 3b stays in progress. Step 7 adds no submodule change or commit.

### Network combat scenes and diagnostics

Migration phase 3b, step 8 is implemented (2026-10-23). It supplies seven scenes and individual gate measurements.
This does not complete the gate matrix, select a correction interval, or establish Android parity.

```powershell
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-scenes
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-metric-checks
```

1. `Battle.Observer`, `BattleRunner.Observer`, and `NetworkSession.Observer` expose the same optional `IBattleObserver`.
   Callbacks run synchronously. Observers must not mutate or re-enter the simulation; exceptions propagate to the caller.
2. Observations are immutable value copies. Core retains no diagnostic log and adds no snapshot fields or network bytes.
   Callbacks cover applied corrections, accepted contacts, homing projectile sweeps, processed inbound hits, and applied beam intervals.
3. Correction observations contain source and application ticks, the actual error before shifting history, and a missing-history flag.
   Resource observations compare values before correction with values after recharge and pending-hit replay.
4. Contact observations retain device, shape, owner, time, and entity identity. Homing observations capture motion before physics and removal.
   Hit observations include records processed after the target disappeared. Beam observations distinguish interval ticks from application ticks.
5. Disabled observers allocate nothing. Enabled counter observers allocate zero bytes after warm-up during correction and beam/contact tests.
   Diagnostic callbacks do not change simulation or packet hashes.

The harness runs dogfight, missile salvos, beam duels, ramming, drone churn, homing missiles, and last-ship mutual kill.
Its definitions use the production devices and public commands. Churn destroys drones, retreats depleted carriers, and deploys fresh reserve carriers.
The mutual-kill script fires side B one tick earlier because the runner updates A first. This gives a perfect-link draw.
The same script applies to every profile; actual committed ticks still determine impaired-link results.

1. Workload windows are `[20,170)` seconds for dogfight and `[20,50)`, `[80,110)`, `[140,170)` for churn.
   Both peers must reach median one ship and 20 drones per side, plus at least 900 live projectiles, in every window.
2. Reports show whole-run and window counts. Beams have separate counts. Either peer's underload disqualifies both workload-dependent verdicts.
3. Movement uses the actual correction and hull radius. Missing history and missing homing copies remain visible.
   Armor and shield correction changes are informational. No limit is inferred for them.
4. Reliable comparisons retain original commit ticks and serialized operation payloads. Duplicates and missing hit records have separate counts.
   Incomplete runs cannot pass final bullet-set agreement. No diagnostic cleanup changes the final sets.
5. Beam-start damage excludes measured late-end excess. The excess bound uses the actual end-processing tick minus the end tick and sender delay.
   Reports retain raw excess ticks, including excess under nominally good profiles. They do not equate nominal latency with reliable delivery time.
6. Point-defense strength uses the same numbered salvos over a perfect link with D=0.
   Bandwidth uses decimal KB and non-overlapping one-second datagram windows, including retries, framing, acknowledgements, and clock probes.
7. `Results/N-<date>.md` records source hashes, repository versions and dirty state, per-peer state and packet hashes, samples, limits, and verdicts.
   The README defines commands and metric applicability. Harness sources and results remain outside Git.

Validation: 760 engine-free tests, 22 infrastructure checks, and eight metric checks pass.
The single-player digest remains `518251C30445EA37B539A066AE26D06AB4EBCAC868D8A5AF6B2B09E685231DC9`.
Physics, Network, Core, Runtime, Core.Tests, and Runtime.Tests compile independently with Unity 6000.0.75f1's compiler and .NET Standard 2.1 references.
Production warnings are errors. Live Editor import verified (owner, 2026-10-27).

Focused validation comprises 20 scene runs and two additional point-defense baselines, at 150 ms and seed 1.
All seven scenes run on Perfect and At cap. Extra runs cover blackout beam boundaries, above-cap drift, asymmetric clocks, and Good point defense.
Blackout and asymmetric repeats retain identical state and packet hashes.

The gate does **not** pass. Workload traffic reaches about 151–153 KB/s per direction against the 16 KB/s median limit.
At-cap dogfight and churn exceed the completion deadline with reliable records still queued; above-cap dogfight also times out.
At-cap churn misses one peer's final drone-count requirement. Shooter-advantage and homing comparisons fail or lack required copies.
At-cap mutual kill leaves different final bullet sets. The asymmetric run reports a three-tick p99 offset against the one-tick limit.
These findings remain visible for the gate review. Step 8 changes diagnostics only; it does not redesign the protocol.

### Network gate matrix and roster extension

Migration phase 3b, step 9 adds a desktop batch runner in the local harness.
Production APIs, gameplay rules, and the wire format are unchanged.

```powershell
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-gate
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-roster
dotnet run -c Release --project Experimental/CombatSimulationHarness -- --network-gate-checks
```

1. The matrix runs seven scenes across six profiles and seeds 1–3 at 150 ms: 126 runs.
   At cap also runs all seven scenes and seeds at 100 and 200 ms: 42 runs.
2. Nine Perfect-link D=0 salvo baselines are cached by seed and interval. These auxiliary runs do not count toward the matrix.
3. Each run appends detailed evidence before the next run starts. Failed limits and simulation timeouts do not stop the matrix.
   Summaries retain separate failed, unmeasured, and underloaded row counts. They never average failures across seeds.
4. Interval eligibility requires every applicable At-cap row to pass for all seven scenes and three seeds.
   Selection also requires passing 150 ms cross-profile coverage and roster checks. Otherwise, the result is no passing correction interval.
   Cross-profile behavior at 100 and 200 ms is not established by this matrix.
5. The owner selected At cap, seeds 1–3, and 150 ms for the six roster case groups.
   Eight session subcases cover deploy/retreat/redeploy, two surrender variants, duplicates, two delayed-deploy variants, a ready reserve, and a missing record.
6. Intermediate assertions run before terminal cleanup. They compare committed revisions, deployment order, retained resources, active IDs, and result events.
   Combat destroys ships through the production beam device. Reserve surrender must not create synthetic entity events.
7. Harness-only transport decorators force duplicate delivery and hold operation sequence 1 while later terminal traffic arrives.
   The held packet and its retries remain blocked for 12 match-relative seconds. Standard matrix runs do not install these decorators.
8. Nine direct Core probes separately test duplicate revisions, non-owner rejection, and watermark delivery before a roster gap.
   These use public `Submit` and `Step`; they make no transport claim.
9. One missing-record session repeats to verify both state hashes, relay hashes, and injected-delivery hashes.
   Roster reporting therefore contains 34 verdicts: 24 sessions, nine Core probes, and one repeat check.

The harness is its own private Git repository (`PavelZinchenko/event-horizon-combat-simulation`, since 2026-10-27), with
`Results/` tracked. The game repository does not include it.

Since 9d P3 the matrix also runs the HomingDrones scene on the same 24 profile, seed and interval cells: 192 runs, reported apart
from the original 168. `--network-gate --tag T` writes `Results/N-<date>-T.md`.

### Network gate result

Migration phase 3b is done (2026-10-28). Every limit has a verdict; the full table, the changes that made limits pass and the
accepted defects are in `TODO/battle_simulator_network.md`, "Gate result".

1. Desktop (`Results/N-2026-10-27-9e.md`): 187 of 201 runs pass (192 matrix runs and 9 D=0 baselines). Roster 34/34.
   Outcome agreement, exactly once, bullet sets, drift, ramming, homing, recovery, point defense, beams and bandwidth pass in
   every run. Bandwidth median at most 9.6 KB/s, payload plus Relay header (limit 16).
2. Failing rows: shooter advantage at cap in 3, 5 and 7 of 42 rows at 100, 150 and 200 ms (accepted, owner); one Above-cap
   position row; one Above-cap tick offset row. The asymmetric tick offset is an expected failure.
3. Correction interval: none passes every row. The owner chose 150 ms (2026-10-27); it is the session default.
4. R1 (`Results/R1-2026-10-27-spin.md`): `--r1-host` on the desktop, `--r1-peer` on the phone over TCP and `adb reverse`, At cap,
   150 ms, Dogfight and Churn. 4 of 8 runs pass; only shooter advantage differs from the desktop verdict. The phone's session
   is replayed on the desktop: 0 differing tick hashes in 19 phone runs, so the full core is bit-identical on IL2CPP ARM64.
5. A coarse session loop raises shooter advantage and tick offset (R1 D1). A session driven by Unity frames needs its own thread
   with short loop gaps (phase 7).
