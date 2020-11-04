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

        public static string CreateNamespacedTag(string? tag, string? ns)
        {
            if (tag == null) return "";
            return ns == null ? tag : $"{ns}:{tag}";
        }

        public static string? Print(object? instance, HealingMLSettings? settings = null) => Print(instance, new Dictionary<object, int>(), new SpaceIndentHelper(), null, settings ?? HealingMLSettings.Default);

        public static string? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, HealingMLSettings settings)
        {
            var type = instance?.GetType();
            IHMLSerializer? customSerializer = null;
            var target = GetCustomSerializer(settings.TypeSerializers, type, ref customSerializer);

            var hmlNameTag = string.Empty;
            if (!string.IsNullOrWhiteSpace(valueName)) hmlNameTag = $" {CreateNamespacedTag("name", settings.Namespace)}=\"{valueName}\"";

            var innerIndent = indents + 1;

            switch (target)
            {
                case HMLSerializationTarget.Null:
                    return $"{indents}<{CreateNamespacedTag("null", settings.Namespace)}{hmlNameTag} />\n";
                case HMLSerializationTarget.Object when type != null && customSerializer != null:
                case HMLSerializationTarget.Array when type != null && customSerializer != null:
                    return customSerializer.Print(instance, visited, indents, valueName, settings) as string;
                case HMLSerializationTarget.Value when type != null:
                    return $"{indents}<{FormatName(type.Name)}>{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(instance, visited, innerIndent, valueName, settings))}</{FormatName(type.Name)}>\n";
                case HMLSerializationTarget.Array when type != null:
                case HMLSerializationTarget.Enumerable when type != null:
                    if (!visited.ContainsKey(instance!))
                    {
                        visited[instance!] = visited.Count;
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{CreateNamespacedTag("array", settings.Namespace)}{hmlIdTag}{hmlNameTag}>\n";
                        if (target == HMLSerializationTarget.Enumerable && instance is IEnumerable enumerable) instance = enumerable.Cast<object>().ToArray();
                        if (!(instance is Array array))
                            tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)} />\n";
                        else
                            for (long i = 0; i < array.LongLength; ++i)
                                tag += Print(array.GetValue(i), visited, innerIndent, null, settings);

                        tag += $"{indents}</{CreateNamespacedTag("array", settings.Namespace)}>\n";
                        return tag;
                    }
                    else
                    {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                    }
                case HMLSerializationTarget.Object when type != null:
                    if(!visited.ContainsKey(instance!))
                    {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{FormatName(type.Name)}{hmlIdTag}{hmlNameTag}";
                        var members = GetMembers(type);
                        var complexMembers = new List<(object? value, string memberName, IHMLSerializer? custom)>();
                        foreach (var member in members)
                        {
                            var value = GetMemberValue(instance, member);
                            var valueType = value?.GetType();
                            IHMLSerializer? targetCustomSerializer = null;
                            var targetMemberTarget = GetCustomSerializer(settings.TypeSerializers, valueType, ref targetCustomSerializer);

                            if (targetMemberTarget >= HMLSerializationTarget.Complex)
                                complexMembers.Add((value, member.Name, targetCustomSerializer));
                            else
                                tag += $" {member.Name}=\"{(targetCustomSerializer != null ? targetCustomSerializer.Print(value, visited, indents, member.Name, settings) : FormatValueType(value))}\"";
                        }

                        if (complexMembers.Count == 0)
                        {
                            tag += " />\n";
                        }
                        else
                        {
                            tag += ">\n";
                            foreach (var (value, name, custom) in complexMembers) tag += custom != null ? custom.Print(value, visited, innerIndent, name, settings) : Print(value, visited, innerIndent, name, settings);

                            tag += $"{indents}</{FormatName(type.Name)}>\n";
                        }

                        return tag;
                    }
                    else
                    {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                    }
                case HMLSerializationTarget.Dictionary when type != null:
                    if (!visited.ContainsKey(instance!))
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
                                    hmlKeyTag = $" {CreateNamespacedTag("key", settings.Namespace)}=\"{args[0].Name}\"";
                                    hmlValueTag = $" {CreateNamespacedTag("value", settings.Namespace)}=\"{args[1].Name}\"";
                                    break;
                                }
                            }

                            @base = @base.BaseType;
                        }

                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{CreateNamespacedTag("map", settings.Namespace)}{hmlIdTag}{hmlNameTag}{hmlKeyTag}{hmlValueTag}";

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

                            var valueTarget = GetCustomSerializer(settings.TypeSerializers, valueType, ref customValueSerializer);
                            var keyTarget = GetCustomSerializer(settings.TypeSerializers, keyType, ref customKeySerializer);

                            if (valueTarget == HMLSerializationTarget.Null)
                                tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)}";
                            else
                                // ReSharper disable once PossibleNullReferenceException
                                tag += $"{innerIndent}<{FormatName(valueType?.Name)}";

                            if (keyTarget == HMLSerializationTarget.Null)
                                tag += " />";
                            else if (keyTarget < HMLSerializationTarget.Complex) tag += $" {CreateNamespacedTag("key", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(key, visited, innerIndent, valueName, settings))}\"";

                            if (valueTarget != HMLSerializationTarget.Null && valueTarget < HMLSerializationTarget.Complex) tag += $" {CreateNamespacedTag("value", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? HMLToStringSerializer.Default).Print(value, visited, innerIndent, valueName, settings))}\"";

                            if (valueTarget < HMLSerializationTarget.Complex && keyTarget < HMLSerializationTarget.Complex)
                            {
                                tag += " />\n";
                            }
                            else
                            {
                                tag += ">\n";
                                if (keyTarget >= HMLSerializationTarget.Complex) tag += Print(key, visited, innerInnerIndent, CreateNamespacedTag("key", settings.Namespace), settings);
                                if (valueTarget >= HMLSerializationTarget.Complex) tag += Print(value, visited, innerInnerIndent, CreateNamespacedTag("value", settings.Namespace), settings);
                                if (valueTarget == HMLSerializationTarget.Null)
                                    tag += $"{innerIndent}</{CreateNamespacedTag("null", settings.Namespace)}>\n";
                                else
                                    // ReSharper disable once PossibleNullReferenceException
                                    tag += $"{innerIndent}</{FormatName(valueType?.Name)}>\n";
                            }
                        }

                        tag += $"{indents}</{CreateNamespacedTag("map", settings.Namespace)}>\n";
                        return tag;
                    }
                    else
                    {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
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

        private static string? FormatTextValueType(object? instance) => instance == null ? "{null}" : instance.ToString()?.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("<", "\\<").Replace(">", "\\>");


        private static string? FormatValueType(object? instance) => instance == null ? "{null}" : instance.ToString()?.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");

        private static string? FormatName(string? typeName) => typeName?.Replace('<', '_').Replace('>', '_').Replace('`', '_');

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
