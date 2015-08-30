//Example for map Script
package Example //Must be our mission name for package to activate before the functions are called.
{
 function defineGeneralAI() //Called to load all general AI's for this map -- will be moved so it's stored in a BASIC file
 {
 //These arrays are for general bots
 $Bot[0,"Name"] = "A bot.";
 $Bot[0,"Race"] = "Human";
 
 //Very important or the console will get spammed.
 //Bioderms and Criollos MUST be Male.
 //Draakans can be types A, B, or C.
 $Bot[0,"Sex"] = "Male";
 $Bot[0,"Skin"] = "Beagle"; //Skin. Don't use the actual team name. (Blood Eagle for example)
 $Bot[0,"Voice"] = "Male1"; //Voice. Don't use the fancy name (Psycho for example).
 $Bot[0,"VoicePitch"] = 1;
 $Bot[0,"Team"] = 1; //Human
 $Bot[0,"Weapons"] = "Chaingun Disc Shocklance"; //List the weapons with spaces. The first weapon listed will be the one he has out on spawn.
 $Bot[0,"Pack"] = "AmmoPack";
 
 //The ammo for each weapon in "Weapons", make sure it's in the same spot in the string as the weapon.
 //If the weapon doesn't use ammo, just place a zero.
 $Bot[0,"Ammo"] = "200 30 0";
 $Bot[0,"RepairKits"] = 1; //Yea.. rep kits
 $Bot[0,"Mines"] = 1; //Mines!!
 $Bot[0,"Grenades"] = 3; //Grenades. I'll make it so you can tell which kind of nades.
 $Bot[0,"Transform"] = "0 0 0 1 0 0 0"; //First 3 numbers are the position. The rest is the rotation.
 
 //These are for detailing on the bots.. shouldn't really be used unless you got a good reason to.
 $Bot[0,"Health"] = 1; //I'm pretty sure 1 is the max for all armors.
 
 
 $BotCount = 1; //Tell the game how many general AI's there are.
 }
};
//You don't have to activate this package yourself, the game activates it just before loading AI's
