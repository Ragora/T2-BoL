function aiConnectByIndex(%index, %team)
{
	if (%index < 0 || $BotProfile[%index, name] $= "")
		return;

	if (%team $= "")
		%team = -1;
  
	//initialize the profile, if required
	if ($BotProfile[%index, skill] $= "")
		$BotProfile[%index, skill] = 0.5;
    if ($BotProfile[%index, race] $= "")
        $BotProfile[%index, race] = "Human";
    if ($BotProfile[%index, Sex] $= "")
        $BotProfile[%index, Sex] = "Male";
    if ($BotProfile[%index, Voice] $= "")
        $BotProfile[%index, Voice] =  "Bot1";
        
    %skin[0] = "BaseBot";
    %skin[1] = "BaseBBot";
    
    if ($BotProfile[%index, Skin] $= '')
        $BotProfile[%index, Skin] = %skin[getRandom(0,1)];
	$BotProfile[%index, Skin] = addTaggedString($BotProfile[%index, Skin]);
	//$BotProfile[%index, Voice] = addTaggedString($BotProfile[%index, Voice]);
  
    %client =  aiConnect($BotProfile[%index, name], %team, $BotProfile[%index, skill], $BotProfile[%index, offense], $BotProfile[%index, voice], $BotProfile[%index, voicePitch]);
    %client.skin = $BotProfile[%index, skin];
    %client.race = $BotProfile[%index, race];
    %client.sex = $BotProfile[%index, sex];
    %client.voice = $BotProfile[%index, voice];
    
    //Make sure our voices and skins are set
    setSkin(%client,getTaggedString(%client.skin)); //Yay.. bots are not ugly anymore!
    setVoice(%client,%client.voice);
    %client.player.setArmor(%client.armor);

	return %client;
}

function aiConnectByName(%name, %team)
{
	if (%name $= "")
		return;

	if (%team $= "")
		%team = -1;

	%foundIndex = -1;
	%index = 0;
	while ($BotProfile[%index, name] !$= "")
	{
		if ($BotProfile[%index, name] $= %name)
		{
			%foundIndex = %index;
			break;
		}
		else
			%index++;
	}

	//see if we found our bot
	if (%foundIndex >= 0)
		return aiConnectByIndex(%foundIndex, %team);

	//else add a new bot profile
	else
	{
		$BotProfile[%index, name] = %name;
		return aiConnectByIndex(%index, %team);
	}
}

function aiBotAlreadyConnected(%name)
{
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%client = ClientGroup.getObject(%i);
		if (%name $= getTaggedString(%client.name))
			return true;
	}

	return false;
}

