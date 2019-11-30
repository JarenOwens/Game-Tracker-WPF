using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameTracker
{
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

        private bool IsNumberKey(System.Windows.Input.Key inKey)
        {
            if (inKey < Key.D0 || inKey > Key.D9)
            {
                if(inKey < Key.NumPad0 || inKey > Key.NumPad9)
                return false;
            }
            return true;
        }

        private bool IsDelOrBackspaceKey(Key inKey)
        {
            return inKey == Key.Delete || inKey == Key.Back;
        }

        private bool IsPeriodKey(Key inKey)
        {
            int count = 0;
            foreach (char c in base.Text.ToCharArray())
            {
                if (c == '.')
                {
                    count += 1;
                }
            }
            if(count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private String LeaveOnlyNumbers(String inString)
        {
            String tmp = inString;
            foreach(char c in inString.ToCharArray())
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), "^[.0-9]*$"))
                {
                    tmp = tmp.Replace(c.ToString(), "");
                }
            }
            return tmp;
        }

        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !IsNumberKey(e.Key) && !IsDelOrBackspaceKey(e.Key) && !IsPeriodKey(e.Key);
        }

        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            base.Text = LeaveOnlyNumbers(Text);
        }
    }
}
