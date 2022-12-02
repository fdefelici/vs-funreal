# FUnreal - Unreal Engine Extension for Visual Studio

[![version](https://img.shields.io/visual-studio-marketplace/v/fdefelici.vs-funreal?color=blue&label=latest)](https://marketplace.visualstudio.com/items?itemName=fdefelici.vs-funreal) [![install](https://img.shields.io/visual-studio-marketplace/i/fdefelici.vs-funreal?color=light-green)](https://marketplace.visualstudio.com/items?itemName=fdefelici.vs-funreal)

`FUnreal` is an extension for **Visual Studio** with the aim of improve workflow of **Unreal Engine** **C++ developers**.

Basically if you've got to the point where you write all your code in one file just because the hassle of adding new files to the project (here I am :raised_hand:), this extension if for you :wink:.

![FUnreal context menu example](./docs/images/intro.png)
*Just a taste of FUnreal in action*

The main concept is an handy context menu in the **Solution Explorer** view to reach - *just a right-click away* - a bunch of useful operations without the need to launch an *Unreal Engine Editor* instance (as for creating plugins or common classes) or alternately working on the filesystem side (adding, renaming or deleting files) and then launching *Unreal Build Tool*.

Futhermore `FUnreal` will try to keep consistent your project, updating UE descriptor files and sources depending on the scenario.

# Features
`FUnreal` currently supports:
* UE: 4.x and 5.x Game C++ Projects
* IDE: Visual Studio 2022 (aka v17.x)
* OS: Windows

and offers the following features:
* Create/Rename/Delete `files` and `folders` (even empty folders will be visibles and manageables)
* Create `C++ classes` choosing from *Unreal Common Classes*
* Create/Rename/Delete `plugins` choosing from *Unreal Plugin Templates*
* Create/Rename/Delete `modules` (for plugin modules and game modules) choosing from *Unreal Templates*
* `Keep in Sync` UE Project and VS Solution (invoking UBT automatically)
* `Keep consistent` the code base, updating properly *.uproject, .uplugin, .Build.cs, .Target.cs*, module source file, and C++ include file directive, even cross modules, depenging on the operation executed (for more details read [here](./docs/DETAILS.md)).

> NOTE: While using `FUnreal` extension, it is still possible to create plugins and C++ classes from Unreal Editor or doing operations on the project directly on filesystem. The important thing is that UBT has been run succesfully and VS Solution has been reloaded.

# Activation
`FUnreal` starts automatically when detects an UE Project, even if the actual activation dependends on Visual Studio extension loading chain (so you couldn't see the context menu right away just after opening VS). Anyway, you can be aware when `FUnreal` have been loaded in two ways:
* A temporary notification message in **VS Status Bar**
* A dedicated Output window named **FUnreal**

![FUnreal notification](./docs/images/notify.png)

# Usage
Once active, `FUnreal` context menu is available in the **Solution Explorer** view on the following items:
* `Game Project` and `.uproject` file
* `Plugin directory` and `.uplugin` file
* `Module directory` and `.Build.cs` file (for both plugin modules and game modules)
* Any `folder` or `file` within a Module directory (or multiple selection of them)

> On these items, you should find `FUnreal` menu at the very first position of the context menu.

After performing the selected operation, `FUnreal` will run **Unreal Build Tool**, so you should receive at end the usual VS dialog advising that the project has been modified externally and need to be reloaded.

That's all! Enjoy :+1:

# Changelog
History of changes [here](./docs/CHANGELOG.md).