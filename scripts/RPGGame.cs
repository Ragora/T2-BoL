// DisplayName = Birth of Legend

//--- GAME RULES BEGIN ---
// There are none, the gameMode conforms itself towards what you do.
//--- GAME RULES END ---

// Notes:
// In Role Playing, you select your race (at the warrior pane), join an RPG server and your
// character is setup (specifically to that server). From there, you can start clans, build
// bases, and war with each other. Basically, it's a freeMode sort of thing.

//exec the AI scripts
exec("scripts/aiRPG.cs");

// exec a bunch of dependencies
exec("scripts/modScripts/server/RPG/initialise.cs");

$InvBanList[RPG, "C4Charge"] = 1;

//-- tracking  ---
function RPGGame::initGameVars(%game)
{
 //%game. = ""; //I guess I'll eventually set some, but most are loaded via BASIC files
}

package RPGGame
{
	function notifyMatchStart(%time){}
	function notifyMatchEnd(%time){}
	
	// FIXME: I have no idea where to find this serverCmd so here's quick hack
	function serverCmdResetControlObject(%client)
	{
		parent::serverCmdResetControlObject(%client);
		if ($CurrentMissionType $= "RPG" && isObject(%client.player) && %client.player.getState() $= "move")
			%client.setControlObject(%client.player);
	}
};
function RPGGame::timeLimitReached(%game){}
function RPGGame::scoreLimitReached(%game){}
function RPGGame::gameOver(%game){}
function RPGGame::checkTimeLimit(%game, %forced){}
function RPGGame::leaveMissionArea(%game, %playerData, %player){}
function RPGGame::enterMissionArea(%game, %playerData, %player){}

function RPGGame::setUpTeams(%game)
{
	defaultGame::setUpTeams(%game);
	%game.numTeams = 1;
	setSensorGroupCount(4);
	$TeamDamage = true; //Allow team Damage
}

function RPGGame::getTeamSkin(%game, %team)
{
    if($host::tournamentMode) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
    {
        return $teamSkin[%team];
    }

    else
    {
    //error("CTFGame::getTeamSkin");
    if(!$host::useCustomSkins)
    {
        %terrain = MissionGroup.musicTrack;
        //error("Terrain type is: " SPC %terrain);
        switch$(%terrain)
        {
            case "lush":
                if(%team == 1)
                    %skin = 'beagle';
                else if(%team == 2)
                    %skin = 'dsword';
                else %skin = 'base';
            
            case "badlands":
                if(%team == 1)
                    %skin = 'swolf';
                else if(%team == 2)
                    %skin = 'dsword';
                else %skin = 'base';
            
            case "ice":
                if(%team == 1)
                    %skin = 'swolf';
                else if(%team == 2)
                    %skin = 'beagle';
                else %skin = 'base';
            
            case "desert":
                if(%team == 1)
                    %skin = 'cotp';
                else if(%team == 2)
                    %skin = 'beagle';
                else %skin = 'base';
            
            case "Volcanic":
                if(%team == 1)
                    %skin = 'dsword';
                else if(%team == 2)
                    %skin = 'cotp';
                else %skin = 'base';
            
            default:
                if(%team == 2)
                    %skin = 'baseb';
                else %skin = 'base';
        }
    }
    else %skin = $teamSkin[%team];
    
    //error("%skin = " SPC getTaggedString(%skin));
    return %skin;
}
}

function RPGGame::getTeamName(%game, %team)
{
   // ---------------------------------------------------
   // z0dd - ZOD 3/30/02. Only display default team names
   //if ( isDemo() || $host::tournamentMode)
   return $TeamName[%team];
   // ---------------------------------------------------
}

//--------------------------------------------------------------------------
function RPGGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);
}

function RPGGame::showStalemateTargets(%game)
{
   cancel(%game.stalemateSchedule);

   //show the targets
   for (%i = 1; %i <= 2; %i++)
   {
      %flag = $TeamFlag[%i];

      //find the object to scope/waypoint....
      //render the target hud icon for slot 1 (a centermass flag)
      //if we just set him as always sensor vis, it'll work fine.
      if (isObject(%flag.carrier))
         setTargetAlwaysVisMask(%flag.carrier.getTarget(), 0x7);
   }
   //schedule the targets to hide
   %game.stalemateObjsVisible = true;
   %game.stalemateSchedule = %game.schedule(%game.stalemateDurationMS, hideStalemateTargets);
}

function RPGGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
   if(%clVictim.headshot && %damageType == $DamageType::Laser && %clVictim.team != %clAttacker.team)
   {
      %clAttacker.scoreHeadshot++;
      if (%game.SCORE_PER_HEADSHOT != 0)
      {
         messageClient(%clAttacker, 'msgHeadshot', '\c0You received a %1 point bonus for a successful headshot.', %game.SCORE_PER_HEADSHOT);
         messageTeamExcept(%clAttacker, 'msgHeadshot', '\c5%1 hit a sniper rifle headshot.', %clAttacker.name); // z0dd - ZOD, 8/15/02. Tell team
      }
      %game.recalcScore(%clAttacker);
   }

   // -----------------------------------------------
   // z0dd - ZOD, 8/25/02. Rear Lance hits
   if(%clVictim.rearshot && %damageType == $DamageType::ShockLance && %clVictim.team != %clAttacker.team)
   {
      %clAttacker.scoreRearshot++;
      if (%game.SCORE_PER_REARSHOT != 0)
      {
         messageClient(%clAttacker, 'msgRearshot', '\c0You received a %1 point bonus for a successful rearshot.', %game.SCORE_PER_REARSHOT);
         messageTeamExcept(%clAttacker, 'msgRearshot', '\c5%1 hit a shocklance rearshot.', %clAttacker.name);
      }
      %game.recalcScore(%clAttacker);
   }
   // -----------------------------------------------

   //the DefaultGame will set some vars
   DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
    
   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer    
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT, %clAttacker.dmgdFlagCarrier = false);
   }
}
      
////////////////////////////////////////////////////////////////////////////////////////
function RPGGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady',"", "SinglePlayerGame"); //Force the SP game objective hud to setup
   %game.resetScore(%client);
   
   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 

   DefaultGame::clientMissionDropReady(%game, %client);

   //Force client Spawn since we're ready now
   %client.schedule(100,"spawn");
   //%client.setControlObject(%client.player);
   commandToClient(%client,'bottomPrint',"Try not to die.",3);
   
   // FIXME: Always assigns radio access for now
   %client.hasRadio = 1;
   %client.radioFrequency = 1;

   // Since this is an RPG gamemode, be sure some things are correct. (May have been a mission switch from CTF or some other gamemode)
   commandToClient(%client,'SetScoreText',"PDA - Personal Data Assistant");
   //Make sure the data hud is working.
   messageClient(%client,'MsgSPCurrentObjective1',"",'Location: Unknown.');
   messageClient(%client,'MsgSPCurrentObjective2',"",'Money: $%1.',%client.money);
}

function RPGGame::assignClientTeam(%game, %client, %respawn)
{
	DefaultGame::assignClientTeam(%game, %client, %respawn);
	// if player's team is not on top of objective hud, switch lines
	messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

function RPGGame::applyConcussion(%game, %player)
{
}

function RPGGame::vehicleDestroyed(%game, %vehicle, %destroyer)
{
}

function RPGGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
	defaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
	commandToClient(%clVictim,'HandleScriptedCommand',2);
	//No, play this epic audio:
	schedule(2000,0,"messageClient",%clVictim,'MsgDeath',"~wfx/Lose.wav");
	forceScoreScreenOpen(%clVictim,"DEATH");
	$Data::ShouldApply[%clVictim.GUID] = false;
	 
	if (%clVictim.isAIControlled())
		%clVictim.drop();
}

function RPGGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
	DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
	%client.observerMode = "";
	commandToClient(%client, 'setHudMode', 'Standard');
	%client.setControlObject(%client.player);
}

function RPGGame::startMatch(%game)
{
   echo("START MATCH");
   //the match has been started, clear the team rank array, and repopulate it...
   for (%i = 0; %i < 32; %i++)
      %game.clearTeamRankArray(%i);

   //used in BountyGame, prolly in a few others as well...
   $matchStarted = true;

   %game.clearDeployableMaxes();

   $missionStartTime = getSimTime();
   %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000);

   // schedule first timeLimit check for 20 seconds
   if(%game.class !$= "SiegeGame")
   {
      %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
   }

   //schedule the end of match countdown
   EndCountdown($Host::TimeLimit * 60 * 1000);

   //reset everyone's score and add them to the team rank array
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      %game.resetScore(%cl);
      %game.populateTeamRankArray(%cl);
   }
   
   // set all clients control to their player
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      
      // Siege game will set the clock differently
      if(%game.class !$= "SiegeGame")
         messageClient(%cl, 'MsgSystemClock', "", $Host::TimeLimit, %curTimeLeftMS);
      
      if( !$Host::TournamentMode && %cl.matchStartReady && %cl.camera.mode $= "pre-game")
      {
         commandToClient(%cl, 'setHudMode', 'Standard');
         %cl.setControlObject( %cl.player );
      }
      else
      {
         if( %cl.matchStartReady )
         {
            if(%cl.camera.mode $= "pre-game")
            {
               %cl.observerMode = "";
               commandToClient(%cl, 'setHudMode', 'Standard');
               
               if(isObject(%cl.player))
                  %cl.setControlObject( %cl.player );
               else
                  echo("can't set control for client: " @ %cl @ ", no player object found!");
            }
            else
               %cl.observerMode = "observerFly";
         }
      }
   }
   
   // on with the show this is it!
   AISystemEnabled( true );
}