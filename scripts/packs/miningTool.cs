//--------------------------------------------------------------------------
// Mining Tool
// can be used by any armor type
// when activated, gives user a "mining tool" that can be used to
// mine rocks that contain various minerals that can be sold or
// used by miner to build weapons, armor, and other objects.
//
// Note: Although this could've been intergrated into the repair gun
// somehow (technically still repairs), it's been built as a seperate
// pack for "balancing".

//--------------------------------------------------------------------------
// Projectile

datablock RepairProjectileData(MiningBeam)
{
   sound = RepairPackFireSound;

   beamRange      = 10;
   beamWidth      = 0.33;
   numSegments    = 20;
   texRepeat      = 0.20;
   blurFreq       = 10.0;
   blurLifetime   = 1.0;
   cutoffAngle    = 25.0;

   textures[0]   = "special/ELFBeam";
   textures[1]   = "special/BlueImpact";

};


//-------------------------------------------------------------------------
// shapebase datablocks

datablock ShapeBaseImageData(MiningToolImage)
{
   shapeFile = "pack_upgrade_repair.dts";
   item = MiningTool;
   mountPoint = 1;
   offset = "0 0 0";
   emap = true;

   gun = MiningToolGunImage;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateSequence[1] = "fire";
   stateSound[1] = RepairPackActivateSound;
   stateTransitionOnTriggerUp[1] = "Deactivate";

   stateName[2] = "Deactivate";
   stateScript[2] = "onDeactivate";
   stateTransitionOnTimeout[2] = "Idle";   
};

datablock ItemData(MiningTool)
{
   className = Pack;
   catagory = "Packs";
   shapeFile = "pack_upgrade_repair.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
   image = "MiningToolImage";
   pickUpName = "a mining tool";

   lightOnlyStatic = true;
   lightType = "PulsingLight";
   lightColor = "0 0 1 0";
   lightTime = 1200;
   lightRadius = 2.5; //It's just a mining tool folks..

   computeCRC = true;
   emap = true;
};

//--------------------------------------------------------------------------
// Mining Gun

datablock ShapeBaseImageData(MiningToolGunImage)
{
   shapeFile = "weapon_repair.dts";
   offset = "0 0 0";

   usesEnergy = true;
   minEnergy = 3;
   cutOffEnergy = 3.1;
   emap = true;

   repairFactorPlayer = 0.002; // <--- attention DaveG!
   repairFactorObject = 0.004; // <--- attention DaveG!

   stateName[0] = "Activate";
   stateTransitionOnTimeout[0] = "ActivateReady";
   stateTimeoutValue[0] = 0.25;

   stateName[1] = "ActivateReady";
   stateScript[1] = "onActivateReady";
   stateSpinThread[1] = Stop;
   stateTransitionOnAmmo[1] = "Ready";
   stateTransitionOnNoAmmo[1] = "ActivateReady";

   stateName[2] = "Ready";
   stateSpinThread[2] = Stop;
   stateTransitionOnNoAmmo[2] = "Deactivate";
   stateTransitionOnTriggerDown[2] = "Validate";

   stateName[3] = "Validate";
   stateTransitionOnTimeout[3] = "Validate";
   stateTimeoutValue[3] = 0.2;
   stateEnergyDrain[3] = 3;
   stateSpinThread[3] = SpinUp;
   stateScript[3] = "onValidate";
   stateIgnoreLoadedForReady[3] = true;
   stateTransitionOnLoaded[3] = "Mine";
   stateTransitionOnNoAmmo[3] = "Deactivate";
   stateTransitionOnTriggerUp[3] = "Deactivate";

   stateName[4] = "Mine";
   stateSound[4] = RepairPackFireSound;
   stateScript[4] = "onMine";
   stateSpinThread[4] = FullSpeed;
   stateAllowImageChange[4] = false;
   stateSequence[4] = "activate";
   stateFire[4] = true;
   stateEnergyDrain[4] = 9;
   stateTimeoutValue[4] = 0.2;
   stateTransitionOnTimeOut[4] = "Mine";
   stateTransitionOnNoAmmo[4] = "Deactivate";
   stateTransitionOnTriggerUp[4] = "Deactivate";
   stateTransitionOnNotLoaded[4] = "Validate";

   stateName[5] = "Deactivate";
   stateScript[5] = "onDeactivate";
   stateSpinThread[5] = SpinDown;
   stateSequence[5] = "activate";
   stateDirection[5] = false;
   stateTimeoutValue[5] = 0.2;
   stateTransitionOnTimeout[5] = "ActivateReady";
};

