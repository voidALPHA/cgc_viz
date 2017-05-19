Haxxis README

Full documentation on the use of Haxxis is available at https://voidalpha.github.io/cgc_viz/.

===================================================
A brief tour

View help:
1. Press ~ to open dev command console
2. Type 'help' and hit enter to view available command types.
3. Enter 'help' followed by any command type to view command options (such as 'help cla').

1. Double-click Haxxis.exe to start the Haxxis client
2. Click the "Load" button in the upper right
3. Select "score_iso_grid.json"
4. To see the visualization, click the "Eval" button in the upper right (or hit F5)
5. If you don't see the visualization try hitting F12 to point the camera at world origin
6. To see choreography, on the bottom section, hit the play icon button next to 'Choreography Sequencer' on the left

===================================================


Hot keys list:

           Tab - Toggles display of Chain Editor
             ~ - Toggles display of dev command console (type 'help' or 'help -full' to list dev commands)
   Shift-Alt-S - Toggles display of system stats lines

             C - Toggles free camera mode (but we default free camera mode ON)
   W,A,S,D,Q,E - Free camera movement (forward, left, back, right, down, up)
         Shift - Accelerated movement when used with the controls above
      RMB-Drag - Point free camera
      MMB-Drag - Move free camera

            F5 - Evaluate
            F8 - Save
           F12 - Point camera at 0,0,0
            F3 - Show debug json instance IDs on node views

Haxxis Editor:

      LMB-Drag - (In workspace) Moves the hovered chain node or group
      MMB-Drag - (In Workspace) Moves the root chain group
     RMB-Click - (In workspace) Add a node (brings up the node picker; node will be added at selected location)
  Shift-Ctrl-T - Also brings up the node picker
         Click - (On a node header) Expand/collapse node
      LMB-Drag - (on a node header) Move node
Shift+LMB-Drag - (on a node header) Move node and all of its children
   Shift-Click - (on a node header) Toggles selection of node
  Shift-Ctrl-D - (optionally just Ctrl-D in standalone) Duplicate selected Choreography Step
  Control+Drag - (on a node header or group) Move node or group into or out of a group
     Control+Z - Undo last operation
     Control+Y - Redo last undone operation


For testing selections:
             ] - Select all (in "score_iso_grid.json")
             [ - Selection none (in "score_iso_grid.json")
             R - Select a row (around the little dot)
             S - Select a column (around the little dot)
             T - Select an altitude (around the little dot)
           Note: Instead of R S T, it's cleaner to click on row, column or height labels in "TestShortChain.json"
		   
Debugging:

 Shift-Ctrl-F1 - Dump debug info about Canvases to Debug Log

===================================================


Haxxis groups:

Haxxis supports groups and group propagation.  This feature is designed to facilitate package construction and maintenance when using Haxxis groups as functional templates, parts of other packages, or as organizational tools.

A group is a collection of nodes.  To create a group, right-click in the background on the package editor and select 'Add Group' in the top right.  To move nodes into or out of a group hold Control and drag a node onto the background of the group that should contain them (including the background for 'no group').  Grouped nodes can be moved together by dragging the background panel of the group.  Groups can be nested within other groups by holding Control and dragging the background panel of the group into the background panel of another group.

A group has several controls along its top edge.  From left to right:
- Propagate - This button saves a group and scans all packages within the HaxxisPackages folder for any other packages using the saved group.  Any changes to the propagated group are copied into the package before reestablishing the group's incoming and outgoing connection.  BE WARNED this immediately destroys any local changes made to the groups in the other packages!  This is useful when one group is being used as a template or a subset of functionality within another group and changes are made that should be mirrored across other packages.
- Divorce - This button removes the link between the group and its backing file without modifying any of the nodes comprising the group itself.  The group is then anonymous and unrelated to its originating file.  BE WARNED that there is no way to undo this operation short of overwriting the originating file with a Save operation.  This is useful when you import a group as a template and don't want propagated changes to the template to affect the current package.
- Reload - This button replaces the current version of the group with what is saved in its backing file.  If a group is anonymous, nothing happens.  Any incoming or outgoing connections will be reconnected to their best node matches of the replacing group.  BE WARNED this immediately destroys any local changes made to the group.  This is useful when you want to manually pull changes made in a template file.
- Save - Saves the group to a file.  These files are saved as .json files and become available for import into other packages through the 'add' control.  If this group is anonymous, Haxxis will prompt you for a save location.
- (Save) As - Saves the group to a new file, prompting the user for a new file destination.
- Toggle Visibility - Hides or shows all of the nodes within the group.  Primarily used to improve UI performance when dealing with very large packages.  The effects of this button are not saved alongside a package: any loaded group will by default display its component nodes.
- Delete - Remove the group from the current package.  If the group is not empty the user will be asked for confirmation before deleting the group.
