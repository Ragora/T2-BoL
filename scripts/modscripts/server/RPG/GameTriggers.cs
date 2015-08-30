//------------------------------------------------------------------------------
// GameTriggers.cs
// Trigger code for RPG Gamemode
// Copyright (c) 2012 Robert MacGregor
//==============================================================================

$BOL::Triggers::Territory = 0;
$BOL::Triggers::Damage = 1;
$BOL::Triggers::TeleportStart = 2;
$PDA::Triggers::TeleportEnd = 3;

function RPGGame::onEnterTrigger(%game, %name, %data, %obj, %colObj)
{
	switch (%obj.Type)
	{
		case $BOL::Triggers::Territory:
			echo("LOL");
			return;
	}
}

function RPGGame::onLeaveTrigger(%game, %name, %data, %obj, %colObj)
{
	return true;
}

function RPGGame::onTickTrigger(%game, %name, %data, %obj)
{
	
	return true;
}
