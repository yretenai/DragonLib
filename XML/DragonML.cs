using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DragonLib.Indent;

namespace DragonLib.XML {
    public static class DragonML {
        private static readonly Dictionary<Type, MemberInfo[]> TypeCache = new();
        private static readonly Dictionary<Type, DragonMLType> TargetCache = new();

        public static string CreateNamespacedTag(string? tag, string? ns) {
            if (tag == null) return "";

            return ns == null ? tag : $"{ns}:{tag}";
        }

        public static string? Print(object? instance, DragonMLSettings? settings = null) {
            return Print(instance, new Dictionary<object, int>(), new SpaceIndentHelper(), null, settings ?? DragonMLSettings.Default, true);
        }

        public static string? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, DragonMLSettings settings, bool root = false) {
            if (root && settings.WriteXmlHeader) return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + Print(instance, visited, indents, valueName, settings);

            var type = instance?.GetType();
            IDragonMLSerializer? customSerializer = null;
            var target = GetCustomSerializer(settings.TypeSerializers, type, ref customSerializer);

            var hmlNameTag = string.Empty;
            if (!string.IsNullOrWhiteSpace(valueName)) hmlNameTag = $" {CreateNamespacedTag("name", settings.Namespace)}=\"{valueName}\"";

            var innerIndent = indents + 1;

            switch (target) {
                case DragonMLType.Null:
                    return $"{indents}<{CreateNamespacedTag("null", settings.Namespace)}{hmlNameTag} />\n";
                case DragonMLType.Object when type != null && customSerializer != null:
                case DragonMLType.Array when type != null && customSerializer != null:
                    return customSerializer.Print(instance, visited, indents, valueName, settings) as string;
                case DragonMLType.Value when type != null:
                    return $"{indents}<{FormatName(type.Name)}>{FormatTextValueType((customSerializer ?? DragonMLToStringSerializer.Default).Print(instance, visited, innerIndent, valueName, settings))}</{FormatName(type.Name)}>\n";
                case DragonMLType.Array when type != null:
                case DragonMLType.Enumerable when type != null:
                    if (!visited.ContainsKey(instance!)) {
                        visited[instance!] = visited.Count;
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{CreateNamespacedTag("array", settings.Namespace)}{hmlIdTag}{hmlNameTag}>\n";
                        if (target == DragonMLType.Enumerable && instance is IEnumerable enumerable) instance = enumerable.Cast<object>().ToArray();

                        if (instance is not Array array)
                            tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)} />\n";
                        else
                            for (long i = 0; i < array.LongLength; ++i)
                                tag += Print(array.GetValue(i), visited, innerIndent, null, settings);

                        tag += $"{indents}</{CreateNamespacedTag("array", settings.Namespace)}>\n";
                        return tag;
                    } else {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                    }
                case DragonMLType.Object when type != null:
                    if (!visited.ContainsKey(instance!)) {
                        visited[instance!] = visited.Count;
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{FormatName(type.Name)}{hmlIdTag}{hmlNameTag}";
                        var members = GetMembers(type);
                        var complexMembers = new List<(object? value, string memberName, IDragonMLSerializer? custom)>();
                        foreach (var member in members) {
                            var value = GetMemberValue(instance, member);
                            var valueType = value?.GetType();
                            IDragonMLSerializer? targetCustomSerializer = null;
                            var targetMemberTarget = GetCustomSerializer(settings.TypeSerializers, valueType, ref targetCustomSerializer);

                            if (targetMemberTarget >= DragonMLType.Complex)
                                complexMembers.Add((value, member.Name, targetCustomSerializer));
                            else
                                tag += $" {member.Name}=\"{(targetCustomSerializer != null ? targetCustomSerializer.Print(value, visited, indents, member.Name, settings) : FormatValueType(value))}\"";
                        }

                        if (visited.Count == 1) {
                            foreach (var (ns, nsUri) in settings.Namespaces) tag += $" xmlns:{ns}=\"{nsUri}\"";

                            if (!settings.Namespaces.ContainsKey(settings.Namespace)) tag += $" xmlns:{settings.Namespace}=\"https://yretenai.com/dragonml/v1\"";
                        }

                        if (complexMembers.Count == 0) {
                            tag += " />\n";
                        } else {
                            tag += ">\n";
                            foreach (var (value, name, custom) in complexMembers) tag += custom != null ? custom.Print(value, visited, innerIndent, name, settings) : Print(value, visited, innerIndent, name, settings);

                            tag += $"{indents}</{FormatName(type.Name)}>\n";
                        }

                        return tag;
                    } else {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                    }
                case DragonMLType.Dictionary when type != null:
                    if (!visited.ContainsKey(instance!)) {
                        visited[instance!] = visited.Count;

                        var hmlKeyTag = string.Empty;
                        var hmlValueTag = string.Empty;

                        var @base = type;
                        while (@base != null) {
                            if (@base.IsConstructedGenericType && (@base.GetGenericTypeDefinition().IsEquivalentTo(typeof(IDictionary<,>)) || @base.GetGenericTypeDefinition().IsEquivalentTo(typeof(Dictionary<,>)))) {
                                var args = @base.GetGenericArguments();
                                if (args.Length > 1) {
                                    hmlKeyTag = $" {CreateNamespacedTag("key", settings.Namespace)}=\"{args[0].Name}\"";
                                    hmlValueTag = $" {CreateNamespacedTag("value", settings.Namespace)}=\"{args[1].Name}\"";
                                    break;
                                }
                            }

                            @base = @base.BaseType;
                        }

                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        var tag = $"{indents}<{CreateNamespacedTag("map", settings.Namespace)}{hmlIdTag}{hmlNameTag}{hmlKeyTag}{hmlValueTag}";

                        if (instance is not IDictionary dictionary) return null;

                        if (dictionary.Count == 0) {
                            tag += " />\n";
                            return tag;
                        }

                        tag += ">\n";

                        var innerInnerIndent = innerIndent + 1;

                        var values = dictionary.Values.Cast<object>().ToArray();
                        var keys = dictionary.Keys.Cast<object>().ToArray();

                        for (var i = 0; i < values.Length; ++i) {
                            var value = values.GetValue(i);
                            var key = keys.GetValue(i);

                            var valueType = value?.GetType();
                            var keyType = key?.GetType();

                            IDragonMLSerializer? customValueSerializer = null;
                            IDragonMLSerializer? customKeySerializer = null;

                            var valueTarget = GetCustomSerializer(settings.TypeSerializers, valueType, ref customValueSerializer);
                            var keyTarget = GetCustomSerializer(settings.TypeSerializers, keyType, ref customKeySerializer);

                            if (valueTarget == DragonMLType.Null)
                                tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)}";
                            else
                                // ReSharper disable once PossibleNullReferenceException
                                tag += $"{innerIndent}<{FormatName(valueType?.Name)}";

