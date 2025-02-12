﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.PropertyGrid.Services;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;

namespace Avalonia.PropertyGrid.Utils
{
    /// <summary>
    /// Class EnumUtils.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the enum values as <see cref="EnumValueWrapper" /> array.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>EnumValueWrapper[].</returns>
        public static EnumValueWrapper[] GetEnumValues(Type enumType, IEnumerable<Attribute>? attributes = null)
        {
            Debug.Assert(enumType is { IsEnum: true });

            return enumType.GetEnumValues()
                .Cast<Enum>()
                .Zip(enumType.GetEnumNames(), (en, name) => new { Enum = en, Name = name })
                .Where(x =>
                {
                    var fieldInfo = enumType.GetField(x.Name);
                    return fieldInfo?.IsDefined<EnumExcludeAttribute>() == false && (attributes == null || IsValueAllowed(attributes, x.Enum));
                })
                .Select(x => CreateEnumValueWrapper(x.Enum, x.Name))
                .ToArray();
        }

        /// <summary>
        /// Gets the enum values as <see cref="EnumValueWrapper"/> array.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>EnumValueWrapper[].</returns>
        public static EnumValueWrapper[] GetEnumValues<T>() where T : Enum
        {
            return GetEnumValues(typeof(T));
        }

        /// <summary>
        /// Creates an <see cref="EnumValueWrapper"/> for the specified enum value.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="enumValueName">The text to display for the enum name.</param>
        /// <returns>The created <see cref="EnumValueWrapper"/>.</returns>
        private static EnumValueWrapper CreateEnumValueWrapper(Enum enumValue, string? enumValueName = null)
        {
            var wrapper = enumValueName == null
                ? new EnumValueWrapper(enumValue)
                : new EnumValueWrapper(enumValue, enumValueName);

            try
            {
                wrapper.DisplayName = LocalizationService.Default[wrapper.DisplayName];
            }
            catch
            {
                wrapper.DisplayName = enumValueName ?? enumValue.ToString()!;
            }

            return wrapper;
        }

        /// <summary>
        /// Gets the unique flags of an enum, excluding those marked with <see cref="EnumExcludeAttribute" />.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="flags">The flags.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> GetUniqueFlagsExcluding<T>(this T flags, IEnumerable<Attribute>? attributes = null) where T : Enum
        {
            var enumType = flags.GetType();
            foreach (Enum value in Enum.GetValues(enumType))
            {
                var fieldInfo = enumType.GetField(value.ToString());
                if (fieldInfo != null && !fieldInfo.IsDefined<EnumExcludeAttribute>() && flags.HasFlag(value) && (attributes == null || IsValueAllowed(attributes, value)))
                {
                    yield return (T)value;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified enum value is allowed based on attributes implementing <see cref="IEnumValueAuthorizeAttribute"/>.
        /// </summary>
        /// <param name="attributes">all attributes.</param>
        /// <param name="value">The enum value.</param>
        /// <returns><c>true</c> if the value is allowed; otherwise, <c>false</c>.</returns>
        public static bool IsValueAllowed(IEnumerable<Attribute> attributes, Enum value)
        {
            foreach (var attribute in attributes.OfType<IEnumValueAuthorizeAttribute>())
            {
                if (!attribute.AllowValue(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}