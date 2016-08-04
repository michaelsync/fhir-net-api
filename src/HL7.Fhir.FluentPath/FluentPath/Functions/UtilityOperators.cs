﻿using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Furore.MetaModel;

namespace Hl7.Fhir.FluentPath.Functions
{
    internal static class UtilityOperators
    {
        static Action<string> WriteLine;
         
        public static IEnumerable<IValueProvider> Extension(this IEnumerable<IValueProvider> focus, string url)
        {
            return focus.Navigate("extension").Where(es => es.Navigate("url").Single().IsEqualTo(new ConstantValue(url)));
        }

        public static IEnumerable<IValueProvider> Trace(this IEnumerable<IValueProvider> focus, string name)
        {
            WriteLine("=== Trace {0} ===".FormatWith(name));

            if (focus == null)
                WriteLine("(null)");

            else if (focus is IEnumerable<IValueProvider>)
            {
                WriteLine("Collection:".FormatWith(name));
                foreach (var element in (IEnumerable<IValueProvider>)focus)
                {
                    if (element.Value != null)
                        WriteLine("   " + element.Value.ToString());
                }
            }
            else if (focus is IValueProvider)
            {
                var element = (IValueProvider)focus;
                WriteLine("Value:".FormatWith(name));

                if (element.Value != null)
                {
                    WriteLine(element.Value.ToString());
                }
            }
            else
                WriteLine(focus.ToString());

            WriteLine(Environment.NewLine);

            return focus;
        }
    }
}