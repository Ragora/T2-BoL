// -----------------------------------------------------
// Block Processing
// -----------------------------------------------------
function getBlockCount(%file,%blockName) //Searches a data file for all occurances of 'blockName'
{
if (!IsFile(%file))
return false;

%blockSearch = strLwr("[" @ %blockName @ "]");

%fileP = new FileObject();
%fileP.openForRead(%file);

%count = 0;
 while (!%fileP.isEOF())
 {
  %line = %fileP.readLine();
  %lineTest = strLwr(%line);
  %Search = strStr(%lineTest,%blockSearch);
  if (%search != -1)
  %count++;
 }
%fileP.detach();
return %count;
}

function getBlockData(%file,%blockName,%num,%data) //Gets values out of a block. Note that 1 is the first block in a file for %num
{
if (!IsFile(%file))
return false;

%blockCount = getBlockCount(%file,%blockName);

if (!%blockCount || %num > %blockCount) //None of 'typeName' exist here
return -1;

%blockSearch = strLwr("[" @ %blockName @ "]");

%fileP = new FileObject();
%fileP.openForRead(%file);

%count = 0;
%lineCount = 0;
 while (!%fileP.isEOF())
 {
  %line = %fileP.readLine();
  %lineCount++;
  
  if (getSubStr(stripSpaces(%line),0,1) !$= ";") //If the beginning of the line is "commented", skip it.
  {
   %lineTest = strLwr(%line);
   %Search = strStr(%lineTest,%blockSearch);

   if (%Search != -1 && %Count != %num)
   %count++;
    else if (%count == %num) //We found it, stop incrementing the count and find our data.
    {
     %Search = strStr(strLwr(%line),strLwr(%data));

     %lineTest = strLwr(strReplace(%line," ","")); //See if we exited our block
     if (GetSubStr(%lineTest, 0, 1) $= "[")  //The beginning of a new block
     return -1;

     if (%search != -1) //We found it,
     {
      %fileP.detach();

      //Since our line might have some sort of commenting after it, we better return to just the "end" symbol..
      %semiS =strStr(%line, ";");
      if (%semiS == -1)
      return -1;
      %line = getSubStr(%line, 0, %semiS);
      //Now find where "equals" is..
      %equalS = strStr(%line, "=");
      if (%equalS == -1)
      return -1;
      %line = getSubStr(%line, %equalS+1, strLen(%line));
      //Is our data in single quotations? If so, convert it to a tagged string.
      if (strStr(%line,"\x27") != -1) //It is.
      %line = addTaggedString(stripChars(%line,"\x27"));
      //Now return our string without quotations.
      %line = stripChars(stripTrailingSpaces(strReplace(%line,%data,"")),"\x22");
      return getSubStr(%line,1,strLen(%line));
    }
   }
  }
 }

%fileP.detach();
return false;
}

//function getBlockLength(%file,%blockName,%num) Won't work until I figure out a way to signal the end of a block without adding any extra characters,
//{
//if (!IsFile(%file))
//return "Not existant.";

//%blockSearch = "[" @ %blockName @ "]";
//%blockSearch = strLwr(%blockSearch); //Convert to lowerCase

//new FileObject(GetBlockCount);
//GetBlockCount.openForRead(%file);

//%count = 0;
//%len = 0;
// while (!GetBlockCount.isEOF())
// {
// %line = GetBlockCount.readLine();
// %lineTest = strLwr(%line);
// %Search = strStr(%lineTest,%blockSearch);
 //if (%search != -1)
// %count++;
// else if (%search != -1 && %count == %num) //We found our wanted block, count it.
 //{
 // if (strStr(%lineTest,%blockSearch) == -1)
 // %len++;
//  else
//  break;
// }
//}
//GetBlockCount.detach();
//return %len;
//}

// -----------------------------------------------------
// Array Processing
// -----------------------------------------------------
function getArrayCount(%file,%array)
{
if (!IsFile(%file))
return false;

%arraySearch = strLwr("\x22" @ %array @ "\x22");

%fileP = new FileObject();
%fileP.openForRead(%file);

%count = 0;
 while (!%fileP.isEOF())
 {
  %line = %fileP.readLine();
  %lineTest = strLwr(%line);
  %Search = strStr(%lineTest,%typeSearch);
  if (%search != -1)
  %count++;
 }
%fileP.detach();
return %count;
}

function getArrayData(%file,%arrayName,%num)
{
if (!IsFile(%file))
return false;

%arrayCount = getArrayCount(%file,%arrayName);

if (!%arrayCount)
return false;

%arraySearch = strLwr("\x22" @ %arrayName @ "\x22");

%fileP = new FileObject();
%fileP.openForRead(%file);

%lineCount = 0;
 while (!%fileP.isEOF())
 {
  %line = stripSpaces(%fileP.readline());
  %lineCount++;
 
   if (getSubStr(%line,0,1) !$= ";") //Is this line a comment?
   {
    %search = strStr(strLwr(%line),%arraySearch);
 
    if (%search !$= -1) //Found it.
    break; //Break the loop, we know the array exists
    if (%fileP.IsEOF() && %search == -1) //Didn't find it, return the error.
    return false;
   }
 }

//Check where the array actually starts..
%line = %fileP.readLine();
 if (%line $= "{") //Data starts on next line..
 {
 %line = %fileP.readLine(); //Drop it down one
  for (%i = 0; %i < %num; %i++) //Keep going untill we hit the wanted data
  {
  %line = %fileP.readLine();
 }
}
else //The line we just grabbed is part of the data
{
if (%num == 0) //The wanted data was on line zero..
return %line;

 for (%i = 0; %i < %num; %i++) //Keep going untill we hit the wanted data
 {
 %line = %fileP.readLine();
 }
}

%fileP.detach();
return %line;
}

