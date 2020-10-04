﻿using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class WordToBeFirst
    {
        private string word;
        private List<string> similarWords;

        public WordToBeFirst(string word)
        {
            this.word = word;
        }

        public WordToBeFirst(string word, List<string> similarWords) : this(word)
        {
            this.similarWords = similarWords;
        }

        internal Tuple<bool,string> Matches(string t)
        {
            if (t == word)
                return new Tuple<bool, string>(true, word);

            foreach (var x in similarWords)
            {
                if (x == t)
                    return new Tuple<bool, string>(true, word);
            }

            return new Tuple<bool, string>(false, word);
        }

        internal bool IsTaken(List<string> taken)
        {
            foreach (var r in taken)
            {
                if (r == this.word)
                    return true;
            }

            return false;
        }

        internal string GetWord()
        {
            throw new NotImplementedException();
        }
    }
}