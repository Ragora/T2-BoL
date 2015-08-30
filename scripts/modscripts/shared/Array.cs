//------------------------------------------------------------------------------
// Array.cs
// Array object you can pass around.
// Copyright (c) 2012 Robert MacGregor
//==============================================================================

function ArrayFactory::create(%this, %name)
{
	if (isObject(%name))
		%name = "";
	%object = new ScriptObject(%name) { class = "ArrayObject"; };
	%object.elementCount = 0;
	return %object;
}

function ArrayObject::setElement(%this, %index, %object)
{
	%replaced = false;
	if (%this.Element[%index] != "")
		%replaced = true;
	else
	{
		%this.elementIndex[%index] = %this.elementCount;
		%this.elementIndices[%this.elementCount] = %index;
		%this.elementCount++;
	}
		
	%this.Element[%index] = %object;
	return %replaced;
}

function ArrayObject::list(%this)
{
	%list = Array.create();
	for (%i = 0; %i < %this.elementCount; %i++)
		%list.setElement(%i, %this.Element[%this.elementIndices[%i]]);
	return %list;
}

function ArrayObject::removeElement(%this, %index)
{
	if (%this.Element[%index] != "")
	{
		%this.Element[%index] = "";
		for (%i = %this.elementIndex[%index]; %i < %this.elementCount; %i++)
			%this.elementIndices[%i] = %this.elementIndices[%i+1];
		%this.elementCount--;
		return true;
	}
	else
		return false;
	return false;
}

function ArrayObject::hasElementValue(%this, %value)
{
	for (%i = 0; %i < %this.elementCount; %i++)
		if (%this.Element[%i] == %value)
			return true;
	return false;
}

function ArrayObject::isElement(%this, %index)
{
	if (%this.Element[%index] == "")
		return false;
	else
		return true;
	return false;
}

function ArrayObject::element(%this, %index)
{
	return %this.Element[%index];
}

function ArrayObject::count(%this)
{
	return %this.elementCount;
}

if (!IsObject(Array))
	new ScriptObject(Array) { class = "ArrayFactory"; };