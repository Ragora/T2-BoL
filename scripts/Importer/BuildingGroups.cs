//------------------------------------------------------------------------------
// A lil sumthin by Alviss
// For C&C
//------------------------------------------------------------------------------

$CCBuildingHealth["ConYard"] = 750;
$CCBuildingHealth["PowerPlant"] = 250;
$CCBuildingHealth["AdvPowerPlant"] = 500;
$CCBuildingHealth["Barracks"] = 300;

//------------------------------------------------------------------------------
//      SimGroup Functions
//------------------------------------------------------------------------------

function SimGroup::UnifyBuildings(%group)
{
  // get the max health, have to remove the Index suffix
  %group.MaxHp = $CCBuildingHealth[ StrReplace(%group.getName(), %group.Index, "") ];

  // give all the pieces the nameplate of the building
  for (%i = 0; %i < %group.GetCount(); %i++)
  {
    %obj = %group.GetObject(%i);

    if (%obj.GetTarget() == -1)
    {
      // best function i've ever learned. :)
      %target = CreateTarget(%obj, "", "", "", "", %obj.team, "");
      %obj.SetTarget(%target);
    }

    %name = $CCTypeToName[%obj.Type, %obj.Team];

    setTargetName(%obj.getTarget(), AddTaggedString(%name));
  }

  // let all the pieces know the change.
  %group.SetHealth(%group.MaxHp);
}

//------------------------------------------------------------------------------

function SimGroup::SetHealth(%group, %Hp)
{
 %group.Hp = %hp;

 if (%hp <= 0)
 {
   %group.DestroyBuilding();
   return;
 }

 %percent = %hp / %group.MaxHp;

 echo(%percent);
 for (%i = 0; %i < %group.GetCount(); %i++)
 {
    %obj = %group.GetObject(%i);

    %max = %obj.getDatablock().maxDamage;

    %obj.SetDamageLevel( %max - (%percent * %max) );
 }

}

//------------------------------------------------------------------------------

function SimGroup::DestroyBuilding(%group)
{
 %type = StrReplace(%group.getName(), %group.Index, "");

 for (%i = 0; %i < %group.GetCount(); %i++)
 {
    %obj = %group.GetObject(%i);
    %team = %obj.team;

    // set the health to zero, so it explodes
    %obj.SetDamageLevel( 0 );
    schedule(1000, 0, Killit, %obj);
 }

 $BuildingCount[%type, %team]--;

 if (%type $= "ConYard")
 {
   Game.gameOver();
   CycleMissions();
 }

 if (%type $= "PowerPlant")
 {
   $TeamDeployedCount[%team, $CCTypeToItem[%type]]--;
   $Power[%team] -= 250;
   CheckPowerLowLevel(%team);
 }
 if (%type $= "AdvPowerPlant")
 {
   $TeamDeployedCount[%team, $CCTypeToItem[%type]]--;
   $Power[%team] -= 500;
   CheckPowerLowLevel(%team);
 }

}


//------------------------------------------------------------------------------
//      SimObject Functions
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// This function intercepts incoming damage calls and subtracts the dmg from the building MaxHp
function GrabBaseDamageCall(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
  if (!$TeamDamage)
  {
    if (isObject(%sourceObject))
    {
          //see if the object is being shot by a friendly
          if(%sourceObject.getDataBlock().catagory $= "Vehicles")
             %attackerTeam = getVehicleAttackerTeam(%sourceObject);
          else
             %attackerTeam = %sourceObject.team;
    }

    if (%attackerTeam == %targetObject.team)
      return;
  }

  %damageScale = %data.damageScale[%damageType];
  if (%damageScale !$= "")
    %amount *= %damageScale;

  if (%targetObject.getTarget() != -1)
  {
    %building = %targetObject.getGroup();

    if (isObject(%building))
    {
      %building.SetHealth(%building.HP - %amount);
    
    }
  }

}








