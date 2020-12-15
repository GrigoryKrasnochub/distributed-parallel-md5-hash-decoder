using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerCSharp.Client
{
    class HashDecoder
    {
        readonly static bool Debug = true;
        readonly static bool LogStartStop = true;
        readonly static bool LogWordCombination = false;
        readonly static bool LogChangeLetterStart = false;

        readonly static bool Parallelizm = true;
        readonly static bool ParallelizmLetters = true;
        readonly static bool ParallelizmChangeLetter = true;

        static List<string> potentialHashedString = new List<string>();

        public static List<string> DecodeMD5Hash(int lettersFrom, int lettersTo, string hash, string[] dictionary)
        {
            if (!Parallelizm)
            {
                for (int i = lettersFrom; i < lettersTo; i++)
                {
                    DecodeMD5Hash(i, hash, dictionary);
                }
            } else
            {
                Parallel.For(lettersFrom, lettersTo, (i) =>
                {
                    DecodeMD5Hash(i, hash, dictionary);
                });
            }

            return potentialHashedString;
        }

        public static List<string> DecodeMD5Hash(int[] lettersCount, string hash, string[] dictionary)
        {
            if (!(Parallelizm && ParallelizmLetters))
            {
                foreach (int letters in lettersCount)
                {
                    if (Debug && LogStartStop) {
                        Console.WriteLine("start {0}", letters);
                    }
                    DecodeMD5Hash(letters, hash, dictionary);
                    if (Debug && LogStartStop)
                    {
                        Console.WriteLine("stop {0}", letters);
                    }
                }
            }
            else
            {
                Parallel.For(0, lettersCount.Length, (i) =>
                {
                    if (Debug && LogStartStop)
                    {
                        Console.WriteLine("start {0}", lettersCount[i]);
                    }
                    DecodeMD5Hash(lettersCount[i], hash, dictionary);
                    if (Debug && LogStartStop)
                    {
                        Console.WriteLine("stop {0}", lettersCount[i]);
                    }
                });
            }

            return potentialHashedString;
        }


        public static List<string> DecodeMD5Hash(int lettersCounter, string hash, string[] dictionary)
        {
            //init result
            string[] result = new string[lettersCounter];
            for (int i = 0; i < lettersCounter; i++)
            {
                result[i] = dictionary[0];
            }
            changeLetter(result, 0, dictionary, hash);

            return potentialHashedString;
        }

        // TODO use callback !!!
        public static void changeLetter(string[] message, int ind, string[] dictionary, string hash)
        {
            if (Parallelizm && ParallelizmChangeLetter && ind == 0)
            {
                Parallel.ForEach<string>(dictionary, (letter) =>
                {
                    string ltmp = letter;
                    string[] messageTmp = (string[])message.Clone();
                    int indTmp = ind;
                    string[] dictionaryTmp = (string[])dictionary.Clone();
                    string hashTmp = hash;
                    changeLetterInMessage(ltmp, messageTmp, indTmp, dictionaryTmp, hashTmp);
                });
            } else
            {
                foreach (string letter in dictionary)
                {
                    changeLetterInMessage(letter, message, ind, dictionary, hash);
                }

            }


            //Parallel.ForEach<string>(dictionary, (letter) =>
            /*
            bool runNext = ind + 1 < message.Length;
            foreach (string letter in dictionary)
            {
                 message[ind] = letter;
                 if (runNext)
                 {
                     changeLetter(message, ind + 1, dictionary, hash);
                 }
                 else
                 {
                    // word is ready
                    string str = "";
                     foreach (string ch in message)
                     {
                         str += ch;
                         if (Debug && LogWordCombination)
                         {
                             Console.Write(ch);
                         }
                     }
                     if (hash == CreateMD5(str))
                     {
                         lock (potentialHashedString)
                         {
                             potentialHashedString.Add(str);
                         }
                     };
                     if (Debug && LogWordCombination)
                     {
                         Console.WriteLine("");
                     }
                 }
             };
             */
        }

        private static void changeLetterInMessage(string letter, string[] message, int ind, string[] dictionary, string hash)
        {
            if (Debug && LogChangeLetterStart && ind == 0)
            {
                Console.WriteLine("start with {0} {1} {2} {3}", letter, ind, dictionary, hash);
            }
            message[ind] = letter;
            if (ind + 1 < message.Length)
            {
                changeLetter(message, ind + 1, dictionary, hash);
            }
            else
            {
                // word is ready
                string str = "";
                foreach (string ch in message)
                {
                    str += ch;
                }
                if (hash == CreateMD5(str))
                {
                    lock (potentialHashedString)
                    {
                        potentialHashedString.Add(str);
                    }
                };
                if (Debug && LogWordCombination)
                {
                    Console.WriteLine(str);
                }
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
