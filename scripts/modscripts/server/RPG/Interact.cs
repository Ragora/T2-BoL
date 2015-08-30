//--$BOL::PDA::Page::Interact----------------------------------------------------------------------------
// Interact.cs
// Functions for object interaction in T2BoL
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

$BOL::Interact::Type::General = 0; // Generic; jus makes conversation

$BOL::Interact::Range = 50; // In Meters
function serverCmdInteractWithObject(%client)
{
	if (!IsObject(%client.player) || %client.player.getState() !$= "Move")
	{
		messageClient(%client, 'MsgClient', "\c3Sorry, you appear to be dead.");
		return false;
	}
	
	%player = %client.player;
	%pos = getWords(%player.getEyeTransform(), 0, 2);
	%vec = %player.getMuzzleVector($WeaponSlot);
	%targetpos=vectoradd(%pos,vectorscale(%vec,5000));
	
	%object = getWord(containerRayCast(%pos,%targetpos,$TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType,%player),0);
	echo(%object);
	if (!isObject(%object) || !%object.isInteractive)
		return false;
	
	%client.pdaPage = $BOL::PDA::Page::Interacted;
	serverCmdShowHud(%client, 'scoreScreen');
	return true;
}

function Player::interactListUpdate(%this)
{
	if (!isObject(%this.interactList))
		%this.interactList = Array.create();
		
	// We don't want to run multiple threads ...
	cancel(%this.interactListUpdateThread);
	// We also don't need dead people or bots to be doing this either
	if ((%this.getState() !$= "Move" && %this.getState() !$= "Mounted") || %this.client.isAIControlled())
		return;
	
	%found = Array.create(); // This takes up one objID per call, but objID's go up to 2^32, so this shouldn't matter as it helps make cleaner code here
	%found_anything = false;
	InitContainerRadiusSearch(%this.getWorldBoxCenter(), $BOL::Interact::Range, $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType);
	while ((%target = containerSearchNext()) != 0)
	{
		//if (!calcExplosionCoverage(%this.getWorldBoxCenter(), %target,$TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType))
		//	continue;
		%found.setElement(%found.count(), %target);
		if(isObject(%target) && %target !$= %this && !%this.interactList.hasElementValue(%target))
			%this.interactList.setElement(%this.interactList.count(), %target);
	}
	
	// Remove any non-found elements from the interact list
	for (%i = %this.interactList.count(); %i > -1; %i--)
		if (!%found.hasElementValue(%this.interactList.element(%i)))
			%this.interactList.removeElement(%i);
	
	%found.delete();
	%this.interactListUpdateThread = %this.schedule(100, "interactListUpdate");
}