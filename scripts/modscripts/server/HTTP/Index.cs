//------------------------------------------------------------------------------
// Index.cs
// Home page for the BoL inbuilt web server
// Copyright (c) 2012 Robert MacGregor
//------------------------------------------------------------------------------

// Function that returns raw HTML formatting for the web browser on the client end to read
function Index::contents(%this)
{
	%data = "<head><title>T2BoL Webserver Testing</title></head><body>" @
	"<center>" @
	"<font size=\x225\x22>Welcome! This is the testing index page!</font>" @
	"<hr></br></br>" @
	"<table border=\x225\x22 width=\x22200\x22>" @
	"<tr>" @
	"<td><center><a href=\x22ActivePlayers.cs\x22>Active Players</a></center></td>" @
	"</tr>" @
	"</table>" @
	"</body>";
	return %data;
}