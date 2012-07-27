// --------------------------------------------------------
// Survival Mission Type
// --------------------------------------------------------

// DisplayName = Survival

//--- GAME RULES BEGIN ---
//Survive as many waves of enemies as possible.
//The bot count increments by one each round.
//If progressive mode is enabled, bots are spawned every thirty seconds.
//--- GAME RULES END ---

$InvBanList[SV, "MiningTool"] = 1;

exec("scripts/aiSurvival.cs");

package SVGame
{
 function UpdateClientTimes(%time) //Used for initial countdown if needed
 {
    %secondsLeft = %time / 1000;
    messageAll('MsgSystemClock', "", (%secondsLeft / 60), %time);
 }
 
 function notifyMatchEnd(%time){} //Survival has NO end
 
 function Disconnect() //Package this function so we can disable the schedules on disconnect
 {
   parent::Disconnect();
   cancel(Game.helper);
   cancel(Game.roundStart);
   cancel(Game.roundMessage[0]);
   cancel(Game.roundMessage[1]);
   cancel(Game.roundMessage[2]);
   cancel(Game.roundMessage[3]);
   cancel(Game.roundMessage[4]);
   cancel(Game.roundMessage[5]);
   cancel(Game.BotWave);
 }
};

function SVGame::initGameVars(%game)
{
   %game.SCORE_PER_KILL = 1;
   %game.SCORE_PER_DEATH = -1;
   %game.SCORE_PER_SUICIDE = -1;
   
   //SV Vars
   %game.rounds = 1;
   %game.playerCount = 0;
   %game.botCount = 0;
   %game.shouldSpawn = 0;
   %game.start = 0;
   %game.AIWon = 0;

   //Now load our game setup vars ...
   %file = "data/survivalPreferences.txt";
   %game.maxBots = getBlockData(%file,"Survival",1,"maxBots");
   %game.difficultyIncrement = getBlockData(%file,"Survival",1,"difficultyIncrement");
   %game.fastDifficultyIncrement = getBlockData(%file,"Survival",1,"fastDifficultyIncrement");
   %game.startDifficulty = getBlockData(%file,"Survival",1,"startDifficulty");
   %game.enableGodbot = getBlockData(%file,"Survival",1,"enableGodbot");
   %game.godBotFrequency = getBlockData(%file,"Survival",1,"godBotFrequency");
   %game.hintsEnabled = getBlockData(%file,"Survival",1,"hintsEnabled");
   %game.hintTimeMS = getBlockData(%file,"Survival",1,"hintTimeMS");
   %game.allowSetup = getBlockData(%file,"Survival",1,"allowSetup");
   %game.setupTimeMS = getBlockData(%file,"Survival",1,"setupTimeMS");
}

function SVGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

   //Ok.. for some odd reason the AI grenade Set isn't created for this gamemode.
   //Without it, the bots don't function very well at all.
   //Make sure it exists when the game starts
   if (!IsObject($AIGrenadeSet))
   AIObjectives.add($AIGrenadeSet = new Simgroup(AIGrenadeSet));
   //Remove console spam..
   if (!IsObject($AIRemoteTurretSet))
   AIObjectives.add($AIRemoteTurretSet = new Simgroup(AIRemoteTurretSet));
   if (!IsObject($ObjectiveQ[2]))
   $ObjectiveQ[2] = new simSet();
}

function SVGame::setUpTeams(%game)
{
  DefaultGame::setUpTeams(%game);
  %game.numTeams = 1; //Players cannot switch teams
  setSensorGroupCount(3); //Team 0, 1, and 2
}

function SVGame::RoundStart(%game)
{
 if (%game.playerCount <= 0)
 {
 messageAll('MsgAll',"\c2The round cannot start untill someone spawns! ~wfx/misc/misc.error.wav");
 Game.shouldSpawn = 1; //Tell the game to start a 10 sec countdown now
 return;
 }

%game.start = 1;
%game.AIWon = 0;
messageAll('MsgSystemClock',"\c2The round has started! ~wfx/misc/red_alert.wav",1,120000); //Start "Player Help" countdown
Game.lockDeadClients();
Game.spawnAI();
Game.helper = Game.schedule(120000,"PlayerHelp");
//Ok.. bots spawn every 30 secs
if ($Host::ProgressiveMode)
Game.BotWave = Game.schedule(180000,"SpawnAI",true);
}

