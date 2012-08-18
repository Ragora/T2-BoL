//------------------------------------------------------------------------------
// torqueExServer.cs
// Torque Extensions for Servers
// Copyright (c) 2012 The DarkDragonDX
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Name: setVoice
// Argument %client: The client object to change the voice of.
// Argument %voice: The name of the voice to change to
// Argument %voicepitch: The voicepitch to use
// Description: Changes the voice of the targeted client object.
//==============================================================================
function setVoice(%client, %voice, %voicepitch)
{
	freeClientTarget(%client);
	%client.voice = %voice;
	%client.voicetag = addtaggedstring(%voice);
	%client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);

	if (IsObject(%client.player))
		%client.player.setTarget(%client.target);
	return true;
}

//------------------------------------------------------------------------------
// Name: setSkin
// Argument %client: The client object to change the voice of.
// Argument %skin: The skin to change to.
// Description: Changes the skin of the targeted client object.
//==============================================================================
function setSkin(%client, %skin)
{
	freeClientTarget(%client);
	%client.skin = addtaggedstring(%skin);
	%client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);

	// If the client has a player object
	if (IsObject(%client.player))
		%client.player.setTarget(%client.target);
	return true;
}

//------------------------------------------------------------------------------
// Name: setName
// Argument %client: The client object to change the skin of.
// Argument %name: The name to change to.
// Description: Changes the name of the targeted client object.
//==============================================================================
function setName(%client, %name)
{
	freeClientTarget(%client);
	%client.namebase = %name;
	%client.name = addtaggedstring(%name);
	%client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);

	if (IsObject(%client.player))
		%client.player.setTarget(%client.target);

	//Update the client in the lobby.
	HideClient(%client);
	ShowClient(%client);
	return true;
}

//------------------------------------------------------------------------------
// Name: setTeam
// Argument %client: The client object to change the team of.
// Argument %name: The team to change to.
// Description: Changes the team of the targeted client object.
//==============================================================================
function setTeam(%client,%team)
{
	%client.team = %team;
	%client.setSensorGroup(%team);
	setTargetSensorGroup(%client.target,%team);
	return true;
}

//------------------------------------------------------------------------------
// Name: hideClientInLobby
// Argument %client: The client to hide.
// Description: Hides this client object from the lobby only.
// (Doesn't have anything to do with the server list)
//==============================================================================
function hideClientInLobby(%client)
{
	messageAllExcept( %client, -1, 'MsgClientDrop', "", Game.kickClientName, %client );
	return true;
}

//------------------------------------------------------------------------------
// Name: showClientInLobby
// Argument %client: The client to show.
// Description: Shows this client object in the lobby only.
// (Doesn't have anything to do with the server list)
//==============================================================================
function showClientInLobby(%client)
{
	messageAllExcept(%client, -1, 'MsgClientJoin', "", %client.name, %client, %client.target, %client.isAIControlled(), %client.isAdmin, %client.isSuperAdmin, %client.isSmurf, %client.Guid);
	return true;
}

//------------------------------------------------------------------------------
// Name: hideClientInList
// Argument %client: The client to hide.
// Description: Hides the client in the server list only.
// WARNING!!! Running this on actual GameConnections is destructive. The game
// will refuse to update the client anymore until they are reshown. This is
// only known to work on AI's without a problem.
//==============================================================================
function hideClientInList(%client)
{
	if (!IsObject(HiddenClientGroup))
	{
		new SimGroup(HiddenClientGroup);
		ServerGroup.add(HiddenClientGroup);
	}
	HiddenClientGroup.add(%client);
	return true;
}

//------------------------------------------------------------------------------
// Name: showClientInList
// Argument %client: The client to show.
// Description: Shows the client in the server list only.
//==============================================================================
function showClientInList(%client)
{
	ClientGroup.add(%client);
	return true;
}

function ServerCMDCheckHTilt(%client){ return %client; }  // CCM-based clients spam fix, for some reason they spam this to the server whenever they strafe.

