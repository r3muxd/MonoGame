// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;

namespace MonoGame.Tests
{
    internal class RegexTestFilter : FullNameFilter
    {
        private readonly TestFilterAction _action;

        public RegexTestFilter (string regex, TestFilterAction action) : base(regex)
        {
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

