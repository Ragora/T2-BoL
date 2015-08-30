// --------------------------------------------------------
// aiSurvival.cs
// AI scripts for bots in the Survival gamemode
// Copyright (c) 2012 The DarkDragonDX
// ========================================================

function SVGame::AIInit(%game)
{
	//call the default AIInit() function
	AIInit();
	return true;
}

function SVGame::onAIRespawn(%game, %client)
{
	//add the default task
	if (! %client.defaultTasksAdded)
	{
		%client.defaultTasksAdded = true;
		%client.addTask(AIEngageTask);
		%client.addTask(AIPickupItemTask);
	//	%client.addTask(AITauntCorpseTask);
		%client.addTask(AIEngageTurretTask);
	//	%client.addtask(AIDetectMineTask);
		%client.addTask(AIPatrolTask);
	}
	%client.setSkillLevel(99);
	%client.hide();
	%client.hideClientInList();
 
	//Now, force the bot to choose a player
    %count = clientGroup.getCount();
	%rnd = getRandom(0,%count);
	%rndcl = clientGroup.getObject(%rnd);

	%client.stepEngage(%rndcl);

	Game.aiCount++;
	//set the inv flag
	%client.spawnUseInv = true;

	
	return true;
}