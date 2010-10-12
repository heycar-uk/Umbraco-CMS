﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.presentation.LiveEditing.Modules;
using ClientDependency.Core;
using System.Web.UI.WebControls;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.IO;
using System.Web.UI;
using umbraco.cms.businesslogic.skinning;
using ClientDependency.Core.Controls;
using umbraco.presentation.umbraco.controls;
using HtmlAgilityPack;
using umbraco.cms.businesslogic.template;
using System.Text;
using System.IO;
using System.Collections;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    [ClientDependency(200, ClientDependencyType.Javascript, "modal/modal.js", "UmbracoClient")]
    [ClientDependency(200, ClientDependencyType.Css, "modal/style.css", "UmbracoClient")]
    [ClientDependency(500, ClientDependencyType.Javascript, "LiveEditing/Modules/SkinModule/js/ModuleInjection.js", "UmbracoRoot")]
    [ClientDependency(800, ClientDependencyType.Javascript, "LiveEditing/Modules/SkinModule/js/disableInstallButtonsOnClick.js", "UmbracoRoot")]
    public class SkinModule : BaseModule
    {
        protected LabelButton m_SkinButton = new LabelButton();
        protected Panel m_SkinModal;

        protected LabelButton m_ModuleButton = new LabelButton();
        protected Panel m_ModuleModal;
        
      
        public SkinModule(LiveEditingManager manager)
            : base(manager)
        { }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();

            
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            Skin ActiveSkin = Skin.CreateFromAlias(Skinning.GetCurrentSkinAlias(nodeFactory.Node.GetCurrent().template));


            m_SkinModal = new Panel();
            m_SkinModal.ID = "LeSkinModal";
            m_SkinModal.Attributes.Add("style", "display: none");

            m_SkinModal.Controls.Add(new UserControl().LoadControl(String.Format("{0}/LiveEditing/Modules/SKinModule/SkinCustomizer.ascx", SystemDirectories.Umbraco)));

            Controls.Add(m_SkinModal);

            m_SkinButton.ID = "LeSkinButton";
            m_SkinButton.CssClass = "button";
            m_SkinButton.ToolTip = ActiveSkin != null && ActiveSkin.Dependencies.Count > 0 ? "Customize skin" : "Change skin";
            m_SkinButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/SKinModule/images/skin.gif", SystemDirectories.Umbraco);

            string s = (ActiveSkin != null && ActiveSkin.Dependencies.Count > 0 ? "setTasksClientScripts();" : "") + "jQuery('#" + m_SkinModal.ClientID + @"').show();" + "jQuery('#" + m_SkinModal.ClientID + @"').ModalWindowShowWithoutBackground('" + ui.GetText("skin") + "',true,500,400,50,0, ['.modalbuton'], null);";

            m_SkinButton.OnClientClick = s +"return false;";

            Controls.Add(m_SkinButton);

            if (UmbracoContext.Current.LiveEditingContext.InSkinningMode && !string.IsNullOrEmpty(UmbracoContext.Current.Request["umbSkinningConfigurator"]))
            {
                ScriptManager.RegisterClientScriptBlock(
                   this,
                   this.GetType(),
                   "ShowSkinModule",
                   "function ShowSkinModule(){" + s + "}",
                   true);


                ClientDependencyLoader.Instance.RegisterDependency(500, "LiveEditing/Modules/SkinModule/js/SkinModuleShowOnStartup.js", "UmbracoRoot", ClientDependencyType.Javascript);
            }

            // modules
            if (CanInsertModules(nodeFactory.Node.GetCurrent().template))
            {
                m_ModuleModal = new Panel();
                m_ModuleModal.ID = "LeModuleModal";
                m_ModuleModal.CssClass = "ModuleSelector";
                m_ModuleModal.Attributes.Add("style", "display: none");

                m_ModuleModal.Controls.Add(new UserControl().LoadControl(String.Format("{0}/LiveEditing/Modules/SKinModule/ModuleSelector.ascx", SystemDirectories.Umbraco)));

                Controls.Add(m_ModuleModal);


                m_ModuleButton.ID = "LeModuleButton";
                m_ModuleButton.CssClass = "button";
                m_ModuleButton.ToolTip = "Insert Module";
                m_ModuleButton.ImageUrl = String.Format("{0}/LiveEditing/Modules/SKinModule/images/module.gif", SystemDirectories.Umbraco);

                m_ModuleButton.OnClientClick = "umbShowModuleSelection();" + "return false;";

                Controls.Add(m_ModuleButton);
            }
        }

        private bool CanInsertModules(int template)
        {
            Template t = new Template(template);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(t.MasterPageFile);

            if (doc.DocumentNode.SelectNodes(string.Format("//*[@class = '{0}']", "umbModuleContainer")) != null)
                return true;
            else
            {
                if (t.HasMasterTemplate)
                    return CanInsertModules(t.MasterTemplate);
                else
                    return false;
            }

        }

        protected override void Manager_MessageReceived(object sender, MesssageReceivedArgs e)
        {
            switch (e.Type)
            {
                case "injectmodule":
                    //update template, insert macro tag

                    if (InsertMacroTag(nodeFactory.Node.GetCurrent().template, e.Message.Split(';')[0], e.Message.Split(';')[1], e.Message.Split(';')[2] == "prepend"))
                    {
                        //ok

                        //presentation.templateControls.Macro m = new presentation.templateControls.Macro();

                        //Hashtable DataValues = helper.ReturnAttributes(e.Message.Split(';')[1]);

                        //m.Alias = DataValues["alias"].ToString();
                        //m.MacroAttributes = DataValues;

                        //StringBuilder sb = new StringBuilder();
                        //StringWriter tw = new StringWriter(sb);
                        //HtmlTextWriter hw = new HtmlTextWriter(tw);

                        //m.RenderControl(hw);

                        //string macroOutput = sb.ToString();

                        //string placeMacroOutput = string.Format("jQuery('.umbModuleContainerPlaceHolder','#{0}').remove();jQuery('#{0}').{1}(\"{2}\");", e.Message.Split(';')[0], e.Message.Split(';')[2], macroOutput);


                        //ScriptManager.RegisterClientScriptBlock(Page, GetType(), new Guid().ToString(), placeMacroOutput, true);
                        
                    }
                    else
                    {
                        //not ok
                    }

                    break;
            }
        }

        private bool InsertMacroTag(int template, string targetId, string tag, bool prepend)
        {
            Template t = new Template(template);

            string TargetFile = t.MasterPageFile;
            string TargetID = targetId;

            HtmlDocument doc = new HtmlDocument();
            doc.Load(TargetFile);

            if (doc.DocumentNode.SelectNodes(string.Format("//*[@id = '{0}']", TargetID)) != null)
            {
                foreach (HtmlNode target in doc.DocumentNode.SelectNodes(string.Format("//*[@id = '{0}']", TargetID)))
                {
                    HtmlNode macrotag = HtmlNode.CreateNode(tag);

                    if (prepend)
                        target.PrependChild(macrotag);
                    else
                        target.AppendChild(macrotag);
                }
                doc.Save(TargetFile);

                return true;
            }
            else
            {
                //might be on master template
                if (t.HasMasterTemplate)
                    return InsertMacroTag(t.MasterTemplate, targetId, tag, prepend);
                else
                    return false;
            }
        }
    }
}