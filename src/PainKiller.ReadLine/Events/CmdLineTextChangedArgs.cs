﻿namespace PainKiller.ReadLine.Events
{
    public class CmdLineTextChangedArgs : EventArgs
    {
        public CmdLineTextChangedArgs(string cmdText) => CmdText = cmdText;
        public string CmdText { get; }
    }
}