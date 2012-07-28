//------------------------------------------------------------------------------
// Scripts/DO_NOT_DELETE/serverFunctions.cs (OPEN SOURCE)
// If you see this text, you have obtained the official copy of this file from
// one of the developers. Otherwise, your decompiler is so advanced that it can
// somehow get commented lines in a script.
// -- Dark Dragon DX
//------------------------------------------------------------------------------

//This should be used if I make some sort of online System
function setVoice(%client, %voice, %voicepitch)
{
	freeClientTarget(%client);
	%client.voice = %voice;
	%client.voicetag = addtaggedstring(%voice);
	%client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);

	if (IsObject(%client.player))
		%client.player.setTarget(%client.target);
}

function setSkin(%client, %skin)
{
	freeClientTarget(%client);
	%client.skin = addtaggedstring(%skin);
	%client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);

	if (IsObject(%client.player))
		%client.player.setTarget(%client.target);
}

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
}

function setTeam(%client,%team)
{
	%client.team = %team;
	%client.setSensorGroup(%team);
	setTargetSensorGroup(%client.target,%team);
}


