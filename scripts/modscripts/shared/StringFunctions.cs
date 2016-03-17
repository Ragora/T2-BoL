//------------------------------------------------------------------------------
// StringFunctions.cs
// String functions
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function textToHash(%text)
{
	new fileObject(TextToHash);
	TextToHash.openForWrite("Hash.txt");
	TextToHash.writeLine(%text);
	TextToHash.detach();
	%hash = getFileCRC("Hash.txt");
	deleteFile("Hash.txt");
	return %hash;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function strReverse(%string)
{
	%len = StrLen(%string);
	%rstring = "";
	for (%i = 0; %i < %len; %i++)
		%rstring = getSubStr(%string,%i,1) @ %rstring;
	return %rstring;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function subStrInsert(%string,%insert,%slot)
{
	%seg = getSubStr(%string,0,%slot);
	%seg = %seg @ %insert;
	%string = %seg @ getSubStr(%string,%slot,strLen(%string));
	return %string;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function subStrRemove(%string,%slot)//Minimum: 1
{
	%half2 = getSubStr(%string,%slot,strLen(%string));
	%half1 = getSubStr(%string,0,%slot-1);
	return %half1 @ %half2;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function strMove(%string,%factor)
{
	%len = GetWordCount(%string);
	for (%i = 0; %i < %len; %i++)
	{
		%sub = getWord(%string,%i);
		%move = subStrInsert(%move,%sub,%i);
	}
	return %move;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function subStrMove(%string,%factor)
{
	%len = strLen(%string);
	for (%i = 0; %i < %len; %i++)
	{
		%sub = getSubStr(%string,%i,1);
		%move = subStrInsert(%move,%sub,%factor);
	}
	return %move;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function subStrScramble(%string)
{
	%len = strLen(%string);
	for (%i = 0; %i < %len; %i++)
	{
		%sub = getSubStr(%string,%i,1);
		%scramble = subStrInsert(%scramble,%sub,getRandom(0,%len));
	}
	return %scramble;
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function strSplit(%string)
{
	%count = strLen(%string);
	%div = %count / 2;
	return getSubStr(%string,0,%div) @ " | " @ getSubStr(%string,%div,%count);
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function stripSpaces(%string)
{
	return strReplace(%string," ","");
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function stripNonNumericCharacters(%string)
{
	%string = strLwr(%string);
	return stripChars(%string,"abcdefghijklmnopqrstuvwxyz`~!@#$%^&*()-_=+\|}]{[/?.>,<;:");
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function getSubStrOccurances(%string,%search)
{
	%len = strLen(%string);
	%srLen = strLen(%search);
	%count = 0;
	for (%i = 0; %i < %len; %i++)
	{
		%strSearch = strStr(%string,%search);
		if (%strSearch != -1) //It exists somewhere in the string
		{
			%count++;
			%string = getSubStr(%string,%strSearch+%srLen,%len);
		}
		else
			return %count;
	}
	return %count;
}

function getSubStrPos(%string,%str,%num)
{
	%len = strLen(%string);
	%subC = 0;
	for (%i = 0; %i < %len; %i++)
	{
		%curPos = %i;
		%sub = getSubStr(%string,%i,1);

		if (%sub $= %str)
		{
			if (%subC == %num)
				return %i;
			%subC++;
		}
	}
	return false;
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function strWhite(%string, %whiteList, %char)
{
	%charLen = strLen(%char);
	for (%i = 0; %i < strLen(%whiteList); %i++)
		for (%h = 0; %h < %charLen; %h++)
		{
			%whiteSeg = getSubStr(%whiteList, %i, %charLen);

		}
	return false;
}

//------------------------------------------------------------------------------
function getFileNameFromString(%string)
{
	if (strStr(%string, "/") == -1)
		return %string;
	else
		return getSubStr(%string,getSubStrPos(%string, "/", getSubStrOccurances(%string, "/")-1)+1,strLen(%string));
}

//------------------------------------------------------------------------------
function subWordCapitalize(%string)
{
	%current = "";
	for (%i = 0; %i < getWordCount(%string); %i++)
	{
		%word = getWord(%string, %i);
		%word = strUpr(getSubStr(%word, 0, 1)) @ getSubStr(%word, 1, strLen(%word));
		%current = %current SPC %word;
	}
	return trim(%current);
}

//-------------------------------------------------------------------------------
function getFileExtensionFromString(%string)
{
	%file = getFileNameFromString(%string);
	%period = strStr(%file,".");
	if (%period == -1)
		return false;
	else
		return getSubStr(%file,strStr(%file,".")+1, strLen(%file));
}

//------------------------------------------------------------------------------
