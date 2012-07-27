$ModVersionText = $ModVersion SPC "BETA"; //Shown in the Enyclopedia.

//------------------------------------------------------------------------------
// Shared Functions
//==============================================================================

//------------------------------------------------------------------------------
// Checks to see if the race is valid.
function isValidRace(%race)
{
 switch$(%race)
 {
  case "Human" or "Bioderm" or "Draakan" or "Criollos": return true;
  default: return false;
 }
return false;
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Returns the "team" of each race. If the race doesn't exist, -1 is returned.
function getRaceTeam(%race)
{
 switch$(%race)
 {
  case "Bioderm" or "Bioderms" or "Derm" or "Derms": return 2;
  case "Criollos" or "Creole" or "Creoles": return 4;
  case "Draakan" or "Drake" or "Draakans" or "Drakes": return 3;
  case "Human" or "Humans" or "Man": return 1;
  default: return -1;
 }
return -1;
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Checks if the gender is valid for specified 'race'
function isValidSex(%race,%sex)
{
 switch$(%race)
 {
  case "Human": if (%sex $= "Male" || %sex $= "Female") return true;
  case "Bioderm": if (%sex $= "Male") return true;
  case "Draakan": if (%sex $= "A" || %sex $= "B") return true;
  case "Criollos": if (%sex $= "Male") return true;
  default: return false;
 }
return false;
}
//------------------------------------------------------------------------------

function loadCharacter(%file) //BETA Function
{
if (!IsFile(%file))
return false;

%name = getBlockData(%file,"Character",1,"Name");
%race = getBlockData(%file,"Character",1,"Race");
%sex = getBlockData(%file,"Character",1,"Sex");

if (%name $= "" || !IsValidRace(%race) || !IsValidSex(%race,%sex) || $Game::LoadedFile[%file] || $Game::LoadedName[%name])
{
error("Unable to load character file '"@%file@"'.");
return false;
}

$Game::CharacterName[$CharacterCount] = %name;
$Game::Race[$CharacterCount] = %Race;
$Game::Sex[$CharacterCount] = %Sex;
$Game::Skin[$CharacterCount] = getBlockData(%file,"Character",1,"Skin");
$Game::Voice[$CharacterCount] = getBlockData(%file,"Character",1,"Voice");
$Game::voicePitch[$CharacterCount] = getBlockData(%file,"Character",1,"VoicePitch");

$Game::Chat[$CharacterCount,"Death"] = getArrayData(%file,"Death",0);
$Game::Chat[$CharacterCount,"Kill"] = getArrayData(%file,"Kill",0);
$Game::Chat[$CharacterCount,"Defend"] = getArrayData(%file,"Defend",0);
$Game::Chat[$CharacterCount,"Defended"] = getArrayData(%file,"Defended",0);
$Game::Chat[$CharacterCount,"Healed"] = getArrayData(%file,"Healed",0);
$Game::Chat[$CharacterCount,"Idiocy"] = getArrayData(%file,"Idiocy",0);
$Game::FileToID[%file] = $CharacterCount;
$Game::LoadedName[%name] = true;
$Game::LoadedFile[%file] = true;

//Ok -- now we need to grab the chat arrays for this character.
$CharacterCount++;
logEcho("Successfully loaded character file '"@%file@"'.");
return true;
}

//------------------------------------------------------------------------------
// Shared Executes
//==============================================================================
exec("scripts/basicFileProcessor.cs");
exec("scripts/fileProcessing.cs");
exec("scripts/stringProcessing.cs");
exec("scripts/numbericProcessing.cs");
