using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace SoonFramework.Data
{
    /// <summary>
    /// DbEntityValidationException扩展方法，显示内部异常内容
    /// </summary>
    public static class DbEntityValidationExceptionExtension
    {
        /// <summary>
        /// 获取DbEntityValidationException的内部异常信息
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns></returns>
        public static string GetMessage(this DbEntityValidationException exception)
        {
            StringBuilder message = new StringBuilder();
            foreach (var error in exception.EntityValidationErrors)
            {
                foreach (var e in error.ValidationErrors)
                {
                    message.Append(e.PropertyName).Append("属性：")
                        .Append(e.ErrorMessage).Append(";");
                }
            }
            return message.ToString();
        }
    }
}
