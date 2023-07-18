using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DragonLib.Indent;

namespace DragonLib.Xml;

public static class DragonMarkup {
    private static readonly Dictionary<Type, MemberInfo[]> TypeCache = new();
    private static readonly Dictionary<Type, DragonMarkupType> TargetCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string CreateNamespacedTag(string? tag, string? ns) {
        if (tag == null) {
            return string.Empty;
        }

        return ns == null ? tag : $"{ns}:{tag}";
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static string? Print(object? instance, DragonMarkupSettings? settings = null) =>
        Print(instance,
            new Dictionary<object, int>(),
            new SpaceIndentHelper(),
            null,
            settings ?? DragonMarkupSettings.Default,
            true);

    public static T[] UnwrapMemory<T>(Memory<T> memory) {
        return memory.ToArray();
    }

    public static T[] UnwrapReadOnlyMemory<T>(Memory<T> memory) {
        return memory.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static string? Print(object? instance,
        Dictionary<object, int> visited,
        IndentHelperBase indents,
        string? valueName,
        DragonMarkupSettings settings,
        bool root = false) {
        if (root && settings.WriteXmlHeader) {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + Print(instance, visited, indents, valueName, settings);
        }

        var type = instance?.GetType();
        IDragonMarkupSerializer? customSerializer = null;
        var target = GetCustomSerializer(settings, type, ref customSerializer);

        var hmlNameTag = string.Empty;
        if (!string.IsNullOrWhiteSpace(valueName)) {
            hmlNameTag = $" {CreateNamespacedTag("name", settings.Namespace)}=\"{valueName}\"";
        }

        var innerIndent = indents + 1;

        switch (target) {
            case DragonMarkupType.Null:
                return $"{indents}<{CreateNamespacedTag("null", settings.Namespace)}{hmlNameTag} />\n";
            case DragonMarkupType.Object when type != null && customSerializer != null:
            case DragonMarkupType.Array when type != null && customSerializer != null:
            case DragonMarkupType.Memory when type != null && customSerializer != null:
                return customSerializer.Print(instance, visited, indents, valueName, settings) as string;
            case DragonMarkupType.Value when type != null:
                return
                    $"{indents}<{FormatName(type, settings.Namespace)}>{FormatTextValueType((customSerializer ?? DragonMarkupToStringSerializer.Default).Print(instance, visited, innerIndent, valueName, settings))}</{FormatName(type, settings.Namespace)}>\n";
            case DragonMarkupType.Array when type != null:
            case DragonMarkupType.Memory when type != null:
            case DragonMarkupType.Enumerable when type != null:
                if (!visited.ContainsKey(instance!)) {
                    visited[instance!] = visited.Count;
                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;
                    var tag = $"{indents}<{CreateNamespacedTag("array", settings.Namespace)}{hmlIdTag}{hmlNameTag}>\n";
                    if (target == DragonMarkupType.Enumerable && instance is IEnumerable enumerable) {
                        instance = enumerable.Cast<object>().ToArray();
                    } else if (target == DragonMarkupType.Memory) {
                        var target1 = instance!.GetType().GetGenericArguments()[0];
                        if (instance.GetType().GetGenericTypeDefinition() == typeof(Memory<>)) {
                            instance = typeof(DragonMarkup).GetMethod("UnwrapMemory")!.MakeGenericMethod(target1).Invoke(null, new[] { instance });
                        } else {
                            instance = typeof(DragonMarkup).GetMethod("UnwrapReadOnlyMemory")!.MakeGenericMethod(target1).Invoke(null, new[] { instance });
                        }
                    }

                    if (instance is Array array) {
                        if (array.Length == 0) {
                            tag = $"{indents}<{CreateNamespacedTag("array", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                            return tag;
                        }

                        if (instance is byte[] buffer) {
                            tag += Print(Convert.ToBase64String(buffer), visited, innerIndent, null, settings);
                        } else {
                            for (long i = 0; i < array.LongLength; ++i) {
                                tag += Print(array.GetValue(i), visited, innerIndent, null, settings);
                            }
                        }
                    } else {
                        tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)} />\n";
                    }

                    tag += $"{indents}</{CreateNamespacedTag("array", settings.Namespace)}>\n";
                    return tag;
                } else {
                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;
                    return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                }
            case DragonMarkupType.Object when type != null:
                if (!visited.ContainsKey(instance!)) {
                    visited[instance!] = visited.Count;
                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;

                    var nsTag = "";
                    if (visited.Count == 1) {
                        foreach (var (ns, nsUri) in settings.Namespaces) {
                            nsTag += $" xmlns:{ns}=\"{nsUri}\"";
                        }

                        if (!settings.Namespaces.ContainsKey(settings.Namespace)) {
                            nsTag += $" xmlns:{settings.Namespace}=\"https://legiayayana.com/dml/v1\"";
                        }
                    }

                    var tag = $"{indents}<{FormatName(type, settings.Namespace)}{nsTag}{hmlIdTag}{hmlNameTag}";
                    var members = GetMembers(type, settings);
                    var complexMembers = new List<(object? value, string memberName, IDragonMarkupSerializer? custom)>();
                    foreach (var member in members) {
                        var value = GetMemberValue(instance, member);
                        var valueType = value?.GetType();
                        IDragonMarkupSerializer? targetCustomSerializer = null;
                        var targetMemberTarget = GetCustomSerializer(settings,
                            valueType,
                            ref targetCustomSerializer);

                        if (targetMemberTarget >= DragonMarkupType.Complex) {
                            complexMembers.Add((value, member.Name, targetCustomSerializer));
                        } else {
                            tag +=
                                $" {member.Name}=\"{(targetCustomSerializer != null ? targetCustomSerializer.Print(value, visited, indents, member.Name, settings) : FormatValueType(value))}\"";
                        }
                    }

                    if (complexMembers.Count == 0) {
                        tag += " />\n";
                    } else {
                        tag += ">\n";
                        foreach (var (value, name, custom) in complexMembers) {
                            tag += custom != null
                                ? custom.Print(value, visited, innerIndent, name, settings)
                                : Print(value, visited, innerIndent, name, settings);
                        }

                        tag += $"{indents}</{FormatName(type, settings.Namespace)}>\n";
                    }

                    return tag;
                } else {
                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;
                    return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                }
            case DragonMarkupType.Dictionary when type != null:
                if (!visited.ContainsKey(instance!)) {
                    visited[instance!] = visited.Count;

                    var hmlKeyTag = string.Empty;
                    var hmlValueTag = string.Empty;

                    var @base = type;
                    while (@base != null) {
                        if (@base.IsConstructedGenericType &&
                            (@base.GetGenericTypeDefinition().IsEquivalentTo(typeof(IDictionary<,>)) ||
                             @base.GetGenericTypeDefinition().IsEquivalentTo(typeof(Dictionary<,>)))) {
                            var args = @base.GetGenericArguments();
                            if (args.Length > 1) {
                                hmlKeyTag = $" {CreateNamespacedTag("key", settings.Namespace)}=\"{args[0].Name}\"";
                                hmlValueTag = $" {CreateNamespacedTag("value", settings.Namespace)}=\"{args[1].Name}\"";
                                break;
                            }
                        }

                        @base = @base.BaseType;
                    }

                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;
                    var tag =
                        $"{indents}<{CreateNamespacedTag("map", settings.Namespace)}{hmlIdTag}{hmlNameTag}{hmlKeyTag}{hmlValueTag}";

                    if (instance is not IDictionary dictionary) {
                        return null;
                    }

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

                        IDragonMarkupSerializer? customValueSerializer = null;
                        IDragonMarkupSerializer? customKeySerializer = null;

                        var valueTarget = GetCustomSerializer(settings,
                            valueType,
                            ref customValueSerializer);
                        var keyTarget = GetCustomSerializer(settings, keyType, ref customKeySerializer);

                        if (valueTarget == DragonMarkupType.Null) {
                            tag += $"{innerIndent}<{CreateNamespacedTag("null", settings.Namespace)}";
                        } else
                            // ReSharper disable once PossibleNullReferenceException
                        {
                            tag += $"{innerIndent}<{FormatName(valueType, settings.Namespace)}";
                        }

                        switch (keyTarget) {
                            case DragonMarkupType.Null:
                                tag += " />";
                                break;
                            case < DragonMarkupType.Complex:
                                tag +=
                                    $" {CreateNamespacedTag("key", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? DragonMarkupToStringSerializer.Default).Print(key, visited, innerIndent, valueName, settings))}\"";
                                break;
                        }

                        if (valueTarget != DragonMarkupType.Null && valueTarget < DragonMarkupType.Complex) {
                            tag +=
                                $" {CreateNamespacedTag("value", settings.Namespace)}=\"{FormatTextValueType((customSerializer ?? DragonMarkupToStringSerializer.Default).Print(value, visited, innerIndent, valueName, settings))}\"";
                        }

                        if (valueTarget < DragonMarkupType.Complex && keyTarget < DragonMarkupType.Complex) {
                            tag += " />\n";
                        } else {
                            tag += ">\n";
                            if (keyTarget >= DragonMarkupType.Complex) {
                                tag += Print(key,
                                    visited,
                                    innerInnerIndent,
                                    CreateNamespacedTag("key", settings.Namespace),
                                    settings);
                            }

                            if (valueTarget >= DragonMarkupType.Complex) {
                                tag += Print(value,
                                    visited,
                                    innerInnerIndent,
                                    CreateNamespacedTag("value", settings.Namespace),
                                    settings);
                            }

                            if (valueTarget == DragonMarkupType.Null) {
                                tag += $"{innerIndent}</{CreateNamespacedTag("null", settings.Namespace)}>\n";
                            } else
                                // ReSharper disable once PossibleNullReferenceException
                            {
                                tag += $"{innerIndent}</{FormatName(valueType, settings.Namespace)}>\n";
                            }
                        }
                    }

                    tag += $"{indents}</{CreateNamespacedTag("map", settings.Namespace)}>\n";
                    return tag;
                } else {
                    var hmlIdTag = settings.UseRefId
                        ? $" {CreateNamespacedTag("id", settings.Namespace)}=\"{visited[instance!]}\""
                        : string.Empty;
                    return $"{indents}<{CreateNamespacedTag("ref", settings.Namespace)}{hmlIdTag}{hmlNameTag} />\n";
                }
            default:
                throw new IndexOutOfRangeException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static DragonMarkupType GetCustomSerializer(
        DragonMarkupSettings settings,
        Type? type,
        ref IDragonMarkupSerializer? customSerializer) {
        var customTypeSerializers = settings.TypeSerializers;
        DragonMarkupType target;
        if (type == null) {
            return DragonMarkupType.Null;
        }

        if (customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type))) {
            customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type)).Value;
            target = customSerializer.OverrideTarget;
        } else if (type is { IsConstructedGenericType: true } &&
                   customTypeSerializers.Any(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition()))) {
            customSerializer = customTypeSerializers.First(x => x.Key.IsAssignableFrom(type.GetGenericTypeDefinition()))
                .Value;
            target = customSerializer.OverrideTarget;
        } else {
            customSerializer = settings.TypeFactories.Select(factory => factory.GetSerializer(type)).FirstOrDefault(instance => instance != null);
            target = customSerializer?.OverrideTarget ?? GetSerializationType(type);
        }

        return target;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string? FormatTextValueType(object? instance) =>
        instance == null
            ? "{null}"
            : instance.ToString()
                ?.Replace("\\", "&#92;", StringComparison.Ordinal)
                .Replace("\r", "&#13;", StringComparison.Ordinal)
                .Replace("\n", "&#10;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string? FormatValueType(object? instance) =>
        instance == null
            ? "{null}"
            : instance.ToString()
                ?.Replace("\\", "&#92;", StringComparison.Ordinal)
                .Replace("\r", "&#10;", StringComparison.Ordinal)
                .Replace("\n", "&#13;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string FormatName(Type? type, string ns) {
        if (type == null) {
            return CreateNamespacedTag("null", ns);
        }

        var name = type.Name;
        if (type.IsGenericType) {
            name = name[..name.IndexOf('`', StringComparison.Ordinal)];
        }

        return name.Replace('<', '_').Replace('>', '_');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static object? GetMemberValue(object? instance, MemberInfo member) {
        return member switch {
            FieldInfo field => field.GetValue(instance),
            PropertyInfo property => property.GetValue(instance),
            _ => null,
        };
    }

    private static bool IsGenericTypePair(Type t, Type a, Type b) {
        if (!t.IsConstructedGenericType) {
            return false;
        }

        var generic = t.GetGenericTypeDefinition();
        return generic == a || generic == b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<MemberInfo> GetMembers(Type? type, DragonMarkupSettings settings) {
        if (type == null) {
            return ArraySegment<MemberInfo>.Empty;
        }

        // ReSharper disable once InvertIf
        if (!TypeCache.TryGetValue(type, out var arrayMembers)) {
            IEnumerable<MemberInfo> members = ArraySegment<MemberInfo>.Empty;
            if (settings.WriteFields) {
                members = members.Concat(
                    type.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.Instance).Where(x => !IsGenericTypePair(x.FieldType, typeof(Span<>), typeof(ReadOnlySpan<>)))
                );
            }

            if (settings.WriteProperties) {
                members = members.Concat(
                    type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance).Where(x => !IsGenericTypePair(x.PropertyType, typeof(Span<>), typeof(ReadOnlySpan<>))).Where(x => x.GetMethod!.GetParameters().Length == 0)
                );
            }

            arrayMembers = members.Where(x => x.GetCustomAttribute<IgnoreDataMemberAttribute>() == null).ToArray();
            TypeCache.Add(type, arrayMembers);
        }

        return arrayMembers;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static DragonMarkupType GetSerializationType(Type? type) {
        if (type == null) {
            return DragonMarkupType.Null;
        }

        // ReSharper disable once InvertIf
        if (!TargetCache.TryGetValue(type, out var target)) {
            if (type.IsArray || typeof(Array).IsAssignableFrom(type)) {
                target = DragonMarkupType.Array;
            } else if (IsGenericTypePair(type, typeof(Memory<>), typeof(ReadOnlyMemory<>))) {
                target = DragonMarkupType.Memory;
            } else if (type.IsEnum || type.IsPrimitive || type == typeof(string)) {
                target = DragonMarkupType.Value;
            } else if (typeof(IDictionary).IsAssignableFrom(type)) {
                target = DragonMarkupType.Dictionary;
            } else if (typeof(IEnumerable).IsAssignableFrom(type)) {
                target = DragonMarkupType.Enumerable;
            } else {
                target = DragonMarkupType.Object;
            }

            TargetCache.Add(type, target);
        }

        return target;
    }

    public static void ClearCache() {
        TypeCache.Clear();
        TargetCache.Clear();
    }
}
