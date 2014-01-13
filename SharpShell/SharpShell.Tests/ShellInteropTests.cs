﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SharpShell.Interop;

namespace SharpShell.Tests
{
    [TestFixture]
    public class ShellInteropTests
    {
        [Test]
        public void CanGetKnownFolderPath()
        {
            //  We must be able to get the documents known path without throwing an exception.
            string path;
            Shell32.SHGetKnownFolderPath(KnownFolders.FOLDERID_Documents, KNOWN_FOLDER_FLAG.KF_NO_FLAGS, IntPtr.Zero, out path);
            Assert.IsNotNullOrEmpty(path);
        }

        [Test]
        public void CanGetAndFreeKnownFolderIdList()
        {
            IntPtr pidl;
            Shell32.SHGetKnownFolderIDList(KnownFolders.FOLDERID_Cookies, KNOWN_FOLDER_FLAG.KF_NO_FLAGS, IntPtr.Zero, out pidl);
            Assert.IsTrue(pidl != IntPtr.Zero);
            Assert.DoesNotThrow(() => Shell32.ILFree(pidl));
        }

        [Test]
        public void CanGetDesktopFolderLocationAndPath()
        {
            //  Asserts we can get the desktop folder pidl, get a path for it and free the pidl.
            IntPtr pidl;
            Shell32.SHGetFolderLocation(IntPtr.Zero, CSIDL.CSIDL_DESKTOP, IntPtr.Zero, 0, out pidl);
            var sb = new StringBuilder(260);
            Assert.IsTrue(Shell32.SHGetPathFromIDList(pidl, sb));
            Assert.IsNotNullOrEmpty(sb.ToString());
            Assert.DoesNotThrow(() => Shell32.ILFree(pidl));
        }

        [Test]
        public void CanEnumerateDesktopFolders()
        {
            //  Asserts that we can correctly use the IEnumIDList interface.

            //  Get the desktop folder.
            IShellFolder desktopFolder;
            Shell32.SHGetDesktopFolder(out desktopFolder);

            //  Create an enumerator and enumerate up to items.
            IEnumIDList enumerator;
            desktopFolder.EnumObjects(IntPtr.Zero, SHCONTF.SHCONTF_FOLDERS, out enumerator);
            uint fetched;
            var items = new IntPtr[20];
            enumerator.Next((uint)items.Length, items, out fetched);
            items = items.Take((int)fetched).ToArray();

            //  Assert the we can get the display name of each item.
            foreach (var item in items)
            {
                STRRET name;
                desktopFolder.GetDisplayNameOf(item, SHGDNF.SHGDN_NORMAL, out name);
                Assert.IsNotNullOrEmpty(name.GetStringValue());
                Assert.DoesNotThrow(() => Marshal.FreeCoTaskMem(item));
            }
            
        }
    }
}
