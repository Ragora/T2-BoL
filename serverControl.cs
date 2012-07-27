//General Server Settings
$Host::allowMissionChangeVote = false; //Mission change vote enabler.

//Server Networking Preferences
//You shouldn't have to touch these unless you are hosting a branch of some server network.
//How to use them:
//1. You should have a server for each segment of your world already set up (IE: Space A1:B3)
//2. For each server's control file, properly enter the IP settings below (EX: $Host::Server["AreaName","Label"] = "IP";
//The "Space" Grid
$Host::Server["Space","A1"] = "192.168.1.1";
$Host::Server["Space","B1"] = "192.168.1.1";
//The Earth Grid
$Host::Server["Earth","A1"] = "192.168.1.1";
$Host::Server["Earth","B1"] = "192.168.1.1";
//The Xeron Grid
$Host::Server["Xeron","A1"] = "192.168.1.1";
$Host::Server["Xeron","B1"] = "192.168.1.1";

//The Birth of Legend Gamemode Preferences
$Host::RPG::keepCorpses = true; //Corpse keep enabler.
$Host::RPG::maxCorpses = 6; //The max number of corpses kept on the map at all times. Default: 6

//Survival Gamemode Preferences
$Host::Survival::maxBots = 16; //The maxmimum number of bots allowed in one round. Default: 16
$Host::Survival::difficultyIncrement = 0.008;  //The difficulty increment for AI's. Default: 0.008
$Host::Survival::fastDifficultyIncrement = 0.09; //The fast difficulty increment for AI's. Default: 0.09
$Host::Survival::startDifficulty = 0.004; //The start difficulty for AI's. Default: 0.004
$Host::Survival::godBotEnabled = true; //God bot enabler.
$Host::Survival::godBotFrequence = 5; //The frequency of the God bot (how many rounds does it take to toggle him?)
$Host::Survival::hintsEnabled = true; //Two-Minute hint enabler.
$Host::Survival::hintTimeMS = 120000; //The time between hints in milliseconds.
$Host::Survival::allowSetup = true; //Setup timer enabler.
$Host::Survival::setupTimeMS = 60000; //Setup time in milliseconds.

