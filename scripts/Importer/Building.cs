//------------------------------------------------------------------------------
// A lil sumthin by Alviss
// For C&C

//------------------------------------------------------------------------------
//      Execution
//------------------------------------------------------------------------------

// This clears the building index's in one swoop
function BuildingManagerInit()
{
 if (isObject(BuildingManager))
   BuildingManager.Delete();

 new ScriptObject(BuildingManager) {};
}

//------------------------------------------------------------------------------

exec("scripts/Importer/BuildingDatablocks.cs");
exec("scripts/Importer/BuildingGroups.cs");
exec("scripts/Importer/Buildings/ConstructionYard.cs");
exec("scripts/Importer/Converter.cs");

// adding the buildings

//------------------------------------------------------------------------------
//      Functions
//------------------------------------------------------------------------------

function exebuild()
{
  exec("scripts/functionality/Building.cs");
}


function LoadCCBuilding(%client, %building, %center)
{
 %team = %client.team;

 // This is used to keep duplicate buildings from joining the same group
 // So, like, we don't have 3 group's named PowerPlant.
 // EDIT: I realize it wouldn't matter now, but i'm too lazy to remove all my work
 BuildingManager.BuildingInc[%building, %team]++;

 switch$(%building)
 {
    case "ConYard":
        Build_ConstructionYard(%client, %center, %team);

    case "PowerPlant":
        Build_PowerPlant(%client, %center, %team);

 }

 // set a timer to unify the building
 NametoId("MissionCleanup/Buildings"@%team@"/"@%building @ BuildingManager.BuildingInc[%building, %team]).Schedule(2000, "UnifyBuildings");
 // it uses a timer because none of the objects have the required parameters set yet, plus it's a good idea
 // to wait until all the pieces are loaded.
}

//------------------------------------------------------------------------------
//      CCMod Package
//------------------------------------------------------------------------------

package CCMod
{
  //------------------------------------------
  // echos and logs the exec call
  function exec(%target)
  {
//    LogEvent(%target);

    //error("Count : " @ DatablockGroup.getCount());

    parent::exec(%target);
  }

  //------------------------------------------
  // So you don't get a lame red spam wall in the console.
  function setTargetSensorGroup(%target, %team)
  {
    if (%target <= 0)
      return;

    parent::setTargetSensorGroup(%target, %team);
  }

  //------------------------------------------
  // So you don't get a lame red spam wall in the console.
  function setTargetName(%target, %name)
  {
    if (%target <= 0)
      return;

    parent::setTargetName(%target, %name);
  }
  //------------------------------------------
  // to capture the MCV when it deploys
  function StaticShapeData::OnAdd(%data, %obj)
  {
   // it's a building piece.
   if (StrStr(%data.GetName(), "BuildingBlock") != -1)
   {
     if (%obj.TheFloor && %obj.type $= "ConYard")
     {
       $CCGame::MCV[%obj.team] = %obj;

     }
   }

   Parent::OnAdd(%data, %obj);
  }

  //------------------------------------------
  // door functions
  function StaticShapeData::OnCollision(%data, %obj)
  {
   // DoorCode
   if (%obj.IsDoor)
   {
     %obj.ClosePosition = %obj.GetPosition();
     %obj.SetPosition("0 0 -100");

     schedule(2000, 0, "eval", %obj@".SetPosition("@%obj@".ClosePosition);");
   }
  }

  //------------------------------------------
  // captures incoming damage
  function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
  {
   // it's a building piece.
   if (StrStr(%data.GetName(), "BuildingBlock") != -1)
     GrabBaseDamageCall(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
   else
     parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
  }

  //------------------------------------------
  // captures incoming destroyed call
  function StaticShapeData::onDestroyed(%this,%obj,%prevState)
  {
     $BuildingCount[%obj.type, %obj.team]--;
     $Power[%obj.team] -= $Power[%obj.team];
     CheckPowerGoodLevel(%obj.team);

//   $TeamDeployedCount[%obj.team, AdvGuardTurretBasePack]--;
  }

};

if (!isActivePackage(CCMod))
  ActivatePackage(CCMod);

//------------------------------------------------------------------------------
//      Support Functions
//------------------------------------------------------------------------------

function addToBuildingGroup(%object)
{
   %TeamGroup = nameToID("Buildings"@%object.team);

   if (%TeamGroup <= 0)
   {
      %TeamGroup = new SimGroup("Buildings"@%object.team);
      MissionCleanup.add(%TeamGroup);
   }

   %index = BuildingManager.BuildingInc[%object.Type, %object.team];

   %BuildingsGroup = nameToID("Buildings"@%object.team@"/"@%object.Type @ %index);

   if ($CCTypeToName[%object.Type, %object.team] !$= %object.Type)
   {
     setTargetName(%object.target,addTaggedString($CCTypeToName[%object.Type, %object.team]));
   }

   if (%BuildingsGroup <= 0)
   {
      %BuildingsGroup = new SimGroup(%object.Type @ %index);
      %TeamGroup.add(%BuildingsGroup);
   }

   %BuildingsGroup.Index = %index;

   %BuildingsGroup.add(%object);
}

//------------------------------------------------------------------------------

function ShapeBase::SightObject(%player, %range)
{
  if (%range $= "")
    %range = 100;

  %pos = %player.getEyePoint();
  %vec = %player.getEyeVector();

  %targetpos = vectoradd(%pos, vectorscale(%vec, %range));
  %ray = containerraycast(%pos, %targetpos, $typemasks::StaticShapeObjectType, %player);

  return getword(%ray, 0);
}

//------------------------------------------------------------------------------

function ShapeBase::SightPos(%player, %range)
{
  if (%range $= "")
    %range = 100;

  %pos = %player.getEyePoint();
  %vec = %player.getEyeVector();

  %targetpos = vectoradd(%pos, vectorscale(%vec, %range));
  %ray = containerraycast(%pos, %targetpos, $typemasks::StaticShapeObjectType | $typemasks::TerrainObjectType, %player);

  return getwords(%ray, 1,3);
}

//------------------------------------------------------------------------------

function ShapeBase::getEyePoint(%player)
{
     %eyePt = getWords(%player.getEyeTransform(), 0, 2);
     return %eyePt;
}

//------------------------------------------------------------------------------

function SimObject::setPosition(%obj, %pos)
{
    %rot = getWords(%obj.getTransform(), 3, 6);
    %trans = %pos@" "@%rot;
    %obj.setTransform(%trans);
}

