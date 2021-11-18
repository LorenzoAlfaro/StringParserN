using System;
using System.Collections.Generic;
using System.IO;

namespace StringParserN
{
    public static class CharWorld
    {
        // Get all my most basic and neutral string manipulations in one place
        // upper level abstractions like parsing VB6 should go in another class
        public static bool CharFound(char find, char[] list)
        {
            foreach (char myChar in list)
            {
                if (myChar == find)
                {
                    return true;
                }
            }
            return false;
        }

        public static void injectSubArray(int start, int end, char[] original, char[] subArray)
        {
            int newIndex = 0;
            for (int i = start; i < end; i++)
            {
                original[i] = subArray[newIndex];
                newIndex += 1;
            }
        }
        public static int findNextChar(char[] array, int index, int end, char endChar)
        {
            for (int i = index; i < end; i++)
            {
                if (array[i] == endChar)
                {
                    return i;
                }
            }
            return -1;
        } // TODO: rename to findCharIndex

        public static int skipSpaces(char[] array, int index, char skipChar)
        {
            for (int i = index; i < array.Length; i++)
            {
                if (array[i] != skipChar)
                {
                    return i;
                }
            }
            return -1;
            //return the character after the spaces [][][][][n] return 5 or could be invalid [][][][][new line]
        }

        // Finds the closing quotes in a char array, "hello".
        // TODO: add a lot of unit testing for this one
        public static int endString(char[] array, int index, char stringChar, char endChar)
        {
            // Expected to be called when you found a '"' character, and I will give the Index returning the end of the string                              
            // Strings within strings work with double quotation marks like this "hello my name is ""Tom"" nice to meet you ""Charles"" he said"
            // This is how VB6 quotes strings within strings
            index = findNextChar(array, index + 1, array.Length, endChar);

            if (index == -1)
            {
                return -1;
            }
            else
            {
                //index += 1; // check of the string within a string
                if (index == array.Length - 1)
                {
                    return index;
                }
                else if (index + 1 < array.Length)
                {
                    if (array[index + 1] != stringChar)
                    {
                        return index;  // There are problems when RS.open "sql"^, adodb                    
                    }
                    else
                    {
                        return endString(array, index + 1, stringChar, endChar); //string within string                                    
                    }
                }
            }
            return -1;
            // They are strings within strings " dfdfdf ""dfdf"" adfdf" & var                                    
        }

        public static int findClosing(char[] array, int index, char startChar, char endChar)
        {
            //(afafaf(dfdf) this breaks, uncomplete par AND ends with closing char
            //I'm expecting to be called when you found startChar character    

            for (int i = index; i < array.Length; i++)
            {
                int offSet = 0;
                int end;
                if (array[i] == startChar)
                {
                    end = findClosing(array, i + 1, startChar, endChar);//corner breaking case q((), incomplete parenthesis                         
                    if (end == -1)
                    {
                        return -1;
                    }
                    else
                    {
                        offSet = 1;
                        i = end;
                    } //because recursive call (()~ incomplete with no more caracters, A((~ RETURN -1
                }
                //
                if (i + offSet < array.Length)
                {
                    if (array[i + offSet] == endChar)
                    {
                        return i + offSet;
                    }
                }
            }
            return -1;
        }

