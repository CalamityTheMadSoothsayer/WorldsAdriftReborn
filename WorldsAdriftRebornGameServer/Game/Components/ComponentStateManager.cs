﻿using System.Reflection;
using System.Runtime.InteropServices;
using Improbable.Entity.Component;
using Improbable.Worker;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Networking.Singleton;

namespace WorldsAdriftRebornGameServer.Game.Components
{
    internal class ComponentStateManager
    {
        private static ComponentStateManager instance { get; set; }

        public static ComponentStateManager Instance
        {
            get
            {
                return instance ?? (instance = new ComponentStateManager());
            }
        }

        private static class HashCache<T>
        {
            public static bool Initialized;
            public static ulong Id;
        }

        protected delegate void RegisterDelegate( ENetPeerHandle player, long entityId, object clientComponentUpdate,
            object serverComponentData );

        private readonly Dictionary<ulong, RegisterDelegate> _handlers = new Dictionary<ulong, RegisterDelegate>();

        //FNV-1 64 bit hash
        public ulong GetHash<T>()
        {
            if (HashCache<T>.Initialized)
            {
                return HashCache<T>.Id;
            }

            ulong hash = 14695981039346656037UL; //offset
            string typeName = typeof(T).FullName;
            for (int i = 0; i < typeName.Length; i++)
            {
                hash ^= typeName[i];
                hash *= 1099511628211UL; //prime
            }

            HashCache<T>.Initialized = true;
            HashCache<T>.Id = hash;
            return hash;
        }

        private ComponentStateManager()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterAllComponentUpdateHandlers(assembly);
            }
        }

        private static bool IsSubclassOfRawGeneric( Type generic, Type toCheck )
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        private void RegisterAllComponentUpdateHandlers( Assembly assembly )
        {
            IEnumerable<Type> definedHandlers = assembly.GetTypes()
                                                        .Where(t => t.GetCustomAttributes(typeof(ComponentStateHandler),
                                                            true).Length > 0);

            MethodInfo registerMethod = this.GetType().GetMethods()
                                            .Where(m => m.Name == nameof(RegisterComponentUpdateHandler))
                                            .Where(m => m.IsGenericMethod)
                                            .FirstOrDefault();

            foreach (Type type in definedHandlers)
            {
                if (IsSubclassOfRawGeneric(typeof(IComponentStateHandler<,,>), type))
                {
                    Type type_baseComponentUpdate = type.BaseType.GetGenericArguments()[0];
                    Type type_clientComponentUpdate = type.BaseType.GetGenericArguments()[1];
                    Type type_serverComponentData = type.BaseType.GetGenericArguments()[2];

                    // dynamically create instance of handler
                    Type handlerMethodArgTypes = typeof(Action<,,,>).MakeGenericType(typeof(ENetPeerHandle),
                        typeof(long), type_clientComponentUpdate, type_serverComponentData);
                    object handler = Activator.CreateInstance(type);
                    Delegate handlerMethod = Delegate.CreateDelegate(handlerMethodArgTypes, handler,
                        type.GetMethod("HandleUpdate",
                            new Type[]
                            {
                                typeof(ENetPeerHandle), typeof(long), type_clientComponentUpdate,
                                type_serverComponentData
                            }));

                    // register created handler
                    MethodInfo genericRegisterComponent = registerMethod.MakeGenericMethod(type_baseComponentUpdate,
                        type_clientComponentUpdate, type_serverComponentData);
                    genericRegisterComponent.Invoke(this, new object[] { handlerMethod });

                    Console.WriteLine("[success] registered ComponentUpdate handler for type " +
                                      type_baseComponentUpdate);
                }
            }
        }

        public void RegisterComponentUpdateHandler<TBase, TClient, TServer>(
            Action<ENetPeerHandle, long, TClient, TServer> onProcess )
        {
            ulong hash = GetHash<TBase>();
            if (!_handlers.ContainsKey(hash))
            {
                _handlers.Add(hash, null);
            }

            _handlers[hash] = ( ENetPeerHandle player, long entityId, object clientComponentUpdate,
                object serverComponentData ) =>
            {
                onProcess(player, entityId, (TClient)clientComponentUpdate, (TServer)serverComponentData);
            };
        }

        public unsafe bool HandleComponentUpdate( ENetPeerHandle player, long entityId, uint componentId,
            byte* componentData, int componentDataLength )
        {
            bool success = false;
            // Console.WriteLine("[info] trying to handle a ComponentUpdateOp for " + componentId);
            var entity = EntityManager.GlobalEntityRealm[entityId];
            if (!entity.GetComponents().Contains(componentId))
            {
                Console.WriteLine("WARNING - Could not match ComponentUpdate " + componentId + " of entity " + entityId);
                return false;
            }
            ComponentProtocol.ClientObject* wrapper = ClientObjects.ObjectAlloc();
            var deserialize = ComponentsManager.Instance.GetDeserializerForComponent(componentId);

            if (deserialize(componentId, 1, componentData, (uint)componentDataLength, &wrapper))
            {
                // now we got a reference to the deserialized component, we can use it to update the component that we already have for the player.
                object storedComponent = entity.Components.First(kvp => kvp.Key == componentId);
                object newComponent = ClientObjects.Instance.Dereference(wrapper->Reference);

                ulong hash = 0;
                MethodInfo? genericGetHash = this.GetType().GetMethods().FirstOrDefault(m => m.Name == nameof(GetHash) && m.IsGenericMethod);

                foreach (IComponentMetaclass componentMetaclass in ComponentDatabase.MetaclassMap.Values)
                {
                    IComponentFactory componentFactory = componentMetaclass as IComponentFactory;
                    if (componentFactory != null && genericGetHash != null &&
                        componentFactory.ComponentId == componentId)
                    {
                        MethodInfo getHash = genericGetHash.MakeGenericMethod(componentFactory.GetType());
                        hash = (ulong)getHash.Invoke(this, new object[] { });
                        break;
                    }
                }
                
                if (_handlers.TryGetValue(hash, out RegisterDelegate handler))
                {
                    handler(player, entityId, newComponent, storedComponent);
                    success = true;
                }

                if (!success)
                {
                    Console.WriteLine("[warning] could not find a handler for component update on " +
                                      componentId);
                }

                ClientObjects.Instance.DestroyReference(wrapper->Reference);
            }
            else
            {
                Console.WriteLine("[error] failed to deserialize ComponentUpdateOp data for id " +
                                  componentId);
            }

            ClientObjects.ObjectFree(componentId, 1, wrapper);

            if (!success)
            {
                Console.WriteLine("[error] if no other error above, no matching component for id " + componentId +
                                  " defined in the game.");
            }

            return success;
        }
    }
}
