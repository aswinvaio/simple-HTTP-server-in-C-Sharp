using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XhtmlHandler
{
    class XhtmlToHtml
    {
        private string _xhtmlHandleBegin = @"\$\$\$";
        private string _xhtmlHandleEnd = @"\$\$\$";
        private string _xhtmlHandleExpression;
        private Type _assemblyInstanceType;
        private object _assemblyInstance;
        private string[] _keyStore;
        private string _xhtmlContent;

        public XhtmlToHtml(string xhtmlContent, string assemblyInfo)
        {
            _xhtmlHandleExpression = _xhtmlHandleBegin + @".*?" + _xhtmlHandleEnd;
            _xhtmlContent = xhtmlContent;
            _keyStore = getAllFieldKeys();

            string assemblyName = assemblyInfo.Split(',')[1].Trim();
            string assemblyClass = assemblyInfo.Split(',')[0].Trim();
            Assembly assemblyContent = Assembly.Load(assemblyName);

            _assemblyInstanceType = assemblyContent.GetType(assemblyClass);
            _assemblyInstance = Activator.CreateInstance(_assemblyInstanceType);
        }

        private string[] getAllFieldKeys()
        {
            string[] allKeys;
            Regex xhtmlHandleFinder = new Regex(_xhtmlHandleExpression);
            allKeys = xhtmlHandleFinder.Matches(_xhtmlContent).Cast<Match>().Select( m => m.Value).ToArray<string>();

            return allKeys.Distinct().ToArray<string>();
        }

        public void WriteFormData(Dictionary<string, string> formVariables)
        {
            foreach (KeyValuePair<string, string> formItem in formVariables)
            {
                if (!writeStringProperty(formItem.Key, formItem.Value))
                    throw new KeyNotFoundException();//property not Found
            }
        }

        public string PlugData()
        {
            string htmlContent = _xhtmlContent;
            foreach (string key in _keyStore)
            {
                string value = readStringProperty(key.Trim('$'));
                if (value == null)
                {
                    value = "";
                }
                Helper.FieldReplacer(ref htmlContent, _xhtmlHandleBegin + key.Trim('$') + _xhtmlHandleEnd, value);
            }

            return htmlContent;
        }

        private string readStringProperty(string propertyName)
        {
            PropertyInfo stringPropertyInfo = _assemblyInstanceType.GetProperty(propertyName);
            if (stringPropertyInfo != null)
            {
                string propertyValue = (string)stringPropertyInfo.GetValue(_assemblyInstance);
                return propertyValue;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        private bool writeStringProperty(string propertyName, string propertyValue)
        {
            PropertyInfo stringPropertyInfo = _assemblyInstanceType.GetProperty(propertyName);
            if (stringPropertyInfo != null && stringPropertyInfo.PropertyType == typeof(string))
            {
                stringPropertyInfo.SetValue(_assemblyInstance, propertyValue, null);
                return true;
            }
            else
            {
                Console.WriteLine("Property {0} is not available in the assemblyInstance!", propertyName);
                return false;
            }
        }
    }
}
