// Text Box Stream Writer
// Dan Jackson, 2011-2012

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Deploy
{
    public class TextBoxStreamWriter : TextWriter
    {
        //(new TextBoxStreamWriter(textBoxLog)).SetConsoleOut();
        //Console.WriteLine("Started.");

        //Trace.Listeners.Add(new ConsoleTraceListener());
        //Trace.WriteLine("Trace started.");

        private delegate void OutputDelegate(string message);

        private TextBox textBox;
        private int maxLength = 30000;
        private TextWriter oldOut = null;
        private TextWriter oldError = null;

        public TextBoxStreamWriter(TextBox textBox)
        {
            this.textBox = textBox;
            this.textBox.Disposed += TextBox_Disposed;
        }

        private void TextBox_Disposed(object sender, EventArgs e)
        {
            this.textBox = null;
            RestoreConsoleOut();
        }

        public void SetConsoleOut()
        {
            oldOut = Console.Out;
            oldError = Console.Error;
            Console.SetOut(this);
            Console.SetError(this);
        }

        public void RestoreConsoleOut()
        {
            this.textBox = null;
            if (oldOut != null)
            {
                Console.SetOut(this.oldOut);
            }
            if (oldError != null)
            {
                Console.SetError(this.oldError);
            }
        }

        public override void Write(string message)
        {
            if (textBox != null && !textBox.IsDisposed)
            {
                if (textBox.InvokeRequired)
                {
                    textBox.BeginInvoke(new OutputDelegate(Write), message);
                }
                else
                {
                    bool caretAtEnd = (textBox.SelectionStart == textBox.Text.Length && textBox.SelectionLength == 0);

                    if (oldOut != null) { oldOut.Write(message); }

                    if (textBox.Text.Length + message.Length > maxLength)
                    {
                        int remove = textBox.Text.Length + message.Length - maxLength;
                        if (remove < 0) { remove = 0; }
                        if (remove > textBox.Text.Length) { remove = textBox.Text.Length; }
                        textBox.Text = textBox.Text.Remove(0, remove);
                    }
                    textBox.AppendText(message);

                    if (caretAtEnd)
                    {
                        textBox.Select(textBox.Text.Length, 0);
                        textBox.ScrollToCaret();
                    }
                }
            }
        }

        public override void Write(char value)
        {
            this.Write(value.ToString());
        }

        public override void WriteLine(string message)
        {
            this.Write(message + NewLine);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

    }

}
