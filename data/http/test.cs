

function ServerApp::execute(%this,%data)
{
	%data = strReplace(%data,"#TIME#",formatTimeString("hh:nn:ss A"));
	%data = strReplace(%data,"#DATE#",formatTimeString("mm/dd/yy"));
	return %data;
}