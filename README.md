# Haxxis

Haxxis is a visualization system based on the Unity3D framework which quickly transforms raw data into 3D-rendered graphics for either manual examination or output to MP4.

This project consists of three major parts:
* The Package Editor allows the user to construct a pipeline that dictates how data is represented in the resultant graphic.
* The Renderer takes the defined pipeline, fetches the data required, and creates the visualization objects for viewing.
* The Choreography allows for programmatic control of the camera, letting the user define a specific path to move along for autonomously-generated videos.

Full documentation on the use of Haxxis can be viewed [here](https://voidalpha.github.io/cgc_viz/).

Haxxis can be paired with a Video Generation System to be able to produce videos in bulk.

# Haxxis's License

Copyright (C) 2017  voidALPHA

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

## General usage notes:

View help:
1. Press `~` to open dev command console
2. Type 'help' and hit enter to view available command types.
3. Enter 'help' followed by any command type to view command options (such as 'help cla').

Test Haxxis:
1. Double-click Haxxis.exe to start the Haxxis client
2. Click the "Load" button in the upper right
3. Select "AcidTest.json"
4. To see the visualization, click the "Eval" button in the upper right (or hit `F5`)
5. If you don't see the visualization try hitting `F12` to point the camera at world origin
6. To see choreography, on the bottom section of the screen, hit the play icon button next to 'Choreography Sequencer' on the left (or hit `Shift-F5`)

# Hot keys list:

Display Toggles:
* `Tab` - Chain Editor
* `Shift-Tab` - Choreography Editor
* `~` - Dev command console (type 'help' or 'help -full' to list dev commands)
* `Shift-Alt-S` - System stats lines

Camera Manipulation:
* `C` - Toggles free camera mode (We default free camera mode ON)
* `W`, `A`, `S`, `D`, `Q`, `E` - Free camera movement (forward, left, back, right, down, up)
* `Shift` - Accelerated movement when used with the controls above
* `RMB-Drag` - Point free camera
* `MMB-Drag` - Move free camera

F-Keys:
* `F2` - Open the Settings window
* `F3` - Search for instances of strings in the Workspace
* `F4` - Show debug json instance IDs on node views
* `F5` - Evaluate currently loaded Package
* `Shift-F5` - Start the choreography for the currently loaded Package
* `F6` - Cancel evaluation of Package
* `F7` - Save current Package
* `F8` - Save current Package to a new file
* `F9` - Load a new Package
* `F10` - Add a Package to the currently loaded one
* `F11` - Start a new Package
* `F12` - Point camera at 0,0,0

## Haxxis Editor:

In the Workspace:
* `LMB-Drag` - Moves the hovered chain node or group
* `MMB-Drag` - Pan around the entire workspace
* `RMB-Click` or `Shift-Ctrl-T` - Add a node (brings up the node picker; node will be added at selected location)

On a Node Header:
* `LMB-Click` - Expand/collapse node
* `LMB-Drag` - Move node
* `Shift-LMB-Drag` - Move node and all of its children
* `Shift-Click` - Toggles selection of node

Misc:
* `Shift-Ctrl-D` (or `Ctrl-D` in standalone) - Duplicate selected Choreography Step
* `Control-LMB-Drag` node or group header - Move node or group between groups
* `Control-Z` - Undo last operation
* `Control-Y` - Redo last undone operation

## Debugging:

* `Shift-Ctrl-F1` - Dump debug info about Canvases to Debug Log

## Filament View:

* `LMB-Drag` - Move the filament cursor

# Licenses of Software Used by Haxxis

* The team icons found in the [CGC_team_icons folder](Assets/Materials/CGC_team_icons) is property of their respective teams.
* Haxxis uses libraries from the FFmpeg project under the [GPL v2 license](https://www.gnu.org/licenses/old-licenses/gpl-2.0.html) in order to produce videos from Haxxis Packages and data.
* The [GraphicRaycasterVA class](Assets/Utility/GraphicRaycasterVA.cs) is based on the base GraphicRaycaster class as made by Unity Technologies, and is made available via the [MIT license](https://bitbucket.org/Unity-Technologies/ui/raw/f0c70f707cf09f959ad417049cb070f8e296ffe2/LICENSE).
* The Mac version of the File Picker (such as most `UNITY_STANDALONE_OSX` conditionally-compiled items in [FilePicker.cs](Assets/Utility/FilePicker.cs) and all of the contents of the [StandaloneFileBrowser](Assets/StandaloneFileBrowser) folder) is made by Gökhan Gökçe and is made available via the [MIT license](https://raw.githubusercontent.com/gkngkc/UnityStandaloneFileBrowser/master/LICENSE.txt).

