using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Dora.Interception.Test
{
    public class InterfaceMappingFixture
    {
        [Fact]
        public void GetNormalInterfaceMapping()
        {
            var mapping = ReflectionUtility.GetInterfaceMapForGenericTypeDefinition(typeof(IFoobar<,>), typeof(Foo<,>));
            Assert.Equal(typeof(IFoobar <,>), mapping.InterfaceType);
            Assert.Equal(typeof(Foo <,>), mapping.TargetType);
            Assert.Equal(4, mapping.InterfaceMethods.Length);

            for (int index = 0; index < 4; index++)
            {
                Assert.Equal(
                    mapping.InterfaceMethods[index].GetCustomAttribute<MethodNameAttribute>().Name,
                    mapping.TargetMethods[index].GetCustomAttribute<MethodNameAttribute>().Name);
            }
        }

        [Fact]
        public void GetExplicitlyImplementedInterfaceMapping()
        {
            var mapping = ReflectionUtility.GetInterfaceMapForGenericTypeDefinition(typeof(IFoobar<,>), typeof(Bar<,>));
            Assert.Equal(typeof(IFoobar<,>), mapping.InterfaceType);
            Assert.Equal(typeof(Bar<,>), mapping.TargetType);
            Assert.Equal(4, mapping.InterfaceMethods.Length);

            for (int index = 0; index < 4; index++)
            {
                Assert.Equal(
                    mapping.InterfaceMethods[index].GetCustomAttribute<MethodNameAttribute>().Name,
                    mapping.TargetMethods[index].GetCustomAttribute<MethodNameAttribute>().Name);
            }
        }

        public interface IFoobar<T1, T2>
        {
            [MethodName("M1")]
            void Invoke(string arg1, string arg2);
            [MethodName("M2")]
            void Invoke(int arg1, int arg2);
            [MethodName("M3")]
            void Invoke(Action<T1> arg1, T2 arg2);
            [MethodName("M4")]
            void Invoke<T>(T1 arg1, T2 arg2, T arg3);
        }

        public class Foo<T3, T4> : IFoobar<T3, T4>
        {
            [MethodName("M1")]
            public void Invoke(string arg1, string arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M2")]
            public void Invoke(int arg1, int arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M3")]
            public void Invoke(Action<T3> arg1, T4 arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M4")]
            public void Invoke<T>(T3 arg1, T4 arg2, T arg3)
            {
                throw new NotImplementedException();
            }
        }

        public class Bar<T3, T4> : IFoobar<T3, T4>
        {
            [MethodName("M1")]
            void IFoobar<T3, T4>.Invoke(string arg1, string arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M2")]
            void IFoobar<T3, T4>.Invoke(int arg1, int arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M3")]
            void IFoobar<T3, T4>.Invoke(Action<T3> arg1, T4 arg2)
            {
                throw new NotImplementedException();
            }

            [MethodName("M4")]
            void IFoobar<T3, T4>.Invoke<T>(T3 arg1, T4 arg2, T arg3)
            {
                throw new NotImplementedException();
            }
        }

        public class MethodNameAttribute : Attribute
        {
            public MethodNameAttribute(string name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }
            public string Name { get; }
        }
    }
}
