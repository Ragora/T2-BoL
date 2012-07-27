// DisplayName = Evolution

//--- GAME RULES BEGIN ---
// Destroy the other team.
//--- GAME RULES END ---

//exec the AI scripts
exec("scripts/aiEV.cs");

$RequiresClient[EV] = false;
$InvBanList[EV, "C4Charge"] = 1;

//-- tracking  ---
function EVGame::initGameVars(%game)
{
 //%game. = ""; //I guess I'll eventually set some, but most are loaded via BASIC files
}

package EVGame
{
function blah(){} //Eh..
};

function EVGame::setUpTeams(%game)
{
   defaultGame::setUpTeams(%game);
   %game.numTeams = 1;
   setSensorGroupCount(4);
   $TeamDamage = true; //Allow team Damage
}

function EVGame::getTeamSkin(%game, %team)
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

function EVGame::getTeamName(%game, %team)
{
   // ---------------------------------------------------
   // z0dd - ZOD 3/30/02. Only display default team names
   //if ( isDemo() || $host::tournamentMode)
   return $TeamName[%team];
   // ---------------------------------------------------
}

//--------------------------------------------------------------------------
function EVGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);
}

function EVGame::showStalemateTargets(%game)
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

function EVGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function EVGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function EVGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);
   messageAll('MsgClearObjHud', "");
}

function EVGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
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
function EVGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady',"", "SinglePlayerGame"); //Force the SP game objective hud to setup
   %game.resetScore(%client);

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 

   DefaultGame::clientMissionDropReady(%game, %client);

   //Force client Spawn since we're ready now
   schedule(1000,0,"forceClientSpawn",%client,true);

   %client.setControlObject(%client.player);
   commandToClient(%client,'bottomPrint',"Try not to die.",3);

   //Since this is an RPG gamemode, be sure some things are correct. (May have been a mission switch from CTF or some other gamemode)
   commandToClient(%client,'SetScoreText',"PDA - Personal Data Assistant");
   //Make sure the data hud is working.
   messageClient(%client,'MsgSPCurrentObjective1',"",'Location: Unknown.');
   messageClient(%client,'MsgSPCurrentObjective2',"",'Money: $%1.',%client.money);
}

function EVGame::assignClientTeam(%game, %client, %respawn)
{
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   // if player's team is not on top of objective hud, switch lines
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

function EVGame::applyConcussion(%game, %player)
{
}

function EVGame::vehicleDestroyed(%game, %vehicle, %destroyer)
{
}

function EVGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
 defaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
 commandToClient(%clVictim,'HandleScriptedCommand',2);
 //commandToClient(%clVictim,'alxPlayMusic',"T2BOL/Music/TribesHymn.mp3"); //Add a little music that becomes audible if the player idles in death for a bit..
 //No, play this epic audio:
 schedule(2000,0,"messageClient",%clVictim,'MsgDeath',"~wfx/Lose.wav");
 forceScoreScreenOpen(%clVictim,"DEATH");
 $Data::ShouldApply[%clVictim.GUID] = false;
 
 if (%clVictim.isAIControlled())
   %clVictim.drop();
}

//...very very messy PDA code below!!

function EVGame::updateScoreHud(%game, %client, %tag) //This is just here for when the PDA is first opened.
{
 if (%client.PDAPage $= "")
 {
 messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );

 messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Main Page');
 messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Please select a command.");
 
 %index = 0;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Command List:");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tSLFSTS\t1>-Self Statistics</a>");
  %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tEMAIL\t1>-Electronic Mail</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tINVENTORY\t1>-View Inventory</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tCLNMG\t1>-Clan Management</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tSAVE\t1>-Save Game</a>");

 %client.PDAPage = "MAIN";
 }
}
function EVGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
commandToClient(%client,'HandleScriptedCommand',9,formatTimeString("HHnn") SPC "Hrs.");
}

function EVGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5)
{
messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );

