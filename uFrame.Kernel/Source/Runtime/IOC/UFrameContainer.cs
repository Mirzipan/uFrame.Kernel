using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UniRx;

namespace uFrame.IOC

{
    /// <summary>
    /// A ViewModel Container and a factory for Controllers and commands.
    /// </summary>
    public class UFrameContainer : IUFrameContainer
    {
        private readonly Dictionary<Type, TypeInjectionInfo> _typeInjectionInfos =
            new Dictionary<Type, TypeInjectionInfo>();

        private readonly Dictionary<Type, TypeReflectionInfo> _typeReflectionInfos =
            new Dictionary<Type, TypeReflectionInfo>();

        private TypeInstanceCollection _instances;
        private TypeMappingCollection _mappings;

        public TypeMappingCollection Mappings
        {
            get { return _mappings ?? (_mappings = new TypeMappingCollection()); }
            set { _mappings = value; }
        }

        public TypeInstanceCollection Instances
        {
            get { return _instances ?? (_instances = new TypeInstanceCollection()); }
            set { _instances = value; }
        }

        public TypeRelationCollection RelationshipMappings
        {
            get { return _relationshipMappings; }
            set { _relationshipMappings = value; }
        }

        public IEnumerable<TType> ResolveAll<TType>()
        {
            foreach (var obj in ResolveAll(typeof(TType)))
                yield return (TType) obj;
        }

        /// <summary>
        /// Resolves all instances of TType or subclasses of TType.  Either named or not.
        /// </summary>
        /// <typeparam name="TType">The Type to resolve</typeparam>
        /// <returns>List of objects.</returns>
        public IEnumerable<object> ResolveAll(Type type)
        {
            foreach (var kv in Instances)
            {
                if (kv.Key.Type == type && !string.IsNullOrEmpty(kv.Key.Name))
                    yield return kv.Value;
            }

            foreach (var kv in Mappings)
            {
                if (string.IsNullOrEmpty(kv.Key.Name)) 
                    continue;
                
#if NETFX_CORE
                var condition = type.GetTypeInfo().IsSubclassOf(mapping.From);
#else
                var condition = type.IsAssignableFrom(kv.Key.Type);
#endif
                if (!condition) 
                    continue;
                
                var item = Activator.CreateInstance(kv.Value);
                Inject(item);
                yield return item;
            }
        }

        /// <summary>
        /// Clears all type-mappings and instances.
        /// </summary>
        public void Clear()
        {
            Instances.Clear();
            Mappings.Clear();
            RelationshipMappings.Clear();
        }

        /// <summary>
        /// Injects registered types/mappings into an object
        /// </summary>
        /// <param name="obj"></param>
        public void Inject(object obj)
        {
            if (obj == null) return;

            var objectType = obj.GetType();
            var typeInjectionInfo = GetTypeInjectionInfo(objectType);

            for (var i = 0; i < typeInjectionInfo.PropertyInjectionInfos.Length; i++)
            {
                var injectionInfo = typeInjectionInfo.PropertyInjectionInfos[i];
                injectionInfo.MemberInfo.SetValue(obj,
                    Resolve(injectionInfo.MemberType, injectionInfo.InjectName, false, null), null);
            }

            for (var i = 0; i < typeInjectionInfo.FieldInjectionInfos.Length; i++)
            {
                var injectionInfo = typeInjectionInfo.FieldInjectionInfos[i];
                injectionInfo.MemberInfo.SetValue(obj,
                    Resolve(injectionInfo.MemberType, injectionInfo.InjectName, false, null));
            }
        }

        /// <summary>
        /// Register a type mapping
        /// </summary>
        /// <typeparam name="TSource">The base type.</typeparam>
        /// <typeparam name="TTarget">The concrete type</typeparam>
        public void Register<TSource, TTarget>(string name = null)
        {
            Mappings[typeof(TSource), name] = typeof(TTarget);
        }

