//--------------------------------------------------------------------------
// R700
// 
// 
//--------------------------------------------------------------------------

datablock AudioProfile(R700SwitchSound)
{
   filename    = "fx/weapons/sniper_activate.wav";
   description = AudioClosest3d;
   preload = true;
   effect = SniperRifleSwitchEffect;
};

datablock AudioProfile(R700RifleFireSound)
{
   filename    = "fx/weapons/sniper_fire.wav";
   description = AudioClose3d;
   preload = true;
   effect = SniperRifleFireEffect;
};

datablock AudioProfile(R700FireWetSound)
{
   filename    = "fx/weapons/sniper_underwater.wav";
   description = AudioClose3d;
   preload = true;
   effect = SniperRifleFireWetEffect;
};

datablock AudioProfile(R700RifleDryFireSound)
{
   filename    = "fx/weapons/chaingun_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(R700RifleProjectileSound)
{
   filename    = "fx/weapons/sniper_miss.wav";
   description = AudioClose3d;
   preload = true;
};

//--------------------------------------
// Projectile
//--------------------------------------
datablock TracerProjectileData(R700Bullet)
{
   doDynamicClientHits = true;

   directDamage        = 0.0842; // z0dd - ZOD, 9-27-02. Was 0.0825
   directDamageType    = $DamageType::Bullet;
   explosion           = "ChaingunExplosion";
   splash              = ChaingunSplash;

   kickBackStrength  = 0.0;
   sound 				= ChaingunProjectile;

   //dryVelocity       = 425.0;
   dryVelocity       = 700.0; // z0dd - ZOD, 8-12-02. Was 425.0
   wetVelocity       = 100.0;
   velInheritFactor  = 1.0;
   fizzleTimeMS      = 3000;
   lifetimeMS        = 3000;
   explodeOnDeath    = false;
   reflectOnWaterImpactAngle = 0.0;
   explodeOnWaterImpact      = false;
   deflectionOnWaterImpact   = 0.0;
   fizzleUnderwaterMS        = 3000;

   tracerLength    = 15.0;
   tracerAlpha     = false;
   tracerMinPixels = 6;
   tracerColor     = 211.0/255.0 @ " " @ 215.0/255.0 @ " " @ 120.0/255.0 @ " 0.75";
	tracerTex[0]  	 = "special/tracer00";
	tracerTex[1]  	 = "special/tracercross";
	tracerWidth     = 0.10;
   crossSize       = 0.20;
   crossViewAng    = 0.990;
   renderCross     = true;

   decalData[0] = ChaingunDecal1;
   decalData[1] = ChaingunDecal2;
   decalData[2] = ChaingunDecal3;
   decalData[3] = ChaingunDecal4;
   decalData[4] = ChaingunDecal5;
   decalData[5] = ChaingunDecal6;
};



//--------------------------------------
// Rifle and item...
//--------------------------------------
datablock ItemData(R700)
{
   className    = Weapon;
   catagory     = "Spawn Items";
   shapeFile    = "weapon_sniper.dts";
   image        = R700Image;
   mass         = 1;
   elasticity   = 0.2;
   friction     = 0.6;
   pickupRadius = 2;
	pickUpName = "an R700";

   computeCRC = true;

};

datablock ShapeBaseImageData(R700Image)
{
	className = WeaponImage;
   shapeFile = "weapon_sniper.dts";
   item = R700;
   projectile = R700Bullet;
   projectileType = TracerProjectile;
	armThread = looksn;

	usesEnergy = false;
	minEnergy = 0;

   stateName[0]                     = "Activate";
   stateTransitionOnTimeout[0]      = "ActivateReady";
   stateSound[0]                    = SniperRifleSwitchSound;
   stateTimeoutValue[0]             = 0.5;
   stateSequence[0]                 = "Activate";

   stateName[1]                     = "ActivateReady";
   stateTransitionOnLoaded[1]       = "Ready";
   stateTransitionOnNoAmmo[1]       = "NoAmmo";

   stateName[2]                     = "Ready";
   stateTransitionOnNoAmmo[2]       = "NoAmmo";
   stateTransitionOnTriggerDown[2]  = "CheckWet";

   stateName[3]                     = "Fire";
   stateTransitionOnTimeout[3]      = "Reload";
   stateTimeoutValue[3]             = 0.5;
   stateFire[3]                     = true;
   stateAllowImageChange[3]         = false;
   stateSequence[3]                 = "Fire";
   stateScript[3]                   = "onFire";

   stateName[4]                     = "Reload";
   stateTransitionOnTimeout[4]      = "Ready";
   stateTimeoutValue[4]             = 0.5;
   stateAllowImageChange[4]         = false;

   stateName[5]                     = "CheckWet";
   stateTransitionOnWet[5]          = "DryFire";
   stateTransitionOnNotWet[5]       = "Fire";
   
   stateName[6]                     = "NoAmmo";
   stateTransitionOnAmmo[6]         = "Reload";
   stateTransitionOnTriggerDown[6]  = "DryFire";
   stateSequence[6]                 = "NoAmmo";
   
   stateName[7]                     = "DryFire";
   stateSound[7]                    = SniperRifleDryFireSound;
   stateTimeoutValue[7]             = 0.5;
   stateTransitionOnTimeout[7]      = "Ready";
};

