using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using RazorEngine;
using RazorEngine.Templating;
using WorkingTimeTracker.Core.Entities;

namespace WorkingTimeTracker.Web.Templates
{
    public class TimeEntrySummaryReportTemplating
    {
        private Lazy<string> template = new Lazy<string>(
            () =>
            {
                return File.ReadAllText(ConfigurationManager.AppSettings["TimeEntrySummaryReportTemplate"]);
            }
        );

        public string Parse(TimeEntrySummaryReportItem[] items)
        {
            return Engine.Razor.RunCompile(template.Value, "TimeEntrySummaryReportTemplate", model: items);
        }
    }
}