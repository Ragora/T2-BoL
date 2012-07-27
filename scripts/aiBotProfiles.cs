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
        
    %skin[0] = 'BaseBot';
    %skin[1] = 'BaseBBot';
    
    if ($BotProfile[%index, Skin] $= '')
        $BotProfile[%index, Skin] = %skin[getRandom(0,1)];
  
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

//Notes:
//Ai Bot profiles: Balance out difficulties and settings.
//Draakans and Derms make up most of the AI list
//Human
$BotProfile[0, name] = "Dalaila Hayes";
$BotProfile[0, skill] = 0.95;
$BotProfile[0, offense] = true;
$BotProfile[0, voicePitch] = 0.875;
$BotProfile[0, race] = "Human";
$BotProfile[0, skin] = 'Swolf';
$BotProfile[0, voice] = "Fem3";
$BotProfile[0, sex] = "Female";

$BotProfile[1, name] = "Cynthia Fisher";
$BotProfile[1, skill] = 0.95;
$BotProfile[1, offense] = true;
$BotProfile[1, voicePitch] = 0.875;
$BotProfile[1, race] = "Human";
$BotProfile[1, skin] = "Swolf";
$BotProfile[1, voice] = "Fem3";
$BotProfile[1, sex] = "Female";

$BotProfile[2, name] = "Commander Jackson";
$BotProfile[2, skill] = 0.95;
$BotProfile[2, offense] = true;
$BotProfile[2, voicePitch] = 0.875;
$BotProfile[2, race] = "Human";
$BotProfile[2, skin] = 'Beagle';
$BotProfile[2, voice] = "Male5";
$BotProfile[2, sex] = "Male";

$BotProfile[3, name] = "Corperal Jones";
$BotProfile[3, skill] = 0.95;
$BotProfile[3, offense] = true;
$BotProfile[3, voicePitch] = 0.875;
$BotProfile[3, race] = "Human";
$BotProfile[3, skin] = 'Beagle';
$BotProfile[3, voice] = "Male5";
$BotProfile[3, sex] = "Male";

//BioDerm
$BotProfile[4, name] = "Beast";
$BotProfile[4, skill] = 0.95;
$BotProfile[4, offense] = true;
$BotProfile[4, voicePitch] = 0.875;
$BotProfile[4, race] = "Bioderm";
$BotProfile[4, skin] = 'Horde';
$BotProfile[4, voice] = "Derm3";
$BotProfile[4, sex] = "Male";

$BotProfile[5, name] = "Retch";
$BotProfile[5, skill] = 0.78;
$BotProfile[5, offense] = false;
$BotProfile[5, voicePitch] = 0.789;
$BotProfile[5, race] = "Bioderm";
$BotProfile[5, skin] = 'Horde';
$BotProfile[5, voice] = "Derm2";

$BotProfile[6, name] = "Hagstomper";
$BotProfile[6, skill] = 0.78;
$BotProfile[6, offense] = false;
$BotProfile[6, voicePitch] = 0.789;
$BotProfile[6, race] = "Bioderm";
$BotProfile[6, skin] = 'Horde';
$BotProfile[6, voice] = "Derm2";

$BotProfile[6, name] = "Doormat";
$BotProfile[6, skill] = 0.78;
$BotProfile[6, offense] = false;
$BotProfile[6, voicePitch] = 0.789;
$BotProfile[6, race] = "Bioderm";
$BotProfile[6, skin] = 'Horde';
$BotProfile[6, voice] = "Derm2";

$BotProfile[7, name] = "Evenkill";
$BotProfile[7, skill] = 0.78;
$BotProfile[7, offense] = false;
$BotProfile[7, voicePitch] = 0.789;
$BotProfile[7, race] = "Bioderm";
$BotProfile[7, skin] = 'Horde';
$BotProfile[7, voice] = "Derm2";

$BotProfile[8, name] = "Heart Eater";
$BotProfile[8, skill] = 0.78;
$BotProfile[8, offense] = false;
$BotProfile[8, voicePitch] = 0.789;
$BotProfile[8, race] = "Bioderm";
$BotProfile[8, skin] = 'Horde';
$BotProfile[8, voice] = "Derm2";

$BotProfile[9, name] = "Torox the Backbreaker";
$BotProfile[9, skill] = 0.78;
$BotProfile[9, offense] = false;
$BotProfile[9, voicePitch] = 0.789;
$BotProfile[9, race] = "Bioderm";
$BotProfile[9, skin] = 'Horde';
$BotProfile[9, voice] = "Derm2";

