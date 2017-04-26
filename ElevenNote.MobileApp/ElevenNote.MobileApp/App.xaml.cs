using ElevenNote.MobileApp.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ElevenNote.MobileApp
{
	public partial class App : Application
	{

        internal static readonly NoteService NoteService = new NoteService();

		public App ()
		{
			InitializeComponent();

            this.MainPage = new NavigationPage(new NotesPage());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