                            switch (keyTarget) {
                                case DragonMLType.Null:
                                    tag += " />";
                                    break;
                                case < DragonMLType.Complex:
                                    tag += $" {CreateNamespacedTag("key", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? DragonMLToStringSerializer.Default).Print(key, visited, innerIndent, valueName, settings))}\"";
                                    break;
                            }

                            if (valueTarget != DragonMLType.Null && valueTarget < DragonMLType.Complex) tag += $" {CreateNamespacedTag("value", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? DragonMLToStringSerializer.Default).Print(value, visited, innerIndent, valueName, settings))}\"";

                            if (valueTarget < DragonMLType.Complex && keyTarget < DragonMLType.Complex) {
                                tag += " />\n";
                            } else {
                                tag += ">\n";
                                if (keyTarget >= DragonMLType.Complex) tag += Print(key, visited, innerInnerIndent, CreateNamespacedTag("key", settings.Namespace), settings);

                                if (valueTarget >= DragonMLType.Complex) tag += Print(value, visited, innerInnerIndent, CreateNamespacedTag("value", settings.Namespace), settings);

                                if (valueTarget == DragonMLType.Null)
                                    tag += $"{innerIndent}</{CreateNamespacedTag("null", settings.Namespace)}>\n";
                                else
                                    // ReSharper disable once PossibleNullReferenceException
                                    tag += $"{innerIndent}</{FormatName(valueType?.Name)}>\n";
                            }
                        }

                        tag += $"{indents}</{CreateNamespacedTag("map", settings.Namespace)}>\n";
                        return tag;
                    } else {
                        var hmlIdTag = settings.UseRefId ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\"" : "";
                        return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DragonMLType GetCustomSerializer(IReadOnlyDictionary<Type, IDragonMLSerializer> customTypeSerializers, Type? type, ref IDragonMLSerializer? customSerializer) {
            DragonMLType target;
            if (type != null && customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type))) {
                customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type)).Value;
                target = customSerializer.OverrideTarget;
            } else if (type is { IsConstructedGenericType: true } && customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition()))) {
                customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition())).Value;
                target = customSerializer.OverrideTarget;
            } else {
                target = GetSerializationType(type);
            }

            return target;
        }

        private static string? FormatTextValueType(object? instance) {
            return instance == null ? "{null}" : instance.ToString()?.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("<", "\\<").Replace(">", "\\>");
        }

        private static string? FormatValueType(object? instance) {
            return instance == null ? "{null}" : instance.ToString()?.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        private static string? FormatName(string? typeName) {
            return typeName?.Replace('<', '_').Replace('>', '_').Replace('`', '_');
        }

        private static object? GetMemberValue(object? instance, MemberInfo member) {
            return member switch {
                FieldInfo field => field.GetValue(instance),
                PropertyInfo property => property.GetValue(instance),
                _ => null
            };
        }

        private static IEnumerable<MemberInfo> GetMembers(Type? type) {
            if (type == null) return ArraySegment<MemberInfo>.Empty;

            // ReSharper disable once InvertIf
            if (!TypeCache.TryGetValue(type, out var members)) {
                members = type.GetFields().Cast<MemberInfo>().Concat(type.GetProperties()).Where(x => x.GetCustomAttribute<IgnoreDataMemberAttribute>() == null).ToArray();
                TypeCache.Add(type, members);
            }

            return members;
        }

        public static DragonMLType GetSerializationType(Type? type) {
            if (type == null) return DragonMLType.Null;

            // ReSharper disable once InvertIf
            if (!TargetCache.TryGetValue(type, out var target)) {
                if (type.IsArray || typeof(Array).IsAssignableFrom(type))
                    target = DragonMLType.Array;
                else if (type.IsEnum || type.IsPrimitive || type == typeof(string))
                    target = DragonMLType.Value;
                else if (typeof(IDictionary).IsAssignableFrom(type))
                    target = DragonMLType.Dictionary;
                else if (typeof(IEnumerable).IsAssignableFrom(type))
                    target = DragonMLType.Enumerable;
                else
                    target = DragonMLType.Object;

                TargetCache.Add(type, target);
            }

            return target;
        }

        public static void ClearCache() {
            TypeCache.Clear();
            TargetCache.Clear();
        }
    }
}
