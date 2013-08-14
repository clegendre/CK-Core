﻿using System;
using System.Linq;
using System.Collections.Generic;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Factory for actual actions from <see cref="ActionConfiguration"/> objects that enables
    /// the <see cref="ConfiguredRouteHost"/> to create new actions and new final routes whenever its configuration changed.
    /// </summary>
    /// <typeparam name="TAction">Actual type of the actions. The only constraint is that it must be a reference type.</typeparam>
    /// <typeparam name="TRoute">Route class that encapsulates actions.</typeparam>
    public abstract class RouteActionFactory<TAction, TRoute>
        where TAction : class
        where TRoute : class
    {
        readonly Dictionary<ActionConfiguration, TAction> _cache;

        /// <summary>
        /// Initializes a new factory.
        /// </summary>
        protected RouteActionFactory()
        {
            _cache = new Dictionary<ActionConfiguration, TAction>();
        }

        internal void Initialize()
        {
            _cache.Clear();
            DoInitialize();
        }

        internal TAction[] GetAllActionsAndUnitialize( bool success )
        {
            TAction[] all = _cache.Values.ToArray();
            _cache.Clear();
            DoUninitialize( success );
            return all;
        }

        internal TAction Create( IActivityMonitor monitor, ActionConfiguration c )
        {
            TAction a;
            if( !_cache.TryGetValue( c, out a ) )
            {
                if( c is Impl.ActionCompositeConfiguration )
                {
                    var seq = c as ActionSequenceConfiguration;
                    if( seq != null )
                    {
                        a = DoCreateSequence( monitor, seq, Create( monitor, seq.Children ) );
                    }
                    else
                    {
                        var par = c as ActionParallelConfiguration;
                        if( par != null )
                        {
                            a = DoCreateParallel( monitor, par, Create( monitor, seq.Children ) );
                        }
                        else throw new InvalidOperationException( "Only Sequence or Parallel composites are supported." );
                    }
                }
                else a = DoCreate( monitor, c );
                _cache.Add( c, a );
            }
            return a;
        }

        internal TAction[] Create( IActivityMonitor monitor, IReadOnlyList<ActionConfiguration> c )
        {
            TAction[] result = new TAction[c.Count];
            for( int i = 0; i < result.Length; ++i ) result[i] = Create( monitor, c[i] );
            return result;
        }

        /// <summary>
        /// Must be implemented to return an empty final route.
        /// This empty final route is used when no configuration exists or if an error occured while 
        /// setting a new configuration.
        /// </summary>
        /// <returns>An empty route. Can be a static shared (immutable) object.</returns>
        internal protected abstract TRoute DoCreateEmptyFinalRoute();

        /// <summary>
        /// Must be implemented to initialize any required shared objects for building new actions and routes.
        /// This is called once prior to any call to other methods of this factory.
        /// </summary>
        protected abstract void DoInitialize();

        /// <summary>
        /// Must be implemented to create a <typeparamref name="TAction"/> from a <see cref="ActionConfiguration"/> object
        /// that is guaranteed to not be a composite (a parallel or a sequence).
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="c">Configuration of the action.</param>
        /// <returns>The created action.</returns>
        protected abstract TAction DoCreate( IActivityMonitor monitor, ActionConfiguration c );

        /// <summary>
        /// Must me implemented to create a parallel action.
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="c">Configuration of the parallel action.</param>
        /// <param name="children">Array of already created children action.</param>
        /// <returns>A parallel action.</returns>
        protected abstract TAction DoCreateParallel( IActivityMonitor monitor, ActionParallelConfiguration c, TAction[] children );

        /// <summary>
        /// Must me implemented to create a sequence action.
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="c">Configuration of the sequence action.</param>
        /// <param name="children">Array of already created children action.</param>
        /// <returns>A sequence action.</returns>
        protected abstract TAction DoCreateSequence( IActivityMonitor monitor, ActionSequenceConfiguration c, TAction[] children );

        /// <summary>
        /// Must be implemented to create the final route class that encapsulates the array of actions of a route. 
        /// </summary>
        /// <param name="monitor">Monitor to use if needed.</param>
        /// <param name="actions">Array of actions for the route.</param>
        /// <param name="configurationName"><see cref="RouteConfiguration"/> name.</param>
        /// <returns>Final route actions encapsulation.</returns>
        internal protected abstract TRoute DoCreateFinalRoute( IActivityMonitor monitor, TAction[] actions, string configurationName );

        /// <summary>
        /// Must be implemented to cleanup any resources (if any) once new actions and routes have been created.
        /// This is always called (even if an error occcured). 
        /// </summary>
        /// <param name="success">True on success, false if creation of routes failed.</param>
        protected abstract void DoUninitialize( bool success );

    }

}
