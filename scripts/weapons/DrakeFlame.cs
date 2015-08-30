 //--------------------------------------
// Draakan Flame "Breath" - Is CCM's flameThrower edited to suit the mod
//--------------------------------------

//color tent for EVERYTHING its red really red 
//1 r
//0.18 g
//0.03 b

//--------------------------------------
// Trail
//--------------------------------------

datablock ParticleData(flameParticle)
{
   dragCoeffiecient     = 0.0;
   gravityCoefficient   = -0.1;
   inheritedVelFactor   = 0.1;

   lifetimeMS           = 500;
   lifetimeVarianceMS   = 50;

   textureName          = "ParticleTest";

   spinRandomMin = -10.0;
   spinRandomMax = 10.0;

   colors[0]     = "1 0.18 0.03 0.4";
   colors[1]     = "1 0.18 0.03 0.3";
   colors[2]     = "1 0.18 0.03 0.0";
   sizes[0]      = 2.0;
   sizes[1]      = 1.0;
   sizes[2]      = 0.8;
   times[0]      = 0.0;
   times[1]      = 0.6;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(flameEmitter)
{
   ejectionPeriodMS = 3;
   periodVarianceMS = 0;

   ejectionOffset = 0.2;
   ejectionVelocity = 10.0;
   velocityVariance = 0.0;

   thetaMin         = 0.0;
   thetaMax         = 10.0;  

   particles = "flameParticle";
};

//--------------------------------------------------------------------------
// Sounds
//--------------------------------------
//This will eventually be some "choking" sound
//datablock AudioProfile(FlamerDryFireSound)
//{
//   filename    = "fx/weapons/plasma_dryfire.wav";
//   description = AudioClose3d;
//   preload = true;
//   effect = PlasmaDryFireEffect;
//};

//--------------------------------------------------------------------------
// Explosion
//--------------------------------------------------------------------------
datablock ParticleData(flameExplosionParticle)
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.2;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 0;
   textureName          = "ParticleTest";
   colors[0]     = "1 0.18 0.03 0.6";
   colors[1]     = "1 0.18 0.03 0.0";
   sizes[0]      = 2;
   sizes[1]      = 2.5;
};

datablock ParticleEmitterData(flameExplosionEmitter)
{
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionOffset = 2.0;
   ejectionVelocity = 4.0;
   velocityVariance = 0.0;
   thetaMin         = 60.0;
   thetaMax         = 90.0;
   lifetimeMS       = 250;

   particles = "flameExplosionParticle";
};

datablock ExplosionData(flameBoltExplosion)
{
   particleEmitter = flameExplosionEmitter;
   particleDensity = 150;
   particleRadius = 1.25;
   faceViewer = true;
};

//--------------------------------------------------------------------------
// Projectiles
//--------------------------------------------------------------------------
datablock LinearFlareProjectileData(FlameboltMain)
{
   projectileShapeName = "turret_muzzlepoint.dts";
   scale               = "1.0 1.0 1.0";
   faceViewer          = true;
   directDamage        = 0.1;
   hasDamageRadius     = true;
   indirectDamage      = 0.08;
   damageRadius        = 4.0;
   kickBackStrength    = 0.0;
   radiusDamageType    = $DamageType::Flame;
   directDamageType    = $DamageType::Flame;

   explosion           = "flameBoltExplosion";
   splash              = PlasmaSplash;

   baseEmitter        = FlameEmitter;

   dryVelocity       = 50.0; // z0dd - ZOD, 7/20/02. Faster plasma projectile. was 55
   wetVelocity       = -1;
   velInheritFactor  = 0.3;
   fizzleTimeMS      = 250;
   lifetimeMS        = 2000;
   explodeOnDeath    = false;
   reflectOnWaterImpactAngle = 0.0;
   explodeOnWaterImpact      = true;
   deflectionOnWaterImpact   = 0.0;
   fizzleUnderwaterMS        = -1;

   //activateDelayMS = 100;
   activateDelayMS = -1;

   size[0]           = 0.2;
   size[1]           = 0.5;
   size[2]           = 0.1;


   numFlares         = 35;
   flareColor        = "1 0.18 0.03";
   flareModTexture   = "flaremod";
   flareBaseTexture  = "flarebase";

   sound        = PlasmaProjectileSound;
   fireSound    = PlasmaFireSound;
   wetFireSound = PlasmaFireWetSound;
   
   hasLight    = true;
   lightRadius = 10.0;
   lightColor  = "0.94 0.03 0.12";
};

//--------------------------------------------------------------------------
// Weapon
//--------------------------------------------------------------------------
datablock ItemData(flamer)
{
   className = Weapon;
   catagory = "Spawn Items";
   shapeFile = "weapon_plasma.dts";
   image = flamerImage;
   mass = 1.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   pickUpName = "you should not see this"; //Players should never be able to pick this up..
};

