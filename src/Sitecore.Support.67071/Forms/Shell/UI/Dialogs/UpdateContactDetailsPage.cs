using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Core.Data.Helpers;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.WFFM.Abstractions.Data;
using Sitecore.WFFM.Abstractions.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
namespace Sitecore.Support.Forms.Shell.UI.Dialogs
{
    using Sitecore.Forms.Shell.UI.Dialogs;
    using Sitecore.Globalization;

    /// copy-pasted original functionality, for the most part. The method we need to change is not virtual
    public class UpdateContactDetailsPage : EditorBase
    {
        private const string MappingKey = "Mapping";

        protected System.Web.UI.WebControls.PlaceHolder Content;

        protected Sitecore.Web.UI.HtmlControls.Checkbox OverwriteContact;

        protected System.Web.UI.HtmlControls.HtmlInputHidden MappedFields;

        protected Sitecore.Web.UI.HtmlControls.Groupbox UserProfileGroupbox;

        private string FormFieldColumnHeader;

        private string ContactDetailsColumnHeader;

        public string RestrictedFieldTypes
        {
            get
            {
                return Sitecore.Web.WebUtil.GetQueryString("RestrictedFieldTypes", "{1F09D460-200C-4C94-9673-488667FF75D1}|{1AD5CA6E-8A92-49F0-889C-D082F2849FBD}|{7FB270BE-FEFC-49C3-8CB4-947878C099E5}");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.MappedFields.Value = base.GetValueByKey("Mapping");
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" if (typeof ($scw) === \"undefined\") {");
                stringBuilder.Append(" window.$scw = jQuery.noConflict(true); }");
                stringBuilder.Append(" $scw(document).ready(function () {");
                stringBuilder.Append(string.Format(" var treeData = $scw.parseJSON('{0}');", ContactFacetsHelper.GetContactFacetsXmlTree()));
                stringBuilder.Append(string.Format(" var fieldsData = $scw.parseJSON('{0}');", this.GetFieldsData(this.RestrictedFieldTypes)));
                stringBuilder.Append(string.Format(" var selectedDataValue = $scw(\"#{0}\").val();", this.MappedFields.ClientID));
                stringBuilder.Append(" var selectedData = [];");
                stringBuilder.Append(" if(selectedDataValue) {");
                stringBuilder.Append(" selectedData = $scw.parseJSON(selectedDataValue); }");
                stringBuilder.Append(" $scw(\"#treeMap\").droptreemap({");
                stringBuilder.Append(" treeData: treeData.Top,");
                stringBuilder.Append(" selected: selectedData,");
                stringBuilder.Append(" listData: fieldsData,");
                stringBuilder.Append(string.Format(" fieldsHeader: \"{0}\",", this.FormFieldColumnHeader));
                stringBuilder.Append(string.Format(" mappedKeysHeader: \"{0}\",", this.ContactDetailsColumnHeader));
                stringBuilder.Append(string.Format(" addFieldTitle: \"{0}\",", DependenciesManager.ResourceManager.Localize("ADD_FIELD")));
                stringBuilder.Append(" change: function (value) {");
                stringBuilder.Append(string.Format(" $scw(\"#{0}\").val(value);", this.MappedFields.ClientID));
                stringBuilder.Append("} });   });");
                this.Page.ClientScript.RegisterClientScriptBlock(base.GetType(), "sc_wffm_update_contact" + this.ClientID, stringBuilder.ToString(), true);
            }
        }
        /// <summary>
        /// Patched method
        /// </summary>
        /// <param name="restrictedTypes"></param>
        /// <returns></returns>
        protected string GetFieldsData(string restrictedTypes = "")
        {
            FormItem formItem = new FormItem(this.CurrentDatabase.GetItem(this.CurrentID,this.CurrentLanguage));
            IEnumerable<IFieldItem> source = from property in formItem.Fields
                                             where string.IsNullOrEmpty(restrictedTypes) || !restrictedTypes.Contains(property.TypeID.ToString())
                                             select property;
            Dictionary<string, string> source2 = source.ToDictionary((IFieldItem property) => property.ID.ToString(), (IFieldItem property) => property.Title);
            IEnumerable<string> values = from d in source2
                                         select string.Concat(new string[]
                                         {
                "{\"id\":\"",
                d.Key.Trim(new char[]
                {
                    '{',
                    '}'
                }),
                "\",\"title\":\"",
                d.Value,
                "\"}"
                                         });
            return "[" + string.Join(",", values) + "]";
        }

        protected override void Localize()
        {
            base.Header = DependenciesManager.ResourceManager.Localize("UPDATE_CONTACT_HEADER");
            base.Text = DependenciesManager.ResourceManager.Localize("UPDATE_CONTACT_DESCRIPTION");
            this.FormFieldColumnHeader = DependenciesManager.ResourceManager.Localize("FORM_FIELD");
            this.ContactDetailsColumnHeader = DependenciesManager.ResourceManager.Localize("CONTACT_DETAILS");
        }

        protected override void SaveValues()
        {
            base.SaveValues();
            base.SetValue("Mapping", this.MappedFields.Value);
        }
    }
}