function SVGame::checkTimeLimit(%game, %forced){}

function SVGame::freeDeadClients(%game)
{
 for (%i = 0; %i < ClientGroup.getCount(); %i++)
 {
  %cl = ClientGroup.getObject(%i);
  %cl.isDead = false;
  %cl.kills = 0; //Since this is called every round, might as well reset the kill count for this round
 }
}

function SVGame::lockDeadClients(%game)
{
 for (%i = 0; %i < ClientGroup.getCount(); %i++)
 {
  %cl = ClientGroup.getObject(%i);
  %cl.isDead = true; //Doesn't matter if the dude is alive -- when he dies the flag is set.
 }
}

function SVGame::startMatch(%game)
{
   DefaultGame::startMatch(%game);
   
   if (!$Host::ProgressiveMode)
   messageAll('MsgSystemClock',"\c2The first round will start in one minute.",1,60000);
   else
   messageAll('MsgSystemClock',"\c2The attack will start in one minute!",1,60000);
   
   Game.roundMessage[0] = schedule(50000,0,"messageAll",'msgAll',"\c2Round starts in ten seconds.~wfx/misc/hunters_10.wav");
   Game.roundMessage[1] = schedule(55000,0,"messageAll",'msgAll',"\c2Round starts in five seconds.~wfx/misc/hunters_5.wav");
   Game.roundMessage[2] = schedule(56000,0,"messageAll",'msgAll',"\c2Round starts in four seconds.~wfx/misc/hunters_4.wav");
   Game.roundMessage[3] = schedule(57000,0,"messageAll",'msgAll',"\c2Round starts in three seconds.~wfx/misc/hunters_3.wav");
   Game.roundMessage[4] = schedule(58000,0,"messageAll",'msgAll',"\c2Round starts in two seconds.~wfx/misc/hunters_2.wav");
   Game.roundMessage[5] = schedule(59000,0,"messageAll",'msgAll',"\c2Round starts in one second.~wfx/misc/hunters_1.wav");
   Game.roundStart = Game.schedule(60000,"RoundStart");
   Game.freeDeadClients();
}

function SVGame::checkRounds(%game)
{
if (%game.start)
return;

 for (%i = 0; %i < ClientGroup.getCount(); %i++)
 {
  %cl = ClientGroup.getObject(%i);
   if (IsObject(%cl.player) && %cl.player.getState() $= "move")
   {
    %cl.streak++;
     if(%cl.streak > $Data::Rounds[%cl.GUID,$MissionName])
     {
     messageClient(%cl,'MsgClient','\c2Congratulations, you broke your old Round record of %1!',%cl.streak--);
     $Data::Rounds[%cl.GUID,$MissionName]++;
     }
   }
 }
}

function SVGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc)
{
   cancel(%clVictim.player.alertThread);
   DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc);
   
    if (IsObject(%clVictim) && !%clVictim.isAIControlled())
    {
     %game.playerCount--; //May be someone switching to observer
     if (%game.start)
     %clVictim.isDead = true;
     %clVictim.streak = 0; //Moved from SVGame::CheckRounds to fix .streak being reset for some reason
    }
    
    if (%game.playerCount <= 0 && %game.start)
    {
     %game.rounds = 1;
     %game.start = 0;
     %game.AIWon = 1; //Tell the game the b0ts won, or else when they disconnect, it'll be counted as a player win
     cancel(Game.helper);
     messageAll('MsgSPCurrentObjective1',"",'Current round: 1');
     messageAll('MsgSPCurrentObjective2',"",'Number of bots: 0');
     messageAll('MsgSystemClock',"\c2Round "@%game.rounds@" belongs to the Artificial Intelligance! The game will now restart back at round 1. ~wfx/misc/flag_return.wav",1,60000);
     Game.roundMessage[0] = schedule(50000,0,"messageAll",'msgAll',"\c2Round starts in ten seconds.~wfx/misc/hunters_10.wav");
     Game.roundMessage[1] = schedule(55000,0,"messageAll",'msgAll',"\c2Round starts in five seconds.~wfx/misc/hunters_5.wav");
     Game.roundMessage[2] = schedule(56000,0,"messageAll",'msgAll',"\c2Round starts in four seconds.~wfx/misc/hunters_4.wav");
     Game.roundMessage[3] = schedule(57000,0,"messageAll",'msgAll',"\c2Round starts in three seconds.~wfx/misc/hunters_3.wav");
     Game.roundMessage[4] = schedule(58000,0,"messageAll",'msgAll',"\c2Round starts in two seconds.~wfx/misc/hunters_2.wav");
     Game.roundMessage[5] = schedule(59000,0,"messageAll",'msgAll',"\c2Round starts in one second.~wfx/misc/hunters_1.wav");
     Game.roundStart = Game.schedule(60000,"RoundStart");
     disconnectAllBots();
     Game.freeDeadClients();
    }
}

