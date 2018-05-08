﻿using System;

using InkPoc.ViewModels;

using Windows.UI.Xaml.Controls;

namespace InkPoc.Views
{
    public sealed partial class RecognizeTextPage : Page
    {
        public RecognizeTextViewModel ViewModel { get; } = new RecognizeTextViewModel();

        public RecognizeTextPage()
        {
            InitializeComponent();
        }
    }
}
