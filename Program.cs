/*
    Copyright (c) 2010-2015 by Genstein and Jason Lautzenheiser.

    This file is (or was originally) part of Trizbort, the Interactive Fiction Mapper.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Trizbort
{
  internal static class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);


      List<int> values = new List<int> {10, 20, 25};
      List<string> strings = new List<string> {"a", "b", "c", "d", "e", "f"};

      string newString = string.Join(",", values);
//      string newString = string.Join(",", values.ToArray());

      string delimited = string.Join(",", strings.ToArray(), 2, 2);
      var options = new Options();
      if (CommandLine.Parser.Default.ParseArguments(args, options))
      {
        if (options.TrizbortFile != string.Empty) Console.WriteLine("Trizbort: {0}", options.TrizbortFile);
      }
      else
      {
        loadMainForm();
      }
    }

    private static void loadMainForm()
    {
      using (var form = new MainForm())
      {
        Application.Run(form);
      }
    }
  }
}