//Special handles here..
if (getSubStr(%arg1,0,5) $= "EMAIL" && getSubStr(%arg1,5,1) !$= "" && isNumber(getSubStr(%arg1,5,1)))
{
%id = getSubStr(%arg1,5,strLen(%arg1));
%client.email = %id;
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:left><a:gamelink\tDELETE\t1>Delete E-Mail</a>                     "@$Data::EMail::Date[%client.GUID,%i]@"<just:right><a:gamelink\tREPLY\t1>Reply</a>");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>From %1:',$Data::EMail::Sender[%client.GUID,%i]);
%index++;
%count = getSubStrOccurance($Data::EMail::Content[%client.GUID,%id],"\t");
echo(%count);

if (%count != 0)
for (%i = 0; %i < %count; %i++)
{
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>%1',getField($Data::EMail::Contents[%client.GUID,%id],%i));
%index++;
}
else
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>%1',$Data::EMail::Contents[%client.GUID,%id]);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tEMAIL\t1>Back To Listing</a>");
return;
}
else if (getSubStr(%arg1,0,7) $= "EMAILID" && isNumber(getSubStr(%arg1,7,strLen(%arg1))))
{
%id = getSubStr(%arg1,7,strLen(%arg1));
%guid = $Data::Client[%id];
%count = $Data::EMail::Count[%guid];
%name =  $Data::ClientName[%id];
%clid = plNameToCid(%name);
if (checkEmails(%guid,%client.emailTitle))
return;
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", '<just:center>E-Mail sent to %1.',%name);
messageClient( %client, 'SetLineHud', "", 'scoreScreen', 1, '<just:center><a:gamelink\tEMAIL\t1>Back to Listing</a>');
$Data::EMail::Title[%guid,%count] = %client.emailTitle;
$Data::EMail::Sender[%guid,%count] = %client.namebase;
$Data::EMail::Contents[%guid,%count] = %client.emailCont;
$Data::EMail::Date[%guid,%count] = formatTimeString("DD, MM dd, yy @ hh:nn A");
$Data::EMail::Count[%guid]++;
if (IsObject(%clid))
messageClient(%clid,'msgClient','\c3Received an E-Mail from %1. ~wfx/misc/warning_beep.wav',%client.namebase);
return;
}

switch$ (%arg1)
{
case "CLOSE":
closeScoreScreen(%client);
case "BACK": //Resets you to the main menu
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Main Page<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Please select a command.");

 %index = 0;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>Command List:");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tSLFSTS\t1>-Self Statistics</a>");
  %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tEMAIL\t1>-Electronic Mail</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tINVENTORY\t1>-View Inventory</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tCLNMG\t1>-Clan Management</a>");
 %index++;
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tSAVE\t1>-Save Game</a>");

%client.PDAPage = "MAIN";

case "CLNSTP":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Clan Management<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing clan setup.");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Clan Name: %1 <a:input\tClanN\tClan Name\tClan Name>[Change]</a>',%client.clanN);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Clan Tag: %1 <a:input\tClanT\tClan Tag\tClan Tag>[Change]</a>',%client.clanT);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Short Description: %1 <a:input\tDesc\tShort Description\tDescription>[Change]</a>',%client.description);
%index++;

if (%client.clanN !$= "" && %client.clanT !$= "")
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tCLNFN>Done</a>');
%index++;

%client.PDAPage = "CLNSTP";

case "EMAIL":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing E-Mails in sequential order.");

%index = 0;
%count = $Data::EMail::Count[%client.GUID];
for (%i = 0; %i <= %count; %i++)
{
 if ($Data::EMail::Title[%client.GUID,%i] !$= "")
 {
  messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tEMAIL%1\t1>%2 - %3</a>',%i,$Data::EMail::Sender[%client.GUID,%i],$Data::EMail::Title[%client.GUID,%i]);
  %index++;
 }
}

if (%index == 0)
{
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center>There are no E-Mails to show.");
%index++;
}

messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tEMAILSEND\t1>[Compose an E-Mail]</a>");
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
%index++;

case "EMAILSEND":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>New E-Mail");

%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Title: %1 <a:input\temailTitle\tTitle\tE-Mail Title>[Change]</a>',%client.emailTitle);
%index++;

%count = getSubStrOccurance(%client.emailCont,"\t");

if (%count != 0)
for (%i = 0; %i < %count; %i++)
{
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Contents: %1 <a:input\temailCont\tContents\tContents>[Change]</a>',%client.emailCont);
}
else
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Contents: %1 <a:input\temailCont\tContents\tContents>[Change]</a>',%client.emailCont);
%index++;
if (%client.emailTitle !$= "" && %client.emailcont !$= "")
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tRECPT>Select Receptiant</a>');

case "RECPT":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Select Receptiant");

%count = $Data::ClientCount;
%index = 0;
for (%i = 0; %i < %count; %i++)
{
 messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tEMAILID%1\t1>%2</a>',%i,$Data::ClientName[%i]);
 %index++;
}

case "CLNFN":
if ($Data::IsInClan[%client.GUID])
{
%ID = $Data::ClanID[%client.GUID];
 if ($Data::ClanLeaderGUID[%ID] == %client.GUID)
 {
 $Data::ClanName[%ID] = %client.clanN;
 $Data::ClanTag[%ID] = %client.clanT;
 $Data::ClanDesc[%ID] = %client.description;
 forceScoreScreenOpen(%client,"CLNMG");
 }
}
else
{
$Data::IsInCLan[%client.GUID] = true;

if ($Data::ClanCount $= "")
$Data::ClanCount = 0;

$Data::ClanName[$Data::ClanCount] = %client.clanN;
$Data::ClanTag[$Data::ClanCount] = %client.clanT;
$Data::ClanDesc[$Data::ClanCount] = %client.desc;
$Data::ClanLeader[$Data::ClanCount] = %client.namebase;
$Data::ClanLeaderGUID[$Data::ClanCount] = %client.GUID;
$Data::ClanMember[$Data::ClanCount, 0] = %client.GUID;
$Data::ClanID[%client.GUID] = $Data::ClanCount;
$Data::ClanCount++;
setName(%client,"\cp\c7" @ %client.clanT @ "\c6" @ %client.namebase @ "\co");
saveGame();
forceScoreScreenOpen(%client,"CLNMG");
}

