// using System;
// using System.IO;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;


// public class Decoder : MonoBehaviour
// {
//     void Start()
//     {
//         return;
//         string decodedMessage = Decode(@"F:\Program Files\unity\_ game jams\GMTK 2023-2\coding_qual_input.txt");
//         Debug.Log(decodedMessage);
//     }

//     static string Decode(string messageFile)
//     {
//         // Read all lines from the text file.
//         string[] lines = File.ReadAllLines(messageFile);

//         // Parse lines to extract numbers and words.
//         List<(int, string)> numbersAndWords = new List<(int, string)>();
//         foreach (string line in lines)
//         {
//             string[] parts = line.Split(' ');
//             int number = int.Parse(parts[0]);
//             string word = parts[1];
//             numbersAndWords.Add((number, word));
//         }

//         // Sort by numbers first.
//         numbersAndWords.Sort((x, y) => x.Item1.CompareTo(y.Item1));

//         // Create the pyramid structure.
//         List<List<string>> pyramid = new List<List<string>>();
//         int currentIndex = 0;

//         foreach ((int number, string word) in numbersAndWords)
//         {
//             if (pyramid.Count == 0 || pyramid.Last().Count == pyramid.Count)
//             {
//                 pyramid.Add(new List<string>());
//                 currentIndex = 0;
//             }
//             pyramid.Last().Add(word);
//             currentIndex++;
//         }

//         // Extract the words depending on the last number in each pyramid's row.
//         List<string> messageWords = new List<string>();

//         for (int i = 0; i < pyramid.Count; i++)
//             messageWords.Add(pyramid[i].Last());

//         // Ajoin the  message words into one word.
//         string decodedMessage = string.Join(" ", messageWords);

//         return decodedMessage;
//     }
// }