datablock ShapeBaseImageData(flamerImage)
{
   className = WeaponImage;
   shapeFile = "turret_muzzlepoint.dts";
   mass = 10;
   item = flamer;
   offset = "-0.45 1 0.5"; // L/R - F/B - T/B
   rotation = "0 1 0 180"; // L/R - F/B - T/B
  // armThread = looknw; -- For some odd reason, this adds a little tiny white triangle next to the reticle
   
   UsesEnergy = true;
   MinEnergy = 1; //At least one point of energy to fire
   fireEnergy = 5.5; //The Draakan Flame uses a fuck load of energy to keep the armor & body cool
   HasTimeout = true;
   TimeoutMS = 6000;
   
   projectileSpread = 7.0 / 1000.0;

   projectile = flameBoltmain;
   projectileType = LinearFlareProjectile;

   stateName[0] = "Activate";
   stateTransitionOnTimeout[0] = "ActivateReady";
   stateTimeoutValue[0] = 0.5;
   stateSequence[0] = "Activate";
   stateSound[0] = PlasmaSwitchSound;

   stateName[1] = "ActivateReady";
   stateTransitionOnLoaded[1] = "Ready";
   stateTransitionOnNoAmmo[1] = "NoAmmo";

   stateName[2] = "Ready";
   stateEmitterTime[2] = 10000;
   stateTransitionOnNoAmmo[2] = "NoAmmo";
   stateTransitionOnTriggerDown[2] = "CheckWet"; //Drakes can't use flames underwater. :)

   stateName[3] = "Fire";
   stateTransitionOnTimeout[3] = "Reload";
   stateTimeoutValue[3] = 0.05;
   stateFire[3] = true;
   stateAllowImageChange[3] = false;
   stateScript[3] = "onFire";
   stateEmitterTime[3] = 0.05;

   stateName[4] = "Reload";
   stateTransitionOnNoAmmo[4] = "NoAmmo";
   stateTransitionOnTimeout[4] = "Ready";
   stateTimeoutValue[4] = 0.05;
   stateAllowImageChange[4] = false;
   stateSequence[4] = "Reload";
   stateSound[4] = PlasmaReloadSound;

   stateName[5] = "NoAmmo";
   stateTransitionOnAmmo[5] = "Reload";
   stateSequence[5] = "NoAmmo";
   stateTransitionOnTriggerDown[5] = "DryFire";

   stateName[6]       = "DryFire";
   stateSound[6]      = PlasmaDryFireSound; //I should make this some 'choking' sound
   stateTimeoutValue[6]        = 1.0;
   stateTransitionOnTimeout[6] = "NoAmmo";
   
   stateName[7]       = "WetFire";
   stateSound[7]      = PlasmaFireWetSound;
   stateTimeoutValue[7]        = 1.0;
   stateTransitionOnTimeout[7] = "Ready";
   
   stateName[8]               = "CheckWet";
   stateTransitionOnWet[8]    = "WetFire";
   stateTransitionOnNotWet[8] = "Fire"; 
};

//--------------------------------------------------------------------------
// Bound Functions
//--------------------------------------------------------------------------
function flamerImage::OnFire(%this,%obj,%slot)
{
%p = parent::onfire(%this,%obj,%slot);
}

function flamerImage::onMount(%this,%obj,%slot)
{ 
if (%obj.client.race !$= "Draakan") //If we're not a Drake
%obj.setInventory("Flamer",0,true);
else
Parent::onMount(%this,%obj,%slot);
}

function flamerImage::onUnmount(%this,%obj,%slot)
{
Parent::onUnmount(%this,%obj,%slot);
}

//--------------------------------------------------------------------------
// Functions
//--------------------------------------------------------------------------
function burnloop(%obj,%sourceObj, %DMG, %DmgTpe) //TODO: Organize this entire weapon file..
{
cancel(%obj.burnloop);

if(!isobject(%obj) || %obj.client.race $= "draakan" || %obj.getclassname() !$= "player" || !%obj.shouldburn)
{
%obj.isburning = false;
%obj.client.shouldscream = false; //Only applies to AI clients.
return;
}

if (%obj.client.shouldscream $="" && %obj.client.isAIControlled()) //Used for bots..
%obj.client.shouldscream = true;

if (%obj.client.shouldscream)
{

%rnd = getrandom(0,1);
switch(%rnd)
{
case 0: schedule(250, %obj.client, "AIPlayAnimSound", %obj.client, "0 0 0", "vqk.help", $AIAnimWave, $AIAnimWave, 0);
case 1: schedule(250, %obj.client, "AIPlayAnimSound", %obj.client, "0 0 0", "gbl.shazbot", $AIAnimWave, $AIAnimWave, 0);
}
%obj.client.shouldscream = false;
}

%obj.damage(%sourceObj, %obj.getposition(), %DMG, %DmgTpe);

if (%obj.getstate() $="dead")
%fire = new ParticleEmissionDummy(){
position = vectoradd(%obj.getPosition(),"0 0 0.2");
dataBlock = "defaultEmissionDummy";
emitter = "FlameEmitter";
};
else
%fire = new ParticleEmissionDummy(){
position = vectoradd(%obj.getPosition(),"0 0 0.5");
dataBlock = "defaultEmissionDummy";
emitter = "FlameEmitter";
};


if (IsObject(%obj.client) && %obj.client.isaicontrolled())
%obj.client.setblinded(1);

if (%obj.burntime $="")
%obj.burntime = 0;

%time = %obj.burntime * 1000;

if (%time == $Host::BurnTime || %time > $Host::BurnTime)
{
%obj.burntime = 0;
Cancel(%obj.burnloop);

if (IsObject(%fire))
%fire.delete();
return;
}
else
{
%obj.burntime++;
}

MissionCleanup.add(%fire);
%fire.schedule(100, "delete");
%obj.burnloop = schedule(100, 0, "burnloop", %obj, %sourceObj.client, 0.007, %DmgTpe);
%obj.isburning = true;
}


function flamer::onadd(%this,%obj) //Move the object to 0 0 0 as soon as it's created - .delete likes to crash.
{
schedule(1,0,"delete",%obj);
}

function settransform(%obj,%trans)
{
%obj.settransform(%trans);
}

function delete(%obj)
{
%obj.delete();
}
