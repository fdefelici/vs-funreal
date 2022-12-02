# FUnreal 'under the hood'

While executing operations, `FUnreal` tryies to keep UE Project consistent - following UE standards and best practices - doing all the heavy lift for your. 

Here a the detail of the project items that will be checked and potentially updated when doing operation on the following elements:
* [Plugin](#plugin)
* [Plugin Module](#plugin-module)
* [Game Module](#game-module)
* [Source Folder](#source-folder)
* [Source File](#source-file)

## Plugin
CRUD operation on a plugin could produce updates to:
* Plugin Directory
* .uplugin (and .uplugin of other plugins depending on it) 
* .Build.cs of other modules depending from the modules of the current plugin
* .uproject

## Plugin Module
CRUD operation on a plugin module could produce updates to:
* Module Directory
* Module .cpp (that contains macro: IMPLEMENT_MODULE) and .h (retrieved as a symmetric path to the .cpp)
* \<MODULE\>_API macro (updated only for Public headers)
* Other modules sources (see [Source File](#source-file) section) dependent from the current module
* .Build.cs (and .Build.cs of other modules depending on it) 
* .uplugin

## Game Module
CRUD operation on a game module could produce updates to:
* Module Directory
* Module .cpp (that contains macro: IMPLEMENT_MODULE, IMPLEMENT_GAME_MODULE, IMPLEMENT_PRIMARY_GAME_MODULE) and .h (retrieved as a symmetric path to the .cpp)
* \<MODULE\>_API macro (updated only for Public headers) 
* Other game module sources (see [Source File](#source-file) section) dependent from the current module 
* .Build.cs (and .Build.cs of other game modules depending on it) 
* .Target.cs (in particular for creation operation *ExtraModuleNames.AddRange* pattern is used.)
* .uproject

## Source Folder
When renaming a source folder in a module **M** which involve **header files** (.h):
* **M** directory is scanned for updating #include directive in other source files (.h, .cpp). 
* Futhermore, in case of **Public** headers all modules depending on **M** will be scanned for updating #include directive as well.

A note on **Empty folders**: they are made 'visible' in Solution Explorer even if UBT standard behaviour is to not add them to the VS Solution

## Source File
When renaming an **header file** (.h) in a module **M**:
* **M** directory is scanned for updating #include directive in other source files (.h, .cpp). 
* Futhermore, if it is a **Public** header all modules depending on **M** will be scanned for updating #include directive as well.
