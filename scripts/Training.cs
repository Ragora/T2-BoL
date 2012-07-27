// don't want this executing when building graphs
if($OFFLINE_NAV_BUILD)
   return;

//  Script for Training
//===================================================================================
//error("Training 1 script");

// package and callbacks
activatePackage(Training);

// variables
$numberOfEnemies[1] = 1;
$numberOfEnemies[2] = 2;
$numberOfEnemies[3] = 3;
$numberOfTeammates = 0; //Raptor, Komodo Dragon and Sharp Tooth - Don't spawn them automatically though
$missionBotSkill[1] = 0.0;
$missionBotSkill[2] = 0.4;
$missionBotSkill[3] = 0.7;

package Training {
//BEGIN TRAINING PACKAGE =======================================================================

function SinglePlayerGame::initGameVars(%game)
{
   echo("initializing training1 game vars");
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

function getTeammateGlobals()
{
//No console errors
}

function openingSpiel()
{
}

// get the ball rolling
//------------------------------------------------------------------------------
function startCurrentMission()
{
	playGui.add(outerChatHud);

    $Player.player.setTransform(PlayerSpawn.getPosition() SPC "0 0 1 3.97136");
    //Raptor (Trainer)
    $Bot1 = aiConnectByName("Raptor",1);
    $Bot1.race = "Draakan";
    $Bot1.sex = "A";
    setVoice($Bot1, "Derm3");
    setSkin($Bot1, "Gecko");
    $Bot1.player.setArmor("LIGHT");
    $Bot1.player.setTransform(RaptorSpawn.getPosition());
    $Bot1.aimAt($player.player.getPosition());
    //Sharp Tooth
    $Bot2 = aiConnectByName("Sharp Tooth",1);
    $Bot2.race = "Draakan";
    $Bot2.sex = "A";
    setVoice($Bot2, "Derm3");
    setSkin($Bot2, "Gecko");
    $Bot2.player.setArmor("LIGHT");
    $Bot2.player.setTransform(Spawn01.getPosition());
    $Bot2.aimAt($Bot1.player.getPosition());
     //Komodo Dragon
    $Bot3 = aiConnectByName("Komodo Dragon",1);
    $Bot3.race = "Draakan";
    $Bot3.sex = "A";
    setVoice($Bot3, "Derm3");
    setSkin($Bot3, "Gecko");
    $Bot3.player.setArmor("LIGHT");
    $Bot3.player.setTransform(Spawn02.getPosition());
    $Bot3.aimAt($Bot1.player.getPosition());
    //Start our texts!
    openingSpiel();
    $DoneIntro = false;
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

function PlayGui::onWake(%this)
{
   parent::onWake(%this);

   if (!$DoneIntro)
   {
   lockArmorHack(1);
   ServerConnection.setBlackOut(false, 1000);
   $DoneIntro = true;
   }
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
   navGraph.preload("skins/base.lbioderm", true);
   navGraph.preload("skins/Horde.lbioderm", false);
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

function openingSpiel() //Not sure how T2 originally handled the training.. but it's temporary
{
updateTrainingObjectiveHud(obj1);
schedule(1000,0,"setActionThread",$bot1.player,"cel1",0);
schedule(3000,0,"setActionThread",$bot2.player,"cel1",0);
schedule(3000,0,"setActionThread",$bot3.player,"cel1",0);
schedule(3000,0,"setActionThread",$player.player,"cel1",0);
doText(TR_01, 3200);
doText(TR_02, 3500);
doText(TR_03, 3800);
doText(TR_04, 4100);
doText(TR_05, 4400);
schedule(4400,0,"lockArmorHack",false); //Got to release our hack for it to work..
schedule(4400,0,"updateTrainingObjectiveHud",obj2);
}

function lockArmorHack(%bool)
{
   if (%bool)
   {
   movemap.pop();
   }
   else
   moveMap.push();
   //$player.player.setMoveState(true);
   //$player.player.schedule(1000,"setMoveState", false);
}
};

function setActionThread(%player,%anim,%bool)
{
%player.setActionThread(%anim,%bool);
}