function MiningToolImage::onUnmount(%data, %obj, %node)
{
   // dismount the mining gun if the player had it mounted
   // need the extra "if" statement to avoid a console error message
   if(%obj.getMountedImage($WeaponSlot))
      if(%obj.getMountedImage($WeaponSlot).getName() $= "MiningToolGunImage")
         %obj.unmountImage($WeaponSlot);
   // if the player was repairing something when the pack was thrown, stop repairing it
   if(%obj.repairing != 0)
      stopMining(%obj);
}

function MiningToolImage::onActivate(%data, %obj, %slot)
{
   // don't activate the pack if player is piloting a vehicle
   if(%obj.isPilot())
   {
      %obj.setImageTrigger(%slot, false);
      return;
   }

   if(!isObject(%obj.getMountedImage($WeaponSlot)) || %obj.getMountedImage($WeaponSlot).getName() !$= "MiningToolGunImage")
   {
      messageClient(%obj.client, 'MsgRepairPackOn', '\c2Mining Tool activated.');

      // make sure player's arm thread is "look"
      %obj.setArmThread(look);

      // mount the repair gun
      %obj.mountImage(MiningToolGunImage, $WeaponSlot);
      // clientCmdsetRepairReticle found in hud.cs
      commandToClient(%obj.client, 'setRepairReticle');
   }
}

function MiningToolImage::onDeactivate(%data, %obj, %slot)
{
   //called when the player hits the "pack" key again (toggle)
   %obj.setImageTrigger(%slot, false);
   // if mining gun was mounted, unmount it
   if(%obj.getMountedImage($WeaponSlot).getName() $= "MiningToolGunImage")
      %obj.unmountImage($WeaponSlot);
}

function MiningToolGunImage::onMount(%this,%obj,%slot)
{
   %obj.setImageAmmo(%slot,true);
   if ( !isDemo() )
      commandToClient( %obj.client, 'setRepairPackIconOn' );
}

function MiningToolGunImage::onUnmount(%this,%obj,%slot)
{
   // called when player switches to another weapon

   // stop mining whatever player was mining
   if(%obj.repairing)
      stopMining(%obj);

   %obj.setImageTrigger(%slot, false);
   // "turn off" the mining tool -- player needs to hit the "pack" key to
   // activate the mining gun again
   %obj.setImageTrigger($BackpackSlot, false);
   if ( !isDemo() )
      commandToClient( %obj.client, 'setRepairPackIconOff' );
}

function MiningToolGunImage::onActivateReady(%this,%obj,%slot)
{
   %obj.errMsgSent = false;
   %obj.selfRepairing = false;
   %obj.repairing = 0;
   %obj.setImageLoaded(%slot, false);
}

