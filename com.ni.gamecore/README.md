# Ni GameCore

Package `com.ni.gamecore` version 0.1.0, Unity 2022.3.

Canonical repository: https://github.com/gameboys84/TEngineNi_UPM

Install this package together with the explicit Git dependencies in the repository README. Unity package manifests cannot declare transitive Git URLs. Project configuration, game entry, protocols and generated game scripts remain in the consuming project's Assets.

TEngineNi tracks this repository as its FrameworkPackages submodule. Do not copy package sources back into Assets or modify PackageCache. Use a Git working branch for framework development and pin tested commits in consumers.

Existing script GUIDs and TEngine assembly names are retained; shared GameCore implementation uses Ni.GameCore / Ni.GameCore.Editor with GameCore namespaces. Project-owned GameCore facades can remain separate. IL2CPP, stripping, generic AOT metadata and code hot update require project-specific validation.

Ni TEngine 0.6.0 maps to the upstream package-declared 6.0.0 baseline; this is not proof of an exact clean upstream commit. GameCore versions are maintained independently. See the repository README for compatibility, dependency installation, licenses and verification scope.
