﻿using System;
using System.Collections.Generic;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

using N2.Edit.FileSystem.Items;
using N2.Tests;
using N2.Edit.FileSystem;
using N2.Web;
using System.Configuration;

namespace N2.Edit.Tests.FileSystem
{
    [TestFixture]
    public class FileSystemTests : ItemPersistenceMockingBase
    {
        RootDirectory upload;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Engine.IEngine engine = N2.Context.Current;
            Url.DefaultExtension = "/";
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            upload = new RootDirectory();
            upload.Title = "Upload";
            upload.Name = "bin/FileSystem/Upload";
        }

        [Test]
        public void CanListFiles_InRootDirectory()
        {
            IList<File> files = upload.GetFiles();
            Assert.That(files.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanListDirectories_InRootDirectory()
        {
            IList<Directory> directories = upload.GetDirectories();
            Assert.That(directories.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanListDirectories_InSubDirectory()
        {
            IList<Directory> directories = upload.GetDirectories();
            IList<Directory> subDirectories = directories[0].GetDirectories();
            Assert.That(subDirectories.Count, Is.EqualTo(1));
            Assert.That(subDirectories[0].Name, Is.EqualTo("Folder 3"));
        }

        [Test]
        public void CanListFiles_InSubDirectory()
        {
            IList<Directory> directories = upload.GetDirectories();
            IList<File> files = directories[0].GetFiles();
            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files[0].Name, Is.EqualTo("File 2.txt"));
        }

        [Test]
        public void CanGetDirectory_ByName()
        {
            Directory d = (Directory)upload.GetChild("Folder1");
            Assert.That(d, Is.Not.Null);
            Assert.That(d.Name, Is.EqualTo("Folder1"));
        }

        [Test]
        public void CanGetFile_ByName()
        {
            File f = (File)upload.GetChild("File.txt");
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Name, Is.EqualTo("File.txt"));
        }

        [Test]
        public void CanGet_FileLength()
        {
            File f = (File)upload.GetChild("File.txt");
            Assert.That(f.Size, Is.EqualTo(13));
        }

        [Test]
        public void CanGetDirectory_ByPath()
        {
            Directory d = (Directory)upload.GetChild("Folder 2/Folder 3");
            Assert.That(d, Is.Not.Null);
            Assert.That(d.Name, Is.EqualTo("Folder 3"));
        }

        [Test]
        public void CanGetFile_ByPath()
        {
            File f = (File)upload.GetChild("Folder 2/Folder 3/File 3.txt");
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Name, Is.EqualTo("File 3.txt"));
        }

        [Test]
        public void CanMoveFile_ToOtherDirectory()
        {
            File f = (File)upload.GetChild("Folder 2/Folder 3/File 3.txt");
            string sourcePath = f.PhysicalPath;
            string destinationPath = N2.Context.Current.Resolve<IWebContext>().MapPath("~/bin/FileSystem/Upload/Folder1/File 3.txt");
            Directory d = (Directory)upload.GetChild("Folder1");
            try
            {
                f.AddTo(d);
                Assert.That(f.PhysicalPath, Is.EqualTo(destinationPath));
                Assert.That(System.IO.File.Exists(destinationPath));
                Assert.That(!System.IO.File.Exists(sourcePath));
            }
            finally
            {
                System.IO.File.Move(destinationPath, sourcePath);
            }
        }

        [Test]
        public void CanMoveFile_ToRootDirectory()
        {
            File f = (File)upload.GetChild("Folder 2/File 2.txt");
            string sourcePath = f.PhysicalPath;
            string destinationPath = N2.Context.Current.Resolve<IWebContext>().MapPath("~/bin/FileSystem/Upload/File 2.txt");
            try
            {
                f.AddTo(upload);
                Assert.That(f.PhysicalPath, Is.EqualTo(destinationPath));
                Assert.That(System.IO.File.Exists(destinationPath));
                Assert.That(!System.IO.File.Exists(sourcePath));
            }
            finally
            {
                System.IO.File.Move(destinationPath, sourcePath);
            }
        }

        [Test]
        public void CanMoveDirectory_ToOtherDirectory()
        {
            Directory d = (Directory)upload.GetChild("Folder 2/Folder 3");
            string sourcePath = d.PhysicalPath;
            string destinationPath = N2.Context.Current.Resolve<IWebContext>().MapPath("~/bin/FileSystem/Upload/Folder1/Folder 3");
            try
            {
                d.AddTo(upload.GetChild("Folder1"));
                Assert.That(d.PhysicalPath, Is.EqualTo(destinationPath));
                Assert.That(System.IO.Directory.Exists(destinationPath));
                Assert.That(!System.IO.Directory.Exists(sourcePath));
            }
            finally
            {
                System.IO.Directory.Move(destinationPath, sourcePath);
            }
        }

        [Test]
        public void CanMoveDirectory_ToRootDirectory()
        {
            Directory d = (Directory)upload.GetChild("Folder 2/Folder 3");
            string sourcePath = d.PhysicalPath;
            string destinationPath = N2.Context.Current.Resolve<IWebContext>().MapPath("~/bin/FileSystem/Upload/Folder 3");
            try
            {
                d.AddTo(upload);
                Assert.That(d.PhysicalPath, Is.EqualTo(destinationPath));
                Assert.That(System.IO.Directory.Exists(destinationPath));
                Assert.That(!System.IO.Directory.Exists(sourcePath));
            }
            finally
            {
                System.IO.Directory.Move(destinationPath, sourcePath);
            }
        }

        [Test]
        public void CanMove_RootDirectory_ToContentItem()
        {
            var trash = new N2.Edit.Trash.TrashContainerItem();
            upload.AddTo(trash);

            Assert.That(upload.Parent, Is.EqualTo(trash));
            Assert.That(trash.Children.Count, Is.EqualTo(1));
        }
    }
}