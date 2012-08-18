//------------------------------------------------------------------------------
// basicDataStorage.cs
// Originally written for T2BoL mod back in the day, now is being rewritten
// for the original implementation was pretty crappy.
// Requires: Array.cs
// Copyright (c) 2012 The DarkDragonDX
//==============================================================================

//------------------------------------------------------------------------------
// Name: BasicDataParser.load
// Argument %file: The file to parse and load into memory.
// Description: This function is the main function of everything; it loads
// %file into memory.
// Return: True if the function succeeded, false if otherwise failed.
//==============================================================================
function BasicDataParser::load(%this, %file)
{
	// Make sure we have our values initialised (math doesn't work right on nonexistent variables!)
	if (%this.filesLoaded == "")
		%this.filesLoaded = 0;
	if (%this.blockEntryCount == "")
		%this.blockEntryCount = 0;
	if (%this.blockInstances == "") 
		%this.blockInstances = 0;
	
	%currentSeconds = formatTimeString("ss");
	// Check to see if the data is valid (returns false if we tried to load a nonexistent file)
	if (!isFile(%file))
	{
		error("basicDataStorage.cs: Attempted to load non-existent file " @ %file @ "!");
		return false;
	}
	// Check to see if this file is already loaded
	if (%this.isLoaded(%file))
	{
		error("basicDataStorage.cs: Attempted to reload data file " @ %file SPC "while it's already in memory! (try unloading or emptying)");
		return false;
	}
	// Add the file entry to memory (for the file check above)
	%this.files[%this.filesLoaded] = %file;
	%this.fileIndex[%file] = %this.filesLoaded;
	%this.filesLoaded++;
	
	// Load the file into memory (function is from fileProcessing.cs)
	%fileData = strReplace(stripChars(getFileBuffer(%file),"\t"),"\n","\t");
	%lineCount = getFieldCount(%fileData);
	
	%isProcessingBlock = false; // Used to set processing mode
	%currentBlock = 0;
	%hadError = false;
	// Iterate through all lines
	for (%i = 0; %i < %lineCount; %i++)
	{
		%currentLine = getField(%fileData,%i);
		// Check to see if this line contains a block definition or not
		%openingBlock = strStr(%currentLine, "[");
		%closingBlock = strStr(%currentLine, "]");
		
		// If we have a block definition, it should be against left margin 
		if (%openingBlock == 0 && %closingBlock > 0 && !%isProcessingBlock)
		{
			%isProcessingBlock = true;
			%blockName = getSubStr(%currentLine,%openingBlock+1,%closingBlock-1);
			
			if (%this.blockInstances[%blockName] == "")
			{
				%this.blockInstances[%blockName] = 0;
				%this.blockEntry[%this.blockEntryCount] = %blockName;
				%this.blockEntryCount++;
			}
			// Create the array to store our block data
			%currentBlock = Array.create();
			%currentBlock.Name = %blockName;
			%currentBlock.File = %file; 
			
			%this.blocks[%blockName,%this.blockInstances] = %currentBlock;
			%this.blockInstances[%blockName]++;
			%this.blockInstances++;
			continue;
		}
		// Results in an error
		else if (%openingBlock == 0 && %closingBlock > 0 && %isProcessingBlock)
		{
			error("basicDataStorage.cs: Error loading file "@ %file @ ", block spacing error.");
			return false;
		}
		
		// If we're processing the actual block
		if (%isProcessingBlock)
		{
			if (%currentLine $="" || %i == %lineCount)
			{
				%isProcessingBlock = false;
				continue;
			}
			else
			{
				%eqPos = strStr(%currentLine,"="); // This is safe since the equals sign should be first.
				if (%eqPos == -1)
				{
					error("basicDataStorage.cs: Unable to read entry for block" SPC %currentBlock.Name @ " in file" SPC %file @ "!");
					%isProcessingBlock = false;
					%hadError = true;
					continue;
				}
				// Note: I got lazy here, just don't have semicolons in your data entries..
				%semiPos = strStr(%currentLine,";");
				if (%semiPos == -1 || getSubStrOccurance(%currentLine,";") > 1)
				{
					error("basicDataStorage.cs: Unable to read entry for block" SPC %currentBlock.Name @ " in file" SPC %file @ "!");
					%isProcessingBlock = false;
					%hadError = true;
					continue;
				}

				%entryName = trim(getSubStr(%currentLine,0,%eqPos-1));
				%entryValue = trim(getSubStr(%currentLine,%eqPos+1,  mAbs(%eqPos-%semiPos)-1   ));
				%currentBlock.setElement(%entryName,%entryValue);
			}
		}
		
	}
	
	if (!%hadError)
		warn("basicDataStorage.cs: Successfully loaded file" SPC %file SPC "in " @ formatTimeString("ss") - %currentSeconds SPC "seconds.");
	return !%hadError;
}

