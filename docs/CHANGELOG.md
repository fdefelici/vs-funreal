# FUnreal Changelog
## v0.1.0: October 25, 2023 
### Improvements
* Update UE version dection strategy to support UE v5.3
### Bug Fix
* Fix USceneComponent class template

## v0.0.9: July 15, 2023 
### Bug Fix
* Template misconfiguration prevented to add some module (blank, blueprint library) to a plugin

## v0.0.8: April 4, 2023 
* Change UE version detection strategy to work with UE official and custom builds

## v0.0.7: December 19, 2022 
* Clean released vsix package

## v0.0.6: December 19, 2022 
### Improvements
* Add UObject to class templates
### Bug Fix
* Template not found when creating a class from Unreal Interface template

## v0.0.5: December 8, 2022 
### Improvements
* Make UE version detection from .uproject more permissive to take into account for custom ue version.

## v0.0.4: December 4, 2022 
### Features
* Create/Rename/Delete files and folders (even empty folders will be visibles and manageables)
* Create C++ classes choosing from *Unreal Common Classes*
* Create/Rename/Delete plugins choosing from *Unreal Plugin Templates*
* Create/Rename/Delete modules (for plugin modules and game modules) choosing from *Unreal Templates*