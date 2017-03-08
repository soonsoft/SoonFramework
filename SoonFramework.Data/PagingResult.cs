using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    public class PagingResult<TResult>
    {
        /// <summary>
        /// 分页返回的结果
        /// </summary>
        public TResult Result 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int RowCount
        {
            get;
            set;
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get;
            set;
        }
    }
}
