﻿// Copyright (C) 2011, Mehdi Khalili
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the <organization> nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using System.Text;
using Bddify.Core;

namespace Bddify.Reporters
{
    public class HtmlReportBuilder
    {
        readonly HtmlReportViewModel _viewModel;
        readonly StringBuilder _html;
        const int TabIndentation = 2;
        int _tabCount;

        public HtmlReportBuilder(HtmlReportViewModel viewModel)
        {
            _viewModel = viewModel;
            _html = new StringBuilder();
        }

        public string BuildReportHtml()
        {
            AddLine("<!DOCTYPE html>");
            using(OpenTag("html"))
            {
                Header();
                Body();
            }

            return _html.ToString();
        }

        private void Header()
        {
            using(OpenTag("head"))
            {
                AddLine("<meta charset='utf-8'/>");
                AddLine("<link href='bddify.css' rel='stylesheet'/>");
                AddLine("<script type='text/javascript' src='jquery-1.7.1.min.js'></script>");
                AddLine(string.Format("<title>Bddify Test Result {0}</title>", DateTime.Now.ToShortDateString()));
            }
        }

        private void Body()
        {
            using(OpenTag("body"))
            {
                using(OpenTag("<div id='main'>", "div"))
                {
                    ResultSummary();

                    ResultDetails();
                }

                Footer();
            }
        }

        private void ResultSummary()
        {
            using (OpenTag("header"))
            {
                AddLine(string.Format("<div id='bddifyTitle'>{0}</div>", _viewModel.Configuration.ReportHeader));
                AddLine(string.Format("<div id='bddifyDescription'>{0}</div>", _viewModel.Configuration.ReportDescription));
            }

            using (OpenTag("<section class='summary'>", "section"))
            {
                using (OpenTag("<ul class='resultSummary'>", "ul"))
                {
                    AddResultListItem("namespace", "Namespaces", _viewModel.Results.Namespaces);
                    AddResultListItem("story", "Stories", _viewModel.Results.Stories);
                    AddResultListItem("Passed", "Passed", _viewModel.Results.Passed);
                    AddResultListItem("Failed", "Failed", _viewModel.Results.Failed);
                    AddResultListItem("Inconclusive", "Inconclusive", _viewModel.Results.Inconclusive);
                    AddResultListItem("NotImplemented", "Not Implemented", _viewModel.Results.NotImplemented);
                    AddResultListItem("NotExecuted", "Not Executed", _viewModel.Results.NotExecuted);
                }
            }
        }

        private void ExpandCollapse()
        {
            using (OpenTag("<div id='expandCollapse'>", "div"))
            {
                AddLine("<a href='#' class='expandAll'>[expand all]</a>");
                AddLine("<a href='#' class='collapseAll'>[collapse all]</a>");
            }
        }

        private void ResultDetails()
        {
            using (OpenTag("<div id='testResult'>", "div"))
            {
                ExpandCollapse();

                using (OpenTag("<ul class='testResult'>", "ul"))
                {
                    foreach (var scenarioGroup in _viewModel.GroupedScenarios)
                    {
                        AddStory(scenarioGroup);
                    }
                }

                AddLine(string.Format("<p><span>Tested at: {0}</span></p>", DateTime.Now));
            }
        }

        private void Footer()
        {
            string footer = @"    <footer>Powered by <a href='https://code.google.com/p/bddify/'>bddify</a> framework</footer>
		<script type='text/javascript'>
		$(document).ready(function() {
			$('.expandAll').click(function() {
				$('.steps').css('display', '');
			});
			$('.collapseAll').click(function() {
				$('.steps').css('display', 'none');
			});
		});
		  function toggle(id) {
		    var e = document.getElementById(id);
		    if (e.style.display == 'none') {
		      e.style.display = '';
		    }
		    else {
		      e.style.display = 'none';
		    }
		  }
		</script>";
            _html.AppendLine(footer);
        }

        private HtmlReportTag OpenTag(string openingTag, string tagName)
        {
            AddLine(openingTag);
            _tabCount++;
            return new HtmlReportTag(tagName, CloseTag);
        }

        private HtmlReportTag OpenTag(string tagName)
        {
            AddLine("<" + tagName + ">");
            _tabCount++;
            return new HtmlReportTag(tagName, CloseTag);
        }