$BotProfile[10, name] = "Face Breaker";
$BotProfile[10, skill] = 0.78;
$BotProfile[10, offense] = false;
$BotProfile[10, voicePitch] = 0.789;
$BotProfile[10, race] = "Bioderm";
$BotProfile[10, skin] = 'Horde';
$BotProfile[10, voice] = "Derm2";

$BotProfile[11, name] = "Monkey Bones";
$BotProfile[11, skill] = 0.78;
$BotProfile[11, offense] = false;
$BotProfile[11, voicePitch] = 0.789;
$BotProfile[11, race] = "Bioderm";
$BotProfile[11, skin] = 'Horde';
$BotProfile[11, voice] = "Derm2";

$BotProfile[12, name] = "Breath of Fear";
$BotProfile[12, skill] = 0.78;
$BotProfile[12, offense] = false;
$BotProfile[12, voicePitch] = 0.789;
$BotProfile[12, race] = "Bioderm";
$BotProfile[12, skin] = 'Horde';
$BotProfile[12, voice] = "Derm2";

$BotProfile[13, name] = "Devours-All";
$BotProfile[13, skill] = 0.78;
$BotProfile[13, offense] = false;
$BotProfile[13, voicePitch] = 0.789;
$BotProfile[13, race] = "Bioderm";
$BotProfile[13, skin] = 'Horde';
$BotProfile[13, voice] = "Derm2";

$BotProfile[14, name] = "Skullcrusher";
$BotProfile[14, skill] = 0.78;
$BotProfile[14, offense] = false;
$BotProfile[15, voicePitch] = 0.789;
$BotProfile[16, race] = "Bioderm";
$BotProfile[17, skin] = 'Horde';
$BotProfile[18, voice] = "Derm2";

//Draakan (Eh.. their real names use the derms' from SPDialog.cs) -- Most have their own backStory, but some are just here to take up room
//My personal picks: Dolosus (duh?), Sharp Tooth, and Iguana
$BotProfile[19, name] = "Dolosus"; //Dolosus Irokhirr Zhor
$BotProfile[19, skill] = 0.95;
$BotProfile[19, offense] = true;
$BotProfile[19, voicePitch] = 0.875;
$BotProfile[19, race] = "Draakan";
$BotProfile[19, skin] = 'Gecko';
$BotProfile[19, voice] = "Derm3";
$BotProfile[19, sex] = "A";

$BotProfile[20, name] = "Rex"; //#367378 (Genetic failure)
$BotProfile[20, skill] = 0.89;
$BotProfile[20, offense] = true;
$BotProfile[20, voicePitch] = 0.781;
$BotProfile[20, race] = "Draakan";
$BotProfile[20, skin] = 'Gecko';
$BotProfile[20, voice] = "Derm2";
$BotProfile[20, sex] = "A";

$BotProfile[21, name] = "Raptor"; //Hrreshig "Raptor" Kolkhris
$BotProfile[21, skill] = 0.88;
$BotProfile[21, offense] = true;
$BotProfile[21, voicePitch] = 0.972;
$BotProfile[21, race] = "Draakan";
$BotProfile[21, skin] = 'Gecko';
$BotProfile[21, voice] = "Derm2";
$BotProfile[21, sex] = "A";

$BotProfile[22, name] = "Iguana"; //Marakh "Iguana" Azarok
$BotProfile[22, skill] = 0.45;
$BotProfile[22, offense] = false;
$BotProfile[22, voicePitch] = 0.673;
$BotProfile[22, race] = "Draakan";
$BotProfile[22, skin] = 'Gecko';
$BotProfile[22, voice] = "Derm1";
$BotProfile[22, sex] = "A";

$BotProfile[23, name] = "Sharp Tooth"; //Murkhofud "Sharp Tooth" Khel
$BotProfile[23, skill] = 0.89;
$BotProfile[23, offense] = true;
$BotProfile[23, voicePitch] = 0.875;
$BotProfile[23, race] = "Draakan";
$BotProfile[23, skin] = 'Gecko';
$BotProfile[23, voice] = "Derm1";
$BotProfile[23, sex] = "A";

$BotProfile[24, name] = "Snake"; //Morax "Snake" Serexhar
$BotProfile[24, skill] = 0.78;
$BotProfile[24, offense] = false;
$BotProfile[24, voicePitch] = 1;
$BotProfile[24, race] = "Draakan";
$BotProfile[24, skin] = 'Gecko';
$BotProfile[24, voice] = "Derm2";
$BotProfile[24, sex] = "A";

