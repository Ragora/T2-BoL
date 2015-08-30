function LaunchCredits(%val)
{
   canvas.showBOLCredits = %val;
   Canvas.setContent(CreditsGui);
}

function cancelCredits()
{
   //delete the action map
   CreditsActionMap.pop();

   //kill the schedules
   cancel($CreditsScrollSchedule);
   cancel($CreditsSlideShow);
   canvas.showBOLCredits = false;

   //kill the music & start menu music
   alxMusicFadeout($pref::audio::musicvolume);
   schedule(mfloor(8000*$pref::audio::musicvolume),0,"alxPlayMusic","T2BOL/music/menu.mp3");

   //load the launch gui back...
   Canvas.setContent(LaunchGui);

   //delete the contents of the ML ctrl so as to free up memory...
   Credits_Text.setText("");
}

function CreditsGui::onWake(%this)
{
   //create an action map to use "esc" to exit the credits screen...
   if (!isObject(CreditsActionMap))
   {
      new ActionMap(CreditsActionMap);
      CreditsActionMap.bindCmd(keyboard, anykey, "cancelCredits();", "");
      CreditsActionMap.bindCmd(keyboard, space, "cancelCredits();", "");
      CreditsActionMap.bindCmd(keyboard, escape, "cancelCredits();", "");
      CreditsActionMap.bindCmd(mouse, button0, "$CreditsPaused = true;", "$CreditsPaused = false;");
      CreditsActionMap.bindCmd(mouse, button1, "$CreditsSpeedUp = true;", "$CreditsSpeedUp = false;");
      if (!isDemo())
         CreditsActionMap.bindCmd(mouse, button2, "creditsNextPic();", "");
   }
   CreditsActionMap.push();

   //build the ML text ctrl...
   if (!canvas.showBOLCredits)
   exec("scripts/creditsText_default.cs");
   else
   exec("scripts/creditsText.cs");
   
   if (!isDemo())
   {
      $CreditsPicIndex = 1;
      CREDITS_Pic.setBitmap("gui/Cred_" @ $CreditsPicIndex @ ".png");
   }
   else
      CREDITS_Pic.setBitmap("gui/Cred_1.bm8");

   //start the credits from the beginning
   $CreditsOffset = 0.0;
   %screenHeight = getWord(getResolution(), 1);
   Credits_Text.resize(getWord(Credits_Text.position, 0),
                        mFloor(%screenHeight / 2) - 125,
                        getWord(Credits_Text.extent, 0),
                        getWord(Credits_Text.extent, 1));

   //start the scrolling
   $CreditsPaused = false;
   $CreditsSpeedUp = false;
   $CreditsScrollSchedule = schedule(3500, 0, scrollTheCredits);

   //start cycling the bitmaps
   if (!isDemo())
      $CreditsSlideShow = schedule(5000, 0, creditsNextPic);

   //start some music
   alxPlayMusic("T2BOL/music/TribesHymn.mp3");
   //alxMusicFadein(0); //Not really needed since the default music has a little bit of a delay before starting
}

function addCreditsLine(%text, %lastLine)
{
   CREDITS_Text.addText(%text @ "\n", %lastline);
}

function scrollTheCredits()
{
   //make sure we're not paused
   if (!$CreditsPaused)
   {
      //if we've scrolled off the top, set the position back down to the bottom
      %parentCtrl = CREDITS_Text.getGroup();
      if (getWord(Credits_Text.position, 1) + getWord(Credits_Text.extent, 1) < 0)
      {
         Credits_Text.position = getWord(Credits_Text.position, 0) SPC getWord(%parentCtrl.extent, 1);
         $CreditsOffset = getWord(Credits_Text.position, 1);
      }

      if ($CreditsSpeedUp)
         %valueToScroll = 10;
      else
         %valueToScroll = 1;

      //scroll the control up a bit
      Credits_Text.resize(getWord(Credits_Text.position, 0),
                           getWord(Credits_Text.position, 1) - %valueToScroll,
                           getWord(Credits_Text.extent, 0),
                           getWord(Credits_Text.extent, 1));
   }

   //schedule the next scroll...
   $CreditsScrollSchedule = schedule(10, 0, scrollTheCredits);
}

function creditsNextPic()
{
   //no slide show in the demo...
   if (isDemo())
      return;

   cancel($CreditsSlideShow);
   if (!$CreditsPaused)
   {
      $CreditsPicIndex += 1;
      if ($CreditsPicIndex > 46)
         $CreditsPicindex = 1;

      //set the bitmap
      CREDITS_Pic.setBitmap("gui/Cred_" @ $CreditsPicIndex @ ".png");
   }

   //schedule the next bitmap
   $CreditsSlideShow = schedule(5000, 0, creditsNextPic);
}
