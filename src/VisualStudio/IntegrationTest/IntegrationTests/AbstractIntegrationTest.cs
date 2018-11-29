﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Test.Apex.VisualStudio;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Harness;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Input;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roslyn.VisualStudio.IntegrationTests
{
    public abstract class AbstractIntegrationTest : VisualStudioHostTest, IDisposable
    {
        protected readonly string ProjectName = "TestProj";
        protected readonly string SolutionName = "TestSolution";

        private readonly MessageFilter _messageFilter;
           private readonly VisualStudioInstanceFactory _instanceFactory;
           private VisualStudioInstanceContext _visualStudioContext;

        /// <summary>
        /// Identifies the first time a Visual Studio instance is launched during an integration test run.
        /// </summary>
        private static bool _firstLaunch = true;

        protected AbstractIntegrationTest()
        {
            Assert.AreEqual(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());

            // Install a COM message filter to handle retry operations when the first attempt fails
            _messageFilter = RegisterMessageFilter();

            try
            {
                Helper.Automation.TransactionTimeout = 20000;
            }
            catch
            {
                _messageFilter.Dispose();
                _messageFilter = null;
                throw;
            }
        }

        public VisualStudioInstance VisualStudioInstance { get; private set; }

        public virtual void Initialize()
        {
            try
            {
                var requiredPackageIds = SharedIntegrationHostFixture.RequiredPackageIds;
                var visualStudio = this.Operations.CreateHost<VisualStudioHost>();
                //foreach(var package in requiredPackageIds)
                //{
                //    visualStudio.Configuration.AddCompositionAssembly(package);
                //}



               _visualStudioContext = _instanceFactory.GetNewOrUsedInstanceAsync(VisualStudio, SharedIntegrationHostFixture.RequiredPackageIds);
                VisualStudioInstance = _visualStudioContext.
                    new VisualStudioInstance(visualStudio, supportedPackageIds, visualStudio.ProcessStartInfo.ExecutablePath);
            }
            catch
            {
                _messageFilter.Dispose();
                throw;
            }
        }

        /// <summary>
        /// This method implements <see cref="IAsyncLifetime.DisposeAsync"/>, and is used for releasing resources
        /// created by <see cref="IAsyncLifetime.InitializeAsync"/>. This method is only called if
        /// <see cref="InitializeAsync"/> completes successfully.
        /// </summary>
        public virtual Task DisposeAsync()
        {
            _visualStudioContext.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual MessageFilter RegisterMessageFilter()
            => new MessageFilter();

        protected void Wait(double seconds)
        {
            var timeout = TimeSpan.FromMilliseconds(seconds * 1000);
            Thread.Sleep(timeout);
        }

        /// <summary>
        /// This method provides the implementation for <see cref="IDisposable.Dispose"/>. This method via the
        /// <see cref="IDisposable"/> interface (i.e. <paramref name="disposing"/> is <see langword="true"/>) if the
        /// constructor completes successfully. The <see cref="InitializeAsync"/> may or may not have completed
        /// successfully.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageFilter.Dispose();
            }
        }

        protected KeyPress Ctrl(VirtualKey virtualKey)
            => new KeyPress(virtualKey, ShiftState.Ctrl);

        protected KeyPress Shift(VirtualKey virtualKey)
            => new KeyPress(virtualKey, ShiftState.Shift);

        protected KeyPress Alt(VirtualKey virtualKey)
            => new KeyPress(virtualKey, ShiftState.Alt);
    }
}
