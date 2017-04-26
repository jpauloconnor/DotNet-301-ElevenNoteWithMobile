using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ElevenNote.MobileApp.Models;
using ElevenNote.Models;
using Newtonsoft.Json;

namespace ElevenNote.MobileApp.ExternalServices
{
    internal class NoteService
    {
        //When we get the BearerToken, we'll put it there and put it in this container statically, across
        //all instances of NoteService it won't change.
        public static string BearerToken { get; set; }

        //Sole purpose is to hold name of url for api without endpoint information.
        //This will change when you deploy to Azure.
        private const string _apiUrl = "https://auri-efa-elevennoteapi-april2017.azurewebsites.net";

        //public async tasks
            //With web development things are already happening asynchronously
            //If you don't do things async, your app will lock up and wait.
            //If Android thinks your app isn't responsive, it will kill the app.
            //When you do things on Main/UI Thread and you didn't do it async, you will block that thread.
            //iOS 7 & 17 seconds to process request. If it doesn't work, you end up at homescreen.
            //Async takes entire function and turns it into a Task.
            //Async & Await work hand in hand 
            //In mobile dev, you have to do async. It's a different user experience because the app takes up
            //the whole screen. This puts the user first. 
            

        /// <summary>
        /// Attempts to log the user in.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> Login(string username, string password)
        {
            //if they didn't pass a username or password, return false
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            //Try catch handles things gracefully
            try
            {
                //for us to make an http request
                //we need to get a client
                using (var client = new HttpClient())
                {
                    // Build API URL.
                    //construct a request
                    //URL constant at top of class.
                    var url = $"{_apiUrl}/token";

                    //
                    // Construct the request.
                    //Trim means get rid of spaces before and after
                    //URLEncode will return the URL encoded string
                    //Same thing for password
                    var requestString = $"grant_type=password&username={HttpUtility.UrlEncode(username.Trim())}&password={HttpUtility.UrlEncode(password.Trim())}";
                    //Here is the header we added in Fiddler
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "www-form-urlencoded; charset=utf-8");

                    // Get the response and set the bearer token if one was returned.
                    // Get the result by awaiting. 
                    // Do a Post, pass in a new string content
                    var result = await client.PostAsync(url, new StringContent(requestString));
                    // If don't get a successful, bearer token.
                    if (!result.IsSuccessStatusCode)
                    {
                        BearerToken = null;
                        return false;
                    }

                    // It's going to read JSON, and try to convert it. Match it's fields up with OauthBearerTokenResponse.
                    //If we get null back, return false.
                    var response = JsonConvert.DeserializeObject<OauthBearerTokenResponse>(await result.Content.ReadAsStringAsync());
                    if (response == null) return false;
                    //IF we got a BearerToekn back, return true.
                    BearerToken = response.access_token; // can't use Deserialize<dynamic> or iOS will go boom
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all the user's notes.
        /// </summary>
        /// <returns></returns>
        public async Task<List<NoteListItem>> GetAll()
        {
            if (string.IsNullOrWhiteSpace(BearerToken)) throw new UnauthorizedAccessException("Bearer token not initialized. Aborting.");

            using (var client = new HttpClient())
            {
                // Build API URL.
                var url = $"{_apiUrl}/api/notes";

                // Construct the request.
                // Options:
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {BearerToken}"); OR
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                // Make the call and get the result.
                var result = await client.GetAsync(url);

                // If the call failed, return an empty list.
                if (!result.IsSuccessStatusCode) return new List<NoteListItem>();

                // Otherwise, deserialize the result and return for use.
                var notes = JsonConvert.DeserializeObject<List<NoteListItem>>(await result.Content.ReadAsStringAsync());
                return notes;
            }
        }

        /// <summary>
        /// Gets note details by ID.
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
        public async Task<NoteDetail> GetById(int noteId)
        {
            if (string.IsNullOrWhiteSpace(BearerToken)) throw new UnauthorizedAccessException("Bearer token not initialized. Aborting.");

            using (var client = new HttpClient())
            {
                // Build API URL.
                var url = $"{_apiUrl}/api/notes/{ noteId }"; // endpoint uses "id" as default, so we don't have to create a querystring with "id="

                // Construct the request.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                // Make the call and get the result.
                var result = await client.GetAsync(url);

                // If the call failed, return a null note object.
                if (!result.IsSuccessStatusCode) return null;

                // Otherwise, deserialize the result and return the note details object for use.
                var note = JsonConvert.DeserializeObject<NoteDetail>(await result.Content.ReadAsStringAsync());
                return note;
            }

        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public async Task<bool> AddNew(NoteCreate note)
        {
            if (string.IsNullOrWhiteSpace(BearerToken)) throw new UnauthorizedAccessException("Bearer token not initialized. Aborting.");

            using (var client = new HttpClient())
            {
                // Build API URL.
                var url = $"{_apiUrl}/api/notes";

                // Construct the request.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                // Create the JSON version of the note object. JSON is a string we'll send to the server.
                var json = JsonConvert.SerializeObject(note);

                // Make the call and get the result.
                var result = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")); // we have to specify we're sending JSON

                // If the call failed, return an empty list.
                return result.IsSuccessStatusCode;

            }
        }

        /// <summary>
        /// Updates the passed note.
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public async Task<bool> Update(NoteEdit note)
        {
            if (string.IsNullOrWhiteSpace(BearerToken)) throw new UnauthorizedAccessException("Bearer token not initialized. Aborting.");

            using (var client = new HttpClient())
            {
                var url = $"{_apiUrl}/api/notes";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                var json = JsonConvert.SerializeObject(note);
                var result = await client.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json")); // we have to specify we're sending JSON

                return result.IsSuccessStatusCode;

            }
        }

        /// <summary>
        /// Deletes the passed note by ID.
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
        public async Task<bool> Delete(int noteId)
        {
            if (string.IsNullOrWhiteSpace(BearerToken)) throw new UnauthorizedAccessException("Bearer token not initialized. Aborting.");

            using (var client = new HttpClient())
            {
                var url = $"{_apiUrl}/api/notes/{noteId}";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                var result = await client.DeleteAsync(url); // DELETE is similar to a GET call - it has no body

                return result.IsSuccessStatusCode;
            }
        }

    }
}