        public void Register(Type source, Type target, string name = null)
        {
            Mappings[source, name] = target;
        }

        /// <summary>
        /// Register a named instance
        /// </summary>
        /// <param name="baseType">The type to register the instance for.</param>
        /// <param name="instance">The instance that will be resolved be the name</param>
        /// <param name="injectNow">Perform the injection immediately</param>
        public void RegisterInstance(Type baseType, object instance = null, bool injectNow = true)
        {
            RegisterInstance(baseType, instance, null, injectNow);
        }

        /// <summary>
        /// Register a named instance
        /// </summary>
        /// <param name="baseType">The type to register the instance for.</param>
        /// <param name="name">The name for the instance to be resolved.</param>
        /// <param name="instance">The instance that will be resolved be the name</param>
        /// <param name="injectNow">Perform the injection immediately</param>
        public virtual void RegisterInstance(Type baseType, object instance = null, string name = null,
            bool injectNow = true)
        {
            Instances[baseType, name] = instance;

            if (injectNow)
                Inject(instance);
        }

        public void RegisterInstance<TBase>(TBase instance) where TBase : class
        {
            RegisterInstance<TBase>(instance, true);
        }

        public void RegisterInstance<TBase>(TBase instance, bool injectNow) where TBase : class
        {
            RegisterInstance<TBase>(instance, null, injectNow);
        }

        public void RegisterInstance<TBase>(TBase instance, string name, bool injectNow = true) where TBase : class
        {
            RegisterInstance(typeof(TBase), instance, name, injectNow);
        }

        public void RegisterInstanceWithInterfaces<TBase>(TBase instance, bool injectNow) where TBase : class
        {
            RegisterInstanceWithInterfaces<TBase>(instance, null, injectNow);
        }

        public void RegisterInstanceWithInterfaces(Type baseType, object instance = null, bool injectNow = true)
        {
            RegisterInstanceWithInterfaces(baseType, instance, null, injectNow);
        }

        public void RegisterInstanceWithInterfaces(Type baseType, object instance = null, string name = null,
            bool injectNow = true)
        {
            var interfaces = baseType.GetInterfaces();
            foreach (var iface in interfaces)
                RegisterInstance(iface, instance, name);

            Instances[baseType, name] = instance;

            if (injectNow)
                Inject(instance);
        }

        public void RegisterInstanceWithInterfaces<TBase>(TBase instance, string name, bool injectNow = true)
            where TBase : class
        {
            RegisterInstanceWithInterfaces(typeof(TBase), instance, name, injectNow);
        }

        public void RegisterInstanceWithInterfaces<TBase>(TBase instance) where TBase : class
        {
            RegisterInstanceWithInterfaces<TBase>(instance, true);
        }

        /// <summary>
        ///  If an instance of T exist then it will return that instance otherwise it will create a new one based off mappings.
        /// </summary>
        /// <typeparam name="T">The type of instance to resolve</typeparam>
        /// <returns>The/An instance of 'instanceType'</returns>
        public T Resolve<T>(string name = null, bool requireInstance = false, object[] args = null) where T : class
        {
            return (T) Resolve(typeof(T), name, requireInstance, args);
        }

        /// <summary>
        /// If an instance of instanceType exist then it will return that instance otherwise it will create a new one based off mappings.
        /// </summary>
        /// <param name="baseType">The type of instance to resolve</param>
        /// <param name="name">The type of instance to resolve</param>
        /// <param name="requireInstance">If true will return null if an instance isn't registered.</param>
        /// <param name="constructorArgs">The arguments to pass to the constructor if any.</param>
        /// <returns>The/An instance of 'instanceType'</returns>
        public object Resolve(Type baseType, string name = null, bool requireInstance = false,
            object[] constructorArgs = null)
        {
            // Look for an instance first
            var item = Instances[baseType, name];
            if (item != null)
                return item;

            if (requireInstance)
                return null;

            // Check if there is a mapping of the type
            var namedMapping = Mappings[baseType, name];
            if (namedMapping == null) 
                return null;
            