function SVGame::spawnPlayer( %game, %client, %respawn )
{
   if (%client.isDead)
   return commandToClient(%client, 'CenterPrint', "You will respawn next round.",2);
   else
   {
    if (!%client.isAIControlled())
    {
     %client.team = 1;
     DefaultGame::spawnPlayer(%game, %client, %respawn);
     Game.playerCount++;
     commandToClient(%client,'bottomPrint',"Try not to die.",3);
    if (%game.shouldSpawn)
    {
     %game.shouldSpawn = 0;
     Game.roundMessage[0] = schedule(10000,0,"messageAll",'MsgSystemClock',"\c2Round starts in ten seconds.~wfx/misc/hunters_10.wav",1,10000);
     Game.roundMessage[1] = schedule(5000,0,"messageAll",'msgAll',"\c2Round starts in five seconds.~wfx/misc/hunters_5.wav");
     Game.roundMessage[2] = schedule(6000,0,"messageAll",'msgAll',"\c2Round starts in four seconds.~wfx/misc/hunters_4.wav");
     Game.roundMessage[3] = schedule(7000,0,"messageAll",'msgAll',"\c2Round starts in three seconds.~wfx/misc/hunters_3.wav");
     Game.roundMessage[4] = schedule(8000,0,"messageAll",'msgAll',"\c2Round starts in two seconds.~wfx/misc/hunters_2.wav");
     Game.roundMessage[5] = schedule(9000,0,"messageAll",'msgAll',"\c2Round starts in one second.~wfx/misc/hunters_1.wav");
     Game.roundStart = Game.schedule(10000,"RoundStart");
     Game.schedule(10000,"RoundStart");
    }
    }
   }
}

function SVGame::onAIKilled(%game, %clVictim, %clAttacker, %damageType, %implement)
{
	DefaultGame::onAIKilled(%game, %clVictim, %clAttacker, %damageType, %implement);

    if (IsObject(%clVictim) && %clVictim.isAIControlled())
    {
     %game.botCount--;
     %clVictim.drop();
     messageAll('MsgSPCurrentObjective2',"",'Number of bots: %1',Game.botCount);
    }

      if (%game.botCount <= 0 && !$Host::ProgressiveMode && !%game.AIWon) //There are no rounds..
      {
       messageAll('MsgSystemClock',"\c2Round "@%game.rounds@" belongs to the players! They have thirty seconds to get ready. ~wfx/misc/flag_return.wav",1,30000);
       %game.rounds++;
       %game.start = 0;
       cancel(Game.helper);
       messageAll('MsgSPCurrentObjective1',"",'Current Round: %1',Game.rounds);
       Game.roundMessage[0] = schedule(20000,0,"messageAll",'msgAll',"\c2Round starts in ten seconds.~wfx/misc/hunters_10.wav");
       Game.roundMessage[1] = schedule(25000,0,"messageAll",'msgAll',"\c2Round starts in five seconds.~wfx/misc/hunters_5.wav");
       Game.roundMessage[2] = schedule(26000,0,"messageAll",'msgAll',"\c2Round starts in four seconds.~wfx/misc/hunters_4.wav");
       Game.roundMessage[3] = schedule(27000,0,"messageAll",'msgAll',"\c2Round starts in three seconds.~wfx/misc/hunters_3.wav");
       Game.roundMessage[4] = schedule(28000,0,"messageAll",'msgAll',"\c2Round starts in two seconds.~wfx/misc/hunters_2.wav");
       Game.roundMessage[5] = schedule(29000,0,"messageAll",'msgAll',"\c2Round starts in one second.~wfx/misc/hunters_1.wav");

       //Award a client's ROUNDs if he beat his old round record
       Game.checkRounds();
       //Allow dead peeps to rejoin
       Game.freeDeadClients();
       Game.roundStart = Game.schedule(30000,"RoundStart");
    }
  }