function MiningToolGunImage::onValidate(%this,%obj,%slot)
{
   // this = miningtoolimage datablock
   // obj = player wielding the repair gun
   // slot = weapon slot

   if(%obj.getEnergyLevel() <= %this.cutOffEnergy)
   {
      stopMining(%obj);
      return;
   }
   %repGun = %obj.getMountedImage(%slot);
   // muzVec is the vector coming from the mining gun's "muzzle"
   %muzVec = %obj.getMuzzleVector(%slot);
   // muzNVec = normalized muzVec
   %muzNVec = VectorNormalize(%muzVec);
   %repairRange = DefaultRepairBeam.beamRange;
   // scale muzNVec to the range the mining beam can reach
   %muzScaled = VectorScale(%muzNVec, %repairRange);
   // muzPoint = the actual point of the gun's "muzzle"
   %muzPoint = %obj.getMuzzlePoint(%slot);
   // rangeEnd = muzzle point + length of beam
   %rangeEnd = VectorAdd(%muzPoint, %muzScaled);
   // search for just about anything that can be damaged as well as interiors
   %searchMasks = $TypeMasks::StaticShapeObjectType;
   // search for objects within the beam's range that fit the masks above
   %scanTarg = ContainerRayCast(%muzPoint, %rangeEnd, %searchMasks, %obj);
   // screen out interiors
   if(%scanTarg && !(%scanTarg.getType() & $TypeMasks::InteriorObjectType))
   {
      // a target in range was found
      %repTgt = firstWord(%scanTarg);
      // is the prospective target damaged?
      if(%repTgt.notRepairable)
      {
         // this is an object that cant be mined at all
         // -- mission specific flag set on the object
         if(!%obj.errMsgSent)
         {
            messageClient(%obj.client, 'MsgRepairPackIrrepairable', '\c2Target cannot be mined.', %repTgt);
            %obj.errMsgSent = true;
         }
         // if player was repairing something, stop the repairs -- we're done
         if(%obj.repairing)
            stopMining(%obj);
      }
      else if(%repTgt.getDamageLevel())
      {
         // yes, it's damaged
         if(%repTgt != %obj.repairing)
         {
            if(isObject(%obj.repairing))
               stopMining(%obj);

            %obj.repairing = %repTgt;
         }
         // setting imageLoaded to true sends us to repair state (function onRepair)
         %obj.setImageLoaded(%slot, true);
      }
      else
      {
         // there is a target in range, but it's not damaged
         if(!%obj.errMsgSent)
         {
            // if the target isn't damaged, send a message to that effect only once
            messageClient(%obj.client, 'MsgRepairPackNotDamaged', '\c2Target is not damaged.', %repTgt);
            %obj.errMsgSent = true;
         }
         // if player was repairing something, stop the repairs -- we're done
         if(%obj.repairing)
            stopMining(%obj);
      }
   }

   //AI hack - too many things influence the aiming, so I'm going to force the repair object for bots only
   else if (%obj.client.isAIControlled() && isObject(%obj.client.repairObject))
   {
      %repTgt = %obj.client.repairObject;
      %repPoint = %repTgt.getAIRepairPoint();
      if (%repPoint $= "0 0 0")
         %repPoint = %repTgt.getWorldBoxCenter();
      %repTgtVector = VectorNormalize(VectorSub(%muzPoint, %repPoint));
      %aimVector = VectorNormalize(VectorSub(%muzPoint, %rangeEnd));

      //if the dot product is very close (ie. we're aiming in the right direction)
      if (VectorDot(%repTgtVector, %aimVector) > 0.85)
      {
         //do an LOS to make sure nothing is in the way...
         %scanTarg = ContainerRayCast(%muzPoint, %repPoint, %searchMasks, %obj);
         if (firstWord(%scanTarg) == %repTgt)
         {
            // yes, it's damaged

            if(isObject(%obj.repairing))
               stopMining(%obj);

            %obj.repairing = %repTgt;
            // setting imageLoaded to true sends us to repair state (function onRepair)
            %obj.setImageLoaded(%slot, true);
         }
      }
   }
   else if(%obj.getDamageLevel())
   {
      // there is no target in range, but the player is damaged
      // check to see if we were repairing something before -- if so, stop repairing old target
      if(%obj.repairing != 0)
         if(%obj.repairing != %obj)
            stopMining(%obj);
      if(isObject(%obj.repairing))
         stopMining(%obj);
      
      %obj.repairing = %obj;
      // quick, to onRepair!
      %obj.setImageLoaded(%slot, true);
   }
   else
   {
      // there is no target in range, and the player isn't damaged
      if(!%obj.errMsgSent)
      {
         // send an error message only once
         messageClient(%obj.client, 'MsgRepairPackNoTarget', '\c2No target to mine.');
         %obj.errMsgSent = true;
      }
      stopMining(%obj);
   }
}

