//------------------------------------------------------------------------------
// rangedVoice.cs
// Functions for ranged voice in T2BoL
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

$BOL::Voice::Whisper = 0;
$BOL::Voice::Range[0] = 15;
$BOL::Voice::Display[0] = "Whispering";

$BOL::Voice::Speak = 1; 
$BOL::voice::Range[1] = 25;
$BOL::Voice::Display[1] = "Speaking";

$BOL::Voice::Yell = 2;
$BOL::Voice::Range[2] = 50;
$BOL::Voice::Display[2] = "Yelling";

$BOL::Voice::Scream = 3;
$BOL::Voice::Range[3] = 100;
$BOL::Voice::Display[3] = "Screaming";

$BOL::Voice::Total = 4;

function serverCmdIncreaseVoiceRange(%client, %noDisplay)
{
	if ($CurrentMissionType $= "RPG" && $Host::GlobalChat)
	{
		messageClient(%client, 'msgClient', "\c3Server has global chat enabled.");
		return;
	}
	else if ($CurrentMissionType !$= "RPG")
	{
		messageClient(%client, 'msgClient', "\c3Server is not running the RPG gamemode currently.");
		return;
	}
		
	if (%client.voiceMode >= $BOL::Voice::Total-1)
		%client.voiceMode = 0;
	else
		%client.voiceMode++;
	%display = $BOL::Voice::Display[%client.voiceMode];
	%range = $BOL::Voice::Range[%client.voiceMode];
	if (!%noDisplay)
		messageClient(%client, 'msgClient',"\c3Voice mode set to \"" @ %display @ "\" (" @ %range @ "m)");
}

function serverCmdDecreaseVoiceRange(%client, %noDisplay)
{
	if ($CurrentMissionType $= "RPG" && $Host::GlobalChat)
	{
		messageClient(%client, 'msgClient', "\c3Server has global chat enabled.");
		return;
	}
	else if ($CurrentMissionType !$= "RPG")
	{
		messageClient(%client, 'msgClient', "\c3Server is not running the RPG gamemode currently.");
		return;
	}
			
	if (%client.voiceMode <= 0)
		%client.voiceMode = $BOL::Voice::Total-1;
	else
		%client.voiceMode--;
	%display = $BOL::Voice::Display[%client.voiceMode];
	%range = $BOL::Voice::Range[%client.voiceMode];
	if (!%noDisplay)
		messageClient(%client, 'msgClient',"\c3Voice mode set to \"" @ %display @ "\" (" @ %range @ "m)");
}

function rangedchatMessageAll(%sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
	%mode = %sender.voiceMode;
	
	if (%mode < 0 || %mode >= $BOL::Voice::Total)
	{
		%sender.voiceMode = 0;
		%mode = 0;
		messageClient(%sender,'msgClient',"\c3Your throat feels agitated.");
	}
	%voicedist = $BOL::Voice::Range[%sender.voiceMode];
	%display = $BOL::Voice::Display[%sender.voiceMode];
	%count = MissionCleanup.getCount();
	
	for (%i = 0; %i < %count; %i++)
	{
		%obj = MissionCleanup.getObject(%i);
		if (%obj.getClassName() $= "Player")
		{
			%dist = vectorDist(%sender.player.getPosition(),%obj.getPosition());
			if (%dist <= %voicedist)
			{
				%string = addTaggedString("(" @ %display @ " - " @ %voicedist @ "m) " @ getTaggedString(%a1));
				chatMessageClient( %obj.client, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %string, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
			}
		}
	}
}