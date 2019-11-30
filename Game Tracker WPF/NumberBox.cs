using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameTracker
{
    //this class is intended to create a text box that only allows numbers
    class NumberBox : TextBox
    {
        public NumberBox()
        {
            TextChanged += new TextChangedEventHandler(OnTextChanged);
            KeyDown += new KeyEventHandler(OnKeyDown);
        }

        new public String Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = LeaveOnlyNumbers(value);
            }
        }

        //checks if the key pressed is a number
        private bool IsNumberKey(System.Windows.Input.Key inKey)
        {
            //if the key isn't 0-9
            if (inKey < Key.D0 || inKey > Key.D9)
            {
                //if the key isn't numpad 0-9, key isn't a number return false
                if(inKey < Key.NumPad0 || inKey > Key.NumPad9)
                return false;
            }
            //key pressed is a number return true
            return true;
        }

        //checks if delete or backspace is pressed
        private bool IsDelOrBackspaceKey(Key inKey)
        {
            return inKey == Key.Delete || inKey == Key.Back;
        }

        //checks if period key is pressed allowing decimal values
        private bool IsPeriodKey(Key inKey)
        {
            //count current amount of decimal points in text to prevent multiple points (ex: "1.0.0")
            int count = 0;
            foreach (char c in base.Text.ToCharArray())
            {
                //add to count if '.' is found
                if (c == '.')
                {
                    count += 1;
                }
            }
            //if a decimal point is found do not allow another
            if(count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //removes characters from the string that are not '.' or 0-9
        private String LeaveOnlyNumbers(String inString)
        {
            String tmp = inString;
            foreach(char c in inString.ToCharArray())
            {
                //if character doesn't match regular expression change character to ""
                if (!System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), "^[.0-9]*$"))
                {
                    tmp = tmp.Replace(c.ToString(), "");
                }
            }
            //return new string
            return tmp;
        }

        //check key validity on key press
        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            //check if key is a number/del/backspace/period
            e.Handled = !IsNumberKey(e.Key) && !IsDelOrBackspaceKey(e.Key) && !IsPeriodKey(e.Key);
        }

        //check text validity when changed via copy/paste
        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //remove characters that aren't numbers from new text
            base.Text = LeaveOnlyNumbers(Text);
        }
    }
}
