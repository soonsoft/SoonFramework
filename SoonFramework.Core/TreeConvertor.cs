using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    /// <summary>
    /// 树形结构转换器
    /// </summary>
    /// <typeparam name="T">数据项类型</typeparam>
    /// <typeparam name="TKey">数据项Key</typeparam>
    public abstract class TreeConvertor<T, TKey>
        where T : class
    {
        protected abstract T CreateItem();

        protected abstract T CloneItem(T item);

        protected abstract TKey GetItemKey(T item);

        protected abstract TKey GetParentKey(T item);

        protected abstract void SetParent(T item, T parent);

        protected abstract IList<T> GetChildren(T item);

        protected abstract void SetChildren(T item1, T item2);

        protected abstract bool IsRootKey(TKey key);

        public virtual IList<T> ToMenuTree(IEnumerable<T> source)
        {
            Guard.ArgumentNotNull(source, "source");

            Dictionary<TKey, T> temp = new Dictionary<TKey, T>(source.Count());
            T node = default(T);
            T subNode = default(T);
            TKey parentKey;
            TKey childKey;

            foreach (T n in source)
            {
                subNode = CloneItem(n);
                parentKey = GetParentKey(subNode);

                if (temp.ContainsKey(parentKey))
                {
                    node = temp[parentKey];
                }
                else
                {
                    node = CreateItem();
                    temp[parentKey] = node;
                }

                SetParent(subNode, node);
                GetChildren(node).Add(subNode);

                childKey = GetItemKey(subNode);
                if (temp.ContainsKey(childKey))
                {
                    node = temp[childKey];
                    SetChildren(subNode, node);
                    SetParent(subNode, node);
                }
                temp[childKey] = subNode;
            }

            IList<T> tree = null;
            foreach (TKey key in temp.Keys)
            {
                if (IsRootKey(key))
                {
                    tree = GetChildren(temp[key]);
                    break;
                }
            }
            if (tree == null)
                tree = new List<T>();
            return tree;
        }
    }
}
