//------------------------------------------------------------------------------
// Human Death Effects v3.0
// Effects By: LuCiD
//==============================================================================

// see player.cs for functions.
datablock AudioProfile(BloodSplashSound)
{
   filename    = "fx/armor/light_LF_water.wav";
   description = AudioClosest3d;
   preload = true;

};

//------------------------------------------------------------------------------
// Splash Mist
//==============================================================================
datablock ParticleData(HumanRedPlayerSplashMist)
{
   dragCoefficient      = 2.0;
   gravityCoefficient   = -0.05;
   inheritedVelFactor   = 0.0;
   constantAcceleration = 0.0;
   lifetimeMS           = 600;
   lifetimeVarianceMS   = 100;
   useInvAlpha          = false;
   spinRandomMin        = -90.0;
   spinRandomMax        = 500.0;
   textureName          = "particleTest";
   colors[0]     = "0.9 0.1 0.1 0.5";
   colors[1]     = "0.6 0.05 0.05 0.5";
   colors[2]     = "0.4 0.0 0.0 0.0";
   sizes[0]      = 0.5;
   sizes[1]      = 0.5;
   sizes[2]      = 0.8;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(HumanRedPlayerSplashMistEmitter)
{
   ejectionPeriodMS = 6;
   periodVarianceMS = 1;
   ejectionVelocity = 4.0;
   velocityVariance = 2.0;
   ejectionOffset   = 0.0;
   thetaMin         = 85;
   thetaMax         = 85;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   lifetimeMS       = 450;
   particles = "HumanRedPlayerSplashMist";
};

//------------------------------------------------------------------------------
// Human Red Pool
//==============================================================================
datablock ShockwaveData(RedBloodHit)
{
   width = 3.0;
   numSegments = 164;
   numVertSegments = 35;
   velocity = -1.5;
   acceleration = 2.0;
   lifetimeMS = 800;
   height = 0.1;
   verticalCurve = 0.5;

   mapToTerrain = false;
   renderBottom = true;
   orientToNormal = true;

   texture[0] = "special/shockwave4";
   texture[1] = "special/droplet";//"special/gradient";
   texWrap = 8.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0]     = "0.9 0.1 0.1 0.5";
   colors[1]     = "0.6 0.05 0.05 0.5";
   colors[2]     = "0.4 0.0 0.0 0.0";
};

//------------------------------------------------------------------------------
// Human Red Blood
//==============================================================================
datablock ParticleData(HumanRedBloodParticle)
{
   dragCoeffiecient     = 0.0;
   gravityCoefficient   = 120.0;
   inheritedVelFactor   = 0.0;

   lifetimeMS           = 1600;
   lifetimeVarianceMS   = 000;

   textureName          = "snowflake8x8";//"particletest";

   useInvAlpha = true;
   spinRandomMin = -30.0;
   spinRandomMax = 30.0;

   colors[0]     = "0.9 0.1 0.1 0.5";
   colors[1]     = "0.6 0.05 0.05 0.5";
   colors[2]     = "0.4 0.0 0.0 0.0";

   sizes[0]      = 0.2;
   sizes[1]      = 0.05;
   sizes[2]      = 0.06;

   times[0]      = 0.0;
   times[1]      = 0.2;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(HumanRedBloodEmitter)
{
   ejectionPeriodMS = 15;
   periodVarianceMS = 5;

   ejectionVelocity = 1.25;
   velocityVariance = 0.50;

   thetaMin         = 0.0;
   thetaMax         = 90.0;

   particles = "HumanRedBloodParticle";
};

//------------------------------------------------------------------------------
// Human Red Droplets Particle
//==============================================================================
datablock ParticleData( HumanRedDropletsParticle )
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.5;
   inheritedVelFactor   = 0.5;
   constantAcceleration = 0.1;
   lifetimeMS           = 300;
   lifetimeVarianceMS   = 100;
   textureName          = "special/droplet";
   colors[0]     = "0.9 0.1 0.1 1.0";
   colors[1]     = "0.6 0.05 0.05 1.0";
   colors[2]     = "0.4 0.0 0.0 0.0";
   sizes[0]      = 0.8;
   sizes[1]      = 0.3;
   sizes[2]      = 0.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( HumanRedDropletsEmitter )
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 0;
   ejectionVelocity = 2;
   velocityVariance = 1.0;
   ejectionOffset   = 0.0;
   thetaMin         = 60;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   particles = "HumanRedDropletsParticle";
};

//------------------------------------------------------------------------------
// Human Red Explosion
//==============================================================================
datablock ExplosionData(HumanRedExplosion)
{
   soundProfile   = BloodSplashSound;
   particleEmitter = HumanRedBloodEmitter;
   particleDensity = 250;
   particleRadius = 1.25;
   faceViewer = true;

   emitter[0] = HumanRedPlayerSplashMistEmitter;
   emitter[1] = HumanRedDropletsEmitter;
   shockwave = RedBloodHit;
};

datablock GrenadeProjectileData(HumanBlood)
{
   projectileShapeName = "turret_muzzlepoint.dts"; //Really small and hard to see
   emitterDelay        = -1;
   directDamage        = 0.0;
   hasDamageRadius     = false;
   indirectDamage      = 0.0;
   damageRadius        = 0.0;
   radiusDamageType    = $DamageType::Default;
   kickBackStrength    = 0;
   bubbleEmitTime      = 1.0;
   //sound               = BloodSplashSound;
   explosion = HumanRedExplosion;
   //explodeOnMaxBounce = true;
   velInheritFactor    = 0.5;
   baseEmitter[0]         = HumanRedBloodEmitter;

   grenadeElasticity = 0.4;
   grenadeFriction   = 0.2;
   armingDelayMS     = 100; // was 400
   muzzleVelocity    = 0;
   drag = 0.1;

};


