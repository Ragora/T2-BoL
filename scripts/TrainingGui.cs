//------------------------------------------------------------------------------
//
// TrainingGui.cs
//
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function LaunchTraining()
{
   LaunchTabView.viewTab( "CAMPAIGN", TrainingGui, 0 );
}

//------------------------------------------------------------------------------
function TrainingGui::onWake( %this )
{
   Canvas.pushDialog( LaunchToolbarDlg );


   //Reset the list
    TrainingMissionList.clear();
    
   if ($Pref::Campaign !$= "")
   TrainingSelectMenu.onSelect(TrainingSelectMenu.findText($Pref::Campaign),$Pref::Campaign);
   else
   TrainingSelectMenu.onSelect(0,TrainingSelectMenu.getText(0));
   TrainingMissionList.sort( 1 );
   TrainingMissionList.setSelectedRow( 0 );
   if ( $pref::TrainingDifficulty > 0 && $pref::TrainingDifficulty < 4 )
      TrainingDifficultyMenu.setSelected( $pref::TrainingDifficulty );
   else
      TrainingDifficultyMenu.setSelected( 1 );
}

//------------------------------------------------------------------------------
function TrainingGui::onSleep( %this )
{
	%this.stopBriefing();

	Canvas.popDialog(LaunchToolbarDlg);
}

//------------------------------------------------------------------------------
function TrainingGui::updateList( %this )
{
%cam = $Pref::Campaign;
%dir = "Data/Campaigns/";
%file = %dir @ %cam @ ".txt";

if (!IsFile(%file))
return false;

TrainingMissionList.clear();
TrainingBriefingText.setValue( "" );
TrainingPic.setBitmap( "" );
TrainingPicFrame.setVisible( false );
%count = getBlockData(%file,"Campaign",1,"MissionCount");

//Before we do the loopy stuff, we should check if there's a specific training mission first
%mission = getBlockData(%file,"Campaign",1,"Practice");
%text = getBlockData(%file,"Campaign",1,"PracticeText");

if (%text != -1 && %mission != -1)
TrainingMissionList.addRow( 0, %text TAB %mission );

 for (%i = 1; %i <= %count; %i++)
 {
  %mission = getBlockData(%file,"Campaign",1,"Mission" @ %i);
  %text = getBlockData(%file,"Campaign",1,"MissionText" @ %i);
  TrainingMissionList.addRow( %i, %text TAB %mission );
  
  //Now the mission list is where we need it, find the player settings..
 if (getBlockCount(%file,%mission) != 0)
 {
  $Training::Name[%Mission] = getBlockData(%file,%mission,1,"Name");
  $Training::Race[%Mission] = getBlockData(%file,%mission,1,"Race");
  $Training::Sex[%Mission] = getBlockData(%file,%mission,1,"Sex");
  $Training::Voice[%Mission] = getBlockData(%file,%mission,1,"Voice");
  $Training::VoicePitch[%Mission] = getBlockData(%file,%mission,1,"VoicePitch");
  $Training::Skin[%Mission] = getBlockData(%file,%mission,1,"Skin");
  $Training::EnemySkin[%Mission] = getBlockData(%file,%mission,1,"EnemySkin");
  $Training::EnemyName[%Mission] = getBlockData(%file,%mission,1,"EnemyName");
  $Training::EnemyTeam[%Mission] = getBlockData(%file,%mission,1,"EnemyTeam");
  $Training::PlayerTeam[%Mission] = getBlockData(%file,%mission,1,"PlayerTeam");
  $Training::StartLives[%Mission] = getBlockData(%file,%mission,1,"StartLives");
  $Training::EnemyRace[%Mission] = getBlockData(%file,%mission,1,"EnemyRace");
 }
 else
 {
  $Training::Name[%Mission] = getBlockData(%file,"Settings",1,"Name");
  $Training::Race[%Mission] = getBlockData(%file,"Settings",1,"Race");
  $Training::Sex[%Mission] = getBlockData(%file,"Settings",1,"Sex");
  $Training::Voice[%Mission] = getBlockData(%file,"Settings",1,"Voice");
  $Training::VoicePitch[%Mission] = getBlockData(%file,"Settings",1,"VoicePitch");
  $Training::Skin[%Mission] = getBlockData(%file,"Settings",1,"Skin");
  $Training::EnemySkin[%Mission] = getBlockData(%file,"Settings",1,"EnemySkin");
  $Training::EnemyName[%Mission] = getBlockData(%file,"Settings",1,"EnemyName");
  $Training::EnemyTeam[%Mission] = getBlockData(%file,"Settings",1,"EnemyTeam");
  $Training::PlayerTeam[%Mission] = getBlockData(%file,"Settings",1,"PlayerTeam");
  $Training::StartLives[%Mission] = getBlockData(%file,"Settings",1,"StartLives");
  $Training::EnemyRace[%Mission] = getBlockData(%file,"Settings",1,"EnemyRace");
 }
}
}

