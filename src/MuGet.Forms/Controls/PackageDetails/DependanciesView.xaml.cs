﻿using MuGet.Forms.Models;
using MuGet.Forms.Views;
using System;
using Xamarin.Forms;

namespace MuGet.Forms.Controls
{
    public partial class DependanciesView : StackLayout
    {
        public DependanciesView()
        {
            InitializeComponent();
        }

        private void DependencyTapped(object sender, EventArgs e)
        {
            if (sender is View v && v.BindingContext is Dependency dependency)
            {
                var packagePage = new PackagePage
                {
                    PackageId = dependency.Id,
                    Version = dependency.VersionRange?.MinVersion != null
                        ? dependency.VersionRange.MinVersion.ToString()
                        : string.Empty
                };

                Navigation.PushAsync(packagePage);
            }
        }
    }
}