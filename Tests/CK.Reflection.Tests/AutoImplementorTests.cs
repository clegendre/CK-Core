#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Reflection.Tests\AutoImplementorTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;

namespace CK.Reflection.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "EmitHelper" )]
    public class AutoImplementorTests
    {
        static ModuleBuilder _moduleBuilder;
        static int _typeID;

        public TypeBuilder CreateTypeBuilder( Type baseType )
        {
            if( _moduleBuilder == null )
            {
                AssemblyName assemblyName = new AssemblyName( "TypeImplementorModule" );
                assemblyName.Version = new Version( 1, 0, 0, 0 );
                // REVIEW: RunAndSave does no longer exists. Uses Run. Not sure about it though... 
                // Also uses direct static AssemblyBuilder.DefineDynamicAssembly instead of passing from AppDomain.Current since App.Domain does no longer exists.
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
                _moduleBuilder = assemblyBuilder.DefineDynamicModule( "TypeImplementorModule" );
            }
            return baseType == null 
                    ? _moduleBuilder.DefineType( "No_Base_Type_" + Interlocked.Increment( ref _typeID ).ToString(), TypeAttributes.Class | TypeAttributes.Public )
                    : _moduleBuilder.DefineType( baseType.Name + Interlocked.Increment( ref _typeID ).ToString(), TypeAttributes.Class | TypeAttributes.Public, baseType );
        }

        #region EmitHelper.ImplementEmptyStubMethod tests

        public abstract class A
        {
            public A CallFirstMethod( int i )
            {
                return FirstMethod( i );
            }

            protected abstract A FirstMethod( int i );
        }

        public abstract class B
        {
            public abstract int M( int i );
        }

        public abstract class C
        {
            public abstract short M( int i );
        }

        public abstract class D
        {
            public abstract Guid M( int i );
        }

        public abstract class E
        {
            public abstract byte M( ref int i );
        }

        public abstract class F
        {
            public abstract byte M( out int i );
        }

        public abstract class G
        {
            public abstract byte M( out Guid i );
        }

        public abstract class H
        {
            public abstract byte M( ref Guid i );
        }

        public abstract class I
        {
            public abstract byte M( out CultureAttribute i );
        }

        public abstract class J
        {
            public abstract byte M( ref CultureAttribute i );
        }

        public abstract class K
        {
            public abstract MK<T> M<T>();
        }

        public class MK<T>
        {
        }

        delegate void DynamicWithOutParameters( out Action a, out byte b, ref Guid g, int x );

        [Test]
        public void ImplementOutParameters()
        {
            {
                var dyn = new DynamicMethod( "TestMethod", typeof( void ), new Type[] { typeof( Action ).MakeByRefType(), typeof( byte ).MakeByRefType(), typeof( Guid ).MakeByRefType(), typeof( int ) } );
                var g = dyn.GetILGenerator();

                var parameters = dyn.GetParameters();
                g.StoreDefaultValueForOutParameter( parameters[0] );
                g.StoreDefaultValueForOutParameter( parameters[1] );
                g.StoreDefaultValueForOutParameter( parameters[2] );
                g.Emit( OpCodes.Ret );

                var d = (DynamicWithOutParameters)dyn.CreateDelegate( typeof( DynamicWithOutParameters ) );
                Action a = () => { };
                Byte b = 87;
                Guid guid = Guid.NewGuid();
                d( out a, out b, ref guid, 6554 );

                a.Should().BeNull();
                b.Should().Be( 0 );
                guid.Should().Be( Guid.Empty );
            }
        }

        [Test]
        public void AutoImplementStubReturnsClassAndProtected()
        {
            Type t = typeof( A );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "FirstMethod", BindingFlags.Instance | BindingFlags.NonPublic ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            A o = (A)Activator.CreateInstance( builtType );
            o.CallFirstMethod( 10 ).Should().BeNull();
        }

        [Test]
        public void AutoImplementStubReturnsInt()
        {
            Type t = typeof( B );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            B o = (B)Activator.CreateInstance( builtType );
            o.M( 10 ).Should().Be( 0 );
        }

        [Test]
        public void AutoImplementStubReturnsShort()
        {
            Type t = typeof( C );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            C o = (C)Activator.CreateInstance( builtType );
            o.M( 10 ).Should().Be( 0 );
        }

        [Test]
        public void AutoImplementStubReturnsGuid()
        {
            Type t = typeof( D );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            D o = (D)Activator.CreateInstance( builtType );
            o.M( 10 ).Should().Be( Guid.Empty );
        }

        [Test]
        public void AutoImplementStubRefInt()
        {
            Type t = typeof( E );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            E o = (E)Activator.CreateInstance( builtType );
            int i = 3712;
            o.M( ref i ).Should().Be( 0 );
            i.Should().Be( 3712 );
        }

        [Test]
        public void AutoImplementStubOutInt()
        {
            Type t = typeof( F );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            F o = (F)Activator.CreateInstance( builtType );
            int i = 45;
            o.M( out i ).Should().Be( 0 );
            i.Should().Be( 0 );
        }

        [Test]
        public void AutoImplementStubOutGuid()
        {
            Type t = typeof( G );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            G o = (G)Activator.CreateInstance( builtType );
            Guid i = Guid.NewGuid();
            o.M( out i ).Should().Be( 0 );
            i.Should().Be( Guid.Empty );
        }

        [Test]
        public void AutoImplementStubRefGuid()
        {
            Type t = typeof( H );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            H o = (H)Activator.CreateInstance( builtType );
            Guid iOrigin = Guid.NewGuid();
            Guid i = iOrigin;
            o.M( ref i ).Should().Be( 0 );
            i.Should().Be( iOrigin );
        }

        [Test]
        public void AutoImplementStubOutClass()
        {
            Type t = typeof( I );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            I o = (I)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;
            o.M( out c ).Should().Be( 0 );
            c.Should().BeNull();
        }

        [Test]
        public void AutoImplementStubRefClass()
        {
            Type t = typeof( J );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            J o = (J)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;
            o.M( ref c ).Should().Be( 0 );
            c.Should().BeSameAs( cOrigin );
        }

        [Test]
        public void AutoImplementGenericMethod()
        {
            Type t = typeof( K );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementEmptyStubMethod( b, t.GetMethod( "M" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            K o = (K)Activator.CreateInstance( builtType );
            CultureAttribute cOrigin = new CultureAttribute();
            CultureAttribute c = cOrigin;

            o.M<int>().Should().BeNull();
        }

        #endregion

        #region EmitHelper.ImplementEmptyStubProperty tests
        
        // Note: 
        // Abstract properties cannot have private accessors.
        public abstract class PA
        {
            public abstract int PublicWriteableValue { get; set; }
            public abstract int ProtectedWriteableValue { get; protected set; }

            public int PublicProperty { get; protected set; }

            public void SetProtectedValues( int v )
            {
                ProtectedWriteableValue = v;
                PublicProperty = v%255;
            }
        }

        public abstract class PB : PA
        {
            public new byte PublicProperty { get { return (byte)base.PublicProperty; } }
        }

        [Test]
        public void AutoImplementStubProperty()
        {
            Type tA = typeof( PA );
            TypeBuilder bA = CreateTypeBuilder( tA );
            EmitHelper.ImplementStubProperty( bA, tA.GetProperty( "PublicWriteableValue" ), true );
            EmitHelper.ImplementStubProperty( bA, tA.GetProperty( "ProtectedWriteableValue" ), true );
            Type builtTypeA = bA.CreateTypeInfo().AsType();
            PA oA = (PA)Activator.CreateInstance( builtTypeA );
            oA.PublicWriteableValue = 4548;
            oA.SetProtectedValues( 2121 );
            oA.PublicWriteableValue.Should().Be( 4548 );
            oA.ProtectedWriteableValue.Should().Be(2121);

            Type tB = typeof( PB );
            TypeBuilder bB = CreateTypeBuilder( tB );
            EmitHelper.ImplementStubProperty( bB, tB.GetProperty( "PublicWriteableValue" ), true );
            EmitHelper.ImplementStubProperty( bB, tB.GetProperty( "ProtectedWriteableValue" ), true );
            Type builtTypeB = bB.CreateTypeInfo().AsType();
            PB oB = (PB)Activator.CreateInstance( builtTypeB );
            oB.PublicWriteableValue = 4548;
            oB.SetProtectedValues( 2121 );
            oB.PublicWriteableValue.Should().Be(4548);
            oB.ProtectedWriteableValue.Should().Be(2121);
            oB.PublicProperty.Should().Be(2121 % 255 );
        }

        public abstract class CNonVirtualProperty
        {
            int _value;

            public CNonVirtualProperty()
            {
                _value = 654312;
            }

            public int PublicProperty { get { return _value * 2; } set { _value = value * 2; } }
        }

        [Test]
        public void AutoImplementStubForNonVirtualPropertyIsStupid()
        {
            Type tN = typeof(CNonVirtualProperty);
            TypeBuilder bN = CreateTypeBuilder(tN);
            EmitHelper.ImplementStubProperty(bN, tN.GetProperty("PublicProperty"), true);
            Type builtTypeN = bN.CreateTypeInfo().AsType();
            CNonVirtualProperty oN = (CNonVirtualProperty)Activator.CreateInstance(builtTypeN);
            oN.PublicProperty.Should().Be(654312 * 2);
            oN.PublicProperty = 2;
            oN.PublicProperty.Should().Be(2 * 4);
        }

        public abstract class CVirtualProperty
        {
            int _value;

            public CVirtualProperty()
            {
                _value = 654312;
            }

            public virtual int PublicProperty { get { return _value * 2; } set { _value = value * 2; } }
        }

        [Test]
        public void AutoImplementStubForVirtualPropertyActuallyReplacesIt()
        {
            Type t = typeof( CVirtualProperty );
            TypeBuilder b = CreateTypeBuilder( t );
            EmitHelper.ImplementStubProperty( b, t.GetProperty( "PublicProperty" ), false );
            Type builtType = b.CreateTypeInfo().AsType();
            CVirtualProperty o = (CVirtualProperty)Activator.CreateInstance( builtType );
            o.PublicProperty.Should().Be(0, "Initial value is lost." );
            o.PublicProperty = 2;
            o.PublicProperty.Should().Be(2, "Mere stub implementation does its job." );
        }

        public interface IPocoKind
        {
            int X { get; }
            int Y { get; set; }
        }

        [Test]
        public void AutoImplementStub_can_force_setter_impl()
        {
            TypeBuilder tB = CreateTypeBuilder( null );
            tB.AddInterfaceImplementation( typeof( IPocoKind ) );
            var xB = EmitHelper.ImplementStubProperty( tB, typeof( IPocoKind ).GetProperty( "X" ), false, true );
            var yB = EmitHelper.ImplementStubProperty( tB, typeof( IPocoKind ).GetProperty( "Y" ), false, true );
            //tB.DefineMethodOverride( xB.GetGetMethod(), typeof( IPocoKind ).GetProperty( "X" ).GetGetMethod() );
            //tB.DefineMethodOverride( yB.GetGetMethod(), typeof( IPocoKind ).GetProperty( "Y" ).GetGetMethod() );
            //tB.DefineMethodOverride( yB.GetSetMethod(), typeof( IPocoKind ).GetProperty( "Y" ).GetSetMethod() );
            Type builtType = tB.CreateTypeInfo().AsType();
            IPocoKind o = (IPocoKind)Activator.CreateInstance( builtType );
            o.Y = 8;
            o.Y.Should().Be(8);
            o.X.Should().Be(0);
            builtType.GetProperty( "X" ).SetValue( o, 19 );
            o.X.Should().Be(19);
        }
        #endregion


        public class BaseOne
        {
            public readonly string CtorMessage;

            private BaseOne()
            {
                CtorMessage += ".private";
            }

            protected BaseOne( int i )
                : this()
            {
                CtorMessage += ".protected";
            }
            
            public BaseOne( string s )
                : this()
            {
                CtorMessage += ".public";
            }
        }

        public class BaseTwo
        {
            public readonly string CtorMessage;

            public BaseTwo( params int[] multi )
            {
                CtorMessage = multi.Length.ToString();
            }
        }

        [AttributeUsage( AttributeTargets.Constructor | AttributeTargets.Parameter, AllowMultiple = true )]
        public class CustAttr : Attribute
        {
            public CustAttr()
            {
            }

            public CustAttr( string name )
            {
                Name = name;
            }

            public string Name { get; set; }

            public string FieldName;
        }

        public class BaseThree
        {
            public readonly string CtorMessage;

            [CustAttr( "OnCtorByParam" )]
            [CustAttr( Name = "OnCtorByProperty" )]
            [CustAttr( FieldName = "OnCtorByField" )]
            public BaseThree( [CustAttr( "OnParamByParam" )]string s0, [CustAttr( Name = "OnParamByProperty" )]string s1, [CustAttr( FieldName = "OnParamByField" )]string s2 )
            {
                CtorMessage = s0 + s1 + s2;
            }
        }

        [Test]
        public void PassThroughConstructors()
        {
            {
                TypeBuilder b = CreateTypeBuilder( typeof( BaseOne ) );
                b.DefinePassThroughConstructors( c => c.Attributes | MethodAttributes.Public );
                Type t = b.CreateTypeInfo().AsType();
                BaseOne one1 = (BaseOne)Activator.CreateInstance( t, 5 );
                one1.CtorMessage.Should().Be(".private.protected");
                BaseOne one2 = (BaseOne)Activator.CreateInstance( t, "a string" );
                one2.CtorMessage.Should().Be(".private.public");
            }
            {
                TypeBuilder b = CreateTypeBuilder( typeof( BaseTwo ) );
                b.DefinePassThroughConstructors( c => c.Attributes | MethodAttributes.Public );
                Type t = b.CreateTypeInfo().AsType();
                var ctor = t.GetConstructors()[0];
                BaseTwo two = (BaseTwo)ctor.Invoke( new object[]{ new int[] { 1, 2, 3, 4 } } );
                two.CtorMessage.Should().Be("4");
            }
            {
                TypeBuilder b = CreateTypeBuilder( typeof( BaseThree ) );
                b.DefinePassThroughConstructors( 
                    c => c.Attributes | MethodAttributes.Public, 
                    ( ctor, attrData ) => attrData.NamedArguments.Any(), 
                    ( param, attrData ) => attrData.ConstructorArguments.Any() );
                    
                Type t = b.CreateTypeInfo().AsType();
                var theCtor = t.GetConstructors()[0];
                BaseThree three = (BaseThree)theCtor.Invoke( new object[]{ "s0", "s1", "s2" } );
                three.CtorMessage.Should().Be("s0s1s2");
                // Only attribute defined via Named arguments are defined on the final ctor.
                CollectionAssert.AreEquivalent( new string[] { "OnCtorByProperty", "OnCtorByField" }, theCtor.GetCustomAttributes<CustAttr>().Select( a => a.Name ?? a.FieldName ) );
                // Only the one defined by constructor argument is redefined.
                theCtor.GetParameters()[0].GetCustomAttributes<CustAttr>().Single().Name.Should().Be("OnParamByParam");
                theCtor.GetParameters()[1].GetCustomAttributes<CustAttr>().Should().BeEmpty();
                theCtor.GetParameters()[2].GetCustomAttributes<CustAttr>().Should().BeEmpty();
            }
            {
                TypeBuilder b = CreateTypeBuilder( typeof( BaseThree ) );
                b.DefinePassThroughConstructors( c => c.Attributes | MethodAttributes.Public );
                Type t = b.CreateTypeInfo().AsType();
                var ctor = t.GetConstructors()[0];
                BaseThree three = (BaseThree)ctor.Invoke( new object[]{ "s0", "s1", "s2" } );
                three.CtorMessage.Should().Be("s0s1s2");
                // Everything is defined.
                CollectionAssert.AreEquivalent( new string[] { "OnCtorByParam", "OnCtorByProperty", "OnCtorByField" }, ctor.GetCustomAttributes<CustAttr>().Select( a => a.Name ?? a.FieldName ) );
                ctor.GetParameters()[0].GetCustomAttributes<CustAttr>().Single().Name.Should().Be("OnParamByParam" );
                ctor.GetParameters()[1].GetCustomAttributes<CustAttr>().Single().Name.Should().Be("OnParamByProperty" );
                ctor.GetParameters()[2].GetCustomAttributes<CustAttr>().Single().FieldName.Should().Be("OnParamByField" );
            }

        }
    }
}
