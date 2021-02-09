using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleTCP.Utils
{
    public static class TypeUtils
    {
        public static Assembly GetAssembly(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == name).FirstOrDefault();
        }

        public static List<Type> GetAllTypes()
        {
            List<Type> types = new List<Type>(2);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type subType in assemblies[i].GetTypes())
                {
                    types.Add(subType);
                }
            }

            return types;
        }

        public static List<Type> GetAllSubTypes(Type type)
        {
            List<Type> types = new List<Type>(2);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type subType in assemblies[i].GetTypes())
                {
                    if (!subType.IsClass || subType.IsAbstract || subType.IsGenericTypeDefinition
                     || !subType.IsPublic || (type.IsClass && !subType.IsSubclassOf(type))
                     || (type.IsInterface && !type.IsAssignableFrom(subType)))
                        continue;

                    types.Add(subType);
                }
            }

            return types;
        }

        public static List<Type> GetSubTypes(Type type)
        {
            List<Type> types = new List<Type>(2);
            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (Type subType in asm.GetTypes())
            {
                if (!subType.IsClass || subType.IsAbstract || subType.IsGenericTypeDefinition
                    || !subType.IsPublic || (type.IsClass && !subType.IsSubclassOf(type))
                    || (type.IsInterface && !type.IsAssignableFrom(subType)))
                    continue;

                types.Add(subType);
            }

            return types;
        }

        public static object CreateInstance(this Type type)
        {
            if (type.IsAbstract || type.IsGenericTypeDefinition || type.IsPrimitive) return null;

            // For value type
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            ConstructorInfo ctor = GetDefaultConstructor(type);

            if (ctor == null) return null;

            return ctor.Invoke(new object[0]);
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        }
    }
}
