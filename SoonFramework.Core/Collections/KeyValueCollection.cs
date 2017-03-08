using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core.Collections
{
    public class KeyValueCollection<TKey, TValue> : ICollection<TValue>, IEnumerable<TValue>
        where TValue : class
    {
        //集合长度
        private int _count = 0;
        //移除的个数
        private int _nullCount = 0;

        private IDictionary<TKey, int> _keys;
        private IList<TValue> _values;

        private const int DefaultCapacity = 5;

        #region Constructors

        public KeyValueCollection()
        {
            _keys = CreateKeysCollection(DefaultCapacity);
            _values = CreateValuesCollection(DefaultCapacity);
        }

        public KeyValueCollection(int capacity)
        {
            if (capacity < DefaultCapacity)
            {
                capacity = DefaultCapacity;
            }
            _keys = CreateKeysCollection(capacity);
            _values = CreateValuesCollection(capacity);
        }

        #endregion

        /// <summary>
        /// 根据索引取值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>值</returns>
        public TValue this[int index]
        {
            get
            {
                return DoGet(index);
            }
        }

        /// <summary>
        /// 根据key取值或者设置值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return DoGet(key);
            }
            set
            {
                if (this._keys.ContainsKey(key))
                {
                    this._values[this._keys[key]] = value;
                }
                else
                {
                    DoAdd(key, value);
                }
            }
        }

        /// <summary>
        /// 创建存放值的列表并制定初始大小
        /// </summary>
        /// <param name="capacity">初始大小</param>
        /// <returns></returns>
        protected virtual IList<TValue> CreateValuesCollection(int capacity)
        {
            return new List<TValue>(capacity);
        }

        /// <summary>
        /// 创建存放key的列表并指定初始大小
        /// </summary>
        /// <param name="capacity">初始大小</param>
        /// <returns></returns>
        protected virtual IDictionary<TKey, int> CreateKeysCollection(int capacity)
        {
            return new Dictionary<TKey, int>(capacity);
        }

        /// <summary>
        /// 从Value中获取Key
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual TKey GetKeyFromValue(TValue value)
        {
            if (GetKey == null)
            {
                throw new NullReferenceException("Func GetKey is null");
            }
            return value != null ? GetKey(value) : default(TKey);
        }

        private Func<TValue, TKey> _getKeyFunc = null;
        /// <summary>
        /// 设置GetKey方法
        /// </summary>
        public Func<TValue, TKey> GetKey
        {
            private get
            {
                return this._getKeyFunc;
            }
            set
            {
                this._getKeyFunc = value;
            }
        }

        /// <summary>
        /// 判断Key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return this._keys.ContainsKey(key);
        }

        #region ICollection<>

        /// <summary>
        /// 向集合添加元素
        /// </summary>
        /// <param name="value"></param>
        public void Add(TValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            TKey key = GetKeyFromValue(value);
            DoAdd(key, value);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            this._keys.Clear();
            this._values.Clear();
            this._nullCount = 0;
            this._count = 0;
        }

        /// <summary>
        /// 确定集合中是否包含值
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>bool</returns>
        public bool Contains(TValue value)
        {
            return this._values.Contains(value);
        }

        /// <summary>
        /// 从特定的索引开始将结合的元素复制到指定的数组中
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">开始复制的索引</param>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            this._values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 返回集合中包含的元素数量
        /// </summary>
        public int Count
        {
            get { return this._keys.Count; }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        public bool IsReadOnly
        {
            get { return this._values.IsReadOnly; }
        }

        /// <summary>
        /// 移除一个元素
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Remove(TValue value)
        {
            TKey key = GetKeyFromValue(value);
            return DoRemove(key);
        }

        /// <summary>
        /// 创建一个迭代器，检查并尝试重建索引
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            this.DoCompact();
            return this._values.GetEnumerator();
        }

        /// <summary>
        /// 创建一个迭代器，检查并尝试重建索引
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            this.DoCompact();
            return this._values.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存项</param>
        protected virtual void DoAdd(TKey key, TValue value)
        {
            Guard.ArgumentNotNull(key, "key");

            this._keys.Add(key, this._count);
            this._values.Add(value);
            this._count++;
        }

        /// <summary>
        /// 根据key移除元素
        /// </summary>
        /// <param name="key">key</param>
        protected virtual bool DoRemove(TKey key)
        {
            Guard.ArgumentNotNull(key, "key");

            if (this._keys.ContainsKey(key))
            {
                int index = this._keys[key];
                this._keys.Remove(key);
                this._values[index] = null;
                this._nullCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取项
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>缓存项</returns>
        protected virtual TValue DoGet(TKey key)
        {
            Guard.ArgumentNotNull(key, "key");

            if (this._keys.ContainsKey(key))
            {
                return this._values[this._keys[key]];
            }
            else
            {
                return default(TValue);
            }
        }

        /// <summary>
        /// 根据索引获取值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        protected virtual TValue DoGet(int index)
        {
            this.DoCompact();
            if (index < 0 || index >= this._count)
            {
                return default(TValue);
            }

            return this._values[index];
        }

        /// <summary>
        /// 移除List中的空引用，重新计算索引
        /// </summary>
        protected virtual void DoCompact()
        {
            if (_nullCount > 0)
            {
                List<TValue> tempList = new List<TValue>(_count - _nullCount);
                int startIndex = -1;
                for (int i = 0; i < _count; i++)
                {
                    if (_values[i] != null)
                    {
                        tempList.Add(_values[i]);
                    }
                    else if (startIndex == -1)
                    {
                        startIndex = i;
                    }
                }

                this._nullCount = 0;
                this._values = tempList;
                this._count = this._values.Count;
                for (int j = startIndex; j < this._count; j++)
                {
                    TValue item = this._values[j];
                    this._keys[GetKeyFromValue(item)] = j;
                }
            }
        }
    }
}
