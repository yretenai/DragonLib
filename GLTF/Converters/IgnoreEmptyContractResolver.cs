using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DragonLib.GLTF.Converters
{
    public class IgnoreEmptyContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(string)) return property;
            if (property.PropertyType == null) return property;
            if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null && property.ShouldSerialize == null)
                property.ShouldSerialize = instance =>
                {
                    if (property.UnderlyingName == null) return true;
                    var value = instance?.GetType().GetProperty(property.UnderlyingName)?.GetValue(instance);
                    switch (value)
                    {
                        case null:
                            return true;
                        case ICollection collection:
                            return collection.Count > 0;
                        default:
                        {
                            var valueType = value.GetType();
                            try
                            {
                                var countObject = valueType.InvokeMember("Count", BindingFlags.GetProperty | BindingFlags.GetField, null, value, null);
                                if (countObject != null) return Convert.ToInt64(countObject) != 0;
                            }
                            catch
                            {
                                // ignored
                            }

                            try
                            {
                                var lengthObject = valueType.InvokeMember("Length", BindingFlags.GetProperty | BindingFlags.GetField, null, value, null);
                                if (lengthObject != null) return Convert.ToInt64(lengthObject) != 0;
                            }
                            catch
                            {
                                // ignored
                            }

                            return true;
                        }
                    }
                };
            return property;
        }
    }
}
