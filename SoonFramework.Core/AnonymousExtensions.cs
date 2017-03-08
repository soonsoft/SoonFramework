using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    /// <summary>
    /// C# 匿名类型扩展
    /// </summary>
    public static class AnonymousExtensions
    {
        private static ConcurrentDictionary<Type, Type> s_dynamicTypes = new ConcurrentDictionary<Type, Type>();

        private static Func<Type, Type> s_dynamicTypeCreator = new Func<Type, Type>(CreateNewPublicAnonymousType);

        /// <summary>
        /// 匿名类型默认为internal访问级别，为了可以支持用Dynamic方式读取可以用该方法将其转换为public类型
        /// 实际上这时候相当于动态创建一个类型，并为其创建实例以及反射填充属性，性能开销较大不建议在集合类中使用
        /// </summary>
        /// <param name="entity">匿名类型实例</param>
        /// <returns>新的public类型实例</returns>
        public static object ToPublicAnonymousType(this object entity)
        {
            var entityType = entity.GetType();
            var dynamicType = s_dynamicTypes.GetOrAdd(entityType, s_dynamicTypeCreator);

            var dynamicObject = Activator.CreateInstance(dynamicType);
            foreach (var entityProperty in entityType.GetProperties())
            {
                var value = entityProperty.GetValue(entity, null);
                dynamicType.GetField(entityProperty.Name).SetValue(dynamicObject, value);
            }

            return dynamicObject;
        }

        /// <summary>
        /// 创建一个新的公有的匿名对象
        /// </summary>
        /// <param name="entityType">对象的Type</param>
        /// <returns>匿名对象Type</returns>
        private static Type CreateNewPublicAnonymousType(Type entityType)
        {
            var asmName = new AssemblyName("DynamicAssembly_" + Guid.NewGuid());
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule("DynamicModule_" + Guid.NewGuid());

            var typeBuilder = moduleBuilder.DefineType(
                entityType.GetType() + "$DynamicType",
                TypeAttributes.Public);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            foreach (var entityProperty in entityType.GetProperties())
            {
                typeBuilder.DefineField(entityProperty.Name, entityProperty.PropertyType, FieldAttributes.Public);
            }

            return typeBuilder.CreateType();
        }
    }
}
