using NUnit.Framework;
using Phantomity.Constants;
using Phantomity.Utils;

namespace Tests
{
    public class UrlParserTestSuite
    {
        [Test]
        public void DeepLinkWithSchemaMethodAndParams()
        {
            var schema = "example";
            var method = "onConnect";
            var query = "payload=100000";
            var url = $"{schema}://{method}?{query}";
            var urlParser = new UrlParser(url);

            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Scheme), schema);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Method), method);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Domain));
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Params), query);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Anchor));
        }
        
        [Test]
        public void DeepLinkWithSchemaAndMethodWithoutParams()
        {
            var schema = "example";
            var method = "onConnect";
            var url = $"{schema}://{method}";
            var urlParser = new UrlParser(url);

            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Scheme), schema);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Method), method);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Domain));
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Params));
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Anchor));
        }
        
        [Test]
        public void DeepLinkWithPrefix()
        {
            var schema = "example";
            var prefix = "phantom/bridge";
            var method = "onConnect";
            var route = $"{prefix}/{method}";
            var query = "payload=100000";
            var url = $"{schema}://{route}?{query}";
            var urlParser = new UrlParser(url);

            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Scheme), schema);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Domain));
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Method), route);
            Assert.AreEqual(urlParser.GetMethodName(prefix), method);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Params), query);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Anchor));
        }
        
        [Test]
        public void UniversalLinkWithoutPrefix()
        {
            var schema = "https";
            var domain = "example.com";
            var method = "onConnect";
            var query = "payload=100000";
            var url = $"{schema}://{domain}/{method}?{query}";
            var urlParser = new UrlParser(url);

            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Scheme), schema);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Domain), domain);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Method), method);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Params), query);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Anchor));
        }
        
        [Test]
        public void UniversalLinkWithPrefix()
        {
            var schema = "https";
            var domain = "example.com";
            var prefix = "phantom/bridge";
            var method = "onConnect";
            var route = $"{prefix}/{method}";
            var query = "payload=100000";
            var url = $"{schema}://{domain}/{route}?{query}";
            var urlParser = new UrlParser(url);

            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Scheme), schema);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Domain), domain);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Method), route);
            Assert.AreEqual(urlParser.GetMethodName(prefix), method);
            Assert.AreEqual(urlParser.GetUrlPart(UrlRegexVariables.Params), query);
            Assert.IsNull(urlParser.GetUrlPart(UrlRegexVariables.Anchor));
        }
        
    }
}