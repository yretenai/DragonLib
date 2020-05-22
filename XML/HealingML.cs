using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public static class HealingML
    {
        private static readonly Dictionary<Type, MemberInfo[]> TypeCache = new Dictionary<Type, MemberInfo[]>();
        private static readonly Dictionary<Type, HMLSerializationTarget> TargetCache = new Dictionary<Type, HMLSerializationTarget>();

        public static string? HMLNamespace = "dragon";

        public static string CreateNamespacedTag(string? tag)
        {
            if (tag == null) return "";
            return HMLNamespace == null ? tag : $"{HMLNamespace}:{tag}";
        }

        public static string? Print(object? instance, IReadOnlyDictionary<Type, IHMLSerializer>? customTypeSerializers = null) => Print(instance, customTypeSerializers ?? new Dictionary<Type, IHMLSerializer>(), new HashSet<object?>(), new SpaceIndentHelper(), null);

        public static string? Print(object? instance, IReadOnlyDictionary<Type, IHMLSerializer> customTypeSerializers, HashSet<object?> visited, IndentHelperBase indents, string? valueName)
        {
            var type = instance?.GetType();
            IHMLSerializer? customSerializer = null;
            var target = GetCustomSerializer(customTypeSerializers, type, ref customSerializer);

            var hmlNameTag = string.Empty;
            if (!string.IsNullOrWhiteSpace(valueName)) hmlNameTag = $" {CreateNamespacedTag("name")}=\"{valueName}\"";

            var innerIndent = indents + 1;

            switch (target)
            {
                case HMLSerializationTarget.Null:
                    return $"{indents}<{CreateNamespacedTag("null")}{hmlNameTag} />\n";
                case HMLSerializationTarget.Object when type != null && customSerializer != null:
                case HMLSerializationTarget.Array when type != null && customSerializer != null:
                    return customSerializer.Print(instance, customTypeSerializers, visited, indents, valueName) as string;
                case HMLSerializationTarget.Value when type != null:
                    return $"{indents}<{FormatName(type?.Name)}>{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(instance, customTypeSerializers, visited, innerIndent, valueName))}</{FormatName(type?.Name)}>\n";
                case HMLSerializationTarget.Array when type != null:
                case HMLSerializationTarget.Enumerable when type != null:
                    if (visited.Add(instance))
                    {
                        var tag = $"{indents}<{CreateNamespacedTag("array")} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag}>\n";
                        if (target == HMLSerializationTarget.Enumerable && instance is IEnumerable enumerable) instance = enumerable.Cast<object>().ToArray();
                        if (!(instance is Array array))
                            tag += $"{innerIndent}<{CreateNamespacedTag("null")} />\n";
                        else
                            for (long i = 0; i < array.LongLength; ++i)
                                tag += Print(array.GetValue(i), customTypeSerializers, visited, innerIndent, null);

                        tag += $"{indents}</{CreateNamespacedTag("array")}>\n";
                        return tag;
                    }
                    else
                    {
                        return $"{indents}<{CreateNamespacedTag("ref")} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag} />\n";
                    }
                case HMLSerializationTarget.Object when type != null:
                    if (visited.Add(instance))
                    {
                        var tag = $"{indents}<{FormatName(type?.Name)} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag}";
                        var members = GetMembers(type);
                        var complexMembers = new List<(object? value, string memberName, IHMLSerializer? custom)>();
                        foreach (var member in members)
                        {
                            var value = GetMemberValue(instance, member);
                            var valueType = value?.GetType();
                            IHMLSerializer? targetCustomSerializer = null;
                            var targetMemberTarget = GetCustomSerializer(customTypeSerializers, valueType, ref targetCustomSerializer);

                            if (targetMemberTarget >= HMLSerializationTarget.Complex)
                                complexMembers.Add((value, member.Name, targetCustomSerializer));
                            else
                                tag += $" {member.Name}=\"{(targetCustomSerializer != null ? targetCustomSerializer.Print(value, customTypeSerializers, visited, indents, member.Name) : FormatValueType(value))}\"";
                        }

                        if (complexMembers.Count == 0)
                        {
                            tag += " />\n";
                        }
                        else
                        {
                            tag += ">\n";
                            foreach (var (value, name, custom) in complexMembers) tag += custom != null ? custom.Print(value, customTypeSerializers, visited, innerIndent, name) : Print(value, customTypeSerializers, visited, innerIndent, name);

                            tag += $"{indents}</{FormatName(type?.Name)}>\n";
                        }

                        return tag;
                    }
                    else
                    {
                        return $"{indents}<{CreateNamespacedTag("ref")} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag} />\n";
                    }
                case HMLSerializationTarget.Dictionary when type != null:
                    if (visited.Add(instance))
                    {
                        var hmlKeyTag = string.Empty;
                        var hmlValueTag = string.Empty;

                        var @base = type;
                        while (@base != null)
                        {
                            if (@base.IsConstructedGenericType && (@base.GetGenericTypeDefinition().IsEquivalentTo(typeof(IDictionary<,>)) || @base.GetGenericTypeDefinition().IsEquivalentTo(typeof(Dictionary<,>))))
                            {
                                var args = @base.GetGenericArguments();
                                if (args.Length > 1)
                                {
                                    hmlKeyTag = $" {CreateNamespacedTag("key")}=\"{args[0].Name}\"";
                                    hmlValueTag = $" {CreateNamespacedTag("value")}=\"{args[1].Name}\"";
                                    break;
                                }
                            }

                            @base = @base.BaseType;
                        }

                        var tag = $"{indents}<{CreateNamespacedTag("map")} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag}{hmlKeyTag}{hmlValueTag}";

                        if (!(instance is IDictionary dictionary)) return null;

                        if (dictionary.Count == 0)
                        {
                            tag += " />\n";
                            return tag;
                        }

                        tag += ">\n";

                        var innerInnerIndent = innerIndent + 1;

                        var values = dictionary.Values.Cast<object>().ToArray();
                        var keys = dictionary.Keys.Cast<object>().ToArray();

                        for (var i = 0; i < values.Length; ++i)
                        {
                            var value = values.GetValue(i);
                            var key = keys.GetValue(i);

                            var valueType = value?.GetType();
                            var keyType = key?.GetType();

                            IHMLSerializer? customValueSerializer = null;
                            IHMLSerializer? customKeySerializer = null;

                            var valueTarget = GetCustomSerializer(customTypeSerializers, valueType, ref customValueSerializer);
                            var keyTarget = GetCustomSerializer(customTypeSerializers, keyType, ref customKeySerializer);

                            if (valueTarget == HMLSerializationTarget.Null)
                                tag += $"{innerIndent}<{CreateNamespacedTag("null")}";
                            else
                                // ReSharper disable once PossibleNullReferenceException
                                tag += $"{innerIndent}<{FormatName(valueType?.Name)}";

                            if (keyTarget == HMLSerializationTarget.Null)
                                tag += " />";
                            else if (keyTarget < HMLSerializationTarget.Complex) tag += $" {CreateNamespacedTag("key")}=\"{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(key, customTypeSerializers, visited, innerIndent, valueName))}\"";

                            if (valueTarget != HMLSerializationTarget.Null && valueTarget < HMLSerializationTarget.Complex) tag += $" {CreateNamespacedTag("value")}=\"{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(value, customTypeSerializers, visited, innerIndent, valueName))}\"";

                            if (valueTarget < HMLSerializationTarget.Complex && keyTarget < HMLSerializationTarget.Complex)
                            {
                                tag += " />\n";
                            }
                            else
                            {
                                tag += ">\n";
                                if (keyTarget >= HMLSerializationTarget.Complex) tag += Print(key, customTypeSerializers, visited, innerInnerIndent, CreateNamespacedTag("key"));
                                if (valueTarget >= HMLSerializationTarget.Complex) tag += Print(value, customTypeSerializers, visited, innerInnerIndent, CreateNamespacedTag("value"));
                                if (valueTarget == HMLSerializationTarget.Null)
                                    tag += $"{innerIndent}</{CreateNamespacedTag("null")}>\n";
                                else
                                    // ReSharper disable once PossibleNullReferenceException
                                    tag += $"{innerIndent}</{FormatName(valueType?.Name)}>\n";
                            }
                        }

                        tag += $"{indents}</{CreateNamespacedTag("map")}>\n";
                        return tag;
                    }
                    else
                    {
                        return $"{indents}<{CreateNamespacedTag("ref")} {CreateNamespacedTag("id")}=\"{instance?.GetHashCode()}\"{hmlNameTag} />\n";
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static HMLSerializationTarget GetCustomSerializer(IReadOnlyDictionary<Type, IHMLSerializer> customTypeSerializers, Type? type, ref IHMLSerializer? customSerializer)
        {
            HMLSerializationTarget target;
            if (type != null && customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type)))
            {
                customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type)).Value;
                target = customSerializer.OverrideTarget;
            }
            else if (type != null && type.IsConstructedGenericType && customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition())))
            {
                customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition())).Value;
                target = customSerializer.OverrideTarget;
            }
            else
            {
                target = GetSerializationTarget(type);
            }

            return target;
        }

        private static string? FormatTextValueType(object? instance) => instance == null ? "{null}" : instance?.ToString()?.Replace("\\", "\\\\")?.Replace("\r", "\\r")?.Replace("\n", "\\n")?.Replace("<", "\\<")?.Replace(">", "\\>");


        private static string? FormatValueType(object? instance) => instance == null ? "{null}" : instance?.ToString()?.Replace("\\", "\\\\")?.Replace("\r", "\\r")?.Replace("\n", "\\n")?.Replace("\"", "\\\"");

        private static string? FormatName(string? typeName) => typeName?.Replace('<', '_')?.Replace('>', '_')?.Replace('`', '_');

        private static object? GetMemberValue(object? instance, MemberInfo member) =>
            member switch
            {
                FieldInfo field => field.GetValue(instance),
                PropertyInfo property => property.GetValue(instance),
                _ => null
            };

        private static IEnumerable<MemberInfo> GetMembers(Type? type)
        {
            if (type == null) return ArraySegment<MemberInfo>.Empty;

            // ReSharper disable once InvertIf
            if (!TypeCache.TryGetValue(type, out var members))
            {
                members = type.GetFields().Cast<MemberInfo>().Concat(type.GetProperties()).Where(x => x.GetCustomAttribute<IgnoreDataMemberAttribute>() == null).ToArray();
                TypeCache.Add(type, members);
            }

            return members;
        }

        public static HMLSerializationTarget GetSerializationTarget(Type? type)
        {
            if (type == null) return HMLSerializationTarget.Null;

            // ReSharper disable once InvertIf
            if (!TargetCache.TryGetValue(type, out var target))
            {
                if (type.IsArray || typeof(Array).IsAssignableFrom(type))
                    target = HMLSerializationTarget.Array;
                else if (type.IsEnum || type.IsPrimitive || type == typeof(string))
                    target = HMLSerializationTarget.Value;
                else if (typeof(IDictionary).IsAssignableFrom(type))
                    target = HMLSerializationTarget.Dictionary;
                else if (typeof(IEnumerable).IsAssignableFrom(type))
                    target = HMLSerializationTarget.Enumerable;
                else
                    target = HMLSerializationTarget.Object;

                TargetCache.Add(type, target);
            }

            return target;
        }

        public static void ClearCache()
        {
            TypeCache.Clear();
            TargetCache.Clear();
        }
    }
}
