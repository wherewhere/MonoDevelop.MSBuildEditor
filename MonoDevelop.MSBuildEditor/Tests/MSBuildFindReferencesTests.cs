﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.MSBuildEditor.Language;
using MonoDevelop.Xml.Parser;
using NUnit.Framework;

namespace MonoDevelop.MSBuildEditor.Tests
{
	[TestFixture]
	public class MSBuildFindReferencesTests : IdeTestBase
	{
		List<(int Offset, int Length)> FindReferences (string doc, MSBuildReferenceKind kind, string name, string parentName)
		{
			string filename = "test.csproj";

			var textDoc = TextEditorFactory.CreateNewDocument (new StringTextSource (doc), filename, MSBuildTextEditorExtension.MSBuildMimeType);

			var xmlParser = new XmlParser (new XmlRootState (), true);
			xmlParser.Parse (new StringReader (doc));
			var xdoc = xmlParser.Nodes.GetRoot ();


			var ctx = MSBuildTestHelpers.CreateEmptyContext ();
			var sb = new MSBuildSchemaBuilder (true, null, new PropertyValueCollector (false), null);
			sb.Run (xdoc, filename, textDoc, ctx);

			var collector = MSBuildReferenceCollector.Create (new MSBuildResolveResult {
				ReferenceKind = kind,
				ReferenceName = name,
				ReferenceItemName = parentName
			});
			collector.Run (xdoc, filename, textDoc, ctx);
			return collector.Results;
		}

		void AssertLocations (string doc, string expectedName, List<(int Offset, int Length)> actual, params (int Offset, int Length)[] expected)
		{
			if (actual.Count != expected.Length) {
				DumpLocations ();
				Assert.Fail ($"List has {actual.Count} items, expected {expected.Length}");
			}

			for (int i = 0; i < actual.Count; i++) {
				var (offset, length) = actual [i];
				var (expecteOffset, expectedLength) = expected [i];
				if (offset != expecteOffset || length != expectedLength || !string.Equals (expectedName, doc.Substring (offset, length), StringComparison.OrdinalIgnoreCase)) {
					DumpLocations ();
					Assert.Fail ($"Position {i}: expected ({expecteOffset}, {expectedLength})='{expectedName}', got ({offset}, {length})='{doc.Substring (offset, length)}'");
				}
			}

			void DumpLocations ()
			{
				Console.WriteLine ("Locations: ");
				foreach (var (offset, length) in actual) {
					Console.WriteLine ($"    ({offset}, {length})='{doc.Substring (offset, length)}'");
				}
			}
		}

		[Test]
		public void FindItemReferences ()
		{
			var doc = @"
<project>
  <itemgroup>
    <foo />
    <bar include='@(foo)' condition=""'@(Foo)'!='$(Foo)'"" somemetadata=""@(Foo)"" foo='a' />
    <!-- check that the metadata doesn't get flagged -->
    <bar><foo>@(foo)</foo></bar>
    <baz include=""@(foo->'%(foo.bar)')"" />
  </itemgroup>
</project>".TrimStart ();

			var refs = FindReferences (doc, MSBuildReferenceKind.Item, "Foo", null);

			AssertLocations (
				doc, "foo", refs,
				(29, 3),
				(56, 3),
				(76, 3),
				(109, 3),
				(199, 3),
				(236, 3),
				(244, 3)
			);
		}

		[Test]
		public void FindPropertyReferences ()
		{
			var doc = @"
<project>
  <propertygroup>
    <foo condition=""'x$(Foo)'==''"">bar $(foo)</foo>
  </propertygroup>
  <target name='Foo' DependsOnTargets='$(Foo)'>
    <itemgroup>
      <foo />
      <bar include='$(foo)' condition=""'@(Foo)'!='$(Foo)'"" somemetadata=""$(Foo)"" foo='a' />
      <bar><foo>$(foo)</foo></bar>
      <foo include=""@(bar->'%(baz.foo)$(foo)')"" />
    </itemgroup>
  </target>
</project>".TrimStart ();

			var refs = FindReferences (doc, MSBuildReferenceKind.Property, "Foo", null);

			AssertLocations (
				doc, "foo", refs,
				(33, 3),
				(52, 3),
				(69, 3),
				(140, 3),
				(199, 3),
				(229, 3),
				(252, 3),
				(287, 3),
				(344, 3)
			);
		}

		[Test]
		public void FindMetadataReferences ()
		{
			var doc = @"
<project>
  <itemgroup>
    <bar foo=""$(foo)"" />
    <bar>
        <foo>baz</foo>
    </bar>
    <foo>
        <foo>baz</foo>
    </foo>
    <foo include=""@(bar->'%(foo)')"" foo='a' />
    <foo include=""@(bar->'%(bar.foo)')"" />
    <foo include=""@(bar->'%(baz.foo)')"" />
    <bar><foo>@(foo)</foo></bar>
  </itemgroup>
  <target name='Foo' DependsOnTargets=""@(bar->'%(Foo)')"">
  </target>
</project>".TrimStart ();

			var refs = FindReferences (doc, MSBuildReferenceKind.Metadata, "foo", "bar");

			AssertLocations (
				doc, "foo", refs,
				(33, 3),
				(68, 3),
				(165, 3),
				(216, 3),
				(280, 3),
				(367, 3)
			);
		}
	}
}