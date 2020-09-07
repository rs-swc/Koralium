﻿using Koralium.SqlToExpression.Exceptions;
using Koralium.SqlToExpression.Models;
using Koralium.SqlToExpression.Stages.CompileStages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Koralium.SqlToExpression.Utils
{
    internal static class MemberUtils
    {
        public static Expression GetMember(IQueryStage previousStage, List<string> identifiers)
        {
            Debug.Assert(previousStage != null, $"{nameof(previousStage)} was null");
            Debug.Assert(identifiers != null, $"{nameof(identifiers)} was null");
            Debug.Assert(identifiers.Count > 0, $"{nameof(identifiers)} was empty");

            return GetMember_Internal(identifiers, previousStage.FromAliases, previousStage.TypeInfo, previousStage.ParameterExpression);
        }

        public static Expression GetMemberGroupByInValue(GroupedStage previousStage, List<string> identifiers)
        {
            Debug.Assert(previousStage != null, $"{nameof(previousStage)} was null");
            Debug.Assert(identifiers != null, $"{nameof(identifiers)} was null");
            Debug.Assert(identifiers.Count > 0, $"{nameof(identifiers)} was empty");

            return GetMember_Internal(identifiers, previousStage.FromAliases, previousStage.TypeInfo, previousStage.ValueParameterExpression);
        }

        public static Expression GetMemberGroupByInKey(GroupedStage previousStage, List<string> identifiers)
        {
            Debug.Assert(previousStage != null, $"{nameof(previousStage)} was null");
            Debug.Assert(identifiers != null, $"{nameof(identifiers)} was null");
            Debug.Assert(identifiers.Count > 0, $"{nameof(identifiers)} was empty");

            return GetMember_Internal(identifiers, previousStage.FromAliases, previousStage.KeyTypeInfo, previousStage.KeyParameterExpression);
        }

        public static List<string> RemoveAlias(IQueryStage previousStage, List<string> identifiers)
        {
            if (previousStage.FromAliases.AliasExists(identifiers[0]))
            {
                if (identifiers.Count < 2)
                {
                    throw new SqlErrorException("Only got an alias as a order by column");
                }
                identifiers = identifiers.GetRange(1, identifiers.Count - 1);
            }
            return identifiers;
        }

        private static Expression GetMember_Internal(
            List<string> identifiers,
            in FromAliases fromAliases,
            in SqlTypeInfo typeInfo,
            in Expression parameterExpression)
        {
            //if (fromAliases.AliasExists(identifiers[0]))
            //{
            //    if (identifiers.Count < 2)
            //    {
            //        throw new SqlErrorException("Only got an alias as a order by column");
            //    }
            //    identifiers = identifiers.GetRange(1, identifiers.Count - 1);
            //}

            if (!typeInfo.TryGetProperty(identifiers[0], out var property))
            {
                throw new SqlErrorException($"Column {identifiers[0]} was not found, maybe it is not in the group by?");
            }

            var memberAccess = Expression.MakeMemberAccess(parameterExpression, property);

            for (int i = 1; i < identifiers.Count; i++)
            {
                memberAccess = Expression.MakeMemberAccess(memberAccess, GetTypeProperty(memberAccess.Type, identifiers[i]));
            }
            return memberAccess;
        }

       

        public static PropertyInfo GetTypeProperty(Type type, string property)
        {
            var propertyInfo = type.GetTypeInfo().GetProperty(property, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            return propertyInfo;
        }
    }
}
