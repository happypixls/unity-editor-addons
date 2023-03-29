# Changelog
All notable changes to this project will be documented in this file.

## [0.0.1] - 10.02.2022
### Added
- Initial version
- DLL Resolver for mobile device compilation and deployment
- Monobehaviour template
- Namespace resolver

## [0.0.2] - 14.02.2022
### Added
- Namespace resolver minor bug fix.
 
## [0.0.3] - 21.02.2022
### Added
- Fade utilities - for cross fading various materials and UI elements.
- Monobehaviour extensions - Allows more relaxed delegate invoke syntax

## [0.0.4] - 21.02.2022
### Added
- Assembly definition file for the runtime extensions.

## [0.0.5] - 21.02.2022
### Added
- References to TMP and URP for the extensions to function properly
- Added package dependency for URP

## [0.0.6] - 21.11.2022
### Removed
- Fading utilities and migrating to using LeanTween

## [1.0.0] - 15.03.2023
### Added
- Templates for creating POC classes, enums and interfaces
- Ability to create these files from context menu within Unity
- Calculators class (for now has normalizing and denormalizing numbers for a given range).
### Fixes
- Some initial code refactor
- Assembly definition creation has been modified to satisfy the same rules as other class/scripts generation
- Moving folders containing classes and asmdef files are now updated appropriately
- A lot of bug fixes :) 

## [1.0.1]
### Hotfix
- Now the namespace resolver will operate only on Editor and Scripts directory leaving any asset store plugins intact that are outside of these directories

## [1.0.2]
### Added
- Docs for MonoBehaviour extension methods
- Added check for point existing in triangle extension methods for Vector2 and Vector3