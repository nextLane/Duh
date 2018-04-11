using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Duh {
    static class Trie {
        public static Node _root { get; }
        static HashSet<string> trieDictionary ;
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


        /* Cases: (1) Missing letters: first letters missing, for every letter, if not found in the 
             * immediate linked nodes try two subsequent nodes. Must end with terminal node, might match termial true
             * (2) Extra letters: might not match terminal true
             * 
             * 
             * */
        //public IEnumerable<string> FindClosestMatch(string word) {
        //    var k = 1; // number of missing or extra letters allowed including start/mid
        //    var letters = word.ToCharArray();
        //    var result = Enumerable.Empty<string>();
        //    int mlF = 0; //missing letter flag
        //    int elF = 0; //extra letter flag

        //    var candidate_nodes = new List<Node>();
        //    candidate_nodes = _root.LinkedNodes.Values.ToList() ;

        //    foreach(var letter in letters) {
        //        if (node.LinkedNodes.ContainsKey(letter)){
        //            node.LinkedNodes.TryGetValue(letter, out node);
        //        }
        //        else {
        //            jumpFlag++;
        //            if(jumpFlag <=k) {
        //                //check the letter in candidate nodes

        //            }
        //        }
        //    }

        //}


        //Levenshtein distance
        //http://people.cs.pitt.edu/~kirk/cs1501/Pruhs/Spring2006/assignments/editdistance/Levenshtein%20Distance.htm

        public static int[] CalculateNextRow(string inputStr, int[] upperRow, char c) {
            var left = upperRow[0] + 1;
            var top = 0;
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


        public static string GetClosestCommands(string _input) {
            var results = Enumerable.Empty<string>();
            var node = _root;
            shortestEd = int.MaxValue;
            input = _input;

            //preparing the first row
            var length = input.Length;
            var firstRow = new int[length+1];
            for(int index = 0; index <=length; index++) {
                firstRow[index] = index;
            }

            RecursiveCalcOfEditDistance(node, firstRow);
            if (!closestPhrases.Any())
                closestPhrases.Add("Nada");
            return closestPhrases.ToList<string>().ElementAt(0);
        }

        //Change from static
        static HashSet<string> closestPhrases = new HashSet<string>();
        static int shortestEd = int.MaxValue;
        static string input;
        public static void RecursiveCalcOfEditDistance(Node node, int[] upperRow) {
            if (!node.LinkedNodes.Any())
                return ;
            foreach (var character in node.LinkedNodes.Keys) {
                Node child;
                node.LinkedNodes.TryGetValue(character, out child);
                upperRow = CalculateNextRow(input, upperRow, character);
                if (child.IsTerminal) {
                    int editDist = GetEditDistance(upperRow);
                    if (editDist  == shortestEd) closestPhrases.Add(child.Value);
                    else if(editDist < shortestEd) {
                        shortestEd = editDist;
                        closestPhrases = new HashSet<string>();
                        closestPhrases.Add(child.Value);
                    }
                }

                RecursiveCalcOfEditDistance(child, upperRow);
            }
            return;
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
        public static void PrintTrie(List<Node> nodes) {

            var childNodes = new List<Node>();
            if (nodes == null || nodes.Count == 0)
                return;

            foreach (var node in nodes) {
                if (node.IsTerminal) {
                    Console.Write("'" + node.Value + "'\n" );
                }
                foreach (var childNode in node.LinkedNodes.Values) {
                    childNodes.Add(childNode);
                }
            }
            Console.WriteLine("********");
            PrintTrie(childNodes);

        }

        public static bool AddWord(string newWord) {
            //to be implemented
            return false;
        }

    }

    class Node {
        public string Value { get; set; }
        public Dictionary<char, Node> LinkedNodes { get; set; }
        public bool IsTerminal { get; set; }
        public Node(string v, Dictionary<char, Node> childNodes = null) {
            this.Value = v;
            this.LinkedNodes = childNodes ?? new Dictionary<char, Node>();
        }
    }




}
