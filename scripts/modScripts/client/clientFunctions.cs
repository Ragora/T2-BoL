//------------------------------------------------------------------------------
// Scripts/DO_NOT_DELETE/clientFunctions.cs (OPEN SOURCE)
// If you see this text, you have obtained the official copy of this file from
// one of the developers. Otherwise, your decompiler is so advanced that it can
// somehow get commented lines in a script. If this is not the case, someone has
// betrayed my trust.
// -- Dark Dragon DX (as of 2011).
//------------------------------------------------------------------------------

//Different from modVersionText, is used to compare our mod to the update server
//Since TribesNext will soon allow binary transfers, this is stored here.
$ModVersion = 1.0; //Looks better as whole numbuhs

//All clientCmds are secured here (so certain funcs can't simply just be disabled)
//Although, hooks can be attached for custom actions.
function clientCmdHandleScriptedCommand(%num,%arg1,%arg2,%arg3,%arg4)
{
	switch(%num)
	{
		case 0: //Pop Dialog
			Canvas.popDialog(%arg1);
		case 1: //BoxYesNo
			messageBoxYesNo(%arg1,%arg2,%arg3,%arg4);
		case 2: //FadeIn
			ServerConnection.setBlackOut(true, %arg1);
		case 3: //Fadeout
			ServerConnection.setBlackOut(false, %arg1);
		case 4: //Show Cursor
			$cursorControlled = %arg1;
			lockMouse(%arg1);

		if (%arg1)
		{
			Canvas.cursorOn();
			GlobalActionMap.bind(mouse, button0, RTS_LeftButton);
			GlobalActionMap.bind(mouse, button1, RTS_RightButton);
			RTS_Command.push();
			$RTS::ButtonPress = false;
		}
		else
		{
			Canvas.cursorOff();
			GlobalActionMap.unbind(mouse, button0);
			GlobalActionMap.unbind(mouse, button1);
			Canvas.setCursor(DefaultCursor);
			RTS_Command.pop();
			$RTS::ButtonPress = true;
		}
		case 5: //Verify Client
			if (%arg1)
			ScoreParent.settext("PDA - PERSONAL DATA ASSISTANT");
			$Pref::LANAccount::GUID = stripNonNumericCharacters($Pref::LANAccount::GUID); //Make sure the GUID is pure before sending. Monkee, you won't be breaking anything here. The server does the same on its side. :)
			//Let the server know we're an actual client.. and if we're offline, send my GUID
			if (!$PlayingOnline)
				commandToServer('VerifyClient',$Pref::LANAccount::GUID,$ModVersion);
			else
				commandToServer('VerifyClient',0,$ModVersion);
			//Turn off the 'continue' button if it's T2Bol.
			if (%arg2)
				DB_ContinueBTN.setActive(0);
			case 6: //Is RTS Game
				hudClusterBack.opacity = 0; //Make it invisible
				clientCmdHandleScriptedCommand(4,true); //Show our cursor
			case 7: //Music fadeout
				alxMusicFadeout($Pref::Audio::MusicVolume);
			case 8: //Music Fadein
				alxMusicFadein(0);
			case 9: //Set client Time
				clockHud.setVisible(0);
				%pos = ClockHud.getPosition();
				%x = getWord(%pos,0);
				%y = getWord(%pos,1);
				%x = %x - -14;
				%y = %y - -4;
				if (!IsObject(timeHud))
				{
					new GuiTextCtrl(timeHud)
					{
						profile ="ClockProfile";
						position = %x SPC %y;
						extent = "41 12";
						text = %arg1 SPC "Hrs";
						horizSizing = "left";
						vertSizing = "bottom";
					};
					playGui.add(timeHud);
				}

				timeHud.setValue(%arg1);
				timeHud.setVisible(1);
			default: //If for some reason we got an invalid command ID, report it to console
				if ($Pref::DeveloperMode) //If dev mode is on (just a value set on the clientside to tell scripts to echo shit to the console)
					error("Scripted Command Handler: Received unknown command request ("@%num@") from server.");
				return false;
	}
	return true;
}
