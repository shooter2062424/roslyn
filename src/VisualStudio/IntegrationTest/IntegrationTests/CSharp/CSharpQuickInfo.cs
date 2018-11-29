﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Test.Utilities;

namespace Roslyn.VisualStudio.IntegrationTests.CSharp
{
    [TestClass]
    public class CSharpQuickInfo : AbstractEditorTest
    {
        protected override string LanguageName => LanguageNames.CSharp;

        public CSharpQuickInfo( )
            : base( nameof(CSharpQuickInfo))
        {
        }

        [TestMethod, Ignore("https://github.com/dotnet/roslyn/issues/19914"), TestCategory(Traits.Features.QuickInfo)]
        public void QuickInfo_MetadataDocumentation()
        {
            SetUpEditor(@"
///<summary>Hello!</summary>
class Program
{
    static void Main(string$$[] args)
    {
    }
}");
            VisualStudioInstance.Editor.InvokeQuickInfo();
            Assert.AreEqual(
                "class\u200e System\u200e.String\r\nRepresents text as a sequence of UTF-16 code units.To browse the .NET Framework source code for this type, see the Reference Source.",
                VisualStudioInstance.Editor.GetQuickInfo());
        }

        [TestMethod, Ignore("https://github.com/dotnet/roslyn/issues/19914"), TestCategory(Traits.Features.QuickInfo)]
        public void QuickInfo_Documentation()
        {
            SetUpEditor(@"
///<summary>Hello!</summary>
class Program$$
{
    static void Main(string[] args)
    {
    }
}");
            VisualStudioInstance.Editor.InvokeQuickInfo();
            Assert.AreEqual("class\u200e Program\r\nHello!", VisualStudioInstance.Editor.GetQuickInfo());
        }

        [TestMethod, Ignore("https://github.com/dotnet/roslyn/issues/19914"), TestCategory(Traits.Features.QuickInfo)]
        public void International()
        {
            SetUpEditor(@"
/// <summary>
/// This is an XML doc comment defined in code.
/// </summary>
class العربية123
{
    static void Main()
    {
         العربية123$$ goo;
    }
}");
            VisualStudioInstance.Editor.InvokeQuickInfo();
            Assert.AreEqual(@"class" + '\u200e' + @" العربية123
This is an XML doc comment defined in code.", VisualStudioInstance.Editor.GetQuickInfo());
        }

        [TestMethod, Ignore("https://github.com/dotnet/roslyn/issues/19914"), TestCategory(Traits.Features.QuickInfo)]
        public void SectionOrdering()
        {
            SetUpEditor(@"
using System;
using System.Threading.Tasks;

class C
{
    /// <exception cref=""Exception""></exception>
    async Task <int> M()
    {
                return await M$$();
            }
        }");

            VisualStudioInstance.Editor.InvokeQuickInfo();
            var expected = "\u200e(awaitable\u200e)\u200e Task\u200e<int\u200e>\u200e C\u200e.M\u200e(\u200e)\u000d\u000a\u000d\u000aUsage:\u000d\u000a  int\u200e x\u200e \u200e=\u200e await\u200e M\u200e(\u200e\u200e)\u200e;\u000d\u000a\u000d\u000aExceptions:\u200e\u000d\u000a\u200e  Exception";
            Assert.AreEqual(expected, VisualStudioInstance.Editor.GetQuickInfo());
        }
    }
}
