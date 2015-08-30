//------------------------------------------------------------------------------
// PDA.cs
// PDA code for T2BoL
// Copyright (c) 2012 Robert MacGregor
//==============================================================================

// 0-100
$BOL::PDA::Page::Main = 0;

$BOL::PDA::Page::Applications = 1;
$BOL::PDA::Page::Close = 2; // Not even necessarily a page but it's used to signal the client wants to close
$BOL::PDA::Page::Stats = 3;
$BOL::PDA::Page::Save = 4;
$BOL::PDA::Page::FactionManagement = 5;

$BOL::PDA::Page::Radio = 6;
$BOL::PDA::Page::Voice = 7;

$BOL::PDA::Function::Increment = 1;
$BOL::PDA::Function::Decrement = 2;

$BOL::PDA::Page::EMail = 8;
$BOL::PDA::Page::Inbox = 9;
$BOL::PDA::Page::Outbox = 10;
$BOL::PDA::Page::Compose = 11;

$BOL::PDA::Page::Wiki = 12;

// 101-201
$BOL::PDA::Page::Interact = 101;
$BOL::PDA::Page::Interacted = 102;
$BOL::PDA::Page::Hack = 103;
$BOL::PDA::Page::Information = 104;

function RPGGame::updateScoreHud(%game, %client, %tag)
{
	if (%client.PDAPage == $BOL::PDA::Page::Main || %client.PDAPage == $BOL::PDA::Page::Interact)
		Game.processGameLink(%client, %client.PDAPage);	
}

function RPGGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5)
{
	%index = 0;
	if (%arg1 != $BOL::PDA::Page::Close)
		%client.PDAPage = %arg1;
	messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
	
	switch(%arg1)
	{		
		//------------------------------------------------------------------------------
		// PDA Applications
		//------------------------------------------------------------------------------
		case $BOL::PDA::Page::Applications:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Applications | Main');
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>| <a:gamelink\t" @ $BOL::PDA::Page::Wiki @ "\t1>Wiki</a> | <color:FF0000>Applications<color:66EEEE> | <a:gamelink\t" @ $BOL::PDA::Page::EMail @ "\t0>E-Mail</a> | <just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Command List:");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Stats @ "\t1>- Self Diagnosis</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Interact @ "\t0>- Interact with Object</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Radio @ "\t0>- Radio</a>");
			%index++;
			if (!$Host::GlobalChat)
			{
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Voice @ "\t0>- Voice Settings</a>");
				%index++;
			}
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::FactionManagement @ "\t1>- Faction Management</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Save @ "\t1>- Save State</a>");
			return;
			
		case $BOL::PDA::Page::Stats:
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Automated Self Diagnosis Systems v1.2<just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Copyright (c) 3030 S.G.S. Corporation');
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Subject Name: " @ %client.namebase);
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Subject Species: " @ %client.race);
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Subject Condition: " @ 100 - mfloor(100*%client.player.getDamageLevel()) @ "%");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Stats @ "\t1>REFRESH</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
			return;
			
		case $BOL::PDA::Page::Radio:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Applications | Radio');
			
			if (!%client.hasRadio)
			{
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>-- You do not have a radio to manage --");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
				return;
			}
	
			switch (%arg2)
			{
				case $BOL::PDA::Function::Increment:
					ServerCmdIncreaseRadioFrequency(%client, true);
				case $BOL::PDA::Function::Decrement:
					ServerCmdDecreaseRadioFrequency(%client, true);
			}
			
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Radio Status: Normal");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Current Frequency: " @ %client.radioFrequency @ "MHz");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Radio @ "\t" @ $BOL::PDA::Function::Increment @ ">[Increment Frequency</a> - <a:gamelink\t" @ $BOL::PDA::Page::Radio @ "\t" @ $BOL::PDA::Function::Decrement @ ">Decrement Frequency]</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
			return;
			
		case $BOL::PDA::Page::Voice:
				messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Applications | Voice Settings');
		
				switch (%arg2)
				{
					case $BOL::PDA::Function::Increment:
						serverCmdIncreaseVoiceRange(%client, true);
					case $BOL::PDA::Function::Decrement:
						serverCmdDecreaseVoiceRange(%client, true);
				}
				
				%voice = %client.voiceMode;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Voice Status: Normal");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Current Voice: \x22" @ $BOL::Voice::Display[%voice] @ "\x22 (" @ $BOL::Voice::Range[%voice] @ " meters)");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Voice @ "\t" @ $BOL::PDA::Function::Increment @ ">[Increment Range</a> - <a:gamelink\t" @ $BOL::PDA::Page::Voice @ "\t" @ $BOL::PDA::Function::Decrement @ ">Decrement Range]</a>");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
				%index++;
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
				return;
		
		case $BOL::PDA::Page::Interact:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Applications | Interact with Object');
				
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>-- Objects within Range --");
			%index++;
			
			%client_team = getTargetSensorGroup(%client.target);
			%object_count = %client.player.interactList.count();
			if (%object_count > 0)
				for (%i = 0; %i < %object_count; %i++)
				{
					%object = %client.player.interactList.element(%i);
					if (isObject(%object))
					{
						%object_target = %object.target;
						if (%object_target != -1)
						{
							%object_team = getTargetSensorGroup(%object_target);
							%object_friendly = %client_team == %object_team;
							%object_friend_text = %object_friendly ? "Friendly" : "Enemy";
							%display = %object_friend_text SPC %object.getClassName() SPC "\x22" @ getTaggedString(getTargetName(%object_target)) @ "\x22";
						}
						else
							%display = "Unknown" SPC %object.getClassName() SPC "(" @ %object @ ")";
						
						messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>" @ %display);
						%index++;
					}
				}
			else
			{
				messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>There are no objects in range.");
				%index++;
			}
			
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
			return;
			
			
		case $BOL::PDA::Page::FactionManagement:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Applications | Faction Management');
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications@ "\t1>RETURN TO MAIN</a>");
			return;
			
		case $BOL::PDA::Page::Save:
			messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Save State<just:right><a:gamelink\tCLOSE\t1>Close</a>');
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Save function is not supported as of now!");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Applications @ "\t1>RETURN TO MAIN</a>");
			return;
			
		//------------------------------------------------------------------------------
		// PDA E-Mail System
		//------------------------------------------------------------------------------
		case $BOL::PDA::Page::Inbox:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>E-Mail | Your Inbox');
			return;
		case $BOL::PDA::Page::Outbox:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>E-Mail | Your Outbox');
			return;
		case $BOL::PDA::Page::Compose:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>E-Mail | Compose a New Mail');
			return;

		
		//------------------------------------------------------------------------------
		// Interaction Commands
		//------------------------------------------------------------------------------
		case $BOL::PDA::Page::Interact:
			return;
			
		//------------------------------------------------------------------------------
		// Handle for Normal PDA functions
		//------------------------------------------------------------------------------
		case $BOL::PDA::Page::Main:
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>| <a:gamelink\t" @ $BOL::PDA::Page::Wiki @ "\t1>Wiki</a> | <a:gamelink\t" @ $BOL::PDA::Page::Applications @ "\t1>Applications</a> | <a:gamelink\t" @ $BOL::PDA::Page::EMail @ "\t1>E-Mail</a> | <just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Welcome to the PDA');
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Welcome to the PDA, this is where you will accomplish some daily tasks.");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Click any of the text in the subheader to begin exploring your PDA.");
			return;
			
		case $BOL::PDA::Page::EMail:
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>E-Mail | Main');
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>| <a:gamelink\t" @ $BOL::PDA::Page::Wiki @ "\t1>Wiki</a> | <a:gamelink\t" @ $BOL::PDA::Page::Applications @ "\t1>Applications</a> | <color:FF0000>E-Mail<color:66EEEE> | <just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>E-Mail Functions:");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center> - <a:gamelink\t" @ $BOL::PDA::Page::Inbox @ "\t1>Your Inbox (?)</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center> - <a:gamelink\t" @ $BOL::PDA::Page::Outbox @ "\t1>Your Outbox (?)</a>");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center> - <a:gamelink\t" @ $BOL::PDA::Page::Compose @ "\t1>Compose a New Mail</a>");
			return;
		
		case $BOL::PDA::Page::Close:
			serverCmdHideHud(%client, 'scoreScreen');
			commandToClient(%client, 'DisplayHuds');
			return;
		
		case $BOL::PDA::Page::Wiki:
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>| <color:FF0000>Wiki<color:66EEEE> | <a:gamelink\t" @ $BOL::PDA::Page::Applications @ "\t1>Applications</a> | <a:gamelink\t" @ $BOL::PDA::Page::EMail @ "\t1>E-Mail</a> | <just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Wiki | Main');
			return;
						
		default: // In case something stupid happens
			messageClient( %client, 'SetScoreHudHeader', "", "<just:center>| Information | <a:gamelink\t" @ $BOL::PDA::Page::Applications @ "\t1>Applications</a> | <a:gamelink\t" @ $BOL::PDA::Page::EMail @ "\t1>E-Mail</a> | <a:gamelink\t" @ $BOL::PDA::Page::Wiki @ "\t1>Wiki</a> | <just:right><a:gamelink\t" @ $BOL::PDA::Page::Close @ "\t1>Close</a>");
			messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>Error | Main');
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>-- An ERROR has occurred in the PDA Subsystem code --");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>-- Please report this error to DarkDragonDX --");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Unknown PDA page: " @ %arg1);
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, " ");
			%index++;
			messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\t" @ $BOL::PDA::Page::Main @ "\t1>-- RETURN TO MAIN --</a>");
			return;
	}
}