// TypeMasks
$TypeMasks::AllObjectType = -1; //Same thing as everything, thanks to Krash123 for telling me this. :)
$TypeMasks::InteractiveObjectType = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::WaterObjectType | $TypeMasks::ProjectileObjectType | $TypeMasks::ItemObjectType | $TypeMasks::CorpseObjectType;
$TypeMasks::UnInteractiveObjectType = $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::StaticRenderedObjectType;
$TypeMasks::BaseAssetObjectType = $TypeMasks::ForceFieldObjectType | $TypeMasks::TurretObjectType | $TypeMasks::SensorObjectType | $TypeMasks::StationObjectType | $TypeMasks::GeneratorObjectType;
$TypeMasks::GameSupportObjectType = $TypeMasks::TriggerObjectType | $TypeMasks::MarkerObjectType | $TypeMasks::CameraObjectType | $TypeMasks::VehicleBlockerObjectType | $TypeMasks::PhysicalZoneObjectType;
$TypeMasks::GameContentObjectType = $TypeMasks::ExplosionObjectType | $TypeMasks::CorpseObjectType | $TypeMasks::DebrisObjectType;
$TypeMasks::DefaultLOSObjectType = $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticObjectType;


// --- Binding Functions
function GameConnection::setVoice(%this, %voice, %voicepitch) { return setVoice(%this, %voice, %voicepitch); }
function GameConnection::setSkin(%this, %skin) { return setSkin(%this, %skin); }
function GameConnection::setName(%this, %name){ return setName(%this, %name); }
function GameConnection::setTeam(%this, %team){ return setTeam(%this, %team); }
function GameConnection::hideInLobby(%this){ return hideClientInLobby(%this); }
function GameConnection::showInLobby(%this){ return showClientInLobby(%this); }
// function GameConnection::hideClientInList(%this){ return hideClientInList(%this); }
// function GameConnection::showClientInList(%this){ return showClientInList(%this); }

function AIConnection::setVoice(%this, %voice, %voicepitch) { return setVoice(%this, %voice, %voicepitch); }
function AIConnection::setSkin(%this, %skin) { return setSkin(%this, %skin); }
function AIConnection::setName(%this, %name){ return setName(%this, %name); }
function AIConnection::setTeam(%this, %team){ return setTeam(%this, %team); }
function AIConnection::hide(%this){ return hideClientInLobby(%this); }
function AIConnection::show(%this){ return showClientInLobby(%this); }
function AIConnection::hideClientInList(%this){ return hideClientInList(%this); }
function AIConnection::showClientInList(%this){ return showClientInList(%this); }

function AIConnection::disengageTasks(%this)
{
	// Don't quite remember exactly what the minimal 
	// requirements here to get the same effect is,
	// but this works fine as it is it seems.
	AIUnassignClient(%this); // Have no idea what this does!
	%this.stop(); 
	%this.clearTasks(); // Clear the Behavior Tree
	%this.clearStep();
	%this.lastDamageClient = -1; 
	%this.lastDamageTurret = -1;
	%this.shouldEngage = -1;
	%this.setEngageTarget(-1);
	%this.setTargetObject(-1);
	%this.pilotVehicle = false;
	%this.defaultTasksAdded = false; 
	return true;
}

function Player::setVoice(%this, %voice, %voicepitch) 
{ 
	if (!isObject(%this.Client))
	{
		%this.Client = new ScriptObject(); // Glue!
		%this.Client.Player = %this;
	}
	return setVoice(%this.Client, %voice, %voicepitch); 
}

function Player::setSkin(%this, %skin) 
{ 
	if (!isObject(%this.Client))
	{
		%this.Client = new ScriptObject(); 
		%this.Client.Player = %this;
	}
	return setSkin(%this, %skin); 
}

function Player::setName(%this, %name)
{ 
	if (!isObject(%this.Client))
	{
		%this.Client = new ScriptObject(); 
		%this.Client.Player = %this;
	}
	return setName(%this, %name); 
}

function Player::setTeam(%this, %team)
{ 
	if (!isObject(%this.Client))
	{
		%this.Client = new ScriptObject(); 
		%this.Client.Player = %this;
	}
	return setTeam(%this, %team); 
}