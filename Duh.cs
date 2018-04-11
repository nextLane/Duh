using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Duh {

    [Cmdlet(VerbsCommon.Get, "duh")]
    public class Duh : Cmdlet {
        // Declare the parameters for the cmdlet.
        //[Parameter(Mandatory = true)]
        //public string InputCommand {
        //    get { return input_command; }
        //    set { input_command = value; }
        //}
        //private string input_command;

        // Overide the ProcessRecord method to process
        // the supplied user name and write out a 
        // greeting to the user by calling the WriteObject
        // method.
        protected override void ProcessRecord() {
            string input;
            IEnumerable<PSObject> res;
            //var history = PowerShell.Create().AddCommand("get-history").Invoke();
            WriteObject("Lets get auto-complete suggestions");
            using (PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace)) {
                res = ps.AddCommand("Get-History")
                        .AddParameter("Count", 1)
                        .Invoke();
                input = res.FirstOrDefault().Properties.ElementAt(1).Value.ToString();

            }

            var answer = Trie.GetAutocompleteSuggestions(input).FirstOrDefault();
            SendKeys.SendWait("Whats up dude, hey jude");
        }
    }

    public class ModuleInitializer : IModuleAssemblyInitializer {
        public void OnImport() {
            var trieDictionary = new HashSet<string>();
            int ignoreLineCount = 2;
            var assembly = Assembly.GetExecutingAssembly();
            var stdGitCommandsFile = "GitCommonStdCommands.txt";
            var commandHistoryFile = "CommandHistory.csv";
            var resourcePrefix = "Duh.Resources.";

            //Loading up all the default standard Git Commands in the trie
            using (Stream stream = assembly.GetManifestResourceStream(string.Concat(resourcePrefix,stdGitCommandsFile)))
            using (StreamReader reader = new StreamReader(stream)) {
                while (reader.Peek() != -1) {
                    var val = reader.ReadLine();
                    trieDictionary.Add(val);
                }
            }
            using (Stream stream = assembly.GetManifestResourceStream(string.Concat(resourcePrefix, commandHistoryFile)))
            using (StreamReader reader = new StreamReader(stream)) {
                while (reader.Peek() != -1) {
                    if (ignoreLineCount > 0) {
                        ignoreLineCount--;
                        reader.ReadLine();
                        continue;
                    }
                    var val = reader.ReadLine().Split(',')[1].Replace("\"", "");
                    trieDictionary.Add(val);
                }
            }

            Trie.LoadTrie(trieDictionary);

        }
    }
}
