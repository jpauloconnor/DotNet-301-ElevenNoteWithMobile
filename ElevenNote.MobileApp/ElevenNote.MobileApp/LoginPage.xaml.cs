using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ElevenNote.MobileApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
		}

        private async void BtnLogin_OnClicked(object sender, EventArgs e)
        {
            // Make sure they filled all the fields.
            if (string.IsNullOrWhiteSpace(fldUsername.Text) || string.IsNullOrWhiteSpace(fldPassword.Text))
            {
                //In that case, await and display the alert. Title of alert, text of alert, and text of buttons.
                await DisplayAlert("Whoops", "Please enter a username and password.", "Okie Dokie");
      
                return;
            }

            // Turn on the "please wait" spinner.
            pleaseWait.IsRunning = true;
            
            fldUsername.IsEnabled = false;
            fldPassword.IsEnabled = false;
            btnLogin.IsEnabled = false;

            // Attempt to log in.
            // await the call, pass it the values of the fields
            //.Continue with -> Whenever it's done, continue with this code. Same 
            // as .then in JavaScript. This is anonymous function with one parameter of task.
            // the function has no name
            // State how this function is going to run explicitly.
            
            await App.NoteService.Login(fldUsername.Text.Trim(), fldPassword.Text)
                .ContinueWith(async task =>
                {
                    // Get the result.
                    // Gets the task that was passed in and attaches the Result
                    var loggedIn = task.Result;

                    // Let them know if login failed.
                    // If not logged in, show an alert.
                    if (!loggedIn)
                    {
                        //This is a point that gets disconnected from main view thread
                        await DisplayAlert("Whoops", "Login failed.", "Okie Dokie");
                        fldUsername.IsEnabled = true;
                        fldPassword.IsEnabled = true;
                        btnLogin.IsEnabled = true;
                        //Turn the IsRunning off.
                        pleaseWait.IsRunning = false;
                        return;
                    }

                    // If login was successful, send them to the notes list page.
                    // Turn off the IsRunning animation.
                    // Don't want it to eat CPU cycles invisibly.
                    pleaseWait.IsRunning = false;

                    // Push a new NotesPage
                    // Create a new notes page and push it on top of the stack.
                    await Navigation.PushAsync(new NotesPage(), true);

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
