using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Bencodex.Types;

namespace Bencodex.Declarative
{
    public class BencodexSerializer<T>
        where T : new()
    {
        public static Dictionary Serialize(T obj)
        {
            var members = GetFields().Cast<MemberInfo>().Union(GetProperties()).ToList();
            var names = members
                .Select(field =>
                    field.GetCustomAttribute(typeof(BencodexPropertyAttribute))
                        as BencodexPropertyAttribute)
                .Where(attr => attr != null)
                .Select(attr => attr.Name);
            var dictionary = default(Dictionary);
            foreach (var (member, fieldName) in members.Zip(names, ValueTuple.Create))
            {
                object value;
                if (member is FieldInfo field)
                {
                    value = field.GetValue(obj);
                }
                else
                {
                    value = (member as PropertyInfo).GetValue(obj);
                }

                var converted = ToBencodex(value);
                dictionary = dictionary.SetItem(fieldName, converted);
            }

            return dictionary;
        }

        public static T Deserialize(Dictionary dictionary)
        {
            var members = GetFields().Cast<MemberInfo>().Union(GetProperties()).ToList();
            var names = members
                .Select(member =>
                    member.GetCustomAttribute(typeof(BencodexPropertyAttribute))
                        as BencodexPropertyAttribute)
                .Where(attr => attr != null)
                .Select(attr => attr.Name);
            var obj = new T();
            foreach (var (member, fieldName) in members.Zip(names, ValueTuple.Create))
            {
                var value = dictionary[fieldName];
                if (member is FieldInfo field)
                {
                    var converted = FromBencodex(value, field.FieldType);
                    field.SetValueDirect(__makeref(obj), converted);
                }
                else
                {
                    var property = (PropertyInfo)member;
                    var converted = FromBencodex(value, property.PropertyType);
                    property.SetValue(obj, converted);
                }
            }

            return obj;
        }

        private static IValue ToBencodex(object obj)
        {
            if (obj is IValue value)
            {
                return value;
            }

            if (obj.GetType().IsDefined(typeof(BencodexObjectAttribute)))
            {
                var deserializeMethod = typeof(BencodexSerializer<>)
                    .MakeGenericType(obj.GetType()).GetMethod(nameof(Serialize));
                return (IValue)deserializeMethod.Invoke(null, new object[] { obj });
            }

            switch (obj)
            {
                case short s:
                    return (Integer)s;
                case int i:
                    return (Integer)i;
                case long l:
                    return (Integer)l;
                case ushort us:
                    return (Integer)us;
                case uint ui:
                    return (Integer)ui;
                case ulong ul:
                    return (Integer)ul;
                case string s:
                    return (Text)s;
                case byte[] bytes:
                    return (Binary)bytes;
                case bool b:
                    return new Bencodex.Types.Boolean(b);
                case IList list:
                    var values = new List<IValue>();
                    foreach (var v in list)
                    {
                        values.Add(ToBencodex(v));
                    }

                    return new Bencodex.Types.List(values);
                case IDictionary dictionary:
                    var entries = new List<KeyValuePair<IKey, IValue>>();
                    foreach (var k in dictionary.Keys)
                    {
                        var v = dictionary[k];
                        entries.Add(
                            new KeyValuePair<IKey, IValue>(
                                (IKey)ToBencodex(k),
                                ToBencodex(v)));
                    }

                    return new Bencodex.Types.Dictionary(entries);
                default:
                    throw new BencodexSerializationException(
                        string.Format(
                            "{0} type isn't supported in {1}.",
                            obj.GetType().FullName,
                            nameof(ToBencodex)));
            }
        }

        private static object FromBencodex(IValue value, Type to)
        {
            if (typeof(IValue).IsAssignableFrom(to))
            {
                return value;
            }

            switch (value)
            {
                case Text text:
                    return (string)text;

                case Binary binary:
                    return (byte[])binary;

                case Bencodex.Types.Boolean boolean:
                    return (bool)boolean;

                case Integer integer:
                    if (to == typeof(short))
                    {
                        return (short)integer;
                    }
                    else if (to == typeof(int))
                    {
                        return (int)integer;
                    }
                    else if (to == typeof(long))
                    {
                        return (long)integer;
                    }
                    else if (to == typeof(ushort))
                    {
                        return (ushort)integer;
                    }
                    else if (to == typeof(uint))
                    {
                        return (uint)integer;
                    }
                    else if (to == typeof(ulong))
                    {
                        return (ulong)integer;
                    }
                    else
                    {
                        throw new BencodexSerializationException(
                            $"Can't convert {nameof(Bencodex.Types.Integer)} to {to.FullName}."
                            + "It isn't supported yet or it seems to be tried to reverse alignment to a different type than when serializing.'");
                    }

                case Bencodex.Types.Dictionary dictionary:
                    if (to.IsDefined(typeof(BencodexObjectAttribute)))
                    {
                        return typeof(BencodexSerializer<>)
                            .MakeGenericType(to)
                            .GetMethod(nameof(Deserialize))
                            .Invoke(null, new object[] { value });
                    }
                    else
                    {
                        return GetDictionaryFromBencodex(dictionary, to);
                    }

                case Bencodex.Types.List list:
                    if (to.IsGenericType &&
                        typeof(IList).IsAssignableFrom(to))
                    {
                        return GetListFromBencodex(list, to);
                    }
                    else
                    {
                        throw new BencodexSerializationException(
                            $"Can't convert {nameof(Bencodex.Types.List)} to {to.FullName}."
                            + "It's not supported yet or it seems to be tried to reverse alignment to a different type than when serializing.'");
                    }

                default:
                    throw new BencodexSerializationException("Not Supported");
            }
        }

        private static object GetListFromBencodex(
            Bencodex.Types.List list,
            Type to)
        {
            var genericType = to.GetGenericArguments()[0];
            var listTypeWithGeneric =
                typeof(List<>).MakeGenericType(genericType);
            IList l = (IList)Activator.CreateInstance(listTypeWithGeneric);
            foreach (var v in list)
            {
                l.Add(FromBencodex(v, genericType));
            }

            if (to.GetGenericTypeDefinition() == typeof(ImmutableList<>))
            {
                return typeof(ImmutableList)
                    .GetMethod("CreateRange")
                    .MakeGenericMethod(genericType)
                    .Invoke(null, new object[] { l });
            }

            return l;
        }

        private static object GetDictionaryFromBencodex(
            Bencodex.Types.Dictionary dictionary, Type to)
        {
            var genericTypeArguments =
                to.GenericTypeArguments;
            var dictionaryTypeWithGeneric =
                to.GetGenericTypeDefinition().MakeGenericType(
                    genericTypeArguments[0],
                    genericTypeArguments[1]);
            var keyValuePairTypeWithGeneric =
                typeof(KeyValuePair<,>).MakeGenericType(
                    genericTypeArguments[0],
                    genericTypeArguments[1]);
            var keyValuePairListType =
                typeof(List<>).MakeGenericType(keyValuePairTypeWithGeneric);
            var pairs = (IList)Activator.CreateInstance(
                keyValuePairListType);

            foreach (var entry in dictionary)
            {
                var pair = Activator.CreateInstance(
                    keyValuePairTypeWithGeneric,
                    FromBencodex(
                        entry.Key,
                        genericTypeArguments[0]),
                    FromBencodex(
                        entry.Value,
                        genericTypeArguments[1])
                );
                pairs.Add(pair);
            }

            if (typeof(ImmutableDictionary<,>) ==
                to.GetGenericTypeDefinition())
            {
                return typeof(ImmutableDictionary)
                    .GetMethods()
                    .Single(m =>
                        m.GetParameters().Length == 1 &&
                        m.Name == "CreateRange")
                    .MakeGenericMethod(
                        genericTypeArguments[0],
                        genericTypeArguments[1])
                    .Invoke(null, new object[] { pairs });
            }

            return
                Activator.CreateInstance(
                    dictionaryTypeWithGeneric,
                    pairs);
        }

        private static IEnumerable<FieldInfo> GetFields()
        {
            CheckMarkedType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic;
            return typeof(T)
                .GetFields(bindingFlags)
                .Where(field =>
                    field.IsDefined(
                        typeof(BencodexPropertyAttribute),
                        false));
        }

        private static IEnumerable<PropertyInfo> GetProperties()
        {
            CheckMarkedType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic;
            return typeof(T)
                .GetProperties(bindingFlags)
                .Where(property =>
                    property.IsDefined(
                        typeof(BencodexPropertyAttribute),
                        false));
        }

        private static void CheckMarkedType()
        {
            if (!typeof(T).IsDefined(typeof(BencodexObjectAttribute), false))
            {
                throw new BencodexSerializationException(
                    $"The type is not marked with {nameof(BencodexObjectAttribute)}.");
            }
        }
    }
}
