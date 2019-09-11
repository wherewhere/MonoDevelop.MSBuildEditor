// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using MonoDevelop.MSBuild.Language;

namespace MonoDevelop.MSBuild.Editor.Navigation
{
	[Export (typeof (INavigableSymbolSourceProvider))]
	[Name (nameof (MSBuildNavigableSymbolSourceProvider))]
	[ContentType (MSBuildContentType.Name)]
	class MSBuildNavigableSymbolSourceProvider : INavigableSymbolSourceProvider
	{
		[Import]
		internal MSBuildNavigationService NavigationService { get; set; }

		public INavigableSymbolSource TryCreateNavigableSymbolSource (ITextView textView, ITextBuffer buffer) =>
			textView.Properties.GetOrCreateSingletonProperty (
				typeof (MSBuildNavigableSymbolSource),
				() => new MSBuildNavigableSymbolSource (buffer, this)
			);
	}
}
