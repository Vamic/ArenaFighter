using System;
using System.Linq;

namespace Util
{
    class ConsoleEx
    {
        //String of characters allowed
        private static char[] allowedCharacters;
        //Amount of the allowed characters that are only allowed once,
        //taking the first elements from allowedCharacters
        private static byte allowedOnceCount;
        //Amount of characters allowed total
        private static int allowedLength;
        //If - is to be allowed (as the first character only)
        private static bool allowNegative;

        private const char CONFIRM_CHAR = (char)13; //Enter

        private const char UNDO_CHAR = (char)8; //Backspace

        private static string FilteredReadLine()
        {
            Console.CursorVisible = true;

            string line = "";
            char[] allowedArray = allowedCharacters;
            char[] allowedOnceArray = new char[allowedOnceCount];
            Array.Copy(allowedCharacters, allowedOnceArray, allowedOnceCount);
            char key;
            do
            {
                key = Console.ReadKey(true).KeyChar;
                
                if (key == UNDO_CHAR && line.Length > 0)
                {
                    //Remove last character
                    line = line.Remove(line.Length - 1);
                    Console.Write("\b \b");
                }
                //Check if character is allowed
                else if (allowedArray.Contains(key) && line.Length < allowedLength)
                {
                    //Check if character is already in the input when its only allowed once
                    if(line.ToCharArray().Contains(key) && allowedOnceArray.Contains(key))
                        continue; //Ignore it and keep going
                    
                    //Add and print it
                    line += key;
                    Console.Write(key);
                }
                //Allow it to be negative
                else if (allowNegative && line.Length == 0 && key == "-"[0])
                {
                    //Add and print it
                    line += key;
                    Console.Write(key);
                }
            } while (key != CONFIRM_CHAR);
            return line;
        }

        /// <summary>
        /// Filtered ReadLine that allows for input of a float
        /// </summary>
        /// <param name="ignoreExceptions">To ignore exceptions and use default values</param>
        internal static float ReadFloat(bool ignoreExceptions = false) 
        {
            allowedCharacters = ".1234567890".ToCharArray();
            //Allow . only once
            allowedOnceCount = 1;

            allowedLength = byte.MaxValue;

            allowNegative = true;

            string input = FilteredReadLine();
            try
            {
                return float.Parse(input);
            } catch (OverflowException ex) //Input too small/large
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;
                //Min if negative, otherwise max
                if (input[0] == "-"[0])
                    return float.MinValue;
                else
                    return float.MaxValue;
            } catch (FormatException ex) //User didn't input anything
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;

                return 0;
            }
            
        }

        /// <summary>
        /// Filtered ReadLine that only allows numbers
        /// </summary>
        /// <param name="ignoreExceptions">To ignore exceptions and returning default values</param>
        internal static int ReadInt(bool ignoreExceptions = false)
        {
            allowedCharacters = "1234567890".ToCharArray();

            allowedLength = int.MinValue.ToString().Length;

            allowNegative = true;

            string input = FilteredReadLine();
            if (input.Length == 0) input = "0";
            try
            {
                return int.Parse(input);
            }
            catch (OverflowException ex)
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;
                //Min if negative, otherwise max
                if (input[0] == "-"[0])
                    return int.MinValue;
                else
                    return int.MaxValue;
            }
            catch (FormatException ex) //User didn't input anything
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;

                return 0;
            }
        }

        /// <summary>
        /// Filtered ReadLine that only allows numbers
        /// </summary>
        /// <param name="limit">Max return value, defaults to 360</param>
        /// <param name="ignoreExceptions">To ignore user passing in nothing and default to 0</param>
        /// <returns>0 to (360 or limit)</returns>
        internal static short ReadAngle(short limit = 360, bool ignoreExceptions = false)
        {
            allowedCharacters = "1234567890".ToCharArray();
            
            allowedLength = (byte)limit.ToString().Length;

            allowNegative = false;
            
            string input = FilteredReadLine();
            if (input.Length == 0) input = "0";
            try
            {
                short angle = short.Parse(input);
                if (angle > limit)
                    throw new OverflowException("Angle is too great");
                return angle;
            }
            catch (OverflowException ex)
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;

                return limit;
            }
        }

        /// <summary>
        /// Filtered ReadLine that only allows characters that ought to be in names
        /// </summary>
        /// <param name="ignoreExceptions">To ignore user passing in nothing and default to Billy Herrington</param>
        /// <returns>A name hopefully</returns>
        internal static string ReadName(int allowedLength = 100, bool ignoreExceptions = false)
        {
            string allowedString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            allowedCharacters = (" " + allowedString.ToLower() + allowedString.ToUpper()).ToCharArray();

            ConsoleEx.allowedLength = allowedLength;

            allowedOnceCount = 0;

            allowNegative = false;

            string input = FilteredReadLine();

            try
            {
                if (input.Trim().Length < 1)
                    throw new Exception("Name cannot be empty.");
                return input;
            }
            catch (Exception ex)
            {
                //If we arent ignoring exceptions, throw it
                if (!ignoreExceptions)
                    throw ex;
                //Otherwise return a name I suppose
                return "Billy Herrington";
            }
        }
    }
}
