This program will port an existing Wise script over to NSIS.  It doesn’t port all functionality 
of WISE to NSIS but it does a lot.  It does not port any GUI aspects of WISE to NSIS, but then why would you? 
 
The “Template” text box above must point to an existing NSIS script.  This template script contains the
 basic parts of an NSIS script and allows you to control where the code generator will place the code.
If you want to add new features to the program then by all means use the code generator options.  It will 
generate C# code for WISE commands it doesn’t understand.  All it does basically is create a method with the 
WISE command commented out.  It is a good stub for you to add the NSIS code you want generated thou. 

This is a quick and dirty tool but can be very useful when converting many WISE scripts to NSIS.  I had to 
convert about 30 scripts with a total of about 50K lines of code and this tool did the mundane work flawlessly.

I hope you find it usefull.




No guarantee is made about anything. Use at your own risk.