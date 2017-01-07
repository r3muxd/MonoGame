using System;
using System.Text.RegularExpressions;

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;

namespace MonoGame.Tests
{
    internal class RegexTestFilter : FullNameFilter
    {
        private readonly Regex _regex;
        private readonly TestFilterAction _action;

        public RegexTestFilter (string regex, TestFilterAction action) : base(regex)
        {
            if (regex == null)
                throw new ArgumentNullException ("regex");
            IsRegex = true;
            _action = action;
        }

        #region ITestFilter Members

        public override bool Pass(ITest test)
        {
            return _action == TestFilterAction.Exclude ^ base.Match(test);
        }

        #endregion
    }
}

