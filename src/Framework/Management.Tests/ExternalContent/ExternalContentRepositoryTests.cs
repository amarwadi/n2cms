﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using N2.Tests;
using N2.Tests.Fakes;
using N2.Persistence.Finder;
using N2.Web;
using Rhino.Mocks;
using N2.Persistence;
using N2.Details;
using N2.Management.Externals;

namespace N2.Management.Tests.ExternalContent
{
	[TestFixture]
	public class ExternalContentRepositoryTests
	{
		Externals.ExternalContentRepository externalRepository;
		ContentItem root;
		ContentItem start;

		[SetUp]
		public void SetUp()
		{
			externalRepository = SetupRepository(out root, out start);
		}

		public static Externals.ExternalContentRepository SetupRepository(out ContentItem root, out ContentItem start)
		{
			FakeRepository<ContentItem> itemRepository;
			FakeRepository<ContentDetail> linkRepository;
			IItemFinder finder;
			var persister = TestSupport.SetupFakePersister(out itemRepository, out linkRepository, out finder);
			var activator = new Persistence.ContentActivator(new Edit.Workflow.StateChanger(), MockRepository.GenerateStub<IItemNotifier>(), new Persistence.Proxying.EmptyProxyFactory());
			itemRepository.Save(root = new ExternalItem { ID = 1, Name = "root" });
			itemRepository.Save(start = new ExternalItem { ID = 2, Name = "start" });

			return new Externals.ExternalContentRepository(new Edit.ContainerRepository<Externals.ExternalItem>(persister, finder, new Host(new ThreadContext(), 1, 2), activator) { Navigate = true }, persister, new Configuration.EditSection());
		}

		[Test]
		public void Container_IsCreated()
		{
			externalRepository.GetOrCreate("Family", "Key", "/hello/world");

			var container = start.Children.Single();
			Assert.That(container, Is.InstanceOf<ExternalItem>());
			Assert.That(container.Name, Is.EqualTo(ExternalItem.ExternalContainerName));
			Assert.That(container.IsPage, Is.False);
		}

		[Test]
		public void FamilyContainer_IsCreated()
		{
			externalRepository.GetOrCreate("Family", "Key", "/hello/world");

			var container = start.Children.Single();
			var family = container.Children.Single();
			Assert.That(family, Is.InstanceOf<ExternalItem>());
			Assert.That(family.Name, Is.EqualTo("Family"));
			Assert.That(family.ZoneName, Is.EqualTo(ExternalItem.ExternalContainerName));
			Assert.That(family.IsPage, Is.False);
		}

		[Test]
		public void FamilyContainerKey_MayDifferInCasing()
		{
			var item1 = externalRepository.GetOrCreate("Family", "Key", "/hello/world");
			var item2 = externalRepository.GetOrCreate("family", "Key", "/hello/world");
			
			Assert.That(item2, Is.SameAs(item1));
		}

		[Test]
		public void ExternalItem_IsCreated()
		{
			externalRepository.GetOrCreate("Family", "Key", "/hello/world");

			var container = start.Children.Single();
			var family = container.Children.Single();
			var item = family.Children.Single();
			Assert.That(item, Is.InstanceOf<ExternalItem>());
			Assert.That(item.Name, Is.EqualTo("Key"));
			Assert.That(item.ZoneName, Is.EqualTo("Family"));
			Assert.That(item.IsPage, Is.True);
			Assert.That(item.Url, Is.EqualTo("/hello/world"));
		}

		[Test]
		public void ExternalItemKey_MayDifferInCasing()
		{
			var item1 = externalRepository.GetOrCreate("Family", "Key", "/hello/world");
			var item2 = externalRepository.GetOrCreate("Family", "key", "/hello/world");

			Assert.That(item2, Is.SameAs(item1));
		}

		[Test]
		public void ExternalItem_MayHaveEmptyKey()
		{
			var item1 = externalRepository.GetOrCreate("Family", "", "/hello/world");
			var item2 = externalRepository.GetOrCreate("Family", "", "/hello/world");

			Assert.That(item2, Is.SameAs(item1));
		}

		[Test]
		public void ExternalItem_MayHaveNullKey()
		{
			var item1 = externalRepository.GetOrCreate("Family", null, "/hello/world");
			var item2 = externalRepository.GetOrCreate("Family", null, "/hello/world");

			Assert.That(item2, Is.SameAs(item1));
		}
	}
}
