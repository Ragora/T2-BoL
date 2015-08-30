//------------------------------------------------------------------------------
// BoLFunctions.cs
// T2BoL Specific Functions
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

function GameConnection::saveState(%this)
{
	if ($CurrentMissionType !$= "RPG")
	{
		error("Not running the BoL gamemode.");
		return false;
	}
		
	return true;
}

function GameConnection::loadState(%this, %file)
{
	if ($CurrentMissionType !$= "RPG")
	{
		error("Not running the BoL gamemode.");
		return false;
	}
		
	return true;
}

function AIConnection::saveState(%this)
{
	if ($CurrentMissionType !$= "RPG")
	{
		error("Not running the BoL gamemode.");
		return false;
	}
		
	return true;
}

function AIConnection::loadState(%this, %file)
{
	if ($CurrentMissionType !$= "RPG")
	{
		error("Not running the BoL gamemode.");
		return false;
	}
		
	return true;
}