//------------------------------------------------------------------------------
// Name: BasicDataParser.unload
// Argument %file: The file of who's entries should be unloaded.
// Description: This function is used to unload data by filename -- useful for
// reloading data from specific files.
// Return: True if the function was successful, false if otherwise failed.
//==============================================================================
function BasicDataParser::unload(%this, %file)
{
	if (!%this.isLoaded(%file))
	{
		error("basicDataStorage.cs: Attempted to unload non-loaded data file " @ %file @ "!");
		return false;
	}
	
	// Unload any data associated with this file now
	%removed = "";
	for (%i = 0; %i < %this.blockEntryCount; %i++)
	{
		%name = %this.blockEntry[%i];
		for (%h = 0; %h < %this.blockInstances[%name]; %h++)
			if (%this.blocks[%name, %h].File $= %file)
			{
				%this.blocks[%name, %h].delete();
				%this.blocks[%name, %h] = "";
				%this.blockEntry[%i] = "";
				%removed = trim(%removed SPC %i);
					
				if (%this.blockInstances[%name] == 1)
					%this.blockInstances[%name] = "";
				else
					%this.blockInstances[%name]--;
			}
	}
	
	// Iterate through our block entries and correct the imbalance
	for (%i = 0; %i < getWordCount(%removed); %i++)
	{
		for (%h = i; %h < %this.blockEntryCount; %h++)
			%this.blockEntry[%h-%i] = %this.blockEntry[%h+1];
		%this.blockEntryCount--;
	}
	
	// Now remove the file entry
	for (%i = %this.fileIndex[%file]; %i < %this.filesLoaded; %i++)
		if (%i != %this.filesLoaded-1)
		{
			%this.files[%i] = %this.files[%i+1];
			%this.fileIndex[%this.files[%i+1]] = %i;
		}
		else
		{
			%this.fileIndex[%file] = "";
			%this.files[%i] = "";
		}
	
	// Decrement the files loaded count and return true
	%this.filesLoaded--;
	return true;
}

//------------------------------------------------------------------------------
// Name: BasicDataParser.count
// Argument %block: The bloick entry to count the occurances of
// Return: The occurances of %block in this parser object. If there is no
// such entry of %block anywhere, false (0) is returned.
//==============================================================================
function BasicDataParser::count(%this, %block)
{
	// Return zero if the block has no entries even registered
	if (%this.blockInstances[%block] == "")
		return false;
	else
		// Return the block Instances otherwise
		return %this.blockInstances[%block];
	return false; // Shouldn't happen
}

//------------------------------------------------------------------------------
// Name: BasicDataParser.empty
// Description: Empties the entire object of any information it may have
// loaded anytime.
// Return: True is always returned from this function.
//==============================================================================
function BasicDataParser::empty(%this)
{	
	// Iterate through our block entries and destroy them
	for (%i = 0; %i < %this.blockEntryCount; %i++)
	{
		%name = %this.blockEntry[%i];
		for (%h = 0; %h < %this.blockInstances[%name]; %h++)
		{
			%this.blocks[%name, %h].delete();
			%this.blocks[%name, %h] = "";
		}
		%this.blockInstances[%name] = "";
		%this.blockEntry[%i] = "";
	}
	
	// Remove the files loaded entries now
	for (%i = 0; %i < %this.filesLoaded; %i++)
	{
		%this.fileIndex[%this.files[%i]] = "";
		%this.files[%i] = "";
	}
	
	// Reset some variables to 0 and return true
	%this.filesLoaded = 0;
	%this.blockInstances = 0;
	%this.blockEntryCount = 0;
	
	return true;
}

//------------------------------------------------------------------------------
// Name: BasicDataParser.isLoaded
// Argument %file: The file to check the loaded status of.
// Description: Returns if %file is loaded into memory of this object or not.
// Return: A boolean representing the loaded status.
//==============================================================================
function BasicDataParser::isLoaded(%this, %file)
{
	// Check to see if this file is already loaded
	for (%i = 0; %i < %this.filesLoaded; %i++)
		if (%this.files[%i] $= %file)
			return true;
	return false;
}

//------------------------------------------------------------------------------
// Name: BasicDataParser.get
// Argument %block: The name of the block to return.
// Argument %occurance: The block index we need to return -- if there's
// multiple entries of %block.
// Description: This function is used to retrieve block entries loaded from
// within any of the files this object has parsed.
// Return: An Array (array.cs) containing relevent information to the requested
// block. If there is no such entry of %block, false is returned.
//==============================================================================
function BasicDataParser::get(%this, %block, %occurance)
{
	// Check ti see uf thus block has only once entry -- in which case %occurance is ignored
	if (%this.count(%block) == 1) return %this.blocks[%block, 0];
	// Otherwise we use %occurance to return the specific index
	else if (%occurance >= 0 && %occurance <= %this.count(%block)) return %this.blocks[%block, %occurance];
	
	return false;
}