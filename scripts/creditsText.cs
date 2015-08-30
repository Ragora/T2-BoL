// Replacement code for the original T2 credits?
if (!isFile("data/creditsText.txt"))
	exec("scripts/creditsText_default.cs");
else
{
	%read = new fileObject();
	%read.openForRead("data/creditsText.txt");
 
	while (!%read.isEOF())
	{
		%line = %read.readline();
		addCreditsLine(%line);
	}
	%read.detach();
}