//Parse our file blahha!
if (!isFile("data/creditsText.txt"))
{
exec("scripts/creditsText_default.cs");
}
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