function MiningToolGunImage::onMine(%this,%obj,%slot)
{
   // this = repairgunimage datablock
   // obj = player wielding the repair gun
   // slot = weapon slot

   if(%obj.getEnergyLevel() <= %this.cutOffEnergy)
   {
      stopMining(%obj);
      return;
   }
   // reset the flag that indicates an error message has been sent
   %obj.errMsgSent = false;
   %target = %obj.repairing;
   if(!%target)
   {
      // no target -- whoops! never mind
      stopMining(%obj);
   }

      else
      {
         // make sure we still have a target -- more vector fun!!!
         %muzVec      = %obj.getMuzzleVector(%slot);
         %muzNVec     = VectorNormalize(%muzVec);
         %repairRange = DefaultRepairBeam.beamRange;
         %muzScaled   = VectorScale(%muzNVec, %repairRange);
         %muzPoint    = %obj.getMuzzlePoint(%slot);
         %rangeEnd    = VectorAdd(%muzPoint, %muzScaled);

         %searchMasks = $TypeMasks::StaticShapeObjectType;

         //AI hack to help "fudge" the repairing stuff...
         if (%obj.client.isAIControlled() && isObject(%obj.client.repairObject) && %obj.client.repairObject == %obj.repairing)
         {
            %repTgt = %obj.client.repairObject;
            %repPoint = %repTgt.getAIRepairPoint();
            if (%repPoint $= "0 0 0")
               %repPoint = %repTgt.getWorldBoxCenter();
            %repTgtVector = VectorNormalize(VectorSub(%muzPoint, %repPoint));
            %aimVector = VectorNormalize(VectorSub(%muzPoint, %rangeEnd));
 
            //if the dot product is very close (ie. we're aiming in the right direction)
            if (VectorDot(%repTgtVector, %aimVector) > 0.85)
               %scanTarg = ContainerRayCast(%muzPoint, %repPoint, %searchMasks, %obj);
         }
         else
            %scanTarg = ContainerRayCast(%muzPoint, %rangeEnd, %searchMasks, %obj);

         if (%scanTarg)
         {
            %pos = getWords(%scanTarg, 1, 3);
            %obstructMask = $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
            %obstruction = ContainerRayCast(%muzPoint, %pos, %obstructMask, %obj);
            if (%obstruction)
               %scanTarg = "0";
         }

         if(%scanTarg)
         {
            // there's still a target out there
            %repTgt = firstWord(%scanTarg);
            
            if (%repTgt.mineral $= "")
            {
            messageClient(%obj.client, 'MsgMineFail',"\c2Rock is empty as of now. Check back later.");
            %obj.errMsgSent = true;
            stopMining(%obj);
            return;
            }

            // is the target damaged?
            if(%repTgt.getDamageLevel())
            {
            %obj.client.collected = %obj.client.collected + %obj.getRepairRate() + MiningToolGunImage.repairFactorPlayer;
            %obj.client.units[%obj.repairing.mineral] = %obj.client.units[%obj.repairing.mineral] + %obj.client.collected;
               if(%repTgt != %obj.repairing)
               {
                  // the target is not the same as the one we were just repairing
                  // stop repairing old target, start repairing new target
                  stopRepairing(%obj);
                  if(isObject(%obj.repairing))
                     stopRepairing(%obj);
 
                  %obj.repairing = %repTgt;
                  // extract the name of what player is repairing based on what it is
                  // if it's a player, it's the player's name (duh)
                  // if it's an object, look for a nametag
                  // if object has no nametag, just say what it is (e.g. generatorLarge)
                  if(%repTgt.getClassName() $= Player)
                     %tgtName = getTaggedString(%repTgt.client.name);
                  else if(%repTgt.getGameName() !$= "")
                     %tgtName = %repTgt.getGameName();
                  else
                     %tgtName = %repTgt.getDatablock().getName();
                  messageClient(%obj.client, 'MsgRepairPackRepairingObj', '\c2Mining rock.. (%3)', %tgtName, %repTgt, %repTgt.mineral);
                  startMining(%obj, false);
               }
               else
               {
                  // it's the same target as last time
                  // changed to fix "2 players can't repair same object" bug
                  if(%obj.repairProjectile == 0)
                  {
                     if(%repTgt.getClassName() $= Player)
                        %tgtName = getTaggedString(%repTgt.client.name);
                     else if(%repTgt.getGameName() !$= "")
                        %tgtName = %repTgt.getGameName();
                     else
                        %tgtName = %repTgt.getDatablock().getName();
                     messageClient(%obj.client, 'MsgRepairPackRepairingObj', '\c2Mining Rock.. (%3)', %tgtName, %repTgt, %repTgt.mineral);
                     startMining(%obj, false);
                  }
               }
            }
            else
            {
               %rateOfRepair = %this.repairFactorObject;
               if(%repTgt.getClassName() $= Player)
               {
                  %tgtName = getTaggedString(%repTgt.client.name);
                  %rateOfRepair = %this.repairFactorPlayer;
               }
               else if(%repTgt.getGameName() !$= "")
                  %tgtName = %repTgt.getGameName();
               else
                  %tgtName = %repTgt.getDatablock().getName();
               if(%repTgt != %obj.repairing)
               {
                  // it isn't the same object we were repairing previously
                  messageClient(%obj.client, 'MsgRepairPackNotDamaged', '\c2%1 is not damaged.', %tgtName);
               }
               else
               {
                  // same target, but not damaged -- we must be done
                  messageClient(%obj.client, 'MsgRepairPackDone', '\c2Rock ran out of minerals.');
                  stopMining(%obj); //Push this through to make sure client gets minerals..
                  Game.objectRepaired(%repTgt, %tgtName);
               }
               %obj.errMsgSent = true;
               stopMining(%obj);
            }
         }
         else
         {
            // whoops, we lost our target
            messageClient(%obj.client, 'MsgRepairPackLostTarget', '\c2No target to mine.');
            stopMining(%obj);
         }
      }
   }


