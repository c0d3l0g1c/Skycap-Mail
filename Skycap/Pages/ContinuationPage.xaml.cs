﻿// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved

using Windows.UI.Xaml.Controls;

namespace Skycap.Pages
{
    /// <summary>
    /// A paged used to flow text from a given text container
    /// Usage: Outputscenarios 1-5 might not fit entirely on a given "printer page"
    /// In that case simply add new continuation pages of the given size until all the content can be displayed
    /// </summary>
    public sealed partial class ContinuationPage : Page
    {
        /// <summary>
        /// Creates a continuation page and links text-flow to a text flow container
        /// </summary>
        /// <param name="textLinkContainer">Text link container which will flow text into this page</param>
        public ContinuationPage(RichTextBlockOverflow textLinkContainer)
        {
            InitializeComponent();
            textLinkContainer.OverflowContentTarget = continuationPageLinkedContainer;
        }
    }
}