        private void CloseTag(string tagName)
        {
            _tabCount--;
            AddLine(tagName);
        }

        private void AddLine(string line)
        {
            int tabWidth = _tabCount * TabIndentation;
            _html.AppendLine(string.Empty.PadLeft(tabWidth) + line);
        }

        private void AddResultListItem(string cssClass, string label, int count)
        {
            using (OpenTag(string.Format("<li class='{0}'>", cssClass), "li"))
            {
                using (OpenTag("<div class='summary'>", "div"))
                {
                    AddLine(string.Format("<div class='summaryLabel'>{0}</div>", label));
                    AddLine(string.Format("<span class='summaryCount'>{0}</span>", count));
                }
            }
        }

        private void AddStory(IGrouping<string, Story> scenarioGroup)
        {
            var story = scenarioGroup.First();
            var scenariosInGroup = scenarioGroup.SelectMany(s => s.Scenarios);
            var storyResult = (StepExecutionResult)scenariosInGroup.Max(s => (int)s.Result);

            using (OpenTag("li"))
            {
                using (OpenTag(string.Format("<div class='story {0}'>", storyResult), "div"))
                {
                    AddStoryMetaDataAndNarrative(story);

                    using (OpenTag("<div class='scenarios'>", "div"))
                    {
                        foreach (var scenario in scenariosInGroup)
                        {
                            AddScenario(scenario);
                        }
                    }
                }
            }
        }

        private void AddScenario(Scenario scenario)
        {
            using (OpenTag(string.Format("<div class='scenario {0}' onclick='toggle(\"{1}\");'>", scenario.Result, scenario.Id), "div"))
            {
                AddLine(string.Format("<div class='scenarioTitle'>{0}</div>", scenario.ScenarioText));

                using(OpenTag(string.Format("<ul class='steps' id='{0}' style='display:none'>", scenario.Id), "ul"))
                {
                foreach (var step in scenario.Steps.Where(s => s.ShouldReport))
                {
                    string stepClass = string.Empty;
                    string result = step.StepTitle;
                    var reportException = step.Exception != null && step.Result == StepExecutionResult.Failed;
                    if (reportException)
                    {
                        stepClass = step.Result + "Exception";
                        if (!string.IsNullOrEmpty(step.Exception.Message))
                        {
                            result += " [Exception Message: '" + step.Exception.Message + "']";
                        }
                    }

                    using (OpenTag(string.Format("<li class='step {0} {1} {2}' onclick='toggle(\"{3}\");'>", step.Result, stepClass, step.ExecutionOrder, step.Id), "li"))
                    {
                        AddLine(string.Format("<span>{0}</span>", result));
                        if (reportException)
                        {
                            using (OpenTag(string.Format("<div class='step {0}' id='{1}'>", stepClass, step.Id), "div"))
                            {
                                AddLine(string.Format("<code>{0}</code>", step.Exception.StackTrace));
                            }
                        }
                    }
                }


                }
            }
        }

        private void AddStoryMetaDataAndNarrative(Story story)
        {
            using (OpenTag("<div class='storyMetaData'>", "div"))
            {
                if (story.MetaData == null)
                {
                    var @namespace = story.Scenarios.First().TestObject.GetType().Namespace;
                    AddLine(string.Format("div class='namespaceName'>{0}</div>", @namespace));
                }
                else
                {
                    AddLine(string.Format("<div class='storyTitle'>{0}</div>", story.MetaData.Title));
                }

                if (story.MetaData != null && !string.IsNullOrEmpty(story.MetaData.AsA))
                {
                    using (OpenTag("<ul class='storyNarrative'>", "ul"))
                    {
                        AddLine(string.Format("<li>{0}</li>", story.MetaData.AsA));
                        AddLine(string.Format("<li>{0}</li>", story.MetaData.IWant));
                        AddLine(string.Format("<li>{0}</li>", story.MetaData.SoThat));
                    }
                }
            }
        }

        class HtmlReportTag : IDisposable
        {
            private readonly string _tagName;
            private readonly Action<string> _closeTagAction;

            public HtmlReportTag(string tagName, Action<string> closeTagAction)
            {
                _tagName = "</" + tagName + ">";
                _closeTagAction = closeTagAction;
            }

            public void Dispose()
            {
                _closeTagAction(_tagName);
            }
        }
    }
}
