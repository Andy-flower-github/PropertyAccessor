using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Utilities
{
    /// <summary>
    /// Andy -> 物件屬性存取器
    /// </summary>
    public class PropertyAccessor
    {
        /// <summary>
        /// Andy -> 利用object建立一個屬性存取器
        /// </summary>
        public static PropertyAccessor Create(object source)
            => new PropertyAccessor(source);

        /// <summary>
        /// Andy -> 利用 class 建立一個屬性存取器
        /// </summary>
        public static PropertyAccessor Create<T>()
            => new PropertyAccessor(typeof(T));

        /// <summary>
        /// Andy -> 複制相同Type的物件資料, 到指定的物件
        /// </summary>
        public static void CloneValues<T>(T source, T destination)
            => Create(destination).SetValues(Create(source).GetKeyValues());

        /// <summary>
        /// Andy -> 檢查兩個相同型別物件的所有屬性值是否相同
        /// </summary>
        public static bool IsValueEqual(object self, object other)
        {
            if (self.GetType() != other.GetType())
                return false;

            return Create(self).GetValues().SequenceEqual(Create(other).GetValues());
        }

        /// <summary>
        /// Andy -> 檢查兩個集合物件的所有元素的屬性值是否相同
        /// </summary>
        public static bool IsManyValueEqual<T>(IEnumerable<T> selfs, IEnumerable<T> others)
        {
            if (selfs.Count() != others.Count())
                return false;

            return selfs.Zip(others, (x, y) => PropertyAccessor.IsValueEqual(x, y)).Any(x => x == false) == false;
        }

        /// <summary>
        /// Andy -> 利用現有的物件, 複制出一個新的物件
        /// </summary>
        public static T Clone<T>(T source) where T : new()
        {
            T newObj = new T();
            CloneValues(source, newObj);
            return newObj;
        }

        /// <summary>
        /// Andy -> 利用現有的 IEnumerable, yield return 一個新 IEnumerable
        /// </summary>
        public static IEnumerable<T> CloneMany<T>(IEnumerable<T> sources) where T : new()
        {
            foreach (var item in sources)
            {
                yield return Clone(item);
            }
        }

        /// <summary>
        /// Andy -> 利用現有的 IEnumerable, yield return 成一個 IEnumerable
        /// </summary>
        public static IEnumerable<R> AutoCloneMany<T, R>(IEnumerable<T> sources, params string[] ignoreProps) where R : new()
        {
            var sourcepa = Create<T>();
            var targetpa = Create<R>();
            var updateNames = targetpa.GetCanWriteProperties().Intersect(sourcepa.GetCanReadProperties())
               .Where(x => !ignoreProps.Contains(x)).ToArray();

            foreach (var item in sources)
            {
                R newObj = new R();
                sourcepa.SetDataSource(item);
                targetpa.SetDataSource(newObj);
                targetpa.SetValues(updateNames, sourcepa.GetValues(updateNames));
                //MapTo(item, updateNames, newObj, updateNames);
                yield return newObj;
            }
        }

        /// <summary>
        /// Andy -> 將不同型別物件, 相同Property 的值由 source to destination
        /// </summary>
        public static void AutoMapTo(object source, object destination, params string[] ignoreProps)
        {
            var sourceAccessor = Create(source);
            var destinationAccressor = Create(destination);
            var updateNames = destinationAccressor.GetCanWriteProperties().Intersect(sourceAccessor.GetCanReadProperties())
                .Where(x => !ignoreProps.Contains(x)).ToArray();
            destinationAccressor.SetValues(updateNames, sourceAccessor.GetValues(updateNames));
        }

        /// <summary>
        /// Andy -> 不同型別物件, 自行定義要複製的 Property
        /// </summary>
        public static void MapTo(object source, string[] sourcePropertyNames, object destination, string[] destinataionPropertyNames)
        {
            var sourceValues = Create(source).GetValues(sourcePropertyNames);
            Create(destination).SetValues(destinataionPropertyNames, sourceValues);
        }


        /// <summary>
        /// Andy -> Dictionary[propertyname, PropertyInfo] - StringComparer.OrdinalIgnoreCase
        /// </summary>
        public Dictionary<string, PropertyInfo> PropertyInfoDic { get; private set; }
        private object dataSource;

        /// <summary>
        /// Andy -> 利用 PropertyName 來存取物件的指定屬性值, 傳入 class
        /// </summary>   
        public PropertyAccessor(Type classType)
            => PropertyInfoDic = classType.GetProperties().ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Andy -> 利用 PropertyName 來存取物件的指定屬性值, 傳入 object
        /// </summary>
        public PropertyAccessor(object dataSource) : this(dataSource.GetType())
            => this.dataSource = dataSource;

        /// <summary>
        /// 設定要存取的物件來源
        /// </summary>
        public PropertyAccessor SetDataSource(object obj)
        {
            dataSource = obj;
            return this;
        }

        /// <summary>
        /// Andy -> 使用 Indexer 來存取 Property
        /// </summary>
        public object this[string propertyName]
        {
            get
            {
                if (PropertyInfoDic.TryGetValue(propertyName, out PropertyInfo pinfo))
                    return pinfo.GetValue(dataSource, null);
                else
                    return null;
            }

            set
            {
                if (PropertyInfoDic.TryGetValue(propertyName, out PropertyInfo pinfo))
                    pinfo.SetValue(dataSource, value, null);
            }
        }

        /// <summary>
        /// Andy -> 取出屬性值, 並轉成指定型別
        /// </summary>
        public T Get<T>(string propertyName)
        {
            object objValue = this[propertyName];
            if (objValue == null)
                return default(T);

            T tvalue = default(T);
            try { tvalue = (T)objValue; }
            catch { }

            return tvalue;
        }

        /// <summary>
        /// Andy -> 檢查屬性名稱是否存在 - propertyName 不分大小寫
        /// </summary>
        public bool ContainsKey(string propertyName)
            => PropertyInfoDic?.ContainsKey(propertyName) ?? false;

        /// <summary>
        /// Andy -> 針對是 Null 的值. 設定物件的初值 numeric => 0, string = "", bool = false, datetime = null
        /// </summary>
        public void SetNullValuesToDefault()
        {
            foreach (var pinfo in PropertyInfoDic.Select(x => x.Value).Where(x => x.CanWrite && this[x.Name] == null))
            {
                var type = pinfo.PropertyType.GetUnderlyingType();
                var value = type.MyDefault();
                if (value != null && Type.GetTypeCode(type) != TypeCode.Object)
                    this[pinfo.Name] = value;
            }
        }

        /// <summary>
        /// Andy -> 取出指定屬性的值
        /// </summary>
        public object[] GetValues(params string[] propertyNames)
            => propertyNames.Select(o => this[o]).ToArray();

        /// <summary>
        /// Andy -> 取出所有欄位的 value
        /// </summary>
        public object[] GetValues()
            => GetValues(GetCanReadProperties());

        /// <summary>
        /// Andy -> 取出所有欄位的 Key-value
        /// </summary>
        public IDictionary<string, object> GetKeyValues()
            => GetCanReadProperties().Select(x => new { key = x, value = this[x] }).ToDictionary(x => x.key, x => x.value);

        /// <summary>
        /// Andy -> 取出指定欄位的 Key-value
        /// </summary>
        public IDictionary<string, object> GetKeyValues(params string[] propertyNames)
            => propertyNames.Select(x => new { key = x, value = this[x] }).ToDictionary(x => x.key, x => x.value);

        /// <summary>
        /// Andy -> 使用 key-value pair 寫入欄位
        /// </summary>
        public void SetValues(IDictionary<string, object> keyvalues)
        {
            foreach (var item in keyvalues)
            {
                if (ContainsKey(item.Key) && PropertyInfoDic[item.Key].CanWrite)
                    this[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Andy -> 使用 keys, values 寫入指定屬性的值
        /// </summary>
        public void SetValues(string[] propertyNames, object[] values)
        {
            int i = 0;
            foreach (var item in propertyNames)
            {
                if (this.PropertyInfoDic[item].CanWrite)
                {
                    this[item] = values[i];
                }
                i++;
            }
        }

        /// <summary>
        /// Andy -> 寫入指定屬性的值
        /// </summary>
        public void SetValues(object[] values)
            => SetValues(this.GetCanReadWriteProperties(), values);

        /// <summary>
        /// Andy -> 取出所有可以 Read & Write 的屬性
        /// </summary>
        public string[] GetCanReadWriteProperties()
            => PropertyInfoDic.Where(x => x.Value.CanRead && x.Value.CanWrite).Select(x => x.Key).ToArray();

        /// <summary>
        /// Andy -> 取出所有可以 Read  的屬性
        /// </summary>
        public string[] GetCanReadProperties()
            => PropertyInfoDic.Where(x => x.Value.CanRead).Select(x => x.Key).ToArray();

        /// <summary>
        /// Andy -> 取出所有可以 Write 的屬性
        /// </summary>
        public string[] GetCanWriteProperties()
            => PropertyInfoDic.Where(x => x.Value.CanWrite).Select(x => x.Key).ToArray();

        /// <summary>
        /// Andy -> 針對兩個屬性值比較大小
        /// </summary>
        public int Comparison<T>(T data1, T data2, string prop) where T : class
            => Comparer.Default.Compare(SetDataSource(data1)[prop], SetDataSource(data2)[prop]);
    }
}