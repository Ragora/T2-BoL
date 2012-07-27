// don't want this executing when building graphs
if($OFFLINE_NAV_BUILD)
   return;

//  Script for Training
//===================================================================================
//error("Training 1 script");

//Note: Quite messy right now.. I'll organize when it's done
//Not sure why this mission causes lots of console spam..

// variables
$numberOfEnemies[1] = 6;
$numberOfEnemies[2] = 6;
$numberOfEnemies[3] = 6;
$numberOfTeammates = 1;
$missionBotSkill[1] = 0.2;
$missionBotSkill[2] = 0.4;
$missionBotSkill[3] = 0.7;

// package and callbacks

package Beginning {
//BEGIN BEGINNING PACKAGE =======================================================================

function SinglePlayerGame::initGameVars(%game)
{
   echo("initializing beginning (TDS mission 1) game vars");
   Game.captorName = "Alchaldes"; //Main dude speaking
   Game.controllerName = "Hammurabi"; //Guy at control panel
   Game.guardOneName = "Romulus";
   Game.guardTwoName = "Hercules";
}

function PlayGui::onWake(%this)
{
   parent::onWake(%this);
   //error("Waking training play gui");
   // okay we know the victim...erm...player is looking
   // and we hope they have a body so lets do this
   if(!game.playedIntro) {
      game.PlayGuiAwake = true;
      beginBeginning();
   }

}

function getTeammateGlobals()
{
    $TeammateWarnom0 = "Gila";
	$teammateSkill0 = 0.5;
	$teammateVoice0 = Derm3;
	$teammateEquipment0 = 0;
	$teammateGender0 = B;
}

function MP3Audio::play(%this)
{
	//too bad...no mp3 in training
}

function toggleCommanderMap(%val)
{
    // No command Circuit fer' you!
}

function toggleTaskListDlg( %val )
{
    // Tasks? What tasks? Get the ef out of there when given the chance!
}

function toggleInventoryHud( %val )
{
   // Not even a message
}

function toggleNetDisplayHud( %val )
{
   // Hello, McFly?  This is training!  There's no net in training!
}

function voiceCapture( %val )
{
   // Uh, who do you think you are talking to?
}

function giveall()
{
   error("When the going gets tough...wussies like you start cheating!");
   messageClient($player, 0, "Cheating eh?  What\'s next?  Camping?");
}

// get the ball rolling
//------------------------------------------------------------------------------
function startCurrentMission()
{
	playGui.add(outerChatHud);

   //fade up from black
   ServerConnection.setBlackOut(true, 0);

   new actionMap(ModifiedMap);
   modifiedMap.bind(mouse0, "xaxis", yaw);
   modifiedMap.bind(mouse0, "yaxis", pitch);
}

function singlePlayerDead()
{
	missionFailed($player.miscMsg[TDSGenericLoss]);
	AIMissionEnd();
	$objectiveQ[$enemyTeam].clear();
}

function beginBeginning()
{
if(game.playedIntro)
return;

$Beginning = ServerConnection.schedule(3000, setBlackOut, false, 4000);
$player.player.setTransform(DP.getTransform());
$enemy0.stepMove(AlchaldesVisit.getPosition());
yaw(1);

return;

game.playedIntro = true;
lockArmorHack(true); //Don't let our playah move

//Now set up the victim
turnPlayerToPosition($enemy0.player.getPosition());
$player.player.schedule(3000,"setInvincibleMode",$InvincibleTime,0.02);
schedule(3200,0,"hideHudHACK",false);
pitch(40);
if(!$firstperson)//Make sure we're in first person every time
toggleFirstPerson($player);
//Although both can't jet.. apparently b0ts need them to get around (even jumping down from stuff)
$player.player.setRechargeRate(0);
$player.player.setEnergyLevel(0);

$teammate0.player.setTransform(Warnom0.getTransform());

//Alchaldes (leader of Criollos)
$enemy0.aimAt($player.player.getPosition());
$enemy0.player.clearInventory();
$enemy0.player.setInventory("EnergyPack",1); //Has a med condition; needs energy pack at all times
setName($enemy0,Game.CaptorName);
setSkin($enemy0,"HALO_SKIN");

//Hammurabi (Cloning -- lead researcher)
$enemy1.aimAt($player.player.getPosition());
$enemy1.player.clearInventory();
$enemy1.player.setInventory("EnergyPack",1);
setName($enemy1,Game.ControllerName);
setVoice($enemy1,"Derm3");
setSkin($enemy1,"HALO_SKIN");

//Guard 1 (Looking at Dolosus)
$enemy2.aimAt($player.player.getPosition());
setName($enemy2,Game.GuardOneName);
setVoice($enemy2,"Derm3");
setSkin($enemy2,"HALO_SKIN");

//Guard 2 (Looking at Gila)
$enemy3.aimAt($teammate0.player.getPosition());
setName($enemy3,Game.GuardTwoName);
setVoice($enemy3,"Derm3");
setSkin($enemy3,"HALO_SKIN");

doText(TDS_M101);
doText(TDS_M102,3000);
doText(TDS_M103,3100);
doText(TDS_M104,3200);
doText(TDS_M105,3300);
doText(TDS_M106,3400);
doText(TDS_M107,3500);
doText(TDS_M108,3600);
doText(TDS_M109,3700);
doText(TDS_M101b,3800);
doText(TDS_M102b,3900);
doText(TDS_M103b,4000);
doText(TDS_M104b,4100);
doText(TDS_M105b,4200);
doText(TDS_M106b,4300);
doText(TDS_M107b,4400);
doText(TDS_M108b,4500);
doText(TDS_M109b,4600);
doText(TDS_M101c,4700);
doText(TDS_M102c,4800);
doText(TDS_M103c,4900);
doText(TDS_M104c,5000);
doText(TDS_M105c,5100);
doText(TDS_M106c,5200);
doText(TDS_M107c,5300);
}

function giveEscortTask(%bot, %target)
{
	%newObjective = new AIObjective(AIOEscortPlayer)
		{
			dataBlock = "AIObjectiveMarker";
			weightLevel1 = 10000;
			description = "Escort Player";
			targetClientId = %target;
			offense = true;
		};
	//echo(%newObjective);
	MissionCleanup.add(%newObjective);
	$ObjectiveQ[$playerTeam].add(%newObjective);
    %bot.stepEscort(%target);
	%bot.escort = %newObjective;

}

function detonateAll()
{
ResearchTurret.applyDamage(400);
SecondaryResearch.applyDamage(400);
MainResearch.applyDamage(400);
Turret1.applyDamage(400);
Turret2.applyDamage(400);
Turret3.applyDamage(400);
BaseSensor.applyDamage(400);
BaseGen2.applyDamage(400);
DefenceGen.applyDamage(400);
ReactorGen2.applyDamage(400);
ReactorGen1.applyDamage(0.5);
InventoryStation1.applyDamage(400);
InventoryStation2.applyDamage(400);
InventoryStation3.applyDamage(400);
InventoryStation5.applyDamage(400);
messageAll('msgAll',"\c4Intercom: Main reactor absorbed most of the energy; high damage has been done to the whole network!");
schedule(1500,0,"messageAll",'msgAll',"\c4Gila: Good! Sounds like any defences they have is down.");
//Alright, give the guards and other b0ts the objectives
$enemy2.addTask(AIEngageTask);
$enemy3.addTask(AIEngageTask);
$enemy4.addTask(AIEngageTask);
$enemy5.addTask(AIEngageTask);
}

function storyMoveToHammurabi()
{
$enemy0.stepMove(AlchaldesWorried.getPosition());
$enemy0.schedule(2000,"aimAt",$enemy1.player.getPosition());
$enemy2.aimAt($enemy0.player.getPosition());
$enemy3.aimAt($enemy0.player.getPosition());
}

function storyMoveToGila()
{
$enemy0.stepMove(AlchaldesObserve.getPosition());
$enemy1.stepMove(HammurabiObserve.getPosition());
$enemy0.schedule(2000,"aimAt",$teammate0.player.getPosition());
$enemy1.schedule(2000,"aimAt",$teammate0.player.getPosition());
}

function storyMoveToDolosus()
{
$enemy0.stepMove(enemy0.getPosition());
$enemy1.stepMove(enemy1.getPosition());
$enemy0.schedule(2000,"aimAt",$player.player.getPosition());
$enemy1.schedule(2000,"aimAt",$player.player.getPosition());
}

function hideHudHACK(%bool)
{
objectiveHud.setVisible(%bool);
hudClusterBack.setVisible(%bool);
inventoryHud.setVisible(%bool);
backPackFrame.setVisible(%bool);
weaponsHud.setVisible(%bool);
clockHUD.setVisible(%bool);
retCenterHUD.setVisible(%bool);
}

//------------------------------------------------------------------------------
function SinglePlayerGame::equip(%game, %player)
{
   //ya start with nothing...NOTHING!
   %player.clearInventory();
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   %set = %player.client.equipment;

      if (%player.client.race $= "Draakan")
      {
       %player.setInventory(Flamer,1);
       %player.use(Flamer);
      }
      else
      {
       %player.setInventory("Chaingun",1,true);
       %player.setInventory("ChaingunAmmo",999,true);
       %player.setInventory("Disc",1,true);
       %player.setInventory("DiscAmmo",999,true);
       %player.setInventory("Shocklance",1,true);
       %player.setInventory("AmmoPack",1,true);
       %player.use("Chaingun");
      }
}

function spawnSinglePlayer()
{
   resetWildCat();
   parent::spawnSinglePlayer();
}

function turnPlayerToPosition(%pos)
{
   %vec = VectorSub($player.player.position, %pos);
   %angle = mATan( getWord(%vec, 0), getWord(%vec, 1) );
   %angle = %angle + 3.141529;
   %newTransform = $player.player.position SPC "0 0 1" SPC %angle;
   $player.player.setTransform(%newTransform);
}

function singlePlayerGame::onAIRespawn(%game, %client)
{
   // DONT add the default tasks
   //error("default tasks not added");
}

function singlePlayerGame::playerSpawned(%game, %player)
{
   parent::playerSpawned(%game, %player);
}

function singlePlayerGame::gameOver(%game)
{
        //enable the voice chat menu again...
   if (isObject(training1BlockMap))
   {
      training1BlockMap.pop();
      training1BlockMap.delete();
   }

   if(HelpTextGui.isVisible())
      helpTextGui.setVisible(false);
      
   //Make sure our hidden HUD elements are visible again
   hideHudHACK(true);
   //Make sure our sounds stopped playing
   doSpecialEffect(5);

   //re-enable the use of the settings button...
   SinglePlayerEscSettingsBtn.setActive(1);

   Parent::gameOver();
}

function trainingPreloads() //Load any skins..
{
   navGraph.preload("skins/Gecko.lbioderm", true);
   navGraph.preload("skins/Gecko.mbioderm", true);
   navGraph.preload("skins/Gecko.hbioderm", true);
   navGraph.preload("skins/base.lbioderm", true);
   navGraph.preload("skins/HALO_Skin.lbioderm", true);
   navGraph.preload("skins/HALO_Skin.mbioderm", true);
   navGraph.preload("skins/HALO_Skin.hbioderm", true);
   navGraph.preload("skins/sensor_pulse_large", true);
   navGraph.preload("skins/base.hmale", false);
   navGraph.preload("skins/beagle.hmale", false);
   navGraph.preload("skins/base.mmale", false);
   navGraph.preload("skins/beagle.mmale", false);
   navGraph.preload("skins/base.lmale", false);
   navGraph.preload("skins/swolf.mmale", false);
   navGraph.preload("skins/beagle.lmale", false);
}

function SinglePlayerGame::missionLoadDone(%game)
{
   Parent::missionLoadDone(%game);
   trainingPreloads();
}

function serverCmdBuildClientTask(%client, %task, %team)
{
	// player shouldnt be able to use the voice commands to do anything
}

function cancel(%sched)
{
parent::Cancel(%sched);
return true;
}

function lockArmorHack(%val)
{
   if (%val)
   {
   movemap.pop();
   modifiedmap.push();
   }
   else
   {
   movemap.push();
   modifiedmap.pop();
   }
   //$player.player.setMoveState(true);
   //$player.player.schedule(1000,"setMoveState", false);
}

function SinglePlayerGame::enterMissionArea(%game, %player){}
function SinglePlayerGame::leaveMissionArea(%game, %player)
{
 schedule(1000,0,"missionComplete", $player.miscMsg[TDSBeginningWin] );
 moveMap.schedule(1000, "pop");
}
};

activatePackage(Beginning);



