# Ni GameCore

Package `com.ni.gamecore` version 0.1.0, Unity 2022.3.

Canonical repository: https://github.com/gameboys84/TEngineNi_UPM

Install this package together with the explicit Git dependencies in the repository README. Unity package manifests cannot declare transitive Git URLs. Project configuration, game entry, protocols and generated game scripts remain in the consuming project's Assets.

TEngineNi tracks this repository as its FrameworkPackages submodule. Do not copy package sources back into Assets or modify PackageCache. Use a Git working branch for framework development and pin tested commits in consumers.

Existing script GUIDs and TEngine assembly names are retained; shared GameCore implementation uses Ni.GameCore / Ni.GameCore.Editor with GameCore namespaces. Project-owned GameCore facades can remain separate. IL2CPP, stripping, generic AOT metadata and code hot update require project-specific validation.

Ni TEngine 0.6.0 maps to the upstream package-declared 6.0.0 baseline; this is not proof of an exact clean upstream commit. GameCore versions are maintained independently. See the repository README for compatibility, dependency installation, licenses and verification scope.

## Portable logic

Runtime/Logic is a nested assembly named Ni.GameCore.Logic with no Unity engine references. It provides IndexedTable, ServerClock, BalanceLedger, OperationTimeline, PacketReader, PacketWriter, PacketFramer, PacketRouter and TcpPacketSession. Consumer asmdefs that use these APIs must reference Ni.GameCore.Logic explicitly. The Unity-facing Ni.GameCore assembly remains separate.

Drive TcpPacketSession.Pump from the host update thread. Use cancellation tokens for connection deadlines. Incoming packets are bounded by body size, queued packet count and queued body bytes. Authentication, encryption, compression and product protocol generation are separate integrations.

The portable logic is committed in this package repository; consumers currently use local development references. Its independent .NET Standard build and Unity regressions are documented in the consumer project's Doc/LogicRewrite.md and test guide. This does not update the published package commit or Ni submodule pointer.
