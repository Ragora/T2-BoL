//------------------------------------------------------------------------------
// Scripts/RPGBrowserGUI.cs
// One of the highlights of T2Bol.
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPGBrowserGui::onWake(%this)
{
if (RPGBrowserGui.pane $= "")
RPG_NewsPane.onActivate();

LaunchTabView.viewTab( "MOD BROWSER", RPGBrowserGui, 0 );
Canvas.pushDialog( LaunchToolbarDlg );

 // This is essentially an "isInitialized" flag...
   if ( RPG_TabView.tabCount() == 0 )
   {
      RPG_TabView.addSet(1, "gui/shll_horztabbuttonB", "5 5 5", "50 50 0", "5 5 5" );
      RPG_TabView.addTab(1,"NEWS",1);
      RPG_TabView.setSelected( 1 );
      RPG_TabView.addTab(2,"ENCYCLOPEDIA");
      RPG_TabView.addTab(3,"DOWNLOADS");
      RPG_TabView.setTabActive(3,0);
   }
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPGBrowserGui::onSleep(%this)
{
   %ctrl = "RPG_" @ %this.pane @ "Pane";
   if ( isObject( %ctrl ) )
      %ctrl.onDeactivate();
}

//------------------------------------------------------------------------------
function BrowserDoSave() //A good ol' converter for browser files. Should work on ANY browser file. Including ones that you insert yourself.
{
	%file = new fileObject();
	%item = RPG_ItemList.getValue();
	%category = RPG_Category.getValue();
	%file.openForWrite("savedDocs/" @ %item @ ".txt");
	%read = new fileObject();
	%read.openForRead("data/encyclopedia/" @ %category @ "/" @ %item @ ".txt");

	%skip = false;
	while (!%read.isEOF())
	{
		%line = %read.readLine();
		%lineTest = strLwr(strReplace(%line," ","")); //strip spaces to test for tags to skip
		%spush = getSubStr(%lineTest,0,7);
		%just = getSubStr(%lineTest,0,6);

		//Ok.. we need to cipher out some useless information before we're through
		if (%just !$= "<just:")
		{
			if (%spush $= "<spush>")
			%skip = true;

			if (%skip == true) //Try to find our <spop> tag
			{
				%search = strStr(%lineTest,"<spop>");
				if (%search != -1)
					%skip = false; //We found our <spop> tag, set %skip to false for the next turn
			}
			else //Otherwise the data is safe -- but we still got to look for my special <a:select> tag
			{
				%select = strStr(%lineTest,"<a:select");
				if (%select != -1) //It exists -- we must convert it
				{
					%line = strReplace(%line,"-","\t"); //Apparently special tags don't work when read from a file, so the negative sign represents \t
					%tag = strStr(%line,"<a:select"); //Find the position of my select tag
					if (%tag != -1) //Ok, it exists. We need to grab our entire tag as a string
					{
						%start = strStr(%line,"<");

						for (%i = 0; %i < strLen(%line); %i++)
						{
							%testC = getSubStr(%line, %i + %start, 3);
							if (%testC $= "</a")
							{
								%end = %i;
								break;
							}
						}
					}
				}
				if (%line !$= "") //If the line is totally blank, don't do shit. Otherwise, attempt to space out the file (for easier read)
				{
					for (%i = 0; %i < strLen(%line); %i++) //Was up a little higher in the function; figured it should be moved down here
					{
						%test = getSubStr(%line,%i,9);
						if (%test $= "<a:select") //We found a select tag.
						{
							%start = %i;
							%end = strLen(%line) - strStr(%line,"</a>") - 3;
							%test = getSubStr(%line,%start,%end);
							%str = getWord(%test,0);
							%line = strReplace(%line,%str,getNameFromSelectTag(%str));
						}
					}
					for (%i = 0; %i < strLen(%line); %i++)
					{
						%sub = getSubStr(%line, %i, 1);
						if (%sub $= ".") //Ok, we found a period. %i is where we are in the string. So we grab everything up until this period and move on
						{
							%file.writeLine(getSubStr(%line, 0, %i+1));
							%line = getSubStr(%line, %i+1, strLen(%line));
						}
					}
				}
			}
		}
	}

	//Detach our file objects (scripts/fileProcessing.cs)
	%file.detach();
	%read.detach();

	messageBoxOk("SUCCESS","The contents of item "@%item@" in category "@%category@" have been saved. Check your mod Directory's sub-dir, /savedDocs for "@%item@".txt."); //YES! After all that the file should be good.
	return true;
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------

function getNameFromSelectTag(%string) //Since the tag should be directly inserted, there is no start.
{
	%len = strLen(%string);

	%start = strStr(%string,">");
	%end = strStr(%string,"</a>");
	%sub = getSubStr(%string,%start,%end);
	%sub = stripTrailingSpaces(stripChars(strReplace(%sub,"</a>",""),">"));
	return %sub;
}
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPGBrowserGui::setKey( %this, %key )
{
   // To avoid console error
}

//------------------------------------------------------------------------------
function RPGBrowserGui::onClose( %this, %key )
{
   // To avoid console error
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// News pane:
//------------------------------------------------------------------------------

function RPG_NewsPane::onActivate(%this)
{
RPGBrowserGui.pane = "News";
}

//------------------------------------------------------------------------------
function RPG_NewsPane::onDeactivate(%this)
{
//RPGBrowserGui.pane = "News";
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Browser pane:
//------------------------------------------------------------------------------

function RPG_BrowserPane::onActivate(%this)
{
	RPGBrowserGui.pane = "Browser";
	RPG_Category.PopulateList();

	//Make sure our DL button is properly placed (resolution may have changed)
	RPG_DownloadButton.setPosition("5", getWord(Canvas.getExtent(), 1) - 163);

	RPG_BrowserPane.refresh();
}
//------------------------------------------------------------------------------

function RPG_BrowserPane::Refresh(%this,%val)
{
	//Ok, we must make the browser dynamic for convenience.
	%text = RPG_Category.getText();

	if (%text $= "Select Category" || %text $= "")
		return RPG_Text.readFromFile("data/encyclopedia/index.txt");

	RPG_ItemList.clear();

    %count = 0;
	%path = "data/encyclopedia/" @ %text @ "/*.txt";
	for( %file = findFirstFile( %path ); %file !$= ""; %file = findNextFile( %path ) )
	{
		%name = getFileNameFromString(strReplace(%file,".txt","")); //Get the fileName from our string (used in the item List)
		if (%name !$= "index")
		{
            RPG_ItemList.addRow(%count, subWordCapitalize(%name));
			%count++;
		}
	}
	RPG_ItemList.sortNumerical(0,0); //Sort the items from A-Z
	//RPG_ItemList.setSelectedRow(RPG_ItemList.findTextIndex($Browser::SelectedText));
	RPG_ItemList.setSelectedRow(RPGBrowserGUI.selectedID[%text]);
}
//------------------------------------------------------------------------------
function RPG_BrowserPane::onDeactivate(%this)
{
//RPGBrowserGui.pane = "Browser";
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Downloads pane:
//------------------------------------------------------------------------------

function RPG_DownloadsPane::onActivate(%this)
{
RPGBrowserGui.pane = "Downloads";
}

//------------------------------------------------------------------------------

function RPG_DownloadsPane::onDeactivate(%this)
{
//RPGBrowserGui.pane = "Downloads";
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPG_TabView::onSelect( %this, %id, %text )
{
	RPG_BrowserPane.setVisible( %id == 2 );
	RPG_NewsPane.setVisible( %id == 1 );

	%ctrl = "RPG_" @ RPGBrowserGui.pane @ "Pane";
	if ( isObject( %ctrl ) )
		%ctrl.onDeactivate();

	switch ( %id )
	{
		case 1:  // News
			RPG_NewsPane.onActivate();
		case 2:  // Encyclopedia
			RPG_BrowserPane.onActivate();
		case 3:  // Downloads
			RPG_DownloadsPane.onActivate();
   }
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPG_Category::PopulateList() //Listing is now alphabatized
{
    RPG_Category.clear();

	%file = "data/encyclopedia/encyclopedia.conf";

	%parser = new ScriptObject(){ class = "BasicDataParser"; };
	%parser.load(%file);
	%block = %parser.get("Config", 0);
	%count = %block.element("EntryCount");
	RPG_Category.count = %count;

	for (%i = 0; %i < %count; %i++)
	{
		%category = %block.element("Entry" @ %i);
		RPG_Category.add(%category,%i);
	}

	if (RPG_Category.selected !$= "") //Fixes the category dropdown resetting
		RPG_Category.setSelected(RPG_Category.selected);
	else
		RPG_Category.setValue("Select Category");
	//Force the browser to have an introduction.
	RPG_Text.readFromFile("data/encyclopedia/index.txt");

	// Dealloc our parser
	%parser.empty();
	%parser.delete();
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPG_Category::onSelect(%this, %id, %text)
{
	RPG_ItemList.clear();
	RPG_Text.setValue("");
	RPG_Category.selected = %id;
	RPG_Text.readFromFile("data/encyclopedia/" @ %text @ "/index.txt"); //Display the intro for the category we selected
	RPG_BrowserPane.refresh();
	if (RPG_ItemList.getSelectedID() == -1) //Does our button really need to be inactive?
		RPG_DownloadButton.setActive(false); //YES!
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPG_ItemList::onSelect(%this, %id, %text)
{
	$Browser::SelectedText = %text;
	RPG_DownloadButton.setActive(true);
	%category = RPG_Category.getvalue();
	RPGBrowserGUI.selectedID[%category] = %id;
	RPG_Text.readFromFile("data/encyclopedia/" @ %category @ "/" @ %text @ ".txt");
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function RPG_Text::readFromFile(%this,%file)
{
	RPG_Text.setValue(""); //Clear the info, for some odd reason .clear() won't work..

	if (!IsFile(%file))
	return -1;

	%fileobj = new FileObject();
	%fileobj.openForRead(%file);

	while (!%fileobj.isEOF())
	{
		%line = %fileobj.readLine() @ "\n";
		RPG_Text.addText(%line,1);
	}
	%fileobj.detach();
}

//------------------------------------------------------------------------------

RPG_DownloadButton.setActive(false); //Eh.. for some reason setting isActive in the GUI file isn't working
