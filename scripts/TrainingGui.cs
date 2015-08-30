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
	TrainingSelectMenu.onSelect(0,TrainingSelectMenu.getText());
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
	
	if (!IsObject(CampaignLoader))
		new ScriptObject(CampaignLoader) { class = "BasicDataParser"; };
	CampaignLoader.empty();
	CampaignLoader.load(%file);
	
	%campaign = CampainLoader.get("Campaign",0);
	%count = %campaign.element("MissionCount");

	//Before we do the loopy stuff, we should check if there's a specific training mission first
	%mission = %campaign.element("Practice");
	%text = %campaign.element("PracticeText");

	if (%text != -1 && %mission != -1)
		TrainingMissionList.addRow( 0, %text TAB %mission );

	%settings = CampainLoader.get("Settings",0);
	for (%i = 1; %i <= %count; %i++)
	{
		%mission = %campain.element("Mission" @ %i);
		%text = %campaign.element("MissionText" @ %i);
		TrainingMissionList.addRow( %i, %text TAB %mission );
		//Now the mission list is where we need it, find the player settings..
		$Training::Name[%Mission] =  %settings.element("Name");
		$Training::Race[%Mission] = %settings.element("Race");
		$Training::Sex[%Mission] =  %settings.element("Sex");
		$Training::Voice[%Mission] = %settings.element("Voice");
		$Training::VoicePitch[%Mission] = %settings.element("VoicePitch");
		$Training::Skin[%Mission] = %settings.element("Skin");
		$Training::EnemySkin[%Mission] = %settings.element("EnemySkin");
		$Training::EnemyName[%Mission] = %settings.element("EnemyName");
		$Training::EnemyTeam[%Mission] = %settings.element("EnemyTeam");
		$Training::PlayerTeam[%Mission] =  %settings.element("PlayerTeam");
		$Training::StartLives[%Mission] = %settings.element("StartLives");
		$Training::EnemyRace[%Mission] = %settings.element("EnemyRace");
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
	%path = "Data/Campaigns/*.dat";
	%count = 0;
	
	if (!IsObject(CampaignLoader))
		new ScriptObject(CampaignLoader) { class = "BasicDataParser"; };
	CampaignLoader.empty();
	CampaignLoader.load(%file);
	
	for( %file = findFirstFile( %path ); %file !$= ""; %file = findNextFile( %path ) )
	{
		CampaignLoader.load(%file);
		%this.add( CampaignLoader.get("Campaign",%count).element("Name"), %count);
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
	return true;
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