function MiningToolGunImage::onDeactivate(%this,%obj,%slot)
{
   stopMining(%obj);
}

function stopMining(%player)
{
   // %player = the player who was using the repair pack

   if(%player.selfRepairing)
   {
      // there is no projectile for self-repairing
      %player.setRepairRate(%player.getRepairRate() - %player.repairingRate);
      %player.selfRepairing = false;
   }
   else if(%player.repairing > 0)
   {
      // player was repairing something else
      //if(%player.repairing.beingRepaired > 0)
      //{
         // don't decrement this stuff if it's already at 0 -- though it shouldn't be
         //%player.repairing.beingRepaired--;
         %player.repairing.setRepairRate(%player.repairing.getRepairRate() - %player.repairingRate);
      //}
      if(%player.repairProjectile > 0)
      {
         // is there a repair projectile? delete it
         %player.repairProjectile.delete();
         %player.repairProjectile = 0;
      }
   }
   
   if (!%player.errMsgSent && isObject(%player.repairing) && %player.repairing.mineral !$= "") //Ok, player stopped mining. Give the player his/her WTF they mined.
   {
   %client = %player.client;
   %mineral = %player.repairing.mineral;
   %collected = %player.client.collected;
   %total = %client.units[%mineral];
   %obj = %player.repairing;
   
   messageClient(%player.client,'msgMiningCollect','\c2Collected %1 units of %2. You now have %3 units of %2.',%collected,%mineral,%total);
   }
   
   %player.client.collected = 0;
   %player.repairing = 0;
   %player.repairingRate = 0;
   %player.setImageTrigger($WeaponSlot, false);
   %player.setImageLoaded($WeaponSlot, false);
}

function startMining(%player, %self)
{
   // %player = the player who was using the repair pack
   // %self = boolean -- is player repairing him/herself?

   if(%self)
   {
      // one repair, hold the projectile
      %player.setRepairRate(%player.getRepairRate() + MiningToolGunImage.repairFactorPlayer);
      %player.selfRepairing = true;
      %player.repairingRate = MiningToolGunImage.repairFactorPlayer;
   }
   else
   {
      //if(%player.repairing.beingRepaired $= "")
      //   %player.repairing.beingRepaired = 1;
      //else
      //   %player.repairing.beingRepaired++;

      //AI hack...
      if (%player.client.isAIControlled() && %player.client.repairObject == %player.repairing)
      {
         %initialPosition  = %player.getMuzzlePoint($WeaponSlot);
         %initialDirection = VectorSub(%initialPosition, %player.repairing.getWorldBoxCenter());
      }
      else
      {
         %initialDirection = %player.getMuzzleVector($WeaponSlot);
         %initialPosition  = %player.getMuzzlePoint($WeaponSlot);
      }
      if(%player.repairing.getClassName() $= Player)
         %repRate = RepairGunImage.repairFactorPlayer;
      else
         %repRate = RepairGunImage.repairFactorObject;
      %player.repairing.setRepairRate(%player.repairing.getRepairRate() + %repRate);
 
      %player.repairingRate = %repRate;
      %player.repairProjectile = new RepairProjectile() {
         dataBlock = MiningBeam;
         initialDirection = %initialDirection;
         initialPosition  = %initialPosition;
         sourceObject     = %player;
         sourceSlot       = $WeaponSlot;
         targetObject     = %player.repairing;
      };
      // ----------------------------------------------------
      // z0dd - ZOD, 5/27/02. Fix lingering projectile bug
      if(isObject(%player.lastProjectile))
         %player.lastProjectile.delete();
         
      %player.HP = %player.repairing.getDamageLevel(); //Is used when player stops 'mining'

      %player.lastProjectile = %player.repairProjectile;
      // End z0dd - ZOD
      // ----------------------------------------------------
      MissionCleanup.add(%player.repairProjectile);
   }
}

function MiningTool::onPickup(%this, %obj, %shape, %amount)
{
   // created to prevent console errors
}
