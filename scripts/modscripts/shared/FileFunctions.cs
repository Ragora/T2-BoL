// -----------------------------------------------------
// FileFunctions.cs
// Basic file functions
// Copyright (c) 2012 Robert MacGregor
// -----------------------------------------------------
function getFileBuffer(%file)
{
	if (!IsFile(%file))
		return -1;

	new FileObject(FileBuffer);
	FileBuffer.openForRead(%file);

	while (!FileBuffer.isEOF())
		%buffer = %buffer @ FileBuffer.readLine() @ "\n";
	FileBuffer.detach();
	return %buffer; //Long string. >.>
}

function getLine(%file, %line)
{
	if (!IsFile(%file))
		return -1;

	new FileObject(FileLine);
	FileLine.openForRead(%file);

	for (%i = 0; %i < %line; %i++)
		%line = FileLine.readLine();
	FileLine.detach();
	return %line;
}

function getLine(%file, %line)
{
	if (!IsFile(%file))
		return -1;

	new FileObject(FileLine);
	FileLine.openForRead(%file);

	for (%i = 0; %i < %line; %i++)
		%line = FileLine.readLine();
	FileLine.detach();
	return %line;
}

// Returns an unsorted list of the contents of %dir (including folders)
function getDirectory(%dir)
{
	%array = Array.create();
	
	%fileCount = 0;
	for( %file = findFirstFile( %dir @ "*.*" ); %file !$= ""; %file = findNextFile( %dir @ "*.*" ) )
	{
				%file = strReplace(%file, %socket.request, "");
				if (strStr(%file, "/") != -1)
				{
					%dir = getSubStr(%file, 0, strStr(%file, "/")) @ "/";
					if (!%dirAdded[%dir])
					{
						%data = %data @ "<a href=\"" @ strReplace(%dir, " ","%20") @ "\">" @ %dir @ "</a><br>\n";
						%dirAdded[%dir] = true;
					}
				}
				else
					%data = %data @ "<a href=\"" @ strReplace(%file, " ", "%20") @ "\">" @ %file @ "</a><br>\n";
			}
	return %array;
}

// -----------------------------------------------------
// Bound Functions
// -----------------------------------------------------
function fileObject::Detach(%this) //Detaches fileObject from file & deletes
{
	%this.close();
	%this.delete();
	return %this;
}