function SVGame::PlayerHelp(%game) //Is called after 2 minutes in each round; tells where the b0ts are
{
 if (%game.start)
 {
 messageAll('MsgSystemClock',"\c2Good hunting! ~wvoice/Training/Any/ANY.hunting.wav",1,120000);
  for (%i =  0; %i < ClientGroup.getcount(); %i++)
  {
   %cl = ClientGroup.getObject(%i);
    if (%cl.isAIControlled() && %cl.team == 2)
    {
     %wp = new Waypoint()
     {
     Datablock = "WayPointMarker";
     Position = %cl.player.getPosition();
     Name = %cl.namebase;
     Team = 2;
     };
     %wp.schedule(3000,"Delete");
    }
  }
 Game.helper = Game.schedule(120000,"PlayerHelp");
 }
}

function SVGame::SpawnAI(%game,%val)
{
 if (%val && !$Host::ProgressiveMode)
 return;
 
 //if (Game.rounds == 2)
// {
 // messageAll('msgAll',"\c3The Godbot is here! ~wfx/misc/red_alert.wav");
 // %bot = aiConnectByName("Godbot",2);
 // %bot.setSkillLevel(1);
 // %bot.player.clearInventory();
//  %bot.player.setArmor("TR2Heavy");
//  %bot.player.setInventory("TR2EnergyPack",1,true);
//  %bot.player.setInventory("TR2Disc",1,true);
//  %bot.player.setInventory("TR2DiscAmmo",900,true);
//  %bot.player.setInventory("TR2Chaingun",1,true);
//  %bot.player.setInventory("TR2ChaingunAmmo",999,true);
//   %bot.player.setInventory("TR2GrenadeLauncher",1,true);
////    %bot.player.setInventory("TR2GrenadeLauncherAmmo",999,true);
 //    %bot.player.setInventory("TR2Shocklance",1,true);
 ///     %bot.player.setInventory("TR2Mortar",1,true);
  //     %bot.player.setInventory("TR2MortarAmmo",999,true);
 //      %bot.player.use("TR2Disc");
 //      %game.botCount = 1;
  ////hideClient(%bot);
 // return;
// }
 %playerCount = clientGroup.getCount() - 1;
 
 for (%i = 0; %i < 2 + Game.rounds+1; %i++)
 {
  %bot = aiConnectByIndex(getRandom(0,$BotProfile::Count),2); //Choose random b0ts by Index
   //Alright -- this'll make the game a bit harder -- depending on the rounds, the bots will have random armors.
   %chance = getRandom(1,5 + Game.rounds);
   %bot.player.clearInventory();
   
   if (%chance > 6) //Medium Armor
   {
    %bot.player.setArmor("Medium");
    %bot.player.setInventory("Chaingun",1,1);
    %bot.player.setInventory("ChaingunAmmo",500,1);
    %bot.player.setInventory("Disc",1,1);
    %bot.player.setInventory("DiscAmmo",30,1);
    %bot.player.setInventory("Shoklance",1,1);
    %bot.player.setInventory("MissileLauncher",1,1);
    %bot.player.setInventory("MissileLauncherAmmo",50);
    %bot.player.use("Chaingun");
   }
   else if (%chance > 10) //JUGGY
   {
    %bot.player.setArmor("Heavy");
    %bot.player.setInventory("Chaingun",1,1);
    %bot.player.setInventory("ChaingunAmmo",500,1);
    %bot.player.setInventory("Disc",1,1);
    %bot.player.setInventory("DiscAmmo",30,1);
    %bot.player.setInventory("Shoklance",1,1);
    %bot.player.use("Chaingun");
   }
   else //Light
   {
    %bot.player.setArmor("Light");
    %bot.player.setInventory("Chaingun",1,1);
    %bot.player.setInventory("ChaingunAmmo",500,1);
    %bot.player.setInventory("Disc",1,1);
    %bot.player.setInventory("DiscAmmo",30,1);
    %bot.player.setInventory("Shoklance",1,1);
    %bot.player.use("Chaingun");
   }
   hideClient(%bot);
   %random = getRandom(0,%playerCount);
   %client = clientGroup.getObject(%random);
   %bot.stepEscort(%client.player);
   %bot.player.setInventory("AmmoPack",1,1);
   setTeam(%bot,2);
   %bot.setSkillLevel(1);
  // %bot.stepEngage(clientGroup.getObject(%random));
 }
 
 if ($Host::ProgressiveMode && %val)
 {
 %game.rounds++; //So we increment every time -- I should put a limit to the bots..
 Game.BotWave = Game.schedule(180000,"SpawnAI",true);
 }
 
messageAll('MsgSPCurrentObjective2',"",'Number of bots: %1',Game.botCount);
}

