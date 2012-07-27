//--------------------------------------------------------------------------
// C4
// Used only in Search & Destroy gameMode
// When deployed (must be deploying on an objective) a timer is started
// When the timer runs out, objective goes boom and offence wins
// But the bomb can be defused by defence.

//--------------------------------------------------------------------------

//--------------------------------------------------------------------------
// Projectile

//-------------------------------------------------------------------------
// shapebase datablocks
datablock ShapeBaseImageData(C4Image)
{
   shapeFile = "pack_upgrade_satchel.dts";
   item = C4Charge;
   mountPoint = 1;
   offset = "0 0 0";
   emap = true;
};

datablock ItemData(C4Charge)
{
   className = Pack;
   catagory = "Packs";
   image = C4Image;
   shapeFile = "pack_upgrade_satchel.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
   pickUpName = "a C4";

   computeCRC = true;
};

datablock StaticShapeData(C4Deployed) : StaticShapeDamageProfile
{
   shapeFile = "pack_upgrade_satchel.dts";
   explosion = SatchelMainExplosion;
   underwaterExplosion = UnderwaterSatchelMainExplosion;
   armDelay = 2500;
   maxDamage = 0.6;

   disabledLevel = 0.5;
   destroyedLevel = 0.6;
   dynamicType = $TypeMasks::StaticShapeObjectType;
   renderWhenDestroyed = false;

   kickBackStrength    = 4000;
};

//--------------------------------------------------------------------------

function C4::onUse(%this, %obj)
{
   %item = new Item() {
      dataBlock = SatchelChargeTossed;
      rotation = "0 0 1 " @ (getRandom() * 360);
   };
   MissionCleanup.add(%item);
   // take pack out of inventory and unmount image
   %obj.decInventory(SatchelCharge, 1);
   %obj.throwObject(%item);
   %item.sourceObject = %obj;

   // z0dd - ZOD, 5/16/02. Schedule a check to see if the satchel is at rest but not stuck to anything
   %item.checkCount = 0;
   %item.velocCheck = %item.getDataBlock().schedule(1000, "checkVelocity", %item);
}

function initArmSatchelCharge(%satchel)
{
   // "deet deet deet" sound
   %satchel.playAudio(1, SatchelChargeActivateSound);
   // also need to play "antenna extending" animation
   %satchel.playThread(0, "deploy");
   %satchel.playThread(1, "activate");

   // delay the actual arming until after sound is done playing
   schedule( 2200, 0, "armSatchelCharge", %satchel );
}

function armSatchelCharge(%satchel)
{
   %satchel.armed = true;
   commandToClient( %satchel.sourceObject.client, 'setSatchelArmed' );
}

function detonateSatchelCharge(%player)
{
   %satchelCharge = %player.thrownChargeId;
   // can't detonate the satchel charge if it isn't armed
   if(!%satchelCharge.armed)
      return;

   //error("Detonating satchel charge #" @ %satchelCharge @ " for player #" @ %player);

   if(%satchelCharge.getDamageState() !$= Destroyed)
   {
      %satchelCharge.setDamageState(Destroyed);
      %satchelCharge.blowup(); 
   }
   
   // Clear the player's HUD:
   %player.client.clearBackpackIcon();   
}

function SatchelChargeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
{
   // lava types
   if(%type >=4 && %type <= 6)
   {
      if(%obj.getDamageState() !$= "Destroyed")
      {
         %obj.armed = true;
         detonateSatchelCharge(%obj.sourceObject);
         return;
      }
   }
   
   // quickSand   
   if(%type == 7)
      if(isObject(%obj.sourceObject))
         %obj.sourceObject.thrownChargeId = 0;
      
  Parent::onEnterLiquid(%data, %obj, %coverage, %type);
}

function SatchelChargeImage::onMount(%data, %obj, %node)
{
   %obj.thrownChargeId = 0;
}

function SatchelChargeImage::onUnmount(%data, %obj, %node)
{
}

function SatchelChargeThrown::onDestroyed(%this, %object, %lastState)
{
   if(%object.kaboom)
      return;
   else
   {
      %object.kaboom = true;

      // the "thwart" flag is set if the charge is destroyed with weapons rather
      // than detonated. A less damaging explosion, but visually the same scale.
      if(%object.thwart)
      {
         messageClient(%object.sourceObject.client, 'msgSatchelChargeDetonate', "\c2Satchel charge destroyed.");
         %dmgRadius = 15; // z0dd - ZOD, 9/27/02. Was 10
         %dmgMod = 0.35; // z0dd - ZOD, 9/27/02. Was 0.3
         %expImpulse = 2000; // z0dd - ZOD, 9/27/02. Was 1000
         %dmgType = $DamageType::Explosion;
      }
      else
      {
         messageClient(%object.sourceObject.client, 'msgSatchelChargeDetonate', "\c2Satchel charge detonated!");
         %dmgRadius = 25; // z0dd - ZOD, 9/27/02. Was 20
         %dmgMod = 1.15; // z0dd - ZOD, 9/27/02. Was 1.0
         %expImpulse = 5000; // z0dd - ZOD, 9/27/02. Was 2500
         %dmgType = $DamageType::SatchelCharge;
      }

      %object.blowingUp = true;
      RadiusExplosion(%object, %object.getPosition(), %dmgRadius, %dmgMod, %expImpulse, %object.sourceObject, %dmgType);

      %object.schedule(1000, "delete");
   }

   // --------------------------------------------------------------------------------
   // z0dd - ZOD, 4/25/02. Satchel bug fix. Prior to fix, clients couldn't pick up 
   // packs when satchel was destroyed from dmg
   if(isObject(%object.sourceObject))
      %object.sourceObject.thrownChargeId = 0;
   // --------------------------------------------------------------------------------
}