//------------------------------------------------------------------------------
function TrainingGui::setKey( %this )
{
}

//------------------------------------------------------------------------------
function TrainingGui::onClose( %this )
{
}

//------------------------------------------------------------------------------
function TrainingDifficultyMenu::onAdd( %this )
{
   %this.add( "Easy", 1 );
   %this.add( "Medium", 2 );
   %this.add( "Hard", 3 );
}

//------------------------------------------------------------------------------
function TrainingDifficultyMenu::onSelect( %this, %id, %text )
{
   $pref::TrainingDifficulty = %id;
}

//------------------------------------------------------------------------------
function TrainingSelectMenu::onAdd( %this )
{

//Uber Dynamic Campaign Listing :)
%path = "Data/Campaigns/*.txt";
%count = 0;
 for( %file = findFirstFile( %path ); %file !$= ""; %file = findNextFile( %path ) )
 {
  %this.add( getBlockData(%file,"Campaign",1,"Name"), %count);
  %count++;
 }
}

//------------------------------------------------------------------------------
function TrainingSelectMenu::onSelect( %this, %id, %text )
{
   $Pref::Campaign = %text;

   %row = TrainingMissionList.getSelectedID() - 1;
   TrainingGui.updateList();
   TrainingGui.stopBriefing();
   TrainingSelectMenu.setText(%text); //Make sure our text is assigned..
   
   if ($Pref::Campaign $= %text)
   TrainingMissionList.setSelectedRow(%row);
   else
   TrainingMissionList.setSelectedRow(0);

}

//------------------------------------------------------------------------------
function TrainingMissionList::onSelect( %this, %id, %text )
{
	TrainingGui.stopBriefing();
   %fileName = "missions/" @ getField( %text, 1 ) @ ".mis";
   %file = new FileObject();
   %state = 0;
   if ( %file.openForRead( %fileName ) )
   {
		// Get the mission briefing text:
      while ( !%file.isEOF() )
      {
         %line = %file.readLine();
         if ( %state == 0 && %line $= "//--- MISSION BRIEFING BEGIN ---" )
            %state = 1;
         else if ( %state > 0 && %line $= "//--- MISSION BRIEFING END ---" )
            break;
         else if ( %state == 1 )
			{
            %briefText = %briefText @ getSubStr( %line, 2, 1000 );
				%state = 2;
			}
         else if ( %state == 2 )
            %briefText = %briefText NL getSubStr( %line, 2, 1000 );
      }

		// Get the mission briefing WAV file:
		while ( !%file.isEOF() )
		{
         %line = %file.readLine();
			if ( getSubStr( %line, 0, 17 ) $= "// BriefingWAV = " )
         {
				%briefWAV = getSubStr( %line, 17, 1000 );
            break;
         }
		}
  
  	   // Get the bitmap name:
		while ( !%file.isEOF() )
		{
         %line = %file.readLine();
			if ( getSubStr( %line, 0, 12 ) $= "// Bitmap = " )
         {
				%briefPic = getSubStr( %line, 12, 1000 );
            break;
         }
		}

      %file.close();
   }
	else
		error( "Failed to open Single Player mission file " @ %fileName @ "!" );

   if (!isDemo())
      %bmp = "gui/" @ %briefPic @ ".png";
   else
      %bmp = "gui/" @ %briefPic @ ".bm8";
      
   if ( isFile( "textures/" @ %bmp ) )
   {
      TrainingPic.setBitmap( %bmp );
      TrainingPicFrame.setVisible( true );
   }
   else
   {
      TrainingPic.setBitmap( "" );
      TrainingPicFrame.setVisible( false );
   }
   
	TrainingPlayBtn.setActive( %briefWAV !$= "" );
   TrainingBriefingText.setValue( %briefText );
	TrainingBriefingScroll.scrollToTop();
	TrainingGui.WAVBase = firstWord( %briefWAV );
	TrainingGui.WAVCount = restWords( %briefWAV );
   %file.delete();

   //if ( TrainingPlayTgl.getValue() )
   //   TrainingGui.startBriefing();
}

