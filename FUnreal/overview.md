# FUnreal - Unreal Engine Extension for Visual Studio

[![version](https://img.shields.io/visual-studio-marketplace/v/fdefelici.vs-funreal?color=blue&label=latest)](https://marketplace.visualstudio.com/items?itemName=fdefelici.vs-funreal) [![install](https://img.shields.io/visual-studio-marketplace/i/fdefelici.vs-funreal?color=light-green)](https://marketplace.visualstudio.com/items?itemName=fdefelici.vs-funreal)

`FUnreal` is an extension for **Visual Studio** with the aim of improve workflow of **Unreal Engine** **C++ developers**.

Basically if you've got to the point where you write all your code in one file just because the hassle of adding new files to the project (here I am :raised_hand:), this extension is for you :wink:.

![FUnreal context menu example](docs/images/intro.png)
*Just a taste of FUnreal in action*

The main concept is an handy context menu in the **Solution Explorer** view to reach - *just a right-click away* - a bunch of useful operations without the need to launch an *Unreal Engine Editor* instance (as for creating plugins or common classes) or alternately working on the filesystem side (adding, renaming or deleting files) and then launching *Unreal Build Tool*.

Futhermore `FUnreal` will try to maintain consistent your project, updating UE descriptor files and sources depending on the scenario, so that you can keep the focus on writing code.

# Features
`FUnreal` currently supports:
* UE: 4.x and 5.x Game C++ Projects
* IDE: Visual Studio 2022 (aka v17.x)
* OS: Windows

and offers the following features:
* Create/Rename/Delete `files` and `folders` (even empty folders will be visibles and manageables)
* Create `C++ classes` choosing from *Unreal Common Classes* or *User Defined* templates
* Create/Rename/Delete `plugins` choosing from *Unreal Plugin Templates* or *User Defined* templates
* Create/Rename/Delete `modules` (for plugin modules and game modules) choosing from *Unreal Templates* or *User Defined* templates
* `Keep in Sync` UE Project and VS Solution (invoking UBT automatically)
* `Keep consistent` the code base, updating properly *.uproject, .uplugin, .Build.cs, .Target.cs*, module source file, and C++ include file directive, even cross modules, depenging on the operation executed (look at [this section](#details) for more details).

> NOTE: While using `FUnreal` extension, it is still possible to create plugins and C++ classes from Unreal Editor, from other IDE plugins or doing operations on the project directly on filesystem. The important thing is that UBT has been run succesfully and VS Solution has been reloaded.

# Documentation
Further details on how `FUnreal` activation works, how to use it in your project and related examples can be found [here](https://github.com/fdefelici/vs-funreal).