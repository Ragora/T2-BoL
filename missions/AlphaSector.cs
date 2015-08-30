//Alpha Sector (broke) script
package AlphaSector //Package is our mission Name
{
 function defineGeneralAI()
 {
 //These arrays are for general bots
 $Bot[0,"Name"] = "Jeff Harding"; //ftl, Jeff.
 $Bot[0,"Race"] = "Human";

 //Very important or the console will get spammed.
 //Bioderms and Criollos MUST be Male.
 //Draakans can be types A, B, or C.
 $Bot[0,"Sex"] = "Male";
 $Bot[0,"Skin"] = "Beagle"; //Skin. Don't use the actual team name. (Blood Eagle for example)
 $Bot[0,"Voice"] = "Male1"; //Voice. Don't use the fancy name (Psycho for example).
 $Bot[0,"VoicePitch"] = 1;
 $Bot[0,"Team"] = 1; //Human
 $Bot[0,"Weapons"] = "Blaster"; //List the weapons with spaces. The first weapon listed will be the one he has out on spawn.
 $Bot[0,"Pack"] = "MiningTool";
 $Bot[0,"Armor"] = "Light";
 $Bot[0,"Objectives"] = false; //The bot goes NOWHERE!

 //The ammo for each weapon in "Weapons", make sure it's in the same spot in the string as the weapon.
 //If the weapon doesn't use ammo, just place a zero.
 $Bot[0,"Ammo"] = "0";
 $Bot[0,"RepairKits"] = 1; //Yea.. rep kits
 $Bot[0,"Mines"] = 0; //Mines!!
 $Bot[0,"Grenades"] = 0; //Grenades. I'll make it so you can tell which kind of nades.
 $Bot[0,"Transform"] = "-159.193 -131.751 277.517 0 0 -1 1.1602"; //First 3 numbers are the position. The rest is the rotation.

 //These are for detailing on the bots.. shouldn't really be used unless you got a good reason to.
 $Bot[0,"Health"] = 1; //I'm pretty sure 1 is the max for all armors.

 $BotCount = 1; //Tell the game how many general AI's there are.
 }

};
