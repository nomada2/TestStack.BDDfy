﻿using System;
using Bddify.Configuration;
using Bddify.Core;
using Bddify.Processors;
using Bddify.Processors.HtmlReporter;
using NUnit.Framework;
using System.Linq;

namespace Bddify.Tests.Configuration
{
    [TestFixture]
    public class BatchProcessorsTests
    {
        [Test]
        public void ReturnsHtmlReporterByDefault()
        {
            var processors = Configurator.BatchProcessors.GetProcessors().ToList();
            Assert.IsTrue(processors.Any(p => p is HtmlReporter));
        }

        [Test]
        public void DoesNotReturnMarkDownReporterByDefault()
        {
            var processors = Configurator.BatchProcessors.GetProcessors().ToList();
            Assert.IsFalse(processors.Any(p => p is MarkDownReporter));
        }

        [Test]
        public void DoesNotReturnHtmlReporterWhenItIsDeactivated()
        {
            Configurator.BatchProcessors.HtmlReporter.Disable();
            var processors = Configurator.BatchProcessors.GetProcessors().ToList();

            Assert.IsFalse(processors.Any(p => p is HtmlReporter));
            Configurator.BatchProcessors.HtmlReporter.Enable();
        }

        [Test]
        public void ReturnsMarkdownReporterWhenItIsActivated()
        {
            Configurator.BatchProcessors.MarkDownReport.Enable();
            var processors = Configurator.BatchProcessors.GetProcessors().ToList();

            Assert.IsTrue(processors.Any(p => p is MarkDownReporter));
            Configurator.BatchProcessors.MarkDownReport.Disable();
        }
    }
}