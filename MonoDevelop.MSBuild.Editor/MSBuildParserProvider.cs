// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

using Microsoft.VisualStudio.Text;

using MonoDevelop.MSBuild.Language;
using MonoDevelop.MSBuild.Schema;

namespace MonoDevelop.MSBuild.Editor.Completion
{
	[Export]
	class MSBuildParserProvider
	{
		[Import (typeof (ITaskMetadataBuilder))]
		public ITaskMetadataBuilder TaskMetadataBuilder { get; set; }

		[Import (typeof (MSBuildSchemaProvider), AllowDefault = true)]
		public MSBuildSchemaProvider SchemaProvider { get; set; }

		[Import (typeof (IRuntimeInformation), AllowDefault = true)]
		public IRuntimeInformation RuntimeInformation { get; set; }

		public MSBuildBackgroundParser GetParser (ITextBuffer buffer)
			=> buffer.Properties.GetOrCreateSingletonProperty (typeof (MSBuildBackgroundParser), () => CreateParser (buffer));

		MSBuildBackgroundParser CreateParser (ITextBuffer buffer)
		{
			Debug.Assert (buffer.ContentType.IsOfType (MSBuildContentType.Name));
			buffer.ContentTypeChanged += ContentTypeChanged;

			var runtimeInfo = RuntimeInformation;
			if (runtimeInfo == null) {
				try {
					runtimeInfo = new MSBuildEnvironmentRuntimeInformation ();
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to initialize runtime info for parser", ex);
					runtimeInfo = new NullRuntimeInformation ();
				}
			}
			return new MSBuildBackgroundParser (
				buffer,
				runtimeInfo,
				SchemaProvider ?? new MSBuildSchemaProvider (),
				TaskMetadataBuilder ?? new NoopTaskMetadataBuilder ()
			);
		}

		void ContentTypeChanged (object sender, ContentTypeChangedEventArgs e)
		{
			if (!e.AfterContentType.IsOfType (MSBuildContentType.Name)) {
				var buffer = (ITextBuffer)sender;
				buffer.ContentTypeChanged -= ContentTypeChanged;
				if (buffer.Properties.TryGetProperty (typeof (MSBuildBackgroundParser), out MSBuildBackgroundParser parser)) {
					parser.Dispose ();
				}
				buffer.Properties.RemoveProperty (typeof (MSBuildBackgroundParser));
			}
		}
	}
}