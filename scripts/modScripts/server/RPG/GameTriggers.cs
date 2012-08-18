//------------------------------------------------------------------------------
// Application SDK for the PDA. (to make my life easier)
// Trigger code for RPG Gamemode
// Copyright (c) 2012 The DarkDragonDX
//==============================================================================

function RPGGame::onEnterTrigger(%game, %name, %data, %obj, %colObj)
{
	switch$(%obj.type)
	{
		case "Transport": if (%obj.targetTransform $= "") return;
			%Colobj.setTransform(%obj.targetTransform);
			if (%Colobj.Usewhiteout) 
				%Obj.setWhiteout(0.8);
			break;
		case "Territory": 
			if (%obj.race $= "") 
				return;
			%obj.client.isOnTerritory[%Colobj.race] = true; 
			setClientTeam(%Colobj.client,getRaceTeam(%Obj.race));
			if (%obj.location $= "")
			{
				messageClient(%Colobj.client,'MsgSPCurrentObjective1',"",'Location: %1.', %obj.race SPC "Territory");
				messageClient(%colObj.client,'msgEnteredRaceTerritory','\c3You have entered %1 territory.',%obj.race); 
			}
			else
			{
				messageClient(%colObj.client,'msgEnteredRaceTerritory','\c3You have entered %1.',%obj.location);
				messageClient(%Colobj.client,'MsgSPCurrentObjective1',"",'Location: %1.', %obj.location);
			}
			break;
		case "Damage": //Will add lots o' vars onto it..
			%obj.damage[%colobj] = true;
			%colObj.isinLava = true;
			break;
	}
	if (%Colobj.message $= "" && %Colobj.type !$= "Territory")
		messageClient(%Colobj.client,'MsgTrigger',%obj.message);
	return true;
}

function RPGGame::onLeaveTrigger(%game, %name, %data, %obj, %colObj)
{
	switch$(%obj.type)
	{
		case "Territory": 
			if (%obj.race $= "") 
				return;
	  
			if (%obj.location $= "")
				messageClient(%Colobj.client,'MsgExitedRaceTerritory',"\c3You have exited "@%obj.race@" territory. Your sensor data is now undetectable.");
			else
				messageClient(%Colobj.client,'MsgExitedRaceTerritory',"\c3You have exited "@%obj.location@".");
			%Colobj.client.isOnTerritory[%obj.race] = false;
			messageClient(%colObj.client,'MsgSPCurrentObjective1',"",'Location: Unknown.');
			setClientTeam(%Colobj.client,0); //Not on the sensor, I think
			break;
		case "Damage":
			%obj.damage[%obj] = false;
			%colObj.isInLava = true;
			break;
	}
	return true;
}

function RPGGame::onTickTrigger(%game, %name, %data, %obj)
{
	switch(%obj.type)
	{
		case "Damage":
			for (%i = 0; %i < MissionCleanup.getCount(); %i++)
			{
				%objT = MissionCleanup.getObject(%i);
				if (%objt.getClassName() $= "Player" && %objt.getState() $= "move")
					if (%obj.damage[%objT] && %objT.isInLava)
						%objT.damage(0, %objT.getPosition(), %obj.damage, %obj,damageType);
				break;
			}
	}
	return true;
}
