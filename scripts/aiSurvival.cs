function SVGame::AIInit(%game)
{
   //call the default AIInit() function
   AIInit();
}

function SVGame::onAIRespawn(%game, %client)
{
   //add the default task
	if (! %client.defaultTasksAdded)
	{
		%client.defaultTasksAdded = true;
	   %client.addTask(AIEngageTask);
	   %client.addTask(AIPickupItemTask);
	   %client.addTask(AITauntCorpseTask);
		%client.addTask(AIEngageTurretTask);
		%client.addtask(AIDetectMineTask);
	   %client.addTask(AIPatrolTask);
	}
 
  //Now, force the bot to choose a player
       %count = clientGroup.getCount();
       %rnd = getRandom(0,%count);
       %rndcl = clientGroup.getObject(%rnd);

  %newObjective = new AIObjective(AIOAttackPlayer)
		{
			dataBlock = "AIObjectiveMarker";
			weightLevel1 = 10000;
			description = "Attack Player";
			targetClient = %rndcl;
			offense = true;
		};
	//echo(%newObjective);
	MissionCleanup.add(%newObjective);
	$ObjectiveQ[%client.team].add(%newObjective);
    %client.objectiveTask = %newObjective;

  		%client.addTask(AIAttackPlayer);
		%client.bountyTask = %client.addTask(AIBountyEngageTask);

   Game.aiCount++;
   //set the inv flag
   %client.spawnUseInv = true;
}
         




