using Plainion.Flames.Model;
using Plainion.Flames.Modules.ETW.Builders;
using NUnit.Framework;

namespace Plainion.Flames.Modules.ETW.Tests
{
    [TestFixture]
    public class SymbolParsingTests_CppStyle
    {
        [TestCase]
        public void CreateMethod_SimpleMethodName_ClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "LRPC_CASSOCIATION::AlpcSendWaitReceivePort" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "LRPC_CASSOCIATION" ) );
            Assert.That( method.Name, Is.EqualTo( "AlpcSendWaitReceivePort" ) );
        }

        [TestCase]
        public void CreateMethod_TemplateClass_ClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "CObjectAccessTracker<IPublicNotification,5000>::CObjectAccessTracker<IPublicNotification,5000>" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "CObjectAccessTracker<IPublicNotification,5000>" ) );
            Assert.That( method.Name, Is.EqualTo( "CObjectAccessTracker<IPublicNotification,5000>" ) );
        }

        [TestCase]
        public void CreateMethod_Destructor_ClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "CStdIdentity::~CStdIdentity" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "CStdIdentity" ) );
            Assert.That( method.Name, Is.EqualTo( "~CStdIdentity" ) );
        }

        [TestCase]
        public void CreateMethod_Scalar_ClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "CStdIdentity::`scalar deleting destructor'" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "CStdIdentity" ) );
            Assert.That( method.Name, Is.EqualTo( "`scalar deleting destructor'" ) );
        }

        [TestCase]
        public void CreateMethod_TemplateWithInnerClass_ClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "TMemBlockBase<RobustIntersections::CLineSegmentIntersection>::Allocate" );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.EqualTo( "TMemBlockBase<RobustIntersections::CLineSegmentIntersection>" ) );
            Assert.That( method.Name, Is.EqualTo( "Allocate" ) );
        }


        [TestCase]
        public void CreateMethod_WithNamespace_NamespaceAndClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "kfapi::CFolderCache::GetIDList" );

            Assert.That( method.Namespace, Is.EqualTo("kfapi") );
            Assert.That( method.Class, Is.EqualTo( "CFolderCache" ) );
            Assert.That( method.Name, Is.EqualTo( "GetIDList" ) );
        }

        [TestCase]
        public void CreateMethod_WithNamespaceAndTemplateClass_NamespaceAndClassAndMethodSplittedByDoubleColon()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", "std::_Tree<std::_Tmap_traits<std::pair<_GUID,unsigned long>,ATL::CComPtr<CTRefObj<kfapi::CFolderIDListInfo,CTRefBase_NoModuleLifetimePolicy> >,kfapi::CComparator<std::pair<_GUID,unsigned long> >,kfapi::throwing_allocator<std::pair<std::pair<_GUID,unsigned long> const ,ATL::CComPtr<CTRefObj<kfapi::CFolderIDListInfo,CTRefBase_NoModuleLifetimePolicy> > > >,0> >::find" );

            Assert.That( method.Namespace, Is.EqualTo( "std" ) );
            Assert.That( method.Class, Is.EqualTo( "_Tree<std::_Tmap_traits<std::pair<_GUID,unsigned long>,ATL::CComPtr<CTRefObj<kfapi::CFolderIDListInfo,CTRefBase_NoModuleLifetimePolicy> >,kfapi::CComparator<std::pair<_GUID,unsigned long> >,kfapi::throwing_allocator<std::pair<std::pair<_GUID,unsigned long> const ,ATL::CComPtr<CTRefObj<kfapi::CFolderIDListInfo,CTRefBase_NoModuleLifetimePolicy> > > >,0> >" ) );
            Assert.That( method.Name, Is.EqualTo( "find" ) );
        }
    }
}
