using AnythingWorld.Utilities;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking.Editor
{
    public static class SignupLoginProcessor
    {
        public struct SignupLoginError
        {
            public string code;
            public string message;

            public SignupLoginError(string code, string message) { this.code = code; this.message = message; }
        }

        public delegate void SubmitSignupLoginError(SignupLoginError error);
        private static SubmitSignupLoginError signupErrorDelegate;
        private static SubmitSignupLoginError loginErrorDelegate;

        public delegate void SubmitCredentialsSuccess();
        private static SubmitCredentialsSuccess credentialsSuccessDelegate;

        public static void SignUp(string signupEmail, string signupPassword, string signupPasswordCheck, string signupFullName, bool termsAccepted, SubmitSignupLoginError errorDelegate, SubmitCredentialsSuccess successDelegate, object owner)
        {
            CoroutineExtension.StartEditorCoroutine(SubmitSignupCoroutine(signupEmail, signupPassword, signupPasswordCheck, signupFullName, termsAccepted, errorDelegate, successDelegate), owner);
        }

        public static void LogIn(string loginEmail, string loginPassword, SubmitSignupLoginError errorDelegate, SubmitCredentialsSuccess successDelegate, object owner)
        {
            CoroutineExtension.StartEditorCoroutine(SubmitLoginCoroutine(loginEmail, loginPassword, errorDelegate, successDelegate), owner);
        }

        private static IEnumerator SubmitSignupCoroutine(string rawEmailInput, string signupPass, string signupPassCheck, string rawSignupName, bool signupTerms, SubmitSignupLoginError errorDelegate, SubmitCredentialsSuccess successDelegate)
        {
            credentialsSuccessDelegate += successDelegate;
            signupErrorDelegate += errorDelegate;
            signupLoginError = new SignupLoginError();

            var cleanedEmail = rawEmailInput.ToLower();

            try
            {
                if (string.IsNullOrEmpty(cleanedEmail) || string.IsNullOrEmpty(signupPass) || string.IsNullOrEmpty(signupPassCheck) || string.IsNullOrEmpty(rawSignupName))
                {
                    //If a field is found to empty, set error and stop submission. return to signup
                    signupLoginError = new SignupLoginError("Missing fields", "All fields must be filled.");
                    signupErrorDelegate(signupLoginError);
                    signupErrorDelegate -= errorDelegate;
                    credentialsSuccessDelegate -= successDelegate;
                    yield break;
                }
                else if (!IsValidEmail(cleanedEmail))
                {
                    {
                        signupLoginError = new SignupLoginError("Invalid Email", "Your email is an invalid address.");
                        signupErrorDelegate(signupLoginError);
                        signupErrorDelegate -= errorDelegate;
                        credentialsSuccessDelegate -= successDelegate;
                        yield break;
                    }
                }
                else if (!signupTerms)
                {
                    //If terms not accepted flag error and return to form. return to signup
                    signupLoginError = new SignupLoginError("Terms of Use", "To create an account, you must accept our terms and conditions.");
                    signupErrorDelegate(signupLoginError);
                    signupErrorDelegate -= errorDelegate;
                    credentialsSuccessDelegate -= successDelegate;
                    yield break;
                }

            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception("Error initializing signupErrors, returning", e));
                signupErrorDelegate -= errorDelegate;
                credentialsSuccessDelegate -= successDelegate;
                yield break;
            }

            UnityWebRequest www = null;

            WWWForm form = new WWWForm();
            form.AddField("email", cleanedEmail);
            form.AddField("password", signupPass);
            form.AddField("passwordCheck", signupPassCheck);
            form.AddField("terms", "true");
            form.AddField("tier", "individual");
            form.AddField("fullName", rawSignupName);

            www = UnityWebRequest.Post("https://subscription-portal-backend.herokuapp.com/users/register", form);
            www.timeout = 15;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ParseSignupResponse(www.downloadHandler.text);
                    ApplyLoginResponse();
                    credentialsSuccessDelegate();
                }
                catch {}
            }
            else
            {
                var errorResponse = JsonUtility.FromJson<LoginErrorResponse>(www.downloadHandler.text);
                if (errorResponse == null)
                {
                    Debug.LogWarning(www.downloadHandler.data);
                    if (AnythingSettings.DebugEnabled) Debug.LogWarning($"Unhandled error signing up to Anything World: {www.downloadHandler.text}, contact support.");
                }
                else
                {
                    ParseSignupLoginError(errorResponse);
                    signupErrorDelegate(signupLoginError);
                }

            }

            signupErrorDelegate -= errorDelegate;
            credentialsSuccessDelegate -= successDelegate;

            www.Dispose();
            yield return null;
        }

        private static IEnumerator SubmitLoginCoroutine(string rawLoginEmail, string loginPass, SubmitSignupLoginError errorDelegate, SubmitCredentialsSuccess successDelegate)
        {
            credentialsSuccessDelegate += successDelegate;
            loginErrorDelegate += errorDelegate;
            signupLoginError = new SignupLoginError();

            var cleanedLoginEmail = rawLoginEmail.ToLower();

            try
            {
                if (string.IsNullOrEmpty(cleanedLoginEmail) || string.IsNullOrEmpty(loginPass))
                {
                    signupLoginError = new SignupLoginError("Missing fields", "All fields must be filled.");
                    loginErrorDelegate(signupLoginError);
                    loginErrorDelegate -= errorDelegate;
                    credentialsSuccessDelegate -= successDelegate;
                    yield break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception("Error initializing signupErrors, returning", e));
                loginErrorDelegate -= errorDelegate;
                credentialsSuccessDelegate -= successDelegate;
                yield break;

            }

            WWWForm form = new WWWForm();
            form.AddField("email", cleanedLoginEmail);
            form.AddField("password", loginPass);

            UnityWebRequest www = UnityWebRequest.Post("https://subscription-portal-backend.herokuapp.com/users/login", form);
            www.timeout = 15;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ParseLoginResponse(www.downloadHandler.text);
                    ApplyLoginResponse();
                    credentialsSuccessDelegate();
                }
                catch { }
            }
            else
            {
                var errorResponse = JsonUtility.FromJson<LoginErrorResponse>(www.downloadHandler.text);
                ParseSignupLoginError(errorResponse);
                loginErrorDelegate(signupLoginError);
                if (AnythingSettings.DebugEnabled) Debug.Log($"Error logging into Anything World: {www.downloadHandler.text}");
            }
            www.Dispose();
            loginErrorDelegate -= errorDelegate;
            credentialsSuccessDelegate -= successDelegate;
            yield return null;
        }

        #region Parsers
        private static string fetchedEmail = "";
        private static string apiKey = "";
        private static void ParseLoginResponse(string text)
        {
            string cleanedText = Regex.Replace(text, @"[[\]]", "");
            string[] arr = cleanedText.Split(',');
            apiKey = arr[3].ToString().Split(':')[1].Trim('\"');
            fetchedEmail = arr[5].ToString().Split(':')[1].Trim('\"');
        }

        private static void ParseSignupResponse(string text)
        {
            string cleanedText = Regex.Replace(text, @"[[\]]", "");
            string[] arr = cleanedText.Split(',');
            apiKey = arr[4].ToString().Split(':')[1].Trim('\"');
            fetchedEmail = arr[0].ToString().Split(':')[1].Trim('\"');
        }

        private static SignupLoginError signupLoginError;
        private static void ParseSignupLoginError(LoginErrorResponse error)
        {
            if (AnythingSettings.DebugEnabled) Debug.Log($"Error code {error.code}: {error.msg}");
            signupLoginError = new SignupLoginError(error.code, error.msg);
        }

        private static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        private static void ApplyLoginResponse()
        {
            AnythingSettings.APIKey = apiKey;
            AnythingSettings.Email = fetchedEmail;
            Undo.RecordObject(AnythingSettings.Instance, "Added API Key and Email to AnythingSettings");
            EditorUtility.SetDirty(AnythingSettings.Instance);
        }
        #endregion Parsers
    }

    public class LoginErrorResponse
    {
        public string code = "";
        public string msg = "";
    }
}