//------------------------------------------------------------------------------
function TrainingPlayTgl::onAction( %this )
{
   if ( %this.getValue() )
   {
      if ( TrainingMissionList.getSelectedId() != -1 )
         TrainingGui.startBriefing();
   }
   else
      TrainingGui.stopBriefing();
}

//--------------------------------------------------------
function TrainingGui::toggleBriefing( %this )
{
   if ( %this.soundHandle $= "" )
   {
      %this.startBriefing();
      alxMusicFadeout($Pref::Audio::MusicVolume);
   }
   else
      %this.stopBriefing();
}

//--------------------------------------------------------
function TrainingGui::startBriefing( %this )
{
	%this.stopBriefing();
   if ( %this.WAVBase !$= "" )
   {
      %this.instance = %this.instance $= "" ? 0 : %this.instance;
	   %this.playNextBriefWAV( %this.WAVBase, 0, %this.WAVCount, %this.instance );
   }
}

//--------------------------------------------------------
function TrainingGui::playNextBriefWAV( %this, %wavBase, %id, %count, %instance )
{
	if ( %instance == %this.instance )
	{
      if ( %id < %count )
      {
		   %wav = "voice/Training/Briefings/" @ %wavBase @ ".brief0" @ ( %id + 1 ) @ ".wav";
	      %this.soundHandle = alxCreateSource( AudioGui, %wav );
	      alxPlay( %this.soundHandle );

	      // Schedule the next WAV:
		   %delay = alxGetWaveLen( %wav ) + 500;
		   %this.schedule( %delay, playNextBriefWAV, %wavBase, %id + 1, %count, %instance );
      }
      else
      {
         // We're all done!
         %this.soundHandle = "";
      }
	}
}

//--------------------------------------------------------
function TrainingGui::stopBriefing( %this )
{
	if ( %this.soundHandle !$= "" )
	{
		alxStop( %this.soundHandle );
		%this.soundHandle = "";
		%this.instance++;
	}
}

//--------------------------------------------------------
function TrainingGui::startTraining( %this )
{
   MessagePopup( "STARTING MISSION", "Initializing, please wait..." );
   Canvas.repaint();
   cancelServerQuery();
   %file = getField( TrainingMissionList.getValue(), 1 );
   $ServerName = "Single Player Training";
   $HostGameType = "SinglePlayer";
   CreateServer( %file, "SinglePlayer" );
   alxMusicFadeout($Pref::Audio::MusicVolume);
   localConnect( $Training::Name[%file], $Training::Race[%file] SPC $Training::Sex[%file], $Training::Skin[%file], $Training::Voice[%file] );
}
