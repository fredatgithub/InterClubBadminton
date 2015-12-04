/*
The MIT License(MIT)
Copyright(c) 2015 Freddy Juhel
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using InterClubBadminton.Properties;
using Tools;
namespace InterClubBadminton
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

    public readonly Dictionary<string, string> _languageDicoEn = new Dictionary<string, string>();
    public readonly Dictionary<string, string> _languageDicoFr = new Dictionary<string, string>();
    private string _currentLanguage = "english";
    private ConfigurationOptions _configurationOptions = new ConfigurationOptions();
    private bool _teamMembersCreated;
    private bool _visualizeTeamLoaded = false;

    private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SaveWindowValue();
      Application.Exit();
    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AboutBoxApplication aboutBoxApplication = new AboutBoxApplication();
      aboutBoxApplication.ShowDialog();
    }

    private void DisplayTitle()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      Text += string.Format(" V{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
      LoadSettingsAtStartup();
    }

    private void LoadSettingsAtStartup()
    {
      DisplayTitle();
      GetWindowValue();
      LoadLanguages();
      SetLanguage(Settings.Default.LastLanguageUsed);
      LoadCombobox(comboBoxSex, Enum.GetNames(typeof(Gender)));
      LoadSeveralComboBoxesWithXmlFile(new List<ComboBox> { comboBoxSimple, comboBoxDouble, comboBoxMixed },
        "Resources/Points.xml", "point", "name", "value");
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed);

    }

    private static void LoadCombobox(ComboBox cb, IEnumerable<string> collectionStrings)
    {
      cb.Items.Clear();
      foreach (string item in collectionStrings)
      {
        cb.Items.Add(item);
      }

      cb.SelectedIndex = 0;
    }

    private void LoadSeveralComboBoxesWithXmlFile(IEnumerable<ComboBox> listOfComboBoxs,
      string filename, params string[] tags)
    {
      foreach (ComboBox comboBox in listOfComboBoxs)
      {
        LoadComboboxWithXmlFile(comboBox, filename, tags);
      }
    }

    private void LoadComboboxWithXmlFile(ComboBox cb, string filename, params string[] tags)
    {
      cb.Items.Clear();
      if (!File.Exists(filename))
      {
        _teamMembersCreated = false;
        return;
      }

      _teamMembersCreated = true;
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(filename);
      }
      catch (Exception exception)
      {
        MessageBox.Show(Resources.Error_while_loading_the + Punctuation.OneSpace +
          Settings.Default.LanguageFileName + Punctuation.OneSpace +
          Resources.xml_file + Punctuation.OneSpace + exception.Message);
        return;
      }
      var result = from node in xDoc.Descendants(tags[0])
                   where node.HasElements
                   let xElementName = node.Element(tags[1])
                   where xElementName != null
                   let xElementValue = node.Element(tags[2])
                   where xElementValue != null
                   select new
                   {
                     NodeValue1 = xElementName.Value,
                     NodeValue2 = xElementValue.Value
                   };
      foreach (var i in result)
      {
        if (!cb.Items.Contains(i.NodeValue1))
        {
          cb.Items.Add(i.NodeValue1);
        }
#if DEBUG
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like +
            Punctuation.Colon + Punctuation.OneSpace + i.NodeValue1);
        }
#endif
      }

      cb.SelectedIndex = 0;
    }

    private void LoadConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void SaveConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void LoadLanguages()
    {
      if (!File.Exists(Settings.Default.LanguageFileName))
      {
        CreateLanguageFile();
      }

      // read the translation file and feed the language
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(Settings.Default.LanguageFileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show(Resources.Error_while_loading_the + Punctuation.OneSpace +
          Settings.Default.LanguageFileName + Punctuation.OneSpace +
          Resources.xml_file + Punctuation.OneSpace + exception.Message);
        CreateLanguageFile();
        return;
      }
      var result = from node in xDoc.Descendants("term")
                   where node.HasElements
                   let xElementName = node.Element("name")
                   where xElementName != null
                   let xElementEnglish = node.Element("englishValue")
                   where xElementEnglish != null
                   let xElementFrench = node.Element("frenchValue")
                   where xElementFrench != null
                   select new
                   {
                     name = xElementName.Value,
                     englishValue = xElementEnglish.Value,
                     frenchValue = xElementFrench.Value
                   };
      foreach (var i in result)
      {
        if (!_languageDicoEn.ContainsKey(i.name))
        {
          _languageDicoEn.Add(i.name, i.englishValue);
        }
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like +
            Punctuation.Colon + Punctuation.OneSpace + i.name);
        }

        if (!_languageDicoFr.ContainsKey(i.name))
        {
          _languageDicoFr.Add(i.name, i.frenchValue);
        }
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like +
            Punctuation.Colon + Punctuation.OneSpace + i.name);
        }
      }
    }

    private static void CreateLanguageFile()
    {
      List<string> minimumVersion = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
        "<terms>",
         "<term>",
        "<name>MenuFile</name>",
        "<englishValue>File</englishValue>",
        "<frenchValue>Fichier</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileNew</name>",
        "<englishValue>New</englishValue>",
        "<frenchValue>Nouveau</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileOpen</name>",
        "<englishValue>Open</englishValue>",
        "<frenchValue>Ouvrir</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSave</name>",
        "<englishValue>Save</englishValue>",
        "<frenchValue>Enregistrer</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSaveAs</name>",
        "<englishValue>Save as ...</englishValue>",
        "<frenchValue>Enregistrer sous ...</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFilePrint</name>",
        "<englishValue>Print ...</englishValue>",
        "<frenchValue>Imprimer ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufilePageSetup</name>",
          "<englishValue>Page setup</englishValue>",
          "<frenchValue>Aperçu avant impression</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufileQuit</name>",
          "<englishValue>Quit</englishValue>",
          "<frenchValue>Quitter</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEdit</name>",
          "<englishValue>Edit</englishValue>",
          "<frenchValue>Edition</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCancel</name>",
          "<englishValue>Cancel</englishValue>",
          "<frenchValue>Annuler</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditRedo</name>",
          "<englishValue>Redo</englishValue>",
          "<frenchValue>Rétablir</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCut</name>",
          "<englishValue>Cut</englishValue>",
          "<frenchValue>Couper</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCopy</name>",
          "<englishValue>Copy</englishValue>",
          "<frenchValue>Copier</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditPaste</name>",
          "<englishValue>Paste</englishValue>",
          "<frenchValue>Coller</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditSelectAll</name>",
          "<englishValue>Select All</englishValue>",
          "<frenchValue>Sélectionner tout</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuTools</name>",
          "<englishValue>Tools</englishValue>",
          "<frenchValue>Outils</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsCustomize</name>",
          "<englishValue>Customize ...</englishValue>",
          "<frenchValue>Personaliser ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsOptions</name>",
          "<englishValue>Options</englishValue>",
          "<frenchValue>Options</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguage</name>",
          "<englishValue>Language</englishValue>",
          "<frenchValue>Langage</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageEnglish</name>",
          "<englishValue>English</englishValue>",
          "<frenchValue>Anglais</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageFrench</name>",
          "<englishValue>French</englishValue>",
          "<frenchValue>Français</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelp</name>",
          "<englishValue>Help</englishValue>",
          "<frenchValue>Aide</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSummary</name>",
          "<englishValue>Summary</englishValue>",
          "<frenchValue>Sommaire</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpIndex</name>",
          "<englishValue>Index</englishValue>",
          "<frenchValue>Index</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSearch</name>",
          "<englishValue>Search</englishValue>",
          "<frenchValue>Rechercher</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpAbout</name>",
          "<englishValue>About</englishValue>",
          "<frenchValue>A propos de ...</frenchValue>",
        "</term>",
        "</terms>"
      };
      StreamWriter sw = new StreamWriter(Settings.Default.LanguageFileName);
      foreach (string item in minimumVersion)
      {
        sw.WriteLine(item);
      }

      sw.Close();
    }

    private void GetWindowValue()
    {
      Width = Settings.Default.WindowWidth;
      Height = Settings.Default.WindowHeight;
      Top = Settings.Default.WindowTop < 0 ? 0 : Settings.Default.WindowTop;
      Left = Settings.Default.WindowLeft < 0 ? 0 : Settings.Default.WindowLeft;
      SetDisplayOption(Settings.Default.DisplayToolStripMenuItem);
      LoadConfigurationOptions();
      _teamMembersCreated = Settings.Default._teamMembersCreated;
      tabControlMain.SelectedIndex = Settings.Default.LastTabUsed;
    }

    private void SaveWindowValue()
    {
      Settings.Default.WindowHeight = Height;
      Settings.Default.WindowWidth = Width;
      Settings.Default.WindowLeft = Left;
      Settings.Default.WindowTop = Top;
      Settings.Default.LastLanguageUsed = frenchToolStripMenuItem.Checked ? "French" : "English";
      Settings.Default.DisplayToolStripMenuItem = GetDisplayOption();
      SaveConfigurationOptions();
      Settings.Default._teamMembersCreated = _teamMembersCreated;
      Settings.Default.LastTabUsed = tabControlMain.SelectedIndex;
      Settings.Default.Save();
    }

    private string GetDisplayOption()
    {
      if (SmallToolStripMenuItem.Checked)
      {
        return "Small";
      }

      if (MediumToolStripMenuItem.Checked)
      {
        return "Medium";
      }

      return LargeToolStripMenuItem.Checked ? "Large" : string.Empty;
    }

    private void SetDisplayOption(string option)
    {
      UncheckAllOptions();
      switch (option.ToLower())
      {
        case "small":
          SmallToolStripMenuItem.Checked = true;
          break;
        case "medium":
          MediumToolStripMenuItem.Checked = true;
          break;
        case "large":
          LargeToolStripMenuItem.Checked = true;
          break;
        default:
          SmallToolStripMenuItem.Checked = true;
          break;
      }
    }

    private void UncheckAllOptions()
    {
      SmallToolStripMenuItem.Checked = false;
      MediumToolStripMenuItem.Checked = false;
      LargeToolStripMenuItem.Checked = false;
    }

    private void FormMainFormClosing(object sender, FormClosingEventArgs e)
    {
      SaveWindowValue();
    }

    private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
    {
      _currentLanguage = Language.French.ToString();
      SetLanguage(Language.French.ToString());
      AdjustAllControls();
    }

    private void englishToolStripMenuItem_Click(object sender, EventArgs e)
    {
      _currentLanguage = Language.English.ToString();
      SetLanguage(Language.English.ToString());
      AdjustAllControls();
    }

    private void SetLanguage(string myLanguage)
    {
      switch (myLanguage)
      {
        case "English":
          frenchToolStripMenuItem.Checked = false;
          englishToolStripMenuItem.Checked = true;
          fileToolStripMenuItem.Text = _languageDicoEn["MenuFile"];
          newToolStripMenuItem.Text = _languageDicoEn["MenuFileNew"];
          openToolStripMenuItem.Text = _languageDicoEn["MenuFileOpen"];
          saveToolStripMenuItem.Text = _languageDicoEn["MenuFileSave"];
          saveasToolStripMenuItem.Text = _languageDicoEn["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = _languageDicoEn["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = _languageDicoEn["MenufilePageSetup"];
          quitToolStripMenuItem.Text = _languageDicoEn["MenufileQuit"];
          editToolStripMenuItem.Text = _languageDicoEn["MenuEdit"];
          cancelToolStripMenuItem.Text = _languageDicoEn["MenuEditCancel"];
          redoToolStripMenuItem.Text = _languageDicoEn["MenuEditRedo"];
          cutToolStripMenuItem.Text = _languageDicoEn["MenuEditCut"];
          copyToolStripMenuItem.Text = _languageDicoEn["MenuEditCopy"];
          pasteToolStripMenuItem.Text = _languageDicoEn["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = _languageDicoEn["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = _languageDicoEn["MenuTools"];
          personalizeToolStripMenuItem.Text = _languageDicoEn["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = _languageDicoEn["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = _languageDicoEn["MenuLanguage"];
          englishToolStripMenuItem.Text = _languageDicoEn["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = _languageDicoEn["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = _languageDicoEn["MenuHelp"];
          summaryToolStripMenuItem.Text = _languageDicoEn["MenuHelpSummary"];
          indexToolStripMenuItem.Text = _languageDicoEn["MenuHelpIndex"];
          searchToolStripMenuItem.Text = _languageDicoEn["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = _languageDicoEn["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = _languageDicoEn["Display"];
          SmallToolStripMenuItem.Text = _languageDicoEn["Small"];
          MediumToolStripMenuItem.Text = _languageDicoEn["Medium"];
          LargeToolStripMenuItem.Text = _languageDicoEn["Large"];
          if (listViewVisualizeTeam.Columns.Count != 0)
          {
            listViewVisualizeTeam.Columns[0].Text = _languageDicoEn["Player number"];
            listViewVisualizeTeam.Columns[1].Text = _languageDicoEn["First name"];
            listViewVisualizeTeam.Columns[2].Text = _languageDicoEn["Last name"];
            listViewVisualizeTeam.Columns[3].Text = _languageDicoEn["Gender"];
            listViewVisualizeTeam.Columns[4].Text = _languageDicoEn["Simple level"];
            listViewVisualizeTeam.Columns[5].Text = _languageDicoEn["double level"];
            listViewVisualizeTeam.Columns[6].Text = _languageDicoEn["Mixed level"];
            listViewVisualizeTeam.Columns[7].Text = _languageDicoEn["License number"];
          }
          
          
          _currentLanguage = "English";
          break;
        case "French":
          frenchToolStripMenuItem.Checked = true;
          englishToolStripMenuItem.Checked = false;
          fileToolStripMenuItem.Text = _languageDicoFr["MenuFile"];
          newToolStripMenuItem.Text = _languageDicoFr["MenuFileNew"];
          openToolStripMenuItem.Text = _languageDicoFr["MenuFileOpen"];
          saveToolStripMenuItem.Text = _languageDicoFr["MenuFileSave"];
          saveasToolStripMenuItem.Text = _languageDicoFr["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = _languageDicoFr["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = _languageDicoFr["MenufilePageSetup"];
          quitToolStripMenuItem.Text = _languageDicoFr["MenufileQuit"];
          editToolStripMenuItem.Text = _languageDicoFr["MenuEdit"];
          cancelToolStripMenuItem.Text = _languageDicoFr["MenuEditCancel"];
          redoToolStripMenuItem.Text = _languageDicoFr["MenuEditRedo"];
          cutToolStripMenuItem.Text = _languageDicoFr["MenuEditCut"];
          copyToolStripMenuItem.Text = _languageDicoFr["MenuEditCopy"];
          pasteToolStripMenuItem.Text = _languageDicoFr["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = _languageDicoFr["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = _languageDicoFr["MenuTools"];
          personalizeToolStripMenuItem.Text = _languageDicoFr["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = _languageDicoFr["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = _languageDicoFr["MenuLanguage"];
          englishToolStripMenuItem.Text = _languageDicoFr["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = _languageDicoFr["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = _languageDicoFr["MenuHelp"];
          summaryToolStripMenuItem.Text = _languageDicoFr["MenuHelpSummary"];
          indexToolStripMenuItem.Text = _languageDicoFr["MenuHelpIndex"];
          searchToolStripMenuItem.Text = _languageDicoFr["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = _languageDicoFr["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = _languageDicoFr["Display"];
          SmallToolStripMenuItem.Text = _languageDicoFr["Small"];
          MediumToolStripMenuItem.Text = _languageDicoFr["Medium"];
          LargeToolStripMenuItem.Text = _languageDicoFr["Large"];
          if (listViewVisualizeTeam.Columns.Count != 0)
          {
            listViewVisualizeTeam.Columns[0].Text = _languageDicoFr["Player number"];
            listViewVisualizeTeam.Columns[1].Text = _languageDicoFr["First name"];
            listViewVisualizeTeam.Columns[2].Text = _languageDicoFr["Last name"];
            listViewVisualizeTeam.Columns[3].Text = _languageDicoFr["Gender"];
            listViewVisualizeTeam.Columns[4].Text = _languageDicoFr["Simple level"];
            listViewVisualizeTeam.Columns[5].Text = _languageDicoFr["double level"];
            listViewVisualizeTeam.Columns[6].Text = _languageDicoFr["Mixed level"];
            listViewVisualizeTeam.Columns[7].Text = _languageDicoFr["License number"];
          }
          
          _currentLanguage = "French";
          break;
        default:
          SetLanguage("English");
          break;
      }
    }

    private void cutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CutToClipboard(tb);
      }
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CopyToClipboard(tb);
      }
    }

    private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { });
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        PasteFromClipboard(tb);
      }
    }

    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { });
      TextBox control = focusedControl as TextBox;
      if (control != null) control.SelectAll();
    }

    private void CutToClipboard(TextBoxBase tb, string errorMessage = "nothing")
    {
      if (tb != ActiveControl) return;
      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIs") + Punctuation.OneSpace +
          Translate(errorMessage) + Punctuation.OneSpace +
          Translate("ToCut") + Punctuation.OneSpace, Translate(errorMessage),
          MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(errorMessage), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
      tb.SelectedText = string.Empty;
    }

    private void CopyToClipboard(TextBoxBase tb, string message = "nothing")
    {
      if (tb != ActiveControl) return;
      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIsNothingToCopy") + Punctuation.OneSpace,
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
    }

    private void PasteFromClipboard(TextBoxBase tb)
    {
      if (tb != ActiveControl) return;
      var selectionIndex = tb.SelectionStart;
      tb.Text = tb.Text.Insert(selectionIndex, Clipboard.GetText());
      tb.SelectionStart = selectionIndex + Clipboard.GetText().Length;
    }

    private void DisplayMessage(string message, string title, MessageBoxButtons buttons)
    {
      MessageBox.Show(this, message, title, buttons);
    }

    private string Translate(string index)
    {
      string result = string.Empty;
      switch (_currentLanguage.ToLower())
      {
        case "english":
          result = _languageDicoEn.ContainsKey(index) ? _languageDicoEn[index] :
           "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
        case "french":
          result = _languageDicoFr.ContainsKey(index) ? _languageDicoFr[index] :
            "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
      }

      return result;
    }

    private static Control FindFocusedControl(Control container)
    {
      foreach (Control childControl in container.Controls.Cast<Control>().Where(childControl => childControl.Focused))
      {
        return childControl;
      }

      return (from Control childControl in container.Controls
              select FindFocusedControl(childControl)).FirstOrDefault(maybeFocusedControl => maybeFocusedControl != null);
    }

    private static Control FindFocusedControl(List<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(params Control[] container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(IEnumerable<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static string PeekDirectory()
    {
      string result = string.Empty;
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      if (fbd.ShowDialog() == DialogResult.OK)
      {
        result = fbd.SelectedPath;
      }

      return result;
    }

    private string PeekFile()
    {
      string result = string.Empty;
      OpenFileDialog fd = new OpenFileDialog();
      if (fd.ShowDialog() == DialogResult.OK)
      {
        result = fd.SafeFileName;
      }

      return result;
    }

    private void SmallToolStripMenuItem_Click(object sender, EventArgs e)
    {
      UncheckAllOptions();
      SmallToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void MediumToolStripMenuItem_Click(object sender, EventArgs e)
    {
      UncheckAllOptions();
      MediumToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void LargeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      UncheckAllOptions();
      LargeToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private static void AdjustControls(params Control[] listOfControls)
    {
      if (listOfControls.Length == 0)
      {
        return;
      }

      int position = listOfControls[0].Width + 33; // 33 is the initial padding
      bool isFirstControl = true;
      foreach (Control control in listOfControls)
      {
        if (isFirstControl)
        {
          isFirstControl = false;
        }
        else
        {
          control.Left = position + 10;
          position += control.Width;
        }
      }
    }

    private void AdjustAllControls()
    {
      AdjustControls(); // insert here all labels, textboxes and buttons, one method per line of controls
    }

    private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      FormOptions frmOptions = new FormOptions(_configurationOptions);

      if (frmOptions.ShowDialog() == DialogResult.OK)
      {
        _configurationOptions = frmOptions.ConfigurationOptions2;
      }
    }

    private static void SetButtonEnabled(Button button, params Control[] controls)
    {
      bool result = true;
      foreach (Control ctrl in controls)
      {
        if (ctrl.GetType() == typeof(TextBox))
        {
          if (((TextBox)ctrl).Text == string.Empty)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ListView))
        {
          if (((ListView)ctrl).Items.Count == 0)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ComboBox))
        {
          if (((ComboBox)ctrl).SelectedIndex == -1)
          {
            result = false;
            break;
          }
        }
      }

      button.Enabled = result;
    }

    private void textBoxName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        // do something
      }
    }

    private void buttonAddPlayer_Click(object sender, EventArgs e)
    {
      Player newPlayer = new Player(textBoxFirstName.Text, textBoxLastName.Text,
        (Gender)comboBoxSex.SelectedIndex, (RankLevel)comboBoxSimple.SelectedIndex,
        (RankLevel)comboBoxDouble.SelectedIndex, (RankLevel)comboBoxMixed.SelectedIndex);
      if (!File.Exists(Settings.Default.PlayersFileName))
      {
        if (!CreateRootXmlFile(Settings.Default.PlayersFileName, "players"))
        {
          MessageBox.Show(Resources.Error_while_trying_to_create_the_file + Punctuation.OneSpace +
            Settings.Default.PlayersFileName);
          return;
        }
      }

      // Check if the player is not already in
      // add one player
      SaveToXmlFile(Settings.Default.PlayersFileName, "player",
        "firstname", newPlayer.FirstName,
        "lastname", newPlayer.LastName,
        "gender", newPlayer.SexGender.ToString(),
        "simplelevel", newPlayer.SimpleLevel.ToString(),
        "doublelevel", newPlayer.DoubleLevel.ToString(),
        "mixedlevel", newPlayer.MixedLevel.ToString());
      textBoxFirstName.Text = string.Empty;
      textBoxLastName.Text = string.Empty;
      // reload list of players with new player and switch over to visualization
      LoadPlayers();
      tabControlMain.SelectedIndex = 1;

    }

    private static void SaveToXmlFile(string fileName, params string[] xmlTags)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(fileName);
      XmlNode root = doc.DocumentElement;
      XmlElement newNode = doc.CreateElement(xmlTags[0]);
      var listOfProperties = new List<XmlElement>();
      for (int i = 1; i < xmlTags.Length - 1; i = i + 2)
      {
        XmlElement newProperty = doc.CreateElement(xmlTags[i]);
        newProperty.InnerText = xmlTags[i + 1];
        listOfProperties.Add(newProperty);
      }

      foreach (XmlElement element in listOfProperties)
      {
        newNode.AppendChild(element);
      }

      root.AppendChild(newNode);
      doc.Save(fileName);
    }

    private bool CreateRootXmlFile(string fileName, string rootTagName = "root")
    {
      bool result = false;
      List<string> minimumVersion = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
        "<" + rootTagName + ">",
        "</" + rootTagName + ">"
      };
      try
      {
        StreamWriter sw = new StreamWriter(fileName);
        foreach (string item in minimumVersion)
        {
          sw.WriteLine(item);
        }

        sw.Close();
        result = true;
      }
      catch (Exception exception)
      {
        MessageBox.Show(Translate("Error while writing the XML file") + Punctuation.OneSpace +
          Punctuation.Colon + Punctuation.OneSpace + Settings.Default.LanguageFileName +
          Punctuation.OneSpace + Punctuation.CrLf + Translate("The error is") +
          Punctuation.OneSpace + exception.Message);
        result = false;
      }

      return result;
    }

    private void textBoxFirstName_TextChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void textBoxLastName_TextChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void comboBoxSex_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void comboBoxSimple_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void comboBoxDouble_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void comboBoxMixed_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
    }

    private void tabPageVisualizeTeam_Enter(object sender, EventArgs e)
    {
      if (!_visualizeTeamLoaded)
      {
        LoadPlayers();
      }
    }

    private void LoadPlayers()
    {
      var listOfPlayers = LoadXmlIntoList(Settings.Default.PlayersFileName,
          "player",
          "firstname",
          "lastname",
          "gender",
          "simplelevel",
          "doublelevel",
          "mixedlevel");
      LoadListView(listViewVisualizeTeam, listOfPlayers);
      _visualizeTeamLoaded = true;
    }

    private void LoadListView(ListView lv, IEnumerable<Player> listOfPlayers)
    {
      lv.Items.Clear();
      lv.Columns.Add(Translate("Player number"), 200, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("First name"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("Last name"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("Gender"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("Simple level"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("Double level"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("Mixed level"), 240, HorizontalAlignment.Left);
      lv.Columns.Add(Translate("License number"), 240, HorizontalAlignment.Left);
      lv.View = View.Details;
      lv.LabelEdit = false;
      lv.AllowColumnReorder = true;
      lv.CheckBoxes = true;
      lv.FullRowSelect = true;
      lv.GridLines = true;
      lv.Sorting = SortOrder.None;
      int playerCount = 1;
      foreach (Player player in listOfPlayers)
      {
        ListViewItem item1 = new ListViewItem("Player " + playerCount) { Checked = false };
        item1.SubItems.Add(player.FirstName);
        item1.SubItems.Add(player.LastName);
        item1.SubItems.Add(player.SexGender.ToString());
        item1.SubItems.Add(player.SimpleLevel.ToString());
        item1.SubItems.Add(player.DoubleLevel.ToString());
        item1.SubItems.Add(player.MixedLevel.ToString());
        item1.SubItems.Add(player.LicenseNumber.ToString());
        lv.Items.Add(item1);
        playerCount++;
        Application.DoEvents();
      }

      ResizeListViewColumns(listViewVisualizeTeam);
    }

    private static void ResizeListViewColumns(ListView lv)
    {
      if (lv.Columns.Count == 0)
      {
        return;
      }

      for (int i = 0; i < lv.Columns.Count - 1; i++)
      {
        lv.AutoResizeColumn(i, GetLongestString(lv.Columns[i].Text, lv));
      }
    }

    private static ColumnHeaderAutoResizeStyle GetLongestString(string headerText, ListView lv)
    {
      return headerText.Length > MaxString(lv.Items).Length ? ColumnHeaderAutoResizeStyle.HeaderSize : ColumnHeaderAutoResizeStyle.ColumnContent;
    }

    private static string MaxString(ListView.ListViewItemCollection items)
    {
      string longest = string.Empty;
      foreach (ListViewItem item in items)
      {
        if (item.ToString().Length > longest.Length)
        {
          longest = item.Text;
        }
      }

      return longest;
    }

    private static bool IsInlistView(ListView listView, ListViewItem lviItem, int columnNumber)
    {
      bool result = false;
      foreach (ListViewItem item in listView.Items)
      {
        if (item.SubItems[columnNumber].Text == lviItem.SubItems[columnNumber].Text)
        {
          result = true;
          break;
        }
      }

      return result;
    }

    private static IEnumerable<Player> LoadXmlIntoList(string fileName, params string[] tags)
    {
      List<Player> result = new List<Player>();
      if (!File.Exists(fileName))
      {
        return result;
      }

      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(fileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show(Resources.Error_while_loading_the + Punctuation.OneSpace +
          Settings.Default.PlayersFileName + Punctuation.OneSpace +
          Resources.xml_file + Punctuation.OneSpace + exception.Message);
        return result;
      }
      var result2 = from node in xDoc.Descendants(tags[0])
                    where node.HasElements
                    let xElementFirstName = node.Element(tags[1])
                    where xElementFirstName != null
                    let xElementLastName = node.Element(tags[2])
                    where xElementLastName != null
                    let xElementGender = node.Element(tags[3])
                    where xElementGender != null
                    let xElementSimpleLevel = node.Element(tags[4])
                    where xElementSimpleLevel != null
                    let xElementDoubleLevel = node.Element(tags[5])
                    where xElementDoubleLevel != null
                    let xElementMixedLevel = node.Element(tags[6])
                    where xElementMixedLevel != null
                    select new
                    {
                      NodeValue1 = xElementFirstName.Value,
                      NodeValue2 = xElementLastName.Value,
                      NodeValue3 = xElementGender.Value,
                      NodeValue4 = xElementSimpleLevel.Value,
                      NodeValue5 = xElementDoubleLevel.Value,
                      NodeValue6 = xElementMixedLevel.Value
                    };
      foreach (var i in result2)
      {
        result.Add(new Player(i.NodeValue1, i.NodeValue2, 
            (Gender)Enum.Parse(typeof(Gender), i.NodeValue3),
            (RankLevel)Enum.Parse(typeof(RankLevel), i.NodeValue4),
            (RankLevel)Enum.Parse(typeof(RankLevel), i.NodeValue5),
            (RankLevel)Enum.Parse(typeof(RankLevel), i.NodeValue6)));
      }

      return result;
    }

    private static void AcceptOnlyNumbers(TextBox textBox)
    {
      if (textBox == null) return;
      int value;
      if (!int.TryParse(textBox.Text, out value))
      {
        textBox.Text = string.Empty;
      }
    }

    private void textBoxLicenseNumber_TextChanged(object sender, EventArgs e)
    {
      SetButtonEnabled(buttonAddPlayer, textBoxFirstName, textBoxLastName, comboBoxSex, comboBoxSimple,
        comboBoxDouble, comboBoxMixed, textBoxLicenseNumber);
      // accept only numbers
      AcceptOnlyNumbers(textBoxLicenseNumber);
    }
  }
}