function SatchelChargeThrown::onCollision(%data,%obj,%col)
{
   // Do nothing...
}

function SatchelChargeThrown::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   if (!%object.blowingUp)
   {
      %targetObject.damaged += %amount;

      if(%targetObject.damaged >= %targetObject.getDataBlock().maxDamage && 
         %targetObject.getDamageState() !$= Destroyed)
      {   
         %targetObject.thwart = true;
         %targetObject.setDamageState(Destroyed);
         %targetObject.blowup(); 
         
         // clear the player's HUD:
         %targetObject.sourceObject.client.clearBackPackIcon();
      }
   }
}

function SatchelCharge::onPickup(%this, %obj, %shape, %amount)
{
   // created to prevent console errors
}

//**************************************************************
// STICKY SATCHEL FUNCTIONS: z0dd - ZOD, 5/16/02
//**************************************************************

function SatchelChargeTossed::onEnterLiquid(%data, %obj, %coverage, %type)
{
   // If it lands in lava or quicksand, delete it
   if(%type >=4 && %type <= 7)
   {
      cancel(%obj.velocCheck);
      if(isObject(%obj.sourceObject))
         %obj.sourceObject.thrownChargeId = 0;

      %obj.sourceObject.client.clearBackPackIcon();
      %obj.schedule(100, "delete");
   }
   else
      cancel(%obj.velocCheck);
}

function SatchelChargeTossed::onLeaveLiquid(%data, %obj, %type)
{
   // On the off chance it passes through to air, reschedule the velocity check
   %obj.checkCount = 0;
   %obj.velocCheck = %obj.getDataBlock().schedule(1000, "checkVelocity", %obj);
}

function SatchelChargeTossed::onCollision(%data, %obj, %col)
{
   // Lets keep thing from floating mid air, the check velocity should handle it afterwards
   if(%col.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::TurretObjectType))
   {
      %vec = (-1.0 + getRandom() * 2.0) SPC (-1.0 + getRandom() * 2.0) SPC getRandom();
      %vec = vectorScale(%vec, 15);
      %pos = %col.getWorldBoxCenter();
      %obj.applyImpulse(%pos, %vec);
   }
}

function SatchelChargeTossed::checkVelocity(%data, %item)
{
   %item.checkCount++;
   if(VectorLen(%item.getVelocity()) < 0.1)
   {
      // Satchel has come to rest but not activated, probably on a 
      // staticshape (station, gen, etc) lets force activation
      cancel(%item.velocCheck);
      activateSatchel(posFromTransform(%item.getTransform()), rotFromTransform(%item.getTransform()), %item.sourceObject);
      %item.schedule(100, "delete");
   }
   else if(%item.checkCount >= 6)
   {
      // satchel's still moving but it's been checked several times,
      // probably thrown off face of earth, delete it
      cancel(%item.velocCheck);
      if(isObject(%item.sourceObject))
         %item.sourceObject.thrownChargeId = 0;

      %item.sourceObject.client.clearBackPackIcon();
      %item.schedule(100, "delete");
   }
   else
   {
      // check back in a little while
      %item.velocCheck = %data.schedule(1000, "checkVelocity", %item);
   }
}

function SatchelChargeTossed::onStickyCollision(%data, %obj)
{
   // We have sticky! Lets setup for the actual charge
   cancel(%obj.velocCheck);
   %pos = %obj.getLastStickyPos();
   %norm = %obj.getLastStickyNormal();
   %intAngle = getTerrainAngle(%norm);
   %rotAxis = vectorNormalize(vectorCross(%norm, "0 0 1"));
   if (getWord(%norm, 2) == 1 || getWord(%norm, 2) == -1)
      %rotAxis = vectorNormalize(vectorCross(%norm, "0 1 0"));

   %rot = %rotAxis @ " " @ %intAngle;
   activateSatchel(%pos, %rot, %obj.sourceObject);
   %obj.schedule(50, "delete");
}

function activateSatchel(%pos, %rot, %source)
{
  // Create the charge and schedule arming
  %satchel = new StaticShape() {
      dataBlock = SatchelChargeThrown;
      sourceObject = %source;
      position = %pos;
      rotation = %rot;
   };
   MissionCleanup.add(%satchel);
   %source.thrownChargeId = %satchel;
   %satchel.armed = false;
   %satchel.damaged = 0.0;
   %satchel.thwart = false;

   // arm itself 2.5 seconds after creation
   schedule(%satchel.getDatablock().armDelay, %satchel, "initArmSatchelCharge", %satchel);
   messageClient(%source.client, 'MsgSatchelChargePlaced', "\c2Satchel charge deployed.");
}
