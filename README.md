#The Stardew Valley maps are compressed by default, use one of the existing tools (e.g. [XNB Parser's](https://dl.dropbox.com/u/17271122/fez/fez_parse_0.7.zip) xnb_decomp.bat) to decompress it before attempting to use this with the file.

## Usage

### Explorer
##### Unpacking and opening
* Just drag the xnb file onto the .exe and it will create a .tbin file of the same name.
* Open with [tIDE Map Editor](http://tide.codeplex.com/releases)

* You may get an error message saying something like: "An error occured while opening the file. Details: Unable to load tile sheet 'desert-new' with image source '-------------------------------------------\DesertTiles' Inner Message: Parameter is not valid."
* Here you see it saying \DesertTiles before "Inner Message" that means the tIDE Editor cannot find the DesertTiles graphics file.
* In your Stardew Valley\Content\Maps\ Folder find the right file (in this case desertTiles.xnb)
* Use one of the already available tools to turn it into a png file and rename it to remove the .png extension
* Place alongside the .tbin map file when opening
* Repeat this until tIDE stops complaining about not being able to load tilesheets

##### Putting it back
* Now when done editing, save and drag the .tbin file onto the .exe again.
* It will overwrite the .xnb file of the same name in this folder with the new data
* Put the xnb File back into the Stardew Valley\Content\Maps\ Folder (Make a copy of the original first!)
* Start Stardew Valley (You can keep doing this save-edit-putback cycle without having to unpack the xnb again first)

### Command Line

Usage: SimpleXNBDemapper <command> <input> <output>
commands: pack, unpack

See explorer usage on how to get the tbin file to load.

If providing only 1 argument it will figure out the command by file extension (.tbin makes it pack and .xnb unpack) and the output file will have the same name as the input file but with different extension.

If providing 2 arguments, it will use an output name that is the name of the input file with the new extension.