using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WiseToNSIS
{
   public partial class Form1 : Form
   {
      StreamReader mInFile;
      StreamWriter mOutFile;

      //This is the global varibles as they were read in.
      List<string> mGlobalVariable = new List<string>();

      List<string> mGeneratedCodeBlocks = new List<string>();
      List<string> mGeneratedCodeCase = new List<string>();
      List<string> mGeneratedCodeFunctions = new List<string>();

      string mCurrentSetOutPath = "";
      bool mOverwriteFlag = true;
      bool mOverwriteBeenSet = false;

     string mEXEFilename = "";
     string mVersionFile = "";
     string mVersionDescription = "";

      bool mEnableComments = false;

      int mNestingLevel = 1;

      //This is set to true if the code is commented out
      bool mRemarkedItem = false;

      public Form1()
      {
         InitializeComponent();

         textBoxWise.Text = "Setup.wse";
         textBoxNSIS.Text = textBoxWise.Text.Substring(0, textBoxWise.Text.Length - 3) + "nsi";
         if (System.IO.File.Exists("Template.nsi"))
         {
             textBoxTemplate.Text = "Template.nsi";
         }
         if (System.IO.File.Exists("..\\Template.nsi"))
         {
             textBoxTemplate.Text = "..\\Template.nsi";
         }
         if (System.IO.File.Exists("..\\..\\Template.nsi"))
         {
             textBoxTemplate.Text = "..\\..\\Template.nsi";
         }

         ///This is used for Exec, to save current $OUTDIT
         this.addGlobalVariable("OUTDIRTemp");
         this.addGlobalVariable("gWise1");
         this.addGlobalVariable("gWise2");

         //convertFile();
      }

      private void buttonInputFile_Click(object sender, EventArgs e)
      {
         OpenFileDialog lFBD = new OpenFileDialog();
         lFBD.Title = "Select Data File";
         lFBD.Filter = "wse files (*.wse)|*.wse|All files (*.*)|*.*";
         lFBD.FilterIndex = 1;
         lFBD.InitialDirectory = System.Environment.CurrentDirectory;
         string lExpandedValue = textBoxWise.Text;
         lFBD.FileName = lExpandedValue;
         if (lFBD.ShowDialog() == DialogResult.OK)
         {
            string lNewDITPath = lFBD.FileName;
            textBoxWise.Text = lNewDITPath;
         }
      }

      private void buttonOutputFile_Click(object sender, EventArgs e)
      {
         SaveFileDialog lFBD = new SaveFileDialog();
         lFBD.Title = "Select Data File";
         lFBD.Filter = "nsi files (*.nsi)|*.nsi|All files (*.*)|*.*";
         lFBD.FilterIndex = 1;
         lFBD.InitialDirectory = System.Environment.CurrentDirectory;
         string lExpandedValue = textBoxNSIS.Text;
         lFBD.FileName = lExpandedValue;
         if (lFBD.ShowDialog() == DialogResult.OK)
         {
            string lNewDITPath = lFBD.FileName;
            textBoxNSIS.Text = lNewDITPath;
         }
      }

      private void buttonConvert_Click(object sender, EventArgs e)
      {
         convertFile();
      }

      private void addGlobalVariable(string aVariable)
      {
         foreach (string lVariable in mGlobalVariable)
         {
            if (lVariable == aVariable)
            {
               return;
            }
         }
         mGlobalVariable.Add(aVariable);
      }

      private void writeOutGlobalVariables()
      {
         StreamReader lInFile = new StreamReader(textBoxNSIS.Text);
         StreamWriter lOutFile = new StreamWriter(textBoxNSIS.Text + ".Temp");
         StreamReader lInTemplate = new StreamReader(textBoxTemplate.Text);

         while (lInTemplate.Peek() > 0)
         {
            string lLine = lInTemplate.ReadLine();
            if (lLine.ToUpper().Contains("#GLOBAL#"))
            {
               lOutFile.WriteLine(";Auto generated variables from WiseToNSIS converter.");
               foreach (string lVariable in mGlobalVariable)
               {
                  string lCommand = "Var /GLOBAL " + lVariable;
                  lOutFile.WriteLine(lCommand);
               }
            }
            else if (lLine.ToUpper().Contains("#BODY#"))
            {
               while (lInFile.Peek() >= 0)
               {
                  //Read the next line
                  lLine = lInFile.ReadLine();
                  lOutFile.WriteLine(lLine);
               }
            }
            else if (lLine.ToUpper().Contains("#mEXEFilename#".ToUpper()))
            {
               lLine = lLine.Replace("#mEXEFilename#", mEXEFilename);
               lOutFile.WriteLine(lLine);
            }
            else if (lLine.ToUpper().Contains("#mVersionFile#".ToUpper()))
            {
               lLine = lLine.Replace("#mVersionFile#", mVersionFile);
               lOutFile.WriteLine(lLine);
            }
            else if (lLine.ToUpper().Contains("#mVersionDescription#".ToUpper()))
            {
               lLine = lLine.Replace("#mVersionDescription#", mVersionDescription);
               lOutFile.WriteLine(lLine);
            }
            else
            {
               lOutFile.WriteLine(lLine);
            }
         }

         lInTemplate.Close();
         lInFile.Close();
         lOutFile.Close();

         System.IO.File.Delete(textBoxNSIS.Text);
         System.IO.File.Move(textBoxNSIS.Text + ".Temp", textBoxNSIS.Text);
      }

      private void convertFile()
      {
         //Open the file
         mInFile = new StreamReader(textBoxWise.Text);
         System.IO.File.Delete(textBoxNSIS.Text);
         mOutFile = new StreamWriter(textBoxNSIS.Text);

         string lLine = "";
         while (mInFile.Peek() >= 0)
         {
            //Read the next line
            lLine = mInFile.ReadLine();
            processLine(lLine);
         }
         mInFile.Close();

         //Write generated code to output file
         foreach (string lCaseLine in mGeneratedCodeCase)
         {
            writeLineOutFile(lCaseLine);
         }
         foreach (string lCaseLine in mGeneratedCodeFunctions)
         {
            writeLineOutFile(lCaseLine);
         }
         mOutFile.Close();

         writeOutGlobalVariables();
      }

     private void processLine(string aLine)
     {
        int lPos = aLine.IndexOf("item:");

        int lRemarkedItem = aLine.IndexOf("remarked item:");
        if(lRemarkedItem == -1)
        {
           mRemarkedItem = false;
        }
        else
        {
           mRemarkedItem = true;
        }

        if (lPos != -1)
        {
           string lItemType = aLine.Substring(lPos + 6);

           if (lItemType.Contains("Open/Close"))
           {
              //Special case becase the file name is on the line
              lPos = lItemType.IndexOf("Open/Close");
              lItemType = lItemType.Substring(0, lPos + 10);
           }
           if (lItemType.Contains("Add Text to"))
           {
              //Special case becase the file name is on the line
              lPos = lItemType.IndexOf("Add Text to");
              lItemType = lItemType.Substring(0, lPos + 11);
           }

           switch(lItemType)
           {
              case "Global":
                 processItemGlobal(aLine);
                 return;
              case "Remark":
                 processItemRemark(aLine); //Tested 
                 return;
              case "Custom Script Item":
                 processItemCustomScriptItem(aLine);
                 return;
              case "Set Variable":
                 processItemSetVariable(aLine); //Tested
                 return;
              case "Create Directory":
                 processItemCreateDirectory(aLine);  //Tested
                 return;
              case "Get System Information":
                 processItemGetSystemInformation(aLine);
                 return;
              case "Insert Line into Text File":
                 processItemInsertLineintoTextFile(aLine); //Tested
                 return;
              case "Get Registry Key Value":
                 processItemGetRegistryKeyValue(aLine);
                 return;
              case "Open/Close":
                 processItemOpenClose(aLine);
                 return;
              case "If/While Statement":
                 processItemIfWhileStatement(aLine);     //tested
                 return;
              case "Add Text to":
                 processItemAddTextto(aLine);  //Tested
                 return;
              case "Else Statement":
                 processItemElseStatement(aLine);
                 return;
              case "End Block":
                 processItemEndBlock(aLine); //Tested
                 return;
              case "Exit Installation":
                 processItemExitInstallation(aLine);
                 return;
              case "Check if File/Dir Exists":
                 processItemCheckifFileDirExists(aLine);
                 return;
              case "Edit Registry":
                 processItemEditRegistry(aLine);
                 return;
              case "Execute Program":
                 processItemExecuteProgram(aLine);
                 return;
              case "Install File":
                 processItemInstallFile(aLine);
                 return;
              case "Self-Register OCXs/DLLs":
                 processItemSelfRegisterOCXsDLLs(aLine);
                 return;
              case "New Event":
                 processItemNewEvent(aLine);
                 return;
              case "Get Temporary Filename":
                 processItemGetTemporaryFilename(aLine); //tested
                 return;
              case "Parse String":
                 processItemParseString(aLine);
                 return;
              case "Check Configuration":
                 processItemCheckConfiguration(aLine);
                 return;
              case "Read INI Value":
                 processItemReadINIValue(aLine);
                 return;
              case "Wizard Block":
                 processItemWizardBlock(aLine);
                 return;
              case "Custom Dialog Set":
                 processItemCustomDialogSet(aLine);
                 return;
              case "Edit INI File":
                 processItemEditINIFile(aLine);
                 return;
              case "Check Disk Space":
                 processItemCheckDiskSpace(aLine);
                 return;
              case "Delete File":
                 processItemDeleteFile(aLine);
                 return;
              case "Display Graphic":
                 processItemDisplayGraphic(aLine);
                 return;
              case "Copy Local File":
                 processItemCopyLocalFile(aLine);
                 return;
              case "Read/Update Text File":
                 processItemReadUpdateTextFile(aLine);
                 return;
              case "Dialog":
                 processItemDialog(aLine);
                 return;
              case "Include Script":
                 processItemIncludeScript(aLine);
                 return;
              case "Display Message":
                 processItemDisplayMessage(aLine);
                 return;
              case "ElseIf Statement":
                 processItemElseIfStatement(aLine);
                 return;
              case "Start/Stop Service":
                 processItemStartStopService(aLine);
                 return;
              case "Get Environment Variable":
                 processItemGetEnvironmentVariable(aLine);
                 return;
              case "Set File Attributes":
                 processItemSetFileAttributes(aLine);
                 return;
              case "Install ODBC Driver":
                 processItemInstallODBCDriver(aLine);
                 return;
              case "Configure ODBC Data Source":
                 processItemConfigureODBCDataSource(aLine);
                 return;
              case "Create Shortcut":
                 processItemCreateShortcut(aLine);
                 return;
              default:
                 generateCode(lItemType);
                 break;
           }
        }
     }

     private void generateCode(string aItemType)
     {
        if(checkBoxCodeGenerator.Checked == false)
        {
           return;
        }

        if (aItemType.Contains("Open/Close"))
        {
           //Special case becase the file name is on the line
           int lPos = aItemType.IndexOf("Open/Close");
           aItemType = aItemType.Substring(0, lPos + 10);
        }
        if (aItemType.Contains("Add Text to"))
        {
           //Special case becase the file name is on the line
           int lPos = aItemType.IndexOf("Add Text to");
           aItemType = aItemType.Substring(0, lPos + 11);
        }
         

        bool lAlreadyAdded = false;
        foreach (string lCodeBlock in mGeneratedCodeBlocks)
        {
           if (lCodeBlock == aItemType)
           {
              lAlreadyAdded = true;
              break;
           }
        }
        {
           writeLineOutFile("");
        }
        if (lAlreadyAdded == false)
        {
           mGeneratedCodeBlocks.Add(aItemType);
           string lItemTypeNoSpace = aItemType.Replace(" ", "");
           lItemTypeNoSpace = lItemTypeNoSpace.Replace("/", "");
           lItemTypeNoSpace = lItemTypeNoSpace.Replace("-", "");
           //Why not just write the code to handle this item
           mGeneratedCodeCase.Add("              case \"" + aItemType + "\":");
           mGeneratedCodeCase.Add("                 processItem" + lItemTypeNoSpace + "(aLine);");
           mGeneratedCodeCase.Add("                 return;");
           mGeneratedCodeFunctions.Add("");
           mGeneratedCodeFunctions.Add("     private void processItem" + lItemTypeNoSpace + "(string aLine)");
           mGeneratedCodeFunctions.Add("     {");
           mGeneratedCodeFunctions.Add("             string lLine = \"\";");
           mGeneratedCodeFunctions.Add("             //Read the next line");
           mGeneratedCodeFunctions.Add("             //This is end or Text=");
           mGeneratedCodeFunctions.Add("             int lEqualPos = lLine.IndexOf('=');");
           mGeneratedCodeFunctions.Add("             if (lEqualPos == -1)");
           mGeneratedCodeFunctions.Add("             {");
           mGeneratedCodeFunctions.Add("                 writeLineOutFile(\"; processItem" + lItemTypeNoSpace + " Needs work\");");
           mGeneratedCodeFunctions.Add("                 return;");
           mGeneratedCodeFunctions.Add("             }");
           mGeneratedCodeFunctions.Add("");
           mGeneratedCodeFunctions.Add("        writeLineOutFile(\";Included Script\" + lLine.Substring(lEqualPos + 1));");
           mGeneratedCodeFunctions.Add("        //");
           mGeneratedCodeFunctions.Add("        //Read and discard end");
           mGeneratedCodeFunctions.Add("        lLine = mInFile.ReadLine();");
           mGeneratedCodeFunctions.Add("     }");
           mGeneratedCodeFunctions.Add("");
        }

        string lLine = mInFile.ReadLine();
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemCustomScriptItem(string aLine)
     {
        string lLine = "";
        //Read the next line
        lLine = mInFile.ReadLine();
        //This is end or Text=
        int lEqualPos = lLine.IndexOf('=');
        if (lEqualPos == -1)
        {
           //Just put in a blank line
           writeLineOutFile(";Did handle Custom Script Item Correctly");
           return;
        }

        writeLineOutFile(";Included Script" + lLine.Substring(lEqualPos + 1));
        //Read and discard end
        lLine = mInFile.ReadLine();
     }

     private void processItemRemark(string aLine)
     {
        string lLine = "";
        //Read the next line
        lLine = mInFile.ReadLine();
        //This is end or Text=
        int lEqualPos = lLine.IndexOf('=');
        if (lEqualPos == -1)
        {
           //Just put in a blank line
           writeLineOutFile("");
           return;
        }

        writeLineOutFile(getBasePath() + ";" + lLine.Substring(lEqualPos + 1));
        //Read and discard end
        lLine = mInFile.ReadLine();
     }

     private void processItemGlobal(string aLine)
     {
        //From Wise
        //EXE Filename=SortDirector.EXE
        //Version File=5.5.0.1900
        //Version Description=SortDirector
        string lLine = "";
        writeLineOutFile(getBasePath() + ";Global Item ignored");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("EXE Filename=");
           if (lPos != -1)
           {
              mEXEFilename = lLine.Substring(lPos + 13);
           }
           lPos = lLine.IndexOf("Version File=");
           if (lPos != -1)
           {
              mVersionFile = lLine.Substring(lPos + 13);
           }
           lPos = lLine.IndexOf("Version Description=");
           if (lPos != -1)
           {
              mVersionDescription = lLine.Substring(lPos + 20);
           }
        }
     }

     private void processItemSetVariable(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lVariableName = null;
        string lValue = null;
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Variable=");
           if (lPos != -1)
           {
              lVariableName = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Value=");
           if (lPos != -1)
           {
              lValue = lLine.Substring(lPos + 6);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }
        if (lVariableName != null && lValue != null)
        {
           //Flag Values Nothing, 
           //Increment,                     Flags=00000100
           //Decrement,                     Flags=00001000
           //Remove trailing backslashes,   Flags=00001100
           //Convert to long file name,     Flags=00010000
           //Convert to short file name,    Flags=00010100
           //Convert to upper case,         Flags=00011000 
           //Convert to lower case,         Flags=00011100 
           //Evaluate Expression,           Flags=00100000 
           //Append to existing Value,      Flags=00000001
           //Remove file name,              Flags=00000010
           //Read Variable from Values File Flags=10000000
           string lCommand = getBasePath();

           string lPostComment = ";Flags=" + lFlags;
           
           if (lFlags[7] == '1')
           {
              lCommand = lCommand + "StrCpy $" + lVariableName + " \'$" + lVariableName + lValue + "\' " + getComment("Doing an append to existing value");
              writeLineOutFile(lCommand);
           }
           else
           {
              lCommand = lCommand + "StrCpy $" + lVariableName + " '" + lValue + "'";
              writeLineOutFile(lCommand);
           }

           lCommand = getBasePath() + "${StrCase} '$" + lVariableName + "' '" + lVariableName + "' ";
           //Uppercase
           if (lFlags[3] == '1' && lFlags[4] == '1' && lFlags[5] == '0')
           {
              lCommand = lCommand + "U " + getComment("Convert to upper case");
              writeLineOutFile(lCommand);
           }
           //Lowercase
           if (lFlags[3] == '1' && lFlags[4] == '1' && lFlags[5] == '1')
           {
              //Not tested
              lCommand = lCommand + "L " + getComment("Convert to lower case");
              writeLineOutFile(lCommand);
           }
           addGlobalVariable(lVariableName);
        }
     }

     /// <summary>
     /// Creates a base path based on current nesting level and comment level
     /// </summary>
     /// <returns></returns>
     private string getBasePath()
     {
        string lBasePath = "";
        for (int lIndex = 0; lIndex < mNestingLevel; ++lIndex)
        {
           lBasePath = lBasePath + "    ";
        }

        if (mRemarkedItem == true)
        {
           lBasePath = lBasePath + ";";
        }

        return lBasePath;
     }


      /// <summary>
      /// Returns the comment only if comments are enabled.
      /// </summary>
      /// <param name="aComment"></param>
      /// <returns></returns>
     private string getComment(string aComment)
     {
        if (mEnableComments)
           return ";" + aComment;

        return "";
     }

     private void processItemCreateDirectory(string aLine)
     {
        string lCommand = "";

        //Read the next line
        string lLine = mInFile.ReadLine();

        int lEqualPos = lLine.IndexOf('=');
        if (lEqualPos != -1)
        {
           string lDirectory = lLine.Substring(lEqualPos + 1);
           lCommand = getBasePath() + "CreateDirectory '" + lDirectory + "'";

           writeLineOutFile(lCommand);
        }
        //
        //Read and discard end
        lLine = mInFile.ReadLine();
     }


     private void processItemGetSystemInformation(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemGetSystemInformation Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }


     private void processItemInsertLineintoTextFile(string aLine)
     {
        //nsislog::log $LOGFILE "This is the log file from the DirectorIT Installer setup" 

        //mGlobalVariables
        string lLine = "";
        string lPathname = null;
        string lNewTest = null;
        int lLineNumber = 0;
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("New Text=");
           if (lPos != -1)
           {
              lNewTest = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Line Number=");
           if (lPos != -1)
           {
              lLineNumber = int.Parse(lLine.Substring(lPos + 12));
           }
        }

        string lCommand = getBasePath() + "nsislog::log '" + lPathname + "' ";

        //Read the next line
        //This is end or Text=
        int lEqualPos = lLine.IndexOf('=');
        //lCommand = lCommand + "'" + lLine.Substring(lEqualPos + 1) + "' " + getComment(";WTON - Insert Line into Text File");
        lCommand = lCommand + "'" + lNewTest + "' " + getComment(";WTON - Insert Line into Text File");

        writeLineOutFile(lCommand);
     }


     private void processItemGetRegistryKeyValue(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lKey = "";
        string lValue = "";
        string lValueName = "";
        string lRoot = "";
        string lDefault = null;
        string lFlags = "00000000";
        string lVariable = "";


        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Key=");
           if (lPos != -1)
           {
              lKey = lLine.Substring(lPos + 4);
           }
           lPos = lLine.IndexOf("Value=");
           if (lPos != -1)
           {
              lValue = lLine.Substring(lPos + 6);
           }
           lPos = lLine.IndexOf("Variable=");
           if (lPos != -1)
           {
              lVariable = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Default=");
           if (lPos != -1)
           {
              lDefault = lLine.Substring(lPos + 8);
           }
           lPos = lLine.IndexOf("Value Name=");
           if (lPos != -1)
           {
              lValueName = lLine.Substring(lPos + 11);
           }
           lPos = lLine.IndexOf("Root=");
           if (lPos != -1)
           {
              lRoot = lLine.Substring(lPos + 5);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }
        switch (lFlags)
        {
           case "00000000": lRoot = "HKEY_CLASSES_ROOT"; break;
           case "00000010": lRoot = "HKEY_CURRENT_USER"; break;
           case "00000100": lRoot = "HKEY_LOCAL_MACHINE"; break;
           case "00000110": lRoot = "HKEY_USERS"; break;
           case "00001000": lRoot = "HKEY_CURRENT_CONFIG"; break;
        }

        addGlobalVariable(lVariable);

        //ReadRegStr $0 HKEY_CLASSES_ROOT subkey name
        string lCommand = getBasePath() + "ReadRegStr $" + lVariable + " \"" + lRoot + "\" \"" + lKey + "\" \"" + lValueName + "\"";
        writeLineOutFile(lCommand);

     }


     private void processItemOpenClose(string aLine)
     {
        //mGlobalVariables
        string lLine = aLine;
        string lPathname = "";
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = 0;
           lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        //If Equals                     Flags=00000000
        //Pause                         Flags=00000001
        //Open New Log                  Flags=00000010
        switch (lFlags)
        {
           //Open log file
           case "00000000":
              writeLineOutFile(getBasePath() + getComment("WISE - log file opened"));
              break;
           //Pause log file writing
           case "00000001":
              writeLineOutFile(getBasePath() + getComment("WISE - log file paused"));
              break;
           //Open new log file
           case "00000010":
              writeLineOutFile(getBasePath() + getComment("WISE - New log file opened File=") + lPathname);
              break;
        }
     }

      /// <summary>
      /// item: If/While Statement
      ///    Variable=COMLINE
      ///    Value= /NI
      ///    Flags=00000010
      /// end
      /// </summary>
      /// <param name="aLine"></param>
     private void processItemIfWhileStatement(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lVariable = "";
        string lValue = null;
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Variable=");
           if (lPos != -1)
           {
              lVariable = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Value=");
           if (lPos != -1)
           {
              lValue = lLine.Substring(lPos + 6);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        string lCommand = getBasePath();

        //If Equals                     Flags= 
        //If Not Equals                 Flags=00000001
        //If Contains                   Flags=00000010
        //If Does not Contain           Flags=00000011
        //If Equals Ignore Case         Flags=00000100
        //If Not Equal Ignore Case      Flags=00000101
        //If Greater Than               Flags=00000110
        //If Greater Than or Equal      Flags=00000111
        //If Less Than                  Flags=00001000
        //If Less Than or Equal         Flags=00001001
        //If Contains Any Letter In     Flags=00001010
        //If Contains Any Letter Not In Flags=00001011
        //If Length Equal To            Flags=00001100
        //If Expression True            Flags=00001101  Note - lVariable is not used for this
        //If Valid Password             Flags=00001110  Note - lVariable is not used for this
        //If Invalid Password           Flags=00001111  Note - lVariable is not used for this
        switch (lFlags)
        {
           //If Equals       ${If} $0 == 'x86'
           case "00000000":
              lCommand = lCommand + "${If} $" + lVariable + " == '" + lValue + "' " + getComment("If Equals");
              writeLineOutFile(lCommand);
              break;
           //If Not Equals
           case "00000001":
              lCommand = lCommand + "${If} $" + lVariable + " != '" + lValue + "' " + getComment("If Not Equals");
              writeLineOutFile(lCommand);
              break;
           //If Contains
           case "00000010":
              lCommand = lCommand + "${StrContains} $0 \"" + lValue + "\" $" + lVariable + getComment("If Contains");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 != \"\" " + getComment("If Not Equals");
              writeLineOutFile(lCommand);
              break;
           //If Does not Contain
           case "00000011":
              lCommand = lCommand + "${StrContains} $0 \"" + lValue + "\" $" + lVariable + getComment("Check it it contains");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 == \"\" " + getComment("If Does not Contain");
              writeLineOutFile(lCommand);
              break;
           //If Equals Ignore Case
           case "00000100":
              writeLineOutFile(getBasePath() + "${StrCase} $gWise1 '" + lValue + "' U ");
              writeLineOutFile(getBasePath() + "${StrCase} $gWise2 '$" + lVariable + "' U ");
              lCommand = lCommand + "${StrContains} $0 $gWise1 $gWise2" + getComment("If Equals Ignore Case");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 == \"\" " + getComment("If Equals Ignore Case");
              writeLineOutFile(lCommand);
              break;
           //If Not Equal Ignore Case
           case "00000101":
              writeLineOutFile(getBasePath() + "${StrCase} $gWise1 '" + lValue + "' U ");
              writeLineOutFile(getBasePath() + "${StrCase} $gWise2 '$" + lVariable + "' U ");
              lCommand = lCommand + "${StrContains} $0 $gWise1 $gWise2" + getComment("If Not Equal Ignore Case");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 != \"\" " + getComment("If Not Equal Ignore Case");
              writeLineOutFile(lCommand);
              break;
           //If Greater Than
           case "00000110":
              lCommand = lCommand + "${If} $" + lVariable + " > '" + lValue + "' " + getComment("If Greater Than");
              writeLineOutFile(lCommand);
              break;
           //If Greater Than or Equal
           case "00000111":
              lCommand = lCommand + "${If} $" + lVariable + " >= '" + lValue + "' " + getComment("If Greater Than or Equal");
              writeLineOutFile(lCommand);
              break;
           //If Less Than
           case "00001000":
              lCommand = lCommand + "${If} $" + lVariable + " < '" + lValue + "' " + getComment("If Less Than");
              writeLineOutFile(lCommand);
              break;
           //If Less Than or Equal
           case "00001001":
              lCommand = lCommand + "${If} $" + lVariable + " <= '" + lValue + "' " + getComment("If Less Than or Equal");
              writeLineOutFile(lCommand);
              break;
           //If Contains Any Letter In
           case "00001010":
              writeLineOutFile(getBasePath() + "${If} Contains Any Letter In " + lValue + " in " + lVariable);
              break;
           //If Contains Any Letter Not In
           case "00001011":              
              writeLineOutFile(getBasePath() + "${If} Contains " + lValue + " in " + lVariable);
              break;
           //If Length Equal To
           case "00001100":
              writeLineOutFile(getBasePath() + "If Length Equal of " + lVariable + " == " + lValue);
              break;
           //If Expression True -- Note - lVariable is not used for this
           case "00001101":
              writeLineOutFile(getBasePath() + "${If} Expression True " + lValue);
              break;
           //If Valid Password -- Note - lVariable is not used for this
           case "00001110":
              writeLineOutFile(getBasePath() + "${If} Valid Password");
              break;
           //If Invalid Password -- Note - lVariable is not used for this
           case "00001111":
              writeLineOutFile(getBasePath() + "${If} Invalid Password" + lValue + " in " + lVariable);
              break;
        }

        mNestingLevel = mNestingLevel + 1;
     }

     private void processItemAddTextto(string aLine)
     {
        //nsislog::log $LOGFILE "This is the log file from the DirectorIT Installer setup" 

        //Get the file name
        int lFileNamePos = aLine.IndexOf("Add Text to");
        string lFileName = aLine.Substring(lFileNamePos + 12);

        string lCommand = getBasePath() + "nsislog::log '" + lFileName + "' ";

        string lLine = mInFile.ReadLine();

        //Read the next line
        //This is end or Text=
        int lEqualPos = lLine.IndexOf('=');
        lCommand = lCommand + "'" + lLine.Substring(lEqualPos + 1) + "' " + getComment(";WTON - Add Test to");
 
        writeLineOutFile(lCommand);
        //
        //Read and discard end
        lLine = mInFile.ReadLine();
     }


     private void processItemElseStatement(string aLine)
     {
        mNestingLevel = mNestingLevel - 1;
        string lCommand = getBasePath() + "${Else}";
        mNestingLevel = mNestingLevel + 1;

        writeLineOutFile(lCommand);

        //
        //Read and discard end
        mInFile.ReadLine();
     }

     private void processItemEndBlock(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lFlags = "0";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        ////OK now check to see if there is an else if next
        //lLine = mInFile.ReadLine();

        ////We are looking for an elseif next
        //if (lLine.Contains("ElseIf"))
        //{
        //   //Wise is starting an else block next so don't unindent
        //   processLine(lLine);
        //   return;
        //}
        //else
        //{
           if (mRemarkedItem == false)
           {
              mNestingLevel = mNestingLevel - 1;
           }

           string lCommand = getBasePath() + "${EndIf}";
           writeLineOutFile(lCommand);
           //processLine(lLine);
        //}
     }


     private void processItemExitInstallation(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemExitInstallation Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemCheckifFileDirExists(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lPathname = null;
        string lMessage = "";
        string lTitle = "";
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Message=");
           if (lPos != -1)
           {
              lMessage = lLine.Substring(lPos + 8);
           }
           lPos = lLine.IndexOf("Title=");
           if (lPos != -1)
           {
              lTitle = lLine.Substring(lPos + 6);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        string lCommand = getBasePath();

        //If File or Directory Exist    Flags=00000000 - Check if file or directory exist and don't start if or while loop just display message
        //If File or Directory Exist    Flags=00000010 - Abort installation if file exists.
        //If File or Directory Exist    Flags=00000100 - Start if block
        //If File or Directory Exist    Flags=00000110 - Start While Loop
        //If File or Directory Exist    Flags=00100110 - Start While Loop - Preform loop once

        //If File or Directory !Exist   Flags=00000001 - Check if file or directory exist and don't start if or while loop just display message
        //If File or Directory !Exist   Flags=00000011 - Abort installation if file exists.
        //If File or Directory !Exist   Flags=00000101 - Start if block
        //If File or Directory !Exist   Flags=00000111 - Start While Loop
        //If File or Directory !Exist   Flags=00100111 - Start While Loop - Preform loop once

        //If File Exist                 Flags=01000000 - Check if file or directory exist and don't start if or while loop just display message
        //If File Exist                 Flags=01000010 - Abort installation if file exists.
        //If File Exist                 Flags=01000100 - Start if block
        //If File Exist                 Flags=01000110 - Start While Loop
        //If File Exist                 Flags=01100110 - Start While Loop - Preform loop once

        //If Directory Exist            Flags=01000001 - Check if file or directory exist and don't start if or while loop just display message
        //If Directory Exist            Flags=01000011 - Abort installation if file exists.
        //If Directory Exist            Flags=01000101 - Start if block
        //If Directory Exist            Flags=01000111 - Start While Loop
        //If Directory Exist            Flags=01100111 - Start While Loop - Preform loop once

        //If Directory Not Writable     Flags=10000000 - Check if file or directory exist and don't start if or while loop just display message
        //If Directory Not Writable     Flags=10000010 - Abort installation if file exists.
        //If Directory Not Writable     Flags=10000100 - Start if block
        //If Directory Not Writable     Flags=10000110 - Start While Loop
        //If Directory Not Writable     Flags=10100110 - Start While Loop - Preform loop once

        //If Module Loaded in Memory    Flags=10000001 - Check if file or directory exist and don't start if or while loop just display message
        //If Module Loaded in Memory    Flags=10000011 - Abort installation if file exists.
        //If Module Loaded in Memory    Flags=10000101 - Start if block
        //If Module Loaded in Memory    Flags=10000111 - Start While Loop
        //If Module Loaded in Memory    Flags=10100111 - Start While Loop - Preform loop once

        //If Preform Loop at least once Flags=00100000  Must preform loop once


        //NSIS code
//${If} ${FileExists} `$INSTDIR\file\*.*`
//  ; file is a directory
//${ElseIf} ${FileExists} `$INSTDIR\file`
//  ; file is a file
//${Else}
//  ; file is neither a file or a directory (i.e. it doesn't exist)
//${EndIf}
        switch (lFlags)
        {
           //If File Exist
           case "01000100":
           case "00000100":
              lCommand = lCommand + "${If} ${FileExists} '" + lPathname + "'" + getComment("If File Exist");
              writeLineOutFile(lCommand);
              break;
           case "01000101":
              lCommand = lCommand + "${If} ${FileExists} '" + lPathname + "*.*'" + getComment("If Directory Exist");
              writeLineOutFile(lCommand);
              break;
           case "00000101":
              lCommand = lCommand + "${If} !${FileExists} '" + lPathname + "*.*'" + getComment("If Directory Exist");
              writeLineOutFile(lCommand);
              break;
           case "10000100":
              lCommand = getBasePath() + ";Directory not writable check - remove it from script";
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} '1' == '2' " + getComment("Directory not writable check - remove it from script");
              writeLineOutFile(lCommand);
              break;
           default:
              lCommand = getBasePath() + ";TODO: Code not implemented for If file or directory exist for flags = " + lFlags;
              writeLineOutFile(lCommand);
              System.Diagnostics.Debug.Assert(false, "Need to add support for CheckIfFileDirExist");
              break;
        }

        mNestingLevel = mNestingLevel + 1;
        System.Diagnostics.Debug.Assert(mNestingLevel >= 0);
     }


     private void processItemEditRegistry(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lKey = null;
        string lNewValue = null;
        string lValueName = null;
        string lRoot = null;
        
        string lTotalKeys = mInFile.ReadLine();
        int lPos = lTotalKeys.IndexOf('=');
        lTotalKeys = lTotalKeys.Substring(lPos+1);
        int lKeyCount = int.Parse(lTotalKeys);

        while (mInFile.Peek() >= 0 && (lLine != "end" || lKeyCount > 0))
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           lPos = lLine.IndexOf("Key=");
           if (lPos != -1)
           {
              lKey = lLine.Substring(lPos + 4);
           }
           lPos = lLine.IndexOf("New Value=");
           if (lPos != -1)
           {
              lNewValue = lLine.Substring(lPos + 10);
           }
           lPos = lLine.IndexOf("Value Name=");
           if (lPos != -1)
           {
              lValueName = lLine.Substring(lPos + 11);
           }
           lPos = lLine.IndexOf("Root=");
           if (lPos != -1)
           {
              lRoot = lLine.Substring(lPos + 5);
           }
           if (lKey != null && lNewValue != null && lValueName != null && lRoot != null)
           {
              switch(lRoot)
              {
                 case "0":lRoot = "HKEY_CLASSES_ROOT";  break;
                 case "1":lRoot = "HKEY_CURRENT_USER";  break;
                 case "2":lRoot = "HKEY_LOCAL_MACHINE"; break;
                 case "3":lRoot = "HKEY_USERS";         break;
                 case "4":lRoot = "HKEY_CURRENT_CONFIG";break;
              }
              lKeyCount = lKeyCount -1;
              string lCommand = getBasePath() + "WriteRegStr " + lRoot + " \"" + lKey + "\" \"" + lValueName + "\" \"" + lNewValue + "\"";
              writeLineOutFile(lCommand);
           
              lKey = null;
              lNewValue = null;
              lValueName = null;
              lRoot = null;
           }
        }
     }

     private void processItemExecuteProgram(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lPathname = null;
        string lCommandLine = "";
        string lVariablesAdded = "";
        string lDefaultDirectory = "";
        string lFlags = "00000000";

        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Command Line=");
           if (lPos != -1)
           {
              lCommandLine = lLine.Substring(lPos + 13);
           }
           lPos = lLine.IndexOf("Variables Added=");
           if (lPos != -1)
           {
              lVariablesAdded = lLine.Substring(lPos + 16);
           }
           lPos = lLine.IndexOf("Default Directory=");
           if (lPos != -1)
           {
              lDefaultDirectory = lLine.Substring(lPos + 18);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        if (lPathname != null )
        {
           //Normal, Don't wait,            Flags=00000000
           //Wait for program to exit.      Flags=00000010
           //Maximized,                     Flags=00000001
           //Minimized,                     Flags=00000100
           //Hidden,                        Flags=00001000
           bool lWaitFor =   (lFlags[6] == '1');
           bool lMaximized = (lFlags[7] == '1');
           bool lMinimized =  (lFlags[5] == '1');
           bool lHidden =     (lFlags[4] == '1');

           string lPostComment = ";Flags=" + lFlags;

//Exec command Execute the specfied program and continue immediately. Note that the file specified must exist on the target system, not the compiling system. $OUTDIR is used for the working directory. The error flag is set if the process could not be launched. Note, if the command could have spaces, you may with to put it in quotes to delimit it from parameters. i.e.: Exec '"$INSTDIR\command.exe" parameters'.  
//ExecWait command
//[user_var(exit code)] Execute the specfied program and wait for the executed process to quit. See Exec for more information. If no output variable is specified ExecWait sets the error flag if the program executed returns a nonzero error code, or if there is an error. If an output variable is specified, ExecWait sets the variable with the exit code (and only sets the error flag if an error occurs; if an error occurs the contents of the user variable are undefined). Note, if the command could have spaces, you may with to put it in quotes to delimit it from parameters. i.e.: ExecWait '"$INSTDIR\command.exe" parameters'  
//ExecShell action
//command
//[parameters]
//[SW_SHOWNORMAL |
// SW_SHOWMAXIMIZED |
// SW_SHOWMINIMIZED] 
//Execute the specfied program using ShellExecute. Note that action is usually "open", "print", etc, but can be an empty string to use the default action. Parameters and the show type are optional. $OUTDIR is used for the working directory. The error flag is set if the process could not be launched.  

           string lCommand = "";
           if (lDefaultDirectory.Length > 0)
           {
              lCommand = getBasePath() + "StrCpy $OUTDIRTemp $OUTDIR " + getComment("Saving outdir");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "StrCpy $OUTDIR \'" + lDefaultDirectory + "\' " + getComment("Setting the working directory for exec command");
              writeLineOutFile(lCommand);
           }

           if (lMaximized)
           {
              lCommand = getBasePath() + ";Maximized flags was ignored";
              writeLineOutFile(lCommand);
           }
           if (lMinimized)
           {
              lCommand = getBasePath() + ";Minimized flags was ignored";
              writeLineOutFile(lCommand);
           }
           if (lHidden)
           {
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + ";Hidden flags was ignored";
           }

           if (lWaitFor)
           {
              lCommand = getBasePath() + "ExecWait '\"" + lPathname + "\" " + lCommandLine + "'" + getComment("Saving outdir");
              writeLineOutFile(lCommand);
           }

           if (lDefaultDirectory.Length > 0)
           {
              lCommand = lCommand + "StrCpy $OUTDIR $OUTDIRTemp " + getComment("Restore outdir");
              writeLineOutFile(lCommand);
           }
        }
     }

     private void processItemInstallFile(string aLine)
     {
        string lCommand = "";
        //mGlobalVariables
        string lLine = "";
        string lSource = null;
        string lDestination = null;
        string lFlags = "0000000000000000";
        string lExtraFlags = "00000000";
        string lDescription = "";
        bool lRecursive = false;
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Source=");
           if (lPos != -1)
           {
              lSource = lLine.Substring(lPos + 7);
              continue;
           }
           lPos = lLine.IndexOf("Description=");
           if (lPos != -1)
           {
              lDescription = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Destination=");
           if (lPos != -1)
           {
              lDestination = lLine.Substring(lPos + 12);
              //lDestination will always have a back slash on it unless it is a file so
              //just always remove it
              char[] lDelimiters = new char[2] {'\\','/'};
              int lEndOfFolder = lDestination.LastIndexOfAny(lDelimiters);
              if(lEndOfFolder != -1)
              {
                 string lTempOut = lDestination.Substring(0,lEndOfFolder);
                 if(lTempOut != mCurrentSetOutPath)
                 {
                    lCommand = getBasePath() + "SetOutPath '" + lTempOut + "' " + getComment("WTON - Changing out dir because it was different");
                    writeLineOutFile(lCommand);
                    mCurrentSetOutPath = lTempOut;
                 }
              }
              continue;
           }
           lPos = lLine.IndexOf("Extra Flags=");
           if (lPos != -1)
           {
              lExtraFlags = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);

              lRecursive = lFlags[7] == '1';

              //If bit 13 is set then set overwrite is off
              if(lFlags[12] == '1')
              {
                 if(mOverwriteFlag == true)
                 {
                    mOverwriteBeenSet = true;
                    mOverwriteFlag = true;
                    //Need to enable
                    lCommand = getBasePath() + "SetOverwrite on " + getComment("WTON");
                    writeLineOutFile(lCommand);
                 }
              }
              else
              {
                 if(mOverwriteFlag == false)
                 {
                    mOverwriteBeenSet = true;
                    mOverwriteFlag = false;
                    //Need to disable
                    lCommand = getBasePath() + "SetOverwrite off " + getComment("WTON");
                    writeLineOutFile(lCommand);
                 }
              }
              continue;
           }
        }

        if (lSource != null && lDestination != null)
        {
           //Default 0000000010000010 is Password Required and Always

           //Flag Values Nothing, 
           //RequirePassword,               Flags=0000000010000010
           //Include Sub-Directories        Flags=0000000110000010  Bit 8
           //Shared DLL Counter             Flags=0000001010000010  Bit 7
           //No Progress Bar                Flags=0000000010100010  Bit 11
           //Self Registry OCX/DLL/EXE/TLB  Flags=0001000010000010  Bit 4
           
           //Replace Options
           //  Always                       Flags=0000000010000010  This is the default
           //  Never                        Flags=0000000010001010  Bit 13
           //  Version Doesn't Matter       Flags=0000000010000010
           //          Same/Older           Flags=0000000010000011  Bit 16
           //          Older                Flags=0000000010000011  Extra Flags=00000001
           //  Time    Doesn't Matter       Flags=0000000010000010
           //          Same/Older           Flags=0000000010010010  Bit 12
           //          Older                Flags=0000010010010010  Bit 6

           //Repair application if missing  Flags=0000000010000010 Extra Flags=00000010 


           if (mOverwriteBeenSet == false)
           {
              //Need to enable
              lCommand = getBasePath() + "SetOverwrite on " + getComment("WTON");
              writeLineOutFile(lCommand);
              mOverwriteBeenSet = true;
           }
           lCommand = getBasePath() + "File ";
           if (lRecursive)
           {
              lCommand = lCommand + " /r /x .svn ";
           }
           lCommand = lCommand + "'" + lSource + "' " + getComment("WTON");

            writeLineOutFile(lCommand);
        }
     }


     private void processItemSelfRegisterOCXsDLLs(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemSelfRegisterOCXsDLLs Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }


     private void processItemNewEvent(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemNewEvent Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void checkBoxEnableComments_CheckedChanged(object sender, EventArgs e)
     {
        if (checkBoxEnableComments.Checked == true)
        {
           mEnableComments = true;
        }
        else
        {
           mEnableComments = false;
        }
     }

     private void processItemGetTemporaryFilename(string aLine)
     {
        string lLine = "";
        //Read the next line
        lLine = mInFile.ReadLine();
        //This is end or Text=
        int lEqualPos = lLine.IndexOf('=');
        if (lEqualPos == -1)
        {
           //Just put in a blank line
           writeLineOutFile("");
           return;
        }

        string lVariableName = lLine.Substring(lEqualPos + 1);

        this.addGlobalVariable(lVariableName);

        writeLineOutFile(getBasePath() + "GetTempFileName $" + lVariableName);
        
        //Read and discard end
        lLine = mInFile.ReadLine();
     }

     int zprocessItemParseStringCount = 0;
     private void processItemParseString(string aLine)
     {
        zprocessItemParseStringCount = zprocessItemParseStringCount + 1;
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemParseString Count=" + zprocessItemParseStringCount.ToString() + " Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemCheckConfiguration(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemCheckConfiguration Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemReadINIValue(string aLine)
     {
         //item: Read INI Value
         //  Variable=NAME
         //  Pathname=%INST%\CUSTDATA.INI
         //  Section=Registration
         //  Item=Name
         //end
        //NSIS ReadINIStr $OUTVALUE 'filename.ini' 'Section' 'Key'

        //mGlobalVariables
        string lLine = "";
        string lVariable = "";
        string lPathname = "";
        string lSection = "";
        string lItem = "";
        string lDefaultValue = "";
        string lFlags = "00000000";

        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Variable=");
           if (lPos != -1)
           {
              lVariable = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Section=");
           if (lPos != -1)
           {
              lSection = lLine.Substring(lPos + 8);
           }
           lPos = lLine.IndexOf("Item=");
           if (lPos != -1)
           {
              lItem = lLine.Substring(lPos + 5);
           }
           lPos = lLine.IndexOf("DefaultValue=");
           if (lPos != -1)
           {
              lDefaultValue = lLine.Substring(lPos + 13);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        addGlobalVariable(lVariable);

        //Normal, Don't wait,            Flags=00000000
        //Remove File Name,              Flags=00000001
        bool lRemoveFileName = (lFlags[7] == '1');

        string lPostComment = ";Flags=" + lFlags;

        string lCommand = "";
        if (lDefaultValue.Length > 0)
        {
           writeLineOutFile(getBasePath() + "ClearErrors" + getComment("Clearing errors so a default value can be set"));
        }
                                                                       //File                Section          Key
        lCommand = getBasePath() + "ReadINIStr $" + lVariable + " '" + lPathname + "' '" + lSection + "' '" + lItem + "'";
        writeLineOutFile(lCommand);

        if (lDefaultValue.Length > 0)
        {
           //The user wanted to set a default value
           writeLineOutFile(getBasePath() + "${If} ${Errors}");
           writeLineOutFile(getBasePath() + "   StrCpy $" + lVariable + " '" + lDefaultValue + "'");
           writeLineOutFile(getBasePath() + "${EndIf}");
        }
     }

     private void processItemWizardBlock(string aLine)
     {
        string lCommand = "";
        //TODO:  The wizard loop block is pretty much just commented out
        lCommand = getBasePath() + ";The wizard loop block is pretty much just commented out - lots of work here if someone wants to do it";
        writeLineOutFile(lCommand);
        lCommand = getBasePath() + "${If} '1' == '2' " + getComment("Wizard Loop Start");
        writeLineOutFile(lCommand);
        mNestingLevel = mNestingLevel + 1;
     }

     private void processItemCustomDialogSet(string aLine)
     {
        string lLine = aLine;
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
        writeLineOutFile(getBasePath() + "; processItemCustomDialogSet Needs work");
     }

     private void processItemEditINIFile(string aLine)
     {
        //WriteINIStr $TEMP\something.ini section1 something 123

         //item: Edit INI File
         //  Pathname=%INST%\CUSTDATA.INI
         //  Settings=[Registration]
         //  Settings=NAME=%NAME%
         //  Settings=COMPANY=%COMPANY%
         //  Settings=
         //end

        string lLine = "";
        string lPathname = "";
        string lSettings = "00000000";
        string lSection = "";

        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Settings=");
           if (lPos != -1)
           {
              lSettings = lLine.Substring(lPos + 9);
              if (lSettings.Length > 0)
              {
                 if (lSettings[0] == '[')
                 {
                    lSection = lSettings.Substring(1,lSettings.Length - 2);
                 }
                 else
                 {
                    lLine = lLine.Trim();
                    int lStartPos = lLine.IndexOf("=");
                    int lEndPos = lLine.IndexOf("=",lStartPos+1);
                    if (lStartPos != -1 && lEndPos != -1)
                    {
                       string lKey = lLine.Substring(lStartPos + 1, lEndPos - lStartPos -1);
                       string lValue = lLine.Substring(lEndPos + 1);
                       string lCommand = getBasePath();
                       lCommand = lCommand + "WriteINIStr \"" + lPathname + "\" \"" + lSection + "\" \"" + lKey + "\" \"" + lValue + "\"";
                       writeLineOutFile(lCommand);
                    }
                 }
              }
           }
        }
     }

     private void processItemCheckDiskSpace(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemCheckDiskSpace Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemDeleteFile(string aLine)
     {
         //item: Delete File
         //   Pathname=%MAINDIR%\Products\SortDirector\03-Development\Code\Source\DematicFramework\build
         //   Flags=00001100
         //end
         //NSIS Delete "FileName"
        //mGlobalVariables
        string lLine = "";
        string lPathname = null;
        string lFlags = "00000000";

        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Pathname=");
           if (lPos != -1)
           {
              lPathname = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        if (lPathname != null)
        {
           //Delete File or directory          Flags=00000000
           //Include Sub-Directories           Flags=00001000
           //Remove Directory Containing Files Flags=00000100
           bool lRecurs = (lFlags[7] == '4');
           bool lRemoveFolder = (lFlags[5] == '5');

           if (lRecurs && lRemoveFolder)
           {
              string lCommand = getBasePath() + "RMDir /r \"" + lPathname + "\"" + getComment("Remove folder flags = " + lFlags);
              writeLineOutFile(lCommand);
           }
           else if (lRecurs)
           {
              string lCommand = getBasePath() + "TODO:  This needs to delete all files but it doesn't work";
              writeLineOutFile(lCommand);
              //string lCommand = getBasePath() + "Delete " + lPathname + getComment("Delete File with flags = " + lFlags);
              //writeLineOutFile(lCommand);
           }
           else if (lRemoveFolder)
           {
              string lCommand = getBasePath() + "RMDir /r \"" + lPathname + "\"" + getComment("Remove folder flags = " + lFlags);
              writeLineOutFile(lCommand);
           }
           else
           {
              string lCommand = getBasePath() + "Delete \"" + lPathname + "\"" +getComment("Delete File with flags = " + lFlags);
              writeLineOutFile(lCommand);
           }
        }

     }

     private void processItemDisplayGraphic(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemDisplayGraphic Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void processItemCopyLocalFile(string aLine)
     {
        //NSIS CopyFiles /SILENT /FILESONLY folder\*.exe "path only"
        string lCommand = "";
        //mGlobalVariables
        string lLine = "";
        string lSource = null;
        string lDestination = null;
        string lFlags = "0000000000000000";
        string lExtraFlags = "00000000";
        string lLocalPath = "";
        string lDescription = "";
        bool lRecursive = false;
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Source=");
           if (lPos != -1)
           {
              lSource = lLine.Substring(lPos + 7);
              continue;
           }
           lPos = lLine.IndexOf("Description=");
           if (lPos != -1)
           {
              lDescription = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Destination=");
           if (lPos != -1)
           {
              lDestination = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Extra Flags=");
           if (lPos != -1)
           {
              lExtraFlags = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Local Path=");
           if (lPos != -1)
           {
              lLocalPath = lLine.Substring(lPos + 12);
              continue;
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
              lRecursive = lFlags[7] == '1';
              continue;
           }
        }

        if (lSource != null && lDestination != null)
        {
           //Default 0000000010000010 is Password Required and Always

           //Flag Values Nothing, 
           //RequirePassword,               Flags=0000000010000010
           //Include Sub-Directories        Flags=0000000110000010  Bit 8  
           //Shared DLL Counter             Flags=0000001010000010  Bit 7
           //No Progress Bar                Flags=0000000010100010  Bit 11
           //Self Registry OCX/DLL/EXE/TLB  Flags=0001000010000010  Bit 4

           //Replace Options
           //  Always                       Flags=0000000010000010  This is the default
           //  Never                        Flags=0000000010001010  Bit 13
           //  Version Doesn't Matter       Flags=0000000010000010
           //          Same/Older           Flags=0000000010000011  Bit 16
           //          Older                Flags=0000000010000011  Extra Flags=00000001
           //  Time    Doesn't Matter       Flags=0000000010000010
           //          Same/Older           Flags=0000000010010010  Bit 12
           //          Older                Flags=0000010010010010  Bit 6

           //Repair application if missing  Flags=0000000010000010 Extra Flags=00000010 


         if (lRecursive)
         {
            //ExecWait 'RoboCopy "c:\" "d:\" "*.*" E' $0
            int lPos = lSource.LastIndexOf('\\');
            if(lPos != -1)
            {
               string lSourcePath = lSource.Substring(0,lPos);
               string lWildCard = lSource.Substring(lPos + 1);
               lCommand = getBasePath() + "ExecWait 'RoboCopy \"" + lSourcePath + "\" ";
               writeLineOutFile(lCommand);
               lCommand = getBasePath() + "                   \"" + lDestination + "\" \"" + lWildCard + "\" E' $0" + getComment("CopyFiles recurs");
               writeLineOutFile(lCommand);
            }
            else
            {
               writeLineOutFile(getBasePath() + "CopyFile not handled - there are is file spec");
            }
         }
         else
         {
            lCommand = getBasePath() + "CopyFiles '" + lSource + "' '" + lDestination + "'" + getComment("CopyFiles no recurs");
            writeLineOutFile(lCommand);
         }
        }
     }

     private void processItemReadUpdateTextFile(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + "${If} \"0\" == \"1\" ;processItemReadUpdateTextFile Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
        mNestingLevel = mNestingLevel + 1;
     }

     private void processItemDialog(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemDialog Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }

     private void writeLineOutFile(string aLine)
     {
        string lLine = "";
        string lTemp = "";
        int lFirstPercentIndex = -1;
        int lSecondPercentIndex = -1;
        for(int lIndex = 0; lIndex < aLine.Length; ++lIndex)
        {
           char lChar = aLine[lIndex];
           if (lChar == '%' && lFirstPercentIndex == -1)
           {
              lFirstPercentIndex = lIndex;
              continue;
           }
           if (lChar == '%' && lFirstPercentIndex != -1 && lSecondPercentIndex == -1)
           {
              lSecondPercentIndex = lIndex;
              //Now some validation
              if (lFirstPercentIndex + 1 == lSecondPercentIndex)
              {
                 lLine = lLine + "%%";
                 lFirstPercentIndex = -1;
                 lSecondPercentIndex = -1;
                 lTemp = "";
                 continue;
              }
              if (aLine[lFirstPercentIndex + 1] == '(')
              {
                 lLine = lLine + "%" + lTemp + "%";
                 lFirstPercentIndex = -1;
                 lSecondPercentIndex = -1;
                 lTemp = "";
                 continue;
              }
              
                 lLine = lLine + "$" + lTemp;
                 lFirstPercentIndex = -1;
                 lSecondPercentIndex = -1;
                 lTemp = "";
                 continue;
           }

           if (lFirstPercentIndex != -1)
           {
              lTemp = lTemp + lChar;
              continue;
           }
           lLine = lLine + lChar;
        }
        if (aLine.Contains('%'))
        {
           int a = 1;
        }
        else
        {
           System.Diagnostics.Debug.Assert(aLine == lLine);
        }
        mOutFile.WriteLine(lLine);
     }

     private void processItemIncludeScript(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemIncludeScript Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }


     int z_processItemDisplayMessageCount = 0;
     private void processItemDisplayMessage(string aLine)
     {
        string lLine = "";
        int lPos = 0;
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }
        z_processItemDisplayMessageCount = z_processItemDisplayMessageCount + 1;
        string lCommand = getBasePath();
        if(lFlags[7] == '1')
        {
           //This starts an if block
            lCommand = lCommand + "${If} \"0\" == \"" + z_processItemDisplayMessageCount + "\" ;processItemDisplayMessage needs work";
           mNestingLevel = mNestingLevel + 1;
        }
        else
        {
           lCommand = lCommand + " ;processItemDisplayMessage needs work Count=" + z_processItemDisplayMessageCount.ToString();
        }
        writeLineOutFile(lCommand);

     }   

     private void processItemElseIfStatement(string aLine)
     {
        mNestingLevel = mNestingLevel - 1;
        //mGlobalVariables
        string lLine = "";
        string lVariable = "";
        string lValue = null;
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Variable=");
           if (lPos != -1)
           {
              lVariable = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Value=");
           if (lPos != -1)
           {
              lValue = lLine.Substring(lPos + 6);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        string lCommand = getBasePath();

        //If Equals                     Flags= 
        //If Not Equals                 Flags=00000001
        //If Contains                   Flags=00000010
        //If Does not Contain           Flags=00000011
        //If Equals Ignore Case         Flags=00000100
        //If Not Equal Ignore Case      Flags=00000101
        //If Greater Than               Flags=00000110
        //If Greater Than or Equal      Flags=00000111
        //If Less Than                  Flags=00001000
        //If Less Than or Equal         Flags=00001001
        //If Contains Any Letter In     Flags=00001010
        //If Contains Any Letter Not In Flags=00001011
        //If Length Equal To            Flags=00001100
        //If Expression True            Flags=00001101  Note - lVariable is not used for this
        //If Valid Password             Flags=00001110  Note - lVariable is not used for this
        //If Invalid Password           Flags=00001111  Note - lVariable is not used for this
        switch (lFlags)
        {
           //If Equals       ${If} $0 == 'x86'
           case "00000000":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " == '" + lValue + "' " + getComment("If Equals");
              writeLineOutFile(lCommand);
              break;
           //If Not Equals
           case "00000001":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " != '" + lValue + "' " + getComment("If Not Equals");
              writeLineOutFile(lCommand);
              break;
           //If Contains
           case "00000010":
              lCommand = lCommand + "${Else}";
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${StrContains} $0 \"" + lValue + "\" $" + lVariable + getComment("If Contains");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 != \"\" " + getComment("If Not Equals");
              writeLineOutFile(lCommand);
              break;
           //If Does not Contain
           case "00000011":
              lCommand = lCommand + "${Else}";
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${StrContains} $0 \"" + lValue + "\" $" + lVariable + getComment("Check it it contains");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 == \"\" " + getComment("If Does not Contain");
              writeLineOutFile(lCommand);
              break;
           //If Equals Ignore Case
           case "00000100":
              lCommand = lCommand + "${Else}";
              writeLineOutFile(lCommand);
              writeLineOutFile(getBasePath() + "${StrCase} $gWise1 '" + lValue + "' U ");
              writeLineOutFile(getBasePath() + "${StrCase} $gWise2 '$" + lVariable + "' U ");
              lCommand = lCommand + "${StrContains} $0 $gWise1 $gWise2" + getComment("If Equals Ignore Case");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 == \"\" " + getComment("If Equals Ignore Case");
              writeLineOutFile(lCommand);
              break;
           //If Not Equal Ignore Case
           case "00000101":
              lCommand = lCommand + "${Else}";
              writeLineOutFile(lCommand);
              writeLineOutFile(getBasePath() + "${StrCase} $gWise1 '" + lValue + "' U ");
              writeLineOutFile(getBasePath() + "${StrCase} $gWise1 '$" + lVariable + "' U ");
              lCommand = lCommand + "${StrContains} $0 $gWise1 $gWise2" + getComment("If Not Equal Ignore Case");
              writeLineOutFile(lCommand);
              lCommand = getBasePath() + "${If} $0 != \"\" " + getComment("If Not Equal Ignore Case");
              writeLineOutFile(lCommand);
              break;
           //If Greater Than
           case "00000110":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " > '" + lValue + "' " + getComment("If Greater Than");
              writeLineOutFile(lCommand);
              break;
           //If Greater Than or Equal
           case "00000111":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " >= '" + lValue + "' " + getComment("If Greater Than or Equal");
              writeLineOutFile(lCommand);
              break;
           //If Less Than
           case "00001000":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " < '" + lValue + "' " + getComment("If Less Than");
              writeLineOutFile(lCommand);
              break;
           //If Less Than or Equal
           case "00001001":
              lCommand = lCommand + "${ElseIf} $" + lVariable + " <= '" + lValue + "' " + getComment("If Less Than or Equal");
              writeLineOutFile(lCommand);
              break;
           //If Contains Any Letter In
           case "00001010":
              writeLineOutFile(";ElseIf Contains Any Letter In");
              break;
           //If Contains Any Letter Not In
           case "00001011":
              writeLineOutFile(";ElseIf Contains Any Letter Not In");
              break;
           //If Length Equal To
           case "00001100":
              writeLineOutFile(";ElseIf Length Equal To");
              break;
           //If Expression True -- Note - lVariable is not used for this
           case "00001101":
              writeLineOutFile(";ElseIf Expression True");
              break;
           //If Valid Password -- Note - lVariable is not used for this
           case "00001110":
              writeLineOutFile(";ElseIf Valid Password");
              break;
           //If Invalid Password -- Note - lVariable is not used for this
           case "00001111":
              writeLineOutFile(";ElseIf Invalid Password");
              break;
        }
        mNestingLevel = mNestingLevel + 1;
     }

     private void processItemStartStopService(string aLine)
     {
        //mGlobalVariables
        string lLine = "";
        string lServiceName = null;
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Service Name=");
           if (lPos != -1)
           {
              lServiceName = lLine.Substring(lPos + 13);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }
           //Start,                     Flags=00000000
           //Stop,                      Flags=00000001
        string lCommand = getBasePath();
        if(lFlags == "00000000")
        {
            lCommand = lCommand + "SimpleSC::StartService ";
        }
        if(lFlags == "00000001")
        {
            lCommand = lCommand + "SimpleSC::StopService ";
        }

        lCommand = lCommand + "\"" + lServiceName + "\"";
        writeLineOutFile(lCommand);
        
     } 

     private void processItemGetEnvironmentVariable(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemGetEnvironmentVariable Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     } 

     private void processItemSetFileAttributes(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemInstallODBCDriver Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }  

     private void processItemInstallODBCDriver(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemInstallODBCDriver Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }     

     private void processItemConfigureODBCDataSource(string aLine)
     {
        string lLine = "";
        writeLineOutFile(getBasePath() + ";processItemConfigureODBCDataSource Needs work");
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
        }
     }  

     private void processItemCreateShortcut(string aLine)
     {
      //CreateDirectory "$SMPROGRAMS\My Company"
      //CreateShortCut "$SMPROGRAMS\My Company\My Program.lnk" "$INSTDIR\My Program.exe" \
      //  "some command line parameters" "$INSTDIR\My Program.exe" 2 SW_SHOWNORMAL \
      //  ALT|CONTROL|SHIFT|F5 "a description"

      //item: Create Shortcut
      //  Source=%MAINDIR%\ResourceKit\Document\Product\SortDirector_Product_Change_Request_Form.pdf
      //  Destination=%GROUP%\Documents\Product Change Request Form.lnk
      //  Icon Number=0
      //  Key Type=1536
      //  Flags=00000001
      //end

        //mGlobalVariables
        string lLine = "";
        string lSource = "";
        string lDestination = "";
        string lIConNumber = "";
        string lKeyType = "";
        string lFlags = "00000000";
        while (mInFile.Peek() >= 0 && lLine != "end")
        {
           //Read the next line
           lLine = mInFile.ReadLine();
           int lPos = lLine.IndexOf("Source=");
           if (lPos != -1)
           {
              lSource = lLine.Substring(lPos + 7);
           }
           lPos = lLine.IndexOf("Destination=");
           if (lPos != -1)
           {
              lDestination = lLine.Substring(lPos + 12);
           }
           lPos = lLine.IndexOf("ICon Number=");
           if (lPos != -1)
           {
              lIConNumber = lLine.Substring(lPos + 12);
           }
           lPos = lLine.IndexOf("Key Type=");
           if (lPos != -1)
           {
              lKeyType = lLine.Substring(lPos + 9);
           }
           lPos = lLine.IndexOf("Flags=");
           if (lPos != -1)
           {
              lFlags = lLine.Substring(lPos + 6);
           }
        }

        string lCommand = "";
        lCommand = getBasePath() + "CreateDirectory \"" + lDestination + "\"";
        writeLineOutFile(lCommand);
        lCommand = getBasePath() + "CreateShortCut \"" + lDestination + "\" \"" + lSource +"\""; ;
        writeLineOutFile(lCommand);

     }

     private void buttonTemplate_Click(object sender, EventArgs e)
     {
        OpenFileDialog lFBD = new OpenFileDialog();
        lFBD.Title = "Select Template NSIS File";
        lFBD.Filter = "nsi files (*.nsi)All files (*.*)|*.*";
        lFBD.FilterIndex = 1;
        lFBD.InitialDirectory = System.Environment.CurrentDirectory;
        string lExpandedValue = textBoxWise.Text;
        lFBD.FileName = lExpandedValue;
        if (lFBD.ShowDialog() == DialogResult.OK)
        {
           string lNewDITPath = lFBD.FileName;
           textBoxTemplate.Text = lNewDITPath;
        }
     }

     private void textBoxWise_TextChanged(object sender, EventArgs e)
     {
        textBoxNSIS.Text = textBoxWise.Text.Substring(0, textBoxWise.Text.Length - 4) + ".nsi";
     }

     private void label4_Click(object sender, EventArgs e)
     {

     }

     private void textBoxTemplate_TextChanged(object sender, EventArgs e)
     {

     }

     private void label3_Click(object sender, EventArgs e)
     {

     }

     private void checkBoxCodeGenerator_CheckedChanged(object sender, EventArgs e)
     {

     }

     private void textBoxNSIS_TextChanged(object sender, EventArgs e)
     {

     }
   }
}