%client.PDAPage = "MAIN";

case "CLNMG":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Clan Management<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing clan management.");
%index = 0;

if (!$Data::IsInClan[%client.GUID])
{
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>You are not in a clan. Try creating a <a:gamelink\tCLNSTP>[New Clan]</a>.');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Or view the <a:gamelink\tCLNLST>[List]</a> of clans.');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
}
else
{
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Clan Name: %1',$Data::ClanName[$Data::ClanID[%client.GUID]]);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Clan Tag: %1', $Data::ClanTag[$Data::ClanID[%client.GUID]]);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Short Description: %1',%client.description);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tCLNMBR>[View]</a> member list.');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tCLNLST>[View]</a> other clans.');
if ($Data::IsInClan[%client.GUID] && $Data::ClanLeaderGUID[$Data::ClanID[%client.GUID]] == %client.GUID)
{
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tCLNSTP>[Edit]</a> Clan.');
}
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
}

%client.PDAPage = "CLNMG";

case "CLNMBR":
%client.PDAPage = "CLNMBR";

messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Clan Management<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing member list.");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Name: %1',%client.namebase);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tBACK\t1>Back To Previous Page</a>');
%index++;

 
case "SLFSTS": //Self Statistics
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Self Statistics<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing your stats.");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Name: %1',%client.namebase);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Race: %1',%client.race);
%index++;

if (%client.money $= "")
%client.money = 0; //You got zilch.

%trans = %client.player.getTransform();
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Cash: $%1',%client.money);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>GPS Coordinates: %1 %2 %3',getWord(%trans,0),getWord(%trans,1),getWord(%trans,2));
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '');
%index++;

messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tSLFSTS\t1>Refresh</a>');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
%client.PDAPage = "SLFSTS";

case "INVENTORY": //View Inventory
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Show Inventory<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing your inventory.");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, 'Minerals--');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, 'Steel: %1 units.', %client.units["Steel"]);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
%client.PDAPage = "INVENTORY";

case "DEATH":
if (isObject(%client.player) && %client.player.getState() $= "Move")
return;

messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Death<just:right>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>You have died.");

messageClient( %client, 'SetLineHud', "", 'scoreScreen', 0, "<just:center><a:gamelink\tRESPAWN\t1>Respawn</a>");
%client.PDAPage = "DEATH";

case "DELETE":
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Electronic Mail<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>E-Mail deleted.");
messageClient( %client, 'SetLineHud', "", 'scoreScreen', 0, '<just:center><a:gamelink\tEMAIL\t1>Back to Listing</a>');
$Data::EMail::Title[%client.GUID,%client.email] = "";

case "SAVE":
saveGame();
closeScoreScreen(%client);
messageClient(%client,'msgSaveSuccess',"\c3Game successfully saved.");

case "RESPAWN":
if (isObject(%client.player) && %client.player.getState() $= "Move")
return;

%client.PDAPage = "MAIN";
forceClientSpawn(%client);

case "SLFSTS": //Self Statistics
messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Self Statistics<just:right><a:gamelink\tCLOSE\t1>Close</a>');
messageClient( %client, 'SetScoreHudSubheader', "", "<just:center>Showing your stats.");
%index = 0;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Name: %1',%client.namebase);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Race: %1',%client.race);
%index++;

if (%client.race $= "Draakan")
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Type: %1',%client.sex);
else
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Sex: %1',%client.sex);
%index++;

if (%client.money $= "")
%client.money = 0; //You got zilch.

%trans = %client.player.getTransform();
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>Cash: $%1',%client.money);
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center>GPS Coordinates: %1 %2 %3',getWord(%trans,0),getWord(%trans,1),getWord(%trans,2));
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '');
%index++;

messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, '<just:center><a:gamelink\tSLFSTS\t1>Refresh</a>');
%index++;
messageClient( %client, 'SetLineHud', "", 'scoreScreen', %index, "<just:center><a:gamelink\tBACK\t1>Back To Main Menu</a>");
%client.PDAPage = "SLFSTS";

default: //If something fails, return to the main menu.
serverCmdProcessGameLink(%client,"BACK");
}
}

function serverCmdShowHud(%client, %tag)
{

   %tagName = getWord(%tag, 1);
   %tag = getWord(%tag, 0);
   
   if (%tag $= 'scoreScreen')
   serverCmdProcessGameLink(%client,%client.PDAPage);

   messageClient(%client, 'OpenHud', "", %tag);
   switch$ (%tagname)
   {
      case "vehicleHud":
         vehicleHud::updateHud(1,%client,%tag);
      case "scoreScreen":
         updateScoreHudThread(%client, %tag);

   }
}

