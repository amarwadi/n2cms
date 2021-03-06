using System.Web;
using N2.Web;

namespace N2.Tests.Fakes
{
	public class FakeHttpContext : HttpContextBase
	{
		public FakeHttpContext()
		{
			request = new FakeHttpRequest();
			response = new FakeHttpResponse();
			server = new FakeHttpServerUtility();
			session = new FakeHttpSessionState();
		}
		public FakeHttpContext(Url url)
			: this()
		{
			request.appRelativeCurrentExecutionFilePath = "~" + url.Path;
			foreach (var q in url.GetQueries())
				request.query[q.Key] = q.Value;
			request.rawUrl = url.PathAndQuery;
		}
		public FakeHttpRequest request;
		public override HttpRequestBase Request
		{
			get { return request; }
		}
		public FakeHttpResponse response;
		public override HttpResponseBase Response
		{
			get { return response; }
		}
		public FakeHttpServerUtility server;
		public override HttpServerUtilityBase Server
		{
			get { return server; }
		}

		public FakeHttpSessionState session;
		public override HttpSessionStateBase Session
		{
			get { return session; }
		}
	}
}