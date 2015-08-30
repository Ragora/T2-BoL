// don't want this executing when building graphs
if($OFFLINE_NAV_BUILD)
   return;

//  Script for Training
//===================================================================================
//error("Training 1 script");

//Note: Quite messy right now.. I'll organize when it's done

// package and callbacks
activatePackage(HotZone);

// variables
$numberOfEnemies[1] = 0;
$numberOfEnemies[2] = 0;
$numberOfEnemies[3] = 0;
$numberOfTeammates = 5;
$missionBotSkill[1] = 0.0;
$missionBotSkill[2] = 0.4;
$missionBotSkill[3] = 0.7;

// additional mission Audio
datablock AudioProfile(HeartbeatSound)
{
   filename    = "fx/misc/heartbeat.wav";
   description = Audio2D;
   preload = true;
   looping = false;
};

package HotZone {
//BEGIN TRAINING PACKAGE =======================================================================

function SinglePlayerGame::initGameVars(%game)
{
   echo("initializing training1 game vars");
}

function getTeammateGlobals()
{
	$TeammateWarnom0 = "Raptor";
	$teammateskill0 = 0.5;
	$teammateVoice0 = Derm3;
	$teammateEquipment0 = 0;
	$teammateGender0 = A;

	$TeammateWarnom1 = "Cobra";
	$teammateSkill1 = 0.5;
	$teammateVoice1 = Derm3;
	$teammateEquipment1 = 0;
	$teammateGender1 = C;
 
    $TeammateWarnom2 = "Sharp Tooth";
	$teammateSkill2 = 0.5;
	$teammateVoice2 = Derm3;
	$teammateEquipment2 = 0;
	$teammateGender2 = A;
 
    $TeammateWarnom3 = "Snake";
	$teammateSkill3 = 0.5;
	$teammateVoice3 = Derm3;
	$teammateEquipment3 = 0;
	$teammateGender3 = A;
 
    $TeammateWarnom4 = "Gila";
	$teammateSkill4 = 0.5;
	$teammateVoice4 = Derm3;
	$teammateEquipment4 = 0;
	$teammateGender4 = B;
}


function AIFollowPath::assume(%task, %client)
{
   %task.setWeightFreq(30);
   %task.setMonitorFreq(10);


//    //next, start the pilot on his way to mounting the vehicle
//    %client.pilotVehicle = true;
//    %client.stepMove($player.flyer.position, 0.25, $AIModeMountVehicle);
}

function AIFollowPath::weight(%task, %client)
{
   %task.setWeight(10000);
}

function AIFollowPath::monitor(%task, %client)
{
   //messageall(0, " AITraining1Pilot::monitor "@%task.locationIndex);
   %group = nameToId(FlightPath);
   if(!%task.locationIndex)
      %task.locationIndex = 0;

   //HACK ALERT!!!
   //since the path for this mission is completely straight, always head for the end of the path
   //%location = %group.getObject(%task.locationIndex);
   %location = %group.getObject(%group.getCount() - 1);

   //see if we've mounted yet
   if(%client.vehicleMounted)
   {
      %client.setPilotDestination(%location.position);

      //else see if we're close enough to the current destination to choose the next
      %pos = %client.vehicleMounted.position;
      %pos2D = getWord(%pos, 0) SPC getWord(%pos, 1) SPC "0";
      %dest =  %group.getObject(%task.locationIndex).position;
      %dest2D = getWord(%dest, 0) SPC getWord(%dest, 1) SPC "0";

      if (VectorDist(%dest2D, %pos2D) < 20)
      {
         if(%group.getCount() > %task.locationIndex + 1) {
            %task.locationIndex++;
            cinematicEvent(%task.locationIndex);
         }
         //else messageAll(0, "Ride Over");
      }
   }
   else
      %client.stepMove($player.flyer.position, 0.25, $AIModeExpress);
}

function PlayGui::onWake(%this)
{
   parent::onWake(%this);
   //error("Waking training play gui");
   // okay we know the victim...erm...player is looking
   // and we hope they have a body so lets do this
   if(!game.playedIntro) {
      game.PlayGuiAwake = true;
      beginHotZone();
   }

}

function aiSetLoadout(%client)
{
%client.player.clearInventory();
%client.player.setArmor("heavy");
%client.player.setInventory("Chaingun",1,true);
%client.player.setInventory("ChaingunAmmo",999,true);
%client.player.setInventory("Disc",1,true);
%client.player.setInventory("Discammo",999,true);
%client.player.setInventory("Shocklance",1,true);
%client.player.setInventory("MissileLauncher",1,true);
%client.player.setInventory("MissileLauncherAmmo",999,true);
%client.player.setInventory("Mortar",1,true);
%client.player.setInventory("MortarAmmo",999,true);
%client.player.setInventory("AmmoPack",1,true);
%client.player.use("Mortar");
}

function beginHotZone() //Don't let the game reset itself a bunch of times
{
if (Game.playedIntro)
return;
%spawn = nameToId();

$player.flyer = new FlyingVehicle(Flyer) {
 position = %spawn.position;
 rotation = %spawn.rotation;
 scale = "1 1 1";
 dataBlock = "HAPCFlyer";
 };
 
%pilot = $teammate0;
game.playedIntro = true;
$player.flyer.pilot = $teammate0;
setTargetSkin(%pilot.target, $teamSkin[$playerTeam]);
%pilot.player.setArmor(light);
%pilot.pilotVehicle = false;
$player.flyer.mountObject(%pilot.player, 0);
%pilot.setControlObject($player.flyer);
%pilot.setPilotPitchRange(-0.2, 0.05, 0.05);
%pilot.addTask(AIFollowPath);

$player.flyer.mountObject($player.player, 1);
$player.flyer.mountObject($teammate1.player, 2);
$player.flyer.mountObject($teammate2.player, 3);
$player.flyer.mountObject($teammate3.player, 4);
$player.flyer.mountObject($teammate4.player, 5);
aiSetLoadout($teammate1);
aiSetLoadout($teammate2);
aiSetLoadout($teammate3);
aiSetLoadout($teammate4);
$player.player.setTransform($player.player.position SPC %spawn.rotation);
$HotZoneBlackout = ServerConnection.schedule(3000, setBlackOut, false, 4000);

//Has to be added after all the bots.. otherwise they die by lava somehow
new WaterBlock(Lava) {
position = "-224 -264 93.1568";
rotation = "1 0 0 0";
scale = "768 608 27.0092";
liquidType = "Lava";
density = "1";
viscosity = "15";
waveMagnitude = "1";
surfaceTexture = "LiquidTiles/Lava";
surfaceOpacity = "1";
envMapTexture = "lava/skies/lava_starrynite_emap";
envMapIntensity = "0.2";
submergeTexture[0] = "special/lavadeath_1";
submergeTexture[1] = "special/lavadeath_2";
removeWetEdges = "1";

locked = "true";
};

}

function MP3Audio::play(%this)
{
	//too bad...no mp3 in training
}

function toggleCommanderMap(%val)
{
   if ( %val )
      messageClient($player, 0, $player.miscMsg[noCC]);
}

function toggleTaskListDlg( %val )
{
   if ( %val )
      messageClient( $player, 0, $player.miscMsg[noTaskListDlg] );
}

function toggleInventoryHud( %val )
{
   if ( %val )
      messageClient( $player, 0, $player.miscMsg[noInventoryHUD] );
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
   
      echo("using default equipment");

      %player.setArmor("Light");
      %player.setInventory(RepairKit,1);
      %player.setInventory(Chaingun, 1);
      %player.setInventory(ChaingunAmmo, 100);
      %player.setInventory(Disc,1);
      %player.setInventory(DiscAmmo, 20);
      %player.setInventory(Shocklance, 1);
      %player.setInventory(AmmoPack,   1);
      
      //DefaultGame.cs does not assign flamer..
      %player.setInventory(flamer,     1);
      %player.weaponCount = 4;

      %player.use(Chaingun);
}

function spawnSinglePlayer()
{
   resetWildCat();
   parent::spawnSinglePlayer();
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
};

package vehicleHack
{
function AIConnection::isMountingVehicle(%client){ return true; }
};
activatePackage(vehicleHack);

function setActionThread(%player,%anim,%bool)
{
%player.setActionThread(%anim,%bool);
}