            var obj = CreateInstance(namedMapping, constructorArgs);
            //Inject(obj);
            return obj;

        }

        public object CreateInstance(Type type, object[] constructorArgs = null)
        {
            // If we have args to pass, just let Activator.CreateInstance figure out
            // what constructor fits best
            if (constructorArgs != null && constructorArgs.Length > 0)
            {
                var obj2 = Activator.CreateInstance(type, constructorArgs);
                Inject(obj2);
                return obj2;
            }

            // Otherwise, find the public constructor that has the most arguments,
            // and then try to resolve these arguments
            TypeReflectionInfo reflectionInfo = GetTypeReflectionInfo(type);

            var maxParameters = reflectionInfo.PublicConstructors[0].Parameters;

            for (var i = 0; i < reflectionInfo.PublicConstructors.Length; i++)
            {
                var c = reflectionInfo.PublicConstructors[i];
                var parameters = c.Parameters;
                if (parameters.Length > maxParameters.Length)
                    maxParameters = parameters;
            }

            var args = new object[maxParameters.Length];
            for (var i = 0; i < maxParameters.Length; i++)
            {
                var parameterInfo = maxParameters[i];
                if (parameterInfo.ParameterType.IsArray)
                {
                    args[i] = ResolveAll(parameterInfo.ParameterType);
                }
                else
                {
                    args[i] =
                        Resolve(parameterInfo.ParameterType) ??
                        Resolve(parameterInfo.ParameterType, parameterInfo.Name);
                }
            }

            var obj = Activator.CreateInstance(type, args);
            Inject(obj);
            return obj;
        }

        public TBase ResolveRelation<TBase>(Type tfor, object[] args = null)
        {
            try
            {
                return (TBase) ResolveRelation(tfor, typeof(TBase), args);
            }
            catch (InvalidCastException castIssue)
            {
                throw new Exception($"Resolve Relation couldn't cast  to {typeof(TBase).Name} from {tfor.Name}",
                    castIssue);
            }
        }

        public void InjectAll()
        {
            foreach (var instance in Instances.Values)
                Inject(instance);
        }

        private TypeRelationCollection _relationshipMappings = new TypeRelationCollection();

        public void RegisterRelation<TFor, TBase, TConcrete>()
        {
            RelationshipMappings[typeof(TFor), typeof(TBase)] = typeof(TConcrete);
        }

        public void RegisterRelation(Type tfor, Type tbase, Type tconcrete)
        {
            RelationshipMappings[tfor, tbase] = tconcrete;
        }

        public object ResolveRelation(Type tfor, Type tbase, object[] args = null)
        {
            var concreteType = RelationshipMappings[tfor, tbase];

            if (concreteType == null)
                return null;

            var result = CreateInstance(concreteType, args);
            //Inject(result);
            return result;
        }

        public TBase ResolveRelation<TFor, TBase>(object[] arg = null)
        {
            return (TBase) ResolveRelation(typeof(TFor), typeof(TBase), arg);
        }

        private TypeInjectionInfo GetTypeInjectionInfo(Type type)
        {
            TypeInjectionInfo typeInjectionInfo;
            if (_typeInjectionInfos.TryGetValue(type, out typeInjectionInfo))
                return typeInjectionInfo;

            typeInjectionInfo = new TypeInjectionInfo(type);
            _typeInjectionInfos.Add(type, typeInjectionInfo);

            return typeInjectionInfo;
        }

        private TypeReflectionInfo GetTypeReflectionInfo(Type type)
        {
            TypeReflectionInfo typeReflectionInfo;
            if (_typeReflectionInfos.TryGetValue(type, out typeReflectionInfo))
                return typeReflectionInfo;

            typeReflectionInfo = new TypeReflectionInfo(type);
            _typeReflectionInfos.Add(type, typeReflectionInfo);

            return typeReflectionInfo;
        }

