function BOLGame::AIInit(%game)
{
   //call the default AIInit() function
   AIInit();
}

function BOLGame::onAIRespawn(%game, %client)
{
   //add the default task
	if (! %client.defaultTasksAdded)
	{
		%client.defaultTasksAdded = true;
	   %client.addTask(AIEngageTask);
	   %client.addTask(AIPickupItemTask);
	 //  %client.addTask(AIUseInventoryTask); They spawn with the stuff they need
	   %client.addTask(AITauntCorpseTask);
		%client.addTask(AIEngageTurretTask);
		%client.addtask(AIDetectMineTask);
	 //  %client.addTask(AIPatrolTask); They don't move unless told to
	}

   //set the inv flag
   %client.spawnUseInv = true;
}


