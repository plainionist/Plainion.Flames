using Plainion.Flames.Model;
using Plainion.Flames.Modules.ETW.Builders;
using NUnit.Framework;

namespace Plainion.Flames.Modules.ETW.Tests
{
    [TestFixture]
    public class SymbolParsingTests_DotNetStyle
    {
        [TestCase]
        public void CreateMethod_ClassAndMethod_ClassAndMethodSplittedByDot()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "DomainNeutralILStubClass.IL_STUB_PInvoke" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "DomainNeutralILStubClass" ) );
            Assert.That( method.Name, Is.EqualTo( "IL_STUB_PInvoke" ) );
        }

        [TestCase]
        public void CreateMethod_NamespaceClassAndMethod_NamespaceAndClassAndMethodSplittedByDot()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "System.Windows.Threading.Dispatcher.KillWin32Timer" );

            Assert.That( method.Namespace, Is.EqualTo( "System.Windows.Threading" ) );
            Assert.That( method.Class, Is.EqualTo( "Dispatcher" ) );
            Assert.That( method.Name, Is.EqualTo( "KillWin32Timer" ) );
        }

        [TestCase]
        public void CreateMethod_Generics_NamespaceAndClassAndMethodSplittedByDot()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "System.Windows.DescendentsWalker`1[System.Windows.InheritablePropertyChangeInfo].IterateChildren" );

            Assert.That( method.Namespace, Is.EqualTo( "System.Windows" ) );
            Assert.That( method.Class, Is.EqualTo( "DescendentsWalker`1[System.Windows.InheritablePropertyChangeInfo]" ) );
            Assert.That( method.Name, Is.EqualTo( "IterateChildren" ) );
        }

        [TestCase]
        public void CreateMethod_GeneratedMethod_NamespaceAndClassAndMethodSplittedByDot()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "PerfView.MainWindow+<>c__DisplayClass9.<ExecuteCommand>b__7" );

            Assert.That( method.Namespace, Is.EqualTo( "PerfView" ) );
            Assert.That( method.Class, Is.EqualTo( "MainWindow+<>c__DisplayClass9" ) );
            Assert.That( method.Name, Is.EqualTo( "<ExecuteCommand>b__7" ) );
        }
    }
}