        //given a string "[Field],[Field] from [Table1]"
        //return (Field)(Field)(Table1)
        //the string may or may not have complete []
        //also assuming there is no [] within [], [[]]        
        public delegate int EndingIndex(char[] array, int i, char startChar, char endChar);
        public static List<int[]> getSubSectionsIndexes(char[] array, char startChar, char endChar, EndingIndex ending)
        {
            List<int[]> results = new List<int[]>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == startChar)
                {
                    int startIndex = i; //save start position

                    int end = ending(array, i + 1, startChar, endChar);//warning: forwarding the index
                    if (end != -1)
                    {

                        results.Add(new int[] { startIndex, end });
                    }
                    else
                    {
                        break;
                    }
                    i = end;
                }

            }
            return results;
        }

        public static List<string> getSubSections(char[] array, char startChar, char endChar, bool limiters, EndingIndex ending)
        {
            List<int[]> results2 = getSubSectionsIndexes(array, startChar, endChar, ending);
            List<string> results = new List<string>();
            foreach (int[] result in results2)
            {
                string theString = new string(getSubArray(result[0], result[1], array));
                if (limiters)
                {
                    results.Add(theString);
                }
                else
                {
                    string theString2 = theString.TrimStart(startChar);
                    theString2 = theString2.TrimEnd(endChar);
                    results.Add(theString2);
                }
            }
            return results;
        }

        public static char[] getSubArray(int start, int end, char[] array)
        {
            //int end can be the size of an array
            int size = end - start;
            if (size <= 0)
            {
                return new char[0];
            }
            else
            {
                char[] variable = new char[size + 1];
                int index = 0;
                for (int i = start; i < end + 1; i++) //end could be the array.length and it would work fine but cant be longer the array.length , MADE IT INCLUSIVE
                {
                    variable[index] = array[i];
                    index += 1;
                }
                return variable;
            }
        }


        // extra string methods to filter

        public static bool bitContains(string LineToCheck, string searchWord, StringComparison comparison)
        {
            return LineToCheck.Contains(searchWord, comparison);
        }

        public delegate bool DrillBit(string LineToCheck, string searchWord, StringComparison comparison = StringComparison.CurrentCulture);

        public static bool Contains(this String str, String substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                                "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                            "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        // Taladros

        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                                StringSplitOptions.RemoveEmptyEntries).Length;
        }
        //Level 1.5
        private static List<string> StreamReaderTaladro(StreamReader sr, string search, string path, string inLineDivisor, bool ignoreCasing, DrillBit bit)
        {
            StringComparison comp = ignoreCasing ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;
            List<string> results = new List<string>();
            int lineNumber = 1; //starts with zero because NotePadd ++ lineNumber starts with zero
            string line = " ";
            while (line != null)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    if (bit(line, search, comp)) //this is like a drillbit, this function can be swapped ////it is a bit too inclusev FRM_EMAIL and FRM_EMAIL_LIST are both counted, add a check for space at the end or '.'
                    {
                        results.Add(path + inLineDivisor + lineNumber.ToString() + inLineDivisor + line);
                    }
                }
                lineNumber += 1;
            }
            return results;
        }
        public static List<string> StreamReaderTaladro2(this string[] lines, string search, string path, string inLineDivisor, bool ignoreCasing, DrillBit bit)
        {
            StringComparison comp = ignoreCasing ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;
            List<string> results = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (bit(lines[i], search, comp)) //this is like a drillbit, this function can be swapped ////it is a bit too inclusev FRM_EMAIL and FRM_EMAIL_LIST are both counted, add a check for space at the end or '.'
                {
                    results.Add(path + inLineDivisor + (i).ToString() + inLineDivisor + lines[i]);
                }
            }
            return results;
        }
        //Level 1
        public static int reverseTaladro(this string[] lines, string search, int currentLine, int boundary, bool ignoreCasing, bool forwards, DrillBit bit) //searches backwards
        {
            StringComparison comp = ignoreCasing ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;
            if (forwards)
            {
                for (int i = (currentLine - 1); i <= boundary; i++) //it goes towards the BEGINING of the file
                {
                    string line = lines[i].TrimStart();
                    if (bit(line, search, comp))
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = (currentLine - 1); i >= boundary; i--) //it goes towards the BEGINING of the file
                {
                    string line = lines[i].TrimStart();
                    if (bit(line, search, comp))
                    {
                        return i;
                    }
                }

            }
            return -1;
        }



        public static bool SelectorBit(string BitName, string LineToCheck, string searchWord, StringComparison comp)  // hack for running static methods from a dropdown
        {
            List<Object> myParams = new List<object>();
            myParams.Add(LineToCheck);
            myParams.Add(searchWord);
            myParams.Add(comp);
            Type type = typeof(CharWorld);

            return (bool)type.GetMethod(BitName).Invoke(null, myParams.ToArray());
        }
    }
    
}
