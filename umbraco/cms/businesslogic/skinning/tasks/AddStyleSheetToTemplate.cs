﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces.skinning;
using HtmlAgilityPack;
using umbraco.IO;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class AddStyleSheetToTemplate : TaskType
    {
        public string TargetFile { get; set; }
        public string StyleSheet { get; set; }
        public string Media { get; set; }

        public AddStyleSheetToTemplate()
        {
            this.Name = "Add StyleSheet To Template";
            this.Description = "Will add an additional stylesheet to a template";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();

            //open template

            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);


            HtmlNode head = doc.DocumentNode.SelectSingleNode("//head");

            if (head != null)
            {
                HtmlNode s = new HtmlNode(HtmlNodeType.Element, doc, 0);
                s.Name = "link";
                s.Attributes.Add("rel", "stylesheet");
                s.Attributes.Add("type", "text/css");

                if(string.IsNullOrEmpty(StyleSheet))
                    s.Attributes.Add("href", Value);
                else
                    s.Attributes.Add("href", StyleSheet);

                if(!string.IsNullOrEmpty(Media))
                    s.Attributes.Add("media", Media);

                head.AppendChild(s);
            }

          

            doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            d.TaskExecutionStatus = TaskExecutionStatus.Completed;
            d.NewValue = Value;
            //save

            return d;
        }

        public override TaskExecutionStatus RollBack(string OriginalValue)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            HtmlNode s = doc.DocumentNode.SelectSingleNode(string.Format("//link [@href = '{0}']", string.IsNullOrEmpty(StyleSheet) ? Value : StyleSheet));

            s.Remove();

            doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            return TaskExecutionStatus.Completed;
        }

        public override string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {
            return string.Format(
                   @"jQuery('#{0}').bind('{2}', function() {{ 
                        var link = $('<link>');
                        link.attr({{
                                type: 'text/css',
                                rel: 'stylesheet',
                                {3}
                                href:{1}
                        }});
                        $('head').append(link); 
                }});",
                   ControlClientId,
                   ClientSideGetValueScript,
                   ClientSidePreviewEventType,
                   string.IsNullOrEmpty(Media) ? "" : string.Format("media :''{0}",Media));
        }
    }
}
