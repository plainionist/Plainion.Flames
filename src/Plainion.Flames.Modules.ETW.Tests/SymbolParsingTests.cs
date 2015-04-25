using Plainion.Flames.Modules.ETW.Builders;
using NUnit.Framework;

namespace Plainion.Flames.Modules.ETW.Tests
{
    [TestFixture]
    public class SymbolParsingTests
    {
        [TestCase]
        public void CreateMethod_FullMethodNameIsNull_MethodNameIsQuestionMark()
        {
            var builder = new CallstackBuilder( new TraceModelBuilder() );
            var method = builder.CreateMethod( "a", null );

            Assert.That( method.Namespace, Is.Null );
            Assert.That( method.Class, Is.Null );
            Assert.That( method.Name, Is.EqualTo( "?" ) );
        }
    }
}
