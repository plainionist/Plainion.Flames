using Plainion.Flames.Model;
using Plainion.Flames.Modules.ETW.Builders;
using NUnit.Framework;

namespace Plainion.Flames.Modules.ETW.Tests
{
    /*
c++
LRPC_CASSOCIATION::AlpcSendWaitReceivePort
CObjectAccessTracker<IPublicNotification,5000>::CObjectAccessTracker<IPublicNotification,5000>
CStdIdentity::~CStdIdentity
CStdIdentity::`scalar deleting destructor'
TMemBlockBase<RobustIntersections::CLineSegmentIntersection>::Allocate
DynArrayImpl<1>::~DynArrayImpl<1>
CResource<ID3D11Resource>::CopySubresourceRegion<1,0>


c#
DomainNeutralILStubClass.IL_STUB_PInvoke
System.Windows.Threading.Dispatcher.KillWin32Timer
System.Windows.DescendentsWalker`1[System.Windows.InheritablePropertyChangeInfo].IterateChildren
System.Windows.Media.Animation.WeakRefEnumerator`1[System.__Canon].MoveNext
PerfView.MainWindow+<>c__DisplayClass9.<ExecuteCommand>b__7
System.Windows.Baml2006.WpfSharedBamlSchemaContext.<Create_BamlType_TextBox>b__5db
     
     */

    [TestFixture]
    public class SymbolParsingTests_CStyle
    {
        [TestCase]
        public void CreateMethod_SimpleMethodName_MethodNameEqualsFullMethodName()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "b(char* x)" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.Null );
            Assert.That( method.Name, Is.EqualTo( "b" ) );
        }

        [TestCase]
        public void CreateMethod_MethodStartingWithUnderscores_MethodNameEqualsFullMethodName()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "__fnDWORD" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.Null );
            Assert.That( method.Name, Is.EqualTo( "__fnDWORD" ) );
        }
    }
}
