using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Duh {

    //This class is to represent a single node in the trie.
    class Node {
        public string Value { get; set; }
        public Dictionary<char, Node> LinkedNodes { get; set; }
        public bool IsTerminal { get; set; }
        public Node(string v, Dictionary<char, Node> childNodes = null) {
            this.Value = v;
            this.LinkedNodes = childNodes ?? new Dictionary<char, Node>();
        }
    }

    //This is the static instance of trie which is loaded in memory. The class has methods to load, search, calculate edit distance
    //and do operations on the trie.
    static class Trie {
        public static Node _root { get; }
        static Trie() {
            _root = new Node(null);
        }

        public static void LoadTrie(IEnumerable words) {
            var character = new char();
            foreach (string word in words) {
                word.Trim();
                var charArray = word.ToCharArray();
                var node = _root;
                for (var index = 0; index < charArray.Length; index++) {
                    character = word[index];
                    if (node.LinkedNodes.ContainsKey(character)) {
                        node.LinkedNodes.TryGetValue(character, out node);
                    }
                    else {
                        var newNode = new Node(node.Value + character.ToString());
                        node.LinkedNodes.Add(character, newNode);
                        node = newNode;
                    }
                }
                node.IsTerminal = true;
            }
        }

        public static bool Search(string input) {
            var node = _root;
            var word = input.ToCharArray();
            foreach (var letter in word) {
                if (!node.LinkedNodes.ContainsKey(letter))
                    return false;
                node.LinkedNodes.TryGetValue(letter, out node);
            }

            if (node.IsTerminal) return true;
            return false;
        }

        public static bool AddWord(string newWord) {
            //to be implemented
            return false;
        }

        public static void PrintTrie(List<Node> nodes) {

            var childNodes = new List<Node>();
            if (nodes == null || nodes.Count == 0)
                return;

            foreach (var node in nodes) {
                if (node.IsTerminal) {
                    Console.Write("'" + node.Value + "'\n");
                }
                foreach (var childNode in node.LinkedNodes.Values) {
                    childNodes.Add(childNode);
                }
            }
            Console.WriteLine("********");
            PrintTrie(childNodes);

        }

        public static IEnumerable<string> GetAutocompleteSuggestions(string input) {
            var results = Enumerable.Empty<string>();
            var node = _root;
            var phrase = input.ToCharArray();
            foreach (var letter in phrase) {
                if (!node.LinkedNodes.ContainsKey(letter))
                    return results;
                node.LinkedNodes.TryGetValue(letter, out node);
            }

            return FindAllAutocompletePhrases(node, new List<string>());
        }

        public static IEnumerable<string> FindAllAutocompletePhrases(Node node, List<string> completions) {
            if (!node.LinkedNodes.Any())
                return Enumerable.Empty<string>();
            foreach (Node child in node.LinkedNodes.Values) {
                if (child.IsTerminal) completions.Add(child.Value);
                completions.Concat(FindAllAutocompletePhrases(child, completions));
            }
            return completions;
        }
        public static IEnumerable<string> GetClosestCommands(string _input) {
            closestPhrases = new HashSet<string>();
            shortestEd = maxEditDistanceThreshold;
            var node = _root;
            input = _input;

            //preparing the first row
            var length = input.Length;
            var firstRow = new int[length + 1];
            for (int index = 0; index <= length; index++) {
                firstRow[index] = index;
            }

            RecursiveCalcOfEditDistance(node, firstRow);
            if (!closestPhrases.Any())
                closestPhrases.Add("Nada");
            return closestPhrases.ToList<string>();
        }

        static HashSet<string> closestPhrases = new HashSet<string>();
        static int shortestEd;
        static string input;
        const int maxEditDistanceThreshold = 20;

        //Levenshtein distance
        //http://people.cs.pitt.edu/~kirk/cs1501/Pruhs/Spring2006/assignments/editdistance/Levenshtein%20Distance.htm

        public static void RecursiveCalcOfEditDistance(Node node, int[] upperRow) {
            if (!node.LinkedNodes.Any())
                return;
            foreach (var character in node.LinkedNodes.Keys) {
                Node child;
                node.LinkedNodes.TryGetValue(character, out child);
                var nextRow = CalculateNextRow(input, upperRow, character);
                //Console.WriteLine(child.Value + ":" + character + "::" + GetString(upperRow) + "***\n" + GetString(nextRow) + "\n");
                if (child.IsTerminal) {
                    int editDist = GetEditDistance(nextRow);
                    //Console.WriteLine(child.Value + ":" + editDist + " , sd=" + shortestEd);
                    if (editDist == shortestEd) closestPhrases.Add(child.Value);
                    else if (editDist < shortestEd) {
                        shortestEd = editDist;
                        closestPhrases = new HashSet<string>();
                        closestPhrases.Add(child.Value);
                    }
                }

                RecursiveCalcOfEditDistance(child, nextRow);
            }
            return;
        }

        public static int[] CalculateNextRow(string inputStr, int[] upperRow, char c) {
            var left = upperRow[0] + 1;
            int top;
            var diagonal = upperRow[0];

            int[] resultRow = new int[upperRow.Length];
            resultRow[0] = left;
            for (var index = 1; index < upperRow.Length; index++) {
                top = upperRow[index];
                resultRow[index] = Math.Min(left + 1, Math.Min(top + 1, diagonal + (c == inputStr.ToCharArray()[index - 1] ? 0 : 1)));
                left = resultRow[index];
                diagonal = top;
            }
            return resultRow;
        }

        public static int GetEditDistance(int[] arr) {
            return arr[arr.Length - 1];
        }

        //public static string GetString(int [] arr) {
        //    string v = "[";
        //    foreach (var a in arr)
        //        v = v+ a + ",";
        //    v += "]";

        //    return v;
        //}

    }
}
