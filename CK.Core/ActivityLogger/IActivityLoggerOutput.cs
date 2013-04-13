#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IActivityLoggerOutput.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// An <see cref="IActivityLoggerClientRegistrar"/> exposes an <see cref="ExternalInput"/> 
    /// (an <see cref="IActivityLoggerClient"/>) that can be registered as a client for any number of other loggers.
    /// </summary>
    public interface IActivityLoggerOutput : IActivityLoggerClientRegistrar
    {
        /// <summary>
        /// Gets an entry point for other loggers: by registering this <see cref="IActivityLoggerClient"/> in other <see cref="IActivityLogger.Output"/>,
        /// log streams can easily be merged.
        /// </summary>
        IActivityLoggerClient ExternalInput { get; }

        /// <summary>
        /// Gets a modifiable list of <see cref="IActivityLoggerClient"/> that can not be removed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This list simply guaranty that an <see cref="InvalidOperationException"/> will be thrown 
        /// if a call to <see cref="IActivityLoggerClientRegistrar.UnregisterClient"/> is 
        /// done on a non removeable client.
        /// </para>
        /// </remarks>
        IList<IActivityLoggerClient> NonRemoveableClients { get; }
    }

}
