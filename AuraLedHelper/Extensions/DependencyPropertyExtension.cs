using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;

namespace AuraLedHelper.Extensions
{
    public static class DependencyPropertyExtension
    {
        private static readonly ConcurrentDictionary<Tuple<TypeInfo, string>, FieldInfo> Cache = new ConcurrentDictionary<Tuple<TypeInfo, string>, FieldInfo>();

        private static FieldInfo GetDependencyPropertyField(TypeInfo type, string propertyName)
        {
            var dpName = propertyName + "Property";
            var key = new Tuple<TypeInfo, string>(type, dpName);
            return Cache.GetOrAdd(key, t => ActuallyGetField(t.Item1, t.Item2));
        }

        private static FieldInfo ActuallyGetField(TypeInfo typeInfo, string propertyName)
        {
            var current = typeInfo;
            while (current != null)
            {
                var ret = current.GetDeclaredField(propertyName);
                if (ret != null && ret.IsStatic) return ret;

                current = current.BaseType != null ? current.BaseType.GetTypeInfo() : null;
            }

            return null;
        }

        public static IObservable<TProperty> OnDependencyPropertyChanged<T, TProperty>(this T source, Expression<Func<T, TProperty>> property)
            where T : DependencyObject
        {
            return Observable.Create<TProperty>(o =>
            {
                var propertyName = property.GetPropertyInfo().Name;
                var propertySelector = property.Compile();

                var field = GetDependencyPropertyField(typeof(T).GetTypeInfo(), propertyName);

                var dp = (DependencyProperty)field.GetValue(null);

                var dpd = DependencyPropertyDescriptor.FromProperty(dp, typeof(T));
                return Observable.FromEventPattern<EventHandler, EventArgs>(
                                h => dpd.AddValueChanged(source, h),
                                h => dpd.RemoveValueChanged(source, h))
                            .Select(e => propertySelector(source))
                            .Subscribe(o);
            });
        }
    }
}