function SVGame::allowsProtectedStatics(%game)
{
   return true;
}

function SVGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   //%player.setArmor("Light");
   %player.setInventory(RepairKit, 1);
   %player.setInventory("Disc", 1);
   %player.setInventory("DiscAmmo", 15);
   %player.setInventory("TargetingLaser", 1);
   %player.weaponCount = 1;
   
   if (%player.client.race $= "Draakan") //Also defined in DefaultGame.cs, but this overrides it.
   %player.setInventory(Flamer,1);

   // do we want to give players a disc launcher instead? GJL: Yes we do!
   %player.use("Disc");
}

function SVGame::pickPlayerSpawn(%game, %client, %respawn)
{
   // if the client is a bot and on team 2, use team 2 -- otherwise we're always team 1
   if (%client.isAIControlled() && %client.team == 2)
   return %game.pickTeamSpawn(2);
   else
   return %game.pickTeamSpawn(1);
}

function SVGame::clientJoinTeam( %game, %client, %team, %respawn )
{
   %game.assignClientTeam( %client );
   
   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );
}

function SVGame::assignClientTeam(%game, %client, %respawn)
{
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   // if player's team is not on top of objective hud, switch lines
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

function SVGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady',"", "SinglePlayerGame"); //Force the client to set up the SP game objective HUD
   %game.resetScore(%client);
   
   //Setup the client's objective hud
   messageClient(%client,'MsgSPCurrentObjective1',"",'Current round: 1');
   messageClient(%client,'MsgSPCurrentObjective2',"",'Number of bots: 0'); //Should update as soon as our player joins

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 
   
   DefaultGame::clientMissionDropReady(%game, %client);
}

function SVGame::AIHasJoined(%game, %client)
{
   %game.botCount++;
}

function SVGame::checkScoreLimit(%game, %client)
{
   //there's no score limit in DM
}

function SVGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
   %client.setSensorGroup(%client.team);
}

function SVGame::resetScore(%game, %client)
{
   %client.deaths = 0;
   %client.kills = 0;
   %client.score = 0;
   %client.efficiency = 0.0;
   %client.suicides = 0;
}

function SVGame::assignClientTeam(%game, %client, %respawn )
{
   if (!%client.isDead)
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   else
   return true;
}

function SVGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if (%game.testKill(%clVictim, %clKiller)) //verify victim was an enemy
   {
      %game.awardScoreKill(%clKiller);
      messageClient(%clKiller, 'MsgDMKill', "", %clKiller.kills);
      %game.awardScoreDeath(%clVictim);
   }       
   else if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
      %game.awardScoreSuicide(%clVictim);     

   messageClient(%clVictim, 'MsgDMPlayerDies', "", %clVictim.deaths + %clVictim.suicides);
}

function SVGame::recalcScore(%game, %client)
{
   %killValue = %client.kills * %game.SCORE_PER_KILL;
   %deathValue = %client.deaths * %game.SCORE_PER_DEATH;
   %suicideValue = %client.suicides * %game.SCORE_PER_SUICIDE;

   if (%killValue - %deathValue == 0)
      %client.efficiency = %suicideValue;
   else
      %client.efficiency = ((%killValue * %killValue) / (%killValue - %deathValue)) + %suicideValue;

   %client.score = mFloatLength(%client.efficiency, 1);
   messageClient(%client, 'MsgYourScoreIs', "", %client.score);
   %game.recalcTeamRanks(%client);
   %game.checkScoreLimit(%client);
}

function SVGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function SVGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function SVGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);
   Game.GameEnded = true;

   messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );
   
   //cancel the schedules..
   cancel(Game.helper);
   cancel(Game.roundStart);
   cancel(Game.roundMessage[0]);
   cancel(Game.roundMessage[1]);
   cancel(Game.roundMessage[2]);
   cancel(Game.roundMessage[3]);
   cancel(Game.roundMessage[4]);
   cancel(Game.roundMessage[5]);
   cancel(Game.BotWave);

   cancel(%game.timeThread);
   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
   }
}

function SVGame::enterMissionArea(%game, %playerData, %player)
{
   %player.client.outOfBounds = false; 
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");
   cancel(%player.alertThread);
}

function SVGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;
                                         
   %player.client.outOfBounds = true;
   messageClient(%player.client, 'LeaveMissionArea', '\c1You have left the mission area. Return or take damage.~wfx/misc/warning_beep.wav');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") left mission area");
   %player.alertThread = %game.schedule(1000, "DMAlertPlayer", 3, %player);
}

function SVGame::DMAlertPlayer(%game, %count, %player)
{
   // MES - I commented below line out because it prints a blank line to chat window
   //messageClient(%player.client, 'MsgDMLeftMisAreaWarn', '~wfx/misc/red_alert.wav');
   if(%count > 1)
      %player.alertThread = %game.schedule(1000, "DMAlertPlayer", %count - 1, %player);
   else
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
}

function SVGame::MissionAreaDamage(%game, %player)
{
   if(%player.getState() !$= "Dead") {                                   
      %player.setDamageFlash(0.1);
      %prevHurt = %player.getDamageLevel();
      %player.setDamageLevel(%prevHurt + 0.05);
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
   }
   else
      %game.onClientKilled(%player.client, 0, $DamageType::OutOfBounds);
}

function SVGame::updateScoreHud(%game, %client, %tag)
{
   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );
   
   if ($Data::Rounds[%client.GUID,$MissionName] $= "")
   $Data::Rounds[%client.GUID,$MissionName] = 0;

   // Send the subheader:
   messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,340,415>\tPLAYER\tRACE\tKILLS\tROUNDS');

   for (%index = 0; %index < $TeamRank[1, count]; %index++)
   {
      //get the client info
      %cl = $TeamRank[1, %index];

      //get the score
      %clScore = mFloatLength( %cl.efficiency, 1 );

      %clKills = mFloatLength( %cl.kills, 0 );
      %clDeaths = mFloatLength( %cl.deaths + %cl.suicides, 0 );
      %clStyle = %cl == %client ? "<color:dcdcdc>" : "";

      //if the client is not an observer, send the message
      if (%client.team != 0)
      {
         if (%client.race !$= "Draakan") //The type will make some difference someday.. lol
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%6<tab:20, 450>\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:370><just:right>%3<rmargin:460>%4',
               %cl.name, %cl.race, %clKills, $Data::Rounds[%client.GUID,$MissionName], %clDeaths, %clStyle );
         else
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%6<tab:20, 450>\t<clip:200>%1</clip><rmargin:280><just:right>%2 (%7)<rmargin:370><just:right>%3<rmargin:460>%4',
               %cl.name, %cl.race, %clKills, $Data::Rounds[%client.GUID,$MissionName], %clDeaths, %clStyle, %cl.sex );
      }
   }

   // Tack on the list of observers:
   %observerCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team == 0)
         %observerCount++;
   }

   if (%observerCount > 0)
   {
	   messageClient( %client, 'SetLineHud', "", %tag, %index, "");
      %index++;
		messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         //if this is an observer
         if (%cl.team == 0)
         {
            %obsTime = getSimTime() - %cl.observerStartTime;
            %obsTimeStr = %game.formatTime(%obsTime, false);
		      messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2',
		                     %cl.name, %obsTimeStr );
            %index++;
         }
      }
   }

   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index );
}

function SVGame::applyConcussion(%game, %player)
{
}


