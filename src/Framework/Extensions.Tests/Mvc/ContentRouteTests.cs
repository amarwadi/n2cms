﻿using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using N2.Configuration;
using N2.Definitions;
using N2.Edit;
using N2.Engine;
using N2.Extensions.Tests.Fakes;
using N2.Extensions.Tests.Mvc.Controllers;
using N2.Extensions.Tests.Mvc.Models;
using N2.Tests;
using N2.Tests.Fakes;
using N2.Web;
using N2.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using HtmlHelper = System.Web.Mvc.HtmlHelper;
using N2.Persistence.Proxying;
using N2.Persistence;
using N2.Definitions.Static;
using N2.Edit.Workflow;

namespace N2.Extensions.Tests.Mvc
{
	public abstract class ContentRouteTests : ItemPersistenceMockingBase
	{
		// /					-> RegularPage
		// /about/				-> AboutUsSectionPage
		// /about/executives/	-> ExecutiveTeamPage
		// /search/				-> SearchPage

		#region SetUp

		protected IEngine engine;
		protected FakeHttpContext httpContext;
		protected RequestContext requestContext;
		protected RouteCollection routes;
		protected IControllerMapper controllerMapper;

		protected RegularPage root;
		protected AboutUsSectionPage about;
		protected ExecutiveTeamPage executives;
		protected SearchPage search;
		protected ContentRoute route;
		protected UrlHelper urlHelper;
		protected HtmlHelper htmlHelper;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			root = CreateOneItem<RegularPage>(1, "root", null);
			about = CreateOneItem<AboutUsSectionPage>(2, "about", root);
			executives = CreateOneItem<ExecutiveTeamPage>(3, "executives", about);
			search = CreateOneItem<SearchPage>(4, "search", root);

			var typeFinder = new FakeTypeFinder2();
			typeFinder.typeMap[typeof(ContentItem)] = this.NearbyTypes()
				.BelowNamespace("N2.Extensions.Tests.Mvc.Models").AssignableTo<ContentItem>().Union(typeof(ContentItem)).ToArray();
			typeFinder.typeMap[typeof(IController)] = this.NearbyTypes()
				.BelowNamespace("N2.Extensions.Tests.Mvc.Controllers").AssignableTo<IController>().Except(typeof(AnotherRegularController))
				.ToArray();

			var changer = new StateChanger();
			var definitions = new DefinitionManager(new[] { new DefinitionProvider(new DefinitionBuilder(new DefinitionMap(), typeFinder, new EngineSection())) }, new ITemplateProvider[0], new ContentActivator(changer, null, new EmptyProxyFactory()), changer);
			var webContext = new ThreadContext();
			var host = new Host(webContext, root.ID, root.ID);
			var parser = new UrlParser(persister, webContext, host, new HostSection());
			controllerMapper = new ControllerMapper(typeFinder, definitions);
			Url.DefaultExtension = "";

			engine = mocks.DynamicMock<IEngine>();
			SetupResult.For(engine.Resolve<ITypeFinder>()).Return(typeFinder);
			SetupResult.For(engine.Definitions).Return(definitions);
			SetupResult.For(engine.UrlParser).Return(parser);
			SetupResult.For(engine.Persister).Return(persister);
			var editUrlManager = new FakeEditUrlManager();
			SetupResult.For(engine.ManagementPaths).Return(editUrlManager);
			engine.Replay();

			route = new ContentRoute(engine, new MvcRouteHandler(), controllerMapper, null);

			httpContext = new FakeHttpContext();
			routes = new RouteCollection { route };
		}

		#endregion

		#region RequestingUrl
		protected RouteData RequestingUrl(Url url)
		{
			httpContext.request.appRelativeCurrentExecutionFilePath = "~" + url.Path;
			httpContext.request.rawUrl = url;

			NameValueCollection nvc = new NameValueCollection();
			foreach (var kvp in url.GetQueries())
				nvc[kvp.Key] = kvp.Value;
			httpContext.request.query = nvc;

			foreach (var kvp in url.GetQueries())
				httpContext.request.query[kvp.Key] = kvp.Value;

			var data = route.GetRouteData(httpContext);
			requestContext = new RequestContext(httpContext, data);
			urlHelper = new UrlHelper(requestContext, routes);
			htmlHelper = new HtmlHelper(new ViewContext { RequestContext = requestContext, HttpContext = httpContext, RouteData = requestContext.RouteData, Writer = httpContext.Response.Output }, new ViewPage(), routes);
			return data;
		}
		#endregion
	}
}
