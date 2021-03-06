﻿namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class UserIdFound
    {
        private int? i = null;
        private string v = null;

        public UserIdFound(int? i)
        {
            this.i = i;
        }

        public UserIdFound(int? i, string v) : this(i)
        {
            this.v = v;
        }

        internal int? GetID()
        {
            return i;
        }

        internal string getError()
        {
            return v;
        }
    }
}