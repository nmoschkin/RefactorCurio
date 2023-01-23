using DataTools.Code.Markers;
using DataTools.Code.Project;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataTools.Code.Emit
{
    /// <summary>
    /// Abstract base class for factory classes that can provide <see cref="IEmitter"/> implementations.
    /// </summary>
    internal abstract class EmitterProvider : IEmitterProvider
    {
        private ISolutionElement context;

        /// <summary>
        /// Create instances for all emitter providers in the specified assembly.
        /// </summary>
        /// <param name="context">The solution context for the provider instances.</param>
        /// <param name="assembly">Optional alternate assembly (taken from <paramref name="context"/>, otherwise.)</param>
        /// <param name="kinds">Optional list of marker kinds that must be supported.</param>
        /// <returns>A list of valid providers.</returns>
        /// <remarks>
        /// If this method is called with a list of marker kinds, then the returned <see cref="IEmitterProvider"/> instances<br />
        /// have indicated that they support <b>at least one</b> of the specified kinds.
        /// </remarks>
        public static IList<IEmitterProvider> GetProviders(ISolutionElement context, MarkerKind[] kinds = null, Assembly assembly = null)
        {
            var l = new List<IEmitterProvider>();

            if (assembly == null) assembly = context.GetType().Assembly;

            var types = assembly.GetTypes().Where(x => x.GetInterfaces().Any(y => y == typeof(IEmitterProvider))).ToList();

            if (types != null && types.Count > 0)
            {
                foreach (var t in types)
                {
                    IEmitterProvider obj = null;

                    try
                    {
                        obj = (IEmitterProvider)Activator.CreateInstance(t, new object[] { context });
                    }
                    catch { }

                    if (obj == null)
                    {
                        try
                        {
                            obj = (IEmitterProvider)Activator.CreateInstance(t);
                        }
                        catch { }
                    }

                    if (obj != null)
                    {
                        bool b = true;
                        if (kinds != null)
                        {
                            b = false;
                            foreach (var kind in kinds)
                            {
                                if (obj.CanProvideEmitter(kind))
                                {
                                    b = true;
                                    break;
                                }
                            }
                        }

                        if (b) l.Add(obj);
                    }
                }
            }

            return l;
        }

        /// <summary>
        /// Gets a provider for the specified <see cref="ISolutionElement"/> context that will handle the specified <see cref="MarkerKind"/>
        /// </summary>
        /// <param name="context">The solution context for the provider instances.</param>
        /// <param name="kind">The marker kind that must be supported.</param>
        /// <param name="assembly">Optional alternate assembly (taken from <paramref name="context"/>, otherwise.)</param>
        /// <returns></returns>
        public static IEmitterProvider GetProvider(ISolutionElement context, MarkerKind kind, Assembly assembly = null)
        {
            return GetProviders(context, new[] { kind }, assembly)?.FirstOrDefault();
        }

        public ISolutionElement Context => context;

        public EmitterProvider(ISolutionElement context)
        {
            this.context = context;
        }

        public abstract bool CanProvideEmitter(MarkerKind kind);

        public abstract IEmitter ProvideEmitter(IMarker marker);
    }

    /// <summary>
    /// Abstract base class for factory classes that can provide <see cref="IEmitter{TMarker}"/> implementations.
    /// </summary>
    internal abstract class EmitterProvider<TMarker> : EmitterProvider, IEmitterProvider<TMarker>
        where TMarker : IMarker, new()
    {
        /// <summary>
        /// Create instances for all emitter providers in the specified assembly.
        /// </summary>
        /// <param name="context">The solution context for the provider instances.</param>
        /// <param name="assembly">Optional alternate assembly (taken from <paramref name="context"/>, otherwise.)</param>
        /// <param name="kinds">Optional list of marker kinds that must be supported.</param>
        /// <returns>A list of valid providers.</returns>
        /// <remarks>
        /// If this method is called with a list of marker kinds, then the returned <see cref="IEmitterProvider"/> instances<br />
        /// have indicated that they support <b>at least one</b> of the specified kinds.
        /// </remarks>
        public static IList<IEmitterProvider<T>> GetProviders<T>(ISolutionElement context, MarkerKind[] kinds = null, Assembly assembly = null)
            where T : IMarker, new()
        {
            var l = new List<IEmitterProvider<T>>();

            if (assembly == null) assembly = context.GetType().Assembly;

            var types = assembly.GetTypes().Where(x => x.GetInterfaces().Any(y => y == typeof(IEmitterProvider<T>))).ToList();

            if (types != null && types.Count > 0)
            {
                foreach (var t in types)
                {
                    IEmitterProvider<T> obj = null;

                    try
                    {
                        obj = (IEmitterProvider<T>)Activator.CreateInstance(t, new object[] { context });
                    }
                    catch { }

                    if (obj == null)
                    {
                        try
                        {
                            obj = (IEmitterProvider<T>)Activator.CreateInstance(t);
                        }
                        catch { }
                    }

                    if (obj != null)
                    {
                        bool b = true;
                        if (kinds != null)
                        {
                            b = false;
                            foreach (var kind in kinds)
                            {
                                if (obj.CanProvideEmitter(kind))
                                {
                                    b = true;
                                    break;
                                }
                            }
                        }

                        if (b) l.Add(obj);
                    }
                }
            }

            return l;
        }

        /// <summary>
        /// Gets a provider for the specified <see cref="ISolutionElement"/> context that will handle the specified <see cref="MarkerKind"/>
        /// </summary>
        /// <param name="context">The solution context for the provider instances.</param>
        /// <param name="kind">The marker kind that must be supported.</param>
        /// <param name="assembly">Optional alternate assembly (taken from <paramref name="context"/>, otherwise.)</param>
        /// <returns></returns>
        public static IEmitterProvider<T> GetProvider<T>(ISolutionElement context, MarkerKind kind, Assembly assembly = null)
            where T : IMarker, new()
        {
            return GetProviders<T>(context, new[] { kind }, assembly)?.FirstOrDefault();
        }

        public EmitterProvider(ISolutionElement context) : base(context)
        {
        }

        public override sealed IEmitter ProvideEmitter(IMarker marker)
        {
            return ProvideEmitter((TMarker)marker);
        }

        public abstract IEmitter<TMarker> ProvideEmitter(TMarker marker);
    }
}