$BotProfile[25, name] = "Gila"; //Gorog "Gila" Jok
$BotProfile[25, skill] = 0.80;
$BotProfile[25, offense] = true;
$BotProfile[25, voicePitch] = 0.578;
$BotProfile[25, race] = "Draakan";
$BotProfile[25, skin] = 'Gecko';
$BotProfile[25, voice] = "Derm1";
$BotProfile[25, sex] = "B";

$BotProfile[26, name] = "Cobra"; //Karghaz "Cobra" Tumoz
$BotProfile[26, skill] = 0.80;
$BotProfile[26, offense] = true;
$BotProfile[26, voicePitch] = 0.578;
$BotProfile[26, race] = "Draakan";
$BotProfile[26, skin] = 'Gecko';
$BotProfile[26, voice] = "Derm1";
$BotProfile[26, sex] = "C";

$BotProfile[27, name] = "Viper"; //Malevolox "Viper" Zon
$BotProfile[27, skill] = 0.80;
$BotProfile[27, offense] = true;
$BotProfile[27, voicePitch] = 0.578;
$BotProfile[27, race] = "Draakan";
$BotProfile[27, skin] = 'Gecko';
$BotProfile[27, voice] = "Derm1";
$BotProfile[27, sex] = "A";

$BotProfile[27, name] = "Komodo Dragon";
$BotProfile[27, skill] = 0.80;
$BotProfile[27, offense] = true;
$BotProfile[27, voicePitch] = 0.578;
$BotProfile[27, race] = "Draakan";
$BotProfile[27, skin] = 'Gecko';
$BotProfile[27, voice] = "Derm1";
$BotProfile[27, sex] = "C";

$BotProfile[27, name] = "Red Dragon";
$BotProfile[27, skill] = 0.78;
$BotProfile[27, offense] = false;
$BotProfile[27, voicePitch] = 0.789;
$BotProfile[27, race] = "Draakan";
$BotProfile[27, skin] = 'Gecko';
$BotProfile[27, voice] = "Derm2";
$BotProfile[27, sex] = "A";

$BotProfile[29, name] = "Torus";
$BotProfile[29, skill] = 0.78;
$BotProfile[29, offense] = false;
$BotProfile[29, voicePitch] = 0.789;
$BotProfile[29, race] = "Draakan";
$BotProfile[29, skin] = 'Gecko';
$BotProfile[29, voice] = "Derm2";
$BotProfile[29, sex] = "A";

$BotProfile[30, name] = "Slicer";
$BotProfile[30, skill] = 0.78;
$BotProfile[30, offense] = false;
$BotProfile[30, voicePitch] = 0.789;
$BotProfile[30, race] = "Draakan";
$BotProfile[30, skin] = 'Gecko';
$BotProfile[30, voice] = "Derm3";
$BotProfile[30, sex] = "C";

$BotProfile[31, name] = "Achilles";
$BotProfile[31, skill] = 0.78;
$BotProfile[31, offense] = false;
$BotProfile[31, voicePitch] = 0.789;
$BotProfile[31, race] = "Criollos";
$BotProfile[31, skin] = 'HALO_SKIN';
$BotProfile[31, voice] = "Derm2";

$BotProfile[32, name] = "King Snake";
$BotProfile[32, skill] = 0.78;
$BotProfile[32, offense] = false;
$BotProfile[32, voicePitch] = 0.789;
$BotProfile[32, race] = "Draakan";
$BotProfile[32, skin] = 'Gecko';
$BotProfile[32, voice] = "Derm2";
$BotProfile[32, sex] = "A";

$BotProfile[33, name] = "Troglodyte";
$BotProfile[33, skill] = 0.78;
$BotProfile[33, offense] = false;
$BotProfile[33, voicePitch] = 0.789;
$BotProfile[33, race] = "Draakan";
$BotProfile[33, skin] = 'Gecko';
$BotProfile[33, voice] = "Derm2";
$BotProfile[33, sex] = "A";

//Criollos (Wow.. only two of them)
$BotProfile[34, name] = "Alchaldes";
$BotProfile[34, skill] = 0.78;
$BotProfile[34, offense] = false;
$BotProfile[34, voicePitch] = 0.789;
$BotProfile[34, race] = "Criollos";
$BotProfile[34, skin] = 'HALO_SKIN';
$BotProfile[34, voice] = "Derm2";

$BotProfile[35, name] = "Hammurabi";
$BotProfile[35, skill] = 0.78;
$BotProfile[35, offense] = false;
$BotProfile[35, voicePitch] = 0.789;
$BotProfile[35, race] = "Criollos";
$BotProfile[35, skin] = 'HALO_SKIN';
$BotProfile[35, voice] = "Derm2";

$BotProfile::Count = 35;