function aiConnectMultiple(%numToConnect, %minSkill, %maxSkill, %team)
{
	//validate the args
	if (%numToConnect <= 0)
		return;

	if (%maxSkill < 0)
		%maxSkill = 0;

	if (%minSkill >= %maxSkill)
		%minSkill = %maxSkill - 0.01;

	if (%team $= "")
		%team = -1;

	//loop through the profiles, and set the flags and initialize
	%numBotsAlreadyConnected = 0;
	%index = 0;
	while ($BotProfile[%index, name] !$= "")
	{
		//initialize the profile if required
		if ($BotProfile[%index, skill] $= "")
			$BotProfile[%index, skill] = 0.5;

		//if the bot is already playing, it shouldn't be reselected
		if (aiBotAlreadyConnected($BotProfile[%index, name]))
		{
			$BotProfile[%index, canSelect] = false;
			%numBotsAlreadyConnected++;
		}
		else
			$BotProfile[%index, canSelect] = true;

		%index++;
	}

	//make sure we're not trying to add more bots than we have...
	if (%numToConnect > (%index - %numBotsAlreadyConnected))
		%numToConnect = (%index - %numBotsAlreadyConnected);

	//build the array of possible candidates...
	%index = 0;
	%tableCount = 0;
	while ($BotProfile[%index, name] !$= "")
	{
		%botSkill = $BotProfile[%index, skill];

		//see if the skill is within range
		if ($BotProfile[%index, canSelect] && %botSkill >= %minSkill && %botSkill <= %maxSkill)
		{
			$BotSelectTable[%tableCount] = %index;
			%tableCount++;
			$BotProfile[%index, canSelect] = false;
		}

		//check the next bot
		%index++;
	}

	//if we didn't find enough bots, we'll have to search the rest of the profiles...
	%searchMinSkill = %minSkill;
	while ((%tableCount < %numToConnect) && (%searchMinSkill > 0))
	{
		%index = 0;
		while ($BotProfile[%index, name] !$= "")
		{
			%botSkill = $BotProfile[%index, skill];

			//see if the skill is within range
			if ($BotProfile[%index, canSelect] && %botSkill >= (%searchMinSkill - 0.1) && %botSkill <= %searchMinSkill)
			{
				$BotSelectTable[%tableCount] = %index;
				%tableCount++;
				$BotProfile[%index, canSelect] = false;
			}

			//check the next bot
			%index++;
		}

		//now lower the search min Skill, and take another pass at a lower skill level
		%searchMinSkill = %searchMinSkill - 0.1;
	}

	//if we're still short of bots, search the higher skill levels
	%searchMaxSkill = %maxSkill;
	while ((%tableCount < %numToConnect) && (%searchMaxSkill < 1.0))
	{
		%index = 0;
		while ($BotProfile[%index, name] !$= "")
		{
			%botSkill = $BotProfile[%index, skill];
			//see if the skill is within range
			if ($BotProfile[%index, canSelect] && %botSkill >= %searchMaxSkill && %botSkill <= (%searchMaxSkill + 0.1))
			{
				$BotSelectTable[%tableCount] = %index;
				%tableCount++;
				$BotProfile[%index, canSelect] = false;
			}

			//check the next bot
			%index++;
		}

		//now raise the search max Skill, and take another pass at a higher skill level
		%searchMaxSkill = %searchMaxSkill + 0.1;
	}

	//since the numToConnect was capped at the table size, we should have enough bots in the
	//table to fulfill the quota

	//loop through five times, picking random indices, and adding them until we've added enough
	%numBotsConnected = 0;
	for (%i = 0; %i < 5; %i++)
	{
		for (%j = 0; %j < %numToConnect; %j++)
		{
			%selectedIndex = mFloor(getRandom() * (%tableCount - 0.1));
			if ($BotSelectTable[%selectedIndex] >= 0)
			{
				//connect the bot
				%botClient = aiConnectByIndex($BotSelectTable[%selectedIndex], %team);
				%numBotsConnected++;

				//adjust the skill level, if required
				%botSkill = %botClient.getSkillLevel();
				if (%botSkill < %minSkill || %botSkill > %maxSkill)
				{
					%newSkill = %minSKill + (getRandom() * (%maxSkill - %minSkill));
					%botClient.setSkillLevel(%newSkill);
				}

				//clear the table entry to avoid connecting duplicates
				$BotSelectTable[%selectedIndex] = -1;

				//see if we've connected enough
				if (%numBotsConnected == %numToConnect)
					return;
			}
		}
	}

	//at this point, we've looped though the table, and kept hitting duplicates, search the table sequentially
	for (%i = 0; %i < %tableCount; %i++)
	{
		if ($BotSelectTable[%i] >= 0)
		{
			//connect the bot
			%botClient = aiConnectByIndex($BotSelectTable[%i], %team);
			%numBotsConnected++;

			//adjust the skill level, if required
			%botSkill = %botClient.getSkillLevel();
			if (%botSkill < %minSkill || %botSkill > %maxSkill)
			{
				%newSkill = %minSKill + (getRandom() * (%maxSkill - %minSkill));
				%botClient.setSkillLevel(%newSkill);
			}

			//clear the table entry to avoid connecting duplicates
			$BotSelectTable[%i] = -1;

			//see if we've connected enough
			if (%numBotsConnected == %numToConnect)
				return;
		}
	}
}

function aiReloadProfiles()
{
	if (!IsObject(BotProfiles))
		new ScriptObject(BotProfiles) { class = "BasicDataParser"; };
	BotProfiles.empty();
	BotProfiles.load("prefs/Bot Profiles.conf");
	%count = BotProfiles.count("Bot");
	for (%i = 0; %i < %count; %i++)
	{
		%Entry = BotProfiles.get("Bot",%i);
		$BotProfile[%i, name] = %Entry.element("Name");
		$BotProfile[%i, skill] = %Entry.element("skill");
		$BotProfile[%i, offense] = %Entry.element("offense");
		$BotProfile[%i, voicePitch] = %Entry.element("voicePitch");
		$BotProfile[%i, race] = %Entry.element("race");
		$BotProfile[%i, skin] = %Entry.element("skin");
		$BotProfile[%i, voice] = %Entry.element("voice");
		$BotProfile[%i, sex] = %Entry.element("sex");
	}
	$BotProfile::Count = %count;
	warn("scripts/aiBotProfiles.cs: Loaded" SPC %count SPC "bot profiles.");
	return true;
}
aiReloadProfiles();