﻿using Contentful.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Contentful.Core.Search
{
    public static class FieldHelpers<T>
    {
        public static string GetPropertyName<U>(Expression<Func<T, U>> selector)
        {
            var member = selector.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException("Provided expression must be a member type");
            }

            var memberList = new List<string>();

            while (member != null)
            {
                if (member.Type == typeof(SystemProperties))
                {
                    //filtering on sys field.
                    memberList.Add("sys");
                }
                else
                {
                    if (member.Member.CustomAttributes.Any(c => c.AttributeType == typeof(JsonPropertyAttribute)))
                    {
                        var attributeData = member.Member.CustomAttributes.First(c => c.AttributeType == typeof(JsonPropertyAttribute));

                        var propertyName = attributeData.ConstructorArguments.FirstOrDefault().Value?.ToString();

                        if (propertyName == null)
                        {
                            propertyName = attributeData.NamedArguments.FirstOrDefault(c => c.MemberName == "PropertyName").TypedValue.Value?.ToString();
                        }

                        //Still null, just go with the default.
                        if (propertyName == null)
                        {
                            propertyName = LowerCaseFirstLetterOfString(member.Member.Name);
                        }

                        memberList.Add(LowerCaseFirstLetterOfString(propertyName));
                    }
                    else
                    {
                        memberList.Add(LowerCaseFirstLetterOfString(member.Member.Name));
                    }
                }
                member = member.Expression as MemberExpression;
            }

            if (memberList.LastOrDefault() != "fields" && memberList.LastOrDefault() != "sys")
            {
                //We do not have a fields or sys object as root, probably filtering on custom type
                memberList.Add("fields");
            }
            return string.Join(".", memberList.Reverse<string>());
        }

        private static string LowerCaseFirstLetterOfString(string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}
