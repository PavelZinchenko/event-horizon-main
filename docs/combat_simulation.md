# Combat Simulation

`Assets/ModulesShared/CombatSimulation/` is the new battle simulator. It will replace `Assets/Modules/BattleSimulator/` ([battle simulator](battle_simulator.md)). Event Horizon and Frontier share one copy through the `ModulesShared` submodule ([Gui: Shared code constraint](gui.md#shared-code-constraint)).

Status: in progress. Physics and the phase 2 core (steps 1–11) exist; phase 2 is complete: a scripted battle runs on the core rules without Unity. Migration phase 3c is complete: quick combat runs on the core in a debug scene ([Unity integration](#unity-integration)). Core provides the fixed tick loop, spawn identity, the core-owned roster, roster commands, the battle result, the timer, ships with an engine, resources and control frames, the published snapshot and event batch, the contact pipeline with damage and ramming, the bullet store with the cannon, homing missile and beam, point defense, drone bays with persistent inventory, and the per-tick target grid; Runtime runs pilots in locked mode, including drone self-destruct requests. Stage P1 is closed (2026-10-17, with the known limits below). It has the stage P1 features: torus broad phase, circles, convex polygons, a soft-step contact solver with speculative contacts, and a free-position query. Stage P2 is closed (2026-10-18, with the known limits below): capsules, convex decomposition, compound bodies, whole-body depenetration, inner-edge contacts and the time-of-impact check of speculative contacts. Stage P3 is closed (2026-10-19, with the known limits below): collision filters, bullets with swept hits and pass-through, ray casts and circle overlaps. Stage P4 is closed (2026-10-19, with the known limits below): hinges, ropes, bullets attached to bodies, group teleport, and contact and joint impulses. The plan and its progress are in `TODO/battle_simulator_implementation.md`; stage results in `TODO/battle_simulator_physics.md`. The findings and decisions are in `TODO/battle_simulator_investigation.md`. The `TODO/` folder is local and is not part of this repository.

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
2. The session exposes `BattleId`, `Tick`, `Snapshot`, `IsCompleted`, `Clock`, and the existing roster and self-destruct command methods.
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
   **Tick batch (reliable, G2, `SessionCodec` version 7).** One reliable message per tick holds the tick's operations in sequence
   order, then its hits. Header: version (8 bits), tick and last operation sequence (`VarUInt`). No battle ID: the connection is
   bound to one battle. Then records until zero padding: kind (3 bits, from 1), record tick − batch tick (zigzag `VarUInt`; a beam
   end record is one tick ahead), then the record fields. The sender is the owner of every record.
   - **Names by stream count.** The stream is ordered and exactly-once, so both peers count its records the same way (`StreamNames`).
     A ship or drone is named by its control index (spawn-record order). A bullet is named by its age: the latest bullet index minus
     its own; each bullet-spawn record takes the next index. A removed bullet keeps its index until the end of its batch, so a hit of
     the same tick can still name it. Operation sequences are consecutive and end at the header's value; a hit's sequence is its
     position in the stream.
   - Bullet spawn: parent (1 kind bit: control index or bullet age), device, barrel and spawn sequence (`VarUInt`), launch flag
     (1 bit). With the flag (plain projectiles, 9d P1): muzzle position per axis on the correction grid (1/1024 unit, bit count
     from the arena) and launch angle (14 bits); 51 bits in a 200-unit arena. The receiver derives the entity ID with
     `SpawnIdentity.CreateChild`; Core checks it.
   - Removal: 1 kind bit, reason (3 bits), control index or bullet age. A drone's bay comes from its launch record.
   - Hit: attacker (1 kind bit plus index or age), target (1 kind bit, then a control index in the receiver's stream or, for a
     bullet, its 64-bit entity ID: the sender may know only a predicted copy, which no stream names), shape and device + 1
     (`VarUInt`), contact (2 bits), impact (3 bits), amount (32-bit float: the target owner commits damage from it), damage
     type (`VarUInt`, below `DamageTypeTable.MaxTypes`; version 6, D4). The target's core rejects a type its table lacks.
     Then an impulse flag (1 bit) and, with the flag, the push impulse (two 32-bit floats; version 7, phase 4 E4). Only a
     projectile's start record on a ship carries one; the target's core rejects it elsewhere and applies it on arrival.
   - Retreat: ship control index. Roster changes and drone launches are rare and keep full-width IDs, records and poses.
   - Missile target (kind 7, 9d P3): missile bullet age, then target control index + 1 in the receiver's stream (`VarUInt`;
     0: no target). About 1.5 bytes in a real batch.
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
   retreat starts (entity and tick only) and missile targets (missile, target, tick). Hit payloads preserve contact boundaries, authority fields, and resolved damage.
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
Only the modifiers stage is empty. Stage 6 queries beam rays and point-defense overlaps before physics advances by `0.02f` seconds with eight substeps.
Publish writes the snapshot and event batch of the current tick, then increments it.
Runtime owns pilot scheduling and network pacing through `TickClock`.

### Core authority and outgoing hits

Migration phase 3b, step 2 is implemented. Core remains transport-free.

1. `BattleSetup` accepts an optional `IReadOnlyCollection<int> localOwners`. It copies and deduplicates nonnegative player indices.
   Omission makes every roster owner local. An empty collection makes none local. `IsLocalOwner(int)` queries the frozen set.
   Owner and side remain separate; local status never changes during a battle.
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
   samples uses the oldest one. A missing beam attaches to the remote beam device. Its spawn event is committed.
   **Adoption (P2).** Before adding a missing bullet, the receiver looks for an unconfirmed copy of the same shot fired at
   another tick (an overdue fire input fires the copy late, with another derived ID). The copy must have the same parent,
   device, barrel and spawn sequence.
   - Beam: the device's attached beam, at any start tick. The owner stopped any earlier beam before this one, so the attached
     beam is a prediction.
   - Projectile: spawn tick within the cannon's shortest shot spacing − 1 ticks (max of cooldown and barrel cooldown), so the
     copy can be no other owner shot. The nearest spawn tick wins, then the earlier one. A wider window would shift every
     copy of a held burst.
   The copy takes the owner's ID, spawn tick and expiry and becomes `Confirmed`; a plain projectile is then re-aimed (P1).
   The body, beam attachment and contacts so far stay. The battle emits a committed `Rename` event (`PreviousId` → `EntityId`);
   the scene moves the bullet view to the new ID. Confirmation changes no recoil or energy; corrections own those (step 5).
   **Timer re-sync (G1).** A confirmed shot at tick T sets the copy's cannon last shot to T, unless the copy's last shot is at or after
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
8. After a correction with acknowledgement A at tick t, the copy replays the damage the owner has not applied:
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
The sequence counts spawns with the same parent, tick, source and slot, in creation order. It covers a `SpawnBullet` trigger with `Quantity` > 1 and a device that fires more than once in a tick. Spawn ticks must be nonnegative; a child requires a nonzero parent ID.
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

   A target that stops at the same time stays valid, so two bullets that hit each other both take the hit. A hit on a bullet with `TargetHitPoints` above 0 adds its damage to the target's damage taken. The hit that brings it to the hit points stops the target at that time. Several weak hits in one step therefore stop a missile at the lethal one. A candidate with `OncePerTargetEntity` (damage over time, D16) counts once per attacker and target entity per step. Ships never become invalid within a step.
4. **Ramming (Q8).** Stage 4 stores each active ship's linear velocity before the step (`PreStepVelocity`). Physics reports a touch only after the solve, when the velocities have already changed. `RammingTracker` counts the touching shape pairs per enemy ship pair. At stage 8 it handles begin events first, then end events. When a count goes from 0 to 1, it adds one candidate per direction at time 1. The damage is `0.1 × |vA − vB| × min(mass A, mass B) × the attacker's RammingDamageMultiplier`, and it applies only above 1 (legacy `DefaultCollisionBehaviour`). The candidate's target shape is the smallest (priority, order) among the pair's begin events in that step. A removal forgets every pair of the removed ship. Ally pairs are not tracked.
5. **Damage (stage 10).** Stage 9 adds each accepted hit on a ship or a missile to its `PendingDamage` and emits a `Hit` event. A ship keeps one pending amount per damage type. Stage 10 applies the totals in one pass, as legacy `Impact.ApplyShield` and `ShipStats.ApplyDamage` do (see "Damage types"). A missile loses hit points (legacy `MissileDamageHandler`). The resolver and stage 10 add the same values in the same order, so they agree on a missile's death. Stage 10 applies damage only to local targets; the authority rules above decide which contacts supply that damage.
6. **Deaths (stage 11).** A ship with armor 0 or less is marked; stage 12 removes it with reason `Death` and a `Killed` transition. A missile with hit points 0 or less gets removal reason `Death`; it wins over an expiry in the same tick. `QueueDeath` remains as a test hook.

### Damage types

Phase 4 step 1 (D4, owner 2026-10-28): the legacy formula, with every type rule as data. Examples: `TODO/battle_simulator_phase4.md`, "Damage examples (D4)"; tests: `Core/Tests/DamageTypeTests.cs`.

1. **Table.** `BattleSetup.DamageTypes` is a `DamageTypeTable`: one `DamageTypeDefinition` per type and a `RammingDamageType`. A damage type is an index into it. The default is `LegacyDamageModel.Types`: impact, energy, heat, corrosive in database enum order; ramming deals impact.
2. **Sources.** `BulletDefinition`, `BeamDefinition` and `PointDefenseDefinition` carry `DamageType` (point defense defaults to energy, legacy `PointDefenseCollisionAction`). `BattleSetup` rejects a type its table lacks, on roster ships and their drones.
3. **Resistances.** `ShipDefinition.Resistances` holds one `DamageResistance` per type, or none (every type in full). `Armor` is the fraction removed before armor, `Shield` the fraction removed before the shield, `EnergyGain` the energy per point that passes the shield. `Armor` and `Shield` are at most 1, so no damage heals. `LegacyDamageModel.Resistances` builds them from the legacy percentages: corrosive takes half of the smallest armor resistance and the shield corrosive resistance; energy absorption over the armor multiplier is the energy gain.
4. **Stage 10.** The shield meets the types group by group in `ShieldOrder` (legacy: corrosive first). A type's load is its damage times `1 − Shield`. A group within the shield left is absorbed; a larger group keeps the same share of each type, and the shield empties. The shield restarts its delay. Armor then takes each remaining amount times `1 − Armor`. Energy gains each remaining amount times `EnergyGain`, up to its maximum, without restarting its delay.
5. **Bullet targets** take damage without types or resistances (legacy `MissileDamageHandler`).
6. **Network.** A hit record carries its damage type. Prediction and the correction replay use the copy's resistances and the same function.

### Result and timer

Stage 13 evaluates elimination first: a side whose slots are all `Destroyed` loses; both sides in one tick give a draw. A `Ready` reserve keeps its side alive.
Then the timer: it advances one tick only while both sides have an active slot (B5). At `TimeLimitTicks` (0 means no limit) the battle ends in a draw with reason `Timeout`.
Any ship deployment or removal on either side resets the timer to zero (owner, 2026-10-20). The tick of the reset counts when both sides then have a ship. This replaces the legacy player/enemy rules until phase 4 adds `TimeOutMode`.
`BattleResult` holds outcome (`Victory` or `Draw`), winning side, reason (`Elimination` or `Timeout`) and the original commit tick.
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
8. **Filters.** `CollisionFilters` gives one category bit per side and class, following the legacy layer matrix. Ship hulls are `Ship(side)` and ignore nothing. An energy bolt is `Energy(side)` and ignores its own side's ships and all energy bolts. It hits enemy ships and missiles and passes allies and other bolts. A missile is `Missile(side)` and ignores its own side's ships, bolts and missiles. It meets enemy ships, missiles and bolts.
9. **Hits.** At stage 8 each physics hit on a live ship becomes a candidate at its sweep fraction (`BulletSweep`). Q1: a cannon bullet stops at its first valid hit, so stage 9 marks it for removal with reason `Death`. The damage then follows the ship pipeline: shield first, then armor. A ship takes each damage effect (`Damage` and `ExtraDamage`) with its own type; a missile and the hit event take the total. A remote ship gets one hit record per damage effect. A hit on a bullet becomes a candidate only when the target is a missile, with the missile's hit points as `TargetHitPoints` (see Missiles and target grid).
10. **Lifetime.** Range / speed, with a `>` check: `LifetimeTicks`. A bullet spawned at tick s flies in the physics of ticks s to s + `LifetimeTicks` − 1. Stage 11 of the last tick marks it `Expiry`.
11. **Removal (stage 12).** Bullets leave after the ship changes, in entity ID order, so the events do not depend on storage. A bullet outlives the ship that fired it; physics ignores its stale owner body.
12. **Push (phase 4 E4).** A hit on a ship or drone adds `Push` × the bullet velocity as an impulse at the target's origin (legacy `PushAction`: no spin; negative pulls). Missiles are never pushed: legacy `MissileDamageHandler` drops impulses. On a remote target the attacker's copy moves at once as a prediction; the first hit record carries the impulse, and the target owner applies it when the record arrives.

### Missiles and target grid

Phase 2 step 7 (Q2a, Q2b). The rules port legacy `HomingController` on new ammunition, with `W = 1 / (0.2 + 2 × weight)`.

1. **Missile.** A bullet with hit points above 0 is a missile (legacy `UnitClass.Missile`). It stops at its first valid hit on a ship or a missile (legacy `DetonateAtTargetAction`): impact damage only. Area explosions come with triggers in phase 5 (owner, 2026-10-20).
2. **Bullet contacts.** A bolt that hits a missile deals its damage and stops. Two missiles that meet both detonate, whatever their hit points: the candidate of each one stops it. A missile that meets a bolt deals nothing and does not stop (legacy `BulletDamageHandler`); only the hit of the bolt counts.
3. **Lifetime.** Range / speed, as for every bullet (Q2a; the legacy 1.2 factor is not kept).
4. **Target grid (R9).** At the start of stage 4, `TargetGrid` takes every active ship: entity ID, side, position and hull radius, at the poses at the start of the step. It is a uniform torus grid with 32 u cells, rebuilt once per tick. It allocates nothing once its arrays fit the battle. Drones join it on the tick after launch: stage 4 precedes the stage 5 spawn.
5. **Target rule.** Range is the centre distance at the nearest torus image minus both radii. The search covers 1.3 × the range of the missile. Inside it, the nearest enemy within 30° of the heading wins. Without one, the enemy with the smallest deviation wins. Equal keys go to the lower `EntityId`, so the result does not depend on grid order. Beyond the radius there is no target (owner, 2026-10-20). Legacy chases an enemy at any distance, and its choice depends on list order. Target priority, decoys and `ChanceToAvoidMissile` come later.
6. **Search timing.** Legacy runs no search at creation, so the first search is in the tick after the launch. The next one comes 101 ticks later (`TickTime.After(2 s)`). A removed target starts a search at once, at stage 4 of the next tick (redesign, owner, 2026-10-20). Every search restarts the period.
7. **Steering (stage 4, `HomingSteering`).** Below full speed along the heading, the velocity changes by `normalize(speed × heading − v) × 0.5 × speed × W × dt`. This also removes sideways velocity. The turn rate is `120 × W` deg/s toward the target, and 0 inside ±5° or without a target. The rate is set, not accelerated. Physics never turns a bullet, so the core sets the heading after the turn before the physics step. The legacy body has the same heading after its step. Smart aim and `IgnoreRotation` come later.

### Beams

Phase 2 step 8 (Q3). Immediate shutdown replaces the legacy lifetime tail (owner decision, 2026-10-20).

1. **Definition.** `BeamDefinition` is a device with barrel index, cooldown seconds, activation cost, energy per second, range and damage per second. Values must be finite; range is positive and costs, cooldown and damage are nonnegative. Phase 2 uses a fixed muzzle direction without spread or recoil.
2. **Activation.** Stage 2 checks the held bit, weapon cooldown, shared barrel cooldown and activation payment. The first active tick pays only activation energy. Later ticks pay `EnergyPerSecond × 0.02` after resource recharge. Each successful tick deals `DamagePerSecond × 0.02`. Failed payment spends nothing and gives no partial damage. The existing resource rule requires positive available energy even for a zero cost.
3. **Shutdown.** Release, failed payment or retreat start stops the ray before stage 6 and starts cooldown on that tick. A held bit can restart after cooldown and energy recovery, but never on the shutdown tick. A beam records one barrel shot on activation; maintaining it does not occupy the barrel.
4. **Lifetime and identity.** Stage 5 creates one entity in `BulletStore` using the normal child ID. It has no physics body or collider, cannot be hit, and has no projectile expiry. The same ID lasts through the activation. Stage 12 removes stopped beams with `Disposal`. Parent removal marks attached beams before releasing parent state; projectiles still outlive their parent.
5. **Queries.** Stage 6 samples the current muzzle pose and performs one pass-through ray cast with eight reusable hit slots. It uses the energy collision filter and ignores the parent body. Enemy ships and missiles block the beam; friendly ships, friendly missiles and energy bolts do not. The ray and its no-hit endpoint use the physics limit of the smaller arena dimension.
6. **Selection.** Stage 8 collects the retained ray candidates; stage 9 selects the nearest valid target after disarms. A hit consumes this tick's ray, not the beam entity. Shield priority breaks equal-distance ties before entity ID and shape order. Damage-over-time deduplication limits each attacker/target pair to one hit per tick.
7. **Overflow.** If all eight returned targets are invalid and physics reports overflow, that tick has no hit. `BeamQueries.ExhaustedRayCount` increments and a warning identifies the battle, tick and beam. Overflow with an accepted target does not increment it.
8. **Validation.** `BeamTests` covers legacy LaserBeam_S_1 values, energy boundaries, cooldowns, retreat, identity, query ordering, shields, missiles, wrapping, snapshots and exhausted buffers. `PointDefenseTests` also checks real disarms through complete battle steps, including exhausted ray buffers. Sustained hits and repeated activations allocate zero bytes after warm-up.

### Point defense

Phase 2 steps 9 and 10 (Q4). The owner chose payment before each zap and legacy checking while ready. Step 10 adds drone energy damage.

1. **Definition.** `PointDefenseDefinition` supplies radius, cooldown seconds, energy cost and optional nonnegative `droneDamage` (default 0). Radius is positive; cooldown and cost are nonnegative. All values are finite. The device needs no barrel. The legacy fixture uses radius 4 (size 3 plus hull radius 1), cooldown 0.3 s and cost 1.
2. **Activation.** Stage 2 checks the held bit and pays the energy cost once. Release or retreat disables the device before queries. Failed zap payment also disables it. Reactivation needs payment and `TickTime.AtLeast(cooldown)` ticks since shutdown. Zero-cost activation and zaps retain the existing resource rule requiring positive available energy.
3. **Query timing.** The first query occurs at first activation tick plus `TickTime.After(cooldown)`. A successful query processes all affordable targets, then the next query waits the same delay. At 0.3 s the delay is 16 ticks. Empty queries leave the device ready, so it queries again next tick. Disable/re-enable preserves the query deadline; redeployment creates fresh state.
4. **Overlap.** Stage 6 queries a circle at the ship's current position, before physics. It includes enemy missiles and energy bolts spawned at stage 5. Enemy drones are also included. Allies, roster ships, beams and bullets pending removal are excluded. Physics measures distance to the target surface and handles torus wrapping. The reusable buffer starts at 32 entries and grows on overflow until every target fits.
5. **Payment and ordering.** Projectile overlaps become time-zero `Disarm` candidates. Drone overlaps become time-zero `Hit` candidates, one per device and drone, including compound hulls. Disarms sort before hits, so projectiles receive payment first. Drone damage reduces shield, then armor at stage 10; it never drains energy. Drones remain valid targets throughout the contact walk, even after lethal accumulated damage. The ship ID and device index identify the attacker. Stage 9 checks contact validity before payment. A failed payment spends nothing, leaves the target valid and stops this device's remaining zaps. A target already disarmed by another device costs nothing and does not reset the skipped device's query deadline. Accepted zaps restart the energy recharge delay.
6. **Removal and events.** An accepted disarm immediately invalidates the bullet within the contact walk, before any time-zero hit. Projectiles and beams can then select valid targets behind it. Stage 9 emits `BattleEventKind.Disarm` with attacker ID, target ID, `DeviceIndex` and the target position sampled at stage 6. Stage 12 removes the bullet with `Disposal`. The device has no separate entity.
7. **Authority.** Defense energy and cooldown are simulated on both copies. The bullet owner commits destruction; the other copy removes the bullet by prediction.
8. **Validation.** All 534 engine-free tests pass, including 29 point-defense cases. Coverage includes payment, cooldowns, retreat, redeployment, filtering, wrapping, same-tick spawns, overlapping devices, allocation order, real beam/projectile continuation and 70-target salvos. Repeated interception and activation allocate zero bytes after warm-up. The large-salvo test also fixed a resolver bug: array growth must finish before the disarm array reference is fetched.

Core and Core.Tests compile separately with Unity 6000.0.75f1's compiler and assembly references. Unity Editor imported the new sources and compiled both assemblies.

### Drone bays and self-destruction

Phase 2 step 10 (Q5/Q6). A drone is a full `ShipEntity` without a roster slot. Its bay belongs to `(RosterId, device index)`.

1. **Definition and inventory.** `DroneBayDefinition` supplies positive capacity, nonnegative finite launch energy cost and a drone `ShipDefinition`. Nested bays are rejected, as legacy excludes bays on drones. Each roster slot owns persistent bay inventory. Launches consume hangar stock; deaths never restore it. Surviving drones and unlaunched stock remain through retreat and redeployment. Replication is deferred.
2. **Launch timing.** Held controls launch while stock, energy and the shared mothership bay timer permit. The timer is separate from weapon barrels. It uses `TickTime.After(0.4f)`: first launch at deployment tick + 20, subsequent launches at least 21 ticks apart. Device index resolves competition between bays. Failed payment spends nothing. Release preserves readiness; retreat blocks launch; redeployment resets the timer.
3. **Intentional timing correction (owner, 2026-10-21).** Legacy `ShipFactory.CreateDroneBays` registers the same platform twice. `ShipSystems` therefore advances its 0.4-second timer twice per tick. The new core advances it once and uses the declared duration.
4. **Spawn.** Stage 2 pays and queues; stage 5 creates at the mothership position and heading, with zero velocity and empty controls. The child ID uses the current mothership ID, tick, device index, lifetime launch slot and sequence. The free-position query searches up to 40 units. On failure it still launches at the returned least-overlapping position and retains the payment (owner, 2026-10-21).
5. **Lifecycle.** Mothership retreat or destruction does not remove drones. Redeployment exposes the new mothership ID through the same bay without resetting drone resources, controls or pilots. Surrender and kill-all destroy roster slots; surviving drones remain in the frozen result snapshot. Drones do not consume roster deployment limits, prevent elimination, prevent timer pauses or reset the timer (owner, 2026-10-21).
6. **Combat.** Drones use the ship engine, resources, devices, damage and ramming pipeline. They touch all drones and enemy roster ships, but pass through allied roster ships. Enemy projectiles and beams hit them. Newly launched drones participate in same-tick point-defense queries and physics. Their first systems/motion update and target-grid entry occur next tick.
7. **Public state.** Ship snapshots and spawn/removal events include nullable `BayKey` and `IsDrone`; a drone's `RosterId` is default. The snapshot supplies the current `MothershipId`; events use `ParentId`. An absent mothership has a null entity ID. No drone removal creates a roster transition. Both snapshot buffers grow to include drones.
8. **Pilot command.** `SelfDestructCommand` carries battle ID, command ID, target tick and entity ID. `Submit` rejects wrong battles, past ticks, null IDs and duplicate pending command IDs. Stage 1 processes it after roster commands and before frames, rejecting targets that are not live drones. Stage 11 marks death; stage 12 removes the drone normally. Runtime exposes `SelfDestruct` and forwards `PilotControls.SelfDestruct` at the locked delay. Providers select `PilotSchedule.Drone` (delay 2, period 2). Pilots decide whether an absent mothership warrants self-destruction.
9. **Allocation.** Pooled ships cache device instances per definition and reset their transient state on reuse. The ship free list reserves capacity as the pool grows. Repeated launches, drone deaths and weapon fire allocate zero bytes after warm-up. Sustained drone zaps also allocate zero bytes.

### Snapshots and events

Stage 14 writes a `BattleSnapshot` of the tick it completed, then increments `Battle.Tick`. `Battle.Snapshot` returns the published one. The constructor publishes tick −1: every slot `Ready`, no ships, no events.

1. Contents: battle ID, tick, timer, result, `Ships` (active roster ships in roster order, then drones by entity ID), `Bullets` (live bullets in storage order; the order has no meaning), `Roster` (every slot in setup order) and `Events` (the tick's presentation batch), plus `OutgoingHits` (resolved contacts for remote ship and drone owners, and contacts on remote bullets for their owners). The lists are `ReadOnlySpan` views over arrays the battle reuses.
2. `ShipState`: entity, roster ID, side, owner, definition, pose and velocities from the physics body, armor, shield and energy (value and maximum), the controls in force and the retreat completion tick.
3. `BulletState`: entity, parent entity, side, owner, definition, pose, velocity and expiry tick. Projectiles retain `Definition` (`BulletDefinition`). Beams set `BeamDefinition` and `IsBeam`, with `Definition` null. Their pose is the sampled ray origin and angle; `BeamEndPoint` and `BeamTargetId` describe the accepted hit or full range. Beam velocity is zero and expiry is `long.MaxValue` because the device owns lifetime.
4. `RosterSlotState`: roster ID, side, owner, status, deployment count and order, revision, active entity (null unless active), the retained B4 deficits and `DamageFraction` (B2).
5. `DeviceState` (phase 4, step 3b): `GetDevices(ship)` returns one entry per device of a `ShipState` from the same snapshot, in control-bit order. `Ready` is legacy `CanBeActivated` for the next tick with the end-of-tick resources; the bit does not count. `Active` is legacy `SystemBase.Active`: the bit was held and the device's checks passed before it spent energy. `Cooldown` is the HUD fraction (0 ready, 1 blocked). Drone bays add `InHangar` and `ActiveDrones`; beams add the attached `Beam`. Inert devices are never ready or active and show cooldown 1. No peer sends it: each derives it from its own simulation. The rules per device are in [the phase 4 plan](../TODO/battle_simulator_phase4.md#device-state-3b).
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

Migration phase 3c. Quick combat runs on the core in a separate scene; the old scene stays the default. The plan and its decisions (S1–S5) are in `TODO/battle_simulator_scene.md`.

| Part | Location |
| --- | --- |
| Adapter, ship mapper, entity ID map, pilots, keyboard driver | `Assets/Scripts/Combat/Integration/Core/` |
| Views | `Assets/Modules/CombatView/` (references Core and Physics; the core never references it) |
| Installer | `Assets/Scripts/Installers/CoreCombatSceneInstaller.cs` |
| Scene | `Assets/Scenes/CoreCombatScene.unity` (`GameScene.CoreCombat`) |
| Editor entry | `Assets/Editor/CoreCombatLauncher.cs`: menu `Tools/Combat/Start Core Quick Combat` |

### Scene selection

`@` in the `TestShipId` debug setting selects the new scene, with or without ship IDs (`@34`). `QuickCombatState.RequiredScenes` then returns `GameScene.CoreCombat`. Outside the Editor the flag needs an editable database. The state binds the same `ICombatModel` for both scenes.
`CoreCombatSceneInstaller` binds only the adapter, its scene presentation, the HUD presenters, the HUD panels and input. It binds no legacy `CombatManager`, `Scene`, AI or factories.

### Adapter

`CoreCombatAdapter` owns the `BattleRunner` and implements the application contracts: `IRosterView`, `IBattleView`, `IShipViews`, `IShipControlSource` and `ICombatCommands`. `CoreScenePresentation` implements `IScenePresentation` over it. The HUD reads these contracts and works unchanged.

1. **Setup.** Player slots get roster IDs 1…n, then enemy slots. The arena is `BattleMapSize` square. The seed comes from `UnityEngine.Random`. There is no time limit: the core draws at timeout, but quick combat never ends on time.
2. **Driver.** `Tick` adds `Time.deltaTime` to an accumulator and steps every `0.02` s, at most 5 steps per frame. A longer frame drops the rest. The adapter handles each tick's events right after its `Step()`, so no event is lost.
3. **Projection.** Slots and ship views copy what they need from the snapshot. They keep no spans. `EntityIdMap` gives each core entity a sequential application `EntityId` at its spawn event and releases it after the removal. The 64-bit core ID is never truncated.
4. **Commands.** Deploy, retreat, surrender and kill-all go to the runner. One retreat request per ship. Surrender steps once at once, because the menu pauses the game.
5. **Battle flow (legacy quick combat).** The enemy deploys its first ready ship at the start and 3 s after its active ship leaves. When an enemy is active and the player has none, the selection panel opens: at once at the start, 3 s after a player ship leaves. `ShipSelection` modes that cannot choose deploy the first ready ship. The open panel pauses the game, so a deployment applies when it closes.
6. **HUD values.** The timer panel shows while both sides have an active ship, without digits. `CanChangeShip` follows the legacy rule. `CanCallReinforcement` is false. `CoreShipControls.GetCooldown` returns the device `Cooldown`; the adapter copies the device states after each step.
7. **Completion.** At the result, stepping stops and the last frame stays on screen. After 3 s of real time the adapter fires `ExitSignal` once. Real time, because surrender comes from the open menu, which pauses the game. `QuickCombatState` returns to the main menu. Quick combat has no rewards.
8. **Unload.** `Dispose` drops the runner and the views without roster events.

### Ships and pilots

`CoreShipMapper` maps each ship build (`IShipSpecification`) to a `ShipDefinition` (phase 4, step 3a). The rules and examples are in [the phase 4 plan](../TODO/battle_simulator_phase4.md#ship-mapping-3a). Armor, shield, energy, weight, ramming and resistances come from the ship stats. The engine uses the legacy `EngineStats` formula, with degrees converted to radians. The device index is the legacy system index: weapons by platform, then devices, then clonning centers, the replicator and the drone bays. Hull barrels become `BarrelDefinition`s; satellite weapons have no barrel (D9). `CoreWeaponMapper` maps a weapon with new-format ammunition to a cannon or a beam. Point defense and drone bays map to their core devices; a drone is mapped from its build with the bay multipliers. Every other part becomes an `InertDeviceDefinition`: it keeps its index and does nothing (D1). Weapons with obsolete ammunition stay inert; the built-in catalog has none since the conversion (step 4c).
The mapper also returns a `CoreShipMapping` per ship and drone: key binding, button icon, activation cost and inert reason per device. The adapter logs the inert devices once per battle.
`CoreHullBuilder` builds the hull from `Ship.Collider` times `ModelScale`. It skips an invalid shape with one warning per ship. A ship with no valid shape gets the legacy sprite collider: a circle around the sprite for `SizeClass.Undefined`, otherwise the sprite physics shape reduced with `ColliderTolerance`. It runs on the main thread; the core receives shapes only.
Editor menus: *Tools/Combat/Check Ship Hulls* builds the hull of every built-in ship from the data and from the sprite and reports parts, inertia and radius. *Tools/Combat/Check Ship Builds* maps every built-in build and reports the armed builds, the behaviour trees they use and the inert reasons with counts.

`CorePilotProvider` gives each player ship a new `PlayerPilot` (`PilotSchedule.PlayerShip`) and every other ship a `ChasePilot` (decision S3). `ChasePilot` is a copy of the scripted battle's pilot; its stand-off distance comes from the weapon range. It never holds an inert device and always holds other devices with a key. Behavior trees replace it in phase 4.
`PlayerPilot` implements `IShipCommandSink` and submits the stored throttle, course and system bits each tick. `CoreAutoActivation` gives every pilot the legacy rule for devices with a key below 0: the pilot holds the device while energy fraction + 0.1 × (key + 1) ≥ 0, plus activation cost ÷ maximum energy while the device is `Active`. `CoreShipControls` groups the other devices by key, as legacy `GetKeyBindings`. A new deployment gets a new pilot, so it starts with empty controls.
`CoreKeyboardDriver` applies the legacy `KeyboardController` rules over `IKeyboard` and `IMouse`. It writes a value to the sink only when its own value changes, so HUD buttons keep theirs. A key held across a ship change acts on the new ship, as in legacy.

### Views

`BattleRenderer` (`CombatView` module) draws ships and bullets as sprites under one root in the combat scene. Scenes load additively and the active scene never changes, so the root is moved into the combat scene.

1. Ships use the model sprite at scale `ModelScale`. Bullets are pooled generic sprites tinted by side. Beams, homing bullets and drones get the generic sprite and one warning per kind until phases 4-5.
2. A removal by death fades the ship out in 0.4 s. There are no other effects or sounds.
3. Each view keeps the previous and current pose and interpolates with alpha = accumulator / 0.02 s.
4. Torus wrap: the controlled ship goes to its image nearest to the anchor, and the anchor follows it. Every other view goes to its image nearest to the new anchor. `IShipView.VisualPosition` is that image, so the radar and the camera agree. Drawn coordinates can leave the base map; simulation positions are never written back.
5. The camera uses the legacy `Combat.Scene.ViewRect` framing of the player and the target, and zooms in on each player deployment.

### Known limits

1. Most weapons and devices are inert until the ammunition conversion (step 4) and phase 5; most built-in builds cannot fire yet. Auto-aiming barrels fire straight; satellites are not attached.
2. No time limit, reinforcement, `OnlyOneShip` rule or reward. Campaign battles still use the old scene.
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
