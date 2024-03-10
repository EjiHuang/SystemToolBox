using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.Reflection;

/*
 * Extension author:Luis Perez
 * 
 * ┌ Instructions for use ──────────────────────────────────────────────────────────┐
 * │Binding:                                                                        │
 * │{Binding}                                                                       │
 * │    => {BindTo}                                                                 │
 * │{Binding Path=PathToProperty, RelativeSource={RelativeSource Self}}             │
 * │    => {BindTo PathToProperty}                                                  │
 * │{Binding Path=PathToProperty,                                                   │
 * │    RelativeSource={RelativeSource AncestorType={x:Type typeOfAncestor}}}       │
 * │    => {BindTo Ancestor.typeOfAncestor.PathToProperty}                          │
 * │{Binding Path=PathToProperty, RelativeSource={RelativeSource TemplatedParent}}  │
 * │    => {BindTo Template.PathToProperty}                                         │
 * │{Binding Path=Text, ElementName=MyTextBox}                                      │
 * │    => {BindTo #MyTextBox.Text}                                                 │
 * │{Binding Path=MyBoolVar, RelativeSource={RelativeSource Self},                  │
 * │    Converter={StaticResource InverseBooleanConverter}}                         │
 * │    => {Binding !MyBoolVar}                                                     │
 * │                                                                                │
 * │Method Binding:                                                                 │
 * │in your class                                                                   │
 * │private void SaveObject() {                                                     │
 * │do something                                                                    │
 * │}                                                                               │
 * │in your xaml                                                                    │
 * │{BindTo SaveObject()}                                                           │
 * │                                                                                │
 * └────────────────────────────────────────────────────────────────────────────────┘
 */

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "WpfBindTo")]

namespace WpfBindTo
{
    public class BindToExtension : MarkupExtension
    {
        private Binding _Binding;
        private string _Path;
        private string _MethodName;

        public BindToExtension() { }

        public BindToExtension(string path)
        {
            _Path = path;
        }

        public void ProcessPath(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(_Path))
            {
                _Binding = new Binding();
                return;
            }

            var parts = _Path.Split('.').Select(o => o.Trim()).ToArray();

            RelativeSource oRelativeSource = null;
            string sElementName = null;

            var partIndex = 0;

            // 格式：{BindTo #MyTextBox.Text}
            if (parts[0].StartsWith("#"))
            {
                // 获取对象名
                sElementName = parts[0].Substring(1);
                partIndex++;
            }
            else if ("ancestors" == parts[0].ToLower() || "ancestor" == parts[0].ToLower())
            {
                if (2 > parts.Length)
                    throw new Exception("Invalid path, expected exactly 2 identifiers ancestors.#Type#.[Path] (e.g. Ancestors.DataGrid, Ancestors.DataGrid.SelectedItem, Ancestors.DataGrid.SelectedItem.Text)");

                var sTypeName = parts[1];
                var oType = (Type)new TypeExtension(sTypeName).ProvideValue(serviceProvider);
                if (null == oType)
                    throw new Exception("Could not find type: " + sTypeName);

                oRelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, oType, 1);
                partIndex += 2;
            }
            else if ("template" == parts[0].ToLower() || "templateparent" == parts[0].ToLower() ||
                "templated" == parts[0].ToLower() || "templatedparent" == parts[0].ToLower())
            {
                oRelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                partIndex++;
            }
            else if ("thiswindow" == parts[0].ToLower())
            {
                oRelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1);
                partIndex++;
            }
            else if ("this" == parts[0].ToLower())
            {
                oRelativeSource = new RelativeSource(RelativeSourceMode.Self);
                partIndex++;
            }

            var sParts4Path = parts.Skip(partIndex);
            IValueConverter ValueConverter = null;

            if (sParts4Path.Any())
            {
                var sLastPart4Path = sParts4Path.Last();

                if (sLastPart4Path.EndsWith("()"))
                {
                    sParts4Path = sParts4Path.Take(sParts4Path.Count() - 1);
                    _MethodName = sLastPart4Path.Remove(sLastPart4Path.Length - 2);
                    ValueConverter = new CallMethodValueConverter(_MethodName);
                }
            }

            var sPath = string.Join(".", sParts4Path.ToArray());

            if (string.IsNullOrWhiteSpace(sPath))
                _Binding = new Binding();
            else
                _Binding = new Binding(sPath);

            if (null != sElementName)
                _Binding.ElementName = sElementName;

            if (null != oRelativeSource)
                _Binding.RelativeSource = oRelativeSource;

            if (null != ValueConverter)
                _Binding.Converter = ValueConverter;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 防止设计时编辑器在ProcessPath中显示与TypeExtension的用户有关的错误
            if (!(serviceProvider is IXamlTypeResolver))
                return null;

            ProcessPath(serviceProvider);
            return _Binding.ProvideValue(serviceProvider);
        }

        private class CallMethodValueConverter : IValueConverter
        {
            private string msMethodName;

            public CallMethodValueConverter(string psMethodName)
            {
                msMethodName = psMethodName;
            }

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (null == value)
                    return null;
                return new CallMethodCommand(value, msMethodName);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class CallMethodCommand : ICommand
        {
            private readonly object moObject;

            private readonly MethodInfo moMethodInfo;
            private readonly bool mbMethodAcceptsParameter;

            private readonly MethodInfo moCanMethodInfo;
            private readonly bool mbCanMethodAcceptsParameter;

            public CallMethodCommand(object poObject, string psMethodName)
            {
                moObject = poObject;

                moMethodInfo = moObject.GetType().GetMethod(psMethodName);

                if (null == moMethodInfo) return;

                var aParameters = moMethodInfo.GetParameters();
                if (2 < aParameters.Length)
                    throw new Exception("You can only bind to a methods take take 0 or 1 parameters.");

                moCanMethodInfo = moObject.GetType().GetMethod("Can" + psMethodName);
                if (null != moCanMethodInfo)
                {
                    // 判断Can方法的返回值是否为布尔型
                    if (typeof(bool) != moCanMethodInfo.ReturnType)
                        throw new Exception("'Can' method must return boolean.");

                    var aCanParameters = moMethodInfo.GetParameters();
                    if (2 < aCanParameters.Length)
                        throw new Exception("You can only bind to a methods take take 0 or 1 parameters.");
                    mbCanMethodAcceptsParameter = aParameters.Any();
                }
                mbMethodAcceptsParameter = aParameters.Any();
            }

            public bool CanExecute(object parameter)
            {
                if (null == moCanMethodInfo)
                    return true;

                var aParameters = !mbMethodAcceptsParameter ? null : new[] { parameter };
                return (bool)moCanMethodInfo.Invoke(moObject, aParameters);
            }

            #pragma warning disable 67  // CanExecuteChanged is not being used but is required by ICommand
            public event EventHandler CanExecuteChanged;
            #pragma warning restore 67  // CanExecuteChanged is not being used but is required by ICommand

            public void Execute(object parameter)
            {
                var aParameters = !mbMethodAcceptsParameter ? null : new[] { parameter };
                moMethodInfo.Invoke(moObject, aParameters);
            }
        }
    }
}
