using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Windows.Forms;


namespace Duh {

    [Cmdlet(VerbsCommon.Get, "duh")]
    public class Duh : Cmdlet {
        protected override void ProcessRecord() {
            string input;
            IEnumerable<PSObject> res;
            using (PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace)) {
                res = ps.AddCommand("Get-History")
                        .AddParameter("Count", 1)
                        .Invoke();
                input = res.FirstOrDefault().Properties.ElementAt(1).Value.ToString();

            }

            var answer = Trie.GetClosestCommands(input);
            SendKeys.SendWait(answer);
        }
    }


    [Cmdlet(VerbsCommon.Set,"Duh")]
    public class SetDuh : Cmdlet {
        protected override void ProcessRecord() {

            Trie.LoadTrie(PowershellHelperClass.GetFrequentHistoryCommands());
            var list = new List<Node>();
            list.Add(Trie._root);
            //Trie.PrintTrie(list);
        }
    }

    public class ModuleInitializer : IModuleAssemblyInitializer {
        public void OnImport() {
            var trieDictionary = new HashSet<string>();
            var assembly = Assembly.GetExecutingAssembly();
            var stdGitCommandsFile = "GitCommonStdCommands.txt";
            var resourcePrefix = "Duh.Resources.";

            //Loading up all the default standard Git Commands in the trie
            using (Stream stream = assembly.GetManifestResourceStream(string.Concat(resourcePrefix,stdGitCommandsFile)))
            using (StreamReader reader = new StreamReader(stream)) {
                while (reader.Peek() != -1) {
                    var val = reader.ReadLine();
                    trieDictionary.Add(val);
                }
            }


            var commandsToBeAdded = PowershellHelperClass.GetFrequentHistoryCommands();
            var trieWords = trieDictionary.ToList<string>().Concat(commandsToBeAdded.ToList<string>());
            Trie.LoadTrie(trieWords);
            var list = new List<Node>();
            list.Add(Trie._root);
        }
    }

    public class PowershellHelperClass {
        public static IEnumerable<string> GetFrequentHistoryCommands() {
            var freqCommands = new Dictionary<string, int>();
            IEnumerable<PSObject> res;
            int freq = 1;
            string command;
            using (PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace)) {

                res = ps.AddCommand("Get-History")
                        .Invoke();

                foreach (var inp in res) {
                    freq = 1;
                    command = inp.Properties.ElementAt(1).Value.ToString();
                    if (freqCommands.TryGetValue(command, out freq)) {
                        freqCommands[command] = freq + 1;
                    }
                    else {
                        freqCommands.Add(command, freq + 1);
                    }
                }

                return freqCommands.Where(x => x.Value > 1).Select(x => x.Key);

            }
        }
    }
}