        private class TypeReflectionInfo
        {
            public readonly ConstructorInfoData[] PublicConstructors;

            public TypeReflectionInfo(Type type)
            {
#if !NETFX_CORE
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
#else
                var constructors = type.GetTypeInfo().DeclaredConstructors.ToArray();
#endif
                PublicConstructors =
                    constructors
                        .Select(constructor => new ConstructorInfoData(constructor, constructor.GetParameters()))
                        .ToArray();
            }

            public struct ConstructorInfoData
            {
                public readonly ConstructorInfo Constructor;
                public readonly ParameterInfo[] Parameters;

                public ConstructorInfoData(ConstructorInfo constructor, ParameterInfo[] parameters)
                {
                    Constructor = constructor;
                    Parameters = parameters;
                }
            }
        }

        private class TypeInjectionInfo
        {
            public readonly InjectionMemberInfo<PropertyInfo>[] PropertyInjectionInfos;
            public readonly InjectionMemberInfo<FieldInfo>[] FieldInjectionInfos;

            public TypeInjectionInfo(Type type)
            {
                var propertyInjectionInfos = new List<InjectionMemberInfo<PropertyInfo>>();
                var fieldInjectionInfos = new List<InjectionMemberInfo<FieldInfo>>();
#if !NETFX_CORE
                var members = type.GetMembers();
#else
                var members = type.GetTypeInfo().DeclaredMembers;
#endif
                Type injectAttributeType = typeof(InjectAttribute);
                foreach (var memberInfo in members)
                {
                    InjectAttribute injectAttribute =
                        (InjectAttribute) Attribute.GetCustomAttribute(memberInfo, injectAttributeType);
                    if (injectAttribute == null)
                        continue;

                    var propertyInfo = memberInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        propertyInjectionInfos.Add(new InjectionMemberInfo<PropertyInfo>(propertyInfo,
                            propertyInfo.PropertyType, injectAttribute.Name));
                        continue;
                    }

                    var fieldInfo = memberInfo as FieldInfo;
                    if (fieldInfo != null)
                    {
                        fieldInjectionInfos.Add(new InjectionMemberInfo<FieldInfo>(fieldInfo, fieldInfo.FieldType,
                            injectAttribute.Name));
                    }
                }

                PropertyInjectionInfos = propertyInjectionInfos.ToArray();
                FieldInjectionInfos = fieldInjectionInfos.ToArray();
            }

            public class InjectionMemberInfo<T> where T : MemberInfo
            {
                public readonly T MemberInfo;
                public readonly Type MemberType;
                public readonly string InjectName;

                public InjectionMemberInfo(T memberInfo, Type memberType, string injectName)
                {
                    MemberInfo = memberInfo;
                    MemberType = memberType;
                    InjectName = injectName;
                }
            }
        }
    }

    public class TypeMappingCollection : Dictionary<TypeInstanceId, Type>
    {
        public Type this[Type from, string name = null]
        {
            get
            {
                var key = new TypeInstanceId(from, name);
                Type mapping = null;
                return TryGetValue(key, out mapping) ? mapping : null;
            }
            set
            {
                var key = new TypeInstanceId(from, name);
                this[key] = value;
            }
        }
    }

    public class TypeInstanceCollection : Dictionary<TypeInstanceId, object>
    {
        public object this[Type from, string name = null]
        {
            get
            {
                var key = new TypeInstanceId(from, name);
                object mapping = null;
                return TryGetValue(key, out mapping) ? mapping : null;
            }
            set
            {
                var key = new TypeInstanceId(from, name);
                this[key] = value;
            }
        }
    }

    public class TypeRelationCollection : Dictionary<TypeMapping, Type>
    {
        public Type this[Type from, Type to]
        {
            get
            {
                var key = new TypeMapping(from, to);
                Type mapping = null;
                return TryGetValue(key, out mapping) ? mapping : null;
            }
            set
            {
                var key = new TypeMapping(from, to);
                this[key] = value;
            }
        }
    }
}